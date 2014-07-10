using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ESRI.ArcGIS.Geoprocessor;
using ESRI.ArcGIS.esriSystem;
using ESRI.ArcGIS.Geoprocessing;
using System.Runtime.InteropServices;

namespace iFormGPTools
{
    public class DataDownloaderTool : IGPProcess
    {
        public object[] m_Parameters;
        public DataDownloaderTool(string config,string workdirectory, string tablename,long pageid, int favorranking )
        {
            m_Parameters = new object[4];
            m_Parameters[0] = workdirectory;
            m_Parameters[1] = config;
            m_Parameters[2] = tablename;
            m_Parameters[3] = pageid;
            m_Parameters[4] = favorranking;
        }

        public string Alias
        {
            get { return "DatadownloadTool"; }
        }

        public object[] ParameterInfo
        {
            get { return m_Parameters; }
        }

        public string ToolName
        {
            get { return "DownloadiFormbuilderDatabase"; }
        }

        public string ToolboxDirectory
        {
            get { return "DownloadiFormbuilderDatabase"; }
        }

        public string ToolboxName
        {
            get { return "DownloadiFormbuilderDatabase"; }
        }
    }
}
