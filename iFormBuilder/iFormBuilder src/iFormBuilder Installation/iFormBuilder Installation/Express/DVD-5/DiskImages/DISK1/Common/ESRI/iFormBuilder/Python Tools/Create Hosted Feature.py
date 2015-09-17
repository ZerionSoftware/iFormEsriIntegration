def createAGSConnection(servername,username,password):
    try:
        arcpy.mapping.CreateGISServerConnectionFile("PUBLISH_GIS_SERVICES",arcpy.env.scratchFolder,"temp.ags",serverURL,"ARCGIS_SERVER",use_arcgis_desktop_staging_folder=False,staging_folder_path="C:/temp",username=username,password=password)
    except Exception, e:
            print e.message   
    agsConnection = os.path.join(arcpy.env.scratchFolder, "temp.ags")
    return agsConnection

import arcpy, os
from arcpy import env
import time
import xml.dom.minidom as DOM

arcpy.env.overwriteOutput = True
arcpy.env.scratchWorkspace = "c:/temp"

# c:\LandUse\ForestCover\scratch
print arcpy.env.scratchFolder
   
#Input Values
#  This is the table that will be joined to the polygon to create the scoring
inputWorkspace= arcpy.GetParameterAsText(0)
if inputWorkspace == '#' or not inputWorkspace:
    inputWorkspace = r"C:\temp\130468969591464501.gdb"

inputlayername = "iFormBuilder_Layer"
workspace= arcpy.GetParameterAsText(1)
if workspace == '#' or not workspace:
    workspace = arcpy.env.scratchFolder

# Create a connection file to the server
servername = arcpy.GetParameterAsText(2)
if servername == '#' or not servername:
    servername = ""
    
serverport = arcpy.GetParameterAsText(3)
if servername == '#' or not servername:
    servername = ""

arcgisserver_un = arcpy.GetParameterAsText(4)
if arcgisserver_un == '#' or not arcgisserver_un:
    arcgisserver_un = ""
    
arcgisserver_pwd = arcpy.GetParameterAsText(5)
if arcgisserver_pwd == '#' or not arcgisserver_pwd:
    arcgisserver_pwd = ""

inFolder = arcpy.GetParameterAsText(6)
if inFolder == '#' or not inFolder:
    inFolder = ""

inServiceName = arcpy.GetParameterAsText(7)
if inServiceName == '#' or not inServiceName:
    inServiceName = ""
    
#Local Variables
inCluster = "default"
inFolderType = "FROM_SERVICE_DEFINITION"
inStartup = "STARTED"
inOverride = "OVERRIDE_DEFINITION"
#inMyContents = "SHARE_ONLINE"
#inPublic = "PRIVATE"
#inOrganization = "NO_SHARE_ORGANIZATION"
#inGroups = "My Group"

serverURL="http://"+servername+":"+str(serverport)+"/arcgis/admin"
inmxd = "c:\\temp\\Template.mxd"
arcpy.AddMessage(inmxd)

#Loop Through the features (and tables) in the Database and Add To Map
arcpy.env.workspace = inputWorkspace

# Local variables:
print arcpy.env.workspace
featureclasses = arcpy.ListFeatureClasses()

layeridx = 0
#need to find a way to create a new blank map
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

inServer = createAGSConnection(servername,arcgisserver_un,arcgisserver_pwd)
outmxd = arcpy.env.scratchFolder + "\\" + str(int(time.time())) + ".mxd"
arcpy.AddMessage(outmxd)
mxd.saveACopy(outmxd)
arcpy.AddMessage(outmxd)
mapDoc = arcpy.mapping.MapDocument(outmxd)

sddraft = arcpy.env.scratchFolder +"\\{}.sddraft".format(inServiceName)
arcpy.AddMessage("Create sddraft file: " +sddraft +"for server" + inServer)
arcpy.mapping.CreateMapSDDraft(mapDoc, sddraft, inServiceName ,"ARCGIS_SERVER",inServer , True, inFolder, "summary", "tags")
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

##configProps = doc.getElementsByTagName('Info')[0]
##propArray = configProps.firstChild
##propSets = propArray.childNodes
##for propSet in propSets:
##    keyValues = propSet.childNodes
##    for keyValue in keyValues:
##        if keyValue.tagName == 'Key':
##            if keyValue.firstChild.data == "WebCapabilities":
##                keyValue.nextSibling.firstChild.data = "Query,Create,Update,Delete,Uploads,Editing"
               
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
    print "Publishing to " + inServer
    arcpy.UploadServiceDefinition_server(sd, inServer,inServiceName)

    #send the URL back to the calling
    service_url="http://"+servername+"/arcgis/rest/services/" + inServiceName +"/" + inFolder +"/FeatureServer"
    arcpy.AddMessage(service_url)
    arcpy.SetParameter(8,service_url)
    print service_url
else: 
    # if the sddraft analysis contained errors, display them
    print analysis['errors']
    arcpy.AddMessage(analysis['errors'])
del mxd
