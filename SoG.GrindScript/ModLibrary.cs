using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Graphics;

namespace SoG.GrindScript
{

	// TO DO: Decide on what fields should be "internal" and which not

    public static class ModLibrary
	{
		#region "Owning" Dictionaries for Mod Content (items, etc.)

		internal static readonly List<ModItem> CustomItems = new List<ModItem>(); // Stores item descriptions and is also used to track which ItemTypes enum values are used to implement custom items

		internal static readonly List<int> CustomEquipmentEffects = new List<int>(); // Tracks which SpecialEffect enum values are used to implement custom effects

		#endregion

		#region "Non-Owning" Helper dictionaries, references, etc.

		internal static readonly Dictionary<int, string> ItemModDictionary = new Dictionary<int, string>(); // Used to tell which custom item was added by which mod

		internal static readonly Dictionary<string, int> RegisteredItems = new Dictionary<string, int>(); // Correlates one string to one item ID - utility for modders

		internal static readonly Dictionary<string, int> RegisteredEquipmentEffects = new Dictionary<string, int>(); // Correlates one string to one special effect ID - utility for modders

		internal static readonly Dictionary<string, ContentManager> ModContentManagers = new Dictionary<string, ContentManager>(); // Keeps a reference to each mod's Content Manager

		#endregion

		#region Helper values and properties for enum real estate

		internal const int ItemTypesStart = 400000;

		internal static int ItemTypesNext { get => ItemTypesStart + CustomItems.Count; }

		internal const int SpecialEffectsStart = 200;

		internal static int SpecialEffectsNext { get => SpecialEffectsStart + CustomEquipmentEffects.Count; }

		#endregion

		#region Utility Functions

		public static ContentManager GetItemModContent(int type)
		{
			return ModContentManagers[ItemModDictionary[type]];
		}

		public static string PrefixResource(string mod, string resource)
        {
            return mod + "/" + resource;
        }

        public static string PrefixResource(int item, string resource)
        {
            return ItemModDictionary[item] + "/" + resource;
        }

		public static string GetModFromPrefixedResource(string prefixed)
		{
			int index = prefixed.IndexOf('/');
			if (index < 0) index = 0;
			return prefixed.Substring(0, index);
		}

		public static string GetResourceFromPrefixedResource(string prefixed)
		{
			int index = prefixed.IndexOf('/');
			return prefixed.Substring(index + 1, prefixed.Length - index - 1);
		}

        #endregion

        #region Content Manager patches for Custom Items

