using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Audio;
using SoG.Modding.Core;
using System.Diagnostics;
using SoG.Modding.ModUtils;
using SoG.Modding.API.Configs;

namespace SoG.Modding.API
{
    public abstract partial class Mod
    {
        public RogueLikeMode.TreatsCurses CreateTreatOrCurse(TreatCurseConfig config)
        {
            if (config == null)
            {
                throw new ArgumentNullException(nameof(config));
            }

            ModLoader registry = Registry;

            Mod mod = registry.LoadContext;

            if (mod != this)
            {
                Logger.Error("Can not create objects outside of a load context.", source: nameof(CreateTreatOrCurse));
                return RogueLikeMode.TreatsCurses.None;
            }

            if (GetLibrary().Curses.Values.Any(x => x.ModID == config.ModID))
            {
                Globals.Logger.Error($"A treat or curse with the ModID {config.ModID} already exists.", source: nameof(CreateTreatOrCurse));
                return RogueLikeMode.TreatsCurses.None;
            }

            RogueLikeMode.TreatsCurses gameID = registry.ID.CurseIDNext++;

            ModCurseEntry entry = new ModCurseEntry(mod, gameID, config.ModID)
            {
                Config = config.DeepCopy(),
                NameHandle = $"TreatCurse_{(int)gameID}_Name",
                DescriptionHandle = $"TreatCurse_{(int)gameID}_Description"
            };

            registry.Library.Curses[gameID] = entry;

            Globals.Game.EXT_AddMiscText("Menus", entry.NameHandle, config.Name);
            Globals.Game.EXT_AddMiscText("Menus", entry.DescriptionHandle, config.Description);

            return gameID;
        }

        public RogueLikeMode.Perks CreatePerk(PerkConfig config)
        {
            if (config == null)
            {
                throw new ArgumentNullException(nameof(config));
            }

            ModLoader registry = Registry;

            Mod mod = registry.LoadContext;

            if (mod != this)
            {
                Globals.Logger.Error("Can not create objects outside of a load context.", source: nameof(CreatePerk));
                return RogueLikeMode.Perks.None;
            }

            if (GetLibrary().Perks.Values.Any(x => x.ModID == config.ModID))
            {
                Globals.Logger.Error($"A perk with ModID {config.ModID} already exists.", source: nameof(CreatePerk));
                return RogueLikeMode.Perks.None;
            }

            RogueLikeMode.Perks gameID = registry.ID.PerkIDNext++;

            ModPerkEntry entry = new ModPerkEntry(mod, gameID, config.ModID)
            {
                Config = config.DeepCopy(),
                TextEntry = $"{(int)gameID}"
            };

            registry.Library.Perks[gameID] = entry;

            Globals.Game.EXT_AddMiscText("Menus", "Perks_Name_" + entry.TextEntry, config.Name);
            Globals.Game.EXT_AddMiscText("Menus", "Perks_Description_" + entry.TextEntry, config.Description);

            return gameID;
        }
    }
}
