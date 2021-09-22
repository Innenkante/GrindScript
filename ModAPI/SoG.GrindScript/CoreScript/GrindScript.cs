using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SoG.Modding.API;
using SoG.Modding.Core;
using SoG.Modding.ModUtils;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SoG.Modding.CoreScript
{
    internal class GrindScript : Mod
    {
        public class ModSaveData
        {
            public List<string> ModsSaved = new List<string>();
        }

        public override string Name => "GrindScript";

        public Texture2D ErrorTexture { get; private set; }

        private Dictionary<int, ModSaveData> _modSaves = new Dictionary<int, ModSaveData>();

        private ModSaveData _arcadeSave;

        public void AnalyzeStorySavesForCompatibility()
        {
            for (int index = 0; index < Globals.Game.xGlobalData.lxCharacterSaves.Count; index++)
            {
                var save = Globals.Game.xGlobalData.lxCharacterSaves[index];

                if (save.bIncompatibleSave || save.sCharacterName == "")
                {
                    continue;
                }

                // TODO: Rewrite this crap

                string appData = Globals.Game.sAppData;

                string path = $"{appData}Characters/" + $"{index}.cha{ModSaving.SaveFileExtension}";

                _modSaves[index] = new ModSaveData();

                if (File.Exists(path))
                {
                    using (BinaryReader stream = new BinaryReader(new FileStream(path, FileMode.Open, FileAccess.Read)))
                    {
                        var modMeta = Globals.API.Saving.PeekGrindScriptData(stream);

                        _modSaves[index].ModsSaved = modMeta.Select(x => x.Name).ToList();
                    }
                }
            }
        }

        public void AnalyzeArcadeSavesForCompatibility()
        {
            if (RogueLikeMode.LockedOutDueToHigherVersionSaveFile)
            {
                _arcadeSave = null;
                return;
            }

            string appData = Globals.Game.sAppData;

            string path = appData + $"arcademode.sav{ModSaving.SaveFileExtension}";

            _arcadeSave = new ModSaveData();

            if (File.Exists(path))
            {
                using (BinaryReader stream = new BinaryReader(new FileStream(path, FileMode.Open, FileAccess.Read)))
                {
                    var modMeta = Globals.API.Saving.PeekGrindScriptData(stream);

                    _arcadeSave.ModsSaved = modMeta.Select(x => x.Name).ToList();
                }
            }
        }

        public void CheckStorySaveCompatibility()
        {
            var menuData = Globals.Game.xGlobalData.xMainMenuData;

            int slot = menuData.iSelectedChar + menuData.iCurrentCharSelectPage * 3;
            float alpha = menuData.fCurrentMenuAlpha;

            if (slot < 0 || slot > 8)
            {
                return;
            }

            if (!_modSaves.ContainsKey(slot))
            {
                return;
            }

            List<string> loadedMods = Globals.API.Loader.Mods.Select(x => x.Name).ToList();
            List<string> saveMods = _modSaves[slot].ModsSaved;

            List<string> missingMods = saveMods.Where(x => !loadedMods.Contains(x)).ToList();
            List<string> newMods = loadedMods.Where(x => !saveMods.Contains(x)).ToList();

            RenderSaveCompatibility(missingMods, newMods, 444, 90 + 65);
        }

        public void CheckArcadeSaveCompatiblity()
        {
            var menuData = Globals.Game.xGlobalData.xMainMenuData;

            if (Globals.Game.xGlobalData.xMainMenuData.iTopMenuSelection != 1)
            {
                return;
            }

            if (_arcadeSave == null)
            {
                return;
            }

            List<string> loadedMods = Globals.API.Loader.Mods.Select(x => x.Name).ToList();
            List<string> saveMods = _arcadeSave.ModsSaved;

            List<string> missingMods = saveMods.Where(x => !loadedMods.Contains(x)).ToList();
            List<string> newMods = loadedMods.Where(x => !saveMods.Contains(x)).ToList();

            RenderSaveCompatibility(missingMods, newMods, 422, 243);
        }

        public void RenderSaveCompatibility(List<string> missingMods, List<string> newMods, int x, int y)
        {
            float alpha = Globals.Game.xGlobalData.xMainMenuData.fCurrentMenuAlpha;

            string message;

            if (missingMods.Count == 0 && newMods.Count == 0)
            {
                message = "Loading is OK!";
            }
            else
            {
                message = "Loading may cause issues!";

                if (missingMods.Count > 0)
                {
                    message += "\n" + "Missing mods:";

                    foreach (var name in missingMods)
                    {
                        message += "\n" + "  " + name;
                    }
                }

                if (newMods.Count > 0)
                {
                    message += "\n" + "New mods:";

                    foreach (var name in newMods)
                    {
                        message += "\n" + "  " + name;
                    }
                }
            }

            Vector2 measure = FontManager.GetFont(FontManager.FontType.Reg7).MeasureString(message);

            Globals.Game._Menu_RenderNotice(Globals.SpriteBatch, 1f, new Rectangle(x - 4, y - (int)measure.Y / 2 - 4, (int)measure.X + 8, (int)measure.Y + 8), false);

            Globals.SpriteBatch.DrawString(FontManager.GetFont(FontManager.FontType.Reg7), message, new Vector2(x, y - (int)measure.Y / 2), Color.White * alpha);
        }

        public override void PostLevelLoad(Level.ZoneEnum level, Level.WorldRegion region, bool staticOnly)
        {
            if (_colliderRCActive)
            {
                RenderMaster render = Globals.Game.xRenderMaster;

                render.UnregisterRenderComponenent(_colliderRC);
                render.RegisterComponent(RenderMaster.SubRenderLayer.AboveSorted, _colliderRC);
            }
        }

        public override void OnDraw()
        {

        }

        public override void Load()
        {
            _colliderRC = new ColliderRC();

            Utils.TryLoadTex(Path.Combine(AssetPath, "NullTexGS"), Globals.Game.Content, out Texture2D errorTexture);

            ErrorTexture = errorTexture;

            Dictionary<string, CommandParser> commands = new Dictionary<string, CommandParser>
            {
                [nameof(ModList)] = ModList,
                [nameof(Help)] = Help,
                [nameof(PlayerPos)] = PlayerPos,
                [nameof(ModTotals)] = ModTotals,
                [nameof(RenderColliders)] = RenderColliders,
                [nameof(Version)] = Version,
            };

            CreateCommands(commands);
        }

        private ColliderRC _colliderRC;

        private bool _colliderRCActive = false;

        #region Commands

        private void Version(string message, int connection)
        {
            CAS.AddChatMessage(
                "Short Version: " + Globals.GameShortVersion + "\n" +
                "Long Version: " + Globals.GameLongVersion + "\n" +
                "Vanilla Version: " + Globals.GameVanillaVersion
                );
        }

        private void Help(string message, int connection)
        {
            Dictionary<string, CommandParser> commandList;

            string[] args = Utils.GetArgs(message);

            if (args.Length == 0)
            {
                commandList = this.ModCommands;
            }
            else
            {
                Mod mod = Globals.API.Loader.Mods.FirstOrDefault(x => x.Name == args[0]);

                if (mod == null)
                {
                    CAS.AddChatMessage($"[{Name}] Unknown mod!");
                    return;
                }

                commandList = mod.ModCommands;
            }

            CAS.AddChatMessage($"[{Name}] Command list{(args.Length == 0 ? "" : $" for {args[0]}")}:");

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

        private void ModList(string message, int connection)
        {
            CAS.AddChatMessage($"[{Name}] Mod Count: {Globals.API.Loader.Mods.Count}");

            var messages = new List<string>();
            var concated = "";
            foreach (var mod in Globals.API.Loader.Mods)
            {
                string name = mod.Name;
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

        private void PlayerPos(string message, int connection)
        {
            var local = Globals.Game.xLocalPlayer.xEntity.xTransform.v2Pos;

            CAS.AddChatMessage($"[{Name}] Player position: {(int)local.X}, {(int)local.Y}");
        }

        private void ModTotals(string message, int connection)
        {
            string[] args = Utils.GetArgs(message);
            if (args.Length != 1)
            {
                CAS.AddChatMessage($"[{Name}] Usage: /{Name}:{nameof(ModTotals)} <unique type>");
                return;
            }

            switch (args[0])
            {
                case "Items":
                    CAS.AddChatMessage($"[{Name}] Items defined: " + Globals.API.Loader.Library.Items.Count);
                    break;
                case "Perks":
                    CAS.AddChatMessage($"[{Name}] Perks defined: " + Globals.API.Loader.Library.Perks.Count);
                    break;
                case "Treats":
                case "Curses":
                    CAS.AddChatMessage($"[{Name}] Treats and Curses defined: " + Globals.API.Loader.Library.Curses.Count);
                    break;
                default:
                    CAS.AddChatMessage($"[{Name}] Usage: /{Name}:{nameof(ModTotals)} <unique type>");
                    break;
            }
        }

        private void RenderColliders(string message, int connection)
        {
            List<string> args = Utils.GetArgs(message).ToList();

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
