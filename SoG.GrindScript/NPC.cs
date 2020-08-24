using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace SoG.GrindScript
{
    public class NPC : ConvertedType
    {
        public bool IsInteractable
        {
            get => _originalType.bIsInteractable;
            set => _originalType.bIsInteractable = value;
        }

        public bool IsGhost
        {
            get => _originalType.bIsTwilightGhost;
            set => _originalType.bIsTwilightGhost = value;
        }

        public bool LookAtPlayerOnInteraction
        {
            get => _originalType.bLookAtPlayerAtInteraction;
            set => _originalType.bLookAtPlayerAtInteraction = value;
        }

        public NPCTypes GetNPCType()
        {
            return Enum.Parse(typeof(NPCTypes), _originalType.enType.ToString());
        }

        public NPC(object originalType) : base(originalType)
        {
            
        }

        
        public static NPC AddNPCTo(LocalGame localGame, NPCTypes npcType, Vector2 position)
        {

            var game = localGame.GetUnderlayingGame();
            //Console.WriteLine(game != null ? "Game is not null man" : "Game is null man");

            var npcCTypex = Utils.GetGameType("SoG.NPCCodex").GetDeclaredNestedType("NPCTypes");

            //Console.WriteLine("Wild npc type:" + npcType.ToString());

            var npcGameType = Enum.Parse(npcCTypex, npcType.ToString());
            //Console.WriteLine(npcGameType.GetType());


            var n = Utils.GetGameType("SoG.Game1").GetPublicInstanceOverloadedMethods("_EntityMaster_AddNPC").First()
                .Invoke((object) game, new[] {npcGameType, position});

            //Console.WriteLine(n.GetType());

            var key = npcGameType;
            var value = n;

            ((Type) game.xLevelMaster.denxNamedEntities.GetType()).GetMethod("Add", new[] {key.GetType(), Utils.GetGameType("SoG.IEntity")}).Invoke(game.xLevelMaster.denxNamedEntities, new []{key, value});
            ((dynamic)n).xRenderComponent.SwitchAnimation(1);

            Console.WriteLine("Initialized NPC: " + npcType);
            return new NPC(n);
        }
    }
}
