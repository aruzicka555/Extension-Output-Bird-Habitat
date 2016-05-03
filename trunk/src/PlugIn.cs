//  Copyright 2005-2010 Portland State University, University of Wisconsin-Madison
//  Authors:  Robert M. Scheller, Jimm Domingo

using Landis.Core;
using Landis.Library.LeafBiomassCohorts;
using Landis.SpatialModeling;
using Landis.Library.Metadata;
using System.Collections.Generic;
using System;
using System.Text.RegularExpressions;
using Landis.Library.Climate;
using System.Linq;
using System.Data;
using System.IO;


namespace Landis.Extension.Output.BirdHabitat
{
    public class PlugIn
        : ExtensionMain
    {

        public static readonly ExtensionType extType = new ExtensionType("output");
        public static readonly string PlugInName = "Output Bird Habitat";
        public static MetadataTable<SpeciesHabitatLog> habitatLog;
        public static MetadataTable<IndividualSpeciesHabitatLog>[] sppLogs;
        //private StreamWriter habitatLog;

        private string localVarMapNameTemplate;
        private string speciesMapNameTemplate;

        private IEnumerable<IMapDefinition> mapDefs;
        private IEnumerable<IVariableDefinition> varDefs;
        private IEnumerable<INeighborVariableDefinition> neighborVarDefs;
        private IEnumerable<IClimateVariableDefinition> climateVarDefs;
        private IEnumerable<IDistanceVariableDefinition> distanceVarDefs;
        private IEnumerable<IModelDefinition> modelDefs;

        private static IInputParameters parameters;
        private static ICore modelCore;


        //---------------------------------------------------------------------

        public PlugIn()
            : base(PlugInName, extType)
        {
        }

        //---------------------------------------------------------------------

        public static ICore ModelCore
        {
            get
            {
                return modelCore;
            }
        }

        //---------------------------------------------------------------------

        public override void LoadParameters(string dataFile, ICore mCore)
        {
            modelCore = mCore;
            InputParametersParser.SpeciesDataset = modelCore.Species;
            InputParametersParser parser = new InputParametersParser();
            parameters = Landis.Data.Load<IInputParameters>(dataFile, parser);
            foreach(ClimateVariableDefinition climateVarDfn in parameters.ClimateVars)
            {
                if(climateVarDfn.SourceName != "Library")
                {
                    DataTable weatherTable = ClimateVariableDefinition.ReadWeatherFile(climateVarDfn.SourceName);
                    parameters.ClimateDataTable = weatherTable;
                }
            }

        }

        //---------------------------------------------------------------------

        /// <summary>
        /// Initializes the component with a data file.
        /// </summary>
        public override void Initialize()
        {

            Timestep = parameters.Timestep;
            SiteVars.Initialize();
            this.localVarMapNameTemplate = parameters.LocalVarMapFileNames;
            this.speciesMapNameTemplate = parameters.SpeciesMapFileNames;
            this.mapDefs = parameters.ReclassMaps;
            this.varDefs = parameters.DerivedVars;
            this.neighborVarDefs = parameters.NeighborVars;
            this.climateVarDefs = parameters.ClimateVars;
            this.distanceVarDefs = parameters.DistanceVars;
            this.modelDefs = parameters.Models;
            if (parameters.SpeciesMapFileNames != null)
                MetadataHandler.InitializeMetadata(parameters.Timestep, parameters.SpeciesMapFileNames, parameters.Models, ModelCore, parameters.LogFileName, parameters.SpeciesLogFileNames);

            //ModelCore.UI.WriteLine("   Opening species habitat log files \"{0}\" ...", parameters.LogFileName);
            //habitatLog = Landis.Data.CreateTextFile(parameters.LogFileName);
            //habitatLog.AutoFlush = true;
            //habitatLog.Write("Time, Species, Avg_Landscape");
            //foreach (IEcoregion ecoregion in PlugIn.ModelCore.Ecoregions)
            //{
            //    habitatLog.Write(", Avg_{0}", ecoregion.Name);
            //}
            //habitatLog.WriteLine("");
            
        }

        //---------------------------------------------------------------------

        /// <summary>
        /// Runs the component for a particular timestep.
        /// </summary>
        /// <param name="currentTime">
        /// The current model timestep.
        /// </param>
        public override void Run()
        {

            // Calculate Local Variables
            foreach (IMapDefinition map in mapDefs)
            {
                List<IForestType> forestTypes = map.ForestTypes;

                foreach (Site site in modelCore.Landscape.AllSites)
                {
                    int mapCode = 0;
                    if (site.IsActive)
                        mapCode = CalcForestType(forestTypes, site);
                    else
                        mapCode = 0;
                    SiteVars.LocalVars[site][map.Name] = mapCode;
                }
            }

            // Calculate Derived Variables
            foreach (IVariableDefinition var in varDefs)
            {
                foreach (Site site in modelCore.Landscape.AllSites)
                {
                    SiteVars.DerivedVars[site][var.Name] = 0;
                }
                List<string> variables = var.Variables;
                List<string> operators = var.Operators;

                // Parse variable name into mapDef and fortype
                for (int i = 0; i < variables.Count; i++)
                {
                    string fullVar = variables[i];
                    // string[] varSplit = Regex.Split(fullVar, "\\[.*?\\]");
                    string[] varSplit = fullVar.Split(new char[] { '[', ']' }, StringSplitOptions.RemoveEmptyEntries);
                    string mapName = varSplit[0];
                    string varName = varSplit[1];
                    int mapCode = 0;

                    foreach (IMapDefinition map in mapDefs)
                    {
                        if (map.Name == mapName)
                        {
                            int forTypeCnt = 1;
                            foreach (IForestType forestType in map.ForestTypes)
                            {
                                if (forestType.Name == varName)
                                {
                                    mapCode = forTypeCnt;
                                }
                                forTypeCnt++;
                            }
                        }
                    }
                    foreach (Site site in modelCore.Landscape.AllSites)
                    {
                        if (SiteVars.LocalVars[site][mapName] == mapCode)
                        {
                            SiteVars.DerivedVars[site][var.Name] = 1;
                        }
                    }
                }

            }

            // Calculate Neighborhood Variables
            foreach (INeighborVariableDefinition neighborVar in neighborVarDefs)
            {
                //Parse LocalVar
                string fullVar = neighborVar.LocalVariable;
                //string[] varSplit = Regex.Split(fullVar, "[]");
                string[] varSplit = fullVar.Split(new char[] { '[', ']' }, StringSplitOptions.RemoveEmptyEntries);
                string varName = "";
                int mapCode = 0;
                string mapName = "";
                if (varSplit.Length > 1)
                {
                    mapName = varSplit[0];
                    varName = varSplit[1];

                    foreach (IMapDefinition map in mapDefs)
                    {
                        if (map.Name == mapName)
                        {
                            int forTypeCnt = 1;
                            foreach (IForestType forestType in map.ForestTypes)
                            {
                                if (forestType.Name == varName)
                                {
                                    mapCode = forTypeCnt;
                                }
                                forTypeCnt++;
                            }
                        }
                    }
                }
                else
                {
                    varName = fullVar;
                }

                // Calculate neighborhood 
                double CellLength = PlugIn.ModelCore.CellLength;
                PlugIn.ModelCore.UI.WriteLine("Creating Dispersal Neighborhood List.");

                List<RelativeLocation> neighborhood = new List<RelativeLocation>();
                int neighborRadius = neighborVar.NeighborRadius;
                int numCellRadius = (int)(neighborRadius / CellLength);
                PlugIn.ModelCore.UI.WriteLine("NeighborRadius={0}, CellLength={1}, numCellRadius={2}",
                        neighborRadius, CellLength, numCellRadius);
                double centroidDistance = 0;
                double cellLength = CellLength;

                for (int row = (numCellRadius * -1); row <= numCellRadius; row++)
                {
                    for (int col = (numCellRadius * -1); col <= numCellRadius; col++)
                    {
                        centroidDistance = DistanceFromCenter(row, col);

                        //PlugIn.ModelCore.Log.WriteLine("Centroid Distance = {0}.", centroidDistance);
                        if (centroidDistance <= neighborRadius)
                        {
                            neighborhood.Add(new RelativeLocation(row, col));
                        }
                    }
                }
                // Calculate neighborhood value (% area of forest types)
                foreach (Site site in modelCore.Landscape.AllSites)
                {
                    int totalNeighborCells = 0;
                    int targetNeighborCells = 0;
                    foreach (RelativeLocation relativeLoc in neighborhood)
                    {
                        Site neighbor = site.GetNeighbor(relativeLoc);
                        if (neighbor != null && neighbor.IsActive)
                        {
                            if (mapName == "")
                            {
                                if (SiteVars.DerivedVars[neighbor][varName] > 0)
                                    targetNeighborCells++;
                            }
                            else if (mapName.Equals("ecoregion", StringComparison.OrdinalIgnoreCase))
                            {
                                if(varName.Equals(PlugIn.ModelCore.Ecoregion[neighbor].Name, StringComparison.OrdinalIgnoreCase))
                                {
                                    targetNeighborCells++;
                                }
                            }
                            else if (SiteVars.LocalVars[neighbor][mapName] == mapCode)
                            {
                                targetNeighborCells++;
                            }
                            totalNeighborCells++;
                        }
                    }
                    double pctValue = 100.0*(double)targetNeighborCells / (double)totalNeighborCells;

                    // Calculate transformation
                    double transformValue = pctValue;
                    if (neighborVar.Transform.Equals("log10", StringComparison.OrdinalIgnoreCase))
                    //if (neighborVar.Transform == "log10")
                    {

                        transformValue = Math.Log10(pctValue + 1);
                    }
                    else if (neighborVar.Transform.Equals("ln", StringComparison.OrdinalIgnoreCase))
                    //else if (neighborVar.Transform == "ln")
                    {
                        transformValue = Math.Log(pctValue + 1);
                    }

                    // Write Site Variable
                    SiteVars.NeighborVars[site][neighborVar.Name] = (float)transformValue;
                }

            }

            // Calculate Climate Variables

            foreach (IClimateVariableDefinition climateVar in climateVarDefs)
            {
                Dictionary<IEcoregion, Dictionary<string, double>> ecoClimateVars = new Dictionary<IEcoregion, Dictionary<string, double>>();
                                
                string varName = climateVar.Name;
                string climateLibVar = climateVar.ClimateLibVariable;
                string climateYear = climateVar.Year;
                int minMonth = climateVar.MinMonth;
                int maxMonth = climateVar.MaxMonth;
                string transform = climateVar.Transform;

                int currentYear = PlugIn.ModelCore.CurrentTime;
                int actualYear = currentYear;

                int firstActiveEco = 0;
                foreach (IEcoregion ecoregion in modelCore.Ecoregions)
                {
                    if (ecoregion.Active)
                    {
                        firstActiveEco = ecoregion.Index;
                        break;
                    }
                }
                if (Climate.Future_MonthlyData != null)
                {
                    AnnualClimate_Monthly AnnualWeather = Climate.Future_MonthlyData[Climate.Future_MonthlyData.Keys.Min()][firstActiveEco];
                    int maxSpinUpYear = Climate.Spinup_MonthlyData.Keys.Max();

                    if (PlugIn.ModelCore.CurrentTime > 0)
                    {
                        currentYear = (PlugIn.ModelCore.CurrentTime - 1) + Climate.Future_MonthlyData.Keys.Min();
                        if (climateYear.Equals("prev", StringComparison.OrdinalIgnoreCase))
                        {
                            if (Climate.Future_MonthlyData.ContainsKey(currentYear - 1))
                            {
                                AnnualWeather = Climate.Future_MonthlyData[currentYear - 1][firstActiveEco];
                            }
                            else
                            {
                                AnnualWeather = Climate.Spinup_MonthlyData[maxSpinUpYear][firstActiveEco];
                            }
                        }
                        else if (climateYear.Equals("current", StringComparison.OrdinalIgnoreCase))
                        {
                            AnnualWeather = Climate.Future_MonthlyData[currentYear][firstActiveEco];
                        }
                        else
                        {
                            string mesg = string.Format("Year for climate variable {0} is {1}; expected 'current' or 'prev'.", climateVar.Name, climateVar.Year);
                            throw new System.ApplicationException(mesg);
                        }
                    }
                    if (PlugIn.ModelCore.CurrentTime == 0)
                    {
                        if (climateYear.Equals("prev", StringComparison.OrdinalIgnoreCase))
                            AnnualWeather = Climate.Spinup_MonthlyData[maxSpinUpYear - 1][firstActiveEco];
                        else if (climateYear.Equals("current", StringComparison.OrdinalIgnoreCase))
                            AnnualWeather = Climate.Spinup_MonthlyData[maxSpinUpYear][firstActiveEco];
                        else
                        {
                            string mesg = string.Format("Year for climate variable {0} is {1}; expected 'current' or 'prev'.", climateVar.Name, climateVar.Year);
                            throw new System.ApplicationException(mesg);
                        }
                    }
                    actualYear = AnnualWeather.Year;
                }
                else
                {
                    if (climateYear.Equals("prev", StringComparison.OrdinalIgnoreCase))
                        actualYear = currentYear - 1;
                }

                if (climateVar.SourceName.Equals("library", StringComparison.OrdinalIgnoreCase))
                {
                    foreach (IEcoregion ecoregion in modelCore.Ecoregions)
                    {
                        if (ecoregion.Active)
                        {
                            if (!ecoClimateVars.ContainsKey(ecoregion))
                            {
                                ecoClimateVars.Add(ecoregion, new Dictionary<string, double>());
                            }
                            AnnualClimate_Monthly AnnualWeather = Climate.Future_MonthlyData[Climate.Future_MonthlyData.Keys.Min()][ecoregion.Index];
                            int maxSpinUpYear = Climate.Spinup_MonthlyData.Keys.Max();

                            if (PlugIn.ModelCore.CurrentTime == 0)
                            {
                                if (climateYear.Equals("prev", StringComparison.OrdinalIgnoreCase))
                                    AnnualWeather = Climate.Spinup_MonthlyData[maxSpinUpYear - 1][ecoregion.Index];
                                else
                                    AnnualWeather = Climate.Spinup_MonthlyData[maxSpinUpYear][ecoregion.Index];
                            }
                            else if (climateYear.Equals("prev", StringComparison.OrdinalIgnoreCase))
                            {
                                if (!Climate.Future_MonthlyData.ContainsKey(currentYear - 1))
                                {
                                    AnnualWeather = Climate.Spinup_MonthlyData[maxSpinUpYear][ecoregion.Index];
                                }
                                else
                                    AnnualWeather = Climate.Future_MonthlyData[currentYear - 1][ecoregion.Index];
                            }
                            else
                            {
                                AnnualWeather = Climate.Future_MonthlyData[currentYear][ecoregion.Index];
                            }

                            double monthTotal = 0;
                            int monthCount = 0;
                            double varValue = 0;
                            var monthRange = Enumerable.Range(minMonth, (maxMonth - minMonth) + 1);
                            foreach (int monthIndex in monthRange)
                            {
                                //if (climateVar.ClimateLibVariable == "PDSI")
                                //{

                                //double monthPDSI = PDSI_Calculator.PDSI_Monthly[monthIndex-1];
                                //   varValue = monthPDSI;
                                //}
                                if (climateVar.ClimateLibVariable.Equals("precip", StringComparison.OrdinalIgnoreCase))
                                //if (climateVar.ClimateLibVariable == "Precip")
                                {
                                    double monthPrecip = AnnualWeather.MonthlyPrecip[monthIndex - 1];
                                    varValue = monthPrecip * 10.0; //Convert cm to mm
                                }
                                else if (climateVar.ClimateLibVariable.Equals("temp", StringComparison.OrdinalIgnoreCase))
                                //else if (climateVar.ClimateLibVariable == "Temp")
                                {
                                    double monthTemp = AnnualWeather.MonthlyTemp[monthIndex - 1];
                                    varValue = monthTemp;
                                }
                                else
                                {
                                    string mesg = string.Format("Climate variable {0} is {1}; expected 'precip' or 'temp'.", climateVar.Name, climateVar.ClimateLibVariable);
                                    throw new System.ApplicationException(mesg);
                                }
                                monthTotal += varValue;
                                monthCount++;
                            }
                            double avgValue = monthTotal / (double)monthCount;
                            double transformValue = avgValue;
                            if (transform.Equals("log10", StringComparison.OrdinalIgnoreCase))
                            //if (transform == "Log10")
                            {
                                transformValue = Math.Log10(avgValue + 1);
                            }
                            else if (transform.Equals("ln", StringComparison.OrdinalIgnoreCase))
                            //else if (transform == "ln")
                            {
                                transformValue = Math.Log(avgValue + 1);
                            }
                            if (!ecoClimateVars[ecoregion].ContainsKey(varName))
                            {
                                ecoClimateVars[ecoregion].Add(varName, 0.0);
                            }
                            ecoClimateVars[ecoregion][varName] = transformValue;
                        }
                    }

                    foreach (Site site in modelCore.Landscape.AllSites)
                    {
                        IEcoregion ecoregion = PlugIn.ModelCore.Ecoregion[site];
                        double climateValue = 0;
                        if (ecoregion != null)
                        {
                            climateValue = ecoClimateVars[ecoregion][varName];
                        }
                        // Write Site Variable
                        SiteVars.ClimateVars[site][varName] = (float)climateValue;
                    }
                }
                else
                {
                    double monthTotal = 0;
                    int monthCount = 0;
                    double varValue = 0;
                    var monthRange = Enumerable.Range(minMonth, (maxMonth - minMonth) + 1);
                    foreach (int monthIndex in monthRange)
                    {
                        string selectString = "Year = '" + actualYear + "' AND Month = '" + monthIndex + "'";
                        DataRow[] rows = parameters.ClimateDataTable.Select(selectString);
                        if (rows.Length == 0)
                        {
                            string mesg = string.Format("Climate data is empty. No record exists for variable {0} in year {1}.",climateVar.Name,actualYear);
                            if (actualYear == 0)
                            {
                                mesg = mesg + "  Note that if using the options Monthly_AverageAllYears or Daily_AverageAllYears you should provide average values for climate variables listed as Year 0.";
                            }
                            throw new System.ApplicationException(mesg);
                        }
                        foreach (DataRow row in rows)
                        {
                            varValue = Convert.ToDouble(row[climateVar.ClimateLibVariable]);
                        }
                        monthTotal += varValue;
                        monthCount++;
                    }
                    double avgValue = monthTotal / (double)monthCount;
                    double transformValue = avgValue;
                    if (transform.Equals("log10", StringComparison.OrdinalIgnoreCase))
                    //if (transform == "Log10")
                    {
                        transformValue = Math.Log10(avgValue + 1);
                    }
                    else if (transform.Equals("ln", StringComparison.OrdinalIgnoreCase))
                    //else if (transform == "ln")
                    {
                        transformValue = Math.Log(avgValue + 1);
                    }
                    foreach (Site site in modelCore.Landscape.AllSites)
                    {
                        SiteVars.ClimateVars[site][varName] = (float)transformValue;
                    }
                }
            }
            
            // Calculate Distance Variables
            foreach (IDistanceVariableDefinition distanceVar in distanceVarDefs)
            {
                //Parse LocalVar
                string fullVar = distanceVar.LocalVariable;
                string[] varSplit = fullVar.Split(new char[] { '[', ']' }, StringSplitOptions.RemoveEmptyEntries);
                string varName = "";
                int mapCode = 0;
                string mapName = "";
                if (varSplit.Length > 1)
                {
                    mapName = varSplit[0];
                    varName = varSplit[1];

                    foreach (IMapDefinition map in mapDefs)
                    {
                        if (map.Name == mapName)
                        {
                            int forTypeCnt = 1;
                            foreach (IForestType forestType in map.ForestTypes)
                            {
                                if (forestType.Name == varName)
                                {
                                    mapCode = forTypeCnt;
                                }
                                forTypeCnt++;
                            }
                        }
                    }
                }
                else
                {
                    varName = fullVar;
                }

                // Calculate distance value (meters)

                double cellLength = PlugIn.ModelCore.CellLength;
                foreach (Site site in modelCore.Landscape.AllSites)
                {
                    /*
                    int maxRows = Math.Max(PlugIn.ModelCore.Landscape.Rows - site.Location.Row,site.Location.Row-PlugIn.ModelCore.Landscape.Rows) ;
                    int maxCols = Math.Max(PlugIn.ModelCore.Landscape.Columns - site.Location.Column, site.Location.Column - PlugIn.ModelCore.Landscape.Columns);
                    double minDistance = double.MaxValue;

                    if (mapName == "")
                            {
                                if (SiteVars.DerivedVars[site][varName] > 0)
                                    minDistance = 0 ;
                            }
                            else if (mapName.Equals("ecoregion", StringComparison.OrdinalIgnoreCase))
                            {
                                if (varName.Equals(PlugIn.ModelCore.Ecoregion[site].Name, StringComparison.OrdinalIgnoreCase))
                                {
                                    minDistance = 0 ;
                                }
                            }
                            else if (SiteVars.LocalVars[site][mapName] == mapCode)
                            {
                                minDistance = 0 ;
                            }

                    double centroidDistance = 0;
                    for (int iteration = 1; (iteration <= maxRows || iteration <= maxCols); iteration++)
                    {
                        int row = iteration;
                        int col = 0;                        
                        centroidDistance = DistanceFromCenter(row, col);
                        if(centroidDistance > minDistance + cellLength)
                        {
                            break;
                        }
                        if (centroidDistance < minDistance)
                        {
                            RelativeLocation relativeloc = new RelativeLocation(row, col);
                            Site neighbor = site.GetNeighbor(relativeloc);
                            if (mapName == "")
                            {
                                if (SiteVars.DerivedVars[neighbor][varName] > 0)
                                    minDistance = centroidDistance;
                            }
                            else if (mapName.Equals("ecoregion", StringComparison.OrdinalIgnoreCase))
                            {
                                if (varName.Equals(PlugIn.ModelCore.Ecoregion[neighbor].Name, StringComparison.OrdinalIgnoreCase))
                                {
                                    minDistance = centroidDistance;
                                }
                            }
                            else if (SiteVars.LocalVars[neighbor][mapName] == mapCode)
                            {
                                minDistance = centroidDistance;
                            }
                        }
                        row = 0;
                        col = iteration;
                        centroidDistance = DistanceFromCenter(row, col);
                        if (centroidDistance > minDistance + cellLength)
                        {
                            break;
                        }
                        if (centroidDistance < minDistance)
                        {
                            RelativeLocation relativeloc = new RelativeLocation(row, col);
                            Site neighbor = site.GetNeighbor(relativeloc);
                            if (mapName == "")
                            {
                                if (SiteVars.DerivedVars[neighbor][varName] > 0)
                                    minDistance = centroidDistance;
                            }
                            else if (mapName.Equals("ecoregion", StringComparison.OrdinalIgnoreCase))
                            {
                                if (varName.Equals(PlugIn.ModelCore.Ecoregion[neighbor].Name, StringComparison.OrdinalIgnoreCase))
                                {
                                    minDistance = centroidDistance;
                                }
                            }
                            else if (SiteVars.LocalVars[neighbor][mapName] == mapCode)
                            {
                                minDistance = centroidDistance;
                            }
                        }
                        row = iteration * -1;
                        col = 0;
                        centroidDistance = DistanceFromCenter(row, col);
                        if (centroidDistance > minDistance + cellLength)
                        {
                            break;
                        }
                        if (centroidDistance < minDistance)
                        {
                            RelativeLocation relativeloc = new RelativeLocation(row, col);
                            Site neighbor = site.GetNeighbor(relativeloc);
                            if (mapName == "")
                            {
                                if (SiteVars.DerivedVars[neighbor][varName] > 0)
                                    minDistance = centroidDistance;
                            }
                            else if (mapName.Equals("ecoregion", StringComparison.OrdinalIgnoreCase))
                            {
                                if (varName.Equals(PlugIn.ModelCore.Ecoregion[neighbor].Name, StringComparison.OrdinalIgnoreCase))
                                {
                                    minDistance = centroidDistance;
                                }
                            }
                            else if (SiteVars.LocalVars[neighbor][mapName] == mapCode)
                            {
                                minDistance = centroidDistance;
                            }
                        }
                        row = 0;
                        col = iteration * -1;
                        centroidDistance = DistanceFromCenter(row, col);
                        if (centroidDistance > minDistance + cellLength)
                        {
                            break;
                        }
                        if (centroidDistance < minDistance)
                        {
                            RelativeLocation relativeloc = new RelativeLocation(row, col);
                            Site neighbor = site.GetNeighbor(relativeloc);
                            if (mapName == "")
                            {
                                if (SiteVars.DerivedVars[neighbor][varName] > 0)
                                    minDistance = centroidDistance;
                            }
                            else if (mapName.Equals("ecoregion", StringComparison.OrdinalIgnoreCase))
                            {
                                if (varName.Equals(PlugIn.ModelCore.Ecoregion[neighbor].Name, StringComparison.OrdinalIgnoreCase))
                                {
                                    minDistance = centroidDistance;
                                }
                            }
                            else if (SiteVars.LocalVars[neighbor][mapName] == mapCode)
                            {
                                minDistance = centroidDistance;
                            }
                        }
                    }
                    */
                    double minDistance = double.MaxValue;
                    // Spiral outward algorithm (http://stackoverflow.com/questions/3330181/algorithm-for-finding-nearest-object-on-2d-grid)
                    // Start coordinates                    
                    int xs = 0;
                    int ys = 0;
                    // Check point (xs, ys)
                    if (mapName == "")
                    {
                        if (SiteVars.DerivedVars[site][varName] > 0)
                            minDistance = 0;
                    }
                    else if (mapName.Equals("ecoregion", StringComparison.OrdinalIgnoreCase))
                    {
                        if (varName.Equals(PlugIn.ModelCore.Ecoregion[site].Name, StringComparison.OrdinalIgnoreCase))
                        {
                            minDistance = 0;
                        }
                    }
                    else if (SiteVars.LocalVars[site][mapName] == mapCode)
                    {
                        minDistance = 0;
                    }

                    int maxD1 = site.Location.Row + site.Location.Column;
                    int maxD2 = site.Location.Row + (PlugIn.ModelCore.Landscape.Columns - site.Location.Column);
                    int maxD3 = (PlugIn.ModelCore.Landscape.Rows - site.Location.Row) + site.Location.Column;
                    int maxD4 = (PlugIn.ModelCore.Landscape.Rows - site.Location.Row) + (PlugIn.ModelCore.Landscape.Columns - site.Location.Column);
                    int maxDistance = Math.Max(maxD1,maxD2);
                    maxDistance = Math.Max(maxDistance, maxD3);
                    maxDistance = Math.Max(maxDistance, maxD4);
                    
                    for (int d = 1; d < maxDistance; d++)
                    {
                        double minCentroidDistance = double.MaxValue;
                        for (int i = 0; i < d + 1; i++)
                        {
                            int x1 = xs - d + i;
                            int y1 = ys - i;
                            // Check point (x1, y1)
                            double centroidDistance = DistanceFromCenter(y1, x1);
                            if (centroidDistance < minCentroidDistance)
                                minCentroidDistance = centroidDistance;
                            if (centroidDistance >= minDistance + cellLength)
                            {
                                break;
                            }
                            if (centroidDistance < minDistance)
                            {
                                RelativeLocation relativeloc = new RelativeLocation(y1, x1);
                                Site neighbor = site.GetNeighbor(relativeloc);
                                int neighborRow = neighbor.Location.Row;
                                int neighborCol = neighbor.Location.Column;
                                if ((neighborRow > 0 && neighborRow <= PlugIn.ModelCore.Landscape.Rows) && ((neighborCol > 0 && neighborCol <= PlugIn.ModelCore.Landscape.Columns)))
                                {
                                    if (mapName == "")
                                    {
                                        if (SiteVars.DerivedVars[neighbor][varName] > 0)
                                            minDistance = centroidDistance;
                                    }
                                    else if (mapName.Equals("ecoregion", StringComparison.OrdinalIgnoreCase))
                                    {
                                        if (varName.Equals(PlugIn.ModelCore.Ecoregion[neighbor].Name, StringComparison.OrdinalIgnoreCase))
                                        {
                                            minDistance = centroidDistance;
                                        }
                                    }
                                    else if (SiteVars.LocalVars[neighbor][mapName] == mapCode)
                                    {
                                        minDistance = centroidDistance;
                                    }
                                }
                            }

                            int x2 = xs + d - i;
                            int y2 = ys + i;
                            // Check point (x2, y2)
                            centroidDistance = DistanceFromCenter(y2, x2);
                            if (centroidDistance < minCentroidDistance)
                                minCentroidDistance = centroidDistance;
                            if (centroidDistance >= minDistance + cellLength)
                            {
                                break;
                            }
                            if (centroidDistance < minDistance)
                            {
                                RelativeLocation relativeloc = new RelativeLocation(y2, x2);
                                Site neighbor = site.GetNeighbor(relativeloc);
                                int neighborRow = neighbor.Location.Row;
                                int neighborCol = neighbor.Location.Column;
                                if ((neighborRow > 0 && neighborRow <= PlugIn.ModelCore.Landscape.Rows) && ((neighborCol > 0 && neighborCol <= PlugIn.ModelCore.Landscape.Columns)))
                                {
                                    if (mapName == "")
                                    {
                                        if (SiteVars.DerivedVars[neighbor][varName] > 0)
                                            minDistance = centroidDistance;
                                    }
                                    else if (mapName.Equals("ecoregion", StringComparison.OrdinalIgnoreCase))
                                    {
                                        if (varName.Equals(PlugIn.ModelCore.Ecoregion[neighbor].Name, StringComparison.OrdinalIgnoreCase))
                                        {
                                            minDistance = centroidDistance;
                                        }
                                    }
                                    else if (SiteVars.LocalVars[neighbor][mapName] == mapCode)
                                    {
                                        minDistance = centroidDistance;
                                    }
                                }
                            }
                           
                        }


                        for (int i = 1; i < d; i++)
                        {
                            int x1 = xs - i;
                            int y1 = ys + d - i;

                            // Check point (x1, y1)
                            double centroidDistance = DistanceFromCenter(y1, x1);
                            if (centroidDistance < minCentroidDistance)
                                minCentroidDistance = centroidDistance;
                            if (centroidDistance >= minDistance + cellLength)
                            {
                                break;
                            }
                            if (centroidDistance < minDistance)
                            {
                                RelativeLocation relativeloc = new RelativeLocation(y1, x1);
                                Site neighbor = site.GetNeighbor(relativeloc);
                                int neighborRow = neighbor.Location.Row;
                                int neighborCol = neighbor.Location.Column;
                                if ((neighborRow > 0 && neighborRow <= PlugIn.ModelCore.Landscape.Rows) && ((neighborCol > 0 && neighborCol <= PlugIn.ModelCore.Landscape.Columns)))
                                {
                                    if (mapName == "")
                                    {
                                        if (SiteVars.DerivedVars[neighbor][varName] > 0)
                                            minDistance = centroidDistance;
                                    }
                                    else if (mapName.Equals("ecoregion", StringComparison.OrdinalIgnoreCase))
                                    {
                                        if (varName.Equals(PlugIn.ModelCore.Ecoregion[neighbor].Name, StringComparison.OrdinalIgnoreCase))
                                        {
                                            minDistance = centroidDistance;
                                        }
                                    }
                                    else if (SiteVars.LocalVars[neighbor][mapName] == mapCode)
                                    {
                                        minDistance = centroidDistance;
                                    }
                                }
                            }
                            int x2 = xs + d - i;
                            int y2 = ys - i;

                            // Check point (x2, y2)
                            centroidDistance = DistanceFromCenter(y2, x2);
                            if (centroidDistance < minCentroidDistance)
                                minCentroidDistance = centroidDistance;
                            if (centroidDistance >= minDistance + cellLength)
                            {
                                break;
                            }
                            if (centroidDistance < minDistance)
                            {
                                RelativeLocation relativeloc = new RelativeLocation(y2, x2);
                                Site neighbor = site.GetNeighbor(relativeloc);
                                int neighborRow = neighbor.Location.Row;
                                int neighborCol = neighbor.Location.Column;
                                if ((neighborRow > 0 && neighborRow <= PlugIn.ModelCore.Landscape.Rows) && ((neighborCol > 0 && neighborCol <= PlugIn.ModelCore.Landscape.Columns)))
                                {
                                    if (mapName == "")
                                    {
                                        if (SiteVars.DerivedVars[neighbor][varName] > 0)
                                            minDistance = centroidDistance;
                                    }
                                    else if (mapName.Equals("ecoregion", StringComparison.OrdinalIgnoreCase))
                                    {
                                        if (varName.Equals(PlugIn.ModelCore.Ecoregion[neighbor].Name, StringComparison.OrdinalIgnoreCase))
                                        {
                                            minDistance = centroidDistance;
                                        }
                                    }
                                    else if (SiteVars.LocalVars[neighbor][mapName] == mapCode)
                                    {
                                        minDistance = centroidDistance;
                                    }
                                }
                            }
                        }
                        if (minCentroidDistance > minDistance + cellLength)
                        {
                            break;
                        }
                    }
                    // Calculate transformation
                    double transformValue = minDistance;
                    if (distanceVar.Transform.Equals("log10", StringComparison.OrdinalIgnoreCase))
                    //if (neighborVar.Transform == "log10")
                    {
                        transformValue = Math.Log10(minDistance + 1);
                    }
                    else if (distanceVar.Transform.Equals("ln", StringComparison.OrdinalIgnoreCase))
                    //else if (neighborVar.Transform == "ln")
                    {
                        transformValue = Math.Log(minDistance + 1);
                    }

                    // Write Site Variable
                    SiteVars.DistanceVars[site][distanceVar.Name] = (float)transformValue;
                }
            }
            
            Dictionary<string, float>[] ecoregionAvgValues = new Dictionary<string,float>[ModelCore.Ecoregions.Count];
            Dictionary<string, float> landscapeAvgValues =  new Dictionary<string,float>();
             int[] activeSiteCount = new int[ModelCore.Ecoregions.Count];
             foreach (IEcoregion ecoregion in ModelCore.Ecoregions)
             {
                 ecoregionAvgValues[ecoregion.Index] = new Dictionary<string, float>();
                 activeSiteCount[ecoregion.Index] = 0;
             }
             foreach (ActiveSite site in ModelCore.Landscape)
             {
                 IEcoregion ecoregion = ModelCore.Ecoregion[site];
                 activeSiteCount[ecoregion.Index]++;
             }

            // Calculate Species Models
            foreach (IModelDefinition model in modelDefs)
            {
                float [] ecoregionSum = new float[ModelCore.Ecoregions.Count];
                float landscapeSum = 0;
                
                foreach (Site site in modelCore.Landscape.AllSites)
                {
                    IEcoregion ecoregion = ModelCore.Ecoregion[site];
                    double modelPredict = 0;
                    int paramIndex = 0;
                    foreach (string parameter in model.Parameters)
                    {
                        string paramType = model.ParamTypes[paramIndex];
                        double paramValue = model.Values[paramIndex];
                        if (paramType.Equals("int", StringComparison.OrdinalIgnoreCase))
                        {
                            modelPredict += paramValue;
                        }
                        else if (paramType.Equals("neighbor", StringComparison.OrdinalIgnoreCase))
                        {
                            if (SiteVars.NeighborVars[site].ContainsKey(parameter))
                            {
                                double modelValue = SiteVars.NeighborVars[site][parameter];
                                modelValue = modelValue * paramValue;
                                modelPredict += modelValue;
                            }
                            else
                            {
                                string mesg = string.Format("The Neighborhood parameter {0} is listed for model {1} but is not included under NeighborhoodVariables.", parameter, model.Name);
                                throw new System.ApplicationException(mesg);
                            }
                        }
                        else if (paramType.Equals("climate", StringComparison.OrdinalIgnoreCase))
                        {
                            if (SiteVars.ClimateVars[site].ContainsKey(parameter))
                            {
                                double modelValue = SiteVars.ClimateVars[site][parameter];
                                modelValue = modelValue * paramValue;
                                modelPredict += modelValue;
                            }
                            else
                            {
                                string mesg = string.Format("The Climate parameter {0} is listed for model {1} but is not included under ClimateVariables.", parameter, model.Name);
                                throw new System.ApplicationException(mesg);
                            }
                        }
                        else if (paramType.Equals("distance", StringComparison.OrdinalIgnoreCase))
                        {
                            if (SiteVars.DistanceVars[site].ContainsKey(parameter))
                            {
                                double modelValue = SiteVars.DistanceVars[site][parameter];
                                modelValue = modelValue * paramValue;
                                modelPredict += modelValue;
                            }
                            else
                            {
                                string mesg = string.Format("The Distance parameter {0} is listed for model {1} but is not included under DistanceVariables.", parameter, model.Name);
                                throw new System.ApplicationException(mesg);
                            }
                        }
                        else if (paramType.Equals("biomass", StringComparison.OrdinalIgnoreCase))
                        {
                            double modelValue = Util.ComputeBiomass(SiteVars.Cohorts[site]);
                            modelValue = modelValue * paramValue;
                            modelPredict += modelValue;
                        }
                        else if (paramType.Equals("lnbiomass", StringComparison.OrdinalIgnoreCase))
                        {
                            double modelValue = Math.Log(Util.ComputeBiomass(SiteVars.Cohorts[site])+1);
                            modelValue = modelValue * paramValue;
                            modelPredict += modelValue;
                        }
                        else if (paramType.Equals("logbiomass", StringComparison.OrdinalIgnoreCase))
                        {
                            double modelValue = Math.Log10(Util.ComputeBiomass(SiteVars.Cohorts[site]) + 1);
                            modelValue = modelValue * paramValue;
                            modelPredict += modelValue;
                        }
                        else if (paramType.Equals("age", StringComparison.OrdinalIgnoreCase))
                        {
                            double modelValue = Util.ComputeAge(SiteVars.Cohorts[site]);
                            modelValue = modelValue * paramValue;
                            modelPredict += modelValue;
                        }
                        else if (paramType.Equals("lnage", StringComparison.OrdinalIgnoreCase))
                        {
                            double modelValue = Math.Log(Util.ComputeAge(SiteVars.Cohorts[site]) + 1);
                            modelValue = modelValue * paramValue;
                            modelPredict += modelValue;
                        }
                        else if (paramType.Equals("logage", StringComparison.OrdinalIgnoreCase))
                        {
                            double modelValue = Math.Log10(Util.ComputeAge(SiteVars.Cohorts[site]) + 1);
                            modelValue = modelValue * paramValue;
                            modelPredict += modelValue;
                        }
                        else if(paramType.Contains("*"))
                        {
                            string[] paramTypes = paramType.Split('*');
                            string[] splitParameters = parameter.Split('*');
                            double paramProduct = 1.0;
                            if (paramTypes.Length != splitParameters.Length)
                            {
                                string mesg = string.Format("For model {0}, the number of parameters in {1} does not match the number of paramter types {2}.", model.Name, parameter, paramType);
                                throw new System.ApplicationException(mesg);
                            }
                            else
                            {
                                int interactionIndex = 0;
                                foreach (string pType in paramTypes)
                                {
                                    string param = splitParameters[interactionIndex];
                                    if (pType.Equals("neighbor", StringComparison.OrdinalIgnoreCase))
                                    {
                                        if (SiteVars.NeighborVars[site].ContainsKey(param))
                                        {
                                            double modelValue = SiteVars.NeighborVars[site][param];
                                            paramProduct *= modelValue;
                                        }
                                        else
                                        {
                                            string mesg = string.Format("The Neighborhood parameter {0} is listed for model {1} but is not included under NeighborhoodVariables.", param, model.Name);
                                            throw new System.ApplicationException(mesg);
                                        }
                                    }
                                    else if (pType.Equals("climate", StringComparison.OrdinalIgnoreCase))
                                    //else if (paramType == "climate")
                                    {
                                        if (SiteVars.ClimateVars[site].ContainsKey(param))
                                        {
                                            double modelValue = SiteVars.ClimateVars[site][param];
                                            paramProduct *= modelValue;
                                        }
                                        else
                                        {
                                            string mesg = string.Format("The Climate parameter {0} is listed for model {1} but is not included under ClimateVariables.", param, model.Name);
                                            throw new System.ApplicationException(mesg);
                                        }
                                    }
                                    else if (pType.Equals("distance", StringComparison.OrdinalIgnoreCase))
                                    //else if (paramType == "climate")
                                    {
                                        if (SiteVars.DistanceVars[site].ContainsKey(param))
                                        {
                                            double modelValue = SiteVars.DistanceVars[site][param];
                                            paramProduct *= modelValue;
                                        }
                                        else
                                        {
                                            string mesg = string.Format("The Distance parameter {0} is listed for model {1} but is not included under DistanceVariables.", param, model.Name);
                                            throw new System.ApplicationException(mesg);
                                        }
                                    }
                                    else if (pType.Equals("biomass", StringComparison.OrdinalIgnoreCase))
                                    //else if (paramType =="biomass")
                                    {
                                        double modelValue = Util.ComputeBiomass(SiteVars.Cohorts[site]);
                                        paramProduct *= modelValue;
                                    }
                                    else
                                    {
                                        string mesg = string.Format("For model {0}, parameter {1} has parameter type {2}; expected 'int', 'neighbor', 'climate', 'distance', 'biomass','lnbiomass', 'logbiomass','age', 'lnage', 'logage', or 'interaction'.", model.Name, param, pType);
                                        throw new System.ApplicationException(mesg);
                                    }
                                    interactionIndex++;
                                }
                                double interactionValue = paramProduct * paramValue;
                                modelPredict += interactionValue;
                            }
                        }
                        else
                        {
                            string mesg = string.Format("For model {0}, parameter {1} has parameter type {2}; expected 'int', 'neighbor', 'climate', 'distance', 'biomass','lnbiomass', 'logbiomass','age', 'lnage', 'logage', or 'interaction'.", model.Name, parameter, paramType);
                            throw new System.ApplicationException(mesg);
                        }

                        paramIndex++;
                    }
                    // Back-transform model prediction
                    float finalPredict = (float)Math.Exp(modelPredict);
                    // Write Site Variable
                    SiteVars.SpeciesModels[site][model.Name] = (float)finalPredict;
                    if(site.IsActive)
                    {
                        ecoregionSum[ecoregion.Index] += finalPredict;
                        landscapeSum += finalPredict;
                    }
                }
                foreach (IEcoregion ecoregion in ModelCore.Ecoregions)
                {
                    //ecoregionAvgValues[ecoregion.Index].Add(model.Name, 0);
                    ecoregionAvgValues[ecoregion.Index][model.Name] = ecoregionSum[ecoregion.Index] / activeSiteCount[ecoregion.Index];
                }
                landscapeAvgValues[model.Name] = landscapeSum / ModelCore.Landscape.ActiveSiteCount;
            }

            
            //foreach (IModelDefinition model in modelDefs)
            //{
            //    habitatLog.Write("{0},", ModelCore.CurrentTime);
            //    habitatLog.Write("{0},", model.Name);
            //    habitatLog.Write("{0}", landscapeAvgValues[model.Name]);
            //    foreach (IEcoregion ecoregion in ModelCore.Ecoregions)
            //    {
            //        habitatLog.Write(",{0}", ecoregionAvgValues[ecoregion.Index][model.Name]);
            //    }
            //    habitatLog.WriteLine("");
            //}


            if (parameters.LogFileName != null)
            {
                foreach (IModelDefinition model in modelDefs)
                {

                    habitatLog.Clear();
                    SpeciesHabitatLog shlog = new SpeciesHabitatLog();
                    shlog.Time = ModelCore.CurrentTime;
                    shlog.Ecoregion = "TotalLandscape";
                    shlog.SpeciesModelName = model.Name;
                    //shlog.NumSites = activeSiteCount[ecoregion.Index];
                    shlog.Index = landscapeAvgValues[model.Name];
                    habitatLog.AddObject(shlog);
                    habitatLog.WriteToFile();

                    foreach (IEcoregion ecoregion in ModelCore.Ecoregions)
                    {
                        habitatLog.Clear();
                        shlog = new SpeciesHabitatLog();
                        shlog.Time = ModelCore.CurrentTime;
                        shlog.Ecoregion = ecoregion.Name;
                        shlog.SpeciesModelName = model.Name;
                        //shl.EcoregionIndex = ecoregion.Index;
                        //shlog.NumSites = activeSiteCount[ecoregion.Index];
                        shlog.Index = ecoregionAvgValues[ecoregion.Index][model.Name];
                        habitatLog.AddObject(shlog);
                        habitatLog.WriteToFile();
                    }
                }
            }

            if (parameters.SpeciesLogFileNames != null)
            {
                int selectModelCount = 0;
                foreach (IModelDefinition model in modelDefs)
                {
                    //MetadataTable<IndividualSpeciesHabitatLog> speciesLog = sppLogs[selectModelCount];
                    sppLogs[selectModelCount].Clear();
                    IndividualSpeciesHabitatLog sppLog = new IndividualSpeciesHabitatLog();
                    sppLog.Time = ModelCore.CurrentTime;
                    sppLog.Ecoregion = "TotalLandscape";
                    sppLog.SpeciesModel = model.Name;
                    //shlog.NumSites = activeSiteCount[ecoregion.Index];
                    sppLog.Index = landscapeAvgValues[model.Name];
                    sppLogs[selectModelCount].AddObject(sppLog);
                    sppLogs[selectModelCount].WriteToFile();

                    foreach (IEcoregion ecoregion in ModelCore.Ecoregions)
                    {
                        sppLogs[selectModelCount].Clear();
                        sppLog = new IndividualSpeciesHabitatLog();
                        sppLog.Time = ModelCore.CurrentTime;
                        sppLog.Ecoregion = ecoregion.Name;
                        sppLog.SpeciesModel = model.Name;
                        //shlog.NumSites = activeSiteCount[ecoregion.Index];
                        sppLog.Index = ecoregionAvgValues[ecoregion.Index][model.Name];
                        sppLogs[selectModelCount].AddObject(sppLog);
                        sppLogs[selectModelCount].WriteToFile();
                    }
                    selectModelCount++;
                }
            }
             

            // Ouput Maps
            if (!(parameters.LocalVarMapFileNames == null))
            {
                //----- Write LocalVar maps --------
                foreach (MapDefinition localVar in parameters.ReclassMaps)
                {
                    string localVarPath = LocalMapFileNames.ReplaceTemplateVars(parameters.LocalVarMapFileNames, localVar.Name, PlugIn.ModelCore.CurrentTime);
                    using (IOutputRaster<BytePixel> outputRaster = modelCore.CreateRaster<BytePixel>(localVarPath, modelCore.Landscape.Dimensions))
                    {
                        BytePixel pixel = outputRaster.BufferPixel;
                        foreach (Site site in PlugIn.ModelCore.Landscape.AllSites)
                        {
                            if (site.IsActive)
                            {
                                pixel.MapCode.Value = (byte)(SiteVars.LocalVars[site][localVar.Name] + 1);
                            }
                            else
                            {
                                //  Inactive site
                                pixel.MapCode.Value = 0;
                            }
                            outputRaster.WriteBufferPixel();
                        }
                    }
                }
            }
            if (!(parameters.NeighborMapFileNames == null))
            {
                //----- Write LocalVar maps --------
                foreach (NeighborVariableDefinition neighborVar in parameters.NeighborVars)
                {
                    string neighborVarPath = NeighborMapFileNames.ReplaceTemplateVars(parameters.NeighborMapFileNames, neighborVar.Name, PlugIn.ModelCore.CurrentTime);
                    using (IOutputRaster<ShortPixel> outputRaster = modelCore.CreateRaster<ShortPixel>(neighborVarPath, modelCore.Landscape.Dimensions))
                    {
                        ShortPixel pixel = outputRaster.BufferPixel;
                        foreach (Site site in PlugIn.ModelCore.Landscape.AllSites)
                        {
                            if (site.IsActive)
                            {
                                pixel.MapCode.Value = (short)(System.Math.Round(SiteVars.NeighborVars[site][neighborVar.Name] * 100.0));
                            }
                            else
                            {
                                //  Inactive site
                                pixel.MapCode.Value = 0;
                            }
                            outputRaster.WriteBufferPixel();
                        }
                    }
                }
            }
            if (!(parameters.ClimateMapFileNames == null))
            {
                //----- Write LocalVar maps --------
                foreach (ClimateVariableDefinition climateVar in parameters.ClimateVars)
                {
                    string climateVarPath = ClimateMapFileNames.ReplaceTemplateVars(parameters.ClimateMapFileNames, climateVar.Name, PlugIn.ModelCore.CurrentTime);
                    using (IOutputRaster<ShortPixel> outputRaster = modelCore.CreateRaster<ShortPixel>(climateVarPath, modelCore.Landscape.Dimensions))
                    {
                        ShortPixel pixel = outputRaster.BufferPixel;
                        foreach (Site site in PlugIn.ModelCore.Landscape.AllSites)
                        {
                            if (site.IsActive)
                            {
                                pixel.MapCode.Value = (short)(System.Math.Round(SiteVars.ClimateVars[site][climateVar.Name] * 100.0));
                            }
                            else
                            {
                                //  Inactive site
                                pixel.MapCode.Value = 0;
                            }
                            outputRaster.WriteBufferPixel();
                        }
                    }
                }
            }
            if (!(parameters.DistanceMapFileNames == null))
            {
                //----- Write DistanceVar maps --------
                foreach (DistanceVariableDefinition distanceVar in parameters.DistanceVars)
                {
                    string distanceVarPath = DistanceMapFileNames.ReplaceTemplateVars(parameters.DistanceMapFileNames, distanceVar.Name, PlugIn.ModelCore.CurrentTime);
                    using (IOutputRaster<ShortPixel> outputRaster = modelCore.CreateRaster<ShortPixel>(distanceVarPath, modelCore.Landscape.Dimensions))
                    {
                        ShortPixel pixel = outputRaster.BufferPixel;
                        foreach (Site site in PlugIn.ModelCore.Landscape.AllSites)
                        {
                            if (site.IsActive)
                            {
                                pixel.MapCode.Value = (short)(System.Math.Round(SiteVars.DistanceVars[site][distanceVar.Name] * 100.0));
                            }
                            else
                            {
                                //  Inactive site
                                pixel.MapCode.Value = 0;
                            }
                            outputRaster.WriteBufferPixel();
                        }
                    }
                }
            }
            if (!(parameters.SpeciesMapFileNames == null))
            {
                //----- Write Species Model maps --------
                foreach (ModelDefinition sppModel in parameters.Models)
                {
                    string sppModelPath = SpeciesMapFileNames.ReplaceTemplateVars(parameters.SpeciesMapFileNames, sppModel.Name, PlugIn.ModelCore.CurrentTime);
                    using (IOutputRaster<ShortPixel> outputRaster = modelCore.CreateRaster<ShortPixel>(sppModelPath, modelCore.Landscape.Dimensions))
                    {
                        ShortPixel pixel = outputRaster.BufferPixel;
                        foreach (Site site in PlugIn.ModelCore.Landscape.AllSites)
                        {
                            if (site.IsActive)
                            {
                                pixel.MapCode.Value = (short)System.Math.Round(SiteVars.SpeciesModels[site][sppModel.Name] * 100.0);
                                //pixel.MapCode.Value = (short)System.Math.Round(SiteVars.SpeciesModels[site][sppModel.Name]);
                            }
                            else
                            {
                                //  Inactive site
                                pixel.MapCode.Value = 0;
                            }
                            outputRaster.WriteBufferPixel();
                        }
                    }
                }
            }

        }


        //---------------------------------------------------------------------

        private byte CalcForestType(List<IForestType> forestTypes,
                                    Site site)
        {
            int forTypeCnt = 0;

            double[] forTypValue = new double[forestTypes.Count];

            foreach(ISpecies species in modelCore.Species)
            {
                double sppValue = 0.0;

                if (SiteVars.Cohorts[site] == null)
                    break;

                sppValue = Util.ComputeBiomass(SiteVars.Cohorts[site][species]);

                forTypeCnt = 0;
                foreach(IForestType ftype in forestTypes)
                {
                    if(ftype[species.Index] != 0)
                    {
                        if(ftype[species.Index] == -1)
                            forTypValue[forTypeCnt] -= sppValue;
                        if (ftype[species.Index] == 1)
                        {
                            double cohortValue = 0;
                            if (sppValue > 0)
                            {
                                foreach (ICohort cohort in SiteVars.Cohorts[site][species])
                                {
                                    if (cohort.Age >= ftype.MinAge && cohort.Age <= ftype.MaxAge)
                                        cohortValue += cohort.Biomass;
                                }
                            }
                            forTypValue[forTypeCnt] += cohortValue;
                        }
                    }
                    forTypeCnt++;
                }
            }

            int finalForestType = 0;
            double maxValue = -0.001;
            forTypeCnt = 0;
            foreach(IForestType ftype in forestTypes)
            {
                if(forTypValue[forTypeCnt]>maxValue)
                {
                    maxValue = forTypValue[forTypeCnt];
                    finalForestType = forTypeCnt+1;
                }
                forTypeCnt++;
            }
            return (byte) finalForestType;
        }

        //-------------------------------------------------------
        //Calculate the distance from a location to a center
        //point (row and column = 0).
        private static double DistanceFromCenter(double row, double column)
        {
            double CellLength = PlugIn.ModelCore.CellLength;
            row = System.Math.Abs(row) * CellLength;
            column = System.Math.Abs(column) * CellLength;
            double aSq = System.Math.Pow(column, 2);
            double bSq = System.Math.Pow(row, 2);
            return System.Math.Sqrt(aSq + bSq);
        }
    }
}
