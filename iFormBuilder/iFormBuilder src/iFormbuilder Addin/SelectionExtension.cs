// Copyright 2012 ESRI
// 
// All rights reserved under the copyright laws of the United States
// and applicable international laws, treaties, and conventions.
// 
// You may freely redistribute and use this sample code, with or
// without modification, provided you include the original copyright
// notice and use restrictions.
// 
// See the use restrictions at <your ArcGIS install location>/DeveloperKit10.1/userestrictions.txt.
// 

using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using ESRI.ArcGIS.Framework;
using ESRI.ArcGIS.esriSystem;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Desktop.AddIns;
using ESRI.ArcGIS.Editor;
using ESRI.ArcGIS.Geodatabase;

namespace iFormToolbar
{
    public class SelectionExtension : ESRI.ArcGIS.Desktop.AddIns.Extension
    {
        private IMap m_map;
        private bool m_hasSelectableLayer;
        private static IDockableWindow s_dockWindow;
        private static SelectionExtension s_extension;
        private iFormBuilderAPI.iFormBuilder api;
        private IEditor3 m_editor;
        private IEditEvents_Event m_editEvents;
        private IEditEvents5_Event m_editEvents5;
        private List<IFeature> deletes { get; set; }
        private List<IFeature> deltas { get; set; }
        private List<IFeature> adds { get; set; }
        private List<iFormBuilderAPI.User> _users { get; set; }
        private bool processedits;

        // Overrides

        protected override void OnStartup()
        {
            s_extension = this;

            // Wire up events
            ArcMap.Events.NewDocument += ArcMap_NewOpenDocument;
            ArcMap.Events.OpenDocument += ArcMap_NewOpenDocument;

            //Listen for a Stop Edits Events to be fired
            //get the editor
            UID editorUid = new UID();
            editorUid.Value = "esriEditor.Editor";
            m_editor = ArcMap.Application.FindExtensionByCLSID(editorUid) as IEditor3;
            m_editEvents = m_editor as IEditEvents_Event;

            EnableEditListeners();
            _users = new List<iFormBuilderAPI.User>();
            Initialize();
        }

        private void EnableEditListeners()
        {
            m_editEvents = m_editor as IEditEvents_Event;
            m_editEvents.OnStartEditing += new IEditEvents_OnStartEditingEventHandler(m_editEvents_OnStartEditing);
            m_editEvents.OnStopEditing += new IEditEvents_OnStopEditingEventHandler(m_editEvents_OnStopEditing);
            m_editEvents.OnDeleteFeature += new IEditEvents_OnDeleteFeatureEventHandler(m_editEvents_OnDeleteFeature);
            m_editEvents.OnChangeFeature += new IEditEvents_OnChangeFeatureEventHandler(m_editEvents_OnChangeFeature);
            m_editEvents.OnCreateFeature += new IEditEvents_OnCreateFeatureEventHandler(m_editEvents_OnCreateFeature);
        }

        private void DisableEventListeners()
        {
            m_editEvents.OnStartEditing -= new IEditEvents_OnStartEditingEventHandler(m_editEvents_OnStartEditing);
            m_editEvents.OnStopEditing -= new IEditEvents_OnStopEditingEventHandler(m_editEvents_OnStopEditing);
            m_editEvents.OnDeleteFeature -= new IEditEvents_OnDeleteFeatureEventHandler(m_editEvents_OnDeleteFeature);
            m_editEvents.OnChangeFeature -= new IEditEvents_OnChangeFeatureEventHandler(m_editEvents_OnChangeFeature);
            m_editEvents.OnCreateFeature -= new IEditEvents_OnCreateFeatureEventHandler(m_editEvents_OnCreateFeature);
        }

        void m_editEvents_OnCreateFeature(IObject obj)
        {
            adds.Add(obj as IFeature);
        }

        void m_editEvents_OnStartEditing()
        {
            adds = new List<IFeature>();
            deletes = new List<IFeature>();
            deltas = new List<IFeature>();
            processedits = true;
        }

        void m_editEvents_OnChangeFeature(IObject obj)
        {
            //Check the Adds for this object
            bool addfeature = true;
            foreach (Feature feature in this.adds)
            {
                if (feature.OID == (obj as IFeature).OID)
                {
                    addfeature = false;
                    break;
                }
            }

            if (!deltas.Contains(obj as IFeature) && addfeature)
                deltas.Add(obj as IFeature);
        }

