using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Landis.Library.Metadata;

namespace Landis.Extension.Output.LandscapeHabitat
{
    public class IndividualSpeciesHabitatLog
    {

        [DataFieldAttribute(Desc = "Bird Model")]
        public string SpeciesModel { set; get; }

        [DataFieldAttribute(Unit = FieldUnits.Year, Desc = "Simulation Year")]
        public int Time { set; get; }

        [DataFieldAttribute(Desc = "Ecoregion Name")]
        public string Ecoregion { set; get; }
        
        [DataFieldAttribute(Unit = "Index", Desc = "Index of Abundance", Format = "0.00")]
        public double Index { set; get; }

    }
}
