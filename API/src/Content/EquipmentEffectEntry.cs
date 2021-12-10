using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoG.Modding.Content
{
    /// <summary>
    /// Represents a modded equipment special effect.
    /// </summary>
    /// <remarks> 
    /// Most of the methods in this class can only be used while a mod is loading, that is, inside <see cref="Mod.Load"/>.
    /// </remarks>
    public class EquipmentEffectEntry : Entry<EquipmentInfo.SpecialEffect>
    {
        #region IEntry Properties

        public override Mod Mod { get; internal set; }

        public override EquipmentInfo.SpecialEffect GameID { get; internal set; }

        public override string ModID { get; internal set; }

        #endregion

        #region Internal Data

        internal Action<PlayerView> _onEquip;

        internal Action<PlayerView> _onRemove;

        #endregion

        #region Public Inteface

        /// <summary>
        /// Gets or sets the callback that will be called when an equipment with this effect is worn.
        /// You can use this callback to do non-stat things such as creating persistent spells.
        /// </summary>
        public Action<PlayerView> OnEquip
        {
            get => _onEquip;
            set
            {
                ErrorHelper.ThrowIfNotLoading(Mod);
                _onEquip = value;
            }
        }

        /// <summary>
        /// Gets or sets the callback that will be called when an equipment with this effect is worn.
        /// You can use this callback to undo your actions in OnEquip.
        /// </summary>
        public Action<PlayerView> OnRemove
        {
            get => _onRemove;
            set
            {
                ErrorHelper.ThrowIfNotLoading(Mod);
                _onRemove = value;
            }
        }

        #endregion

        internal EquipmentEffectEntry()
        {

        }

        internal override void Cleanup()
        {
            // Nothing for now
        }

        internal override void Initialize()
        {
            // Nothing for now
        }
    }
}
