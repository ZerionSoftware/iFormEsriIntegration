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
    public class SyncSelectedDataButton : ESRI.ArcGIS.Desktop.AddIns.Button
    {
        IGeoProcessor2 _gp = null;
        Geoprocessor GP;
        /// <summary>
        /// A Queue of GPProcess objects each of which represents a geoprocessing tool to be executed asynchronously.
        /// </summary>
        private Queue<IGPProcess> _myGPToolsToExecute = new Queue<IGPProcess>();
        public SyncSelectedDataButton()
        {
            GP = new Geoprocessor();
        }

        private List<IWorkspace> processedworkspaces;
        protected override void OnClick()
        {
            //Read all the Layers in the MXD and process the Sync Table for that workspace
            IEnumLayer layers = ArcMap.Document.FocusMap.get_Layers();
            ILayer layer = layers.Next();
            SelectionTargetComboBox s_combo = SelectionTargetComboBox.GetSelectionComboBox();
            iFormBuilderAPI.iFormBuilder api = new iFormBuilderAPI.iFormBuilder();
            api.ReadConfiguration(Utilities.iFormConfigFile);
            processedworkspaces = new List<IWorkspace>();

            while (layer != null)
            {
                //if (layer.GetType() == typeof(IFeatureLayer))
                //{
                    IFeatureLayer fLayer = layer as IFeatureLayer;
                    IDataset pDataSet = fLayer.FeatureClass as IDataset;
                    try
                    {
                        if (!processedworkspaces.Contains(pDataSet.Workspace))
                        {
                            GPMessageEventHandler gpEventHandler = new GPMessageEventHandler();
                            //get an instance of the geoprocessor
                            //instruct the geoprocessing engine to overwrite existing datasets
                            GP.OverwriteOutput = true;
                            GP.AddToolbox(Utilities.iFormFolder + "\\" + "iFormTools.tbx");

                            // Create a variant. Data is in the workspace.
                            IVariantArray parameters = new VarArrayClass();
                            parameters.Add(pDataSet.Workspace.PathName);
                            parameters.Add(Utilities.iFormConfigFile);
                            parameters.Add(1);

                            GP.ToolExecuted += new EventHandler<ESRI.ArcGIS.Geoprocessor.ToolExecutedEventArgs>(gpToolExecuted);
                            GP.ExecuteAsync("SynciFormDatabase", parameters);

                            //add this workspace to the processed workspaces
                            processedworkspaces.Add(pDataSet.Workspace);
                        }
                    }
                    catch
                    {
                        MessageBox.Show("Synchronization Failed");
                    }
                    layer = layers.Next();
                //}
            }
        }

        private void gpToolExecuted(object sender, ToolExecutedEventArgs e)
        {
            try
            {
                IGeoProcessorResult2 result = e.GPResult as IGeoProcessorResult2;
                if (result.Status.Equals(esriJobStatus.esriJobSucceeded))
                {
                    //Check that there are no information or warning messages.
                    if (result.MaxSeverity == 0)
                    {
                        MessageBox.Show("Synchronization Completed");
                    }
                    else
                    {
                        MessageBox.Show("Synchronization Failed");
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
            catch { }
            finally
            {
                GP.ToolExecuted -= new EventHandler<ESRI.ArcGIS.Geoprocessor.ToolExecutedEventArgs>(gpToolExecuted);
            }
        }
    }
}
