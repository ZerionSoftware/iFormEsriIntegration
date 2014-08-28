using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using ESRI.ArcGIS.Carto;

namespace iFormToolbar
{
    public static class Utilities
    {
        public static string iFormFolder
        {
            get
            {
                string agsfolder = Environment.GetFolderPath(Environment.SpecialFolder.CommonDocuments) + "\\ArcGIS";
                string iformfolder = String.Empty;
                if (Directory.Exists(agsfolder))
                {
                    iformfolder = agsfolder + "\\iformbuilder";
                    if (!Directory.Exists(iformfolder))
                        Directory.CreateDirectory(iformfolder);
                }
                return iformfolder;
            }
        }

        public static string iFormSchedulerFolder
        {
            get
            {
                string iformfolder = iFormFolder;
                string schedulefolder = string.Format("{0}\\{1}", iformfolder, DateTime.Now.Ticks.ToString());
                if (!Directory.Exists(schedulefolder))
                    Directory.CreateDirectory(schedulefolder);
                return schedulefolder;
            }
        }

        private static string _iformconfigfile = iFormFolder + "\\config.xml";
        public static string iFormConfigFile
        {
            get
            {
                return _iformconfigfile;
            }
            set
            {
                _iformconfigfile = value;
            }
        }

        public static Boolean DoesConfigFileExist
        {
            get
            {
                FileInfo fi = new FileInfo(_iformconfigfile);
                return fi.Exists;
            }
        }

        public static string iFormToolbox
        {
            get { return Utilities.iFormFolder + "\\" + "iFormTools.tbx"; }
        }

        public static bool LayerExists(IMap map, string LayerName)
        {
            ILayer pLayer;
            IEnumLayer pLayers = map.Layers;
            pLayer = pLayers.Next();
            while (pLayer != null)
            {
                if (pLayer.Name == LayerName)
                    return true;
                pLayer = pLayers.Next();
            }
            return false;
        }
    }
}
