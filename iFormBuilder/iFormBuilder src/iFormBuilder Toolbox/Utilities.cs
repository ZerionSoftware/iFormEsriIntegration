using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace iFormBuilder_Toolbox
{
    public class Utilities
    {
        public static string iFormFolder
        {
            get
            {
                string agsfolder = string.Format("{0)\\{1}", Environment.GetFolderPath(Environment.GetFolderPath(Environment.SpecialFolder.CommonDocuments)), "ArcGIS");
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

        public static string iFormConfigFile
        {
            get
            {
                return iFormFolder + "\\config.xml";
            }
        }
    }

}
