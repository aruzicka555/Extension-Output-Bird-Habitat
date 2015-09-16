using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Landis.Library.Metadata;

namespace Landis.Extension.Output.BirdHabitat
{
    public class IndividualSpeciesHabitatLog
    {
        [DataFieldAttribute(Unit = FieldUnits.Year, Desc = "Simulation Year")]
        public int Time { set; get; }

        [DataFieldAttribute(Desc = "Ecoregion Name")]
        public string Ecoregion { set; get; }

        //[DataFieldAttribute(Desc = "Species Name")]
        //public string SpeciesName { set; get; }

        [DataFieldAttribute(Unit = FieldUnits.Count, Desc = "Index of Abundance", Format = "0.00")]
        public double SppHabitat { set; get; }

    }
}
