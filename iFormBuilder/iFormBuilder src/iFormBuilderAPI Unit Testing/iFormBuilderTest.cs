using iFormBuilderAPI;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using System.Collections.Generic;

namespace iFormBuilderAPI_Unit_Testing
{
    
    
    /// <summary>
    ///This is a test class for iFormBuilderTest and is intended
    ///to contain all iFormBuilderTest Unit Tests
    ///</summary>
    [TestClass()]
    public class iFormBuilderTest
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
        //[ClassInitialize()]
        //public static void MyClassInitialize(TestContext testContext)
        //{
        //}
        //
        //Use ClassCleanup to run code after all tests in a class have run
        //[ClassCleanup()]
        //public static void MyClassCleanup()
        //{
        //}
        //
        //Use TestInitialize to run code before running each test
        IConfiguration config;
        long pagetotest = 8667965;
        string configfile = @"C:\Projects\crs_config.xml";

        //long pagetotest = 144678;
        [TestInitialize()]
        public void MyTestInitialize()
        {

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
        ///A test for ifombuilder_accesscode
        ///</summary>
        [TestMethod()]
        public void GetAccessCode()
        {
            iFormBuilder api = new iFormBuilder();
            api.ReadConfiguration(configfile);

            AccessCode actual;
            actual = api.accesscode;
            Assert.IsFalse(actual.isExpired);
        }

        /// <summary>
        ///A test for ifombuilder_accesscode
        ///</summary>
        [TestMethod()]
        public void GetAccessToken()
        {
            iFormBuilder api = new iFormBuilder();
            api.ReadConfiguration(configfile);

            AccessCode actual;
            actual = api.accesscode;
            Assert.IsFalse(actual.isExpired);
        }

        /// <summary>
        ///A test for GetPage
        ///</summary>
        [TestMethod()]
        public void GetPageTest()
        {
            iFormBuilder api = new iFormBuilder();
            api.ReadConfiguration(configfile);

            long pageid = pagetotest; // TODO: Initialize to an appropriate value
            //Page expected = null; // TODO: Initialize to an appropriate value
            Page actual;
            actual = api.GetPage(pageid);
            Assert.AreEqual(actual.ID, pageid);
        }

        /// <summary>
        ///A test for GetOptionList
        ///</summary>
        [TestMethod()]
        public void GetOptionListTest()
        {
            iFormBuilder api = new iFormBuilder();
            api.ReadConfiguration(configfile);

            int optionlist = 221271; // TODO: Initialize to an appropriate value
            OptionList actual;
            actual = api.GetOptionList(optionlist);
            Assert.AreEqual(optionlist, actual.OPTION_LIST_ID);
        }

        /// <summary>
        ///A test for GetRecords
        ///</summary>
        [TestMethod()]
        public void GetRecordsTest()
        {
            //IConfiguration configuration = null; // TODO: Initialize to an appropriate value
            iFormBuilder api = new iFormBuilder();
            api.ReadConfiguration(configfile);

            long pageid = pagetotest; // TODO: Initialize to an appropriate value
            //Page expected = null; // TODO: Initialize to an appropriate value
            Page page;
            page = api.GetPage(pageid); // TODO: Initialize to an appropriate value
            //Records expected = null; // TODO: Initialize to an appropriate value
            Records actual = api.GetRecords(page, 0, 1);
            Assert.IsTrue(actual.RecordSet.Count != 0);
        }

        /// <summary>
        ///A test for SaveConfiguration
        ///</summary>
        [TestMethod()]
        public void SaveConfigurationTest()
        {
            string agsfolder = Environment.GetFolderPath(Environment.SpecialFolder.Personal) + "\\ArcGIS";
            string iformfolder = String.Empty;
            if (Directory.Exists(agsfolder))
            {
                iformfolder = agsfolder + "\\iformbuilder";
                if (!Directory.Exists(iformfolder))
                    Directory.CreateDirectory(iformfolder);
            }

            string configfile = agsfolder + "\\iformbuilder\\config.xml";
            iFormBuilder api = new iFormBuilder();
            api.ReadConfiguration(configfile);
            configfile = agsfolder + "\\iformbuilder\\config_UNIT.xml";

            string path = iformfolder; // TODO: Initialize to an appropriate value
            bool actual;
            actual = api.SaveConfiguration(configfile);
            Assert.IsTrue(actual);
        }

        /// <summary>
        ///A test for ReadConfiguration
        ///</summary>
        [TestMethod()]
        public void ReadConfigurationTest()
        {
            IConfiguration configuration = new Configuration();
            Environment.GetFolderPath(Environment.SpecialFolder.Personal);
            string agsfolder = Environment.GetFolderPath(Environment.SpecialFolder.Personal) + "\\ArcGIS";
            string configfile = agsfolder + "\\iformbuilder\\config_UNIT.xml";
            if (!File.Exists(configfile))
                Assert.Fail("No config file found");
            iFormBuilder api = new iFormBuilder();
            api.ReadConfiguration(configfile);

            bool actual;
            actual = api.ReadConfiguration(configfile);
            Assert.IsTrue(configuration.iformpassword == config.iformpassword);
        }

        /// <summary>
        ///A test for GetAllPagesInProfile
        ///</summary>
        [TestMethod()]
        public void GetAllPagesInProfileTest()
        {
            iFormBuilder api = new iFormBuilder();
            api.ReadConfiguration(configfile);

            //List<Page> expected = null; // TODO: Initialize to an appropriate value
            List<Page> actual;
            actual = api.GetAllPagesInProfile();
            Assert.IsTrue(actual != null);
        }

        [TestMethod]
        public void AddRecord()
        {
            iFormBuilder api = new iFormBuilder();
            api.ReadConfiguration(configfile);

            RecordsCreator rc = new RecordsCreator();
            Record record = new Record();
            Field field = new Field();
            field.ELEMENT_ID = 6702660;
            field.VALUE = "Unit Test Add";
            record.FIELDS.Add(field);
            rc.RECORDS.Add(record);

            Assert.IsTrue(api.CreateRecord(pagetotest, rc) != -1);
        }

        [TestMethod]
        public void GetFormAssignment()
        {
            iFormBuilder api = new iFormBuilder();
            api.ReadConfiguration(configfile);
            FormAssignment fm = api.GetFormAssignment();
            Assert.IsTrue(fm.FormItems.Count != 0);
        }

        [TestMethod]
        public void GetAllUsersInProfile()
        {
            iFormBuilder api = new iFormBuilder();
            api.ReadConfiguration(configfile);
            List<User> fm = api.GetAllUserInProfile();
            Assert.IsTrue(fm.Count != 0);
        }

        /// <summary>
        ///A test for DeleteRecords
        ///</summary>
        [TestMethod()]
        public void DeleteRecordsTest()
        {
            iFormBuilder api = new iFormBuilder();
            api.ReadConfiguration(configfile);

            //Page page = target.GetPage(this.pagetotest); // TODO: Initialize to an appropriate value
            List<int> DeleteRecords = new List<int>(); // TODO: Initialize to an appropriate value
            DeleteRecords.Add(8);
            //bool expected = false; // TODO: Initialize to an appropriate value
            bool actual;
            List<long> pages = new List<long>();
            pages.Add(pagetotest);
            actual = api.DeleteRecords(pages, DeleteRecords);
            Assert.IsTrue(actual);
        }
    }
}
