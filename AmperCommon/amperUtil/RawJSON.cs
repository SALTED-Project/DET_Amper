

namespace amperUtil.Http
{
    public class RawJSON
    {
        private string m_json;
        public RawJSON(string json)
        {
            m_json = json;
        }

        public override string ToString()
        {
            return m_json;
        }

        //public string ToString_Rename (string oldName, string newName)
        //{
        //    string patternOld = "\"";
        //    patternOld += oldName;
        //    patternOld += "\":";

        //    string patternNew = "\"";
        //    patternNew += newName;
        //    patternNew += "\":";

        //    string s =  m_json.Replace(patternOld, patternNew);
        //    return s;
        //}
    }
}