        private static bool OnOneHandedDictionaryFillPrefix(ref Dictionary<ushort, string> dict, string sWeaponName)
		{
			string mod = GetModFromPrefixedResource(sWeaponName);

			if(mod == "")
			{
				// Continue with original, this ain't no mod item
				return true;
			}

			string resource = GetResourceFromPrefixedResource(sWeaponName);
			string prefix = mod + "/";

			// The idea is to add a prefix so that we can tell later on that this is a mod item
			// "Weapons/" before resource was purposefully removed
			dict.Add(100, prefix + "Sprites/Heroes/OneHanded/AttackA/" + resource + "/Up");
			dict.Add(101, prefix + "Sprites/Heroes/OneHanded/AttackA/" + resource + "/Right");
			dict.Add(102, prefix + "Sprites/Heroes/OneHanded/AttackA/" + resource + "/Down");
			dict.Add(103, prefix + "Sprites/Heroes/OneHanded/AttackA/" + resource + "/Right");
			dict.Add(104, prefix + "Sprites/Heroes/OneHanded/AttackB/" + resource + "/Up");
			dict.Add(105, prefix + "Sprites/Heroes/OneHanded/AttackB/" + resource + "/Right");
			dict.Add(106, prefix + "Sprites/Heroes/OneHanded/AttackB/" + resource + "/Down");
			dict.Add(107, prefix + "Sprites/Heroes/OneHanded/AttackB/" + resource + "/Right");
			dict.Add(132, prefix + "Sprites/Heroes/OneHanded/MillionStab/Finish/" + resource + "/Up");
			dict.Add(133, prefix + "Sprites/Heroes/OneHanded/MillionStab/Finish/" + resource + "/Right");
			dict.Add(134, prefix + "Sprites/Heroes/OneHanded/MillionStab/Finish/" + resource + "/Down");
			dict.Add(135, prefix + "Sprites/Heroes/OneHanded/MillionStab/Finish/" + resource + "/Right");
			dict.Add(144, prefix + "Sprites/Heroes/OneHanded/MillionStab/Level 4/Finish/" + resource + "/Up");
			dict.Add(145, prefix + "Sprites/Heroes/OneHanded/MillionStab/Level 4/Finish/" + resource + "/Right");
			dict.Add(146, prefix + "Sprites/Heroes/OneHanded/MillionStab/Level 4/Finish/" + resource + "/Down");
			dict.Add(147, prefix + "Sprites/Heroes/OneHanded/MillionStab/Level 4/Finish/" + resource + "/Right");
			dict.Add(550, prefix + "Sprites/Heroes/Charge/OneHand/Start/" + resource + "/Up");
			dict.Add(551, prefix + "Sprites/Heroes/Charge/OneHand/Start/" + resource + "/Right");
			dict.Add(552, prefix + "Sprites/Heroes/Charge/OneHand/Start/" + resource + "/Down");
			dict.Add(553, prefix + "Sprites/Heroes/Charge/OneHand/Start/" + resource + "/Right");
			dict.Add(554, prefix + "Sprites/Heroes/Charge/OneHand/Fladder/" + resource + "/Up");
			dict.Add(555, prefix + "Sprites/Heroes/Charge/OneHand/Fladder/" + resource + "/Right");
			dict.Add(556, prefix + "Sprites/Heroes/Charge/OneHand/Fladder/" + resource + "/Down");
			dict.Add(557, prefix + "Sprites/Heroes/Charge/OneHand/Fladder/" + resource + "/Right");
			dict.Add(558, prefix + "Sprites/Heroes/Charge/OneHand/Fladder/" + resource + "/Up");
			dict.Add(559, prefix + "Sprites/Heroes/Charge/OneHand/Fladder/" + resource + "/Right");
			dict.Add(560, prefix + "Sprites/Heroes/Charge/OneHand/Fladder/" + resource + "/Down");
			dict.Add(561, prefix + "Sprites/Heroes/Charge/OneHand/Fladder/" + resource + "/Right");
			dict.Add(574, prefix + "Sprites/Heroes/OneHanded/Shadow Clone/" + resource + "/Up");
			dict.Add(575, prefix + "Sprites/Heroes/OneHanded/Shadow Clone/" + resource + "/Right");
			dict.Add(576, prefix + "Sprites/Heroes/OneHanded/Shadow Clone/" + resource + "/Down");
			dict.Add(577, prefix + "Sprites/Heroes/OneHanded/Shadow Clone/" + resource + "/Right");
			dict.Add(578, prefix + "Sprites/Heroes/OneHanded/Samurai Slash/" + resource + "/Up");
			dict.Add(579, prefix + "Sprites/Heroes/OneHanded/Samurai Slash/" + resource + "/Right");
			dict.Add(580, prefix + "Sprites/Heroes/OneHanded/Samurai Slash/" + resource + "/Down");
			dict.Add(581, prefix + "Sprites/Heroes/OneHanded/Samurai Slash/" + resource + "/Right");

			// No need for original anymore
			return false;
		}

