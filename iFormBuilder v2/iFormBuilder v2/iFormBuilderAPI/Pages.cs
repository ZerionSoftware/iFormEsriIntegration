using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using System.Net;
using System.Runtime.Serialization.Json;
using iFormBuilderAPI.DataContractObjects;

namespace iFormBuilderAPI
{
    /// <summary>
    /// 
    /// </summary>
    public class Pages
    {
        IConfiguration _iFormConfig;
        /// <summary>
        /// Constructor Method for the Pages Call
        /// </summary>
        //public Pages()
        //{
        //    _iFormConfig = new Configuration();
        //}
        public Pages(IConfiguration iFormConfiguration)
        {
            _iFormConfig = iFormConfiguration;
        }

        /// <summary>
        /// Gets the page.
        /// </summary>
        /// <param name="page_id">The page_id.</param>
        /// <returns>Page</returns>
        public Page GetPage(long page_id)
        {
            try
            {
                Administration admin = new Administration();
                string requesturl = "https://" + _iFormConfig.iformserverurl + ".iformbuilder.com/exzact/api/profiles/" + _iFormConfig.profileid + "/pages/" + page_id;
                WebResponse request = Utilities.GetRequest(requesturl, admin.GetAccessCode());
                DataContractJsonSerializer jsonSerializer = new DataContractJsonSerializer(typeof(iFormBuilderPage));
                iFormBuilderPage access = (iFormBuilderPage)jsonSerializer.ReadObject(request.GetResponseStream());

                if (access.PAGE.ID != 0)
                {
                    Page page = (Page)access.PAGE;
                    page.Elements = access.ELEMENTS;

                    //Look for any subforms in the page
                    foreach (Element ele in page.SubformElements)
                    {
                        page.Subforms.Add((Page)this.GetPage(ele.DATA_SIZE));
                    }
                    return page;
                }

                return null;
            }
            catch (Exception ex)
            {
                return null;
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
                string requesturl = "https://" + _iFormConfig.iformserverurl + ".iformbuilder.com/exzact/api/profiles/" + _iFormConfig.profileid + "/pages";
                WebResponse request = Utilities.GetRequest(requesturl, admin.GetAccessCode());
                DataContractJsonSerializer jsonSerializer = new DataContractJsonSerializer(typeof(iFormBuilderPages));
                iFormBuilderPages access = (iFormBuilderPages)jsonSerializer.ReadObject(request.GetResponseStream());
                return access.PAGES;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public int CreatePage()
        {
            return 0;
        }

        public bool DeletePage(int pageid)
        {
            return false;
        }

        public bool AssignPage(int pageid)
        {
            return false;
        }
    }
}
