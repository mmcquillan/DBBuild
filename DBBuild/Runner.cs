using System.Data;
using System.IO;

namespace DBBuild
{
    class Runner
    {

        #region Constructor
        public Runner(Macros macros)
        {
            mac = macros;
            if (mac.GetTF("$DBTRUSTED$"))
            {
                db = new SMOConnect(mac.Get("$DBSERVER$"), mac.Get("$DBCATALOG$"), true);
            }
            else
            {
                db = new SMOConnect(mac.Get("$DBSERVER$"), mac.Get("$DBCATALOG$"), mac.Get("$DBLOGIN$"), mac.Get("$DBPASSWORD$"));
            }
        }
        #endregion

        #region Members
        private SMOConnect db;
        private Macros mac;
        #endregion

        #region PUBLIC ExecuteSQL
        public void ExecuteSQL(string sql)
        {

            // execute through db
            db.ExecuteSQL(sql);

        }
        #endregion

        #region PUBLIC SetCatalog
        public void SetCatalog()
        {

            // update the catalog
            mac.Set("$DBCATALOG$", db.GetCatalogName());

        }
        #endregion

        #region PUBLIC SetupVersioning
        public void SetupVersioning()
        {

            // check to see this is not a special db
            string curDB = db.GetCatalogName().ToUpper();
            if (curDB == "MASTER" || curDB == "MSDB" || curDB == "TEMPDB" || curDB == "MODEL")
            {
                UI.Feedback("WARNING", "Cannot mark a system DB for versioning (currently [" + curDB + "])", mac.GetTF("$VERBOSE$"));
            }
            else
            {

                // needed vars
                string sql;
                bool needVersion = true;
                bool needChanges = true;

                // get the list of expected tables (dbo.DBBVersion & dbo.DBBChanges)
                sql = "SELECT TABLE_SCHEMA + '.' + TABLE_NAME AS TName FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_TYPE = 'BASE TABLE' AND (TABLE_SCHEMA + '.' + TABLE_NAME = '" + mac.Get("$DBBVERSION$") + "' OR TABLE_SCHEMA + '.' + TABLE_NAME = '" + mac.Get("$DBBCHANGES$") + "')";
                DataSet ds = db.GetDataSet(sql);

                // check to see if this has dbbversion/dbbchanges
                if (ds.Tables.Count == 1)
                {
                    foreach (DataRow row in ds.Tables[0].Rows)
                    {

                        // check if we need the DBBVersion
                        if (row["TName"].ToString().ToUpper() == mac.Get("$DBBVERSION$").ToUpper())
                        {
                            needVersion = false;
                        }

                        // check if we need the DBBChanges
                        if (row["TName"].ToString().ToUpper() == mac.Get("$DBBCHANGES$").ToUpper())
                        {
                            needChanges = false;
                        }
                    }
                }

                // if not there, create DBBVersion
                if (needVersion)
                {
                    // create table
                    sql = "CREATE TABLE " + mac.Get("$DBBVERSION$") + " (InstalledOn datetime NOT NULL CONSTRAINT PK_" + mac.Get("$DBBVERSION$").Replace('.', '_') + " PRIMARY KEY, LastUpdateOn datetime NOT NULL, CurrentState nvarchar(30) NOT NULL)";
                    db.ExecuteSQL(sql);

                    // set initial version info
                    sql = "INSERT INTO " + mac.Get("$DBBVERSION$") + " (InstalledOn, LastUpdateOn, CurrentState) VALUES (GetUTCDate(), GetUTCDate(), 'INITIALIZING')";
                    db.ExecuteSQL(sql);
                }

                // if not there, create DBBChanges
                if (needChanges)
                {
                    // create table
                    sql = "CREATE TABLE " + mac.Get("$DBBCHANGES$") + " (Script nvarchar(450) NOT NULL CONSTRAINT PK_" + mac.Get("$DBBCHANGES$").Replace('.', '_') + " PRIMARY KEY, ChangeState nvarchar(30) NOT NULL, StartedOn datetime NOT NULL, CompletedOn datetime NULL)";
                    db.ExecuteSQL(sql);
                }

            }

        }
        #endregion

        #region PUBLIC VersionStart
        public void VersionStart()
        {

            // needed vars
            string sql;

            // set the Version State
            sql = "UPDATE " + mac.Get("$DBBVERSION$") + " SET CurrentState = 'BUILDING'";
            db.ExecuteSQL(sql);

        }
        #endregion

        #region PUBLIC VersionSucceeded
        public void VersionSucceeded()
        {

            // needed vars
            string sql;

            // set the Version State
            sql = "UPDATE " + mac.Get("$DBBVERSION$") + " SET LastUpdateOn = GetUTCDate(), CurrentState = 'SUCCESSFUL'";
            db.ExecuteSQL(sql);

        }
        #endregion

        #region PUBLIC VersionFailed
        public void VersionFailed()
        {

            // needed vars
            string sql;

            // set the Version State
            sql = "UPDATE " + mac.Get("$DBBVERSION$") + " SET LastUpdateOn = GetUTCDate(), CurrentState = 'FAILED'";
            db.ExecuteSQL(sql);

        }
        #endregion

        #region PUBLIC ChangeCheck
        public bool ChangeCheck(string script)
        {

            // needed vars
            string sql;
            bool exists = false;

            // query the number if this exists
            sql = "SELECT Count(*) AS Cnt FROM " + mac.Get("$DBBCHANGES$") + " WHERE Script = '" + script + "'";
            DataSet ds = db.GetDataSet(sql);

            // check for the existance of
            if (ds.Tables[0].Rows[0]["Cnt"].ToString() == "1")
            {
                exists = true;
            }

            // return result
            return !exists;

        }
        #endregion

        #region PUBLIC ChangeStart
        public void ChangeStart(string script)
        {

            // needed vars
            string sql;

            // set the Version State
            sql = "INSERT INTO " + mac.Get("$DBBCHANGES$") + " (Script, ChangeState, StartedOn) VALUES ('" + script + "', 'BUILDING', GetUTCDate())";
            db.ExecuteSQL(sql);

        }
        #endregion

        #region PUBLIC ChangeSucceeded
        public void ChangeSucceeded(string script)
        {

            // needed vars
            string sql;

            // set the Version State
            sql = "UPDATE " + mac.Get("$DBBCHANGES$") + " SET ChangeState = 'SUCCEEDED', CompletedOn = GetUTCDate() WHERE Script = '" + script + "'";
            db.ExecuteSQL(sql);

        }
        #endregion

        #region PUBLIC ChangeFailed
        public void ChangeFailed(string script)
        {

            // needed vars
            string sql;

            // set the Version State
            sql = "DELETE FROM " + mac.Get("$DBBCHANGES$") + " WHERE Script = '" + script + "'";
            db.ExecuteSQL(sql);

        }
        #endregion

    }
}
