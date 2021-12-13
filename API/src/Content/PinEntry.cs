using System;

namespace SoG.Modding.Content
{
    /// <summary>
    /// Represents a modded pin from Arcade Mode.
    /// </summary>
    /// <remarks> 
    /// Most of the methods in this class can only be used while a mod is loading, that is, inside <see cref="Mod.Load"/>.
    /// </remarks>
    public class PinEntry : Entry<PinCodex.PinType>
    {
        /// <summary>
        /// Enumerates the available pin symbols.
        /// </summary>
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

        /// <summary>
        /// Enumerates the available pin shapes.
        /// </summary>
        public enum Shape
        {
            Circle = 0,
            Square = 1,
            Plus = 2,
            Tablet = 3,
            Diamond = 4
        }

        /// <summary>
        /// Enumerates the available pin colors.
        /// </summary>
        public enum Color
        {
            YellowOrange = 0,
            Seagull = 1,
            Coral = 2,
            Conifer = 3,
            BilobaFlower = 4,

            /// <summary>
            /// This color usually represents sticky pins.
            /// </summary>
            White = 1337,
        }

        #region IEntry Properties

        public override Mod Mod { get; internal set; }

        public override PinCodex.PinType GameID { get; internal set; }

        public override string ModID { get; internal set; }

        #endregion

        #region Internal Data

        internal Symbol pinSymbol = Symbol.Star;

        internal Shape pinShape = Shape.Square;

        internal Color pinColor = Color.Seagull;

        internal bool isSticky = false;

        internal bool isBroken = false;

        internal string description = "Some modded pin that isn't very descriptive!";

        internal Func<bool> conditionToDrop = null;

        internal Action<PlayerView> equipAction = null;

        internal Action<PlayerView> unequipAction = null;

        internal bool createCollectionEntry = true;

        #endregion

        #region Public Interface

        /// <summary>
        /// Gets or sets the pin's symbol.
        /// </summary>
        public Symbol PinSymbol
        {
            get => pinSymbol;
            set
            {
                ErrorHelper.ThrowIfNotLoading(Mod);
                pinSymbol = value;
            }
        }

        /// <summary>
        /// Gets or sets the pin's shape.
        /// </summary>
        public Shape PinShape
        {
            get => pinShape;
            set
            {
                ErrorHelper.ThrowIfNotLoading(Mod);
                pinShape = value;
            }
        }

        /// <summary>
        /// Gets or sets the pin's color.
        /// </summary>
        public Color PinColor
        {
            get => pinColor;
            set
            {
                ErrorHelper.ThrowIfNotLoading(Mod);
                pinColor = value;
            }
        }

        /// <summary>
        /// Gets or sets whenever this pin is sticky. <para/>
        /// Sticky pins appear as white, and cannot be unequipped.
        /// </summary>
        public bool IsSticky
        {
            get => isSticky;
            set
            {
                ErrorHelper.ThrowIfNotLoading(Mod);
                isSticky = value;
            }
        }

        /// <summary>
        /// Gets or sets whenever this pin is broken. <para/>
        /// Broken pins have a cracked appearance.
        /// </summary>
        public bool IsBroken
        {
            get => isBroken;
            set
            {
                ErrorHelper.ThrowIfNotLoading(Mod);
                isBroken = value;
            }
        }

        /// <summary>
        /// Gets or sets the pin's in-game description.
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
        /// Gets or sets the condition for the pin to drop. <para/>
        /// If the method is not empty, and its return value is false,
        /// then pins of this type will be skipped when selecting a pin drop.
        /// For example, in vanilla, the three red smash balls pin can drop only if you have a two handed weapon equipped.
        /// </summary>
        public Func<bool> ConditionToDrop
        {
            get => conditionToDrop;
            set
            {
                ErrorHelper.ThrowIfNotLoading(Mod);
                conditionToDrop = value;
            }
        }

        /// <summary>
        /// Gets or sets the action to run when this pin is equipped.
        /// </summary>
        public Action<PlayerView> EquipAction
        {
            get => equipAction;
            set
            {
                ErrorHelper.ThrowIfNotLoading(Mod);
                equipAction = value;
            }
        }

        /// <summary>
        /// Gets or sets the action to run when this pin is unequipped.
        /// </summary>
        public Action<PlayerView> UnequipAction
        {
            get => unequipAction;
            set
            {
                ErrorHelper.ThrowIfNotLoading(Mod);
                unequipAction = value;
            }
        }

        /// <summary>
        /// Gets or sets whenever to create a pin display in Traveller's pin collection.
        /// This is set to true by default.
        /// </summary>
        public bool CreateCollectionEntry
        {
            get => createCollectionEntry;
            set
            {
                ErrorHelper.ThrowIfNotLoading(Mod);
                createCollectionEntry = value;
            }
        }

        #endregion

        internal PinEntry() { }

        internal PinEntry(Mod owner, PinCodex.PinType gameID, string modID)
        {
            Mod = owner;
            GameID = gameID;
            ModID = modID;
        }

        internal override void Initialize()
        {
            if (createCollectionEntry)
            {
                PinCodex.SortedPinEntries.Add(GameID);
            }
        }

        internal override void Cleanup()
        {
            if (createCollectionEntry)
            {
                PinCodex.SortedPinEntries.Remove(GameID);
            }
        }
    }
}
