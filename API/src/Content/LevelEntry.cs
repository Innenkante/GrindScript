namespace SoG.Modding.Content
{
    /// <summary>
    /// Represents a modded level, and defines ways to create it.
    /// </summary>
    /// <remarks> 
    /// Most of the methods in this class can only be used while a mod is loading, that is, inside <see cref="Mod.Load"/>.
    /// </remarks>
    public class LevelEntry : Entry<Level.ZoneEnum>
    {
        #region IEntry Properties

        public override Mod Mod { get; internal set; }

        public override Level.ZoneEnum GameID { get; internal set; }

        public override string ModID { get; internal set; }

        #endregion

        #region Internal Data

        internal LevelBuilder builder = null;

        internal LevelLoader loader = null;

        internal Level.WorldRegion worldRegion = Level.WorldRegion.NotLoaded;

        #endregion

        #region Public Interface

        /// <summary>
        /// Gets or sets the builder for this level.
        /// The builder is used to initialize the level blueprint with all of the static objects that will
        /// appear in it (spawn points, static environments, etc.)
        /// </summary>
        public LevelBuilder Builder
        {
            get => builder;
            set
            {
                ErrorHelper.ThrowIfNotLoading(Mod);
                builder = value;
            }
        }

        /// <summary>
        /// Gets or sets the level loader for this level. <para/>
        /// The loader is called whenever this level is entered. Its task is to initialize all of the 
        /// dynamic objects that will appear in the level (NPCs, state dependent stuff, etc.)
        /// </summary>
        public LevelLoader Loader
        {
            get => loader;
            set
            {
                ErrorHelper.ThrowIfNotLoading(Mod);
                loader = value;
            }
        }

        /// <summary>
        /// Gets or sets the world region of this level. <para/>
        /// The world region is used for some game logic, such as audio loading and unloading.
        /// </summary>
        public Level.WorldRegion WorldRegion
        {
            get => worldRegion;
            set
            {
                ErrorHelper.ThrowIfNotLoading(Mod);
                worldRegion = value;
            }
        }

        #endregion

        internal LevelEntry() { }

        internal LevelEntry(Mod owner, Level.ZoneEnum gameID, string modID)
        {
            Mod = owner;
            GameID = gameID;
            ModID = modID;
        }

        internal override void Initialize()
        {
            // Nothing for now
        }

        internal override void Cleanup()
        {
            // Nothing for now
        }
    }
}
