namespace SoG.Modding.Configs
{
    public class StatusEffectConfig
    {
        public StatusEffectConfig(string uniqueID)
        {
            ModID = uniqueID;
        }

        bool _isBuff = false;

        public bool IsBuff
        {
            get => _isBuff;
            set => _isBuff = value;
        }

        public bool IsDebuff
        {
            get => !_isBuff;
            set => _isBuff = !value;
        }

        public string TexturePath { get; set; } = "";

        public string ModID { get; set; } = "";

        public StatusEffectConfig DeepCopy()
        {
            StatusEffectConfig clone = (StatusEffectConfig)MemberwiseClone();

            return clone;
        }
    }
}
