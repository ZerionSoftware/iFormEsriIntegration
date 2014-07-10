using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.IO;
using System.Diagnostics;
using Newtonsoft.Json;

namespace iFormBuilderAPI
{
    public class FormAssignment
    {
                private string _url;
        private string _username;
        private string _password;
        public FormAssignment(string url, string username,string password)
        {
            _url = url;
            _username = username;
            _password = password;
                PopulateFormList();
        }

        public List<FormResult> FormItems { get; set; }

        /// <summary>
        /// Populates the recordset.
        /// </summary>
        public void PopulateFormList( )
        {
            try
            {
                string requesturl = _url;
                WebResponse webResponse = Utilities.DownloadRequest(requesturl, _username, _password);
                Encoding enc = Encoding.Default;
                string configuration = String.Empty;
                using (StreamReader reader = new StreamReader(webResponse.GetResponseStream()))
                {
                    configuration = reader.ReadToEnd();
                }
                Debug.WriteLine(configuration);

                Newtonsoft.Json.JsonSerializer json = new Newtonsoft.Json.JsonSerializer();

                json.NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore;
                json.ObjectCreationHandling = Newtonsoft.Json.ObjectCreationHandling.Replace;
                json.MissingMemberHandling = Newtonsoft.Json.MissingMemberHandling.Ignore;
                json.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;

                StringReader sr = new StringReader(configuration);
                Newtonsoft.Json.JsonTextReader jReader = new JsonTextReader(sr);
                Newtonsoft.Json.Linq.JArray result = (Newtonsoft.Json.Linq.JArray)json.Deserialize(jReader, Type.GetType("System.Data.DataSet"));

                //Get the Record Set
                jReader.Close();

                List<FormResult> results = new List<FormResult>();
                return;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }

    public class FormResult
    {
        public FormResult()
        {
        }
        public string COLLECT_USERS { get; set; }
        public string CREATED_BY { get; set; }
        public DateTime CREATED_DATE { get; set; }
        public int IS_COPYABLE { get; set; }
        public string NAME { get; set; }
        public long PAGE_ID { get; set; }
        public string TYPE { get; set; }
        public long VERSION { get; set; }
        public string VIEW_USERS { get; set; }
    }
}
