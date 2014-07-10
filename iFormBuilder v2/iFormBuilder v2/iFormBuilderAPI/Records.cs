using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Runtime.Serialization.Json;
using iFormBuilderAPI.DataContractObjects;
using System.IO;
using Newtonsoft.Json;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.ServiceModel.Channels;
using Newtonsoft.Json.Linq;
using System.Data;
using System.Collections;
using System.Diagnostics;
using System.Web.Script.Serialization;

namespace iFormBuilderAPI
{
    public class Records
    {
        private Page _page;
        private string _url;
        private string _username;
        private string _password;
        private int _favorranking;
        /// <summary>
        /// Initializes a new instance of the <see cref="Records"/> class.
        /// </summary>
        /// <param name="page">The page.</param>
        /// <param name="url">The URL.</param>
        /// <param name="username">The username.</param>
        /// <param name="password">The password.</param>
        /// <param name="autopopulaterecords">if set to <c>true</c> [autopopulaterecords].</param>
        public Records(Page page, string url, string username, string password, int favorranking )
        {
            _page = page;
            _url = url;
            _username = username;
            _password = password;
            _favorranking = favorranking;
            _results = new List<Result>();
        }

        private List<Result> _results;
        public List<Result> RecordSet
        {
            set { _results = value; }
            get
            {
                return _results;
            }
        }

        public List<long> DownloadIDs(string requesturl)
        {
            try
            {
                WebResponse webResponse = Utilities.DownloadRequest(requesturl, _username, _password);

                Encoding enc = Encoding.Default;
                string configuration = String.Empty;
                using (StreamReader reader = new StreamReader(webResponse.GetResponseStream()))
                {
                    configuration = reader.ReadToEnd();
                }
                Debug.WriteLine(configuration);

                Newtonsoft.Json.Linq.JArray result = JArray.Parse(configuration);
                List<long> ids = new List<long>();
                for (int x = 0; x < result.Count(); x++)
                {
                    ids.Add(long.Parse(result[x].ToString().Replace("{", "").Replace("}", "")));
                }
                return ids;
            }
            catch (Exception ex)
            {
                throw new Exception(string.Format("Error in Download ID: {0}",ex.Message));
            }
        }

        public List<Result> GetPageFeed(string requesturl, AccessCode accesscode)
        {
            Administration admin = new Administration();
            WebResponse webResponse = Utilities.GetJSONRequest(requesturl, accesscode);

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

            List<Result> results = new List<Result>();
            results = processresults(result, _page, _favorranking);
            return results;
        }
        /// <summary>
        /// Populates the recordset
        /// </summary>
        public Result CreateRecord(string requesturl)
        {
            try
            {
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

                List<Result> results = new List<Result>();
                results = processresults(result, _page, _favorranking);

                //Since we should only have one return the first record
                return results[0];
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// Processresultses the specified result.
        /// </summary>
        /// <param name="result">The result.</param>
        /// <param name="page">The page.</param>
        /// <returns></returns>
        /// <exception cref="System.Exception">No Records were downloaded for processing</exception>
        private List<Result> processresults(JArray result,Page page, int favorranking)
        {
            if (result == null)
                throw new Exception("No Records were downloaded for processing");

            List<Result> results = new List<Result>();
            List<Result> children;
            //Read through the Records
            JToken token = result.First;
            Result r = new Result();
            while (token != null)
            {
                r = new Result();
                r.Record = JsonConvert.DeserializeObject<Dictionary<string, object>>(token.First.First.ToString());
                r.iFormPage = page;
                r.PageName = page.NAME;
                r.PageID = page.ID;
                //Process the Results Field
                if(token.First.Next.Last.Last != null)
                    r.RecordLocation = processlocation((JArray)token.First.Next.Last.Last.Last);
                //r.Record = JsonConvert.DeserializeObject<Dictionary<string, object>>(token.First.First.ToString());
                JArray childArray;
                if (page.HasSubForms)
                {
                    foreach (Element ele in page.SubformElements)
                    {
                        foreach (KeyValuePair<string, object> kvp in r.Record)
                        {

                            if (ele.NAME == kvp.Key)
                            {
                                if (kvp.Value != null)
                                {
                                    
                                    if (kvp.Value.ToString().Contains('{'))
                                    {
                                        childArray = (JArray)kvp.Value;
                                        children = processresults(childArray, page.GetSubformPage(ele), favorranking);
                                        foreach (Result childresult in children)
                                            r.Children.Add(childresult);
                                    }
                                }
                            }
                        }
                    }
                }

                string createlocation = string.Empty;
                string modlocation = String.Empty;
                string location = String.Empty;

                foreach (KeyValuePair<string, object> kvp in r.Record)
                {
                    if (page.HasLocationWidget && page.LocationElement.NAME == kvp.Key && kvp.Value != null)
                        location = kvp.Value.ToString().Replace(",", ".");

                    if (kvp.Key == "MODIFIED_LOCATION" && kvp.Value != null)
                        modlocation = kvp.Value.ToString().Replace(",", ".");

                    if (kvp.Key == "CREATED_LOCATION" && kvp.Value != null)
                        createlocation = kvp.Value.ToString().Replace(",",".");
                }

                //Make sure some type of location has been set
                if (r.RecordLocation == null)
                {
                    r.RecordLocation = new Location();
                    //Set the Priority of the location field
                    if (location.Length != 0 && location != "API")
                        r.RecordLocation.SetLocationFromField(location);
                    else if (modlocation.Length != 0 && modlocation != "API")
                        r.RecordLocation.SetLocationFromField(modlocation);
                    else if (createlocation.Length != 0 && createlocation != "API")
                        r.RecordLocation.SetLocationFromField(createlocation);
                    else
                        r.RecordLocation.SetDefaultValues();
                }
                else
                {
                    //Set the Priority of the location field
                    if (location.Length != 0  && location != "API")
                        r.RecordLocation.UpdateValuesFromField(location);
                    else if (modlocation.Length != 0 && modlocation != "API")
                        r.RecordLocation.UpdateValuesFromField(modlocation);
                    else if (createlocation.Length != 0 && createlocation != "API")
                        r.RecordLocation.UpdateValuesFromField(createlocation);
                }

                //Reset the Location Based on a Favorable Ranking
                if(favorranking == 0  && location.Length != 0)
                    r.RecordLocation.SetLocationFromField(location);
                if (favorranking == 1 && modlocation.Length != 0)
                    r.RecordLocation.SetLocationFromField(modlocation);
                if(favorranking == 2  && createlocation.Length != 0)
                    r.RecordLocation.SetLocationFromField(createlocation);
  
                results.Add(r);
                token = token.Next;
            }

            return results;
        }

        private Location processlocation(JArray locationinformation)
        {
            Location loc = new Location();
            loc.Latitude = float.Parse(locationinformation.First.ToString());
            loc.Longitude = float.Parse(locationinformation.Last.ToString());
            return loc;
        }
    }

    public class Result
    {
        public Result() {
            this.Children = new List<Result>();
        }
        public Page iFormPage { get; set; }
        public string PageName { get; set; }
        public long PageID { get; set; }
        public Dictionary<string,object> Record { get; set; }
        public Location RecordLocation {get;set;}
        public List<Result> Children { get; set; }
        public bool HasChildren { get { return this.Children.Count != 0; } }
    }

    [ServiceContract]
    interface IClientSideProfileService
    {
        // There is no need to write a DataContract for the complex type returned by the service.
        // The client will use a JsonObject to browse the JSON in the received message.

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json)]
        Message GetMemberProfile();
    }
}