        void m_editEvents_OnDeleteFeature(IObject obj)
        {
            deletes.Add(obj as IFeature);
        }

        private void m_editEvents_OnStopEditing(bool save)
        {
            if (processedits)
            {
                DisableEventListeners();
                if (save)
                {
                    //Get the Pages that will be need to download against
                    List<iFormBuilderAPI.Page> pages = new List<iFormBuilderAPI.Page>();
                    //Check to see if Save was clicked on the Edits
                    long paged = 0;
                    foreach (IFeature feature in this.deltas)
                    {
                        paged = int.Parse(feature.get_Value(feature.Fields.FindField("PAGEID")).ToString());
                        if (!this.IsPageDownloaded(pages, paged))
                            pages.Add(api.GetPage(paged));

                        ConvertFeaturetoElement(GetPage(pages, paged), feature);
                    }

                    //Get and features that have been added
                    foreach (IFeature feature in this.adds)
                    {
                        paged = int.Parse(feature.get_Value(feature.Fields.FindField("PAGEID")).ToString());
                        if (!this.IsPageDownloaded(pages, paged))
                            pages.Add(api.GetPage(paged));

                        ConvertFeaturetoElement(GetPage(pages, paged), feature);

                    }

                    //Delete the Records
                    if (this.deletes.Count != 0)
                    {
                        List<int> todelete = new List<int>();
                        List<long> pageids = new List<long>();
                        foreach (IFeature feature in this.adds)
                        {
                            todelete.Add(int.Parse(feature.get_Value(feature.Fields.FindField("ID")).ToString()));
                            pageids.Add(int.Parse(feature.get_Value(feature.Fields.FindField("PAGEID")).ToString()));
                        }
                        api.DeleteRecords(pageids, todelete);
                    }
                }

                EnableEditListeners();
                processedits = false;
            }
        }

        private void ConvertFeaturetoElement(iFormBuilderAPI.Page page, IFeature feature)
        {
            try
            {
                //Create a New Record from this feature
                iFormBuilderAPI.RecordsCreator recC = new iFormBuilderAPI.RecordsCreator();
                iFormBuilderAPI.Record rec = new iFormBuilderAPI.Record();
                rec.PARENT_PAGE_ID = page.ID;
                rec.FIELDS = new List<iFormBuilderAPI.Field>();
                iFormBuilderAPI.Field field = new iFormBuilderAPI.Field();
                bool addfield = true;
                long recordid;
                foreach (iFormBuilderAPI.Element ele in page.Elements)
                {
                    //Check to see if this an element to ignore
                    if (ele.IgnoreElement)
                        continue;

                    //Does this field exist
                    //TODO:  Need further field validation steps
                    //Does a Domain Field Have a Value
                    //All required fields entered
                    if (feature.Fields.FindField(ele.NAME) != -1)
                    {
                        field = new iFormBuilderAPI.Field();
                        field.ELEMENT_ID = ele.ID;
                        field.VALUE = feature.get_Value(feature.Fields.FindField(ele.NAME)).ToString();
                        addfield = true;

                        if (ele.WidgetType == iFormBuilderAPI.Element.Widget.Image)
                        {
                            //Only add this element if the picture is properly formated
                            if (feature.get_Value(feature.Fields.FindField(ele.NAME)).ToString().Length == 0 || !feature.get_Value(feature.Fields.FindField(ele.NAME)).ToString().Contains(".jpg"))
                                addfield = false;
                            else
                                field.EXT = "JPG";
                        }
                        if (addfield)
                            rec.FIELDS.Add(field);
                    }
                }

                recC.RECORDS.Add(rec);
                recordid = api.CreateRecord(page.ID, recC);
                if (recordid != -1)
                {
                    //TODO:  Apply the record ID to this record to allow linkage to iFormBuilder
                    /*IDataset dataset = feature.Table as IDataset;
                    if(m_editor.EditState != esriEditState.esriStateEditing)
                        m_editor.StartEditing(dataset.Workspace);
                    m_editor.StartOperation();
                    ICursor update;
                    QueryFilter qf = new QueryFilter();
                    qf.WhereClause = string.Format("OBJECTID={0}",feature.OID);
                    update = feature.Table.Update(qf, true);
                    IRow row = update.NextRow();
                    row.set_Value(feature.Fields.FindField("ID"), recordid);
                    m_editor.StopOperation("Stop");
                    //if (m_editor.EditState != esriEditState.esriStateEditing)
                    //    m_editor.stop(dataset.Workspace);*/
                }
            }
            catch
            {

            }
        }

