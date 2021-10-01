using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoG.Modding
{
    /// <summary>
    /// Represents standalone mod metadata.
    /// </summary>
    public class ModMetadata : IModMetadata
    {
        public ModMetadata()
        {

        }

        public ModMetadata(IModMetadata meta)
        {
            NameID = meta.NameID;
            ModVersion = meta.ModVersion;
            DisableObjectCreation = meta.DisableObjectCreation;
            AllowDiscoveryByMods = meta.AllowDiscoveryByMods;
        }

        public string NameID { get; set; }

        public Version ModVersion { get; set; }

        public bool DisableObjectCreation { get; set; }

        public bool AllowDiscoveryByMods { get; set; }
    }
}
