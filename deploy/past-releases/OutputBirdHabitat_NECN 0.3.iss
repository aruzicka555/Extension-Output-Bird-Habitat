#define PackageName      "Output Bird Habitat NECN"
#define PackageNameLong  "Output Bird Habitat NECN Extension"
#define Version          "0.3"
#define ReleaseNumber    "b22"
#define ReleaseType      "official"
#define CoreVersion      "6.1"
#define CoreReleaseAbbr  ""


#define ExtDir "C:\Program Files\LANDIS-II\v6\bin\extensions"
#define AppDir "C:\Program Files\LANDIS-II\v6\"
#define LandisPlugInDir "C:\Program Files\LANDIS-II\plug-ins"

#include "package (Setup section) v6.0.iss"

[Files]
; This .dll IS the extension (ie, the extension's assembly)
; NB: Do not put an additional version number in the file name of this .dll
; (The name of this .dll is defined in the extension's \src\*.csproj file)
Source: ..\..\src\bin\Debug\Landis.Extension.Output.BirdHabitatNECN.dll; DestDir: {#ExtDir}; Flags: replacesameversion 

; Requisite auxiliary libraries
; NB. These libraries are used by other extensions and thus are never uninstalled.
Source: ..\..\src\bin\Debug\Landis.Library.Metadata.dll; DestDir: {#ExtDir}; Flags: replacesameversion uninsneveruninstall
Source: ..\..\src\bin\Debug\Landis.Library.AgeOnlyCohorts.dll; DestDir: {#ExtDir}; Flags: replacesameversion uninsneveruninstall
Source: ..\..\src\bin\Debug\Landis.Library.LeafBiomassCohorts.dll; DestDir: {#ExtDir}; Flags: replacesameversion uninsneveruninstall
Source: ..\..\src\bin\Debug\Landis.Library.Climate.dll; DestDir: {#ExtDir}; Flags: replacesameversion uninsneveruninstall
Source: ..\..\src\bin\Debug\Landis.Library.Cohorts.dll; DestDir: {#ExtDir}; Flags: replacesameversion uninsneveruninstall
Source: ..\..\src\bin\Debug\Landis.SpatialModeling.dll; DestDir: {#ExtDir}; Flags: replacesameversion uninsneveruninstall


; User Guides are no longer shipped with installer


; Complete example for testing the extension
Source: ..\examples\NECN\*.txt; DestDir: {#AppDir}\examples\Output Bird Habitat NECN
Source: ..\examples\NECN\*.gis; DestDir: {#AppDir}\examples\Output Bird Habitat NECN
Source: ..\examples\NECN\*.bat; DestDir: {#AppDir}\examples\Output Bird Habitat NECN
Source: ..\examples\NECN\*.csv; DestDir: {#AppDir}\examples\Output Bird Habitat NECN
Source: ..\examples\NECN\*.xlsx; DestDir: {#AppDir}\examples\Output Bird Habitat NECN


; LANDIS-II identifies the extension with the info in this .txt file
; NB. New releases must modify the name of this file and the info in it
#define InfoTxt "OutputBirdHabitat_NECN 0.3.txt"
Source: {#InfoTxt}; DestDir: {#LandisPlugInDir}

[Run]
;; Run plug-in admin tool to add the entry for the plug-in
#define PlugInAdminTool  CoreBinDir + "\Landis.PlugIns.Admin.exe"

Filename: {#PlugInAdminTool}; Parameters: "remove ""Output Bird Habitat NECN"" "; WorkingDir: {#LandisPlugInDir}
Filename: {#PlugInAdminTool}; Parameters: "add ""{#InfoTxt}"" "; WorkingDir: {#LandisPlugInDir}

[Code]
{ Check for other prerequisites during the setup initialization }
#include "package (Code section) v3.iss"

//-----------------------------------------------------------------------------

function InitializeSetup_FirstPhase(): Boolean;
begin
  Result := True
end;


