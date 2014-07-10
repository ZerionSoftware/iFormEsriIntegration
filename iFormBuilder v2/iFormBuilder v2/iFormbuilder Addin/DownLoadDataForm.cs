using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.esriSystem;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Geoprocessing;
using ESRI.ArcGIS.Geoprocessor;
using GeoprocessorEventHelper;
using iFormBuilderAPI;

namespace iFormToolbar
{
    public partial class DownLoadDataForm : Form
    {
        Geoprocessor GP;
        public iFormBuilder iFormAPI { get; set; }
        public DownLoadDataForm()
        {
            InitializeComponent();
            iFormAPI = new iFormBuilder();
            iFormAPI.ReadConfiguration(Utilities.iFormConfigFile);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            //Check to see if a workspace has been set
            try
            {
                iFormBuilderAPI.Page page = (this.cboDownloadLayers.SelectedItem as PageItem).Page;
                GPMessageEventHandler gpEventHandler = new GPMessageEventHandler();
                //instruct the geoprocessing engine to overwrite existing datasets
                GP = new Geoprocessor();
                GP.OverwriteOutput = true;
                GP.AddToolbox(Utilities.iFormFolder + "\\" + "iFormTools.tbx");

                // Create a variant. Data is in the workspace.
                IVariantArray parameters = new VarArrayClass();
                iFormConfigurationWindow iFormWindow = iFormConfigurationWindow.GetiFormWindow();
                if (iFormWindow == null)
                    parameters.Add(Utilities.iFormFolder);
                else if (iFormWindow.label10.Text.Contains(".gdb"))
                    parameters.Add(iFormWindow.label10.Text);
                else
                    parameters.Add(Utilities.iFormFolder);

                parameters.Add(Utilities.iFormConfigFile);
                parameters.Add(page.NAME);
                parameters.Add(page.ID);
                parameters.Add(this.checkBox1.Checked.ToString());
                parameters.Add((cboLocationRanking.SelectedItem as LocationWidgetFavoring).Value);

                GP.ToolExecuted += new EventHandler<ESRI.ArcGIS.Geoprocessor.ToolExecutedEventArgs>(gpToolExecuted);
                GP.ExecuteAsync("DownloadiFormbuilderDatabase", parameters);

                System.Diagnostics.Trace.WriteLine("Done");
            }
            catch (Exception ex)
            {
                //listView1.Items.Add(new ListViewItem(new string[2] { "N/A", ex.Message }, "error"));
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
                        //Get the return value.
                        object returnValue = result.ReturnValue;
                        //This will return the Geodatabase created from these features
                        Type factoryType;
                        string path = returnValue.ToString();
                        if (path.Contains(".gdb"))
                            factoryType = Type.GetTypeFromProgID("esriDataSourcesGDB.FileGDBWorkspaceFactory");
                        else
                            factoryType = Type.GetTypeFromProgID("esriDataSourcesGDB.SdeWorkspaceFactory");
                        IWorkspaceFactory workspaceFactory = (IWorkspaceFactory)Activator.CreateInstance
                            (factoryType);
                        IWorkspace inWorkspace = workspaceFactory.OpenFromFile(path, 0);
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
                                //Check to see if this feature layer is already in the map
                                if (!Utilities.LayerExists(ArcMap.Document.FocusMap, featureLayer.Name))
                                    ArcMap.Document.FocusMap.AddLayer(featureLayer);
                            }
                            datasetName = enumDatasetName.Next();
                        }
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
            catch
            { }
            finally
            {
                GP.ToolExecuted -= new EventHandler<ESRI.ArcGIS.Geoprocessor.ToolExecutedEventArgs>(gpToolExecuted);
            }
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void DownLoadDataForm_Load(object sender, EventArgs e)
        {
            try
            {
                //Bind the Pages to the Combo Box
                iFormConfigurationWindow iWindow = iFormConfigurationWindow.GetiFormWindow();
                this.cboDownloadLayers.DataSource = iWindow.Pages;
                this.cboDownloadLayers.DisplayMember = "Name";

                this.cboLocationRanking.Items.Clear();
                this.cboLocationRanking.Items.Add(new LocationWidgetFavoring(0, "Default"));
                this.cboLocationRanking.Items.Add(new LocationWidgetFavoring(1,"Location Widget"));
                this.cboLocationRanking.Items.Add(new LocationWidgetFavoring(2,"Created Location"));
                this.cboLocationRanking.Items.Add(new LocationWidgetFavoring(3,"Modified Location"));
                this.cboLocationRanking.DisplayMember = "Name";
                this.cboLocationRanking.SelectedIndex = 0;

            }
            catch
            {
                return;
            }
        }
    }

    public class LocationWidgetFavoring
    {
        public LocationWidgetFavoring(int code, String name)
        {
            Value = code;
            Name = name;
        }

        public int Value { get; set; }
        public string Name { get; set; }

    }

    public class PageItem
    {
        public PageItem(iFormBuilderAPI.Page pg, String name)
        {
            Page = pg;
            Name = name;
        }

        public iFormBuilderAPI.Page Page { get; set; }
        public string Name { get; set; }

    }
}
