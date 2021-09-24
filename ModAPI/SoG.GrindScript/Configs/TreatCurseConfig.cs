namespace SoG.Modding.Configs
{
    public class TreatCurseConfig
    {
        public TreatCurseConfig(string uniqueID)
        {
            ModID = uniqueID;
        }

        bool _isTreat = false;

        public bool IsTreat 
        {
            get => _isTreat;
            set => _isTreat = value;
        }

        public bool IsCurse
        {
            get => !_isTreat;
            set => _isTreat = !value;
        }

        public string TexturePath { get; set; } = "";

        public string Name { get; set; } = "Candy's Shenanigans";

        public string Description { get; set; } = "It's a mysterious treat or curse!";

        public string ModID { get; set; } = "";

        public float ScoreModifier { get; set; } = 0f;

        public TreatCurseConfig DeepCopy()
        {
            TreatCurseConfig clone = (TreatCurseConfig)MemberwiseClone();

            return clone;
        }
    }
}
