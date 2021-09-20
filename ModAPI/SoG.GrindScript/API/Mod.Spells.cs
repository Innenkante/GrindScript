using SoG.Modding.API.Configs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SoG.Modding.Core;
using SoG.Modding.ModUtils;

namespace SoG.Modding.API
{
    public abstract partial class Mod
    {
        public SpellCodex.SpellTypes CreateSpell(SpellConfig config)
        {
            if (config == null)
            {
                throw new ArgumentNullException(nameof(config));
            }

            Mod mod = Registry.LoadContext;

            if (mod == null)
            {
                Globals.Logger.Error("Can not create objects outside of a load context.", source: nameof(CreateSpell));
                return SpellCodex.SpellTypes.NULL;
            }

            SpellCodex.SpellTypes gameID = Registry.ID.SpellIDNext++;

            Registry.Library.Spells[gameID] = new ModSpellEntry(mod, gameID, config.ModID)
            {
                Config = config.DeepCopy()
            };

            return gameID;
        }
    }
}
