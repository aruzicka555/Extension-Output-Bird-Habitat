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
    public static class LocalMapFileNames
    {
        public const string Variable = "local-var-name";
        public const string TimestepVar = "timestep";


        private static IDictionary<string, bool> knownVars;
        private static IDictionary<string, string> varValues;

        //---------------------------------------------------------------------

        static LocalMapFileNames()
        {
            knownVars = new Dictionary<string, bool>();
            knownVars[Variable] = true;
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
                                                 string localVarMapName,
                                                 int    timestep)
        {
            varValues[Variable] = localVarMapName;
            varValues[TimestepVar] = timestep.ToString();
            return OutputPath.ReplaceTemplateVars(template, varValues);
        }
        //---------------------------------------------------------------------

        public static string ReplaceTemplateVars(string template,
                                                 string localVarMapName)
        {
            varValues[Variable] = localVarMapName;
            varValues[TimestepVar] = "{timestep}";
            return OutputPath.ReplaceTemplateVars(template, varValues);
        }

    }
}
