using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SoG.Modding.Patching;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SoG.Modding.GrindScriptMod
{
    /// <summary>
    /// Handles callbacks for Main Menu patches.
    /// This includes save mod compatibility information, and the mod menu.
    /// </summary>
    internal class MainMenuWorker
    {
        private class ModMenu
        {
            private int _selection = 0;
            public int Selection
            {
                get => _selection;
                set => _selection = Math.Min(Math.Max(value, MinSelection), MaxSelection);
            }

            public readonly int MinSelection = 0;

            public readonly int MaxSelection = 1;
        }

        public class ModSaveData
        {
            public List<ModMetadata> ModMetaList = new List<ModMetadata>();
        }

        public static readonly GlobalData.MainMenu.MenuLevel ReservedModMenuID = (GlobalData.MainMenu.MenuLevel)300;

        private Dictionary<int, ModSaveData> _modSaves = new Dictionary<int, ModSaveData>();

        private ModSaveData _arcadeSave;

        private ModMenu _modMenu = new ModMenu();

        private int _previousTopMenuSelection;

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
                        var saveData = Globals.ModManager.Saving.PeekMetadata(stream);

                        _modSaves[index].ModMetaList.AddRange(saveData.Mods);
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
                    var saveData = Globals.ModManager.Saving.PeekMetadata(stream);

                    _arcadeSave.ModMetaList.AddRange(saveData.Mods);
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

            List<ModMetadata> loadedMods = Globals.ModManager.ActiveMods.Select(x => new ModMetadata(x)).ToList();
            List<ModMetadata> saveMods = _modSaves[slot].ModMetaList;

            List<ModMetadata> missingMods = saveMods.Where(x => !loadedMods.Any(y => y.NameID == x.NameID)).ToList();
            List<ModMetadata> newMods = loadedMods.Where(x => !saveMods.Any(y => y.NameID == x.NameID)).ToList();

            RenderMessage(GetSaveCompatibiltyText(missingMods, newMods), 444, 90 + 65);
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

            List<ModMetadata> loadedMods = Globals.ModManager.ActiveMods.Select(x => new ModMetadata(x)).ToList();
            List<ModMetadata> saveMods = _arcadeSave.ModMetaList;

            List<ModMetadata> missingMods = saveMods.Where(x => !loadedMods.Any(y => y.NameID == x.NameID)).ToList();
            List<ModMetadata> newMods = loadedMods.Where(x => !saveMods.Any(y => y.NameID == x.NameID)).ToList();

            RenderMessage(GetSaveCompatibiltyText(missingMods, newMods), 422, 243);
        }

        private string GetSaveCompatibiltyText(List<ModMetadata> missingMods, List<ModMetadata> newMods)
        {
            string message;

            if (missingMods.Count == 0 && newMods.Count == 0)
            {
                message = "Loading is OK!\nSave mod list is compatible.";
            }
            else
            {
                message = "Loading may cause issues!";

                if (missingMods.Count > 0)
                {
                    message += "\n" + "Missing mods:";

                    foreach (var meta in missingMods)
                    {
                        message += "\n" + "  " + meta.NameID + " v." + (meta.ModVersion?.ToString() ?? "Unknown");
                    }
                }

                if (newMods.Count > 0)
                {
                    message += "\n" + "New mods:";

                    foreach (var meta in newMods)
                    {
                        message += "\n" + "  " + meta.NameID + " v." + (meta.ModVersion?.ToString() ?? "Unknown");
                    }
                }
            }

            return message;
        }

        public void RenderMessage(string message, int x, int y)
        {
            float alpha = Globals.Game.xGlobalData.xMainMenuData.fCurrentMenuAlpha;

            Vector2 measure = FontManager.GetFont(FontManager.FontType.Reg7).MeasureString(message);

            Globals.Game._Menu_RenderNotice(Globals.SpriteBatch, 1f, new Rectangle(x - 4, y - (int)measure.Y / 2 - 4, (int)measure.X + 8, (int)measure.Y + 8), false);

            Globals.SpriteBatch.DrawString(FontManager.GetFont(FontManager.FontType.Reg7), message, new Vector2(x, y - (int)measure.Y / 2), Color.White * alpha);
        }


        public void PostTopMenuInterface()
        {
            var menuData = Globals.Game.xGlobalData.xMainMenuData;
            var inputData = Globals.Game.xInput_Menu;
            var audio = Globals.Game.xSoundSystem;

            if (inputData.Left.bPressed && menuData.iTopMenuSelection != 4)
            {
                _previousTopMenuSelection = menuData.iTopMenuSelection;
                menuData.iTopMenuSelection = 4;
                audio.PlayInterfaceCue("Menu_Move");
            }
            else if (inputData.Right.bPressed && menuData.iTopMenuSelection == 4)
            {
                menuData.iTopMenuSelection = _previousTopMenuSelection;
                audio.PlayInterfaceCue("Menu_Move");
            }

            if (inputData.Action.bPressed && menuData.iTopMenuSelection == 4)
            {
                audio.PlayInterfaceCue("Menu_Changed");

                // TODO: Lol
                Globals.Game.xGlobalData.xMainMenuData.Transition(ReservedModMenuID);
            }
        }

        public void RenderReloadModsButton()
        {
            var menuData = Globals.Game.xGlobalData.xMainMenuData;

            Color selected = Color.White;
            Color notSelected = Color.Gray * 0.8f;

            var text = "Reload Mods";
            var font = FontManager.GetFont(FontManager.FontType.Verdana12);
            Vector2 center = font.MeasureString(text) / 2f;

            Color colorToUse = menuData.iTopMenuSelection == 4 ? selected : notSelected;

            Globals.Game._RenderMaster_RenderTextWithOutline(font, text, new Vector2(160, 268) - center, Vector2.Zero, 1f, colorToUse, Color.Black);
        }

        public void RenderModMenuButton()
        {
            var menuData = Globals.Game.xGlobalData.xMainMenuData;
            var spriteBatch = Globals.SpriteBatch;

            Color selected = Color.White;
            Color notSelected = Color.Gray * 0.8f;
            float alpha = menuData.fCurrentMenuAlpha;

            var texture = Globals.ModManager.GrindScript.ModMenuText;
            Vector2 center = new Vector2(texture.Width / 2, texture.Height / 2);

            Color colorToUse = menuData.iTopMenuSelection == 4 ? selected : notSelected;

            spriteBatch.Draw(texture, new Vector2(160 - center.X, 245 - center.Y), null, colorToUse, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);

            //var font = FontManager.GetFont(FontManager.FontType.Bold10);
            //center = font.MeasureString("(Unimplemented)") / 2f;
            //Globals.Game._RenderMaster_RenderTextWithOutline(font, "(Unimplemented)", new Vector2(160, 268) - center, Vector2.Zero, 1f, colorToUse, Color.Black);
        }

        public void MenuUpdate()
        {
            if (Globals.Game.xGlobalData.xMainMenuData.enTargetMenuLevel != GlobalData.MainMenu.MenuLevel.Null)
            {
                return;
            }

            if (Globals.Game.xGlobalData.xMainMenuData.enMenuLevel == ReservedModMenuID)
            {
                ModMenuInterface();
            }
        }

        private void ModMenuInterface()
        {
            var input = Globals.Game.xInput_Menu;

            var previousSelection = _modMenu.Selection;

            if (input.Down.bPressed)
            {
                _modMenu.Selection++;
            }
            else if (input.Up.bPressed)
            {
                _modMenu.Selection--;
            }

            if (previousSelection != _modMenu.Selection)
            {
                Globals.Game.xSoundSystem.PlayInterfaceCue("Menu_Move");
            }

            if (input.Action.bPressed)
            {
                Globals.Game.xSoundSystem.PlayInterfaceCue("Menu_Change");

                switch (_modMenu.Selection)
                {
                    case 0:
                        Globals.ModManager.Loader.Reload();
                        break;
                    case 1:
                        break;
                }

            }
            else if (input.MenuBack.bPressed)
            {
                Globals.Game.xSoundSystem.PlayInterfaceCue("Menu_Cancel");

                Globals.Game.xGlobalData.xMainMenuData.Transition(GlobalData.MainMenu.MenuLevel.TopMenu);
            }
        }

        public void ModMenuRender()
        {
            float alpha = Globals.Game.xGlobalData.xMainMenuData.fCurrentMenuAlpha;
            Color selected = Color.White;
            Color notSelected = Color.Gray * 0.8f;

            SpriteBatch spriteBatch = Globals.SpriteBatch;

            Globals.Game._Menu_RenderContentBox(spriteBatch, alpha, new Rectangle(235, 189, 173, 138));


            Texture2D reloadModsTex = Globals.ModManager.GrindScript.ReloadModsText;
            Vector2 reloadModsCenter = new Vector2(reloadModsTex.Width / 2, reloadModsTex.Height / 2);
            Color reloadModsColor = _modMenu.Selection == 0 ? selected : notSelected;

            spriteBatch.Draw(reloadModsTex, new Vector2(320, 225), null, reloadModsColor, 0f, reloadModsCenter, 1f, SpriteEffects.None, 0f);

            Texture2D modListTex = Globals.ModManager.GrindScript.ModListText;
            Vector2 modListCenter = new Vector2(modListTex.Width / 2, modListTex.Height / 2);
            Color modListColor = _modMenu.Selection == 1 ? selected : notSelected;
            modListColor *= 0.6f;

            spriteBatch.Draw(modListTex, new Vector2(320, 251), null, modListColor, 0f, modListCenter, 1f, SpriteEffects.None, 0f);

            string message = "Mods loaded:\n";

            foreach (Mod mod in Globals.ModManager.ActiveMods)
            {
                message += mod.NameID + " v." + (mod.ModVersion?.ToString() ?? "Unknown") + "\n";
            }

            RenderMessage(message.TrimEnd('\n'), 422, 243);
        }
    }
}
