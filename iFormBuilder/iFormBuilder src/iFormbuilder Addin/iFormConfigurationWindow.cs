using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Xml;
using iFormBuilderAPI;
using ESRI.ArcGIS.CatalogUI;
using ESRI.ArcGIS.Catalog;
using ESRI.ArcGIS.Geodatabase;
using iFormToolbar.Properties;

namespace iFormToolbar
{
    public partial class iFormConfigurationWindow : UserControl
    {
        private static bool s_enabled;
        private static iFormConfigurationWindow s_window;
        private static System.Windows.Forms.TextBox s_textbox;
        public iFormBuilder iFormAPI { get; set; }
        public List<PageItem> Pages { get; set; }
        public iFormConfigurationWindow(object hook)
        {
            InitializeComponent();
            this.Hook = hook;
            UpdateIFormsConfiguration();
            s_window = this;
        }

        internal static iFormConfigurationWindow GetiFormWindow()
        {
            return s_window;
        }

        private void UpdateIFormsConfiguration()
        {
            if (File.Exists(Utilities.iFormConfigFile))
            {
                iFormAPI = new iFormBuilder();
                iFormAPI.ReadConfiguration(Utilities.iFormConfigFile);

                this.txtClientID.Text = iFormAPI.iformconfig.clientid;
                this.txtRefreshCode.Text = iFormAPI.iformconfig.refreshcode;
                this.txtiFormServerURL.Text = iFormAPI.iformconfig.iformserverurl;
                this.txtiFormUserName.Text = iFormAPI.iformconfig.iformusername;
                this.txtiFormpassword.Text = iFormAPI.iformconfig.iformpassword;
                this.txtArcGISURL.Text = iFormAPI.iformconfig.arcgisurl;
                this.txtArcGISUsername.Text = iFormAPI.iformconfig.arcgisusername;
                this.txtArcGISPassword.Text = iFormAPI.iformconfig.arcgispassword;
                this.txtProfileID.Text = iFormAPI.iformconfig.profileid.ToString();
                this.txtSecretKey.Text = iFormAPI.iformconfig.secretkey;
                UpdatePageInformation();
            }

        }

        private void UpdatePageInformation()
        {
            try
            {
                this.Pages = new List<PageItem>();
                List<iFormBuilderAPI.Page> getPages = iFormAPI.GetAllPagesInProfile();
                if (getPages == null)
                    return;

                PageItem pg;
                foreach (iFormBuilderAPI.Page page in getPages)
                {
                    pg = new PageItem(page, page.NAME);
                    this.Pages.Add(pg);
                }
            }
            catch
            {
                return;
            }
        }

        /// <summary>
        /// Host object of the dockable window
        /// </summary>
        private object Hook
        {
            get;
            set;
        }

        internal static bool Exists
        {
            get
            {
                return (s_textbox == null) ? false : true;
            }
        }

        internal static void Clear()
        {
            if (s_textbox != null)
                s_textbox.Text = "";
        }

        internal static void AddItem(string layerName, int selectionCount)
        {
            if (s_textbox == null)
                return;
        }

        internal static void SetEnabled(bool enabled)
        {
            s_enabled = enabled;

            // if the dockable window was never displayed, listview could be null
            if (s_textbox == null)
                return;

            if (enabled)
            {
            }
            else
            {
                Clear();
            }
        }

        /// <summary>
        /// Implementation class of the dockable window add-in. It is responsible for 
        /// creating and disposing the user interface class of the dockable window.
        /// </summary>
        public class AddinImpl : ESRI.ArcGIS.Desktop.AddIns.DockableWindow
        {
            private iFormConfigurationWindow m_windowUI;
            public AddinImpl()
            {
            }

            protected override IntPtr OnCreateChild()
            {
                m_windowUI = new iFormConfigurationWindow(this.Hook);
                return m_windowUI.Handle;
            }

            protected override void Dispose(bool disposing)
            {
                if (m_windowUI != null)
                    m_windowUI.Dispose(disposing);

                base.Dispose(disposing);
            }
        }

