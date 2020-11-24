using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Schema;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace SoG.GrindScript
{
    public class CustomItem : ConvertedObject
    {
        public const int BaseItemTypesPos = 400000;

        public int Id { get; set; }

        public int EnType
        {
            get => (int)_originalObject.enType;
            set => _originalObject.enType = (dynamic)Enum.ToObject(Utils.GetGameType("SoG.ItemCodex+ItemTypes"), value);
        }

        public bool IsVanillaItem 
            => Enum.IsDefined(Utils.GetGameType("SoG.ItemCodex+ItemTypes"), EnType);

        public bool IsDefinedModItem
            => !IsVanillaItem && EnType >= BaseItemTypesPos && EnType <= BaseItemTypesPos + BaseScript.CustomItems.Count;

        public CustomItem(object originalObject) : base(originalObject)
        {
            
        }

        public static bool ValueIsVanillaItem(int enType)
        {
            return Enum.IsDefined(Utils.GetGameType("SoG.ItemCodex+ItemTypes"), enType);
        }

        public static bool ValueIsDefinedModItem(int enType)
        {
            return !ValueIsVanillaItem(enType) && enType >= BaseItemTypesPos && enType <= BaseItemTypesPos + BaseScript.CustomItems.Count;
        }

        public static CustomItem AddCustomItemTo(LocalGame game, string name, string description, string appearance, int value)
        {
            int newId = 1;
            if (BaseScript.CustomItems.Count > 0)
                newId = BaseScript.CustomItems.Count + 1;


            dynamic newItem = Utils.GetGameType("SoG.ItemDescription").GetConstructor(Type.EmptyTypes).Invoke(null);

            string baseEntryName = name.Replace(" ", "");

            Ui.AddMiscTextTo(game, "Items", baseEntryName + "_Name", name, MiscTextTypes.GenericItemName);
            Ui.AddMiscTextTo(game,"Items", baseEntryName + "_Description", description, MiscTextTypes.GenericItemDescription);

            newItem.sFullName = name;
            newItem.txDisplayImage = game.GetContentManager().Load<Texture2D>(appearance);
            newItem.lenCategory.Add((dynamic)Enum.ToObject(ModCodex.SoGType.ItemCategories, 0));
            newItem.sNameLibraryHandle = baseEntryName + "_Name";
            newItem.sDescriptionLibraryHandle = baseEntryName + "_Description";
            newItem.sCategory = "";
            newItem.iInternalLevel = 1;
            newItem.iValue = value;

            dynamic items = Utils.GetGameType("SoG.ItemCodex").GetPublicStaticField("denxLoadedDescriptions");


            newItem.enType = (dynamic)Enum.ToObject(Utils.GetGameType("SoG.ItemCodex+ItemTypes"), 400000 + newId);


            items[newItem.enType] = newItem;

            Console.WriteLine("Added the custom item called " + name + " with the id " + newId + "...");

            var customItem = new CustomItem(newItem) {Id = newId};

            BaseScript.CustomItems.Add(customItem);

            return customItem;
        }

        public void AddItemCategories(params ItemCategories[] cats)
        {
            dynamic xSet = Original.lenCategory;
            foreach (ItemCategories cat in cats)
            {
                xSet.Add((dynamic)Enum.ToObject(Utils.GetGameType("SoG.ItemCodex+ItemCategories"), (int)cat));
            }
        }

        public void RemoveItemCategories(params ItemCategories[] cats)
        {
            dynamic xSet = Original.lenCategory;
            foreach (ItemCategories cat in cats)
            {
                xSet.Remove((dynamic)Enum.ToObject(Utils.GetGameType("SoG.ItemCodex+ItemCategories"), (int)cat));
            }
        }

        public void SpawnOn(LocalGame game,Player player)
        {
            // While the Player fix is coming, this should work for now
            dynamic playaa = ((dynamic)Utils.GetGameType("SoG.Program").GetMethod("GetTheGame")?.Invoke(null, null)).xLocalPlayer;

            var function = Utils.GetGameType("SoG.Game1").GetDeclaredMethods("_EntityMaster_AddItem").First();
            function.Invoke(game.GetUnderlayingGame(), new[] { EnType, playaa.xEntity.xTransform.v2Pos, playaa.xEntity.xRenderComponent.fVirtualHeight, playaa.xEntity.xCollisionComponent.ibitCurrentColliderLayer, Vector2.Zero });
        }

        #region GetItemInstance patch

        private static bool OnGetItemInstancePrefix(ref dynamic __result, ref int enType)
        {
            if (CustomItem.ValueIsVanillaItem(enType))
            {
                // Continue with original
                return true;
            }

            dynamic items = Utils.GetGameType("SoG.ItemCodex").GetPublicStaticField("denxLoadedDescriptions");
            dynamic enumType = Enum.ToObject(Utils.GetGameType("SoG.ItemCodex+ItemTypes"), enType);

            if (!items.ContainsKey(enumType))
            {
                // No such item? Continue with original anyway...
                return true;
            }

            // Create new instance and return it, skipping original code
            __result = Activator.CreateInstance(Utils.GetGameType("SoG.Item"));

            // The "Regional" Content Manager is used here. Default shadow seems to be the rabby feet's shadow
            __result.enType = enumType;
            __result.xRenderComponent.txTexture = items[enumType].txDisplayImage;
            __result.xRenderComponent.txShadowTexture = ((dynamic)Utils.GetGameType("SoG.Program").GetMethod("GetTheGame")?.Invoke(null, null)).xLevelMaster.contRegionContent.Load<Texture2D>("Items/DropAppearance/hartass02");
            __result.sFullName = items[enumType].sFullName;

            if (items[enumType].lenCategory.Contains((dynamic)Enum.ToObject(Utils.GetGameType("SoG.ItemCodex+ItemCategories"), (int)ItemCategories.GrantToServer)))
            {
                __result.bGiveToServer = true;
            }
            __result.xCollisionComponent.xMovementCollider = Activator.CreateInstance(Utils.GetGameType("SoG.SphereCollider"), 10f, Vector2.Zero, __result.xTransform, 1f, __result);
            __result.xCollisionComponent.xMovementCollider.bCollideWithFlat = true;

            // Skip original code
            return false;
        }

        #endregion
    }

    public class CustomEquipmentInfo : ConvertedObject
    {
        public CustomEquipmentInfo(object originalObject) : base(originalObject)
        {

        }

        public int EnType
        {
            get => (int)_originalObject.enItemType;
            set => _originalObject.enItemType = (dynamic)Enum.ToObject(Utils.GetGameType("SoG.ItemCodex+ItemTypes"), value);
        }

        public static CustomEquipmentInfo AddEquipmentInfoForCustomItem(string resource, int enType)
        {
            if (!CustomItem.ValueIsDefinedModItem(enType))
            {
                Console.WriteLine("Error in AddEquipmentInfoForCustomItem(): Item " + enType + " has no defined ItemDescription.");
                return null;
            }
            CustomEquipmentInfo xInfo = new CustomEquipmentInfo(Activator.CreateInstance(Utils.GetGameType("SoG.EquipmentInfo"), resource, Enum.ToObject(Utils.GetGameType("SoG.ItemCodex+ItemTypes"), enType)));

            BaseScript.CustomEquipmentInfos.Add(xInfo);

            Console.WriteLine("Custom item with the id " + enType + " now has equipment info...");

            return xInfo;
        }

        // Valid Stats for Items: HP, EP, ATK, MATK, DEF, ASPD, CSPD, Crit, CritDMG, ShldHP, EPRegen, ShldRegen
        // Everything else is ignored for items (such as DamageResistance or whatever - those are used for buffs, etc)
        // The value 0 unsets a stat, if possible.
        // Best called with parameters specified in "paramName: value" format
        public void SetStatChanges(int HP = 0, int EP = 0, int ATK = 0, int MATK = 0, int DEF = 0, int ASPD = 0, int CSPD = 0, int Crit = 0, int CritDMG = 0, int ShldHP = 0, int EPRegen = 0, int ShldRegen = 0)
        {
            dynamic xDict = _originalObject.deniStatChanges;
            dynamic type = Utils.GetGameType("SoG.EquipmentInfo+StatEnum");

            // Sets / unsets for each stat
            if (HP == 0) xDict.Remove((dynamic)Enum.ToObject(type, (int)StatEnum.HP));
            else xDict.Add((dynamic)Enum.ToObject(type, (int)StatEnum.HP), HP);
            if (EP == 0) xDict.Remove((dynamic)Enum.ToObject(type, (int)StatEnum.EP));
            else xDict.Add((dynamic)Enum.ToObject(type, (int)StatEnum.EP), EP);
            if (ATK == 0) xDict.Remove((dynamic)Enum.ToObject(type, (int)StatEnum.ATK));
            else xDict.Add((dynamic)Enum.ToObject(type, (int)StatEnum.ATK), ATK);
            if (MATK == 0) xDict.Remove((dynamic)Enum.ToObject(type, (int)StatEnum.MATK));
            else xDict.Add((dynamic)Enum.ToObject(type, (int)StatEnum.MATK), MATK);
            if (DEF == 0) xDict.Remove((dynamic)Enum.ToObject(type, (int)StatEnum.DEF));
            else xDict.Add((dynamic)Enum.ToObject(type, (int)StatEnum.DEF), DEF);
            if (ASPD == 0) xDict.Remove((dynamic)Enum.ToObject(type, (int)StatEnum.ASPD));
            else xDict.Add((dynamic)Enum.ToObject(type, (int)StatEnum.ASPD), ASPD);
            if (CSPD == 0) xDict.Remove((dynamic)Enum.ToObject(type, (int)StatEnum.CSPD));
            else xDict.Add((dynamic)Enum.ToObject(type, (int)StatEnum.CSPD), CSPD);
            if (Crit == 0) xDict.Remove((dynamic)Enum.ToObject(type, (int)StatEnum.Crit));
            else xDict.Add((dynamic)Enum.ToObject(type, (int)StatEnum.Crit), Crit);
            if (CritDMG == 0) xDict.Remove((dynamic)Enum.ToObject(type, (int)StatEnum.CritDMG));
            else xDict.Add((dynamic)Enum.ToObject(type, (int)StatEnum.CritDMG), CritDMG);
            if (ShldHP == 0) xDict.Remove((dynamic)Enum.ToObject(type, (int)StatEnum.ShldHP));
            else xDict.Add((dynamic)Enum.ToObject(type, (int)StatEnum.ShldHP), ShldHP);
            if (EPRegen == 0) xDict.Remove((dynamic)Enum.ToObject(type, (int)StatEnum.EPRegen));
            else xDict.Add((dynamic)Enum.ToObject(type, (int)StatEnum.EPRegen), EPRegen);
            if (ShldRegen == 0) xDict.Remove((dynamic)Enum.ToObject(type, (int)StatEnum.ShldRegen));
            else xDict.Add((dynamic)Enum.ToObject(type, (int)StatEnum.ShldRegen), ShldRegen);
        }

        private static bool OnGetEquipmentInfoPrefix(ref dynamic __result, ref int enType)
        {
            if (CustomItem.ValueIsVanillaItem(enType))
            {
                // Continue with original
                return true;
            }

            if (!CustomItem.ValueIsDefinedModItem(enType))
            {
                // No such item? Continue with original anyway...
                return true;
            }

            // Return info stored in BaseScript
            int lmao = enType;
            __result = BaseScript.CustomEquipmentInfos.Find(info => (int)info.EnType == lmao).Original;

            // Skip original code
            return false;
        }
    }
}
