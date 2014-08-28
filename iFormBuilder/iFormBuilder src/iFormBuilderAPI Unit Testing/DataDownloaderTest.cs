using ESRI.ArcGIS.esriSystem;
using iFormTools;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using iFormBuilderAPI;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS;
using System;
using System.Collections.Generic;

namespace iFormBuilderAPI_Unit_Testing
{
    
    
    /// <summary>
    ///This is a test class for DataDownloaderTest and is intended
    ///to contain all DataDownloaderTest Unit Tests
    ///</summary>
    [TestClass()]
    public class DataDownloaderTest
    {


        private TestContext testContextInstance;

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext
        {
            get
            {
                return testContextInstance;
            }
            set
            {
                testContextInstance = value;
            }
        }

        #region Additional test attributes
        // 
        //You can use the following additional attributes as you write your tests:
        //
        //Use ClassInitialize to run code before running the first test in the class
        [ClassInitialize()]
        public static void MyClassInitialize(TestContext testContext)
        {
            RuntimeManager.Bind(ProductCode.EngineOrDesktop);
            ESRI.ArcGIS.esriSystem.IAoInitialize aoInitialize = new ESRI.ArcGIS.esriSystem.AoInitializeClass();
            aoInitialize.Initialize(esriLicenseProductCode.esriLicenseProductCodeEngine);
        }
        //
        //Use ClassCleanup to run code after all tests in a class have run
        //[ClassCleanup()]
        //public static void MyClassCleanup()
        //{
        //}
        //DR
        //Use TestInitialize to run code before running each test
        //long pagetotest = 19135;  //Complex SMART Form
        //string tablename = "ocotillo_master_2";
        //string configfile = @"C:\Users\trav5516\Documents\ArcGIS\iformbuilder\config_dudek.xml";

        //long pagetotest = 148052; //Simple Unit Testing Form
        //       string tablename = "unit_testing";
        //       string configfile = @"\\psf\Home\Documents\ArcGIS\iformbuilder\config_esri.xml";
        
               // long pagetotest = 148833;
                //string tablename = "somrep_hh_questionnaire_Som";
                //string configfile = @"\\psf\Home\Documents\ArcGIS\iformbuilder\config_wv.xml";
            
        
        //long pagetotest = 142606540;
        //string tablename = "drc_parent_form";
        //string configfile = @"C:\Project Work\iFormBuilder Tools\Config\wvdrc_config.xml";


        //long pagetotest = 152692;//152596;
        //string tablename = "babati_hhp_sw";//"babati_shocks_vulnerabilities_sw;
        //string configfile = @"C:\Project Work\iFormBuilder Tools\Config\wvbabati_config.xml";
         
        //long pagetotest = 152117;
        //string tablename = "Parent_CoverForm";
        //string configfile = @"C:\Project Work\iFormBuilder Tools\Config\wvniger_config.xml";
         
        //long pagetotest = 142606573;
        //string tablename = "flat_survey";
        //string configfile = @"C:\Project Work\iFormBuilder Tools\Config\wvdrc_config.xml";
        long pagetotest = 409301;
        string tablename = "lead_capture_api";
        string configfile = @"\\psf\Home\Documents\My Projects\iFormBuilder Testing\load_config.xml";

        [TestInitialize()]
        public void MyTestInitialize()
        {
            RuntimeManager.Bind(ProductCode.Desktop);
            ESRI.ArcGIS.esriSystem.IAoInitialize aoInitialize = new ESRI.ArcGIS.esriSystem.AoInitializeClass();
            aoInitialize.Initialize(esriLicenseProductCode.esriLicenseProductCodeAdvanced);
        }
        //
        //Use TestCleanup to run code after each test has run
        //[TestCleanup()]
        //public void MyTestCleanup()
        //{
        //}
        //
        #endregion

        /// <summary>
        ///A test for SchemaBuilder
        ///</summary>
        [TestMethod()]
        public void SchemaBuilderTest1()
        {
            RuntimeManager.Bind(ProductCode.EngineOrDesktop);
            ESRI.ArcGIS.esriSystem.IAoInitialize aoInitialize = new ESRI.ArcGIS.esriSystem.AoInitializeClass();
            aoInitialize.Initialize(esriLicenseProductCode.esriLicenseProductCodeAdvanced);

            iFormBuilder api = new iFormBuilder();
            api.ReadConfiguration(configfile);

            DataDownloader target = new DataDownloader(api.iformconfig);
            long pageid = pagetotest;
            string workspacepath = @"C:\temp"; 
            IWorkspace actual;
            actual = target.SchemaBuilder(pageid, workspacepath,false);
            Assert.IsTrue(actual != null);
        }

        [TestMethod()]
        public void SchemaBuilder_AccessCodeTest()
        {
            RuntimeManager.Bind(ProductCode.EngineOrDesktop);
            ESRI.ArcGIS.esriSystem.IAoInitialize aoInitialize = new ESRI.ArcGIS.esriSystem.AoInitializeClass();
            aoInitialize.Initialize(esriLicenseProductCode.esriLicenseProductCodeAdvanced);

            iFormBuilder api = new iFormBuilder();
            api.ReadConfiguration(configfile);

            //Generate an access code
            iFormBuilder api_download = new iFormBuilder(api.accesscode.access_token,api.iformconfig.iformserverurl,api.iformconfig.profileid);

            DataDownloader target = new DataDownloader(api_download.iformconfig, api_download.accesscode);
            long pageid = pagetotest;
            string workspacepath = @"C:\temp";
            IWorkspace actual;
            actual = target.SchemaBuilder(pageid, workspacepath, false);
            Assert.IsTrue(actual != null);
        }

