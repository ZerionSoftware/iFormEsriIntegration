using ESRI.ArcGIS.esriSystem;
using ArcGISTools;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS;
using iFormTools;

namespace iFormBuilder_Unit_Testing
{
    
    
    /// <summary>
    ///This is a test class for UtilitiesTest and is intended
    ///to contain all UtilitiesTest Unit Tests
    ///</summary>
    [TestClass()]
    public class UtilitiesTest
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
        [ClassCleanup()]
        public static void MyClassCleanup()
        {
        }

        //
        //Use TestInitialize to run code before running each test
        [TestInitialize()]
        public void MyTestInitialize()
        {
            try
            {
                ESRI.ArcGIS.esriSystem.IAoInitialize aoInitialize = new ESRI.ArcGIS.esriSystem.AoInitializeClass();
                aoInitialize.Initialize(esriLicenseProductCode.esriLicenseProductCodeEngine);
            }
            catch (Exception ex)
            { }
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
        ///A test for CreateFileGdbWorkspace
        ///</summary>
        [TestMethod()]
        public void CreateFileGdbWorkspaceTest()
        {
            string path = "C:\\Users\\trav5516\\Sandbox\\Output"; // TODO: Initialize to an appropriate value
            IWorkspace expected = null; // TODO: Initialize to an appropriate value
            IWorkspace actual;
            String fileName = DateTime.Now.ToFileTimeUtc().ToString();
            actual = Utilities.CreateFileGdbWorkspace(path, fileName);
            Assert.IsTrue(actual.PathName == path + "\\" + fileName +".gdb");
         }

        /// <summary>
        ///A test for downloaddata
        ///</summary>
        [TestMethod()]
        public void downloaddataTest()
        {
            MyClassInitialize(TestContext);
            DataDownloader target = new DataDownloader(); // TODO: Initialize to an appropriate value
            string expected = string.Empty; // TODO: Initialize to an appropriate value
            IWorkspace actual;
            int pageid = 148052;
            string path = "C:\\Users\\trav5516\\Sandbox\\Output";
            actual = target.SchemaBuilder(pageid, path);
            actual = target.DownloadData(pageid, "unit_testing", actual);
            Assert.IsTrue(actual != null);
        }
    }
}
