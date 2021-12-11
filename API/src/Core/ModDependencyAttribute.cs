using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoG.Modding
{
    /// <summary>
    /// Defines a dependency on another mod.
    /// Mods that have dependencies require all of them to be present and loaded before they are.
    /// If a dependency is missing or disabled, the mod will fail to load.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
    public class ModDependencyAttribute : Attribute
    {
        /// <summary>
        /// Gets the dependency's identifier.
        /// </summary>
        public string NameID { get; }

        /// <summary>
        /// Gets the required dependency version.
        /// </summary>
        public string ModVersion { get; }

        /// <summary>
        /// Gets whenever higher versions of the dependency are okay to use.
        /// </summary>
        public bool AllowHigherVersions { get; }

        public ModDependencyAttribute(string NameID, string ModVersion, bool AllowHigherVersions = true)
        {
            this.NameID = NameID;
            this.ModVersion = ModVersion;
            this.AllowHigherVersions = AllowHigherVersions;
        }
    }
}
