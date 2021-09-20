using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SoG.Modding.API.Configs;
using SoG.Modding.Core;

namespace SoG.Modding.API
{
    public abstract partial class Mod
    {
        public BaseStats.StatusEffectSource CreateStatusEffect(StatusEffectConfig config)
        {
            if (config == null)
            {
                throw new ArgumentNullException(nameof(config));
            }

            Mod mod = Registry.LoadContext;

            if (mod == null)
            {
                Globals.Logger.Error("Can not create objects outside of a load context.", source: nameof(CreateSpell));
                return BaseStats.StatusEffectSource.SlowLv1;
            }

            BaseStats.StatusEffectSource gameID = Registry.ID.StatusEffectIDNext++;

            Registry.Library.StatusEffects[gameID] = new ModStatusEffectEntry(mod, gameID, config.ModID)
            {
                Config = config.DeepCopy()
            };

            return gameID;
        }
    }
}
