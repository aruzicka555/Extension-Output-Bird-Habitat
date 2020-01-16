using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Landis.Library.Metadata;
using Landis.Utilities;
using Landis.Core;
using System.IO;

namespace Landis.Extension.Output.BirdHabitat
{
    public static class MetadataHandler
    {

        
        public static ExtensionMetadata Extension {get; set;}

        public static void InitializeMetadata(int Timestep, string SpeciesMapFileName, IEnumerable<IModelDefinition> modelDefs, ICore mCore, string LogFileName, string SpeciesLogFileNames)
        {
            ScenarioReplicationMetadata scenRep = new ScenarioReplicationMetadata() {
                RasterOutCellArea = PlugIn.ModelCore.CellArea,
                TimeMin = PlugIn.ModelCore.StartTime,
                TimeMax = PlugIn.ModelCore.EndTime//,
                //ProjectionFilePath = "Projection.?" 
            };

            Extension = new ExtensionMetadata(mCore){
                Name = PlugIn.PlugInName,
                TimeInterval = Timestep, 
                ScenarioReplicationMetadata = scenRep
            };

            
            //---------------------------------------
            //          table outputs:   
            //---------------------------------------
            if (LogFileName != null)
            {
                System.IO.Directory.CreateDirectory(Path.GetDirectoryName(LogFileName));
                PlugIn.habitatLog = new MetadataTable<SpeciesHabitatLog>(LogFileName);
                OutputMetadata tblOut_events = new OutputMetadata()
                {
                    Type = OutputType.Table,
                    Name = "SppHabitatLog",
                    FilePath = PlugIn.habitatLog.FilePath,
                    Visualize = true,
                };
                tblOut_events.RetriveFields(typeof(SpeciesHabitatLog));
                Extension.OutputMetadatas.Add(tblOut_events);
            }
            if (SpeciesLogFileNames != null)
            {
                PlugIn.sppLogs = new MetadataTable<IndividualSpeciesHabitatLog>[50];
                int selectModelCount = 0;
                foreach (ModelDefinition sppModel in modelDefs)
                {
                    string sppLogPath = BirdHabitat.SpeciesLogFileNames.ReplaceTemplateVars(SpeciesLogFileNames, sppModel.Name);
                    System.IO.Directory.CreateDirectory(Path.GetDirectoryName(sppLogPath));
                    PlugIn.sppLogs[selectModelCount] = new MetadataTable<IndividualSpeciesHabitatLog>(sppLogPath);
                    selectModelCount++;

                    OutputMetadata tblOut_events = new OutputMetadata()
                    {
                        Type = OutputType.Table,
                        Name = ("SpeciesLog_" + sppModel.Name),
                        FilePath = sppLogPath,
                        Visualize = true,
                    };
                    tblOut_events.RetriveFields(typeof(IndividualSpeciesHabitatLog));
                    Extension.OutputMetadatas.Add(tblOut_events);
                }
            }

            //---------------------------------------            
            //          map outputs:         
            //---------------------------------------

            foreach (ModelDefinition sppModel in modelDefs)
            {
                
                string sppMapPath = SpeciesMapFileNames.ReplaceTemplateVars(SpeciesMapFileName, sppModel.Name);
                
                OutputMetadata mapOut_Birds = new OutputMetadata()
                {
                    Type = OutputType.Map,
                    Name = ("Species Habitat Map: " + sppModel.Name),
                    //sppModel.Name,
                    FilePath = @sppMapPath,
                    Map_DataType = MapDataType.Continuous,
                    Map_Unit = "Index of Abundance",
                    Visualize = true,
                };
                Extension.OutputMetadatas.Add(mapOut_Birds);
            }
            //---------------------------------------
            MetadataProvider mp = new MetadataProvider(Extension);
            mp.WriteMetadataToXMLFile("Metadata", Extension.Name, Extension.Name);




        }
    }
}