		private static bool OnTwoHandedDictionaryFillPrefix(ref Dictionary<ushort, string> dict, string sWeaponName)
		{
			string mod = GetModFromPrefixedResource(sWeaponName);

			if (mod == "")
			{
				// Continue with original, this ain't no mod item
				return true;
			}

			string resource = GetResourceFromPrefixedResource(sWeaponName);
			string prefix = mod + "/";

			// The idea is to add a prefix so that we can tell later on that this is a mod item
			// "Weapons/" before resource was purposefully removed
			dict.Add(200, prefix + "Sprites/Heroes/TwoHanded/AttackA/" + resource + "/Up");
			dict.Add(201, prefix + "Sprites/Heroes/TwoHanded/AttackA/" + resource + "/Right");
			dict.Add(202, prefix + "Sprites/Heroes/TwoHanded/AttackA/" + resource + "/Down");
			dict.Add(203, prefix + "Sprites/Heroes/TwoHanded/AttackA/" + resource + "/Right");
			dict.Add(212, prefix + "Sprites/Heroes/TwoHanded/AttackB/" + resource + "/Up");
			dict.Add(213, prefix + "Sprites/Heroes/TwoHanded/AttackB/" + resource + "/Right");
			dict.Add(214, prefix + "Sprites/Heroes/TwoHanded/AttackB/" + resource + "/Down");
			dict.Add(215, prefix + "Sprites/Heroes/TwoHanded/AttackB/" + resource + "/Right");
			dict.Add(204, prefix + "Sprites/Heroes/TwoHanded/Overhead/" + resource + "/Up");
			dict.Add(205, prefix + "Sprites/Heroes/TwoHanded/Overhead/" + resource + "/Right");
			dict.Add(206, prefix + "Sprites/Heroes/TwoHanded/Overhead/" + resource + "/Down");
			dict.Add(207, prefix + "Sprites/Heroes/TwoHanded/Overhead/" + resource + "/Right");
			dict.Add(208, prefix + "Sprites/Heroes/TwoHanded/Spin/Level 1-2/" + resource + "/Up");
			dict.Add(209, prefix + "Sprites/Heroes/TwoHanded/Spin/Level 1-2/" + resource + "/Right");
			dict.Add(210, prefix + "Sprites/Heroes/TwoHanded/Spin/Level 1-2/" + resource + "/Down");
			dict.Add(211, prefix + "Sprites/Heroes/TwoHanded/Spin/Level 1-2/" + resource + "/Left");
			dict.Add(280, prefix + "Sprites/Heroes/TwoHanded/Spin/Level 3/" + resource + "/Up");
			dict.Add(281, prefix + "Sprites/Heroes/TwoHanded/Spin/Level 3/" + resource + "/Right");
			dict.Add(282, prefix + "Sprites/Heroes/TwoHanded/Spin/Level 3/" + resource + "/Down");
			dict.Add(283, prefix + "Sprites/Heroes/TwoHanded/Spin/Level 3/" + resource + "/Left");
			dict.Add(284, prefix + "Sprites/Heroes/TwoHanded/Spin/Level 3/" + resource + "/Up");
			dict.Add(285, prefix + "Sprites/Heroes/TwoHanded/Spin/Level 3/" + resource + "/Right");
			dict.Add(286, prefix + "Sprites/Heroes/TwoHanded/Spin/Level 3/" + resource + "/Down");
			dict.Add(287, prefix + "Sprites/Heroes/TwoHanded/Spin/Level 3/" + resource + "/Left");
			dict.Add(216, prefix + "Sprites/Heroes/TwoHanded/Overhead/" + resource + "/Up");
			dict.Add(217, prefix + "Sprites/Heroes/TwoHanded/Overhead/" + resource + "/Right");
			dict.Add(218, prefix + "Sprites/Heroes/TwoHanded/Overhead/" + resource + "/Down");
			dict.Add(219, prefix + "Sprites/Heroes/TwoHanded/Overhead/" + resource + "/Right");
			dict.Add(221, prefix + "Sprites/Heroes/TwoHanded/Overhead/" + resource + "/Right");
			dict.Add(223, prefix + "Sprites/Heroes/TwoHanded/Overhead/" + resource + "/Right");
			dict.Add(225, prefix + "Sprites/Heroes/TwoHanded/Overhead/" + resource + "/Right");
			dict.Add(227, prefix + "Sprites/Heroes/TwoHanded/Overhead/" + resource + "/Right");
			dict.Add(229, prefix + "Sprites/Heroes/TwoHanded/Overhead/" + resource + "/Right");
			dict.Add(231, prefix + "Sprites/Heroes/TwoHanded/Overhead/" + resource + "/Right");
			dict.Add(233, prefix + "Sprites/Heroes/TwoHanded/Overhead/" + resource + "/Right");
			dict.Add(235, prefix + "Sprites/Heroes/TwoHanded/Overhead/" + resource + "/Right");
			dict.Add(260, prefix + "Sprites/Heroes/TwoHanded/Smash/" + resource + "/Up");
			dict.Add(261, prefix + "Sprites/Heroes/TwoHanded/Smash/" + resource + "/Right");
			dict.Add(262, prefix + "Sprites/Heroes/TwoHanded/Smash/" + resource + "/Down");
			dict.Add(263, prefix + "Sprites/Heroes/TwoHanded/Smash/" + resource + "/Right");
			dict.Add(650, prefix + "Sprites/Heroes/Charge/TwoHand/Start/" + resource + "/Up");
			dict.Add(651, prefix + "Sprites/Heroes/Charge/TwoHand/Start/" + resource + "/Right");
			dict.Add(652, prefix + "Sprites/Heroes/Charge/TwoHand/Start/" + resource + "/Down");
			dict.Add(653, prefix + "Sprites/Heroes/Charge/TwoHand/Start/" + resource + "/Right");
			dict.Add(654, prefix + "Sprites/Heroes/Charge/TwoHand/Fladder/" + resource + "/Up");
			dict.Add(655, prefix + "Sprites/Heroes/Charge/TwoHand/Fladder/" + resource + "/Right");
			dict.Add(656, prefix + "Sprites/Heroes/Charge/TwoHand/Fladder/" + resource + "/Down");
			dict.Add(657, prefix + "Sprites/Heroes/Charge/TwoHand/Fladder/" + resource + "/Right");
			dict.Add(658, prefix + "Sprites/Heroes/Charge/TwoHand/Fladder/" + resource + "/Up");
			dict.Add(659, prefix + "Sprites/Heroes/Charge/TwoHand/Fladder/" + resource + "/Right");
			dict.Add(660, prefix + "Sprites/Heroes/Charge/TwoHand/Fladder/" + resource + "/Down");
			dict.Add(661, prefix + "Sprites/Heroes/Charge/TwoHand/Fladder/" + resource + "/Right");

			// No need for original anymore
			return false;
		}

