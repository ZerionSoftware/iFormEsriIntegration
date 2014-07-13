using ESRI.ArcGIS.esriSystem;
using iFormTools;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using ESRI.ArcGIS.Geodatabase;
using iFormBuilderAPI;
using ESRI.ArcGIS;

namespace iFormBuilderAPI_Unit_Testing
{
    
    
    /// <summary>
    ///This is a test class for DataUpdaterTest and is intended
    ///to contain all DataUpdaterTest Unit Tests
    ///</summary>
    [TestClass()]
    public class DataUpdaterTest
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
        long pagetotest = 148052; //Simple Unit Testing Form
        IConfiguration config;
        [TestInitialize()]
        public void MyTestInitialize()
        {
            config = new Configuration();
            config.clientid = "";
            config.refreshcode = "";
            config.iformserverurl = "";
            config.iformusername = "";
            config.iformpassword = "";
            config.profileid = 0;
            config.arcgisurl = "";
            config.arcgisusername = "";
            config.arcgispassword = "";

            RuntimeManager.Bind(ProductCode.Desktop);
            ESRI.ArcGIS.esriSystem.IAoInitialize aoInitialize = new ESRI.ArcGIS.esriSystem.AoInitializeClass();
            aoInitialize.Initialize(esriLicenseProductCode.esriLicenseProductCodeAdvanced);
        }
        //
        //Use ClassCleanup to run code after all tests in a class have run
        //[ClassCleanup()]
        //public static void MyClassCleanup()
        //{
        //}
        //
        //Use TestInitialize to run code before running each test
        //[TestInitialize()]
        //public void MyTestInitialize()
        //{
        //}
        //
        //Use TestCleanup to run code after each test has run
        //[TestCleanup()]
        //public void MyTestCleanup()
        //{
        //}
        //
        #endregion


        /// <summary>
        ///A test for UpdateElementFromFeature
        ///</summary>
        [TestMethod()]
        public void UpdateElementFromFeatureTest()
        {

            DataDownloader target = new DataDownloader(config); // TODO: Initialize to an appropriate value
            long pageid = pagetotest; // TODO: Initialize to an appropriate value
            string tablename = "unit_testing"; // TODO: Initialize to an appropriate value
            string workspacepath = @"C:\Users\trav5516\Sandbox\Output";
            IWorkspace workspace;
            workspace = target.SchemaBuilder(pageid, workspacepath,false);
            workspace = target.DownloadData(pageid, tablename, workspace,1);
            DataUpdater updater = new DataUpdater(config);
            IFeature feature = null;
            Page schemapage = null;
            updater.UpdateElementFromFeature(feature, schemapage);
            Assert.Inconclusive("A method that does not return a value cannot be verified.");
        }
    }
}
