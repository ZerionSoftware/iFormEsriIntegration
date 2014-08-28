using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.esriSystem;
using ESRI.ArcGIS.Geodatabase;
using iFormBuilderAPI;

namespace ArcGISTools
{
    public class Utilities
    {
        /// <summary>
        /// Creates the file GDB workspace.
        /// Create Workspace will only create a file GDB
        /// </summary>
        /// <param name="path">The path.</param>
        /// <returns></returns>
        public static IWorkspace CreateWorkspace(String path, String gdbName)
        {
            try
            {
                // Instantiate a file geodatabase workspace factory and create a file geodatabase.
                // The Create method returns a workspace name object.
                Type factoryType = factoryType = Type.GetTypeFromProgID("esriDataSourcesGDB.FileGDBWorkspaceFactory");
                IWorkspaceFactory workspaceFactory = (IWorkspaceFactory)Activator.CreateInstance(factoryType);
                IWorkspaceName workspaceName = workspaceFactory.Create(path, gdbName, null, 0);

                // Cast the workspace name object to the IName interface and open the workspace.
                IName name = (IName)workspaceName;
                IWorkspace workspace = (IWorkspace)name.Open();
                return workspace;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public static String TranslateFieldName(String fieldname)
        {
            Regex rx = new Regex("^[a-zA-Z][a-zA-Z0-9_]+[a-zA-Z0-9]$");
            //Check the String Name
            string name = fieldname.Replace("_", "");
            if (fieldname.Length > 64)
                name = name.Substring(0, 64);

            if (!rx.IsMatch(name))
            {
                name = fieldname;

                //need to fix this string
                //Console.Write(string.Format("Did not fit:  ", element.NAME));
                //Chop the last character of the name off
                name = fieldname.Substring(0, name.Length - 1);
            }

            return name;
        }

        public static String TranslateAliasName(String alias)
        {
            Regex rx = new Regex("^[a-zA-Z][a-zA-Z0-9_]+[a-zA-Z0-9]$");
            //Check the String Name
            string name = alias.Replace("{", "").Replace("{", "").Replace("/"," ");
            if (name.Length > 64)
                name = name.Substring(0, 64);

            return name;
        }

        /// <summary>
        /// Translates the iFormBuilder Field to Esri Field Type.
        /// </summary>
        /// <param name="element">a valid IElement from iFormAPI.</param>
        /// <returns>IFieldEdit</returns>
        public static IFieldEdit translateIFormToEsri(iFormBuilderAPI.IElement element)
        {
            try
            {
                IFieldEdit fieldEdit = new FieldClass();
                //Make sure the field name is valid


                fieldEdit.Name_2 = Utilities.TranslateFieldName(element.NAME);
                switch (element.DATA_TYPE)
                {
                    case 1:
                        fieldEdit.Type_2 = esriFieldType.esriFieldTypeString;
                        fieldEdit.Length_2 = element.DATA_SIZE;
                        break;
                    case 11:
                        fieldEdit.Type_2 = esriFieldType.esriFieldTypeString;
                        fieldEdit.Length_2 = 250;
                        break;
                    case 2:
                    case 6:
                        fieldEdit.Type_2 = esriFieldType.esriFieldTypeInteger;
                        break;
                    case 3:
                        fieldEdit.Type_2 = esriFieldType.esriFieldTypeDate;
                        break;
                    //case 11:
                    //    fieldEdit.Type_2 = esriFieldType.esriFieldTypeRaster;
                    //    break;
                    default:
                        fieldEdit.Type_2 = esriFieldType.esriFieldTypeString;
                        fieldEdit.Length_2 = 500;
                        break;

                }
                fieldEdit.AliasName_2 = TranslateAliasName(element.Alias);
                return fieldEdit;
            }
            catch (Exception ex)
            {
                throw new Exception(string.Format("Error in Transforming Field class: {0} ", ex.Message));
            }
        }

        /// <summary>
        /// Translates the iFormBuilder Field to Esri Field Type.
        /// </summary>
        /// <param name="element">a valid IElement from iFormAPI.</param>
        /// <returns>IFieldEdit</returns>
        public static IFieldEdit translateIFormToEsri(iFormBuilderAPI.IElement element,Option opt)
        {
            try
            {
                IFieldEdit fieldEdit = new FieldClass();
                //Make sure the field name is valid
                string fieldname = Utilities.TranslateFieldName(element.NAME);
                if (fieldname.Length > 64)
                    fieldname = fieldname.Substring(0, (64 - opt.KEY_VALUE.Length));

                fieldEdit.Name_2 = string.Format("{0}{1}", fieldname, opt.KEY_VALUE);
                fieldEdit.Type_2 = esriFieldType.esriFieldTypeInteger;
                string alias = string.Format("{0}:{1}", element.Alias, opt.LABEL);
                fieldEdit.AliasName_2 = TranslateAliasName(alias);
                return fieldEdit;
            }
            catch (Exception ex)
            {
                throw new Exception(string.Format("Error in Transforming Field class: {0} ", ex.Message));
            }
        }

        public static void ConvertFeatureToElement(long pageid, IFeature feature)
        {
        }

        public static bool DoesDomainExist(IWorkspace workspace, string domain)
        {
            IWorkspaceDomains pwsDomain = workspace as IWorkspaceDomains;
            return (pwsDomain.get_DomainByName(domain) != null);
        }

        public static IDataset GetDataSet(IWorkspace inWorkspace, string objectname)
        {
            IEnumDatasetName enumDatasetName = inWorkspace.get_DatasetNames(esriDatasetType.esriDTAny);
            IDatasetName datasetName = enumDatasetName.Next();
            IDataset dataset = null;
            IFeatureWorkspace featureWorkspace = (IFeatureWorkspace)inWorkspace;
            while (datasetName != null)
            {
                if (datasetName.Name == objectname)
                {
                    if (datasetName.Type == esriDatasetType.esriDTFeatureClass)
                        dataset = (IDataset)featureWorkspace.OpenFeatureClass(objectname);
                    else
                        dataset = (IDataset)featureWorkspace.OpenTable(objectname);
                    return dataset;
                }

                datasetName = enumDatasetName.Next();

            }

            return null;
        }

        public static bool DoesTableExist(IWorkspace inworkspace, string tablename)
        {
            return GetTable(inworkspace, tablename) != null;
        }

        public static ITable GetTable(IWorkspace inWorkspace, string objectname)
        {
            IEnumDatasetName enumDatasetName = inWorkspace.get_DatasetNames(esriDatasetType.esriDTAny);
            IDatasetName datasetName = enumDatasetName.Next();
            IDataset dataset = null;
            IFeatureWorkspace featureWorkspace = (IFeatureWorkspace)inWorkspace;
            while (datasetName != null)
            {
                if (datasetName.Name == objectname)
                {
                    if (datasetName.Type == esriDatasetType.esriDTFeatureClass)
                        dataset = (IDataset)featureWorkspace.OpenFeatureClass(objectname);
                    else
                        dataset = (IDataset)featureWorkspace.OpenTable(objectname);
                    return (ITable)dataset;
                }

                datasetName = enumDatasetName.Next();

            }

            return null;
        }

        public static bool DoesRelationshipClassExist(IWorkspace inworkspace, string RelationshipClassname)
        {
            return GetRelationshipClass(inworkspace, RelationshipClassname) != null;
        }

        public static IRelationshipClass GetRelationshipClass(IWorkspace inWorkspace, string objectname)
        {
            IEnumDatasetName enumDatasetName = inWorkspace.get_DatasetNames(esriDatasetType.esriDTAny);
            IDatasetName datasetName = enumDatasetName.Next();
            IDataset dataset = null;
            IFeatureWorkspace featureWorkspace = (IFeatureWorkspace)inWorkspace;
            while (datasetName != null)
            {
                if (datasetName.Name == objectname)
                {
                    if (datasetName.Type == esriDatasetType.esriDTRelationshipClass)
                    {
                        dataset = (IDataset)featureWorkspace.OpenRelationshipClass(objectname);
                        return (IRelationshipClass)dataset;
                    }
                }

                datasetName = enumDatasetName.Next();

            }

            return null;
        }

        public static long GetHighestID(IWorkspace workspace,string tablename,string fieldname)
        {
            ITable featureClass = Utilities.GetTable(workspace, tablename);
            //find the highest id in the table
            ICursor cursor = (ICursor)featureClass.Search(null, false);

            IDataStatistics dataStatistics = new DataStatisticsClass();
            dataStatistics.Field = fieldname;
            dataStatistics.Cursor = cursor;

            ESRI.ArcGIS.esriSystem.IStatisticsResults statisticsResults = dataStatistics.Statistics;
            Console.WriteLine("Current ID Value - {0}", statisticsResults.Maximum);
            return long.Parse(statisticsResults.Maximum.ToString());
        }

        public static long GetHighestIDFromRecordSet(List<Result> recordset)
        {
            long highestrecord = 0;
            foreach (Result result in recordset)
            {
                foreach (KeyValuePair<string, object> kvp in result.Record)
                {
                    if (kvp.Key == "ID")
                    {
                        if (long.Parse(kvp.Value.ToString()) > highestrecord)
                            highestrecord = long.Parse(kvp.Value.ToString());
                        break;
                    }
                }
            }

            return highestrecord;
        }

    }


}
