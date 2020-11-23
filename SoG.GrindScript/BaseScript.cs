using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace SoG.GrindScript
{
    public partial class BaseScript
    {
        private static int ItemTypesCurrent = 400000;
        protected int ItemTypesStart = 0;
        protected int ItemTypesCount = 0;

        private readonly dynamic _game;

        public LocalGame LocalGame { get; }

        public Player LocalPlayer { get; }

        public SpriteBatch SpriteBatch { get; }

        protected Dictionary<string, ModCodex.ItemDescription> loadedItemDescriptions = new Dictionary<string, ModCodex.ItemDescription>();

        protected Dictionary<string, ModCodex.WeaponInfo> loadedWeaponInfos = new Dictionary<string, ModCodex.WeaponInfo>();

        private readonly dynamic MiscTextGodDefault;

        protected BaseScript() 
        {
            Utils.Initialize(AppDomain.CurrentDomain.GetAssemblies().First(a => a.GetName().Name == "Secrets Of Grindea"));

            _game = Utils.GetGameType("SoG.Program").GetMethod("GetTheGame")?.Invoke(null, null);

            LocalGame = new LocalGame(_game);

            SpriteBatch = (SpriteBatch)Utils.GetGameType("SoG.Game1").
                GetField("spriteBatch", BindingFlags.Instance | BindingFlags.NonPublic)?.GetValue(_game);

            LocalPlayer = new Player(_game.xLocalPlayer);

            MiscTextGodDefault = _game.xMiscTextGod_Default;
        }


        protected SpriteFont GetFont(FontType font)
        {
            return (SpriteFont)Utils.GetGameType("SoG.FontManager").GetMethod("GetFont")?.Invoke(null, new object[] { (int)font });
        }

        protected ModCodex.ItemDescription AddModItem(string ID)
        {
            ModCodex.ItemDescription xDesc = new ModCodex.ItemDescription();
            loadedItemDescriptions.Add(ID, xDesc);
            return xDesc;
        }

        protected void LoadModItems()
        {
            int totalCount = 0;
            dynamic dictionary = Utils.GetGameType("SoG.ItemCodex").GetField("denxLoadedDescriptions", BindingFlags.Public | BindingFlags.Static).GetValue(null);
            foreach (ModCodex.ItemDescription item in loadedItemDescriptions.Values)
            {
                item.Original.enType = (dynamic)Enum.ToObject(ModCodex.SoGType.ItemTypes, ItemTypesCurrent + totalCount);
                totalCount++;
                dictionary[item.Original.enType] = item.Original;
            }

            // Indexes for this mod
            ItemTypesStart = ItemTypesCurrent;
            ItemTypesCount = totalCount;

            // Get next available position
            ItemTypesCurrent += totalCount;
        }

        protected int GetModItemFromString(string ID)
        {
            SoG.GrindScript.ModCodex.ItemDescription xIt;
            if (!loadedItemDescriptions.TryGetValue(ID, out xIt))
            {
                return -1;
            }
            return (int)xIt.Original.enType;
        }

        protected void AddMiscText(string category, string entry, string text, MiscTextTypes textType = MiscTextTypes.Default)
        {
            try
            {
                dynamic textEntry = Utils.GetGameType("SoG.MiscText").GetConstructor(new Type[0] { }).Invoke(new object[0] { });
                MiscTextGodDefault.dsxTextCollections[category].dsxTexts[entry] = textEntry;

                // Set extra params

                // No support for color tags yet...
                textEntry.sUnparsedFullLine = textEntry.sUnparsedBaseLine = text;

                switch (textType)
                {
                    case MiscTextTypes.GenericItemName:
                        textEntry.iMaxWidth = 170;
                        textEntry.iMaxHeight = 19;
                        textEntry.enFontType = (dynamic)Enum.ToObject(Utils.GetGameType("SoG.FontManager").GetDeclaredNestedType("FontType"), (int)FontType.Bold8Spacing1); //Bold8Spacing1
                        break;
                    case MiscTextTypes.GenericItemDescription:
                        // "You screwed up but we're putting in random values anyway" case
                        textEntry.iMaxWidth = 200;
                        textEntry.iMaxHeight = 100;
                        textEntry.enFontType = (dynamic)Enum.ToObject(Utils.GetGameType("SoG.FontManager").GetDeclaredNestedType("FontType"), (int)FontType.Reg7); //Reg 7
                        break;
                    case MiscTextTypes.Default:
                    default:
                        // "You screwed up but we're putting in random values anyway" case
                        textEntry.iMaxWidth = 200;
                        textEntry.iMaxHeight = 100;
                        textEntry.enFontType = (dynamic)Enum.ToObject(Utils.GetGameType("SoG.FontManager").GetDeclaredNestedType("FontType"), (int)FontType.Reg7); //Reg 7
                        break;
                }
            }
            catch(Exception e)
            {
                Console.WriteLine("Couldn't add misc text " + category + ":" + entry + ":" + text);
                Console.WriteLine("Reason: " + e);
            }
        }

        public virtual void OnDraw()
        {
            return;
        }

        public virtual void OnPlayerDamaged(ref int damage, ref byte type)
        {
            return;
        }

        public virtual void OnPlayerKilled()
        {
            return;
        }

        public virtual void PostPlayerLevelUp(Player player)
        {
            return;
        }

        public virtual void OnEnemyDamaged(Enemy enemy, ref int damage, ref byte type)
        {
            return;
        }

        public virtual void OnNPCDamaged(NPC enemy, ref int damage, ref byte type)
        {
            return;
        }

        public virtual void OnNPCInteraction(NPC npc)
        {
            return;
        }

        public virtual void OnArcadiaLoad()
        {
            return;
        }
    }
}
