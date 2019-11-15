//  Copyright 2005-2010 Portland State University, University of Wisconsin-Madison
//  Authors:  Robert M. Scheller, Jimm Domingo

using Landis.Utilities;
using Landis.Core;
using System.Collections.Generic;

namespace Landis.Extension.Output.LandscapeHabitat
{
    /// <summary>
    /// Methods for working with the template for filenames of reclass maps.
    /// </summary>
    public static class NeighborMapFileNames
    {
        public const string NeighborVar = "neighbor-var-name";
        public const string TimestepVar = "timestep";


        private static IDictionary<string, bool> knownVars;
        private static IDictionary<string, string> varValues;

        //---------------------------------------------------------------------

        static NeighborMapFileNames()
        {
            knownVars = new Dictionary<string, bool>();
            knownVars[NeighborVar] = true;
            knownVars[TimestepVar] = true;

            varValues = new Dictionary<string, string>();
        }

        //---------------------------------------------------------------------

        public static void CheckTemplateVars(string template)
        {
            OutputPath.CheckTemplateVars(template, knownVars);
        }

        //---------------------------------------------------------------------

        public static string ReplaceTemplateVars(string template,
                                                 string reclassMapName,
                                                 int    timestep)
        {
            varValues[NeighborVar] = reclassMapName;
            varValues[TimestepVar] = timestep.ToString();
            return OutputPath.ReplaceTemplateVars(template, varValues);
        }
    }
}
