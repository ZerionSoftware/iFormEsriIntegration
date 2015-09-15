using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ESRI.ArcGIS.SystemUI;
using iFormBuilderAPI;

namespace iFormTools
{
    public class DataUploader
    {
        public IArcGISPortal Portal(Configuration config)
        {
            ArcGISSingleSignon signon = new ArcGISSingleSignon();
            IArcGISPortal portal = signon as IArcGISPortal;
            portal. = "test";

            string token = String.Empty;
            string referer = config.arcgispassword;
            int expire = 0;
            string user = config.arcgisusername;

            signon.GetToken(0,ref token,ref referer,ref expire,ref user);
            //signon.GetCurrentToken(

            return signon as IArcGISPortal;
        }
    }
}
