using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoG.Modding.API.Configs
{
    public class PinConfig
    {
        public enum Symbol
        {
            Bow = 0,
            Sword = 1,
            Star = 2,
            Shield = 3,
            UpArrow = 4,
            Potion = 5,
            Exclamation = 6
        }

        public enum Shape
        {
            Circle = 0,
            Square = 1,
            Plus = 2,
            Tablet = 3,
            Diamond = 4
        }

        public enum Color
        {
            YellowOrange = 0,
            Seagull = 1,
            Coral = 2,
            Conifer = 3,
            BilobaFlower = 4,
        }

        public PinConfig(string uniqueID)
        {
            ModID = uniqueID;
        }

        public string ModID { get; set; }

        public Symbol PinSymbol { get; set; } = Symbol.Star;

        public Shape PinShape { get; set; } = Shape.Square;

        public Color PinColor { get; set; } = Color.Seagull;

        public bool IsSticky { get; set; } = false;

        public bool IsBroken { get; set; } = false;

        public string Description { get; set; } = "Some modded pin that isn't very descriptive!";

        public Func<bool> ConditionToDrop { get; set; } = null;

        public Action<PlayerView> EquipAction { get; set; } = null;

        public Action<PlayerView> UnequipAction { get; set; } = null;

        public PinConfig DeepCopy()
        {
            PinConfig clone = (PinConfig)MemberwiseClone();

            return clone;
        }
    }
}
