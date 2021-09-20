using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SoG.Modding.Core;

namespace SoG.Modding.API.Configs
{
    public class SpellConfig
    {
        public SpellConfig(string uniqueID)
        {
            ModID = uniqueID;
        }

        public string ModID { get; set; }

        public SpellBuilder Builder { get; set; }

        public bool IsMagicSkill { get; set; }

        public bool IsUtilitySkill { get; set; }

        public bool IsMeleeSkill { get; set; }

        public SpellConfig DeepCopy()
        {
            SpellConfig clone = (SpellConfig)MemberwiseClone();

            return clone;
        }
    }
}
