using SoG.Modding.Utils;
using System;

namespace SoG.Modding.Content
{
    /// <summary>
    /// Represents a modded perk from Arcade Mode.
    /// </summary>
    /// <remarks> 
    /// Most of the methods in this class can only be used while a mod is loading, that is, inside <see cref="Mod.Load"/>.
    /// </remarks>
    public class PerkEntry : Entry<RogueLikeMode.Perks>
    {
        #region IEntry Properties

        public override Mod Mod { get; internal set; }

        public override RogueLikeMode.Perks GameID { get; internal set; }

        public override string ModID { get; internal set; }

        #endregion

        #region Internal Data

        internal Action<PlayerView> runStartActivator = null;

        internal int essenceCost = 15;

        internal string name = "Bishop's Shenanigans";

        internal string description = "It's some weird perk or moldable!";

        internal string texturePath = "";

        internal string textEntry;

        internal Func<bool> unlockCondition;

        #endregion

        #region Public Interface

        /// <summary>
        /// Gets or sets the action to run when a run is started. <para/>
        /// For simple perks that apply their effects at run start only,
        /// you can place their logic here. <para/>
        /// For more complex perks, you will need to check if a perk is active using <see cref="GlobalData.RogueLikePersistentData.IsPerkEquipped"/> inside a mod hook.
        /// </summary>
        public Action<PlayerView> RunStartActivator
        {
            get => runStartActivator;
            set
            {
                ErrorHelper.ThrowIfNotLoading(Mod);
                runStartActivator = value;
            }
        }

        /// <summary>
        /// Gets or sets the essence cost to unlock this perk.
        /// </summary>
        public int EssenceCost
        {
            get => essenceCost;
            set
            {
                ErrorHelper.ThrowIfNotLoading(Mod);
                essenceCost = value;
            }
        }

        /// <summary>
        /// Gets or sets the display name of this perk.
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
        /// Gets or sets the icon of the perk. The texture path is relative to "Content/".
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
        /// Gets or sets the condition for the perk to be available in Bishop's selection.
        /// If no condition is provided, the perk is available by default.
        /// </summary>
        public Func<bool> UnlockCondition
        {
            get => unlockCondition;
            set
            {
                ErrorHelper.ThrowIfNotLoading(Mod);
                unlockCondition = value;
            }
        }

        #endregion

        internal PerkEntry() { }

        internal override void Initialize()
        {
            if (!IsVanilla)
            {
                textEntry = $"{(int)GameID}";
            }

            Globals.Game.EXT_AddMiscText("Menus", "Perks_Name_" + textEntry, name);
            Globals.Game.EXT_AddMiscText("Menus", "Perks_Description_" + textEntry, description);

            // Texture on demand
        }

        internal override void Cleanup()
        {
            Globals.Game.EXT_RemoveMiscText("Menus", "Perks_Name_" + textEntry);
            Globals.Game.EXT_RemoveMiscText("Menus", "Perks_Description_" + textEntry);

            if (ModUtils.IsModContentPath(texturePath))
            {
                AssetUtils.UnloadAsset(Globals.Game.Content, texturePath);
            }
        }
    }
}