        private iFormBuilderAPI.Page GetPage(List<iFormBuilderAPI.Page> pages, long pageid)
        {
            foreach (iFormBuilderAPI.Page page in pages)
            {
                if (page.ID == pageid)
                    return page;
            }

            return null;
        }

        private bool IsPageDownloaded(List<iFormBuilderAPI.Page> pages, long pageid)
        {
            return GetPage(pages, pageid) != null;
        }

        protected override void OnShutdown()
        {
            Uninitialize();

            ArcMap.Events.NewDocument -= ArcMap_NewOpenDocument;
            ArcMap.Events.OpenDocument -= ArcMap_NewOpenDocument;

            m_map = null;
            s_dockWindow = null;
            s_extension = null;

            base.OnShutdown();
        }

        protected override bool OnSetState(ExtensionState state)
        {
            // Optionally check for a license here
            this.State = state;

            if (state == ExtensionState.Enabled)
                Initialize();
            else
                Uninitialize();

            return base.OnSetState(state);
        }

        protected override ExtensionState OnGetState()
        {
            return this.State;
        }


        // Privates
        private void Initialize()
        {
            // If the extension hasn't been started yet or it's been turned off, bail
            if (s_extension == null || this.State != ExtensionState.Enabled)
                return;

            // Reset event handlers
            IActiveViewEvents_Event avEvent = ArcMap.Document.FocusMap as IActiveViewEvents_Event;
            avEvent.ItemAdded += AvEvent_ItemAdded;
            avEvent.ItemDeleted += AvEvent_ItemAdded;
            avEvent.SelectionChanged += UpdateSelCountDockWin;
            avEvent.ContentsChanged += avEvent_ContentsChanged;


            // Update the UI
            m_map = ArcMap.Document.FocusMap;
        
            SelCountDockWin.SetEnabled(true);
            UpdateSelCountDockWin();
            m_hasSelectableLayer = CheckForSelectableLayer();

            //Intialize the Connection to iForms Data
            api = new iFormBuilderAPI.iFormBuilder();
            api.ReadConfiguration(Utilities.iFormConfigFile);
            FillComboBox();
        }

        private void Uninitialize()
        {
            if (s_extension == null)
                return;

            // Detach event handlers
            IActiveViewEvents_Event avEvent = m_map as IActiveViewEvents_Event;
            avEvent.ItemAdded -= AvEvent_ItemAdded;
            avEvent.ItemDeleted -= AvEvent_ItemAdded;
            avEvent.SelectionChanged -= UpdateSelCountDockWin;
            avEvent.ContentsChanged -= avEvent_ContentsChanged;
            avEvent = null;

            // Update UI
            SelectionTargetComboBox selCombo = SelectionTargetComboBox.GetSelectionComboBox();
            if (selCombo != null)
                selCombo.ClearAll();

            SelCountDockWin.SetEnabled(false);
        }

        private void UpdateSelCountDockWin()
        {
            // If the dockview hasn't been created yet
            if (!SelCountDockWin.Exists)
                return;

            // Update the contents of the listView, when the selection changes in the map. 
            IFeatureLayer featureLayer;
            IFeatureSelection featSel;

            SelCountDockWin.Clear();

            // Loop through the layers in the map and add the layer's name and
            // selection count to the list box
            for (int i = 0; i < m_map.LayerCount; i++)
            {
                if (m_map.get_Layer(i) is IFeatureSelection)
                {
                    featureLayer = m_map.get_Layer(i) as IFeatureLayer;
                    if (featureLayer == null)
                        break;

                    featSel = featureLayer as IFeatureSelection;

                    int count = 0;
                    if (featSel.SelectionSet != null)
                        count = featSel.SelectionSet.Count;
                    SelCountDockWin.AddItem(featureLayer.Name, count);
                }
            }
        }

