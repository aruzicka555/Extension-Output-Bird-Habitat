//  Copyright 2005-2010 Portland State University, University of Wisconsin-Madison
//  Authors:  Robert M. Scheller, Jimm Domingo

using Landis.Library.LeafBiomassCohorts;
using System.Collections.Generic;
using System;
//using Landis.Cohorts;

namespace Landis.Extension.Output.BirdHabitat
{
    /// <summary>
    /// Methods for computing biomass for groups of cohorts.
    /// </summary>
    public static class Util
    {
        public static int ComputeBiomass(ISpeciesCohorts cohorts)
        {
            int total = 0;
            if (cohorts != null)
                foreach (ICohort cohort in cohorts)
                    total += cohort.Biomass;
            return total;
        }

        //---------------------------------------------------------------------

        public static int ComputeBiomass(ISiteCohorts cohorts)
        {
            int total = 0;
            if (cohorts != null)
                foreach (ISpeciesCohorts speciesCohorts in cohorts)
                    total += ComputeBiomass(speciesCohorts);
            return total;
        }

        //---------------------------------------------------------------------

        public static int ComputeAge(ISiteCohorts cohorts)
        {
            int dominantAge = 0;
            Dictionary<int, int> ageDictionary = new Dictionary<int, int>();
            if (cohorts != null)
                foreach (ISpeciesCohorts speciesCohorts in cohorts)
                {
                    foreach (ICohort cohort in speciesCohorts)
                    {
                        int age = cohort.Age;
                        int biomass = cohort.Biomass;
                        if (ageDictionary.ContainsKey(age))
                        {
                            ageDictionary[age] = ageDictionary[age] + biomass;
                        }
                        else
                        {
                            ageDictionary[age] = biomass;
                        }
                    }
                }

            int maxBiomass = 0;
            foreach (var kvp in ageDictionary)
            {
                if (kvp.Value > maxBiomass)
                {
                    dominantAge = kvp.Key;
                    maxBiomass = kvp.Value;
                }

            }

            return dominantAge;
        }

        //---------------------------------------------------------------------
    }
}
