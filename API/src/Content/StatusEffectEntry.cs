using SoG.Modding.Utils;

namespace SoG.Modding.Content
{
    /// <summary>
    /// Represents a modded status effect.
    /// </summary>
    /// <remarks> 
    /// Most of the methods in this class can only be used while a mod is loading, that is, inside <see cref="Mod.Load"/>.
    /// </remarks>
    public class StatusEffectEntry : Entry<BaseStats.StatusEffectSource>
    {
        #region IEntry Properties

        public override Mod Mod { get; internal set; }

        public override BaseStats.StatusEffectSource GameID { get; internal set; }

        public override string ModID { get; internal set; }

        #endregion

        #region Internal Data

        internal string texturePath = "";

        #endregion

        #region Public Interface

        /// <summary>
        /// Gets or sets the icon's texture path. The texture path is relative to "Config/".
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

        #endregion

        internal StatusEffectEntry()
        {
        }

        internal override void Initialize()
        {
            // Nothing, texture is loaded on demand
        }

        internal override void Cleanup()
        {
            if (ModUtils.IsModContentPath(texturePath))
            {
                AssetUtils.UnloadAsset(Globals.Game.Content, texturePath);
            }
        }
    }
}
