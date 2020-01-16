using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Landis.Library.Metadata;

namespace Landis.Extension.Output.LandscapeHabitat
{
    
    public class SpeciesHabitatLog
    {
        [DataFieldAttribute(Unit = FieldUnits.Year, Desc = "Simulation Year")]
        public int Time { set; get; }

        [DataFieldAttribute(Desc = "Ecoregion Name")]
        public string Ecoregion { set; get; }

        [DataFieldAttribute(Desc = "Species Model Name")]
        public string SpeciesModelName { set; get; }

        [DataFieldAttribute(Unit = "Index", Desc = "Index", Format = "0.00")] 
        public double Index { set; get; }
        
    }
}