        private void FillComboBox()
        {
            try
            {
                api.ReadConfiguration(Utilities.iFormConfigFile);
                List<iFormBuilderAPI.Page> getPages = api.GetAllPagesInProfile();
                if (getPages == null)
                    return;
                
                SelectionTargetComboBox selCombo = SelectionTargetComboBox.GetSelectionComboBox();
                if (selCombo == null)
                    return;

                selCombo.ClearAll();

                // Loop through the layers in the map and add the layer's name to the combo box.

                foreach (iFormBuilderAPI.Page page in getPages)
                {
                    selCombo.AddItem(page.NAME, page);
                }

                if(_users.Count == 0)
                    _users = api.GetAllUserInProfile();

                UserTargetComboBox userBox = UserTargetComboBox.GetSelectionComboBox();
                if (userBox == null)
                    return;

                userBox.ClearAll();
                foreach (iFormBuilderAPI.User user in _users)
                {
                    userBox.AddItem(user.DisplayName, user);
                }
            }
            catch
            {
                return;
            }
        }

        private bool CheckForSelectableLayer()
        {
            IMap map = ArcMap.Document.FocusMap;
            // Bail if map has no layers
            if (map.LayerCount == 0)
                return false;

            // Fetch all the feature layers in the focus map
            // and see if at least one is selectable
            UIDClass uid = new UIDClass();
            uid.Value = "{40A9E885-5533-11d0-98BE-00805F7CED21}";
            IEnumLayer enumLayers = map.get_Layers(uid, true);
            IFeatureLayer featureLayer = enumLayers.Next() as IFeatureLayer;
            while (featureLayer != null)
            {
                if (featureLayer.Selectable == true)
                    return true;
                featureLayer = enumLayers.Next() as IFeatureLayer;
            }
            return false;
        }

        // Event handlers

        private void ArcMap_NewOpenDocument()
        {
            IActiveViewEvents_Event pageLayoutEvent = ArcMap.Document.PageLayout as IActiveViewEvents_Event;
            pageLayoutEvent.FocusMapChanged += new IActiveViewEvents_FocusMapChangedEventHandler(AVEvents_FocusMapChanged);

            Initialize();
        }

        private void avEvent_ContentsChanged()
        {
            m_hasSelectableLayer = CheckForSelectableLayer();
        }

        private void AvEvent_ItemAdded(object Item)
        {
            m_map = ArcMap.Document.FocusMap;
            FillComboBox();
            UpdateSelCountDockWin();
            m_hasSelectableLayer = CheckForSelectableLayer();
        }

        private void AVEvents_FocusMapChanged()
        {
            Initialize();
        }

        // Statics

        //internal static IDockableWindow GetSelectionCountWindow()
        //{
        //    if (s_extension == null)
        //        GetExtension();

        //    // Only get/create the dockable window if they ask for it
        //    if (s_dockWindow == null)
        //    {
        //        // Use GetDockableWindow directly intead of FromID as we want the client IDockableWindow not the internal class
        //        UID dockWinID = new UIDClass();
        //        dockWinID.Value = ThisAddIn.IDs.SelCountDockWin;
        //        s_dockWindow = ArcMap.DockableWindowManager.GetDockableWindow(dockWinID);
        //        s_extension.UpdateSelCountDockWin();
        //    }

        //    return s_dockWindow;
        //}

        internal static IDockableWindow GetConfigWindow()
        {
            if (s_extension == null)
                GetExtension();

            // Only get/create the dockable window if they ask for it
            if (s_dockWindow == null)
            {
                // Use GetDockableWindow directly intead of FromID as we want the client IDockableWindow not the internal class
                UID dockWinID = new UIDClass();
                dockWinID.Value = ThisAddIn.IDs.iFormConfigurationWindow;
                s_dockWindow = ArcMap.DockableWindowManager.GetDockableWindow(dockWinID);
                //s_extension.UpdateSelCountDockWin();
            }

            return s_dockWindow;
        }

        internal static bool IsExtensionEnabled()
        {
            if (s_extension == null)
                GetExtension();

            if (s_extension == null)
                return false;

            return s_extension.State == ExtensionState.Enabled;
        }

        internal static bool HasSelectableLayer()
        {
            if (s_extension == null)
                GetExtension();

            if (s_extension == null)
                return false;

            return s_extension.m_hasSelectableLayer;
        }

        private static SelectionExtension GetExtension()
        {
            if (s_extension == null)
            {
                // Call FindExtension to load this just-in-time extension.
                UID id = new UIDClass();
                id.Value = ThisAddIn.IDs.SelectionExtension;
                ArcMap.Application.FindExtensionByCLSID(id);
            }

            return s_extension;
        }
    }
}
