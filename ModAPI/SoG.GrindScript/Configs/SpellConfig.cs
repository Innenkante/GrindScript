namespace SoG.Modding.Configs
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
