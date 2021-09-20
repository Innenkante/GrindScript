using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoG.Modding.API.Configs
{
    public class PerkConfig
    {
        public PerkConfig(string uniqueID)
        {
            ModID = uniqueID;
        }

        public Action<PlayerView> RunStartActivator { get; set; } = null;

        public int EssenceCost { get; set; } = 15;

        public string Name { get; set; } = "Bishop's Shenanigans";

        public string Description { get; set; } = "It's some weird perk or moldable!";

        public string TexturePath { get; set; } = "";

        public string ModID { get; set; } = "";

        public PerkConfig DeepCopy()
        {
            PerkConfig clone = (PerkConfig)MemberwiseClone();

            return clone;
        }
    }
}
