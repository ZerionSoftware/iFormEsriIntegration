using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
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
            //if(SinceDate.Ticks != 0)
            //   datastring  = string.Format("&SINCE_DATE={0}-{1}-{2}", SinceDate.Year, SinceDate.Month,SinceDate.Day);

            //TODO:  Implement Methods for handling the ID parameter.  This will correct issue with Date Slice not being fine grained enough
            if (highestrecord != 0)
                datastring = string.Format("&SINCE_ID={0}", highestrecord);

            //Download the List of IDs to parse
            string requesturl = string.Format("{0}/pages/{1}/feed?FORMAT=JSON&SORT=ID&DEEP=1&SORT_BY=ASC&LIMIT=250&FILTER[0][KEY]=ID&FILTER[0][CONDITION]=%3E&FILTER[0][VALUE]={2}", ProfileURL, page.ID, highestrecord);
            // requesturl = "https://" + iformconfig.iformserverurl + ".iformbuilder.com/exzact/dataJSON.php?TYPE=ID&PAGE_ID=" + page.ID + "&TABLE_NAME=_data" + iformconfig.profileid + "_" + page.NAME.ToLower() + datastring;
            Records records = new Records(page, requesturl, iformconfig.iformusername, iformconfig.iformpassword, favorranking);
            //List<long> ids = records.DownloadIDs(requesturl);
            //Sort the List of IDs
            
            records.RecordSet = records.GetPageFeed(requesturl, accesscode);
            /*foreach (long id in ids)
            {
                try
                {
                    requesturl = string.Format("https://{0}.iformbuilder.com/exzact/dataJSON.php?PAGE_ID={1}&TABLE_NAME=_data{2}&ID={3}", iformconfig.iformserverurl, page.ID, iformconfig.profileid + "_" + page.NAME.ToLower(), id);
                    result = records.CreateRecord(requesturl);
                    records.RecordSet.Add(result);
                }
                catch
                {
                }
            }*/
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
                    string opts = "client_id=" + iformconfig.clientid + "&code=" + iformconfig.refreshcode + "&grant_type=refresh_token";
                    string requesturl = "https://" + iformconfig.iformserverurl + ".iformbuilder.com/exzact/api/oauth/token";
                    // parameters: name1=value1&name2=value2	
                    WebRequest webRequest = WebRequest.Create(requesturl);
                    webRequest.Method = "POST";
                    Stream os = null;
                    try
                    {
                        webRequest.ContentType = "application/x-www-form-urlencoded";
                        byte[] bytes = Encoding.ASCII.GetBytes(opts);
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
                }

                return _code;
            }
            set { _code = value; }
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
