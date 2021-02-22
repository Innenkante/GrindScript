using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace SoG.GrindScript
{
    public partial class BaseScript
    {
        private readonly dynamic _game;

        protected ContentManager ModContent;

        public LocalGame LocalGame { get; }

        public Player LocalPlayer { get; }

        public SpriteBatch SpriteBatch { get; }

        protected BaseScript() 
        {
            Utils.Initialize(AppDomain.CurrentDomain.GetAssemblies().First(a => a.GetName().Name == "Secrets Of Grindea"));

            ModContent = new ContentManager(Utils.GetTheGame().Content.ServiceProvider, "ModContent/" + this.GetType().Name);

            Console.WriteLine(this.GetType().Name + " ContentManager path set as " + ModContent.RootDirectory);

            _game = Utils.GetTheGame();

            LocalGame = new LocalGame(_game);

            SpriteBatch = (SpriteBatch)Utils.GetGameType("SoG.Game1").
                GetField("spriteBatch", BindingFlags.Instance | BindingFlags.NonPublic)?.GetValue(_game);

            LocalPlayer = new Player(_game.xLocalPlayer);
        }

        protected SpriteFont GetFont(FontManager.FontType font)
        {
            return (SpriteFont)Utils.GetGameType("SoG.FontManager").GetMethod("GetFont")?.Invoke(null, new object[] { (int)font });
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

        public virtual void OnCustomContentLoad()
        {
            return;
        }

        public virtual bool OnChatParseCommand(string command, string argList, int connection)
        {
            // Connection param can usually be ignored
            return true;
        }

        public virtual void OnItemUse(ItemCodex.ItemTypes enItem, PlayerView xView, ref bool bSend)
        {
            return;
        }
    }
}
