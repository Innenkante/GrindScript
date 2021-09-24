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
        public string NameID { get; set; }

        public Version Version { get; set; }

        public Version GrindScriptVersion { get; set; }

        public bool DisableObjectCreation { get; set; }

        public bool AllowDiscoveryByMods { get; set; }
    }
}