        private void loadConfigurationToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //Load an existing configuration file
            OpenFileDialog fdlg = new OpenFileDialog();
            fdlg.Title = "Load Existing Configuration";
            fdlg.InitialDirectory = Utilities.iFormFolder;
            fdlg.Filter = "All files (*.*)|*.*|All files (*.*)|*.*";
            fdlg.FilterIndex = 2;
            fdlg.RestoreDirectory = true;
            if (fdlg.ShowDialog() == DialogResult.OK)
            {
                Utilities.iFormConfigFile = fdlg.FileName;
                UpdateIFormsConfiguration();
            }
        }

        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //Save the configuration
            saveconfig();
        }

        private void saveAsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Displays a SaveFileDialog so the user can save the Image
            // assigned to Button2.
            SaveFileDialog saveFileDialog1 = new SaveFileDialog();
            saveFileDialog1.Filter = "XML |*.xml";
            saveFileDialog1.Title = "Save the Configuration File";
            saveFileDialog1.ShowDialog();

            // If the file name is not an empty string open it for saving.
            if (saveFileDialog1.FileName != "")
            {
                // Saves the Image via a FileStream created by the OpenFile method.
                Utilities.iFormConfigFile = saveFileDialog1.FileName;
                saveconfig();
            }
        }

        private void saveconfig()
        {
            iFormAPI.iformconfig.clientid = this.txtClientID.Text;
            iFormAPI.iformconfig.refreshcode = this.txtRefreshCode.Text;
            iFormAPI.iformconfig.secretkey = this.txtSecretKey.Text;
            iFormAPI.iformconfig.iformserverurl = this.txtiFormServerURL.Text;
            iFormAPI.iformconfig.iformusername = this.txtiFormUserName.Text;
            iFormAPI.iformconfig.iformpassword = this.txtiFormpassword.Text;
            iFormAPI.iformconfig.arcgisurl = this.txtArcGISURL.Text;
            iFormAPI.iformconfig.arcgisusername = this.txtArcGISUsername.Text;
            iFormAPI.iformconfig.arcgispassword = this.txtArcGISPassword.Text;
            iFormAPI.iformconfig.profileid = int.Parse(this.txtProfileID.Text);

            iFormAPI.SaveConfiguration(Utilities.iFormConfigFile);
            UpdateIFormsConfiguration();
        }

        private void label10_Click(object sender, EventArgs e)
        {

        }

        private void btnSetWorkspace_Click(object sender, EventArgs e)
        {

        }

        private void setWorkspaceToolStripMenuItem_Click(object sender, EventArgs e)
        {
            IGxDialog gxDialog = new GxDialog();
            IGxObjectFilter gxObjectFilter_FGDB = new GxFilterFileGeodatabases();
            IGxObjectFilterCollection gxObjectFilterCollection = (IGxObjectFilterCollection)gxDialog;
            gxObjectFilterCollection.AddFilter(gxObjectFilter_FGDB, false);
            if (gxDialog.DoModalSave(0))
            {
                string workspaceName = gxDialog.FinalLocation.FullName + "/" + gxDialog.Name;
                this.label10.Text = workspaceName;
            }
        }

        private void createToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //A Workspace will only be created a File Geodatabase
            String fileName = DateTime.Now.ToFileTimeUtc().ToString();
            IWorkspace workspace = ArcGISTools.Utilities.CreateWorkspace(Utilities.iFormTempFolder, fileName);
            this.label10.Text = workspace.PathName;
        }

        private void validateToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //Check the Settings in the Configuration File
            picServerValid.Visible = true;
            picServerValid.Image = Resources.check_symbol_grn;

        }

        private void toolStripMenuItem2_Click(object sender, EventArgs e)
        {
            /*iFormConfigurationWindow iFormWindow = iFormConfigurationWindow.GetiFormWindow();
            if (!iFormWindow.label10.Text.Contains(".gdb"))
                return;

            //Create a Scheduled Task Based on the Synchorization Task that is currently displayed
            // Get the service on the local machine
            using (TaskService ts = new TaskService())
            {
                // Create a new task definition and assign properties
                TaskDefinition td = ts.NewTask();
                td.RegistrationInfo.Description = string.Format("Automatic updates for: {0}", iFormWindow.label10.Text);

                // Create a trigger that will fire the task at this time every other day
                td.Triggers.Add(new DailyTrigger { DaysInterval = 2 });

                // Create an action that will launch Notepad whenever the trigger fires
                //TODO:  Create a Auto Updater Python Script in the iFormBuilder Documents Locations
                string mydocpath = Utilities.iFormSchedulerFolder;
                string scheduleFile = mydocpath + @"\AutoUpdater.py";
                StringBuilder sb = new StringBuilder();

                sb.AppendLine("import arcpy");
                sb.AppendLine(string.Format("arcpy.ImportToolbox(\"{0}\\iFormTools.tbx\")",Utilities.iFormFolder));

                sb.AppendLine(string.Format("arcpy.SynciFormDatabase(\"{0}\",\"{1}\",{2})", iFormWindow.label10.Text, Utilities.iFormConfigFile, 1));
                sb.AppendLine();

                using (StreamWriter outfile = new StreamWriter(scheduleFile, true))
                {
                    outfile.Write(sb.ToString());
                }

                td.Actions.Add(new ExecAction(@"C:\Python27\ArcGIS10.1\python.exe",string.Format(@"{0}",scheduleFile), null));

                // Register the task in the root folder
                ts.RootFolder.RegisterTaskDefinition(string.Format("AutoUpdate{0}",DateTime.Now.Ticks.ToString()), td);
            }*/
        }

        private void label12_Click(object sender, EventArgs e)
        {

        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }
    }
}
