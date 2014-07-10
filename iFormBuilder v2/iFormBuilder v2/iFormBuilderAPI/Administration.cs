using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.Serialization.Json;
using System.Text;

namespace iFormBuilderAPI
{
    public class Administration
    {
        Configuration _iFormConfig;
        public Administration()
        {
            _iFormConfig = new Configuration();
        }
        /// <summary>
        /// Retrive the access code used to handle interactions with iFormBuilder site
        /// </summary>
        public AccessCode GetAccessCode()
        {
            string opts = "client_id=" + _iFormConfig.clientid + "&code=" + _iFormConfig.refreshcode + "&grant_type=refresh_token";

            string requesturl = "https://" + _iFormConfig.iformserverurl + ".iformbuilder.com/exzact/api/oauth/token";
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
                //TODO:  Handle Exception
                //MessageBox.Show(ex.Message, "HttpPost: Request error",
                //   MessageBoxButtons.OK, MessageBoxIcon.Error);
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
            AccessCode access = (AccessCode)jsonSerializer.ReadObject(webResponse.GetResponseStream());
            return access;
        }
    }
}
