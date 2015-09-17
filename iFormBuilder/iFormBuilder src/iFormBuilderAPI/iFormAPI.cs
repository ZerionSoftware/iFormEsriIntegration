using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Security.Cryptography;
using System.Text;
//using System.Threading.Tasks;
using iFormBuilderAPI.DataContractObjects;
using Newtonsoft.Json;
//using iFormBuilderAPI.DataContractObjects;

namespace iFormBuilderAPI
{
    public partial class iFormBuilder
    {
        public Configuration iformconfig { get; set; }
        private List<OptionList> allOptions { get; set; }
        public iFormBuilder()
        {
            iformconfig = new Configuration();
        }        
        /// <summary>
        /// Construct the iFormbuilder from the parts instead of the config fiel
        /// </summary>
        /// <returns>iFormBuilder</returns>
        public iFormBuilder(string accesscode, string servername, int profileid)
        {
            iformconfig = new Configuration();
            _code = new AccessCode();
            _code.access_token = accesscode;
            _code.expires_in = 3600;
            iformconfig.iformserverurl = servername;
            iformconfig.profileid = profileid;
        }

        public iFormBuilder(string secretkey,string clientid,string servername, int profileid)
        {
            iformconfig = new Configuration();
            iformconfig.secretkey = secretkey;
            iformconfig.clientid = clientid;
            _code = new AccessCode();
            iformconfig.iformserverurl = servername;
            iformconfig.profileid = profileid;
        }
        public iFormBuilder(IConfiguration configuration)
        {
            iformconfig = configuration as Configuration;
        }

