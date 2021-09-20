using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SoG.Modding;
using SoG.Modding.Extensions;

namespace SoG.FeatureExample
{
    static class CaveLevelStuff
    {
        public static void Build(LevelBlueprint bprint)
        {
			bprint.AddBackgroundSprites(new LevelBlueprint.BackgroundSpriteBP("Bkg/StartVillage/Storagecave/storagecave", new Vector2(16400f, 0f)));
			bprint.SetLevelBounds(16400, 0, 640, 360);
			bprint.lxLevelPartitions.Add(new Level.LevelPartition(new Rectangle(16400, 0, 640, 360), bLowMusicVolume: true, Level.LevelPartition.ReverbSetting.Cave));
			bprint.lxZoningFields.Add(new Level.ZoningField(new Rectangle(16681, 348, 56, 12), new Rectangle(16681, 328, 56, 30), Level.ZoneEnum.FirstVillage_Outside, 2));
			bprint.AddSpawnpoint(new Vector2(16400f, 0f) + new Vector2(308f, 318f), 1);
			bprint.aiLayerDefaultHeight = new int[2] { 0, 40 };

			Vector2 caveOffset = new Vector2(16400f, 0f);

			bprint.AddCollider(new BoxCollider(new Rectangle(0, 331, 279, 29)), 1, false, caveOffset);
			bprint.AddCollider(new BoxCollider(new Rectangle(341, 331, 299, 29)), 1, false, caveOffset);
			bprint.AddCollider(new BoxCollider(new Rectangle(264, 355, 105, 5)), 1, false, caveOffset);
			bprint.AddCollider(new BoxCollider(new Rectangle(109, 252, 46, 84)), 1, false, caveOffset);
			bprint.AddCollider(new BoxCollider(new Rectangle(94, 185, 38, 77)), 1, false, caveOffset);
			bprint.AddCollider(new BoxCollider(new Rectangle(121, 176, 66, 60)), 1, false, caveOffset);
			bprint.AddCollider(new BoxCollider(new Rectangle(173, 149, 32, 64)), 1, false, caveOffset);
			bprint.AddCollider(new BoxCollider(new Rectangle(418, 136, 41, 29)), 1, false, caveOffset);
			bprint.AddCollider(new BoxCollider(new Rectangle(492, 194, 63, 149)), 1, false, caveOffset);
			bprint.AddCollider(new BoxCollider(new Rectangle(141, 296, 33, 62), MathHelper.ToRadians(-45f)), 1, false, caveOffset);
			bprint.AddCollider(new BoxCollider(new Rectangle(172, 194, 31, 35), MathHelper.ToRadians(-42f)), 1, false, caveOffset);
			bprint.AddCollider(new BoxCollider(new Rectangle(169, 121, 156, 50), MathHelper.ToRadians(-16f)), 1, false, caveOffset);
			bprint.AddCollider(new BoxCollider(new Rectangle(276, 115, 152, 54), MathHelper.ToRadians(11f)), 1, false, caveOffset);
			bprint.AddCollider(new BoxCollider(new Rectangle(452, 258, 51, 118), MathHelper.ToRadians(45f)), 1, false, caveOffset);
			bprint.AddCollider(SphereCollider.CreateShell(new Vector2(466f, 162f), 31f), 1, false, caveOffset);
			bprint.AddCollider(SphereCollider.CreateShell(new Vector2(477f, 184f), 25f), 1, false, caveOffset);

			bprint.AddStaticObjects(
				new LevelBlueprint.LevelObjectBlueprint(LevelBlueprint.StaticObject._UniqueShit_FirstVillage_Storage_Skitnere, new Vector2(16508f, 354f), true, 0f, 1),
				new LevelBlueprint.LevelObjectBlueprint(LevelBlueprint.StaticObject._Indoors_SitStuff_Pall, new Vector2(16572f, 280f), true, 0f, 1),
				new LevelBlueprint.LevelObjectBlueprint(LevelBlueprint.StaticObject._Indoors_Misc_ScrollPile01, new Vector2(16599f, 265f), true, 0f, 1),
				new LevelBlueprint.LevelObjectBlueprint(LevelBlueprint.StaticObject._Indoors_Bordsdeko_Scroll01, new Vector2(16618f, 264f), true, 0f, 1),
				new LevelBlueprint.LevelObjectBlueprint(LevelBlueprint.StaticObject._Indoors_SitStuff_Bench01, new Vector2(16638f, 227f), true, 0f, 1),
				new LevelBlueprint.LevelObjectBlueprint(LevelBlueprint.StaticObject._Indoors_SitStuff_WoodenChairRight, new Vector2(16646f, 208f), true, 0f, 1),
				new LevelBlueprint.LevelObjectBlueprint(LevelBlueprint.StaticObject._Indoors_Misc_ScrollPile01, new Vector2(16671f, 201f), true, 0f, 1),
				new LevelBlueprint.LevelObjectBlueprint(LevelBlueprint.StaticObject._Indoors_Misc_ScrollPile01, new Vector2(16667f, 193f), true, 0f, 1),
				new LevelBlueprint.LevelObjectBlueprint(LevelBlueprint.StaticObject._Indoors_Bordsdeko_Scroll01, new Vector2(16656f, 176f), true, 0f, 1),
				new LevelBlueprint.LevelObjectBlueprint(LevelBlueprint.StaticObject._Indoors_TDD_Chess, new Vector2(16758f, 204f), true, 0f, 1),
				new LevelBlueprint.LevelObjectBlueprint(LevelBlueprint.StaticObject._Indoors_SitStuff_WoodenChairDown, new Vector2(16783f, 234f), true, 0f, 1),
				new LevelBlueprint.LevelObjectBlueprint(LevelBlueprint.StaticObject._Indoors_SitStuff_WoodenChairDown, new Vector2(16804f, 240f), true, 0f, 1),
				new LevelBlueprint.LevelObjectBlueprint(LevelBlueprint.StaticObject._Indoors_TDD_Workdesk_small, new Vector2(16792f, 248f), true, 0f, 1),
				new LevelBlueprint.LevelObjectBlueprint(LevelBlueprint.StaticObject._Indoors_TDD_Drawer_small, new Vector2(16764f, 178f), true, 0f, 1),
				new LevelBlueprint.LevelObjectBlueprint(LevelBlueprint.StaticObject._Cave_Sign, new Vector2(16711f, 264f), true, 0f, 1)
				);

			bprint.AddDynamicObjects(
				new LevelBlueprint.LevelObjectBlueprint(LevelBlueprint.StaticObject._Dynamic_Crate, new Vector2(16611f, 242f), true, 0f, 1),
				new LevelBlueprint.LevelObjectBlueprint(LevelBlueprint.StaticObject._Dynamic_Crate, new Vector2(16710f, 165f), true, 0f, 1),
				new LevelBlueprint.LevelObjectBlueprint(LevelBlueprint.StaticObject._Dynamic_Crate, new Vector2(16722f, 185f), true, 0f, 1),
				new LevelBlueprint.LevelObjectBlueprint(LevelBlueprint.StaticObject._Dynamic_Crate, new Vector2(16745f, 189f), true, 0f, 1),
				new LevelBlueprint.LevelObjectBlueprint(LevelBlueprint.StaticObject._Dynamic_Crate, new Vector2(16729f, 198f), true, 0f, 1),
				new LevelBlueprint.LevelObjectBlueprint(LevelBlueprint.StaticObject._Dynamic_Crate, new Vector2(16790f, 203f), true, 0f, 1),
				new LevelBlueprint.LevelObjectBlueprint(LevelBlueprint.StaticObject._Dynamic_Crate, new Vector2(16799f, 180f), true, 0f, 1),
				new LevelBlueprint.LevelObjectBlueprint(LevelBlueprint.StaticObject._Dynamic_Barrel, new Vector2(16572f, 261f), true, 0f, 1),
				new LevelBlueprint.LevelObjectBlueprint(LevelBlueprint.StaticObject._Dynamic_Barrel, new Vector2(16575f, 244f), true, 0f, 1),
				new LevelBlueprint.LevelObjectBlueprint(LevelBlueprint.StaticObject._Dynamic_Barrel, new Vector2(16622f, 214f), true, 0f, 1),
				new LevelBlueprint.LevelObjectBlueprint(LevelBlueprint.StaticObject._Dynamic_Barrel, new Vector2(16635f, 188f), true, 0f, 1),
				new LevelBlueprint.LevelObjectBlueprint(LevelBlueprint.StaticObject._Dynamic_Barrel, new Vector2(16700f, 176f), true, 0f, 1),
				new LevelBlueprint.LevelObjectBlueprint(LevelBlueprint.StaticObject._Dynamic_Barrel, new Vector2(16710f, 222f), true, 0f, 1),
				new LevelBlueprint.LevelObjectBlueprint(LevelBlueprint.StaticObject._Dynamic_Barrel, new Vector2(16811f, 298f), true, 0f, 1),
				new LevelBlueprint.LevelObjectBlueprint(LevelBlueprint.StaticObject._Dynamic_Barrel, new Vector2(16848f, 237f), true, 0f, 1),
				new LevelBlueprint.LevelObjectBlueprint(LevelBlueprint.StaticObject._Dynamic_Barrel, new Vector2(16834f, 221f), true, 0f, 1),
				new LevelBlueprint.LevelObjectBlueprint(LevelBlueprint.StaticObject._Dynamic_Barrel, new Vector2(16858f, 222f), true, 0f, 1),
				new LevelBlueprint.LevelObjectBlueprint(LevelBlueprint.StaticObject._Dynamic_Jar, new Vector2(16591f, 294f), true, 0f, 1),
				new LevelBlueprint.LevelObjectBlueprint(LevelBlueprint.StaticObject._Dynamic_Jar, new Vector2(16614f, 305f), true, 0f, 1),
				new LevelBlueprint.LevelObjectBlueprint(LevelBlueprint.StaticObject._Dynamic_Jar, new Vector2(16638f, 298f), true, 0f, 1),
				new LevelBlueprint.LevelObjectBlueprint(LevelBlueprint.StaticObject._Dynamic_Jar, new Vector2(16752f, 291f), true, 0f, 1),
				new LevelBlueprint.LevelObjectBlueprint(LevelBlueprint.StaticObject._Dynamic_Jar, new Vector2(16757f, 270f), true, 0f, 1),
				new LevelBlueprint.LevelObjectBlueprint(LevelBlueprint.StaticObject._Dynamic_Jar, new Vector2(16823f, 237f), true, 0f, 1),
				new LevelBlueprint.LevelObjectBlueprint(LevelBlueprint.StaticObject._Dynamic_Jar, new Vector2(16789f, 224f), true, 0f, 1),
				new LevelBlueprint.LevelObjectBlueprint(LevelBlueprint.StaticObject._Dynamic_Jar, new Vector2(16830f, 191f), true, 0f, 1),
				new LevelBlueprint.LevelObjectBlueprint(LevelBlueprint.StaticObject._Dynamic_Jar, new Vector2(16736f, 225f), true, 0f, 1),
				new LevelBlueprint.LevelObjectBlueprint(LevelBlueprint.StaticObject._Dynamic_Jar, new Vector2(16614f, 190f), true, 0f, 1)
			);
		}
    }
}
