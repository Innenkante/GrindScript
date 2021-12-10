using Microsoft.Xna.Framework.Graphics;
using SoG.Modding.Content;
using SoG.Modding.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SoG.Modding.GrindScriptMod
{
    internal class GrindScript : Mod
    {
        public override string NameID => "GrindScript";

        public override Version ModVersion => new Version("0.16");

        public Texture2D ErrorTexture { get; private set; }

        public Texture2D ModMenuText { get; private set; }

        public Texture2D ModListText { get; private set; }

        public Texture2D ReloadModsText { get; private set; }

        public override void PostLevelLoad(Level.ZoneEnum level, Level.WorldRegion region, bool staticOnly)
        {
            if (_colliderRCActive)
            {
                RenderMaster render = Globals.Game.xRenderMaster;

                render.UnregisterRenderComponenent(_colliderRC);
                render.RegisterComponent(RenderMaster.SubRenderLayer.AboveSorted, _colliderRC);
            }
        }

        public override void Load()
        {
            _colliderRC = new ColliderRC();

            AssetUtils.TryLoadTexture(Path.Combine(AssetPath, "NullTexGS"), Globals.Game.Content, out Texture2D tex);
            ErrorTexture = tex;

            AssetUtils.TryLoadTexture(Path.Combine(AssetPath, "ModMenu"), Globals.Game.Content, out tex);
            ModMenuText = tex;

            AssetUtils.TryLoadTexture(Path.Combine(AssetPath, "ModList"), Globals.Game.Content, out tex);
            ModListText = tex;

            AssetUtils.TryLoadTexture(Path.Combine(AssetPath, "ReloadMods"), Globals.Game.Content, out tex);
            ReloadModsText = tex;

            Dictionary<string, CommandParser> commandList = new Dictionary<string, CommandParser>
            {
                [nameof(ModList)] = ModList,
                [nameof(Help)] = Help,
                [nameof(PlayerPos)] = PlayerPos,
                [nameof(ModTotals)] = ModTotals,
                [nameof(RenderColliders)] = RenderColliders,
                [nameof(Version)] = Version,
            };

            CommandEntry commands = CreateCommands();

            foreach (var command in commandList)
            {
                commands.SetCommand(command.Key, command.Value);
            }
        }

        public override void Unload()
        {
            _colliderRC = null;

            AssetUtils.UnloadAsset(Globals.Game.Content, Path.Combine(AssetPath, "NullTexGS"));
            ErrorTexture = null;

            AssetUtils.UnloadAsset(Globals.Game.Content, Path.Combine(AssetPath, "ModMenu"));
            ModMenuText = null;

            AssetUtils.UnloadAsset(Globals.Game.Content, Path.Combine(AssetPath, "ModList"));
            ModListText = null;

            AssetUtils.UnloadAsset(Globals.Game.Content, Path.Combine(AssetPath, "ReloadMods"));
            ReloadModsText = null;
        }

        private ColliderRC _colliderRC;

        private bool _colliderRCActive = false;

        #region Commands

        private void Version(string[] args, int connection)
        {
            CAS.AddChatMessage(
                "SoG Version: " + Globals.GameLongVersion + "\n" +
                "GrindScript Version:" + ModVersion.ToString()
                );
        }

        private void Help(string[] args, int connection)
        {
            List<string> commandList;

            if (args.Length == 0)
            {
                commandList = GetCommands().GetCommandList();
            }
            else
            {
                Mod mod = Globals.ModManager.ActiveMods.FirstOrDefault(x => x.NameID == args[0]);

                if (mod == null)
                {
                    CAS.AddChatMessage($"[{NameID}] Unknown mod!");
                    return;
                }

                commandList = mod.GetCommands()?.GetCommandList() ?? new List<string>();
            }

            if (commandList.Count == 0)
            {
                CAS.AddChatMessage($"[{NameID}] No commands are defined for this mod!");
                return;
            }

            CAS.AddChatMessage($"[{NameID}] Command list{(args.Length == 0 ? "" : $" for {args[0]}")}:");

            var messages = new List<string>();
            var concated = "";
            foreach (var cmd in commandList)
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

        private void ModList(string[] args, int connection)
        {
            CAS.AddChatMessage($"[{NameID}] Mod Count: {Globals.ModManager.ActiveMods.Count}");

            var messages = new List<string>();
            var concated = "";
            foreach (var mod in Globals.ModManager.ActiveMods)
            {
                string name = mod.NameID;
                if (concated.Length + name.Length > 40)
                {
                    messages.Add(concated);
                    concated = "";
                }
                concated += name + " ";
            }
            if (concated != "")
                messages.Add(concated);

            foreach (var line in messages)
                CAS.AddChatMessage(line);
        }

        private void PlayerPos(string[] args, int connection)
        {
            var local = Globals.Game.xLocalPlayer.xEntity.xTransform.v2Pos;

            CAS.AddChatMessage($"[{NameID}] Player position: {(int)local.X}, {(int)local.Y}");
        }

        private void ModTotals(string[] args, int connection)
        {
            if (args.Length != 1)
            {
                CAS.AddChatMessage($"[{NameID}] Usage: /{NameID}:{nameof(ModTotals)} <unique type>");
                return;
            }

            switch (args[0])
            {
                case "Items":
                    CAS.AddChatMessage($"[{NameID}] Items defined: " + Globals.ModManager.Library.GetStorage<ItemCodex.ItemTypes, ItemEntry>().Count);
                    break;
                case "Perks":
                    CAS.AddChatMessage($"[{NameID}] Perks defined: " + Globals.ModManager.Library.GetStorage<RogueLikeMode.Perks, PerkEntry>().Count);
                    break;
                case "Treats":
                case "Curses":
                    CAS.AddChatMessage($"[{NameID}] Treats and Curses defined: " + Globals.ModManager.Library.GetStorage<RogueLikeMode.TreatsCurses, CurseEntry>().Count);
                    break;
                default:
                    CAS.AddChatMessage($"[{NameID}] Usage: /{NameID}:{nameof(ModTotals)} <unique type>");
                    break;
            }
        }

        private void RenderColliders(string[] args, int connection)
        {
            _colliderRC.RenderCombat = args.Contains("-c");
            _colliderRC.RenderLevel = args.Contains("-l");
            _colliderRC.RenderMovement = args.Contains("-m");

            _colliderRCActive = _colliderRC.RenderCombat || _colliderRC.RenderLevel || _colliderRC.RenderMovement;

            RenderMaster render = Globals.Game.xRenderMaster;

            render.UnregisterRenderComponenent(_colliderRC);

            if (_colliderRCActive)
            {
                render.RegisterComponent(RenderMaster.SubRenderLayer.AboveSorted, _colliderRC);
                string msg = "Collider rendering enabled for ";
                msg += _colliderRC.RenderCombat ? "Combat, " : "";
                msg += _colliderRC.RenderLevel ? "Level, " : "";
                msg += _colliderRC.RenderMovement ? "Movement, " : "";

                msg = msg.Remove(msg.Length - 2, 2);

                CAS.AddChatMessage(msg);
            }
            else
            {
                CAS.AddChatMessage("Collider rendering disabled.");
            }
        }

        #endregion
    }
}
