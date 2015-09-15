using System;
using System.Collections;
using System.Data.SqlClient;
using System.IO;
using System.Runtime.InteropServices;
using System.Xml;
using ESRI.ArcGIS.esriSystem;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Geometry;
using ESRI.ArcGIS.Geoprocessing;
using iFormTools;
using ESRI.ArcGIS.DataSourcesFile;


namespace ESRI.Solutions.iFormBuilder.GPTools
{
    public class SynciFormDatabase : IGPFunction
    {
        // Local members
        private string m_ToolName = "SynciFormDatabase"; //Function Name
        private string m_DisplayName = "SynciFormDatabase"; //Function Name
        private string m_metadatafile = "SynciFormDatabase.xml";
        private IArray m_Parameters;             // Array of Parameters
        private IGPUtilities m_GPUtilities;      // GPUtilities object

        public SynciFormDatabase()
        {
            m_GPUtilities = new GPUtilitiesClass();
        }
        #region IGPFunction Members

        // Set the name of the function tool. 
        // This name appears when executing the tool at the command line or in scripting. 
        // This name should be unique to each toolbox and must not contain spaces.
        public string Name
        {
            get { return m_ToolName; }
        }

        // Set the function tool Display Name as seen in ArcToolbox.
        public string DisplayName
        {
            get { return m_DisplayName; }
        }

        public IArray InputParameters
        {
            set
            {
                m_Parameters = value;
            }
            get
            {
                return m_Parameters;
            }
        }

        private IGPMessages m_message;
        public IGPMessages Messages
        {
            get { return m_message; }
            set { m_message = value; }
        }

        // This is the location where the parameters to the Function Tool are defined. 
        // This property returns an IArray of parameter objects (IGPParameter). 
        // These objects define the characteristics of the input and output parameters. 
        public IArray ParameterInfo
        {
            get
            {
                if (m_Parameters == null)
                {
                    m_Parameters = new ArrayClass();
                    IGPParameterEdit param1 = APLGPUtils.CreateParameterEdit("WorkingDirectory", "Working Directory", esriGPParameterDirection.esriGPParameterDirectionInput, esriGPParameterType.esriGPParameterTypeRequired, (IGPDataType)new GPStringTypeClass(), true) as IGPParameterEdit;
                    m_Parameters.Add(param1);

                    IGPParameterEdit param1a = APLGPUtils.CreateParameterEdit("ConfigurationFile", "Configuration File", esriGPParameterDirection.esriGPParameterDirectionInput, esriGPParameterType.esriGPParameterTypeRequired, (IGPDataType)new DEFileType(), true) as IGPParameterEdit;
                    m_Parameters.Add(param1a);

                    IGPParameterEdit param3 = APLGPUtils.CreateParameterEdit("FavorRanking", "Favor Ranking for Location", esriGPParameterDirection.esriGPParameterDirectionInput, esriGPParameterType.esriGPParameterTypeRequired, (IGPDataType)new GPLongTypeClass(), true) as IGPParameterEdit;
                    m_Parameters.Add(param3);

                    //IGPParameterEdit param2a = APLGPUtils.CreateParameterEdit("TableName", "Table Name from iFormBuilder to Download", esriGPParameterDirection.esriGPParameterDirectionInput, esriGPParameterType.esriGPParameterTypeRequired, (IGPDataType)new GPStringTypeClass(), true) as IGPParameterEdit;
                    //m_Parameters.Add(param2a);

                    //IGPParameterEdit param2 = APLGPUtils.CreateParameterEdit("PageID", "Page From iformbuilder to download", esriGPParameterDirection.esriGPParameterDirectionInput, esriGPParameterType.esriGPParameterTypeRequired, (IGPDataType)new GPStringTypeClass(), true) as IGPParameterEdit;
                    //m_Parameters.Add(param2);

                    //IGPParameterEdit paramdate = APLGPUtils.CreateParameterEdit("SinceDate", "Date For the Sync", esriGPParameterDirection.esriGPParameterDirectionInput, esriGPParameterType.esriGPParameterTypeRequired, (IGPDataType)new GPDateTypeClass(), true) as IGPParameterEdit;
                    //m_Parameters.Add(paramdate);

                    //IGPParameterEdit outworkspace = APLGPUtils.CreateParameterEdit("outworkspace", "Output Workspace", esriGPParameterDirection.esriGPParameterDirectionOutput, esriGPParameterType.esriGPParameterTypeDerived, (IGPDataType)new GPStringTypeClass(), true) as IGPParameterEdit;
                    //m_Parameters.Add(outworkspace);
                }
                return m_Parameters;
            }
        }

        // This method will update the output parameter value with the additional area field.
        public void UpdateParameters(IArray paramvalues, IGPEnvironmentManager pEnvMgr)
        {
            m_Parameters = paramvalues;
        }

        // Validate: This will validate each parameter and return messages.
        // This method will check that a given set of parameter values are of the 
        // appropriate number, DataType, and Value.
        public IGPMessages Validate(IArray paramvalues, bool updateValues, IGPEnvironmentManager envMgr)
        {
            if (m_Parameters == null)
                m_Parameters = ParameterInfo;

            // Call InternalValidate (Basic Validation). Are all the required parameters supplied?
            // Are the Values to the parameters the correct data type?
            IGPMessages validateMsgs = m_GPUtilities.InternalValidate(m_Parameters, paramvalues, updateValues, true, envMgr);

            if (m_GPUtilities == null) m_GPUtilities = new GPUtilitiesClass();
            return validateMsgs;
        }

