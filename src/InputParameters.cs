//  Copyright 2005-2010 Portland State University, University of Wisconsin-Madison
//  Authors:  Robert M. Scheller, Jimm Domingo

using Edu.Wisc.Forest.Flel.Util;
using System.Collections.Generic;
using System.Data;

namespace Landis.Extension.Output.BirdHabitat
{
    /// <summary>
    /// The parameters for the plug-in.
    /// </summary>
    public class InputParameters
        : IInputParameters
    {
        private int timestep;
        private List<IMapDefinition> mapDefns;
        private string localVarMapFileNames;
        private string neighborMapFileNames;
        private string climateMapFileNames;
        private string distanceMapFileNames;
        private string speciesMapFileNames;
        private List<IVariableDefinition> varDefn;
        private List<INeighborVariableDefinition> neighborVarDefn;
        private List<IClimateVariableDefinition> climateVarDefn;
        private List<IDistanceVariableDefinition> distanceVarDefn;
        private List<IModelDefinition> modelDefn;
        private DataTable climateDataTable;
        private string logFileName;
        private string speciesLogFileNames;

        //---------------------------------------------------------------------

        /// <summary>
        /// Timestep (years)
        /// </summary>
        public int Timestep
        {
            get {
                return timestep;
            }
            set {
                if (value < 0)
                    throw new InputValueException(value.ToString(),"Value must be = or > 0.");
                timestep = value;
            }
        }

        //---------------------------------------------------------------------


        /// <summary>
        /// Reclass maps
        /// </summary>
        public List<IMapDefinition> ReclassMaps
        {
            get {
                return mapDefns;
            }
        }

        //---------------------------------------------------------------------
        /// <summary>
        /// Derived Variables
        /// </summary>
        public List<IVariableDefinition> DerivedVars
        {
            get
            {
                return varDefn;
            }
        }

        //---------------------------------------------------------------------
        /// <summary>
        /// Neighborhood Variables
        /// </summary>
        public List<INeighborVariableDefinition> NeighborVars
        {
            get
            {
                return neighborVarDefn;
            }
        }

        //---------------------------------------------------------------------
        /// <summary>
        /// Climate Variables
        /// </summary>
        public List<IClimateVariableDefinition> ClimateVars
        {
            get
            {
                return climateVarDefn;
            }
        }

        //---------------------------------------------------------------------
        /// <summary>
        /// Distance Variables
        /// </summary>
        public List<IDistanceVariableDefinition> DistanceVars
        {
            get
            {
                return distanceVarDefn;
            }
        }

        //---------------------------------------------------------------------
        /// <summary>
        /// Species Models
        /// </summary>
        public List<IModelDefinition> Models
        {
            get
            {
                return modelDefn;
            }
        }

        //---------------------------------------------------------------------

        /// <summary>
        /// Template for the filenames for reclass maps.
        /// </summary>
        public string LocalVarMapFileNames
        {
            get {
                return localVarMapFileNames;
            }
            set {
                BirdHabitat.LocalMapFileNames.CheckTemplateVars(value);
                localVarMapFileNames = value;
            }
        }

        //---------------------------------------------------------------------
        /// <summary>
        /// Template for the filenames for neighborhood maps.
        /// </summary>
        public string NeighborMapFileNames
        {
            get
            {
                return neighborMapFileNames;
            }
            set
            {
                BirdHabitat.NeighborMapFileNames.CheckTemplateVars(value);
                neighborMapFileNames = value;
            }
        }
        //---------------------------------------------------------------------
        /// <summary>
        /// Template for the filenames for climate maps.
        /// </summary>
        public string ClimateMapFileNames
        {
            get
            {
                return climateMapFileNames;
            }
            set
            {
                BirdHabitat.ClimateMapFileNames.CheckTemplateVars(value);
                climateMapFileNames = value;
            }
        }
        //---------------------------------------------------------------------
        /// <summary>
        /// Template for the filenames for distance maps.
        /// </summary>
        public string DistanceMapFileNames
        {
            get
            {
                return distanceMapFileNames;
            }
            set
            {
                BirdHabitat.DistanceMapFileNames.CheckTemplateVars(value);
                distanceMapFileNames = value;
            }
        }
        //---------------------------------------------------------------------
        /// <summary>
        /// Template for the filenames for species model results.
        /// </summary>
        public string SpeciesMapFileNames
        {
            get
            {
                return speciesMapFileNames;
            }
            set
            {
                BirdHabitat.SpeciesMapFileNames.CheckTemplateVars(value);
                speciesMapFileNames = value;
            }
        }

        //---------------------------------------------------------------------
        /// <summary>
        /// Climate Data Table.
        /// </summary>
        public DataTable ClimateDataTable
        {
            get
            {
                return climateDataTable;
            }
            set
            {
                climateDataTable = value;
            }
        }
        //---------------------------------------------------------------------
        /// <summary>
        /// Filename for log file.
        /// </summary>
        public string LogFileName
        {
            get
            {
                return logFileName;
            }
            set
            {
                if (value == null)
                    throw new InputValueException(value.ToString(), "Value must be a file path.");
                logFileName = value;
            }
        }
        //---------------------------------------------------------------------
        /// <summary>
        /// Template for the filenames for species model logs.
        /// </summary>
        public string SpeciesLogFileNames
        {
            get
            {
                return speciesLogFileNames;
            }
            set
            {
                BirdHabitat.SpeciesLogFileNames.CheckTemplateVars(value);
                speciesLogFileNames = value;
            }
        }
        //---------------------------------------------------------------------

        public InputParameters(int speciesCount)
        {
            mapDefns = new List<IMapDefinition>();
            varDefn = new List<IVariableDefinition>();
            neighborVarDefn = new List<INeighborVariableDefinition>();
            climateVarDefn = new List<IClimateVariableDefinition>();
            distanceVarDefn = new List<IDistanceVariableDefinition>();
            modelDefn = new List<IModelDefinition>();
            climateDataTable = new DataTable();
        }
        //---------------------------------------------------------------------

    }
}
