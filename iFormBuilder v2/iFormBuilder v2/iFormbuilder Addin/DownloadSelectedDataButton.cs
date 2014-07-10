// Copyright 2012 ESRI
// 
// All rights reserved under the copyright laws of the United States
// and applicable international laws, treaties, and conventions.
// 
// You may freely redistribute and use this sample code, with or
// without modification, provided you include the original copyright
// notice and use restrictions.
// 
// See the use restrictions at <your ArcGIS install location>/DeveloperKit10.1/userestrictions.txt.
// 

using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using ESRI.ArcGIS.esriSystem;
using ESRI.ArcGIS.Framework;
using iFormTools;
using ESRI.ArcGIS.Geoprocessor;
using ESRI.ArcGIS.Geoprocessing;
using ESRI.Solutions.iFormBuilder.GPTools;
using ESRI.ArcGIS.Geodatabase;
using System.Diagnostics;
using GeoprocessorEventHelper;
using ESRI.ArcGIS.Carto;

namespace iFormToolbar
{
    public class DownloadSelectedDataButton : ESRI.ArcGIS.Desktop.AddIns.Button
    {
        public DownloadSelectedDataButton()
        {
        }

        protected override void OnClick()
        {
            //Open the Windows Form
            DownLoadDataForm dForm = new DownLoadDataForm();
            dForm.Show();
            return;
        }  
    }
}
