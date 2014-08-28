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

namespace iFormToolbar
{
    public class ToggleConfigWinBtn : ESRI.ArcGIS.Desktop.AddIns.Button
    {
        public ToggleConfigWinBtn()
        {

        }

        protected override void OnClick()
        {
            IDockableWindow dockWindow = SelectionExtension.GetConfigWindow();

            if (dockWindow == null)
                return;

            dockWindow.Show(!dockWindow.IsVisible());
        }

        protected override void OnUpdate()
        {
            this.Enabled = SelectionExtension.IsExtensionEnabled();
        }
    }

}
