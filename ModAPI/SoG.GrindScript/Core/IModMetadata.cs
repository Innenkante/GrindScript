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

        /// <summary>
        /// If set to true, game object creation will be disallowed for this mod.
        /// This means that no new game objects can be added by this mod (such as custom items).
        /// Additionally, the mod will be excluded from mod list saving, and mod compatibility checks.
        /// </summary>
        bool DisableObjectCreation { get; }

        /// <summary>
        /// If set to false, other mods can't query for this mod.
        /// </summary>
        bool AllowDiscoveryByMods { get; }
    }
}
