using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Hedronoid.HNDBuild
{
    /// <summary>
    /// One build step
    /// </summary>
    public class HNDBuildStep : ScriptableObject
    {
        public virtual List<string> GetRequiredEnvValues()
        {
            return new List<string>();
        }

        /// <summary>
        /// Override to do you stuff in the build process
        /// </summary>
        /// <param name="environment">Environment can be read from and written to.</param>
        public virtual void Execute(HNDBuildEnvironment environment)
        {
            // Inherit and use. Environment can be read from and written to.
        }
    }
}