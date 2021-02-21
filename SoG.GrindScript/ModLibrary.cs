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
		// "Owning" Dictionaries for Mod Content (items, etc.)

		internal static readonly Dictionary<ItemCodex.ItemTypes, ModItemData> ItemDetails = new Dictionary<ItemCodex.ItemTypes, ModItemData>();

		internal static readonly Dictionary<EnemyCodex.EnemyTypes, ModEnemyData> EnemyDetails = new Dictionary<EnemyCodex.EnemyTypes, ModEnemyData>();

		internal static readonly Dictionary<DynamicEnvironmentCodex.ObjectTypes, ModDynEnvData> DynEnvDetails = new Dictionary<DynamicEnvironmentCodex.ObjectTypes, ModDynEnvData>();

		// "Non-Owning" Helper dictionaries, references, etc.

		internal static readonly Dictionary<string, ItemCodex.ItemTypes> RegisteredItems = new Dictionary<string, ItemCodex.ItemTypes>(); // Correlates one string to one item ID - utility for modders

		internal static readonly Dictionary<string, EquipmentInfo.SpecialEffect> RegisteredEquipmentEffects = new Dictionary<string, EquipmentInfo.SpecialEffect>(); // Correlates one string to one special effect ID - utility for modders

		// Helper values and properties for enum real estate

		internal const ItemCodex.ItemTypes ItemTypesStart = (ItemCodex.ItemTypes)400000;

		internal static ItemCodex.ItemTypes ItemTypesNext => ItemTypesStart + ItemDetails.Count;

		internal const EquipmentInfo.SpecialEffect SpecialEffectsStart = (EquipmentInfo.SpecialEffect)200;

		internal static ushort SpecialEffectsCount = 0;

		internal static EquipmentInfo.SpecialEffect SpecialEffectsNext => SpecialEffectsStart + SpecialEffectsCount;

		internal const EnemyCodex.EnemyTypes EnemyTypesStart = (EnemyCodex.EnemyTypes)400000;

		internal static EnemyCodex.EnemyTypes EnemyTypesNext => EnemyTypesStart + EnemyDetails.Count;

		internal const DynamicEnvironmentCodex.ObjectTypes DynEnvTypeStart = (DynamicEnvironmentCodex.ObjectTypes)10000;

		internal static DynamicEnvironmentCodex.ObjectTypes DynEnvTypeNext => (DynamicEnvironmentCodex.ObjectTypes)((int)DynEnvTypeStart + DynEnvDetails.Count);

		// Utility Functions

		public static void AddItemAlias(string name, ItemCodex.ItemTypes enType)
		{
			RegisteredItems.Add(name, enType);
		}

		public static ItemCodex.ItemTypes ItemAliasValue(string name)
		{
			ItemCodex.ItemTypes enType;
			if(!RegisteredItems.TryGetValue(name, out enType))
			{
				enType = (ItemCodex.ItemTypes)(-1);
			}
			return enType;
		}

		public static void AddItemEffectAlias(string name, EquipmentInfo.SpecialEffect enType)
		{
			RegisteredEquipmentEffects.Add(name, enType);
		}

		public static EquipmentInfo.SpecialEffect ItemEffectAliasValue(string name)
		{
			EquipmentInfo.SpecialEffect enType;
			if (!RegisteredEquipmentEffects.TryGetValue(name, out enType))
			{
				enType = (EquipmentInfo.SpecialEffect)(ushort.MaxValue);
			}
			return enType;
		}
	}
}
