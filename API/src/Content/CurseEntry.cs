using SoG.Modding.Utils;

namespace SoG.Modding.Content
{
    /// <summary>
    /// Represents a modded treat or curse from Arcade Mode.
    /// </summary>
    /// <remarks> 
    /// Most of the methods in this class can only be used while a mod is loading, that is, inside <see cref="Mod.Load"/>.
    /// </remarks>
    public class CurseEntry : Entry<RogueLikeMode.TreatsCurses>
    {
        #region IEntry Properties

        public override Mod Mod { get; internal set; }

        public override RogueLikeMode.TreatsCurses GameID { get; internal set; }

        public override string ModID { get; internal set; }

        #endregion

        #region Internal Data

        internal bool isTreat = false;

        internal string texturePath = "";

        internal string name = "Candy's Shenanigans";

        internal string description = "It's a mysterious treat or curse!";

        internal float scoreModifier = 0f;

        internal string nameHandle = "";

        internal string descriptionHandle = "";

        #endregion

        #region Public Interface

        /// <summary>
        /// Gets or sets the object as being a treat or curse. <para/>
        /// The only difference between a treat and curse is the NPC shop in which they appear.
        /// </summary>
        public bool IsTreat
        {
            get => isTreat;
            set
            {
                ErrorHelper.ThrowIfNotLoading(Mod);
                isTreat = value;
            }
        }

        /// <summary>
        /// Gets or sets the object as being a treat or curse. <para/>
        /// The only difference between a treat and curse is the NPC shop in which they appear.
        /// </summary>
        public bool IsCurse
        {
            get => !IsTreat;
            set => IsTreat = !value;
        }

        /// <summary>
        /// Gets or sets the texture path of the treat or curse's icon. This path is relative to the "Content" folder.
        /// </summary>
        public string TexturePath
        {
            get => texturePath;
            set
            {
                ErrorHelper.ThrowIfNotLoading(Mod);
                texturePath = value;
            }
        }

        /// <summary>
        /// Gets or sets the name displayed inside the game.
        /// </summary>
        public string Name
        {
            get => name;
            set
            {
                ErrorHelper.ThrowIfNotLoading(Mod);
                name = value;
            }
        }

        /// <summary>
        /// Gets or sets the description displayed inside the game.
        /// </summary>
        public string Description
        {
            get => description;
            set
            {
                ErrorHelper.ThrowIfNotLoading(Mod);
                description = value;
            }
        }

        /// <summary>
        /// Gets or sets the score modifier. <para/>
        /// A score modifier of 0.2 corresponds to +20%, -0.15 to -15%, etc.
        /// </summary>
        public float ScoreModifier
        {
            get => scoreModifier;
            set
            {
                ErrorHelper.ThrowIfNotLoading(Mod);
                scoreModifier = value;
            }
        }

        #endregion

        internal CurseEntry() { }

        internal CurseEntry(Mod owner, RogueLikeMode.TreatsCurses gameID, string modID)
        {
            Mod = owner;
            GameID = gameID;
            ModID = modID;
        }

        internal override void Initialize()
        {
            nameHandle = $"TreatCurse_{(int)GameID}_Name";
            descriptionHandle = $"TreatCurse_{(int)GameID}_Description";

            Globals.Game.EXT_AddMiscText("Menus", nameHandle, name);
            Globals.Game.EXT_AddMiscText("Menus", descriptionHandle, description);

            // Texture on demand
        }

        internal override void Cleanup()
        {
            Globals.Game.EXT_RemoveMiscText("Menus", nameHandle);
            Globals.Game.EXT_RemoveMiscText("Menus", descriptionHandle);

            if (ModUtils.IsModContentPath(texturePath))
            {
                AssetUtils.UnloadAsset(Globals.Game.Content, texturePath);
            }
        }
    }
}
