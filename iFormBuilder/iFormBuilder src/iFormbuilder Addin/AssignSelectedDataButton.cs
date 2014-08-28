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
using System.Windows.Forms;

namespace iFormToolbar
{
    public class AssignSelectedDataButton : ESRI.ArcGIS.Desktop.AddIns.Button
    {

        IGeoProcessor2 _gp = null;
        /// <summary>
        /// A Queue of GPProcess objects each of which represents a geoprocessing tool to be executed asynchronously.
        /// </summary>
        private Queue<IGPProcess> _myGPToolsToExecute = new Queue<IGPProcess>();
        public AssignSelectedDataButton()
        {

        }

        protected override void OnClick()
        {
            try
            {
                if (!Utilities.DoesConfigFileExist)
                {
                    //Send a warning that the extension needs to be setup
                    MessageBox.Show(string.Format("No Configuration File Available.  Please save config.xml at {0}", Utilities.iFormFolder));
                }

                    UserTargetComboBox s_combo = UserTargetComboBox.GetSelectionComboBox();
                    if (s_combo.SelectedTable == null)
                        return;

                    iFormBuilderAPI.iFormBuilder api = new iFormBuilderAPI.iFormBuilder();
                    api.ReadConfiguration(Utilities.iFormConfigFile);
                    iFormBuilderAPI.User user = s_combo.SelectedTable as iFormBuilderAPI.User;

                    // Update the contents of the listView, when the selection changes in the map. 
                    IFeatureLayer featureLayer;
                    IFeatureSelection featSel;

                    SelCountDockWin.Clear();

                    ICursor cur;
                    IQueryFilter qF = new QueryFilter();
                    qF.WhereClause = "1=1";
                    // Loop through the layers in the map and add the layer's name and
                    // selection count to the list box
                    for (int i = 0; i < ArcMap.Document.FocusMap.LayerCount; i++)
                    {
                        if (ArcMap.Document.FocusMap.get_Layer(i) is IFeatureSelection)
                        {
                            featureLayer = ArcMap.Document.FocusMap.get_Layer(i) as IFeatureLayer;
                            if (featureLayer == null)
                                break;

                            featSel = featureLayer as IFeatureSelection;
                            IFeature feat;
                            IFeatureCursor featCur;

                            int count = 0;
                            if (featSel.SelectionSet != null)
                            {
                                featSel.SelectionSet.Search(qF, true, out cur);
                                featCur = (IFeatureCursor)cur;
                                feat = featCur.NextFeature();
                                while (feat != null)
                                {
                                    count = int.Parse(feat.get_Value(feat.Fields.FindField("ID")).ToString());
                                    //Assign this record to the user in the ComboBox
                                    api.AssignRecords(long.Parse(feat.get_Value(feat.Fields.FindField("PAGEID")).ToString()), int.Parse(feat.get_Value(feat.Fields.FindField("ID")).ToString()), user.ID);
                                    feat = featCur.NextFeature();
                                }

                                //Do Until (pFeature Is Nothing)
                            }
                        }
                    }
                }
         
            catch (Exception ex)
            {
            }
        }

        private void gpToolExecuted(object sender, ToolExecutedEventArgs e)
        {
            IGeoProcessorResult2 result = e.GPResult as IGeoProcessorResult2;
            if (result.Status.Equals(esriJobStatus.esriJobSucceeded))
            {
                //Check that there are no information or warning messages.
                if (result.MaxSeverity == 0)
                {
                }
                else
                {
                    //Application specific code.
                }
            }
            else
            {
                //Get all messages.
                IGPMessages msgs = result.GetResultMessages();
                for (int i = 0; i < result.MessageCount; i++)
                {
                    IGPMessage2 msg = msgs.GetMessage(i) as IGPMessage2;
                    //Application specific code.
                }
            }
        }
        protected override void OnUpdate()
        {
            this.Enabled = SelectionExtension.IsExtensionEnabled();
        }

        static void OnGPPostToolExecute(object sender, GPPostToolExecuteEventArgs e)
        {
            System.Diagnostics.Trace.WriteLine(e.Result.ToString());
        }

        static void OnGPToolboxChanged(object sender, EventArgs e)
        {
            System.Diagnostics.Trace.WriteLine("OnGPToolboxChanged");
        }

        static void OnGPPreToolExecute(object sender, GPPreToolExecuteEventArgs e)
        {
            System.Diagnostics.Trace.WriteLine(e.Description);
        }

        static void OnGPMessage(object sender, GPMessageEventArgs e)
        {
            System.Diagnostics.Trace.WriteLine(e.Message);
        }
    }
}
