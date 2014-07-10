using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.IO;
using System.ServiceModel.Web;
using System.Runtime.Serialization.Json;
using iFormBuilderAPI;
using System.Collections;


    /// <summary>
    /// 
    /// </summary>
    internal static class Utilities
    {
        /// <summary>
        /// 
        /// </summary>
        public enum PostType
        {
            POST,
            GET,
            DELETE,
            DOWNLOAD,
            PUT
        }
        /// <summary>
        /// Gets the request.
        /// </summary>
        /// <param name="url">The URL.</param>
        /// <param name="iFormConfig">The i form config.</param>
        /// <returns></returns>
        public static WebResponse GetRequest(string url, AccessCode iFormConfig)
        {
            WebRequest wrGETURL;

            //Append the Acces Token to the request
            url = url + "?ACCESS_TOKEN=" + iFormConfig.access_token + "&VERSION=5.1";

            wrGETURL = WebRequest.Create(url);
            return wrGETURL.GetResponse();
        }

        /// <summary>
        /// Gets the request.
        /// </summary>
        /// <param name="url">The URL.</param>
        /// <param name="iFormConfig">The i form config.</param>
        /// <returns></returns>
        public static WebResponse GetJSONRequest(string url, AccessCode iFormConfig)
        {
            try
            {
                WebRequest webRequest = WebRequest.Create(url);
                webRequest.Method = "GET";
                webRequest.ContentType = "application/json";
                webRequest.Headers.Add(HttpRequestHeader.Authorization, string.Format("Bearer {0}", iFormConfig.access_token));
                webRequest.Headers.Add("X-IFORM-API-VERSION", "5.1");
                return webRequest.GetResponse();
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public static WebResponse DownloadRequest(string url, string username, string password)
        {
            // parameters: name1=value1&name2=value2	
            WebRequest webRequest = WebRequest.Create(url);
            webRequest.Timeout = 600000;
            //webRequest.Method = postmethod.ToString();
                Stream os = null;
                try
                { // send the Post
                    //Add the Access Code and the Parameter to the String
                    string opts = "USERNAME=" + username + "&PASSWORD=" + password;
                    webRequest.ContentType = "application/x-www-form-urlencoded";

                    byte[] bytes = Encoding.UTF8.GetBytes(opts);
                    webRequest.ContentLength = bytes.Length;   //Count bytes to send
                    webRequest.Method = "POST";

                    os = webRequest.GetRequestStream();
                    os.Write(bytes, 0, bytes.Length);         //Send it
                    WebResponse webResponse = webRequest.GetResponse();
                    return webResponse;
                }
                catch (WebException ex)
                {
                     return null;
                }
                finally
                {
                    if (os != null)
                    {
                        os.Close();
                    }
                }
        }

        public static WebResponse JSONPost(string uri, string parameters, string accestoken, PostType postmethod)
        {
            // parameters: name1=value1&name2=value2	
            WebRequest webRequest = WebRequest.Create(uri);
            webRequest.Method = postmethod.ToString();
            if (postmethod != PostType.GET)
            {
                Stream os = null;
                try
                { // send the Post
                    //Add the Access Code and the Parameter to the String
                    Administration admin = new Administration();

                    webRequest.ContentType = "application/json";
                    webRequest.Headers.Add(HttpRequestHeader.Authorization, string.Format("Bearer {0}", accestoken));
                    webRequest.Headers.Add("X-IFORM-API-REQUEST-ENCODING", "JSON");
                    webRequest.Headers.Add("X-IFORM-API-VERSION", "5.1");
                    using (var streamWriter = new StreamWriter(webRequest.GetRequestStream()))
                    {
                        streamWriter.Write(parameters);
                    }
                    WebResponse webResponse = webRequest.GetResponse();
                    return webResponse;
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
            }

            return null;
        } // end HttpPos

        /// <summary>
        /// HTTPs the post.
        /// </summary>
        /// <param name="uri">The URI.</param>
        /// <param name="parameters">The parameters.</param>
        /// <param name="postmethod">The postmethod.</param>
        /// <returns></returns>
        public static WebResponse HttpPost(string uri, string parameters,string accestoken ,PostType postmethod)
        {
            // parameters: name1=value1&name2=value2	
            WebRequest webRequest = WebRequest.Create(uri);
            webRequest.Method = postmethod.ToString();
            if (postmethod != PostType.GET)
            {
                Stream os = null;
                try
                { // send the Post
                    //Add the Access Code and the Parameter to the String
                    Administration admin = new Administration();
                    string opts = "ACCESS_TOKEN=" + accestoken + "&VERSION=5.1";
                    if (parameters == null)
                        parameters = opts;
                    else
                        parameters = string.Format("{0}&{1}", opts, parameters);
                        
                    webRequest.ContentType = "application/x-www-form-urlencoded";
                    byte[] bytes = Encoding.ASCII.GetBytes(parameters);
                    webRequest.ContentLength = bytes.Length;   //Count bytes to send
                    webRequest.Method = postmethod.ToString();

                    os = webRequest.GetRequestStream();
                    os.Write(bytes, 0, bytes.Length);         //Send it
                    WebResponse webResponse = webRequest.GetResponse();
                    return webResponse;
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
            }

            return null;
        } // end HttpPos

        public static byte[] ToByteArray(BitArray bits)
        {
            byte[] ret = new byte[bits.Length / 8];
            bits.CopyTo(ret, 0);
            return ret;
        }
    }

