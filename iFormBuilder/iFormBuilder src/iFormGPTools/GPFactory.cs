// Copyright 1995-2004 ESRI
//
// All rights reserved under the copyright laws of the United States.
// You may freely redistribute and use this sample code, with or without modification.
//
// Disclaimer: THE SAMPLE CODE IS PROVIDED "AS IS" AND ANY EXPRESS OR IMPLIED 
// WARRANTIES, INCLUDING THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS 
// FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL ESRI OR 
// CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, 
// OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF 
// SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS 
// INTERRUPTION) SUSTAINED BY YOU OR A THIRD PARTY, HOWEVER CAUSED AND ON ANY 
// THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT ARISING IN ANY 
// WAY OUT OF THE USE OF THIS SAMPLE CODE, EVEN IF ADVISED OF THE POSSIBILITY OF 
// SUCH DAMAGE.
//
// For additional information contact: Environmental Systems Research Institute, Inc.
// Attn: Contracts Dept.
// 380 New York Street
// Redlands, California, U.S.A. 92373 
// Email: contracts@esri.com

//Ismael Chivite. Appliations Prototype Lab.
//November 2006

using System;
using System.Runtime.InteropServices;
using ESRI.ArcGIS.ADF.CATIDs;
using ESRI.ArcGIS.esriSystem;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Geoprocessing;

namespace ESRI.Solutions.iFormBuilder.GPTools
{
    [Guid("0D4BBFE5-4ABC-48D7-A4CA-17F2860FF93F"), ComVisible(true)]
    public class GPFactory : IGPFunctionFactory
    {
        // Register the Function Factory with the ESRI Geoprocessor Function Factory Component Category.
        #region "Component Category Registration"
        [ComRegisterFunction()]
        static void Reg(string regKey)
        {
            GPFunctionFactories.Register(regKey);
        }

        [ComUnregisterFunction()]
        static void Unreg(string regKey)
        {
            GPFunctionFactories.Unregister(regKey);
        }

        #endregion
        // Utility Function added to create the function names.
        private IGPFunctionName CreateGPFunctionNames(long index)
        {
            IGPFunctionName functionName = new GPFunctionNameClass();
            IGPName name;

            switch (index)
            {
                case (0):
                    name = (IGPName)functionName;
                    name.Category = "iFormBuilder";
                    name.Description = "Download iFormbuilder Database";
                    name.DisplayName = "Download iFormbuilder Database";
                    name.Name = "DownloadiFormbuilderDatabase";
                    name.Factory = (IGPFunctionFactory)this;
                    break;
                case (1):
                    name = (IGPName)functionName;
                    name.Category = "iFormBuilder";
                    name.Description = "Sync iFormbuilder Database";
                    name.DisplayName = "Sync iFormbuilder Database";
                    name.Name = "SynciFormDatabase";
                    name.Factory = (IGPFunctionFactory)this;
                    break;
                case (2):
                    name = (IGPName)functionName;
                    name.Category = "iFormBuilder";
                    name.Description = "Download iFormbuilder Database Using Access Code";
                    name.DisplayName = "Download iFormbuilder Database Using Access Code";
                    name.Name = "DownloadiFormDatabaseWithAccessCode";
                    name.Factory = (IGPFunctionFactory)this;
                    break;
                case (3):
                    name = (IGPName)functionName;
                    name.Category = "iFormBuilder";
                    name.Description = "Get iFormBuilder Token";
                    name.DisplayName = "Get token from iFormBuilder to allow data downloads";
                    name.Name = "GetiFormToken";
                    name.Factory = (IGPFunctionFactory)this;
                    break;
            }

            return functionName;
        }

        // Implementation of the Function Factory
        #region IGPFunctionFactory Members

        // This is the name of the function factory. 
        // This is used when generating the Toolbox containing the function tools of the factory.
        public string Name
        {
            get { return "iFormbuilder Database Managment"; }
        }

        // This is the alias name of the factory.
        public string Alias
        {
            get { return "iFormbuilder Database Managment"; }
        }

        // This is the class id of the factory. 
        public UID CLSID
        {
            get
            {
                UID id = new UIDClass();
                id.Value = this.GetType().GUID.ToString("B");
                return id;
            }
        }

        // This method will create and return a function object based upon the input name.
        public IGPFunction GetFunction(string Name)
        {
            IGPFunction gpFunction = null;
            switch (Name)
            {
                case ("DownloadiFormbuilderDatabase"):
                    gpFunction = new ESRI.Solutions.iFormBuilder.GPTools.DownloadiFormDatabase();
                    break;
                case ("SynciFormDatabase"):
                    gpFunction = new ESRI.Solutions.iFormBuilder.GPTools.SynciFormDatabase();
                    break;
                case ("DownloadiFormDatabaseWithAccessCode"):
                    gpFunction = new ESRI.Solutions.iFormBuilder.GPTools.DownloadiFormDatabaseWithAccessCode();
                    break;
                case ("GetiFormToken"):
                    gpFunction = new ESRI.Solutions.iFormBuilder.GPTools.GetiFormToken();
                    break;
            }

            return gpFunction; ;
        }

        // This method will create and return a function name object based upon the input name.
        public IGPName GetFunctionName(string Name)
        {
            IGPName gpName = new GPFunctionNameClass();

            switch (Name)
            {
                case ("DownloadiFormbuilderDatabase"):
                    return (IGPName)CreateGPFunctionNames(0);
                case ("SynciFormDatabase"):
                    return (IGPName)CreateGPFunctionNames(1);
                case ("DownloadiFormDatabaseWithAccessCode"):
                    return (IGPName)CreateGPFunctionNames(2);
                case ("GetiFormToken"):
                    return (IGPName)CreateGPFunctionNames(3);

            }
            return null;
        }

        // This method will create and return an enumeration of function names that the factory supports.
        public IEnumGPName GetFunctionNames()
        {
            IArray nameArray = new EnumGPNameClass();
            nameArray.Add(CreateGPFunctionNames(0));
            nameArray.Add(CreateGPFunctionNames(1));
            nameArray.Add(CreateGPFunctionNames(2));
            nameArray.Add(CreateGPFunctionNames(3));
            return (IEnumGPName)nameArray;
        }
        public IEnumGPEnvironment GetFunctionEnvironments()
        {
            return null;
        }

        #endregion
    }
}
