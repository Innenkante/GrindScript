using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoG.Modding.Configs
{
    public class WorldRegionConfig
    {
        public WorldRegionConfig(string uniqueID)
        {
            ModID = uniqueID;
        }

        public string ModID { get; set; } = "";

        public WorldRegionConfig DeepCopy()
        {
            WorldRegionConfig clone = (WorldRegionConfig)MemberwiseClone();

            return clone;
        }
    }
}
