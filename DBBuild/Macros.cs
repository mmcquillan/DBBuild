using System.Text;
using System.Collections;

namespace DBBuild
{
    class Macros
    {

        #region Members
        private Hashtable vars = new Hashtable();
        #endregion

        #region PUBLIC Set
        public void Set(string key, string value)
        {
            if (vars.ContainsKey(key))
            {
                vars[key] = value;
            }
            else
            {
                vars.Add(key, value);
            }
        }
        #endregion

        #region PUBLIC Get
        public string Get(string key)
        {
            return vars[key].ToString();
        }
        #endregion

        #region PUBLIC GetTF
        public bool GetTF(string key)
        {
            if (vars[key].ToString().ToUpper() == "TRUE")
                return true;
            else
            {
                return false;
            }
        }
        #endregion

        #region PUBLIC Substitute
        public string Substitute(string eval)
        {
            StringBuilder txt = new StringBuilder(eval);
            foreach (string key in vars.Keys)
            {
                txt.Replace(key, vars[key].ToString());
            }
            return txt.ToString();
        }
        #endregion

        #region PUBLIC Show
        public void Show()
        {
            foreach (string key in vars.Keys)
            {
                UI.TwoColumns(key, vars[key].ToString());
            }
        }
        #endregion

    }
}
