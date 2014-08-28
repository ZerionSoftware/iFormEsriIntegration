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
  public class TrueFalseToggleComboBox : ESRI.ArcGIS.Desktop.AddIns.ComboBox
  {
    private static TrueFalseToggleComboBox s_comboBox;
    private int m_selAllCookie;
    public bool SelectedDownloadMethod { get; set; }

    public TrueFalseToggleComboBox()
    {      
      m_selAllCookie = -1;
      s_comboBox = this;

        //Add the items to the tool bar
      s_comboBox.AddItem("True", true);
      s_comboBox.AddItem("False", false);
    }
    
    internal static TrueFalseToggleComboBox GetSelectionComboBox()
    {
      return s_comboBox;
    }

    internal void AddItem(string itemName, bool favor)
    {
      // Add each item to combo box.
      int cookie = s_comboBox.Add(itemName, favor);
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
              SelectedDownloadMethod = bool.Parse(item.Tag.ToString());
      }
    }
  }

}
 