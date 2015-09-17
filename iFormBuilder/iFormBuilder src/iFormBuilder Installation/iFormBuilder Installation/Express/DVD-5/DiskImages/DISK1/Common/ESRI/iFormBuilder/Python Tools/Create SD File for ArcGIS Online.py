def createAGSConnection(servername,username,password,servicetype):
    try:
        arcpy.mapping.CreateGISServerConnectionFile("PUBLISH_GIS_SERVICES",arcpy.env.scratchFolder,"temp.ags",serverURL,servicetype,use_arcgis_desktop_staging_folder=False,staging_folder_path="C:/temp",username=username,password=password)
    except Exception, e:
            print e.message   
    agsConnection = os.path.join(arcpy.env.scratchFolder, "temp.ags")
    return agsConnection

import arcpy, os
from arcpy import env
import time
import xml.dom.minidom as DOM
import urllib2, urllib,re
import json
from arcrest.agol import admin

arcpy.env.overwriteOutput = True

workspace = arcpy.GetParameterAsText(0)
if workspace == '#' or not workspace:
    workspace = r"C:\Users\TRAVIS~1\AppData\Local\Temp\arcBFD6\130501019124545107.gdb"

inServiceName = arcpy.GetParameterAsText(1)
if inServiceName == '#' or not inServiceName:
    inServiceName = "my_test"

servericetype = "MY_HOSTED_SERVICES"
#Local Variables
inCluster = "default"
inFolderType = "FROM_SERVICE_DEFINITION"
inStartup = "STARTED"
inOverride = "OVERRIDE_DEFINITION"
#inMyContents = "SHARE_ONLINE"
#inPublic = "PRIVATE"
#inOrganization = "NO_SHARE_ORGANIZATION"
#inGroups = "My Group"
inputlayername = "iFormBuilder_Layer"

#Loop Through the features (and tables) in the Database and Add To Map
arcpy.env.workspace = workspace

# Local variables:
print arcpy.env.workspace
featureclasses = arcpy.ListFeatureClasses()
layeridx = 0

inmxd = "c:\\temp\\Template.mxd" #TODO Fix this location
arcpy.AddMessage(inmxd)
mxd = arcpy.mapping.MapDocument(inmxd)

for fc in featureclasses:
    arcpy.AddMessage("Add Feature Class: " + str(layeridx))
    iFormBuilderTest_SaveToLayer_lyr = arcpy.env.scratchFolder +"\\iFormBuilder_SaveToLayer" + str(layeridx) +".lyr"
    
    # Process: Make Feature Layer
    inputlayername = str(fc) + "_lyr"
    arcpy.MakeFeatureLayer_management(fc, inputlayername  , "", "", "#")
    
    # Process: Save To Layer File
    arcpy.SaveToLayerFile_management(inputlayername, iFormBuilderTest_SaveToLayer_lyr, "", "CURRENT")
    
    df = arcpy.mapping.ListDataFrames(mxd)[0]
    addLayer = arcpy.mapping.Layer(iFormBuilderTest_SaveToLayer_lyr)
    arcpy.mapping.AddLayer(df, addLayer, "BOTTOM")
    layeridx = layeridx + 1
    
#Add any tables in the Geodatabase
tablelist = arcpy.ListTables()
for table in tablelist:
    if table == 'SynchronizationTable' or table == "TableInformation" or table == "DomainList" or '_ATTACH' in table:
        continue
    else:
        arcpy.AddMessage(table)
        tableview = arcpy.mapping.TableView(arcpy.env.workspace +"\\"+ table)
        arcpy.mapping.AddTableView(df, tableview)

mxd.description = "description"
mxd.summary = "summary"
mxd.tags = "tags"

outmxd = "C:\\temp\\" + str(int(time.time())) + ".mxd"
mxd.saveACopy(outmxd)
arcpy.AddMessage(outmxd)
agol = admin.AGOL(username="travisbutcher_npo",password="july1974")
agol.createFeatureService(outmxd,inServiceName,"True","True","",None,None)
arcpy.AddMessage("Check ArcGIS Online for the Service")
exit

