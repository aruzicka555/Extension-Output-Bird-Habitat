
using Edu.Wisc.Forest.Flel.Util;
using Landis.Core;
using System.Collections.Generic;

namespace Landis.Extension.Output.BirdHabitat
{
    /// <summary>
    /// Methods for working with the template for filenames of reclass maps.
    /// </summary>
    public static class SpeciesLogFileNames
    {
        public const string SpeciesNameVar = "species-name";

        private static IDictionary<string, bool> knownVars;
        private static IDictionary<string, string> varValues;

        //---------------------------------------------------------------------

        static SpeciesLogFileNames()
        {
            knownVars = new Dictionary<string, bool>();
            knownVars[SpeciesNameVar] = true;

            varValues = new Dictionary<string, string>();
        }

        //---------------------------------------------------------------------

        public static void CheckTemplateVars(string template)
        {
            OutputPath.CheckTemplateVars(template, knownVars);
        }

        //---------------------------------------------------------------------

        public static string ReplaceTemplateVars(string template,
                                                 string speciesMapName)
        {
            varValues[SpeciesNameVar] = speciesMapName;
            return OutputPath.ReplaceTemplateVars(template, varValues);
        }
        //---------------------------------------------------------------------


    
    }
}