		private static bool OnLoadBatchPrefix(ref Dictionary<ushort, string> dis, dynamic __instance)
		{
			// This is a prefix overwrite patch thingy - original is not supposed to run!
			foreach (KeyValuePair<ushort, string> kvp in dis)
			{
				try
				{
					// Do the content olala

					string potentialMod = GetModFromPrefixedResource(kvp.Value);

					ContentManager Content;

					string file = kvp.Value;

					if (!ModLibrary.ModContentManagers.TryGetValue(potentialMod, out Content))
					{
						// Dis not a mod item, probs?
						Content = __instance.contWeaponContent;
					}
					else
					{
						// Since this is a mod item, strip the mod prefix - it's not needed since
						// it's duplicated in the content manager root directory
						file = GetResourceFromPrefixedResource(kvp.Value);
					}

					__instance.ditxWeaponTextures.Add(kvp.Key, Content.Load<Texture2D>(file));
				}
				catch
				{
					Utils.GetTheGame().Log("Failed to load weapon texture at: " + kvp.Value);
					__instance.ditxWeaponTextures[kvp.Key] = Utils.GetGameType("SoG.RenderMaster").GetPublicStaticField("txNullTex");
				}
			}
			return false;
		}

		private static bool On_Animations_GetAnimationSetPrefix(dynamic xPlayerView, string sAnimation, string sDirection, bool bWithWeapon, bool bCustomHat, bool bWithShield, bool bWeaponOnTop, ref dynamic __result)
		{
			dynamic ret = Utils.DefaultConstructObject("SoG.PlayerAnimationTextureSet");
			ret.bWeaponOnTop = bWeaponOnTop;
			string sAttackPath = "";

			ContentManager VanillaContent = Utils.GetGameType("SoG.RenderMaster").GetPublicStaticField("contPlayerStuff");

			ret.txBase = VanillaContent.Load<Texture2D>("Sprites/Heroes/" + sAttackPath + sAnimation + "/" + sDirection);

			// Skipped code which isn't used in vanilla

			if (bWithShield && xPlayerView.xEquipment.DisplayShield != null && xPlayerView.xEquipment.DisplayShield.sResourceName != "")
			{
				string potentialMod = ModLibrary.GetModFromPrefixedResource(xPlayerView.xEquipment.DisplayShield.sResourceName);

				ContentManager Content;

				string resource = ModLibrary.GetResourceFromPrefixedResource(xPlayerView.xEquipment.DisplayShield.sResourceName);

				if (!ModLibrary.ModContentManagers.TryGetValue(potentialMod, out Content))
				{
					// Dis not a mod item, probs?
					Content = VanillaContent;
				}

				if (Content == VanillaContent)
				{
					ret.txShield = Content.Load<Texture2D>("Sprites/Heroes/" + sAttackPath + sAnimation + "/Shields/" + resource + "/" + sDirection);
				}
				else
				{
					// Shortened path for mod loaders
					ret.txShield = Content.Load<Texture2D>("Sprites/Heroes/" + sAttackPath + sAnimation + "/" + resource + "/" + sDirection);
				}
			}
			if (bWithWeapon)
			{
				ret.txWeapon = Utils.GetGameType("SoG.RenderMaster").GetPublicStaticField("txNullTex");
			}

			__result = ret;
			return false;
		}

		#endregion
	}
}
