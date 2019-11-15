//  Copyright 2005-2010 Portland State University, University of Wisconsin-Madison
//  Authors:  Robert M. Scheller, Jimm Domingo

//using Edu.Wisc.Forest.Flel.Util;
using System.Collections.Generic;

namespace Landis.Extension.Output.LandscapeHabitat
{
    /// <summary>
    /// The definition of a Distance Variable.
    /// </summary>
    public interface IDistanceVariableDefinition
    {
        /// <summary>
        /// Var name
        /// </summary>
        string Name
        {
            get;set;
        }

        //---------------------------------------------------------------------

        /// <summary>
        /// Local Variable
        /// </summary>
        string LocalVariable
        {
            get;
            set;
        }
        //---------------------------------------------------------------------
        /// <summary>
        /// Transform
        /// </summary>
        string Transform
        {
            get;
            set;
        }
        //---------------------------------------------------------------------
    }

    /// <summary>
    /// The definition of a Distance Variable.
    /// </summary>
    public class DistanceVariableDefinition
        : IDistanceVariableDefinition
    {
        private string name;
        private string localVariable;
        private string transform;

        //---------------------------------------------------------------------

        /// <summary>
        /// Var name
        /// </summary>
        public string Name
        {
            get
            {
                return name;
            }
            set
            {
                name = value;
            }
        }

        //---------------------------------------------------------------------

        /// <summary>
        /// LocalVariable
        /// </summary>
        public string LocalVariable
        {
            get
            {
                return localVariable;
            }
            set
            {
                localVariable = value;
            }
        }

        //---------------------------------------------------------------------
        /// <summary>
        /// Transform
        /// </summary>
        public string Transform
        {
            get
            {
                return transform;
            }
            set
            {
                transform = value;
            }
        }

        //---------------------------------------------------------------------

        /// <summary>
        /// Initialize a new instance.
        /// </summary>
        public DistanceVariableDefinition()
        {
        }
        //---------------------------------------------------------------------

    }
}
