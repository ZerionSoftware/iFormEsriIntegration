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

namespace iFormToolbar
{
  public class ZoomToLayerMultiItem : ESRI.ArcGIS.Desktop.AddIns.MultiItem
  {
    protected override void OnClick(Item item)
    {
      ESRI.ArcGIS.Carto.ILayer layer = item.Tag as ESRI.ArcGIS.Carto.ILayer;
      ESRI.ArcGIS.Geometry.IEnvelope env = layer.AreaOfInterest;
      ArcMap.Document.ActiveView.Extent = env;
      ArcMap.Document.ActiveView.Refresh();
    }

    protected override void OnPopup(ItemCollection items)
    {
      ESRI.ArcGIS.Carto.IMap map = ArcMap.Document.FocusMap;
      for (int i = 0; i < map.LayerCount; i++)
      {
        ESRI.ArcGIS.Carto.ILayer layer = map.get_Layer(i);
        Item item = new Item();
        item.Caption = layer.Name;
        item.Enabled = layer.Visible;
        item.Message = layer.Name;
        item.Tag = layer;
        items.Add(item);
      }
    }
  }
}
