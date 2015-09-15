using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Security.Cryptography;

namespace iFormBuilderAPI
{
    /// <summary>
    /// Primary Configuration File for the iFormBuilder Toolkit
    /// </summary>
    public class Configuration : IConfiguration
    {
        public Configuration()
        {
        }

        private string _iformserverurl;
        public string iformserverurl
        {
            set { _iformserverurl = value; }
            get { return _iformserverurl; }
        }

        private string _clientid;
        public string clientid
        {
            set { _clientid = value; }
            get { return _clientid; }
        }

        private string _refreshcode;
        public string refreshcode
        {
            set { _refreshcode = value; }
            get { return _refreshcode; }
        }

        private string _secretkey;
        public string secretkey
        {
            set { _secretkey = value; }
            get { return _secretkey; }
        }

        private string _iformusername;
        public string iformusername
        {
            set { _iformusername = value; }
            get { return _iformusername; }
        }

        private string _iformpassword;
        public string iformpassword
        {
            set { _iformpassword = value; }
            get { return _iformpassword; }
        }
        private int _profileid;
        public int profileid
        {
            set { _profileid = value; }
            get { return _profileid; }
        }

        private string _pageid;
        public string pageid
        {
            set { _pageid = value; }
            get { return _pageid; }
        }

        private string _arcgisurl;
        public string arcgisurl
        {
            set { _arcgisurl = value; }
            get { return _arcgisurl; }
        }

        private string _arcgisusername;
        public string arcgisusername
        {
            set { _arcgisusername = value; }
            get { return _arcgisusername; }
        }

        private string _arcgispassword;
        public string arcgispassword
        {
            set { _arcgispassword = value; }
            get { return _arcgispassword; }
        }

        internal bool ReadConfiguration(string path)
        {
            try
            {
                XmlDocument doc = new XmlDocument();
                doc.Load(path);
                XmlNode node = doc.SelectSingleNode("config");
                foreach (XmlNode children in node.ChildNodes)
                {
                    switch (children.Name)
                    {
                        case "clientid":
                            this.clientid = children.InnerText;
                            break;
                        case "refreshcode":
                            this.refreshcode = children.InnerText;
                            break;
                        case "secretkey":
                            this.secretkey = children.InnerText;
                            break;
                        case "iformserverurl":
                            this.iformserverurl = children.InnerText;
                            break;
                        case "iformusername":
                            this.iformusername = children.InnerText;
                            break;
                        case "iformpassword":
                            this.iformpassword = children.InnerText;
                            break;
                        case "profileid":
                            this.profileid = int.Parse(children.InnerText);
                            break;
                        case "arcgisurl":
                            this._arcgisurl = children.InnerText;
                            break;
                        case "arcgisusername":
                            this._arcgisusername = children.InnerText;
                            break;
                        case "arcgispassword":
                            this.arcgispassword = children.InnerText;
                            break;
                    }
                }
                string URI = node.InnerText;
                return true;
            }
            catch
            { return false; }
        }

        internal bool SaveConfiguration(string path)
        {
            //TODO:  Implement Security on the passwords
            XmlTextWriter writer = new XmlTextWriter(path, null);

            //Write the root element
            writer.WriteStartElement("config");

            //Write sub-elements
            writer.WriteElementString("clientid", this.clientid);
            writer.WriteElementString("refreshcode", _refreshcode);
            writer.WriteElementString("iformserverurl", _iformserverurl);
            writer.WriteElementString("iformusername", _iformusername);
            writer.WriteElementString("iformpassword", _iformpassword);
            writer.WriteElementString("profileid", _profileid.ToString());
            writer.WriteElementString("arcgisurl", _arcgisurl);
            writer.WriteElementString("arcgisusername", _arcgisusername);
            writer.WriteElementString("arcgispassword", _arcgispassword);

            // end the root element
            writer.WriteEndElement();

            //Write the XML to file and close the writer
            writer.Close();
            return true;
        }
    }
}
