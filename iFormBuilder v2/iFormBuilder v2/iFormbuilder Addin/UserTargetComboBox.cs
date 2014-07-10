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
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Desktop.AddIns;
namespace iFormToolbar
{
  public class UserTargetComboBox : ESRI.ArcGIS.Desktop.AddIns.ComboBox
  {
    private static UserTargetComboBox s_comboBox;
    private int m_selAllCookie;
    public object SelectedTable { get; set; }

    public UserTargetComboBox()
    {      
      m_selAllCookie = -1;
      s_comboBox = this;
    }
    
    internal static UserTargetComboBox GetSelectionComboBox()
    {
      return s_comboBox;
    }

    internal void AddItem(string itemName, iFormBuilderAPI.User user)
    {
      // Add each item to combo box.
      int cookie = s_comboBox.Add(itemName, user);
    }

    internal void ClearAll()
    {
      m_selAllCookie = -1;
      s_comboBox.Clear();
    }

    protected override void OnUpdate()
    {
      this.Enabled = SelectionExtension.IsExtensionEnabled();
    }

    protected override void OnSelChange(int cookie)
    {
      if (cookie == -1)
        return;

      foreach (ComboBox.Item item in this.items)
      {
          if (item.Cookie == cookie)
              SelectedTable = item.Tag;
      }
    }
  }

}
 