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
        }

        public string NameID { get; set; }

        public Version ModVersion { get; set; }
    }
}
