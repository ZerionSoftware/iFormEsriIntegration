using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using iFormBuilderAPI;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Geometry;
using System.Collections;
using Newtonsoft.Json.Linq;
using System.Diagnostics;
using Newtonsoft.Json;
using ESRI.ArcGIS.SystemUI;
using ESRI.ArcGIS.Geoprocessing;
using ESRI.ArcGIS.esriSystem;
using System.Net;
using System.Text.RegularExpressions;
using ESRI.ArcGIS.GeoDatabaseUI;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Geoprocessor;
using ESRI.ArcGIS.ADF;
using ESRI.ArcGIS.DataManagementTools;

namespace iFormTools
{
    public class DataDownloader
    {
        IConfiguration iformconfig;
        iFormBuilder iformbuilder;
        private string SyncTable = "SynchronizationTable";
        long highestrecord = 0;
        Administration admin;

        public DataDownloader(IConfiguration config)
        {
            iformconfig = config;
            iformbuilder = new iFormBuilder(config);
        }

        public DataDownloader(IConfiguration config,AccessCode accesscode)
        {
            iformconfig = config;
            iformbuilder = new iFormBuilder(config);
            iformbuilder.accesscode = accesscode;
        }

        public IWorkspace DownloadDataAndSchema(long pageid, string workspacepath, string tablename, bool SubFormsAsTables, int favorranking)
        {
            try
            {
                IWorkspace workspace = SchemaBuilder(pageid, workspacepath, SubFormsAsTables);
                return DownloadData(pageid, tablename, workspace, favorranking);
            }
            catch
            { return null; }
        }

