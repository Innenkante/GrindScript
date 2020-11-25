using System;
using System.Collections.Generic;
using System.ComponentModel;
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

            Console.WriteLine("Added the custom item called " + name + " with the id " + (400000 + newId) + "...");

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

    // Similar to CustomEquipmentInfo, but it has some extra info
    public class CustomFacegearInfo : CustomEquipmentInfo
    {
        public CustomFacegearInfo(object originalObject) : base(originalObject)
        {

        }

        public static CustomFacegearInfo AddFacegearInfoForCustomItem(string resource, int enType)
        {
            if (!CustomItem.ValueIsDefinedModItem(enType))
            {
                Console.WriteLine("Error in AddFacegearInfoForCustomItem(): Item " + enType + " has no defined ItemDescription.");
                return null;
            }

            // Deal with resources
            CustomFacegearInfo xInfo = new CustomFacegearInfo(Activator.CreateInstance(Utils.GetGameType("SoG.FacegearInfo"), Enum.ToObject(Utils.GetGameType("SoG.ItemCodex+ItemTypes"), enType)));
            xInfo.Original.sResourceName = resource;

            string sHatPath = "Sprites/Equipment/Facegear/" + resource + "/";
            dynamic Content = ((dynamic)Utils.GetGameType("SoG.Program").GetMethod("GetTheGame")?.Invoke(null, null)).Content;

            for (int i = 0; i < 4; i++)
            {
                string sDir = "Up";
                if (i == 1) sDir = "Right";
                else if (i == 2) sDir = "Down";
                else if (i == 3) sDir = "Left";
                try
                {
                    xInfo.Original.atxTextures[i] = Content.Load<Texture2D>(sHatPath + sDir);
                }
                catch(Exception e)
                {
                    Console.WriteLine("Exception for facegear " + enType + "'s " + sDir + " texture: " + e.Message + " -> Setting to txNullTex");
                    xInfo.Original.atxTextures[i] = Utils.GetGameType("SoG.RenderMaster").GetPublicStaticField("txNullTex");
                }
            }

            // "Default" params - modders should set these to the correct values later on, though
            xInfo.Original.av2RenderOffsets[0] = new Vector2(0f, 0f);
            xInfo.Original.av2RenderOffsets[1] = new Vector2(0f, 0f);
            xInfo.Original.av2RenderOffsets[2] = new Vector2(0f, 0f);
            xInfo.Original.av2RenderOffsets[3] = new Vector2(0f, 0f);

            xInfo.SetOverHair(true, true, true, true); // All above 
            xInfo.SetOverHat(false, false, false, false); // All behind hats
            xInfo.SetBehindCharacter(false, false, false, false); // Don't render behind character

            BaseScript.CustomFacegearInfos.Add(xInfo);

            Console.WriteLine("Custom item with the id " + enType + " now has facegear info...");

            return xInfo;
        }

        public void SetOverHair(bool up, bool right, bool down, bool left)
        {
            Original.abOverHair[0] = up;
            Original.abOverHair[1] = right;
            Original.abOverHair[2] = down;
            Original.abOverHair[3] = left;
        }

        public void SetOverHat(bool up, bool right, bool down, bool left)
        {
            Original.abOverHat[0] = up;
            Original.abOverHat[1] = right;
            Original.abOverHat[2] = down;
            Original.abOverHat[3] = left;
        }

        public void SetBehindCharacter(bool up, bool right, bool down, bool left)
        {
            Original.abBehindCharacter[0] = up;
            Original.abBehindCharacter[1] = right;
            Original.abBehindCharacter[2] = down;
            Original.abBehindCharacter[3] = left;
        }

        public void SetRenderOffsets(Vector2 up, Vector2 right, Vector2 down, Vector2 left)
        {
            if (up != null) Original.av2RenderOffsets[0] = new Vector2(up.X, up.Y);
            if (right != null) Original.av2RenderOffsets[1] = new Vector2(right.X, right.Y);
            if (down != null) Original.av2RenderOffsets[2] = new Vector2(down.X, down.Y);
            if (left != null) Original.av2RenderOffsets[3] = new Vector2(left.X, left.Y);
        }

        private static bool OnGetFacegearInfoPrefix(ref dynamic __result, ref int enType)
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
            __result = BaseScript.CustomFacegearInfos.Find(info => (int)info.EnType == lmao).Original;

            // Skip original code
            return false;
        }
    }

    // Similar to CustomEquipmentInfo, but it has some extra info
    public class CustomHatInfo : CustomEquipmentInfo
    {

        public CustomHatInfo(object originalObject) : base(originalObject)
        {

        }

        public static CustomHatInfo AddHatInfoForCustomItem(string resource, int enType)
        {
            if (!CustomItem.ValueIsDefinedModItem(enType))
            {
                Console.WriteLine("Error in AddHatInfoForCustomItem(): Item " + enType + " has no defined ItemDescription.");
                return null;
            }
            CustomHatInfo xInfo = new CustomHatInfo(Activator.CreateInstance(Utils.GetGameType("SoG.HatInfo"), Enum.ToObject(Utils.GetGameType("SoG.ItemCodex+ItemTypes"), enType)));
            xInfo.Original.sResourceName = resource;

            string sHatPath = "Sprites/Equipment/Hats/" + resource + "/";
            dynamic Content = ((dynamic)Utils.GetGameType("SoG.Program").GetMethod("GetTheGame")?.Invoke(null, null)).Content;

            for (int i = 0; i < 4; i++)
            {
                string sDir = "Up";
                if (i == 1) sDir = "Right";
                else if (i == 2) sDir = "Down";
                else if (i == 3) sDir = "Left";
                try
                {
                    xInfo.Original.xDefaultSet.atxTextures[i] = Content.Load<Texture2D>(sHatPath + sDir);
                }
                catch (Exception e)
                {
                    Console.WriteLine("Exception for hat " + enType + "'s " + sDir + " texture: " + e.Message + " -> Setting to txNullTex");
                    xInfo.Original.xDefaultSet.atxTextures[i] = Utils.GetGameType("SoG.RenderMaster").GetPublicStaticField("txNullTex");
                }
            }

            // "Default" params - modders should set these to the correct values later on, though
            xInfo.Original.xDefaultSet.av2RenderOffsets[0] = new Vector2(0f, 0f);
            xInfo.Original.xDefaultSet.av2RenderOffsets[1] = new Vector2(0f, 0f);
            xInfo.Original.xDefaultSet.av2RenderOffsets[2] = new Vector2(0f, 0f);
            xInfo.Original.xDefaultSet.av2RenderOffsets[3] = new Vector2(0f, 0f);

            xInfo.SetUnderHair(false, false, false, false); // All above 
            xInfo.SetBehindCharacter(false, false, false, false); // Don't render behind character
            xInfo.SetObstructions(true, true, false); // Obstruct all hair except bottom - default for most hats

            BaseScript.CustomHatInfos.Add(xInfo);

            Console.WriteLine("Custom item with the id " + enType + " now has hat info...");

            return xInfo;
        }

        public void SetUnderHair(bool up, bool right, bool down, bool left)
        {
            Original.xDefaultSet.abUnderHair[0] = up;
            Original.xDefaultSet.abUnderHair[1] = right;
            Original.xDefaultSet.abUnderHair[2] = down;
            Original.xDefaultSet.abUnderHair[3] = left;
        }

        public void SetBehindCharacter(bool up, bool right, bool down, bool left)
        {
            Original.xDefaultSet.abBehindCharacter[0] = up;
            Original.xDefaultSet.abBehindCharacter[1] = right;
            Original.xDefaultSet.abBehindCharacter[2] = down;
            Original.xDefaultSet.abBehindCharacter[3] = left;
        }

        public void SetObstructions(bool top, bool sides, bool bottom)
        {
            Original.xDefaultSet.bObstructsTop = top;
            Original.xDefaultSet.bObstructsSides = sides;
            Original.xDefaultSet.bObstructsBottom = bottom;
        }

        public void SetRenderOffsets(Vector2 up, Vector2 right, Vector2 down, Vector2 left)
        {
            if (up != null) Original.xDefaultSet.av2RenderOffsets[0] = new Vector2(up.X, up.Y);
            if (right != null) Original.xDefaultSet.av2RenderOffsets[1] = new Vector2(right.X, right.Y);
            if (down != null) Original.xDefaultSet.av2RenderOffsets[2] = new Vector2(down.X, down.Y);
            if (left != null) Original.xDefaultSet.av2RenderOffsets[3] = new Vector2(left.X, left.Y);
        }

        public void AddAltSet(int enType)
        {
            dynamic enumType = Enum.ToObject(Utils.GetGameType("SoG.ItemCodex+ItemTypes"), enType);
            if (!Original.denxAlternateVisualSets.Contains(enumType))
            {
                Original.denxAlternateVisualSets[enumType] = Activator.CreateInstance(Utils.GetGameType("SoG.HatInfo+VisualSet"));
            }

            // Copy textures...
            string sHatPath = "Sprites/Equipment/Facegear/" + Original.sResourceName + "/";
            dynamic Content = ((dynamic)Utils.GetGameType("SoG.Program").GetMethod("GetTheGame")?.Invoke(null, null)).Content;
            dynamic xAlt = Original.denxAlternateVisualSets[enumType];
            for (int i = 0; i < 4; i++)
            {
                string sDir = "Up";
                if (i == 1) sDir = "Right";
                else if (i == 2) sDir = "Down";
                else if (i == 3) sDir = "Left";
                try
                {
                    xAlt.atxTextures[i] = Content.Load<Texture2D>(sHatPath + sDir);
                }
                catch (Exception e)
                {
                    Console.WriteLine("Exception for hat " + enType + "'s " + sDir + " texture: " + e.Message + " -> Setting to txNullTex");
                    xAlt.atxTextures[i] = Utils.GetGameType("SoG.RenderMaster").GetPublicStaticField("txNullTex");
                }
            }

            // "Default" params - modders should set these to the correct values later on, though
            xAlt.av2RenderOffsets[0] = new Vector2(0f, 0f);
            xAlt.av2RenderOffsets[1] = new Vector2(0f, 0f);
            xAlt.av2RenderOffsets[2] = new Vector2(0f, 0f);
            xAlt.av2RenderOffsets[3] = new Vector2(0f, 0f);

            SetUnderHairForAltSet(enType, false, false, false, false); // All above 
            SetBehindCharacterForAltSet(enType, false, false, false, false); // Don't render behind character
            SetObstructionsForAltSet(enType, true, true, false); // Obstruct all hair except bottom - default for most hats
        }

        public void SetUnderHairForAltSet(int enType, bool up, bool right, bool down, bool left)
        {
            dynamic enumType = Enum.ToObject(Utils.GetGameType("SoG.ItemCodex+ItemTypes"), enType);
            if (!Original.denxAlternateVisualSets.Contains(enumType))
            {
                Console.WriteLine("Can't set alternate visual set parameters for item " + EnType + " -> " + enType + " if alt set hasn't been created!");
            }
            Original.denxAlternateVisualSets[enumType].abUnderHair[0] = up;
            Original.denxAlternateVisualSets[enumType].abUnderHair[1] = right;
            Original.denxAlternateVisualSets[enumType].abUnderHair[2] = down;
            Original.denxAlternateVisualSets[enumType].abUnderHair[3] = left;
        }

        public void SetBehindCharacterForAltSet(int enType, bool up, bool right, bool down, bool left)
        {
            dynamic enumType = Enum.ToObject(Utils.GetGameType("SoG.ItemCodex+ItemTypes"), enType);
            if (!Original.denxAlternateVisualSets.Contains(enumType))
            {
                Console.WriteLine("Can't set alternate visual set parameters for item " + EnType + " -> " + enType + " if alt set hasn't been created!");
            }
            Original.denxAlternateVisualSets[enumType].abBehindCharacter[0] = up;
            Original.denxAlternateVisualSets[enumType].abBehindCharacter[1] = right;
            Original.denxAlternateVisualSets[enumType].abBehindCharacter[2] = down;
            Original.denxAlternateVisualSets[enumType].abBehindCharacter[3] = left;
        }

        public void SetObstructionsForAltSet(int enType, bool top, bool sides, bool bottom)
        {
            dynamic enumType = Enum.ToObject(Utils.GetGameType("SoG.ItemCodex+ItemTypes"), enType);
            if (!Original.denxAlternateVisualSets.Contains(enumType))
            {
                Console.WriteLine("Can't set alternate visual set parameters for item " + EnType + " -> " + enType + " if alt set hasn't been created!");
            }
            Original.denxAlternateVisualSets[enumType].bObstructsTop = top;
            Original.denxAlternateVisualSets[enumType].bObstructsSides = sides;
            Original.denxAlternateVisualSets[enumType].bObstructsBottom = bottom;
        }

        public void SetRenderOffsetsForAltSet(int enType, Vector2 up, Vector2 right, Vector2 down, Vector2 left)
        {
            dynamic enumType = Enum.ToObject(Utils.GetGameType("SoG.ItemCodex+ItemTypes"), enType);
            if (!Original.denxAlternateVisualSets.Contains(enumType))
            {
                Console.WriteLine("Can't set alternate visual set parameters for item " + EnType + " -> " + enType + " if alt set hasn't been created!");
            }
            if (up != null) Original.denxAlternateVisualSets[enumType].av2RenderOffsets[0] = new Vector2(up.X, up.Y);
            if (right != null) Original.denxAlternateVisualSets[enumType].av2RenderOffsets[1] = new Vector2(right.X, right.Y);
            if (down != null) Original.denxAlternateVisualSets[enumType].av2RenderOffsets[2] = new Vector2(down.X, down.Y);
            if (left != null) Original.denxAlternateVisualSets[enumType].av2RenderOffsets[3] = new Vector2(left.X, left.Y);
        }

        private static bool OnGetHatInfoPrefix(ref dynamic __result, ref int enType)
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
            __result = BaseScript.CustomHatInfos.Find(info => (int)info.EnType == lmao).Original;

            // Skip original code
            return false;
        }
    }

    public class CustomWeaponInfo : CustomEquipmentInfo
    {
        public CustomWeaponInfo(object originalObject) : base(originalObject)
        {

        }

        public static CustomWeaponInfo AddWeaponInfoForCustomItem(string resource, int enType, int enWeaponCategory, bool isMagicWeapon, string palette = "")
        {
            if (!CustomItem.ValueIsDefinedModItem(enType))
            {
                Console.WriteLine("Error in AddWeaponInfoForCustomItem(): Item " + enType + " has no defined ItemDescription.");
                return null;
            }

            // For now, no custom palettes. C'est la vie
            palette = "blueish";
            CustomWeaponInfo xInfo = new CustomWeaponInfo(Activator.CreateInstance(Utils.GetGameType("SoG.WeaponInfo"), resource, Enum.ToObject(Utils.GetGameType("SoG.ItemCodex+ItemTypes"), enType), Enum.ToObject(Utils.GetGameType("SoG.WeaponInfo+WeaponCategory"), enWeaponCategory), palette));

            // Standard weapon multipliers and stuff

            xInfo.Original.enAutoAttackSpell = (dynamic)Enum.ToObject(Utils.GetGameType("SoG.WeaponInfo+AutoAttackSpell"), (int)AutoAttackSpell.None);
            if (enWeaponCategory == (int)WeaponCategory.OneHanded)
            {
                if (isMagicWeapon)
                {
                    xInfo.Original.enAutoAttackSpell = (dynamic)Enum.ToObject(Utils.GetGameType("SoG.WeaponInfo+AutoAttackSpell"), (int)AutoAttackSpell.Generic1H);
                }
                xInfo.Original.iDamageMultiplier = 90;
            }
            else if (enWeaponCategory == (int)WeaponCategory.TwoHanded)
            {
                if (isMagicWeapon)
                {
                    xInfo.Original.enAutoAttackSpell = (dynamic)Enum.ToObject(Utils.GetGameType("SoG.WeaponInfo+AutoAttackSpell"), (int)AutoAttackSpell.Generic2H);
                }
                xInfo.Original.iDamageMultiplier = 125;
            }

            BaseScript.CustomWeaponInfos.Add(xInfo);

            Console.WriteLine("Custom item with the id " + enType + " now has weapon info...");

            return xInfo;
        }

        private static bool OnGetWeaponInfoPrefix(ref dynamic __result, ref int enType)
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
            __result = BaseScript.CustomWeaponInfos.Find(info => (int)info.EnType == lmao).Original;

            // Skip original code
            return false;
        }
    }

}
