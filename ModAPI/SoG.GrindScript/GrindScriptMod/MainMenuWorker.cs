using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SoG.Modding.Patches;
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
        public class ModSaveData
        {
            public List<string> ModsSaved = new List<string>();
        }

        private Dictionary<int, ModSaveData> _modSaves = new Dictionary<int, ModSaveData>();

        private ModSaveData _arcadeSave;

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
                        var modMeta = Globals.ModManager.Saving.PeekGrindScriptData(stream);

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
                    var modMeta = Globals.ModManager.Saving.PeekGrindScriptData(stream);

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

            List<string> loadedMods = Globals.ModManager.Mods.Select(x => x.NameID).ToList();
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

            List<string> loadedMods = Globals.ModManager.Mods.Select(x => x.NameID).ToList();
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
                message = "Loading is OK!\nSave mod list is compatible.";
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
                Globals.ModManager.Loader.Reload();
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
            var spriteBatch = HelperCallbacks.SpriteBatch;

            Color selected = Color.White;
            Color notSelected = Color.Gray * 0.8f;
            float alpha = menuData.fCurrentMenuAlpha;

            var texture = Globals.ModManager.GrindScript.ModMenuText;
            Vector2 center = new Vector2(texture.Width / 2, texture.Height / 2);

            Color colorToUse = menuData.iTopMenuSelection == 4 ? selected : notSelected;

            spriteBatch.Draw(texture, new Vector2(160 - center.X, 245 - center.Y), null, colorToUse, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);

            var font = FontManager.GetFont(FontManager.FontType.Bold10);

            center = font.MeasureString("(Unimplemented)") / 2f;

            Globals.Game._RenderMaster_RenderTextWithOutline(font, "(Unimplemented)", new Vector2(160, 268) - center, Vector2.Zero, 1f, colorToUse, Color.Black);
        }

        public void PostTopMenuRender()
        {
            RenderReloadModsButton();
        }
    }
}
