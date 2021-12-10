namespace SoG.Modding.Content
{
    /// <summary>
    /// Represents a modded entity of type ISpellInstance.
    /// Some spells can act as player spells, and have additional information associated with them.
    /// </summary>
    /// <remarks> 
    /// Most of the methods in this class can only be used while a mod is loading, that is, inside <see cref="Mod.Load"/>.
    /// </remarks>
    public class SpellEntry : Entry<SpellCodex.SpellTypes>
    {
        #region IEntry Properties

        public override Mod Mod { get; internal set; }

        public override SpellCodex.SpellTypes GameID { get; internal set; }

        public override string ModID { get; internal set; }

        #endregion

        #region Internal Data

        internal SpellBuilder builder = null;

        internal bool isMagicSkill = false;

        internal bool isUtilitySkill = false;

        internal bool isMeleeSkill = false;

        #endregion

        #region Public Interface
        
        /// <summary>
        /// Gets or sets the builder of the spell instance.
        /// The builder is called when an instance of this spell must be made.
        /// Use this to create a subclass of ISpellInstance, initialize it, and return it.
        /// </summary>
        public SpellBuilder Builder
        {
            get => builder;
            set
            {
                ErrorHelper.ThrowIfNotLoading(Mod);
                builder = value;
            }
        }

        /// <summary>
        /// Gets or sets whenever this spell is a player magical skill.
        /// </summary>
        public bool IsMagicSkill
        {
            get => isMagicSkill;
            set
            {
                ErrorHelper.ThrowIfNotLoading(Mod);
                isMagicSkill = value;
            }
        }

        /// <summary>
        /// Gets or sets whenever this spell is an player utility skill.
        /// </summary>
        public bool IsUtilitySkill
        {
            get => isUtilitySkill;
            set
            {
                ErrorHelper.ThrowIfNotLoading(Mod);
                isUtilitySkill = value;
            }
        }

        /// <summary>
        /// Gets or sets whenever this spell is a player melee skill.
        /// </summary>
        public bool IsMeleeSkill
        {
            get => isMeleeSkill;
            set
            {
                ErrorHelper.ThrowIfNotLoading(Mod);
                isMeleeSkill = value;
            }
        }

        #endregion

        internal SpellEntry() { }

        internal SpellEntry(Mod owner, SpellCodex.SpellTypes gameID, string modID)
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
