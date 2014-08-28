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
//December 2005


using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.esriSystem;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Geoprocessing;

namespace ESRI.Solutions.iFormBuilder.GPTools
{

    public class APLGPUtils
    {
        public APLGPUtils()
        {

        }
        static public IGPValue GetParameterValueByName(IArray paramvalues, string name)
        {
            IGPUtilities gpUtils = new GPUtilitiesClass();
            IGPParameter gpParameter;

            for (int i = 0; i < paramvalues.Count; i++)
            {
                gpParameter = (IGPParameter)paramvalues.get_Element(i);
                if (gpParameter.Name.ToUpper() == name.ToUpper())
                    return gpUtils.UnpackGPValue(gpParameter);
            }
            return null;
        }
        static public IGPParameter GetParameterByName(IArray paramvalues, string name)
        {
            IGPParameter gpParameter;
            for (int i = 0; i < paramvalues.Count; i++)
            {
                gpParameter = (IGPParameter)paramvalues.get_Element(i);
                if (gpParameter.Name.ToUpper() == name.ToUpper())
                    return gpParameter;
            }
            return null;
        }

        static public IGPParameterEdit CreateParameterEdit(string name, string displayName, esriGPParameterDirection paramDirection, esriGPParameterType paramType, IGPDataType gpDataType, bool enabled)
        {
            IGPParameterEdit parameterEdit = new GPParameterClass();

            parameterEdit.DataType = gpDataType;
            parameterEdit.Value = gpDataType.CreateValue("");
            parameterEdit.ParameterType = paramType;
            parameterEdit.Direction = paramDirection;
            parameterEdit.DisplayName = displayName;
            parameterEdit.Name = name;
            parameterEdit.Enabled = enabled;

            return parameterEdit;
        }
        static public IGPEnvironment GetEnvironment(IGPEnvironmentManager environmentManager, string name)
        {
            IGPUtilities gpUtils = new GPUtilitiesClass();
            IGPEnvironment returnEnv = null;

            if (environmentManager.GetLocalEnvironments().Count > 0)
                returnEnv = gpUtils.GetEnvironment(environmentManager.GetLocalEnvironments(), name);

            if (returnEnv == null)
                returnEnv = gpUtils.GetEnvironment(environmentManager.GetEnvironments(), name);

            return returnEnv;
        }

        static public int GetFeaturesForInput(IGPValue inputValue, out IFeatureClass inputFClass, out IFeatureCursor inputFeatures)
        {
            //                                       
            // GET FEATURECURSOR RESPECTING SELECTION
            IGPUtilities gpUtils = new GPUtilitiesClass();
            int numFeatures = 0;
            IGeoFeatureLayer inputFLayer = null;

            if (inputValue is IDEFeatureClass)
            {
                // IF THE INPUT IS A FEATURECLASS THEN JUST OPEN THE DATASET AND
                // CREATE A FEATURE CURSOR OF ALL THE FEATURES AND RETURN THE TOTAL
                // NUMBER OF FEATURES IN THE FEATURECLASS
                inputFClass = (IFeatureClass)gpUtils.OpenDataset(inputValue);
                inputFeatures = inputFClass.Search(null, false);
                numFeatures = inputFClass.FeatureCount(null);
            }
            else
            {
                // IF THE INPUT IS A FEATURELAYER THEN OPEN THE FEATURELAYER
                // AND USE THE DISPLAYFEATURECLASS THAT RESPECTS JOINED FIELDS.
                inputFLayer = (IGeoFeatureLayer)gpUtils.OpenDataset(inputValue);
                inputFClass = inputFLayer.DisplayFeatureClass;
                IFeatureSelection inputSelection = (IFeatureSelection)inputFLayer;
                if (inputSelection.SelectionSet.Count > 0)
                {
                    // IF THE FEATURELAYER HAS A SELECTION THEN CREATE A FEATURECURSOR
                    // OF ALL THE SELECTED FEATURES AND RETURN THE NUMBER OF FEATURES
                    // IN THE SELECTION
                    ICursor selectionCursor = null;
                    inputSelection.SelectionSet.Search(null, false, out selectionCursor);
                    inputFeatures = (IFeatureCursor)selectionCursor;
                    numFeatures = inputSelection.SelectionSet.Count;
                }
                else
                {
                    // IF THE FEATURELAYER HAS NO SELECTION THEN CREATE A FEATURECURSOR
                    // USING THE DEFINITIONEXPRESSION OF THE LAYER AND RETURN THE NUMBER
                    // OF FEATURES BASED ON THE DEFINITIONEXPRESSION.
                    IFeatureLayerDefinition layerDef = (IFeatureLayerDefinition)inputFLayer;
                    IQueryFilter queryFilter = new QueryFilterClass();
                    queryFilter.WhereClause = layerDef.DefinitionExpression;
                    inputFeatures = inputFLayer.SearchDisplayFeatures(queryFilter, false);
                    numFeatures = inputFLayer.FeatureClass.FeatureCount(queryFilter);
                }
            }

            return numFeatures;
        }
    }
}
