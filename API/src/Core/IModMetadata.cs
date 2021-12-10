using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoG.Modding
{
    /// <summary>
    /// Defines the metadata of a mod for Secrets of Grindea.
    /// </summary>
    public interface IModMetadata
    {
        /// <summary>
        /// The mod identifier. This should be unique, as mods with the same identifier will cause conflicts.
        /// </summary>
        string NameID { get; }

        /// <summary>
        /// The mod's version.
        /// </summary>
        Version ModVersion { get; }
    }
}