        public string ProfileURL { get { return "https://" + iformconfig.iformserverurl + ".iformbuilder.com/exzact/api/profiles/" + iformconfig.profileid; } }
        /// <summary>
        /// Gets the option list.
        /// </summary>
        /// <param name="optionlist">The optionlist.</param>
        /// https://SERVER_NAME.iformbuilder.com/exzact/api/profiles/PROFILE_ID/optionlists/OPTIONLIST_ID
        /// <returns></returns>
        public OptionList GetOptionList(int optionlist)
        {
            try
            {
                //Check to see if the option list has already been downloaded
                if (this.allOptions == null)
                    this.allOptions = new List<OptionList>();
                else
                {
                    //Find the Option List in the options
                    foreach (OptionList optList in allOptions)
                    {
                        if (optList.OPTION_LIST_ID == optionlist)
                            return optList;
                    }
                }
                
                //If the option list was not found above try and find it in the system
                string requesturl = ProfileURL + "/optionlists/" + optionlist;
                WebResponse request = Utilities.GetRequest(requesturl, accesscode);
                DataContractJsonSerializer jsonSerializer = new DataContractJsonSerializer(typeof(iFormBuilderOptionList));
                iFormBuilderOptionList access = (iFormBuilderOptionList)jsonSerializer.ReadObject(request.GetResponseStream());

                if (access.OPTIONLIST != null)
                {
                    OptionList optList = (OptionList)access.OPTIONLIST;
                    optList.OPTIONS = access.OPTIONS;
                    //Add this optionlist to the all options so it only downloads once
                    this.allOptions.Add(optList);
                    return optList;
                }

                return null;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public List<OptionList> GetAllOptionLists()
        {
            try
            {
                string requesturl = ProfileURL + "/optionlists/";
                WebResponse request = Utilities.GetRequest(requesturl, accesscode);
                DataContractJsonSerializer jsonSerializer = new DataContractJsonSerializer(typeof(iFormBuilderOptionLists));
                iFormBuilderOptionLists access = (iFormBuilderOptionLists)jsonSerializer.ReadObject(request.GetResponseStream());
                return access.OPTIONLISTS;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        /// <summary>
        /// Gets the option list.
        /// </summary>
        /// <param name="optionlist">The optionlist.</param>
        /// https://SERVER_NAME.iformbuilder.com/exzact/api/profiles/PROFILE_ID/optionlists/OPTIONLIST_ID
        /// <returns></returns>
        public OptionList CreateOptionList(OptionList optionlist)
        {
            try
            {
                string requesturl = ProfileURL + "/optionlists/";
                WebResponse request = Utilities.HttpPost(requesturl, optionlist.GetParameters, accesscode.access_token, Utilities.PostType.POST);
                if (request != null)
                {
                    DataContractJsonSerializer jsonSerializer = new DataContractJsonSerializer(typeof(iFormBuilderOptionList));
                    iFormBuilderOptionList access = (iFormBuilderOptionList)jsonSerializer.ReadObject(request.GetResponseStream());

                    if (access.OPTIONLIST_ID != null)
                        return this.GetOptionList(int.Parse(access.OPTIONLIST_ID));
                }

                return null;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public bool DeleteOptionList(OptionList optionlist)
        {
            try
            {
                string requesturl = string.Format("{0}/optionlists/{1}", ProfileURL, optionlist.ID);
                WebResponse request = Utilities.HttpPost(requesturl, null, accesscode.access_token, Utilities.PostType.DELETE);
                if (request != null)
                {
                    DataContractJsonSerializer jsonSerializer = new DataContractJsonSerializer(typeof(iFormBuilderStatus));
                    iFormBuilderStatus access = (iFormBuilderStatus)jsonSerializer.ReadObject(request.GetResponseStream());
                    return access.STATUS;
                }

                return false;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        /// <summary>
        /// Gets all pages in profile.
        /// </summary>
        /// <returns>List<Page></returns>
        public List<Page> GetAllPagesInProfile()
        {
            try
            {
                Administration admin = new Administration();
                string requesturl = ProfileURL + "/pages";
                WebResponse request = Utilities.GetRequest(requesturl, accesscode);
                DataContractJsonSerializer jsonSerializer = new DataContractJsonSerializer(typeof(iFormBuilderPages));
                iFormBuilderPages access = (iFormBuilderPages)jsonSerializer.ReadObject(request.GetResponseStream());
                return access.PAGES;
            }
            catch (Exception ex)
            {
                throw new Exception(string.Format("Error getting pages from profile: {0}", ex.Message));
            }
        }

        public Page GetPage(long pageid)
        {
            try
            {
                string requesturl = this.ProfileURL + "/pages/" + pageid;
                WebResponse request = Utilities.GetRequest(requesturl, accesscode);
                DataContractJsonSerializer jsonSerializer = new DataContractJsonSerializer(typeof(iFormBuilderPage));
                iFormBuilderPage access = (iFormBuilderPage)jsonSerializer.ReadObject(request.GetResponseStream());

                if (access.PAGE.ID != 0)
                {
                    Page page = (Page)access.PAGE;
                    page.Elements = access.ELEMENTS;

                    //Look for any subforms in the page
                    foreach (Element ele in page.SubformElements)
                    {
                        //The DATA_SIZE is page id of a subform
                        Console.WriteLine(ele.NAME);
                        page.Subforms.Add((Page)this.GetPage(ele.DATA_SIZE));
                    }
                    return page;
                }

                return null;
            }
            catch (Exception ex)
            {
                throw new Exception(string.Format("Error getting page: {0}", ex.Message));
            }
        }

        public List<User> GetAllUserInProfile()
        {
            //https://ServerName.iformbuilder.com/exzact/api/profiles/ProfileId/uesrs
            Users users = new Users();
            users.DETAIL = true;
            //string json = JsonConvert.SerializeObject(users, Formatting.Indented);
            Administration admin = new Administration();
            string requesturl = ProfileURL + "/users?DETAIL='true'";
            WebResponse request = Utilities.GetJSONRequest(requesturl, accesscode);
            DataContractJsonSerializer jsonSerializer = new DataContractJsonSerializer(typeof(iFormUsers));
            iFormUsers access = (iFormUsers)jsonSerializer.ReadObject(request.GetResponseStream());
            return access.USERS;
        }

        public FormAssignment GetFormAssignment()
        {
            //Referer: http://esri.iformbuilder.com/exzact/adminAssignment.php
            string requesturl = string.Format("https://{0}.iformbuilder.com/exzact/_getDisplayDataJSON.php", iformconfig.iformserverurl);
            FormAssignment fm = new FormAssignment(requesturl, iformconfig.iformusername, iformconfig.iformpassword);
            return fm;
        }

        public Records GetRecords(Page page, long highestrecord, int favorranking)
        {
            string datastring = "";
            //TODO:  Implement Methods for handling the ID parameter.  This will correct issue with Date Slice not being fine grained enough
            if (highestrecord != 0)
                datastring = string.Format("&SINCE_ID={0}", highestrecord);

            //Download the List of IDs to parse
            string requesturl = string.Format("{0}/pages/{1}/feed?FORMAT=JSON&SORT=ID&DEEP=1&SORT_BY=ASC&LIMIT=250&FILTER[0][KEY]=ID&FILTER[0][CONDITION]=%3E&FILTER[0][VALUE]={2}", ProfileURL, page.ID, highestrecord);
            Records records = new Records(page, requesturl, iformconfig.iformusername, iformconfig.iformpassword, favorranking);
            records.RecordSet = records.GetPageFeed(requesturl, accesscode);
            return records;
        }

        public long CreateRecord(long pageid, RecordsCreator record)
        {
            try
            {
                string requesturl = ProfileURL + "/pages/" + pageid + "/records";
                string json = JsonConvert.SerializeObject(record, Formatting.Indented);
                WebResponse request = Utilities.JSONPost(requesturl, json, this.accesscode.access_token, Utilities.PostType.POST);
                if (request == null)
                    return -1;

                DataContractJsonSerializer jsonSerializer = new DataContractJsonSerializer(typeof(RecordsStatus));
                RecordsStatus access = (RecordsStatus)jsonSerializer.ReadObject(request.GetResponseStream());
                //Get the RecordID of the created record
                if (access.STATUS && access.RECORDS.Count != 0)
                    return access.RECORDS[0].RECORD_ID;

                return -1;
            }
            catch (Exception ex)
            {
                return -1;
            }
        }

        public bool EditRecord(Record record)
        {
            return true;
        }

        public bool DeleteRecords(List<long> pageid, List<int> DeleteRecords)
        {
            try
            {
                for (int i = 0; i < DeleteRecords.Count; i++)// record in DeleteRecords)
                {
                    DeleteRecord(pageid[i], DeleteRecords[i]);
                }

                return true;
            }
            catch (Exception ex)
            {
                throw new Exception(string.Format("Error getting page: {0}", ex.Message));
            }
        }

        public bool DeleteRecord(long pageid, int record)
        {
            //https:// SERVER_NAME.iformbuilder.com/exzact/api/profiles/PROFILE_ID/pages/PAGE_ID/records/RECORD_ID
            string requesturl = ProfileURL + "/pages/" + pageid + "/records/" + record;
            WebResponse request = Utilities.HttpPost(requesturl, null, this.accesscode.access_token, Utilities.PostType.POST);
            DataContractJsonSerializer jsonSerializer = new DataContractJsonSerializer(typeof(iFormBuilderStatus));
            iFormBuilderStatus access = (iFormBuilderStatus)jsonSerializer.ReadObject(request.GetResponseStream());
            return access.STATUS;
        }

        public bool AssignRecords(long pageid, long recordid, long assignto)
        {
            AssignTO aRecord = new AssignTO();
            aRecord.ASSIGN = assignto;
            //https://ServerName.iformbuilder.com/exzact/api/profiles/ProfileId/pages/PageId/records/RecordId/assignments
            string requesturl = string.Format("{0}/pages/{1}/records/{2}/assignments", ProfileURL, pageid, recordid);
            string json = JsonConvert.SerializeObject(aRecord, Formatting.Indented);
            WebResponse request = Utilities.JSONPost(requesturl, json, this.accesscode.access_token, Utilities.PostType.POST);
            DataContractJsonSerializer jsonSerializer = new DataContractJsonSerializer(typeof(iFormBuilderStatus));
            iFormBuilderStatus access = (iFormBuilderStatus)jsonSerializer.ReadObject(request.GetResponseStream());
            return access.STATUS;
        }

        private AccessCode _code {get; set;}
        public AccessCode accesscode
        {
            get
            {
                if (_code == null || _code.isExpired)
                {
                    string requesturl = "https://" + iformconfig.iformserverurl + ".iformbuilder.com/exzact/api/oauth/token";
                    string opts = string.Empty;
                    if (iformconfig.refreshcode.Length == 0)
                    {
                        //Get the Code using the Secret Key
                        
                        string jwt = CreateJWT(iformconfig.secretkey, iformconfig.clientid, requesturl, 600);
                        Console.Write(jwt);
                        opts = string.Format("grant_type=urn:ietf:params:oauth:grant-type:jwt-bearer&assertion={0}", jwt);
                    }
                    else
                    {
                        opts = "client_id=" + iformconfig.clientid + "&code=" + iformconfig.refreshcode + "&grant_type=refresh_token";
                    }

                    if (opts == String.Empty)
                        return null;

                    WebRequest webRequest = WebRequest.Create(requesturl);
                    webRequest.Method = "POST";
                    Stream os = null;
                    try
                    {
                        webRequest.ContentType = "application/x-www-form-urlencoded";
                        //byte[] bytes = Encoding.ASCII.GetBytes(opts);
                        byte[] bytes = Encoding.UTF8.GetBytes(opts);
                        webRequest.ContentLength = bytes.Length;   //Count bytes to send
                        os = webRequest.GetRequestStream();
                        os.Write(bytes, 0, bytes.Length);         //Send it
                    }
                    catch (WebException ex)
                    {
                    }
                    finally
                    {
                        if (os != null)
                        {
                            os.Close();
                        }
                    }

                    WebResponse webResponse = webRequest.GetResponse();
                    DataContractJsonSerializer jsonSerializer = new DataContractJsonSerializer(typeof(AccessCode));
                    _code = (AccessCode)jsonSerializer.ReadObject(webResponse.GetResponseStream());
                    return _code;
                }
                else
                    return _code;
            }
            set { _code = value; }
        }

        private string CreateJWT(string clientsecret,string clientkey,string clienturl, long expiration)
        {
            //Dim JWT As String
            //Dim EncodedJWT As Byte()
            String JWT;
            byte[] EncodedJWT;
            char q = (char)34;

            //Build Header and Claim Set String
            String strHeader = "{'alg':'HS256','typ':'JWT'}".Replace("'", q.ToString());
            TimeSpan timespan = DateTime.Now.ToUniversalTime().Subtract(new DateTime(1970, 1, 1));
            long iat =  timespan.Ticks / 10000000;
            long exp = iat + expiration;
            string strClaimSet = ("{" + string.Format("'iss':'{0}','aud':'{1}','exp':{2},'iat':{3}", clientkey, clienturl, exp.ToString(), iat.ToString()) + "}").Replace("'", q.ToString());

            //Build JWT String
            string headerBytes = ToBase64URL(strHeader);
            var claimerBytes = ToBase64URL(strClaimSet);
            JWT = string.Format("{0}.{1}", headerBytes, claimerBytes);

            //Convert Client Secret to UTF8 Byte()
            byte[] bytKey = System.Text.Encoding.UTF8.GetBytes(clientsecret);

            //Encode JWT String
            HMACSHA256 hmac_encode = new HMACSHA256(bytKey);
            EncodedJWT = hmac_encode.ComputeHash(System.Text.Encoding.UTF8.GetBytes(JWT));

            //Sign and return JWT
            String signature =  ToBase64URL(EncodedJWT);
            return string.Format("{0}.{1}", JWT, signature);
        }


        string ToBase64URL(string text)
        {
            byte[] b = System.Text.Encoding.UTF8.GetBytes(text);
            return ToBase64URL(b);
        }


    string ToBase64URL(byte[] byt)
    {
        string result = string.Empty;
        //Convert to base 64 string
        result = Convert.ToBase64String(byt);
        //Make URL Friendly
        result = result.Replace("+", "-").Replace("/", "_").Replace("=","");
        Console.WriteLine(result.IndexOf("="));
        return result;
      }

        public bool ReadConfiguration(string path)
        {
            //Force the System to get a new Access Code for this login
            _code = null;

            return iformconfig.ReadConfiguration(path);
        }

        public bool SaveConfiguration(string path)
        {
            return iformconfig.SaveConfiguration(path);
        }
    }
}
