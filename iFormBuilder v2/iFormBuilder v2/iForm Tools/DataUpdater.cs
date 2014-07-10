using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ESRI.ArcGIS.Geodatabase;
using iFormBuilderAPI;

namespace iFormTools
{
    public class DataUpdater
    {
        Configuration m_config { get; set; }
        public DataUpdater(IConfiguration config)
        {
            m_config = config as Configuration;
        }
        public void UpdateElementFromFeature(IFeature feature, Page schemapage)
        {

        }
    }
}
