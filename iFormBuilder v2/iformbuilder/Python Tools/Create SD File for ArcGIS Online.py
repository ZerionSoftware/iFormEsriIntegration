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

arcpy.env.overwriteOutput = True
arcpy.env.scratchWorkspace = "c:/temp"

# c:\LandUse\ForestCover\scratch
print arcpy.env.scratchFolder
   
#Input Values
#  This is the table that will be joined to the polygon to create the scoring
inputWorkspace = arcpy.env.scratchWorkspace

inputlayername = "iFormBuilder_Layer"
workspace= arcpy.GetParameterAsText(0)
if workspace == '#' or not workspace:
    workspace = arcpy.env.scratchFolder

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

#Loop Through the features (and tables) in the Database and Add To Map
arcpy.env.workspace = inputWorkspace

# Local variables:
print arcpy.env.workspace
featureclasses = arcpy.ListFeatureClasses()

layeridx = 0
#need to find a way to create a new blank map

inmxd = "c:\\temp\\Template.mxd"
arcpy.AddMessage(inmxd)
mxd = arcpy.mapping.MapDocument(inmxd)

for fc in featureclasses:
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
    if not table == 'SynchronizationTable':
        if not '_ATTACH' in table:
            arcpy.AddMessage(table)
            tableview = arcpy.mapping.TableView(arcpy.env.workspace +"\\"+ table)
            arcpy.mapping.AddTableView(df, tableview)

mxd.description = "description"
mxd.summary = "summary"
mxd.tags = "tags"

outmxd = arcpy.env.scratchFolder + "\\" + str(int(time.time())) + ".mxd"
arcpy.AddMessage(outmxd)
mxd.saveACopy(outmxd)
arcpy.AddMessage(outmxd)
mapDoc = arcpy.mapping.MapDocument(outmxd)
sddraft = arcpy.env.scratchFolder +"\\{}.sddraft".format(inServiceName)

if servericetype == "ARCGIS_SERVER":
    inServer = createAGSConnection(servername,arcgisserver_un,arcgisserver_pwd,servericetype)
    arcpy.AddMessage("Create sddraft file: " +sddraft +"for server" + inServer)
    arcpy.mapping.CreateMapSDDraft(mapDoc, sddraft, inServiceName ,servericetype,inServer , True, inFolder, "summary", "tags")
else:
    arcpy.AddMessage("Creating SD File for ArcGIS Online")
    arcpy.mapping.CreateMapSDDraft(mapDoc, sddraft, inServiceName ,servericetype,None, True, None, "summary", "iFormBuilder")

sd = arcpy.env.scratchFolder +"\\{}.sd".format(inServiceName)

arcpy.AddMessage("Edit sddraft file")
doc = DOM.parse(sddraft)
# Find all elements named TypeName. This is where the server object extension (SOE) names are defined.              
typeNames = doc.getElementsByTagName('SVCExtension')
for typeName in typeNames:
    for child in typeName.childNodes:
        if child.tagName == "Enabled" or child.tagName == "TypeName":
            if child.firstChild.data == "FeatureServer":
                for child2 in typeName.childNodes:
                    if child2.tagName == "Enabled":
                        arcpy.AddMessage('Enabling Feature Service')
                        child2.firstChild.data = "true"
          
replaceservice = "true"
if replaceservice == "true":
    newType = 'esriServiceDefinitionType_Replacement'
    newState = 'esriSDState_Published'
    myTagsType = doc.getElementsByTagName('Type')
    for myTagType in myTagsType:
        if myTagType.parentNode.tagName == 'SVCManifest':
            if myTagType.hasChildNodes():
                myTagType.firstChild.data = newType
    
    myTagsState = doc.getElementsByTagName('State')
    for myTagState in myTagsState:
        if myTagState.parentNode.tagName == 'SVCManifest':
            if myTagState.hasChildNodes():
                myTagState.firstChild.data = newState
                              
# Output to a new sddraft.
outXml = arcpy.env.scratchFolder +'\\{}_Web.sddraft'.format(inServiceName)
f = open(outXml, 'w')     
doc.writexml( f )     
f.close()      

# create service definition draft
arcpy.AddMessage("Analyzing Map Service for Publishing")

# analyze sddraft for errors
analysis = arcpy.mapping.AnalyzeForSD(outXml)

# stage and upload the service if the sddraft analysis did not contain errors
if analysis['errors'] == {}:
    ## create service definition
    arcpy.AddMessage("Create Service Definition File")
    env.workspace = "C:/temp"
    arcpy.StageService_server(outXml, sd)
    
    #arcpy.AddMessage("Successfully published service.")
    arcpy.AddMessage("Creating SD File at :" + sd)
    if servericetype == "ARCGIS_SERVER":
	print "not implmented"
    else:
	arcpy.SetParameter(9,sd)
else: 
    # if the sddraft analysis contained errors, display them
    print analysis['errors']
    arcpy.AddMessage(analysis['errors'])
del mxd
