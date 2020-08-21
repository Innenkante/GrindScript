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
            Console.WriteLine(game != null ? "Game is not null man" : "Game is null man");

            var npcCodex = Utils.GetGameType("SoG.NPCCodex");
            var npcTypes = npcCodex.GetNestedType("NPCTypes");

            Console.WriteLine("Wild npc type:" + npcType.ToString());

            var npc = Utils.GetGameType("SoG.Game1")
                .GetMethod("_EntityMaster_AddNPC", new[] {npcTypes, typeof(Vector2)}).Invoke(game,
                    new[] {npcTypes.GetField(npcType.ToString()).GetValue(null), new Vector2(100, 500)});

            var addToDic = Utils.GetGameType("SoG.LevelMaster").GetField("denxNamedEntities").FieldType
                .GetMethod("Add", new[] {npcTypes, Utils.GetGameType("SoG.IEntity")});

            addToDic.Invoke(game.xLevelMaster.denxNamedEntities,
                new object[] {npcTypes.GetField(npcType.ToString()).GetValue(null), npc});





            npc.xRenderComponent.SwitchAnimation(1);

            Console.WriteLine("Initialized NPC... " + npc.enType.ToString());

           // var npc = game._EntityMaster_AddNPC(npcTypes.GetField(npcType.ToString()).GetValue(null), new Vector2(100, 500)); //hacky af

           //another idea: hook up the initialization and execute the code afterwards/before :thinking:

            //I think adding the npc to the render loops kind of fucks up, because the guy is initialized but not not displayed



            return new NPC(npc);
        }
    }
}
