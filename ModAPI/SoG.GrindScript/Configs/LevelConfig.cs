namespace SoG.Modding.Configs
{
    public class LevelConfig
    {
        public LevelConfig(string uniqueID)
        {
            ModID = uniqueID;
        }

        public string ModID { get; set; }

        /// <summary> 
        /// Called when the game prepares a level switch.
        /// It receives a <see cref="LevelBlueprint"/> as an argument, and it should add to it the information needed for the level.
        /// </summary>
        public LevelBuilder Builder { get; set; } = null;

        /// <summary>
        /// Called after the game has instantiated the <see cref="LevelBlueprint"/>.
        /// The Loader should instantiate most state-dependent objects found within the level, such as entities and bagmans.
        /// </summary>
        public LevelLoader Loader { get; set; } = null;

        /// <summary> Sets the World Region, which is used for asset loading and certain gameplay logic. </summary>
        public Level.WorldRegion WorldRegion { get; set; } = Level.WorldRegion.NotLoaded;

        public LevelConfig DeepCopy()
        {
            LevelConfig clone = (LevelConfig)MemberwiseClone();

            return clone;
        }
    }
}