        /// <summary>
        /// Executes the specified paramvalues.
        /// </summary>
        /// <param name="paramvalues">The paramvalues.</param>
        /// <param name="trackcancel">The trackcancel.</param>
        /// <param name="envMgr">The env MGR.</param>
        /// <param name="message">The message.</param>
        public void Execute(IArray paramvalues, ITrackCancel trackcancel, IGPEnvironmentManager envMgr, IGPMessages message)
        {
            try
            {
                if (paramvalues == null)
                    paramvalues = this.InputParameters;

                IGPMessages gpmessages = Validate(paramvalues, false, envMgr);
                IGPUtilities gpUtils = new GPUtilitiesClass();
                IGPParameter gpParameter;
                if (message == null)
                    message = m_message;

                if (message != null)
                    message.AddMessage("Synchronizing Database");
                gpParameter = (IGPParameter)paramvalues.get_Element(0);
                string path = gpUtils.UnpackGPValue(gpParameter).GetAsText();
                if (message != null)
                    message.AddMessage(string.Format("Data will be synchronized at: {0}", path));

                gpParameter = (IGPParameter)paramvalues.get_Element(1);
                string configfile = gpUtils.UnpackGPValue(gpParameter).GetAsText();
                if (message != null)
                    message.AddMessage(string.Format("Configuration file used will be: {0}",configfile));

                gpParameter = (IGPParameter)paramvalues.get_Element(2);
                int favorranking = int.Parse(gpUtils.UnpackGPValue(gpParameter).GetAsText());
                if (message != null)
                    message.AddMessage(string.Format("Favor Ranking is: {0}", favorranking.ToString()));

                iFormBuilderAPI.iFormBuilder api = new iFormBuilderAPI.iFormBuilder();
                api.ReadConfiguration(configfile);
                string addstring = String.Empty;
                if (addstring == String.Empty)
                {
                    if (message != null)
                        message.AddMessage(string.Format("Database Sync Started {0}", "TEST"));
                }
                //This line will do nothing

                DataDownloader target = new DataDownloader(api.iformconfig); // TODO: Initialize to an appropriate value
                string expected = string.Empty; // TODO: Initialize to an appropriate value
                Type factoryType;
                if(path.Contains(".gdb"))
                    factoryType = Type.GetTypeFromProgID("esriDataSourcesGDB.FileGDBWorkspaceFactory");
                else
                     factoryType = Type.GetTypeFromProgID("esriDataSourcesGDB.SdeWorkspaceFactory");
                
                IWorkspaceFactory workspaceFactory = (IWorkspaceFactory)Activator.CreateInstance
                    (factoryType);
                IWorkspace inWorkspace = workspaceFactory.OpenFromFile(path, 0);
                inWorkspace = target.SynchronizeData(inWorkspace, favorranking);
                if (message != null)
                    message.AddMessage("Data Synchronized from the server");
            }
            catch (Exception ex)
            {
                if (message != null)
                    message.AddError(8565957, ex.Message + ex.StackTrace);
            }
            finally
            {
                if (message != null)
                    message.AddMessage("Finished Downloading Database");
            }
        }

        // This is the function name object for the Geoprocessing Function Tool. 
        // This name object is created and returned by the Function Factory.
        // The Function Factory must first be created before implementing this property.
        public IName FullName
        {
            get
            {
                // Add CalculateArea.FullName getter implementation
                IGPFunctionFactory functionFactory = new ESRI.Solutions.iFormBuilder.GPTools.GPFactory();
                return (IName)functionFactory.GetFunctionName(m_ToolName);
            }
        }

        // This is used to set a custom renderer for the output of the Function Tool.
        public object GetRenderer(IGPParameter pParam)
        {
            return null;
        }

        // This is the unique context identifier in a [MAP] file (.h). 
        // ESRI Knowledge Base article #27680 provides more information about creating a [MAP] file. 
        public int HelpContext
        {
            get { return 0; }
        }

        // This is the path to a .chm file which is used to describe and explain the function and its operation. 
        public string HelpFile
        {
            get { return ""; }
        }

        // This is used to return whether the function tool is licensed to execute.
        public bool IsLicensed()
        {
            return true;
        }

        // This is the name of the (.xml) file containing the default metadata for this function tool. 
        // The metadata file is used to supply the parameter descriptions in the help panel in the dialog. 
        // If no (.chm) file is provided, the help is based on the metadata file. 
        // ESRI Knowledge Base article #27000 provides more information about creating a metadata file.
        public string MetadataFile
        {
            get { return m_metadatafile; }
        }

        // This is the class id used to override the default dialog for a tool. 
        // By default, the Toolbox will create a dialog based upon the parameters returned 
        // by the ParameterInfo property.
        public UID DialogCLSID
        {
            get { return null; }
        }

        #endregion
        private void OnStep()
        {

        }
    }
}
