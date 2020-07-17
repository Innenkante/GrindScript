using System;
using System.Collections.Generic;
using System.Linq;
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

            var npc = game._EntityMaster_AddNPC((dynamic)NPCTypes.TaiMing_PetMonkey, new Vector2(100, 500)); //hacky af

            return new NPC(npc);
        }
    }
}
