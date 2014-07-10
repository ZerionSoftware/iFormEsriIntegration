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
                Environment.GetFolderPath(Environment.SpecialFolder.Personal);
                string agsfolder = Environment.GetFolderPath(Environment.SpecialFolder.Personal) + "\\ArcGIS";
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
