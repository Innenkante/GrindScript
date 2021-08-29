using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SoG.Modding.Utils;

namespace SoG.Modding.Core
{
    /// <summary>
    /// Prepares chat commands that come bundled with GrindScript.
    /// </summary>
    internal static class GSCommands
    {
        /// <summary> 
        /// Acts as the "mod name" when using GrindScript commands 
        /// </summary>
        public const string APIName = "GrindScript";

        /// <summary>
        /// Builds and returns a dictionary of commands for the API.
        /// </summary>
        public static Dictionary<string, CommandParser> GetCommands()
        {
            return new Dictionary<string, CommandParser>
            {
                [nameof(ModList)] = ModList,
                [nameof(Help)] = Help,
                [nameof(PlayerPos)] = PlayerPos,
                [nameof(ModTotals)] = ModTotals
            };
        }

        private static void Help(string message, int connection)
        {
            Dictionary<string, CommandParser> commandList;

            string[] args = Tools.GetArgs(message);
            if (args.Length == 0)
            {
                commandList = Globals.API.Registry.Library.Commands[APIName];
            }
            else if (!Globals.API.Registry.Library.Commands.TryGetValue(args[0], out commandList))
            {
                CAS.AddChatMessage($"[{APIName}] Unknown mod!");
                return;
            }

            CAS.AddChatMessage($"[{APIName}] Command list{(args.Length == 0 ? "" : $" for {args[0]}")}:");

            var messages = new List<string>();
            var concated = "";
            foreach (var cmd in commandList.Keys)
            {
                if (concated.Length + cmd.Length > 40)
                {
                    messages.Add(concated);
                    concated = "";
                }
                concated += cmd + " ";
            }
            if (concated != "")
                messages.Add(concated);

            foreach (var line in messages)
                CAS.AddChatMessage(line);
        }

        private static void ModList(string message, int connection)
        {
            CAS.AddChatMessage($"[{APIName}] Mod Count: {Globals.API.Registry.LoadedMods.Count}");

            var messages = new List<string>();
            var concated = "";
            foreach (var mod in Globals.API.Registry.LoadedMods)
            {
                string name = mod.GetType().Name;
                if (concated.Length + name.Length > 40)
                {
                    messages.Add(concated);
                    concated = "";
                }
                concated += mod.GetType().Name + " ";
            }
            if (concated != "")
                messages.Add(concated);

            foreach (var line in messages)
                CAS.AddChatMessage(line);
        }

        private static void PlayerPos(string message, int connection)
        {
            var local = Globals.Game.xLocalPlayer.xEntity.xTransform.v2Pos;

            CAS.AddChatMessage($"[{APIName}] Player position: {(int)local.X}, {(int)local.Y}");
        }

        private static void ModTotals(string message, int connection)
        {
            string[] args = Tools.GetArgs(message);
            if (args.Length != 1)
            {
                CAS.AddChatMessage($"[{APIName}] Usage: /{APIName}:{nameof(ModTotals)} <unique type>");
                return;
            }

            switch (args[0])
            {
                case "Items":
                    CAS.AddChatMessage($"[{APIName}] Items defined: " + Globals.API.Registry.Library.Items.Count);
                    break;
                case "Perks":
                    CAS.AddChatMessage($"[{APIName}] Perks defined: " + Globals.API.Registry.Library.Perks.Count);
                    break;
                case "Treats":
                case "Curses":
                    CAS.AddChatMessage($"[{APIName}] Treats and Curses defined: " + Globals.API.Registry.Library.Curses.Count);
                    break;
                default:
                    CAS.AddChatMessage($"[{APIName}] Usage: /{APIName}:{nameof(ModTotals)} <unique type>");
                    break;
            }
        }
    }
}
