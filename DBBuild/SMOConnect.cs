using System.Data;
using System.Data.SqlClient;
using Microsoft.SqlServer.Management.Smo;
using Microsoft.SqlServer.Management.Common;

namespace DBBuild
{

    public class SMOConnect
    {

        #region Constructors

        public SMOConnect()
        {
            InitVars();
            SetConnectionString();
            Connect();

        }

        public SMOConnect(string DataLinkFilePath)
        {
            InitVars();
            data_link_file = DataLinkFilePath;
            SetConnectionString();
            Connect();
        }

        public SMOConnect(string DBServer, string DBCatalog, bool WindowsAuthentication)
        {
            InitVars();
            db_server = DBServer;
            db_catalog = DBCatalog;
            db_windowsauth = WindowsAuthentication;
            SetConnectionString();
            Connect();
        }

        public SMOConnect(string DBServer, string DBCatalog, string DBUser, string DBPassword)
        {
            InitVars();
            db_server = DBServer;
            db_catalog = DBCatalog;
            db_user = DBUser;
            db_password = DBPassword;
            SetConnectionString();
            Connect();
        }

        #endregion

        #region Members

        private string db_server;
        private string db_catalog;
        private string db_user;
        private string db_password;
        private bool db_windowsauth;
        private string data_link_file;
        private string conn_string;
        private SqlConnection cn;
        private Server server;

        #endregion

        #region PUBLIC ExecuteSQL
        public void ExecuteSQL(string sql)
        {

            // execute sql string
            server.ConnectionContext.ExecuteNonQuery(sql);

        }
        #endregion

        #region PUBLIC GetDataSet
        public DataSet GetDataSet(string sql)
        {

            // query and return data set
            DataSet ds = server.ConnectionContext.ExecuteWithResults(sql);

            // return the dataset
            return ds;

        }
        #endregion

        #region PUBLIC GetCatalogName
        public string GetCatalogName()
        {

            // send back the name of the db
            DataSet ds = GetDataSet("SELECT DB_NAME() AS DBName");
            return ds.Tables[0].Rows[0]["DBName"].ToString();

        }
        #endregion

        #region PRIVATE InitVars
        private void InitVars()
        {
            db_server = "";
            db_catalog = "";
            db_user = "";
            db_password = "";
            db_windowsauth = false;
            data_link_file = "";
        }
        #endregion

        #region PRIVATE SetConnectionString
        private void SetConnectionString()
        {

            // here is a string to hold the constructed value
            string cs;

            // eval if we can just reference a config file
            if (data_link_file.Length != 0)
            {
                cs = "File Name=" + data_link_file + ";";
            }

              // no config file specified, so start evaluating
            else
            {
                // start with the standard provider type and server
                cs = "Data Source=" + db_server + ";";

                // add the next common block with the initial DB
                cs = cs + "Initial Catalog=" + db_catalog + ";";

                // yes windows authentication
                if (db_windowsauth)
                {
                    cs = cs + "Integrated Security=SSPI;";
                }

                  // no windows authentication
                else
                {
                    cs = cs + "User ID=" + db_user + ";Password=" + db_password + ";";
                }
            }

            // set the field
            conn_string = cs;

        }
        #endregion

        #region PRIVATE Connect
        private void Connect()
        {

            // make a connection object
            cn = new SqlConnection(conn_string);

            // establish an smo connection
            server = new Server(new ServerConnection(cn));

            // force a connection
            server.ConnectionContext.Connect();

        }
        #endregion

        #region Properties

        public string Server
        {
            get
            {
                return db_server;
            }
            set
            {
                db_server = value;
                SetConnectionString();
            }
        }

        public string Catalog
        {
            get
            {
                return db_catalog;
            }
            set
            {
                db_catalog = value;
                SetConnectionString();
            }
        }

        public string User
        {
            get
            {
                return db_user;
            }
            set
            {
                db_user = value;
                SetConnectionString();
            }
        }

        public string Password
        {
            get
            {
                return db_password;
            }
            set
            {
                db_password = value;
                SetConnectionString();
            }
        }

        public bool WindowsAuth
        {
            get
            {
                return db_windowsauth;
            }
            set
            {
                db_windowsauth = value;
                SetConnectionString();
            }
        }

        public string DataLinkFile
        {
            get
            {
                return data_link_file;
            }
            set
            {
                data_link_file = value;
                SetConnectionString();
            }
        }

        public string ConnectString
        {
            get
            {
                return conn_string;
            }
        }

        #endregion

    }
}