        private List<int> _processeddomains;
        List<assigntodomain> domainassignments;
        /// <summary>
        /// Builds the Schema for the output database.
        /// </summary>
        /// <param name="pageid">The pageid.</param>
        /// <param name="workspacepath">The workspace path.</param>
        /// <returns></returns>
        public IWorkspace SchemaBuilder(long pageid, string workspacepath, bool SubFormsAsTables)
        {
            //Generate a random workspace for processing the data
            try
            {
                String fileName = DateTime.Now.ToFileTimeUtc().ToString();
                IWorkspace workspace = ArcGISTools.Utilities.CreateWorkspace(workspacepath, fileName);
                if (workspace == null)
                    throw new Exception("Workspace Not Created in Schemabuilder");

                return schemabuilder(pageid, workspace, SubFormsAsTables);
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }

        public IWorkspace SchemaBuilder(long pageid, IWorkspace workspace,bool SubFormsAsTables)
        {
            return schemabuilder(pageid, workspace, SubFormsAsTables);
        }

        private IWorkspace schemabuilder(long pageid, IWorkspace workspace, bool SubFormsAsTables)
        {
            List<int> pageids = new List<int>();
            domainassignments = new List<assigntodomain>();

            iFormBuilderAPI.Page schemaPage = iformbuilder.GetPage(pageid);

            _processeddomains = new List<int>();
            domainassignments = new List<assigntodomain>();

            CreateMetadataTables(workspace, schemaPage);
            CreateStandaloneFeatureClass((IFeatureWorkspace)workspace, schemaPage, SubFormsAsTables);
            
            //Add a Table to Manage Sync with iFormBuilder
            IDataset dataset = ArcGISTools.Utilities.GetDataSet(workspace, SyncTable);
            if (dataset == null)
                CreateSyncTable(workspace);

            //Proccess the Workspace Domains created
            foreach (assigntodomain assdom in domainassignments)
            {
                AssignDomainToField(assdom, ref workspace);
            }

            BuildRelationshipClasses(schemaPage, workspace);
            return workspace;

        }

        /// <summary>
        /// Downloads the data.
        /// </summary>
        /// <param name="pageid">The pageid.</param>
        /// <param name="tablename">The tablename.</param>
        /// <param name="workspace">The workspace.</param>
        /// <returns></returns>
        public IWorkspace DownloadData(long pageid, string tablename, IWorkspace workspace, int favorranking)
        {
            try
            {
                iFormBuilderAPI.Page schemaPage;
                schemaPage = iformbuilder.GetPage(pageid);
                downloaddata(schemaPage, workspace, 0,favorranking);
                return workspace;
            }
            catch (Exception ex)
            {
                throw new Exception(string.Format("Error during datadownload:  {0}  Extended Error: {1}", ex.Message, ex.StackTrace));
            }
        }
        public IWorkspace DownloadData(long pageid, string tablename, IWorkspace workspace, long recordid, int favorranking)
        {
            try
            {
                iFormBuilderAPI.Page schemaPage;
                schemaPage = iformbuilder.GetPage(pageid);
                downloaddata(schemaPage, workspace, recordid, favorranking);

                return workspace;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                throw new Exception("Error in Datadownload", ex);
            }
        }
        public IWorkspace SynchronizeData(IWorkspace workspace, int favorranking)
        {
            ITable synctable = ArcGISTools.Utilities.GetTable(workspace, "SynchronizationTable");
            if (synctable != null)
            {
                ICursor cursor;
                IQueryFilter qF = new QueryFilter();
                qF.WhereClause = "1=1";
                cursor = synctable.Search(qF, false);
                IRow row = cursor.NextRow();
                while (row != null)
                {
                    DateTime syncdate = (DateTime)row.get_Value(row.Fields.FindField("SynchronizationDate"));
                    long pageid = long.Parse(row.get_Value(row.Fields.FindField("SynchronizationPageID")).ToString());
                    string tablename = row.get_Value(row.Fields.FindField("SynchronizationTableName")).ToString();
                    highestrecord = ArcGISTools.Utilities.GetHighestID(workspace, tablename, "ID");   //long.Parse(row.get_Value(row.Fields.FindField("SynchronizationRecordID")).ToString());
                    DownloadData(pageid, tablename, workspace, highestrecord, favorranking);
                    row = cursor.NextRow();
                }
                return workspace;
            }
            else
                return null;
        }

        private bool downloaddata(iFormBuilderAPI.Page page, IWorkspace inWorkspace, long recordid,int favorranking)
        {
            try
            {
                Records results = iformbuilder.GetRecords(page, recordid, favorranking);
                List<Result> records = new List<Result>();
                while (results.RecordSet.Count != 0)
                {
                    processresults(results.RecordSet, page, inWorkspace);
                    highestrecord = ArcGISTools.Utilities.GetHighestIDFromRecordSet(results.RecordSet);
                    results = iformbuilder.GetRecords(page, highestrecord, favorranking);
                }

                //Flatten the Table Before the Output
                BuildFlatTable(page, inWorkspace);
                return true;
            }
            catch (Exception ex)
            {
                throw new Exception("Error in Datadownload", ex);
            }
        }

        private bool processresults(List<Result> results, iFormBuilderAPI.Page page, IWorkspace inWorkspace)
        {
            try
            {
                //Read through the Records
                // Cast the workspace to the IWorkspaceEdit interface.
                IWorkspaceEdit workspaceEdit = (IWorkspaceEdit)inWorkspace;

                // Start an edit session. An undo/redo stack isn't necessary in this case.
                workspaceEdit.StartEditing(false);

                // Start an edit operation.
                workspaceEdit.StartEditOperation();

                ITable dataset = ArcGISTools.Utilities.GetTable(inWorkspace, TranslateTableNames(page.NAME));
                // Get the class's attachment manager.
                ITableAttachments tableAttachments = (ITableAttachments)dataset;
                IAttachmentManager attachmentManager = tableAttachments.AttachmentManager;

                //Go through all the tables for Download
                foreach (Result token in results)
                {
                    try
                    {
                        processrow(token, inWorkspace, page, dataset, attachmentManager);
                    }
                    catch
                    { }
                }

                // method can be used.
                workspaceEdit.StopEditOperation();

                // Stop the edit session. The saveEdits parameter indicates the edit session
                // will be committed.
                workspaceEdit.StopEditing(true);

                //Set the Syncronization Flag to Future Updates
                UpdateSyncTables(inWorkspace, page);

                return true;
            }
            catch (Exception ex)
            {
                Debug.Write(ex.Message);
                throw new Exception("Error in process results", ex);
            }
        }

        private void UpdateSyncTables(IWorkspace inWorkspace, iFormBuilderAPI.Page page)
        {
            ITable dataset = ArcGISTools.Utilities.GetTable(inWorkspace, "SynchronizationTable");
            //Search for this Page Name
            ICursor cursor;
            IQueryFilter qF = new QueryFilter();
            qF.WhereClause = string.Format("SynchronizationPageID={0}", page.ID);
            cursor = dataset.Search(qF, false);
            IRow row = cursor.NextRow();
            if (row == null)
                row = dataset.CreateRow();

            int index = row.Fields.FindField("SynchronizationDate");
            if (index != -1)
                row.set_Value(row.Fields.FindField("SynchronizationDate"), DateTime.Now);
            index = row.Fields.FindField("SynchronizationPageID");
            if (index != -1)
                row.set_Value(row.Fields.FindField("SynchronizationPageID"), page.ID);
            index = row.Fields.FindField("SynchronizationTableName");
            if (index != -1)
                row.set_Value(row.Fields.FindField("SynchronizationTableName"), page.NAME);

            //Find the Highest ID in the parent Table and Set the Record ID flag
            index = row.Fields.FindField("SynchronizationRecordID");
            if (index != -1)
                row.set_Value(row.Fields.FindField("SynchronizationRecordID"), ArcGISTools.Utilities.GetHighestID(inWorkspace,page.NAME,"ID"));


            row.Store();
        }

        private String TranslateFieldName(String fieldname)
        {
            string name;
            try
            {
                if (fieldname == "ID")
                    return fieldname;

                Regex rx = new Regex("^[a-zA-Z][a-zA-Z0-9_]+[a-zA-Z0-9]$");
                //Check the String Name
                name = fieldname.Replace("_", "");
                if (name.Length > 64)
                    name = name.Substring(0, 64);

                if (!rx.IsMatch(name))
                {
                    name = fieldname;
                    //need to fix this string
                    name = fieldname.Substring(0, name.Length - 1);
                }

                return name;
            }
            catch(Exception ex)
            {
                Console.WriteLine(string.Format("Error Translating Name:{0}",ex.Message));
                return null;
            }
        }
        private bool processrow(Result record, IWorkspace inWorkspace, iFormBuilderAPI.Page pageid, ITable dataset, IAttachmentManager attachmentManager)
        {
            try
            {
                IRow row = dataset.CreateRow();
                IField field;
                object value;
                int index;
                foreach (KeyValuePair<string, object> kvp in record.Record)
                {
                    // Create a row. The row's attribute values should be set here and if
                    // a feature is being created, the shape should be set as well.
                    try
                    {
                        index = row.Fields.FindField(TranslateFieldName(kvp.Key));
                    }
                    catch (Exception ex)
                    {
                        throw new Exception("Error during index management", ex);
                    }

                    if (index == -1)
                    {
                        Console.WriteLine("Key:{0}  Value:{1}", kvp.Key, kvp.Value); 
                        try
                        {
                        if (kvp.Value != null)
                        {
                            //This is likely a multi select (lets check and process)
                            foreach (Element ele in pageid.Elements)
                            {
                                if (ele.NAME == kvp.Key && ele.WidgetType == Element.Widget.MultiSelect)
                                {
                                    OptionList optList = iformbuilder.GetOptionList(ele.OPTION_LIST_ID);
                                    System.Array arr = kvp.Value.ToString().Split(',');
                                    //Loop throught the array and the option this to find the correct field this value goes into
                                    bool foundfield = false;
                                    //find this label value in the optionlist
                                    foreach (Option opt in optList.OPTIONS)
                                    {
                                        string fName = string.Format("{0}_{1}", kvp.Key, opt.KEY_VALUE);
                                        index = row.Fields.FindField(TranslateFieldName(fName));
                                        //Field was not found
                                        if (index == -1)
                                        {
                                            Console.WriteLine(string.Format("Field not found:{0}", fName));
                                            continue;
                                        }

                                        field = row.Fields.get_Field(index);

                                        foreach (System.Object o in arr) // (int i = 0; i < arr.Length; i++)
                                        {
                                            if (o.ToString() == opt.KEY_VALUE)
                                            {
                                                foundfield = true;
                                                value = 1;
                                                this.SetRowValue(row, index, value);
                                                break;
                                            }
                                        }
                                        if (!foundfield)
                                        {
                                            value = 0;
                                            this.SetRowValue(row, index, value);
                                        }

                                        foundfield = false;
                                    }
                                }
                            }
                        }
                                            }
                    catch (Exception ex)
                    {
                        throw new Exception("Error during multi-select management", ex);
                    }
                    }
                    else
                    {
                        field = row.Fields.get_Field(index);
                        value = kvp.Value;
                        if (value != null && field.Type == esriFieldType.esriFieldTypeString)
                        {
                            //Make sure the value is less than the string length
                            if (value.ToString().Length > field.Length)
                                value = value.ToString().Substring(0, field.Length);
                        }
                        this.SetRowValue(row, index, value);

                        //Add the Attachment if relevant
                        if (kvp.Value != null && kvp.Value.ToString().Contains("https"))
                            AttachImage(attachmentManager, kvp.Value.ToString(), row.OID);
                    }
                }



                if (record.RecordLocation != null)
                {
                    foreach (string recordfield in this.iFormLocationItems)
                    {
                        this.SetRowValue(row, row.Fields.FindField(recordfield), record.RecordLocation.GetLocationValue(recordfield));
                    }

                    if (row.Fields.FindField("Shape") != -1)
                    {
                        //If this is a Feature Class Set the Shape Field
                        IPoint p;
                        p = new PointClass();
                        p.X = record.RecordLocation.X;
                        p.Y = record.RecordLocation.Y;
                        Debug.WriteLine(string.Format("X = {0}, Y = {1}", p.X, p.Y));
                        this.SetRowValue(row, row.Fields.FindField("Shape"), p);
                    }
                }

                //Add some information into the Special Fields
                if (row.Fields.FindField("PAGEID") != -1)
                {
                    this.SetRowValue(row,row.Fields.FindField("PAGEID"), pageid.ID);
                }
                if (record.HasChildren)
                {

                    string tablename = String.Empty;
                    ITable table = null;
                    foreach (Result child in record.Children)
                    {
                        if (tablename != TranslateTableNames(child.PageName))
                        {
                            tablename = TranslateTableNames(child.PageName);
                            table = ArcGISTools.Utilities.GetTable(inWorkspace, tablename);
                            // Get the class's attachment manager.
                            ITableAttachments tableAttachments = (ITableAttachments)table;
                            if (tableAttachments != null)
                                attachmentManager = tableAttachments.AttachmentManager;
                            else
                                attachmentManager = null;
                        }
                        processrow(child, inWorkspace, child.iFormPage, table, attachmentManager);
                    }
                }

                //Check to see what the highest value for the ID is
                row.Store();
                return true;
            }
            catch (Exception ex)
            {
                throw new Exception("Error during row processing", ex);
            }
        }

        private void SetRowValue(IRow row, int field, object value)
        {
            try
            {
                row.set_Value(field, value);
            }
            catch (Exception ex)
            {
                //Write the Exeception but do not throw
                Console.WriteLine(string.Format("Error setting row value:  {0}", ex.Message));
            }
        }

        private void AttachImage(IAttachmentManager attachmentManager, string url, int RowOID)
        {
            WebClient wc = new WebClient();
            byte[] bytes = wc.DownloadData(url);

            // Open a file as a memory blob stream.
            IMemoryBlobStream2 memoryBlobStream = new MemoryBlobStreamClass();
            memoryBlobStream.ImportFromMemory(ref bytes[0], (uint)bytes.Length);

            //Parse out the file name off the URL
            string strFileName = url.Substring(url.LastIndexOf("/") + 1);

            // Create an attachment.
            IAttachment attachment = new AttachmentClass
            {
                ContentType = "image/jpeg",
                Data = memoryBlobStream,
                Name = strFileName
            };

            // Assign the attachment to the feature with ObjectID (OID) = 1.
            attachmentManager.AddAttachment(RowOID, attachment);
        }

        private void CreateStandaloneFeatureClass(IFeatureWorkspace workspace, iFormBuilderAPI.Page page, bool SubFormAsFeatureClass)
        {
            try
            {
                // Create a fields collection for the feature class.
                IFields fieldsCollection = new FieldsClass();
                IFieldsEdit fieldsEdit = (IFieldsEdit)fieldsCollection;
                IField field = null;

                foreach (Element ele in page.Elements)
                {
                    //If there is a subform create a new stand-alone feature class or table
                    if (!ele.IgnoreElement)
                    {

                        if (ele.WidgetType == Element.Widget.MultiSelect)
                        {
                            //If this is a Multi Select Field Break the Option list into individual fields to allow the data to be analyzed better
                            //Get the Option List for this multi select
                            OptionList optList = iformbuilder.GetOptionList(ele.OPTION_LIST_ID);
                            foreach (Option opt in optList.OPTIONS)
                            {
                                field = ArcGISTools.Utilities.translateIFormToEsri(ele,opt);
                                fieldsEdit.AddField(field);
                            }
                        }
                        else
                        {
                            //Add a Field to the Workspace
                            field = ArcGISTools.Utilities.translateIFormToEsri(ele);
                            fieldsEdit.AddField(field);
                            Console.WriteLine(field.Name);
                        }

                        if (ele.OPTION_LIST_ID != 0 && ele.WidgetType != Element.Widget.MultiSelect)
                        {
                            assigntodomain assDom = new assigntodomain();
                            assDom.domain = CreateWorkspaceDomains((IWorkspace)workspace, ele.OPTION_LIST_ID, ele.WidgetType);
                            if (assDom.domain != null)
                            {
                                assDom.ID = ele.OPTION_LIST_ID;
                                assDom.field = field;
                                assDom.name = TranslateTableNames(page.NAME);

                                domainassignments.Add(assDom);
                            }
                        }
                    }
                }

                // Add an ObjectID field to the fields collection. This is mandatory for feature classes.
                IField oidField = new FieldClass();
                IFieldEdit oidFieldEdit = (IFieldEdit)oidField;
                oidFieldEdit.Name_2 = "OID";
                oidFieldEdit.Type_2 = esriFieldType.esriFieldTypeOID;
                fieldsEdit.AddField(oidField);

                //Add the iForm Specific Items
                foreach (IField iField in this.iFormItems(page))
                {
                    fieldsEdit.AddField(iField);
                }

                // Create a geometry definition (and spatial reference) for the feature class.
                IGeometryDef geometryDef = new GeometryDefClass();
                IGeometryDefEdit geometryDefEdit = (IGeometryDefEdit)geometryDef;
                geometryDefEdit.GeometryType_2 = esriGeometryType.esriGeometryPoint;
                ISpatialReferenceFactory spatialReferenceFactory = new
                    SpatialReferenceEnvironmentClass();
                ISpatialReference spatialReference =
                    spatialReferenceFactory.CreateGeographicCoordinateSystem((int)
                    esriSRGeoCSType.esriSRGeoCS_WGS1984);
                ISpatialReferenceResolution spatialReferenceResolution =
                    (ISpatialReferenceResolution)spatialReference;
                spatialReferenceResolution.ConstructFromHorizon();
                spatialReferenceResolution.SetDefaultXYResolution();
                ISpatialReferenceTolerance spatialReferenceTolerance =
                    (ISpatialReferenceTolerance)spatialReference;
                spatialReferenceTolerance.SetDefaultXYTolerance();
                geometryDefEdit.SpatialReference_2 = spatialReference;

                // Add a geometry field to the fields collection. This is where the geometry definition is applied.
                IField geometryField = new FieldClass();
                IFieldEdit geometryFieldEdit = (IFieldEdit)geometryField;
                geometryFieldEdit.Name_2 = "Shape";
                geometryFieldEdit.Type_2 = esriFieldType.esriFieldTypeGeometry;
                geometryFieldEdit.GeometryDef_2 = geometryDef;

                fieldsEdit.AddField(geometryField);

                // Use IFieldChecker to create a validated fields collection.
                IFieldChecker fieldChecker = new FieldCheckerClass();
                IEnumFieldError enumFieldError = null;
                IFields validatedFields = null;
                fieldChecker.ValidateWorkspace = (IWorkspace)workspace;
                fieldChecker.Validate(fieldsCollection, out enumFieldError, out validatedFields);
                IFieldError fieldError = null;
                if (enumFieldError != null)
                {
                    enumFieldError.Reset();
                    while ((fieldError = enumFieldError.Next()) != null)
                    {
                        IField errorField = fieldsCollection.get_Field(fieldError.FieldIndex);
                        Console.WriteLine("Field '{0}': Error '{1}'", errorField.Name,
                          fieldError.FieldError);
                    }
                }
 
                // The enumFieldError enumerator can be inspected at this point to determine 
                // which fields were modified during validation.

                // Create the feature class. Note that the CLSID parameter is null—this indicates to use the
                // default CLSID, esriGeodatabase.Feature (acceptable in most cases for feature classes).
                string tablename = page.NAME.Replace(" ", "");
                IFeatureClass featureClass = workspace.CreateFeatureClass
                    (TranslateTableNames(page.NAME), validatedFields, null, null,
                    esriFeatureType.esriFTSimple, "Shape", "");

                //Enable the Attachments for this feature class
                EnableAttachments((IDataset)featureClass);

                //Process any Subforms for this pages
                foreach (iFormBuilderAPI.Page subform in page.Subforms)
                {
                    if (SubFormAsFeatureClass)
                        CreateStandaloneFeatureClass(workspace, subform, SubFormAsFeatureClass);
                    else
                        CreateObjectClass((IWorkspace)workspace, subform);
                }
            }
            catch (Exception ex)
            {
                throw new Exception(string.Format("Error in Creating Standonlone Feature Class: {0} ", ex.Message));
            }
        }

        private void EnableAttachments(IDataset featureclass)
        {
            // Create the geoprocessor.
            IGeoProcessor2 gp = new GeoProcessorClass();

            // Create an IVariantArray to hold the parameter values.
            IVariantArray parameters = new VarArrayClass();

            // Populate the variant array with parameter values.
            parameters.Add(featureclass);

            // Execute the tool.
            gp.Execute("EnableAttachments_management", parameters, null);
        }

        private string TranslateTableNames(String TableName)
        {
            return TableName.Replace(" ", "");
        }
        private IObjectClass CreateObjectClass(IWorkspace workspace, iFormBuilderAPI.Page page)
        {
            try
            {
                //Set the Table object
                ITable table = null;

                //Check to see if this table already exists in the workspace
                if (!ArcGISTools.Utilities.DoesTableExist(workspace, TranslateTableNames(page.NAME)))
                {
                    IFields fieldsCollection = new FieldsClass();
                    IFieldsEdit fieldsEdit = (IFieldsEdit)fieldsCollection;
                    foreach (Element ele in page.Elements)
                    {
                        Console.WriteLine(ele.NAME);
                        //If there is a subform create a new stand-alone table
                        if (ele.DATA_TYPE != 18)
                        {
                            //Add a Field to the Workspace
                            IField field = ArcGISTools.Utilities.translateIFormToEsri(ele);
                            fieldsEdit.AddField(field);

                            //Add the Field Information to the Metadata Table


                            if (ele.OPTION_LIST_ID != 0)
                            {
                                //Translate the Pick List into the Coded Value Domain
                                assigntodomain assDom = new assigntodomain();
                                assDom.domain = CreateWorkspaceDomains((IWorkspace)workspace, ele.OPTION_LIST_ID, ele.WidgetType);
                                if (assDom.domain != null)
                                {
                                    assDom.ID = ele.OPTION_LIST_ID;
                                    assDom.field = field;
                                    assDom.name = TranslateTableNames(page.NAME);

                                    domainassignments.Add(assDom);
                                }
                            }
                        }
                    }

                    //Add the iForm Specific Items
                    foreach (IField iField in this.iFormItems(page))
                    {
                        fieldsEdit.AddField(iField);
                    }

                    IFeatureWorkspace featureWorkspace = (IFeatureWorkspace)workspace;
                    IObjectClassDescription ocDescription = new ObjectClassDescriptionClass();

                    // Use IFieldChecker to create a validated fields collection.
                    IFieldChecker fieldChecker = new FieldCheckerClass();
                    IEnumFieldError enumFieldError = null;
                    IFields validatedFields = null;
                    fieldChecker.ValidateWorkspace = workspace;
                    fieldChecker.Validate(fieldsCollection, out enumFieldError, out validatedFields);

                    // The enumFieldError enumerator can be inspected at this point to determine 
                    // which fields were modified during validation.
                    table = featureWorkspace.CreateTable(TranslateTableNames(page.NAME), validatedFields, ocDescription.InstanceCLSID, null, "");
                }
                else
                    table = ArcGISTools.Utilities.GetTable((IWorkspace)workspace, TranslateTableNames(page.NAME));

                //Enable the Attachments for this feature class
                EnableAttachments((IDataset)table);

                //Process any Subforms for this pages
                foreach (iFormBuilderAPI.Page subform in page.Subforms)
                {
                    CreateObjectClass((IWorkspace)workspace, subform);
                }

                return (IObjectClass)table;
            }
            catch (Exception ex)
            {
                throw new Exception(string.Format("Error in Creating Object class: {0} Table Name: {1} " ,ex.Message, page.NAME));
            }
        }


        private void CreateMetadataTables(IWorkspace workspace,iFormBuilderAPI.Page schemapage)
        {
            IFields fieldsCollection = new FieldsClass();
            IFieldsEdit fieldsEdit = (IFieldsEdit)fieldsCollection;

            IField field = new FieldClass();
            IFieldEdit fieldedit = (IFieldEdit)field;
            fieldedit.Name_2 = "TableName";
            fieldedit.Type_2 = esriFieldType.esriFieldTypeString;
            fieldedit.Length_2 = 100;
            fieldsEdit.AddField(field);

            field = new FieldClass();
            fieldedit = (IFieldEdit)field;
            fieldedit.Name_2 = "FieldName";
            fieldedit.Type_2 = esriFieldType.esriFieldTypeString;
            fieldedit.Length_2 = 100;
            fieldsEdit.AddField(field);

            field = new FieldClass();
            fieldedit = (IFieldEdit)field;
            fieldedit.Name_2 = "FieldType";
            fieldedit.Type_2 = esriFieldType.esriFieldTypeString;
            fieldsEdit.AddField(field);

            field = new FieldClass();
            fieldedit = (IFieldEdit)field;
            fieldedit.Name_2 = "FieldAlias";
            fieldedit.Type_2 = esriFieldType.esriFieldTypeString;
            fieldedit.Length_2 = 500;
            fieldsEdit.AddField(field);

            field = new FieldClass();
            fieldedit = (IFieldEdit)field;
            fieldedit.Name_2 = "DomainName";
            fieldedit.Type_2 = esriFieldType.esriFieldTypeString;
            fieldedit.Length_2 = 100;
            fieldsEdit.AddField(field);


            IFeatureWorkspace featureWorkspace = (IFeatureWorkspace)workspace;
            IObjectClassDescription ocDescription = new ObjectClassDescriptionClass();

            // Use IFieldChecker to create a validated fields collection.
            IFieldChecker fieldChecker = new FieldCheckerClass();
            IEnumFieldError enumFieldError = null;
            IFields validatedFields = null;
            fieldChecker.ValidateWorkspace = workspace;
            fieldChecker.Validate(fieldsCollection, out enumFieldError, out validatedFields);

            // The enumFieldError enumerator can be inspected at this point to determine 
            // which fields were modified during validation.
            ITable table = featureWorkspace.CreateTable("TableInformation", validatedFields,
                ocDescription.InstanceCLSID, null, "");

            //Create the Pick List Table
            fieldsCollection = new FieldsClass();
            fieldsEdit = (IFieldsEdit)fieldsCollection;

            field = new FieldClass();
            fieldedit = (IFieldEdit)field;
            fieldedit.Name_2 = "DomainID";
            fieldedit.Type_2 = esriFieldType.esriFieldTypeString;
            fieldedit.Length_2 = 100;
            fieldsEdit.AddField(field);

            field = new FieldClass();
            fieldedit = (IFieldEdit)field;
            fieldedit.Name_2 = "DomainName";
            fieldedit.Type_2 = esriFieldType.esriFieldTypeString;
            fieldedit.Length_2 = 100;
            fieldsEdit.AddField(field);

            field = new FieldClass();
            fieldedit = (IFieldEdit)field;
            fieldedit.Name_2 = "Code";
            fieldedit.Type_2 = esriFieldType.esriFieldTypeString;
            fieldedit.Length_2 = 100;
            fieldsEdit.AddField(field);

            field = new FieldClass();
            fieldedit = (IFieldEdit)field;
            fieldedit.Name_2 = "Value";
            fieldedit.Type_2 = esriFieldType.esriFieldTypeString;
            fieldsEdit.AddField(field);

            fieldChecker = new FieldCheckerClass();
            enumFieldError = null;
            validatedFields = null;
            fieldChecker.ValidateWorkspace = workspace;
            fieldChecker.Validate(fieldsCollection, out enumFieldError, out validatedFields);

            // The enumFieldError enumerator can be inspected at this point to determine 
            // which fields were modified during validation.
            table = featureWorkspace.CreateTable("DomainList", validatedFields,
                ocDescription.InstanceCLSID, null, "");

            //Populate the Metadata Tables
            SetMetadataInformation(workspace, schemapage);
            return;
        }

        private void SetMetadataInformation(IWorkspace workspace,iFormBuilderAPI.Page shemaPage)
        {
            //Process all the page elements
            ITable dataset = ArcGISTools.Utilities.GetTable(workspace, "TableInformation");
            ITable domainsdataset = ArcGISTools.Utilities.GetTable(workspace, "DomainList");
            IRow row;
            foreach (Element ele in shemaPage.Elements)
            {
                if (!ele.IgnoreElement)
                {
                    try
                    {
                        row = dataset.CreateRow();
                        row.set_Value(row.Fields.FindField("TableName"), shemaPage.NAME);
                        row.set_Value(row.Fields.FindField("FieldName"), ele.NAME);
                        row.set_Value(row.Fields.FindField("FieldAlias"), ele.Alias);
                        row.set_Value(row.Fields.FindField("FieldType"), ele.WidgetType);
                        row.set_Value(row.Fields.FindField("DomainName"), ele.OPTION_LIST_ID);
                        row.Store();


                    }
                    catch (Exception ex)
                    {
                        //ignore the metadata error (not critical)
                        Console.WriteLine(ex.Message);
                        continue;
                    }
                }

            }

            foreach (iFormBuilderAPI.Page subform in shemaPage.Subforms)
            {
                SetMetadataInformation(workspace,subform);
            }
        }

        private void PopulateDomainMetaDataTable(OptionList optList, IWorkspace workspace)
        {
            ITable domainsdataset = ArcGISTools.Utilities.GetTable(workspace, "DomainList");
            IRow row;
            foreach (Option opt in optList.OPTIONS)
            {
                row = domainsdataset.CreateRow();
                row.set_Value(row.Fields.FindField("DomainID"), optList.OPTION_LIST_ID);
                row.set_Value(row.Fields.FindField("DomainName"), optList.NAME);
                row.set_Value(row.Fields.FindField("Code"), opt.KEY_VALUE);
                row.set_Value(row.Fields.FindField("Value"), opt.LABEL);
                row.Store();
            }
            return;
        }

        /// <summary>
        /// Creates the sync table.
        /// </summary>
        /// <param name="workspace">The workspace.</param>
        /// <returns></returns>
        private IObjectClass CreateSyncTable(IWorkspace workspace)
        {
            IFields fieldsCollection = new FieldsClass();
            IFieldsEdit fieldsEdit = (IFieldsEdit)fieldsCollection;

            IField field = new FieldClass();
            IFieldEdit fieldedit = (IFieldEdit)field;
            fieldedit.Name_2 = "SynchronizationDate";
            fieldedit.Type_2 = esriFieldType.esriFieldTypeDate;
            fieldsEdit.AddField(field);

            field = new FieldClass();
            fieldedit = (IFieldEdit)field;
            fieldedit.Name_2 = "SynchronizationRecordID";
            fieldedit.Type_2 = esriFieldType.esriFieldTypeInteger;
            fieldsEdit.AddField(field);

            field = new FieldClass();
            fieldedit = (IFieldEdit)field;
            fieldedit.Name_2 = "SynchronizationPageID";
            fieldedit.Type_2 = esriFieldType.esriFieldTypeInteger;
            fieldsEdit.AddField(field);

            field = new FieldClass();
            fieldedit = (IFieldEdit)field;
            fieldedit.Name_2 = "SynchronizationTableName";
            fieldedit.Type_2 = esriFieldType.esriFieldTypeString;
            fieldedit.Length_2 = 100;
            fieldsEdit.AddField(field);


            IFeatureWorkspace featureWorkspace = (IFeatureWorkspace)workspace;
            IObjectClassDescription ocDescription = new ObjectClassDescriptionClass();

            // Use IFieldChecker to create a validated fields collection.
            IFieldChecker fieldChecker = new FieldCheckerClass();
            IEnumFieldError enumFieldError = null;
            IFields validatedFields = null;
            fieldChecker.ValidateWorkspace = workspace;
            fieldChecker.Validate(fieldsCollection, out enumFieldError, out validatedFields);

            // The enumFieldError enumerator can be inspected at this point to determine 
            // which fields were modified during validation.
            ITable table = featureWorkspace.CreateTable("SynchronizationTable", validatedFields,
                ocDescription.InstanceCLSID, null, "");

            return (IObjectClass)table;
        }

        /// <summary>
        /// Creates the workspace domains.
        /// </summary>
        /// <param name="workspace">The workspace.</param>
        /// <param name="optListId">The opt list id.</param>
        /// <param name="ele">The ele.</param>
        /// <returns></returns>
        private IDomain CreateWorkspaceDomains(IWorkspace workspace, int optListId, iFormBuilderAPI.Element.Widget ele)
        {
            if (_processeddomains.Contains(optListId))
                foreach (assigntodomain assdom in domainassignments)
                {
                    if (assdom.ID == optListId)
                        return assdom.domain;
                }

            OptionList optList = iformbuilder.GetOptionList(optListId);

            //check to see if this domain exists already
            if (ArcGISTools.Utilities.DoesDomainExist(workspace, optList.NAME))
                return null;

            // The code to create a coded value domain.
            ICodedValueDomain codedValueDomain = new CodedValueDomainClass();

            // Value and name pairs.
            foreach (Option opt in optList.OPTIONS)
            {
                //codedValueDomain.AddCode(opt.KEY_VALUE, opt.LABEL);
                //TODO:  Fix this a variable
                codedValueDomain.AddCode(opt.KEY_VALUE, opt.KEY_VALUE);
            }

            //Populate the netadata table for this domain
            PopulateDomainMetaDataTable(optList, workspace);

            // The code to set the common properties for the new coded value domain.
            IDomain domain = (IDomain)codedValueDomain;
            //Clean up the domain name
            domain.Name = Regex.Replace(optList.NAME, @"^""|""$|\\n?|/|\s+|'", string.Empty);
            if (ele == Element.Widget.Number || ele == Element.Widget.MultiSelect)
                domain.FieldType = esriFieldType.esriFieldTypeInteger;
            else
                domain.FieldType = esriFieldType.esriFieldTypeString;

            domain.SplitPolicy = esriSplitPolicyType.esriSPTDuplicate;
            domain.MergePolicy = esriMergePolicyType.esriMPTDefaultValue;

            _processeddomains.Add(optListId);
            return domain;
        }

        /// <summary>
        /// Assigns the domain to field.
        /// </summary>
        /// <param name="assdom">The assdom.</param>
        /// <param name="inWorkspace">The in workspace.</param>
        private void AssignDomainToField(assigntodomain assdom, ref IWorkspace inWorkspace)
        {
            if (!ArcGISTools.Utilities.DoesDomainExist(inWorkspace, assdom.domain.Name))
            {
                IWorkspaceDomains workspacedomains = (IWorkspaceDomains)inWorkspace;
                workspacedomains.AddDomain(assdom.domain);
            }

            IDataset dataset = ArcGISTools.Utilities.GetDataSet(inWorkspace, assdom.name);
            ITable featureClass = (ITable)dataset;

            // Get the workspace and cast it to the IWorkspaceDomains interface and get the requested domain.
            IWorkspaceDomains workspaceDomains = (IWorkspaceDomains)inWorkspace;
            IDomain domain = workspaceDomains.get_DomainByName(assdom.domain.Name);

            // Get the field to assign the domain to.
            IFields fields = featureClass.Fields;
            int fieldIndex = featureClass.FindField(assdom.field.Name);
            IField field = fields.get_Field(fieldIndex);

            // Check that the field and domain have the same field type.
            if (field.Type == domain.FieldType)
            {
                // Cast the feature class to the ISchemaLock and IClassSchemaEdit interfaces.
                ISchemaLock schemaLock = (ISchemaLock)featureClass;
                IClassSchemaEdit classSchemaEdit = (IClassSchemaEdit)featureClass;

                // Attempt to get an exclusive schema lock.
                try
                {
                    // Lock the class and alter the domain.
                    schemaLock.ChangeSchemaLock(esriSchemaLock.esriExclusiveSchemaLock);
                    classSchemaEdit.AlterDomain(assdom.field.Name, domain);
                    Console.WriteLine("The domain was successfully assigned.");
                }
                catch (Exception exc)
                {
                    // Handle the exception in a way appropriate for the application.
                    Console.WriteLine(exc.Message);
                }
                finally
                {
                    // Set the schema lock to be a shared lock.
                    schemaLock.ChangeSchemaLock(esriSchemaLock.esriSharedSchemaLock);
                }
            }
            else
            {
                Console.WriteLine("The field and the domain have different field types: " +
                    "Field = {0}, Domain = {1}", field.Type, domain.FieldType);
            }
        }

        /// <summary>
        /// Builds the relationship classes.
        /// </summary>
        /// <param name="page">The page.</param>
        /// <param name="workspace">The workspace.</param>
        private void BuildRelationshipClasses(iFormBuilderAPI.Page page, IWorkspace workspace)
        {
            try
            {
                //Build the Relationship Classes between the form
                IFeatureWorkspace featureWorkspace = workspace as IFeatureWorkspace;
                IDataset pDsOrigin;
                IDataset pDsDest;

                foreach (iFormBuilderAPI.Page subform in page.Subforms)
                {
                    pDsOrigin = ArcGISTools.Utilities.GetDataSet(workspace, TranslateTableNames(page.NAME));
                    pDsDest = ArcGISTools.Utilities.GetDataSet(workspace, TranslateTableNames(subform.NAME));
                    // Creating a relationship class without an intermediate table.
                    string relationshipname = string.Format("{0}_{1}", TranslateTableNames(page.NAME), TranslateTableNames(subform.NAME));
                    if (!ArcGISTools.Utilities.DoesRelationshipClassExist(workspace, relationshipname))
                    {
                        IRelationshipClass relClass =
                            featureWorkspace.CreateRelationshipClass(relationshipname,
                            pDsOrigin as IObjectClass, pDsDest as IObjectClass, TranslateTableNames(subform.NAME), TranslateTableNames(page.NAME),
                            esriRelCardinality.esriRelCardinalityOneToMany,
                            esriRelNotification.esriRelNotificationNone, true, false, null, "ID", "", "PARENTRECORDID", "");
                    }

                    //Build an Relationship classes for any of the children
                    if (subform.HasSubForms)
                    {
                        BuildRelationshipClasses(subform, workspace);
                    }
                }
            }
            catch (Exception ex)
            {
            }
            return;
        }

        public bool BuildFlatTable(iFormBuilderAPI.Page page, IWorkspace workspace)
        {
            try
            {
                IDataset pDsOrigin;
                IDataset pDsDest;
                //Build the Relationship Classes between the form
                IFeatureWorkspace featureWorkspace = workspace as IFeatureWorkspace;

                //Create virtual relate
                // Build a memory relationship class.
                pDsOrigin = ArcGISTools.Utilities.GetDataSet(workspace, TranslateTableNames(page.NAME));
                string flat_table = string.Format("{0}_flat", (pDsOrigin as IFeatureClass).AliasName);
                IDataset pDataset;
                if (ArcGISTools.Utilities.DoesTableExist(featureWorkspace as IWorkspace, flat_table))
                {
                    pDataset = ArcGISTools.Utilities.GetDataSet(featureWorkspace as IWorkspace, flat_table);
                    pDataset.Delete();
                }

                //Create the Flat table to process
                Geoprocessor gp = new Geoprocessor();
                IWorkspace tempWS = CreateFeatureWorkspace();
                gp.SetEnvironmentValue("workspace", tempWS.PathName);

                //IFeatureClass flatTable = ArcGISTools.Utilities.GetDataSet(featureWorkspace as IWorkspace, flat_table) as IFeatureClass;
                MakeFeatureLayer makeLayer = new MakeFeatureLayer(pDsOrigin, "JoinLayer");
                gp.Execute(makeLayer, null);

                foreach (iFormBuilderAPI.Page subform in page.Subforms)
                {
                    pDsDest = ArcGISTools.Utilities.GetDataSet(workspace, TranslateTableNames(subform.NAME));
                    AddJoin addJoin = new AddJoin("JoinLayer", "ID", pDsDest as IFeatureClass, "PARENTRECORDID");
                    addJoin.join_type = "KEEP_ALL";
                    //addJoin.out_layer_or_view = makeLayer.out_layer;
                    gp.Execute(addJoin, null);
                }

                // Initialize the CopyFeatures tool
                CopyFeatures copyFeaturesOut = new CopyFeatures();
                copyFeaturesOut.in_features = "JoinLayer";
                copyFeaturesOut.out_feature_class = string.Format(@"{0}\{1}", workspace.PathName, flat_table);
                gp.Execute(copyFeaturesOut, null);
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }

        }

        public IWorkspace CreateFeatureWorkspace()
        {

            // Create an in-memory workspace factory.
            Type factoryType = Type.GetTypeFromProgID(
                "esriDataSourcesGDB.InMemoryWorkspaceFactory");
            IWorkspaceFactory workspaceFactory = (IWorkspaceFactory)Activator.CreateInstance
                (factoryType);

            // Create an in-memory workspace.
            IWorkspaceName workspaceName = workspaceFactory.Create(null, "TempWS", null, 0);

            // Open the workspace using the name object.
            IName name = (IName)workspaceName;
            IWorkspace workspace = (IWorkspace)name.Open();
            return workspace;
        }

        public static IFeatureClass CreateFeatureClass(IFeatureWorkspace featWorkspace,IFeatureClass fcClass, string name)
        {
            ISpatialReferenceFactory pSpatialRefFactory = new SpatialReferenceEnvironmentClass();
            IGeographicCoordinateSystem pGeographicCoordSys = pSpatialRefFactory.CreateGeographicCoordinateSystem((int)esriSRGeoCSType.esriSRGeoCS_WGS1984);
            ISpatialReference pSpaRef = pGeographicCoordSys;
            pSpaRef.SetDomain(-180, 180, -90, 90);

            IFieldsEdit pFldsEdt = new FieldsClass();
            IFieldEdit pFldEdt = new FieldClass();

            pFldEdt = new FieldClass();
            pFldEdt.Type_2 = esriFieldType.esriFieldTypeOID;
            pFldEdt.Name_2 = "OBJECTID";
            pFldEdt.AliasName_2 = "OBJECTID";
            pFldsEdt.AddField(pFldEdt);

            double dGridSize = 1000;

            IGeometryDefEdit pGeoDef;
            pGeoDef = new GeometryDefClass();
            pGeoDef.GeometryType_2 = esriGeometryType.esriGeometryPolygon;
            pGeoDef.SpatialReference_2 = pSpaRef;
            pGeoDef.GridCount_2 = 1;
            pGeoDef.set_GridSize(0, dGridSize);
            pGeoDef.AvgNumPoints_2 = 0;
            pGeoDef.HasM_2 = false;
            pGeoDef.HasZ_2 = false;

            pFldEdt = new FieldClass();
            pFldEdt.Name_2 = "SHAPE";
            pFldEdt.AliasName_2 = "SHAPE";
            pFldEdt.Type_2 = esriFieldType.esriFieldTypeGeometry;
            pFldEdt.GeometryDef_2 = pGeoDef;
            pFldsEdt.AddField(pFldEdt);

            IFeatureClass pFClass = featWorkspace.CreateFeatureClass(name, pFldsEdt, null, null, esriFeatureType.esriFTSimple, "SHAPE", "");
            return pFClass;
        }


        public IFeatureClass JoinLayer_Table(IFeatureWorkspace featureWorkspace, IFeatureClass fClass, IFeatureClass jClass, iFormBuilderAPI.Page page, iFormBuilderAPI.Page subpage, string workspacepath)
        {
            try
            {
                Geoprocessor gp = new Geoprocessor();
                //Check to see if the flat table exists

                //fClass = ArcGISTools.Utilities.GetDataSet(featureWorkspace as IWorkspace, flat_table) as IFeatureClass;


                return null;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private bool createFlatTable(string workspacename,string sourcename)
        {
            // Create workspace name objects.
            IWorkspaceName sourceWorkspaceName = new WorkspaceNameClass();
            IWorkspaceName targetWorkspaceName = new WorkspaceNameClass();
            IName targetName = (IName)targetWorkspaceName;

            // Set the workspace name properties.
            sourceWorkspaceName.PathName = workspacename;
            sourceWorkspaceName.WorkspaceFactoryProgID = "esriDataSourcesGDB.FileGDBWorkspaceFactory";
            targetWorkspaceName.PathName = workspacename;
            targetWorkspaceName.WorkspaceFactoryProgID = "esriDataSourcesGDB.FileGDBWorkspaceFactory";

            // Create a name object for the source feature class.
            IFeatureClassName featureClassName = new FeatureClassNameClass();

            // Set the featureClassName properties.
            IDatasetName sourceDatasetName = (IDatasetName)featureClassName;
            sourceDatasetName.WorkspaceName = sourceWorkspaceName;
            sourceDatasetName.Name = sourcename;
            IName sourceName = (IName)sourceDatasetName;

            // Create an enumerator for source datasets.
            IEnumName sourceEnumName = new NamesEnumeratorClass();
            IEnumNameEdit sourceEnumNameEdit = (IEnumNameEdit)sourceEnumName;

            // Add the name object for the source class to the enumerator.
            sourceEnumNameEdit.Add(sourceName);

            // Create a GeoDBDataTransfer object and a null name mapping enumerator.
            IGeoDBDataTransfer geoDBDataTransfer = new GeoDBDataTransferClass();
            IEnumNameMapping enumNameMapping = null;

            // Use the data transfer object to create a name mapping enumerator.
            Boolean conflictsFound = geoDBDataTransfer.GenerateNameMapping(sourceEnumName, targetName, out enumNameMapping);
            enumNameMapping.Reset();
            // Check for conflicts.
            if (conflictsFound)
            {
                // Iterate through each name mapping.
                enumNameMapping.Reset();
                INameMapping nameMapping = null;
                while ((nameMapping = enumNameMapping.Next()) != null)
                {
                    // Append a "_new" suffix to the mapping's target name.
                    nameMapping.TargetName += "_flat";

                    // Iterate through the mapping's children.
                    IEnumNameMapping childEnumNameMapping = nameMapping.Children;
                    if (childEnumNameMapping != null)
                    {
                        childEnumNameMapping.Reset();

                        // Iterate through each child mapping.
                        INameMapping childNameMapping = null;
                        while ((childNameMapping = childEnumNameMapping.Next()) != null)
                        {
                            childNameMapping.TargetName += "_flat";
                        }
                    }
                }

            }

            // Start the transfer.
            geoDBDataTransfer.Transfer(enumNameMapping, targetName);
            return true;
        }

        public string ExportFlatTable(IFeatureClass srcTable,string workspacepath,string outputname)
        {
            try
            {
                IDatasetName srcName = (srcTable as IDataset).FullName as IDatasetName;

                IWorkspaceName destWsName = new WorkspaceNameClass();
                destWsName.PathName = workspacepath;
                destWsName.WorkspaceFactoryProgID = "esriDataSourcesGDB.FileGDBWorkspaceFactory";
                
                IDatasetName destName = new FeatureClassNameClass() as IDatasetName;
                destName.Name = string.Format("A{0}", DateTime.Now.Ticks.ToString());
                destName.WorkspaceName = destWsName;

                IFeatureClassName destFcName = destName as IFeatureClassName;
                destFcName.FeatureType = esriFeatureType.esriFTSimple;
                destFcName.ShapeFieldName = "Shape";
                destFcName.ShapeType = esriGeometryType.esriGeometryPoint;

                IQueryFilter queryF = null;
                ISelectionSet selnSet = null;
                IGeometryDef geomDef = null;

                IExportOperation exOp = new ExportOperationClass();
                exOp.ExportFeatureClass(srcName, queryF, selnSet, geomDef, destFcName, 0);

                return destName.Name;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// Is the form items.
        /// </summary>
        /// <param name="page">The page.</param>
        /// <returns></returns>
        public List<IField> iFormItems(iFormBuilderAPI.Page page)
        {
            List<IField> _list = new List<IField>();
            IField field = new FieldClass();
            IFieldEdit fieldedit = (IFieldEdit)field;
            fieldedit.Name_2 = "ID";
            fieldedit.Type_2 = esriFieldType.esriFieldTypeInteger;
            _list.Add(fieldedit);

            field = new FieldClass();
            fieldedit = (IFieldEdit)field;
            fieldedit.Name_2 = "PAGEID";
            fieldedit.AliasName_2 = "Page ID";
            fieldedit.Type_2 = esriFieldType.esriFieldTypeInteger;
            fieldedit.DefaultValue_2 = page.ID;
            _list.Add(fieldedit);


            field = new FieldClass();
            fieldedit = (IFieldEdit)field;
            fieldedit.Name_2 = "PARENTRECORDID";
            fieldedit.AliasName_2 = "PARENT RECORD ID";
            fieldedit.Type_2 = esriFieldType.esriFieldTypeInteger;
            _list.Add(fieldedit);

            field = new FieldClass();
            fieldedit = (IFieldEdit)field;
            fieldedit.Name_2 = "PARENTPAGEID";
            fieldedit.AliasName_2 = "PARENT PAGE ID";
            fieldedit.Type_2 = esriFieldType.esriFieldTypeInteger;
            _list.Add(fieldedit);

            field = new FieldClass();
            fieldedit = (IFieldEdit)field;
            fieldedit.Name_2 = "PARENTELEMENTID";
            fieldedit.AliasName_2 = "PARENT ELEMENT ID";
            fieldedit.Type_2 = esriFieldType.esriFieldTypeInteger;
            _list.Add(fieldedit);

            field = new FieldClass();
            fieldedit = (IFieldEdit)field;
            fieldedit.Name_2 = "CREATEDDATE";
            fieldedit.AliasName_2 = "CREATED DATE";
            fieldedit.Type_2 = esriFieldType.esriFieldTypeDate;
            _list.Add(fieldedit);


            field = new FieldClass();
            fieldedit = (IFieldEdit)field;
            fieldedit.Name_2 = "CREATEDBY";
            fieldedit.AliasName_2 = "CREATED BY";
            fieldedit.Type_2 = esriFieldType.esriFieldTypeString;
            fieldedit.Length_2 = 100;
            _list.Add(fieldedit);

            field = new FieldClass();
            fieldedit = (IFieldEdit)field;
            fieldedit.Name_2 = "CREATEDLOCATION";
            fieldedit.AliasName_2 = "CREATED LOCATION";
            fieldedit.Type_2 = esriFieldType.esriFieldTypeString;
            fieldedit.Length_2 = 100;
            _list.Add(fieldedit);

            field = new FieldClass();
            fieldedit = (IFieldEdit)field;
            fieldedit.Name_2 = "CREATEDDEVICEID";
            fieldedit.AliasName_2 = "CREATED DEVICE ID";
            fieldedit.Type_2 = esriFieldType.esriFieldTypeString;
            fieldedit.Length_2 = 100;
            _list.Add(fieldedit);

            field = new FieldClass();
            fieldedit = (IFieldEdit)field;
            fieldedit.Name_2 = "MODIFIEDDATE";
            fieldedit.AliasName_2 = "MODIFIED DATE";
            fieldedit.Type_2 = esriFieldType.esriFieldTypeDate;
            _list.Add(fieldedit);

            field = new FieldClass();
            fieldedit = (IFieldEdit)field;
            fieldedit.Name_2 = "MODIFIEDBY";
            fieldedit.AliasName_2 = "MODIFIED BY";
            fieldedit.Type_2 = esriFieldType.esriFieldTypeString;
            fieldedit.Length_2 = 100;
            _list.Add(fieldedit);

            field = new FieldClass();
            fieldedit = (IFieldEdit)field;
            fieldedit.Name_2 = "MODIFIEDLOCATION";
            fieldedit.AliasName_2 = "MODIFIED LOCATION";
            fieldedit.Type_2 = esriFieldType.esriFieldTypeString;
            fieldedit.Length_2 = 100;
            _list.Add(fieldedit); ;

            field = new FieldClass();
            fieldedit = (IFieldEdit)field;
            fieldedit.Name_2 = "MODIFIEDDEVICEID";
            fieldedit.AliasName_2 = "MODIFIED DEVICE ID";
            fieldedit.Type_2 = esriFieldType.esriFieldTypeString;
            fieldedit.Length_2 = 100;
            _list.Add(fieldedit);

            field = new FieldClass();
            fieldedit = (IFieldEdit)field;
            fieldedit.Name_2 = "RECORDLINK";
            fieldedit.AliasName_2 = "RECORD LINK";
            fieldedit.Type_2 = esriFieldType.esriFieldTypeString;
            fieldedit.Length_2 = 100;
            _list.Add(fieldedit);

            field = new FieldClass();
            fieldedit = (IFieldEdit)field;
            fieldedit.Name_2 = "Latitude";
            fieldedit.Type_2 = esriFieldType.esriFieldTypeDouble;
            _list.Add(fieldedit);

            field = new FieldClass();
            fieldedit = (IFieldEdit)field;
            fieldedit.Name_2 = "Longitude";
            fieldedit.Type_2 = esriFieldType.esriFieldTypeDouble;
            _list.Add(fieldedit);

            field = new FieldClass();
            fieldedit = (IFieldEdit)field;
            fieldedit.Name_2 = "Altitude";
            fieldedit.Type_2 = esriFieldType.esriFieldTypeDouble;
            _list.Add(fieldedit);

            field = new FieldClass();
            fieldedit = (IFieldEdit)field;
            fieldedit.Name_2 = "Speed";
            fieldedit.Type_2 = esriFieldType.esriFieldTypeDouble;
            _list.Add(fieldedit);

            field = new FieldClass();
            fieldedit = (IFieldEdit)field;
            fieldedit.Name_2 = "Accuracy";
            fieldedit.Type_2 = esriFieldType.esriFieldTypeDouble;
            _list.Add(fieldedit);

            field = new FieldClass();
            fieldedit = (IFieldEdit)field;
            fieldedit.Name_2 = "Provider";
            fieldedit.Type_2 = esriFieldType.esriFieldTypeDouble;
            _list.Add(fieldedit);

            field = new FieldClass();
            fieldedit = (IFieldEdit)field;
            fieldedit.Name_2 = "Time";
            fieldedit.Type_2 = esriFieldType.esriFieldTypeDouble;
            _list.Add(fieldedit);
            return _list;
        }

        public ArrayList iFormLocationItems
        {
            get
            {
                ArrayList arrList = new ArrayList();
                arrList.Add("Latitude");
                arrList.Add("Longitude");
                arrList.Add("Altitude");
                arrList.Add("Speed");
                arrList.Add("Accuracy");
                arrList.Add("Provider");
                arrList.Add("Time");
                return arrList;
            }
        }
    }

    internal class assigntodomain
    {
        internal int ID { get; set; }
        internal string name { get; set; }
        internal IFeatureClass featureclass { get; set; }
        internal IDomain domain { get; set; }
        internal IField field { get; set; }
    }
}
