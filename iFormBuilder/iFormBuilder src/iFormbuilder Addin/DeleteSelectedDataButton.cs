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
using iFormGPTools;
using ESRI.ArcGIS.Carto;

namespace iFormToolbar
{
    public class DeleteSelectedDataButton : ESRI.ArcGIS.Desktop.AddIns.Button
    {
        IGeoProcessor2 _gp = null;
        /// <summary>
        /// A Queue of GPProcess objects each of which represents a geoprocessing tool to be executed asynchronously.
        /// </summary>
        private Queue<IGPProcess> _myGPToolsToExecute = new Queue<IGPProcess>();
        public DeleteSelectedDataButton()
        {

        }

        protected override void OnClick()
        {
            SelectionTargetComboBox s_combo = SelectionTargetComboBox.GetSelectionComboBox();
            iFormBuilderAPI.iFormBuilder api = new iFormBuilderAPI.iFormBuilder();
            api.ReadConfiguration(Utilities.iFormConfigFile);
            iFormBuilderAPI.Page page = s_combo.SelectedTable as iFormBuilderAPI.Page;
            try
            {
            }
            catch (Exception ex)
            {
                //listView1.Items.Add(new ListViewItem(new string[2] { "N/A", ex.Message }, "error"));
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
                    //Get the return value.
                    object returnValue = result.ReturnValue;
                    //This will return the Geodatabase created from these features
                    Type factoryType = Type.GetTypeFromProgID("esriDataSourcesGDB.FileGDBWorkspaceFactory");
                    IWorkspaceFactory workspaceFactory = (IWorkspaceFactory)Activator.CreateInstance
                        (factoryType);
                    IWorkspace inWorkspace = workspaceFactory.OpenFromFile(returnValue.ToString(), 0);
                    IEnumDatasetName enumDatasetName = inWorkspace.get_DatasetNames(esriDatasetType.esriDTAny);
                    IDatasetName datasetName = enumDatasetName.Next();
                    IFeatureWorkspace featureWorkspace = (IFeatureWorkspace)inWorkspace;
                    while (datasetName != null)
                    {
                        if (datasetName.Type == esriDatasetType.esriDTFeatureClass)
                        {
                            IFeatureLayer featureLayer = new FeatureLayer();// = (ESRI.ArcGIS.Carto.IFeatureLayer)datasetName;
                            featureLayer.Name = datasetName.Name;
                            featureLayer.FeatureClass = featureWorkspace.OpenFeatureClass(datasetName.Name);
                            ArcMap.Document.FocusMap.AddLayer(featureLayer);
                        }
                        //else
                        //    dataset = (IDataset)featureWorkspace.OpenTable(objectname);

                        datasetName = enumDatasetName.Next();
                    }
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
