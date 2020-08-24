using System;
using System.Linq;
using System.Reflection;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace SoG.GrindScript
{
    public partial class BaseScript
    {
        private readonly dynamic _game;

        public LocalGame LocalGame { get; }

        public LocalPlayer LocalPlayer { get; }

        public SpriteBatch SpriteBatch { get; }


        protected BaseScript() 
        {
            Utils.Initialize(AppDomain.CurrentDomain.GetAssemblies().First(a => a.GetName().Name == "Secrets Of Grindea"));

            _game = Utils.GetGameType("SoG.Program").GetMethod("GetTheGame")?.Invoke(null, null);

            LocalGame = new LocalGame(_game);

            SpriteBatch = (SpriteBatch)Utils.GetGameType("SoG.Game1").
                GetField("spriteBatch", BindingFlags.Instance | BindingFlags.NonPublic)?.GetValue(_game);

            LocalPlayer = new LocalPlayer(_game.xLocalPlayer);
        }


        protected SpriteFont GetFont(FontType font)
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