        /// <summary>
        ///A test for DownloadData
        ///</summary>
        [TestMethod()]
        public void DownloadDataTest()
        {
            RuntimeManager.Bind(ProductCode.EngineOrDesktop);
            ESRI.ArcGIS.esriSystem.IAoInitialize aoInitialize = new ESRI.ArcGIS.esriSystem.AoInitializeClass();
            aoInitialize.Initialize(esriLicenseProductCode.esriLicenseProductCodeAdvanced);

            iFormBuilder api = new iFormBuilder();
            api.ReadConfiguration(configfile);
            //IConfiguration config = null; // TODO: Initialize to an appropriate value
            DataDownloader target = new DataDownloader(api.iformconfig); // TODO: Initialize to an appropriate value
            long pageid = pagetotest; // TODO: Initialize to an appropriate value
            string workspacepath = @"C:\temp";
            IWorkspace workspace;
            workspace = target.SchemaBuilder(pageid, workspacepath,true);
            workspace = target.DownloadData(pageid, tablename, workspace,2);
            Assert.IsTrue(workspace != null);
        }

        /// <summary>
        ///A test for SynchronizeData
        ///</summary>
        [TestMethod()]
        public void SynchronizeDataTest()
        {
            RuntimeManager.Bind(ProductCode.EngineOrDesktop);
            ESRI.ArcGIS.esriSystem.IAoInitialize aoInitialize = new ESRI.ArcGIS.esriSystem.AoInitializeClass();
            aoInitialize.Initialize(esriLicenseProductCode.esriLicenseProductCodeAdvanced);

            iFormBuilder api = new iFormBuilder();
            api.ReadConfiguration(configfile);

            DataDownloader target = new DataDownloader(api.iformconfig);
            long pageid = pagetotest;
            string workspacepath = @"C:\temp\DRC_Congo_Extract.gdb";
            IWorkspace workspace;
            Type factoryType = factoryType = Type.GetTypeFromProgID("esriDataSourcesGDB.FileGDBWorkspaceFactory");
            IWorkspaceFactory workspaceFactory = (IWorkspaceFactory)Activator.CreateInstance(factoryType);
            workspace = workspaceFactory.OpenFromFile(workspacepath, 0);
            workspace = target.SynchronizeData(workspace, 2);
            Assert.IsTrue(workspace != null);
        }

        [TestMethod()]
        public void FlattenDatabase()
        {
            RuntimeManager.Bind(ProductCode.EngineOrDesktop);
            ESRI.ArcGIS.esriSystem.IAoInitialize aoInitialize = new ESRI.ArcGIS.esriSystem.AoInitializeClass();
            aoInitialize.Initialize(esriLicenseProductCode.esriLicenseProductCodeAdvanced);

            iFormBuilder api = new iFormBuilder();
            api.ReadConfiguration(configfile);

            DataDownloader target = new DataDownloader(api.iformconfig);
            long pageid = pagetotest;
            string workspacepath = @"C:\temp\DRC_Congo_Extract.gdb";
            IWorkspace workspace;
            Type factoryType = factoryType = Type.GetTypeFromProgID("esriDataSourcesGDB.FileGDBWorkspaceFactory");
            IWorkspaceFactory workspaceFactory = (IWorkspaceFactory)Activator.CreateInstance(factoryType);
            workspace = workspaceFactory.OpenFromFile(workspacepath, 0);
            Page testPage = api.GetPage(pageid);
            bool test = target.BuildFlatTable(testPage, workspace);
            Assert.IsTrue(test);
        }

        /// <summary>
        ///A test for SynchronizeData
        ///</summary>
        [TestMethod()]
        public void Test_GetAllOptionLists()
        {
            iFormBuilder api = new iFormBuilder();
            api.ReadConfiguration(configfile);
            List<OptionList> test = api.GetAllOptionLists();

            Assert.IsTrue(test != null);
        }

        /// <summary>
        ///A test for SynchronizeData
        ///</summary>
        [TestMethod()]
        public void UploadExcelFile()
        {
            iFormBuilder api = new iFormBuilder();
            api.ReadConfiguration(configfile);
            
            string workspacepath = @"C:\Users\trav5516\Sandbox\iFormBuilder\Upload Excel Test.xlsx";
            UploadExcelFile target = new UploadExcelFile(api.iformconfig);
            List<OptionList> list = target.CreateOptionList(workspacepath);
            List<OptionList> test = api.GetAllOptionLists();

            bool createlist = true;
            foreach (OptionList o in list)
            {
                foreach (OptionList o1 in test)
                {
                    if (o.NAME == o1.NAME)
                    {
                        createlist = api.DeleteOptionList(o1);
                        break;
                    }
                }

                if(createlist)
                    api.CreateOptionList(o);
            }

            Assert.IsTrue(list.Count == 3);
        }
    }
}
