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
        public NPC(object originalType) : base(originalType)
        {
        }

        
        public static NPC AddNPCTo(dynamic game, NPCTypes npcType, Vector2 position)
        {
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
