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
    public enum FavorRankings
    {
        LocationWidget,
        CreatedLocation,
        ModifiedLocation
    }

  public class LocationFavoringComboBox : ESRI.ArcGIS.Desktop.AddIns.ComboBox
  {
    private static LocationFavoringComboBox s_comboBox;
    private int m_selAllCookie;
    public int SelectedRanking{ get; set; }

    public LocationFavoringComboBox()
    {      
      m_selAllCookie = -1;
      s_comboBox = this;

    }
    
    internal static LocationFavoringComboBox GetSelectionComboBox()
    {
      return s_comboBox;
    }

    internal void AddItem(string itemName, FavorRankings rank)
    {
       //Add each item to combo box.
      int cookie = s_comboBox.Add(itemName, rank);
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
              SelectedRanking = int.Parse(item.Tag.ToString());
      }
    }
  }
}

 