using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Schema;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace SoG.GrindScript
{
    public class ModItem : ConvertedObject
    {
        public const int BaseItemTypesPos = 400000;

        public ModEquipment EquipInfo { get; private set; }

        public ModFacegear FacegearInfo { get => EquipInfo as ModFacegear; }

        public ModHat HatInfo { get => EquipInfo as ModHat; }

        public ModWeapon WeaponInfo { get => EquipInfo as ModWeapon; }

        private ContentManager _managerToUse = null;

        public ContentManager ManagerToUse
        {
            get 
            {
                if(_managerToUse == null)
                {
                    return Utils.GetTheGame().Content;
                }
                else
                {
                    return _managerToUse;
                }
            }
            set => _managerToUse = value;
        }

        public int RelativeID { get; set; }

        public int IntType
        {
            get => (int)_originalObject.enType;
            set => _originalObject.enType = Utils.GetEnumObject("SoG.ItemCodex+ItemTypes", value);
        }

        public bool IsVanillaItem 
            => Enum.IsDefined(Utils.GetGameType("SoG.ItemCodex+ItemTypes"), IntType);

        public bool IsModItem
            => !IsVanillaItem && IntType >= BaseItemTypesPos && IntType <= BaseItemTypesPos + BaseScript.CustomItems.Count;

        public ModItem(object originalObject) : base(originalObject)
        {
            
        }

        public static bool ValueIsVanillaItem(int intType)
        {
            return Enum.IsDefined(Utils.GetGameType("SoG.ItemCodex+ItemTypes"), intType);
        }

        public static bool ValueIsModItem(int intType)
        {
            return !ValueIsVanillaItem(intType) && intType >= BaseItemTypesPos && intType <= BaseItemTypesPos + BaseScript.CustomItems.Count;
        }

        public void AddItemCategories(params ItemCategories[] categories)
        {
            dynamic xSet = Original.lenCategory;
            foreach (ItemCategories cat in categories)
            {
                xSet.Add(Utils.GetEnumObject("SoG.ItemCodex+ItemCategories", (int)cat));
            }
        }

        public void RemoveItemCategories(params ItemCategories[] categories)
        {
            dynamic xSet = Original.lenCategory;
            foreach (ItemCategories cat in categories)
            {
                xSet.Remove(Utils.GetEnumObject("SoG.ItemCodex+ItemCategories", (int)cat));
            }
        }

        public void SpawnOn(LocalGame game,Player player)
        {
            // While the Player fix is coming, this should work for now
            dynamic playaa = game.GetUnderlayingGame().xLocalPlayer;

            var function = Utils.GetGameType("SoG.Game1").GetDeclaredMethods("_EntityMaster_AddItem").First();
            function.Invoke(game.GetUnderlayingGame(), new[] { IntType, playaa.xEntity.xTransform.v2Pos, playaa.xEntity.xRenderComponent.fVirtualHeight, playaa.xEntity.xCollisionComponent.ibitCurrentColliderLayer, Vector2.Zero });
        }

        public void SpawnAt(Vector2 v2Pos, float fVirtualHeight, int colliderLayer, Vector2 v2Dir)
        {
            if(v2Pos == null || v2Dir == null)
            {
                Console.WriteLine("Tried to spawn item at null position or with null direction!");
                return;
            }

            dynamic game = Utils.GetTheGame();

            var function = Utils.GetGameType("SoG.Game1").GetDeclaredMethods("_EntityMaster_AddItem").First();
            function.Invoke(game, new dynamic[] { IntType, v2Pos, fVirtualHeight, colliderLayer, v2Dir });
        }

        private static bool OnGetItemInstancePrefix(ref dynamic __result, ref int enType)
        {
            if (ModItem.ValueIsVanillaItem(enType))
            {
                // Continue with original
                return true;
            }

            dynamic items = Utils.GetGameType("SoG.ItemCodex").GetPublicStaticField("denxLoadedDescriptions");
            dynamic enumType = Utils.GetEnumObject("SoG.ItemCodex+ItemTypes", enType);

            if (!items.ContainsKey(enumType))
            {
                // No such item? Continue with original anyway...
                return true;
            }

            // Create new instance and return it, skipping original code
            __result = Utils.DefaultConstructObject("SoG.Item");

            // The "Regional" Content Manager is used here. Default shadow seems to be the rabby feet's shadow
            __result.enType = enumType;
            __result.xRenderComponent.txTexture = items[enumType].txDisplayImage;
            __result.xRenderComponent.txShadowTexture = Utils.GetTheGame().xLevelMaster.contRegionContent.Load<Texture2D>("Items/DropAppearance/hartass02");
            __result.sFullName = items[enumType].sFullName;

            if (items[enumType].lenCategory.Contains(Utils.GetEnumObject("SoG.ItemCodex+ItemCategories", (int)ItemCategories.GrantToServer)))
            {
                __result.bGiveToServer = true;
            }

            __result.xCollisionComponent.xMovementCollider = Utils.ConstructObject("SoG.SphereCollider", 10f, Vector2.Zero, __result.xTransform, 1f, __result);
            __result.xCollisionComponent.xMovementCollider.bCollideWithFlat = true;

            // Skip original code
            return false;
        }

        public static ModItem AddItem(string name, string description, string appearance, int value, ContentManager manager)
        {
            dynamic Content = manager ?? Utils.GetTheGame().Content;
            int newId = BaseScript.CustomItems.Count + 1;
            dynamic newItem = Utils.DefaultConstructObject("SoG.ItemDescription");
            string baseEntryName = name.Replace(" ", "");

            Ui.AddMiscTextTo("Items", baseEntryName + "_Name", name, MiscTextTypes.GenericItemName);
            Ui.AddMiscTextTo("Items", baseEntryName + "_Description", description, MiscTextTypes.GenericItemDescription);

            newItem.sFullName = name;
            newItem.txDisplayImage = Content.Load<Texture2D>(appearance);
            newItem.sNameLibraryHandle = baseEntryName + "_Name";
            newItem.sDescriptionLibraryHandle = baseEntryName + "_Description";
            newItem.sCategory = "";
            newItem.iInternalLevel = 1;
            newItem.iValue = value;
            newItem.enType = Utils.GetEnumObject("SoG.ItemCodex+ItemTypes", BaseItemTypesPos + newId);

            dynamic items = Utils.GetGameType("SoG.ItemCodex").GetPublicStaticField("denxLoadedDescriptions");
            items[newItem.enType] = newItem;

            var customItem = new ModItem(newItem) { RelativeID = newId };
            BaseScript.CustomItems.Add(customItem);

            customItem.ManagerToUse = Content;

            Console.WriteLine("Added custom item" + name + " with game ID " + (BaseItemTypesPos + newId) + ", relative ID " + newId + "...");
            return customItem;
        }

        public ModEquipment AddEquipmentInfo(string resource)
        {
            int intType = this.IntType;
            if(EquipInfo != null)
            {
                Console.WriteLine("Error: Item " + intType + " already has equipment info!");
                return null;
            }

            if (!ModItem.ValueIsModItem(intType))
            {
                Console.WriteLine("Error: Can't add equipment info to item " + intType + " since it has no defined ItemDescription!");
                return null;
            }

            EquipInfo = new ModEquipment(Activator.CreateInstance(Utils.GetGameType("SoG.EquipmentInfo"), resource, Enum.ToObject(Utils.GetGameType("SoG.ItemCodex+ItemTypes"), intType)));
            Console.WriteLine("Custom item with the id " + intType + " now has equipment info...");

            return EquipInfo;
        }

        public ModFacegear AddFacegearInfo(string resource)
        {
            int intType = this.IntType;
            if (EquipInfo != null)
            {
                Console.WriteLine("Error: Item " + intType + " already has equipment info!");
                return null;
            }

            if (!ModItem.ValueIsModItem(intType))
            {
                Console.WriteLine("Error: Can't add equipment info to item " + intType + " since it has no defined ItemDescription!");
                return null;
            }

            // Deal with resources
            ModFacegear xInfo = new ModFacegear(Utils.ConstructObject("SoG.FacegearInfo", Utils.GetEnumObject("SoG.ItemCodex+ItemTypes", intType)));
            xInfo.Original.sResourceName = resource;

            string sHatPath = "Sprites/Equipment/Facegear/" + resource + "/";
            dynamic Content = Utils.GetTheGame().Content;

            string[] sDirs = { "Up", "Right", "Down", "Left" };
            for (int i = 0; i <= 3; i++)
            {
                try
                {
                    xInfo.Original.atxTextures[i] = Content.Load<Texture2D>(sHatPath + sDirs[i]);
                }
                catch (Exception e)
                {
                    Console.WriteLine("Exception for facegear " + intType + "'s " + sDirs[i] + " texture: " + e.Message + ". Setting to txNullTex.");
                    xInfo.Original.atxTextures[i] = Utils.GetGameType("SoG.RenderMaster").GetPublicStaticField("txNullTex");
                }

                xInfo.Original.av2RenderOffsets[i] = new Vector2(0f, 0f);
            }

            // "Default" params - modders should set these to the correct values later on, though
            xInfo.SetOverHair(true, true, true, true); // All above 
            xInfo.SetOverHat(false, false, false, false); // All behind hats
            xInfo.SetBehindCharacter(false, false, false, false); // Don't render behind character

            EquipInfo = xInfo;

            Console.WriteLine("Custom item with the id " + intType + " now has facegear info...");
            return EquipInfo as ModFacegear;
        }

        public ModHat AddHatInfo(string resource, bool doubleSlot = false)
        {
            int intType = this.IntType;
            if (EquipInfo != null)
            {
                Console.WriteLine("Error: Item " + intType + " already has equipment info!");
                return null;
            }

            if (!ModItem.ValueIsModItem(intType))
            {
                Console.WriteLine("Error: Can't add equipment info to item " + intType + " since it has no defined ItemDescription!");
                return null;
            }

            ModHat xInfo = new ModHat(Activator.CreateInstance(Utils.GetGameType("SoG.HatInfo"), Enum.ToObject(Utils.GetGameType("SoG.ItemCodex+ItemTypes"), intType)));
            xInfo.Original.sResourceName = resource;

            string sHatPath = "Sprites/Equipment/Hats/" + resource + "/";
            dynamic Content = ((dynamic)Utils.GetGameType("SoG.Program").GetMethod("GetTheGame")?.Invoke(null, null)).Content;

            string[] sDirs = { "Up", "Right", "Down", "Left" };
            for (int i = 0; i <= 3; i++)
            {
                try
                {
                    xInfo.Original.xDefaultSet.atxTextures[i] = Content.Load<Texture2D>(sHatPath + sDirs[i]);
                }
                catch (Exception e)
                {
                    Console.WriteLine("Exception for hat " + intType + "'s " + sDirs[i] + " texture: " + e.Message + " -> Setting to txNullTex");
                    xInfo.Original.xDefaultSet.atxTextures[i] = Utils.GetGameType("SoG.RenderMaster").GetPublicStaticField("txNullTex");
                }

                xInfo.Original.xDefaultSet.av2RenderOffsets[i] = new Vector2(0f, 0f);
            }

            // "Default" params - modders should set these to the correct values later on, though
            xInfo.SetUnderHair(false, false, false, false); // All above 
            xInfo.SetBehindCharacter(false, false, false, false); // Don't render behind character
            xInfo.SetObstructions(true, true, false); // Obstruct all hair except bottom - default for most hats
            xInfo.Original.bDoubleSlot = doubleSlot; // if true, the slot is a mask and prevents facegear equip

            EquipInfo = xInfo;

            Console.WriteLine("Custom item with the id " + intType + " now has hat info...");
            return EquipInfo as ModHat;
        }

        public ModWeapon AddWeaponInfo(string resource, int enWeaponCategory, bool isMagicWeapon, string palette = "")
        {
            int intType = this.IntType;
            if (EquipInfo != null)
            {
                Console.WriteLine("Error: Item " + intType + " already has equipment info!");
                return null;
            }

            if (!ModItem.ValueIsModItem(intType))
            {
                Console.WriteLine("Error: Can't add equipment info to item " + intType + " since it has no defined ItemDescription!");
                return null;
            }

            // For now, no custom palettes. C'est la vie
            palette = "blueish";
            ModWeapon xInfo = new ModWeapon(Utils.ConstructObject("SoG.WeaponInfo", resource, Utils.GetEnumObject("SoG.ItemCodex+ItemTypes", intType), Utils.GetEnumObject("SoG.WeaponInfo+WeaponCategory", enWeaponCategory), palette));

            // Standard weapon multipliers and stuff

            xInfo.Original.enAutoAttackSpell = Utils.GetEnumObject("SoG.WeaponInfo+AutoAttackSpell", (int)AutoAttackSpell.None);
            if (enWeaponCategory == (int)WeaponCategory.OneHanded)
            {
                if (isMagicWeapon)
                {
                    xInfo.Original.enAutoAttackSpell = Utils.GetEnumObject("SoG.WeaponInfo+AutoAttackSpell", (int)AutoAttackSpell.Generic1H);
                }
                xInfo.Original.iDamageMultiplier = 90;
            }
            else if (enWeaponCategory == (int)WeaponCategory.TwoHanded)
            {
                if (isMagicWeapon)
                {
                    xInfo.Original.enAutoAttackSpell = Utils.GetEnumObject("SoG.WeaponInfo+AutoAttackSpell", (int)AutoAttackSpell.Generic2H);
                }
                xInfo.Original.iDamageMultiplier = 125;
            }

            EquipInfo = xInfo;

            Console.WriteLine("Custom item with the id " + intType + " now has weapon info...");
            return EquipInfo as ModWeapon;
        }

    }

    public class ModEquipment : ConvertedObject
    {
        public ModEquipment(object originalObject) : base(originalObject)
        {

        }

        public int IntType
        {
            get => (int)_originalObject.enItemType;
            set => _originalObject.enItemType = Utils.GetEnumObject("SoG.ItemCodex+ItemTypes", value);
        }

        public void SetStats(int HP = 0, int EP = 0, int ATK = 0, int MATK = 0, int DEF = 0, int ASPD = 0, int CSPD = 0, int Crit = 0, int CritDMG = 0, int ShldHP = 0, int EPRegen = 0, int ShldRegen = 0)
        {
            // Valid Stats for Items: HP, EP, ATK, MATK, DEF, ASPD, CSPD, Crit, CritDMG, ShldHP, EPRegen, ShldRegen
            // Everything else is ignored for items (such as DamageResistance or whatever - those are used for buffs, etc)
            // The value 0 unsets a stat, if possible.
            // Best called with parameters specified in "paramName: value" format
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
            if (ModItem.ValueIsVanillaItem(enType) || !ModItem.ValueIsModItem(enType))
            {
                // Borked - continue with original
                return true;
            }
            int lmao = enType;
            try
            {
                __result = BaseScript.CustomItems.Find(info => (int)info.IntType == lmao && info.EquipInfo.GetType() == typeof(ModEquipment)).EquipInfo.Original;
            }
            catch(Exception e)
            {
                Console.WriteLine("OnGetWeaponPrefix failed! Error: " + e.Message + ". Item that caused it: " + enType);
                throw;
            }
            // Return info from BaseScript, skip original code
            return false;
        }
    }

    public class ModFacegear : ModEquipment
    {
        public ModFacegear(object originalObject) : base(originalObject)
        {

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
            if (ModItem.ValueIsVanillaItem(enType) || !ModItem.ValueIsModItem(enType))
            {
                // Borked - continue with original
                return true;
            }

            int lmao = enType;
            try 
            {
                __result = BaseScript.CustomItems.Find(info => (int)info.IntType == lmao && info.EquipInfo.GetType() == typeof(ModFacegear)).FacegearInfo.Original;
            }
            catch(Exception e)
            {
                Console.WriteLine("OnGetWeaponPrefix failed! Error: " + e.Message + ". Item that caused it: " + enType);
                throw;
            }
            // Return info stored in BaseScript and skip original code
            return false;
        }
    }

    public class ModHat : ModEquipment
    {

        public ModHat(object originalObject) : base(originalObject)
        {

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
            dynamic enumType = Utils.GetEnumObject("SoG.ItemCodex+ItemTypes", enType);
            if (!Original.denxAlternateVisualSets.Contains(enumType))
            {
                Original.denxAlternateVisualSets[enumType] = Utils.DefaultConstructObject("SoG.HatInfo+VisualSet");
            }

            // Copy textures...
            string sHatPath = "Sprites/Equipment/Facegear/" + Original.sResourceName + "/";
            dynamic Content = Utils.GetTheGame().Content;
            dynamic xAlt = Original.denxAlternateVisualSets[enumType];

            string[] sDirs = { "Up", "Right", "Down", "Left" };
            for (int i = 0; i <= 3; i++)
            {
                try
                {
                    xAlt.atxTextures[i] = Content.Load<Texture2D>(sHatPath + sDirs[i]);
                }
                catch (Exception e)
                {
                    Console.WriteLine("Exception for hat " + enType + "'s " + sDirs[i] + " texture: " + e.Message + " -> Setting to txNullTex");
                    xAlt.atxTextures[i] = Utils.GetGameType("SoG.RenderMaster").GetPublicStaticField("txNullTex");
                }
                xAlt.av2RenderOffsets[i] = new Vector2(0f, 0f);
            }

            // "Default" params - modders should set these to the correct values later on, though
            SetUnderHairForAltSet(enType, false, false, false, false); // All above 
            SetBehindCharacterForAltSet(enType, false, false, false, false); // Don't render behind character
            SetObstructionsForAltSet(enType, true, true, false); // Obstruct all hair except bottom - default for most hats
        }

        public void SetUnderHairForAltSet(int enType, bool up, bool right, bool down, bool left)
        {
            dynamic enumType = Utils.GetEnumObject("SoG.ItemCodex+ItemTypes", enType);
            if (!Original.denxAlternateVisualSets.Contains(enumType))
            {
                Console.WriteLine("Can't set alternate visual set parameters for item " + IntType + " -> " + enType + " if alt set hasn't been created!");
            }
            Original.denxAlternateVisualSets[enumType].abUnderHair[0] = up;
            Original.denxAlternateVisualSets[enumType].abUnderHair[1] = right;
            Original.denxAlternateVisualSets[enumType].abUnderHair[2] = down;
            Original.denxAlternateVisualSets[enumType].abUnderHair[3] = left;
        }

        public void SetBehindCharacterForAltSet(int enType, bool up, bool right, bool down, bool left)
        {
            dynamic enumType = Utils.GetEnumObject("SoG.ItemCodex+ItemTypes", enType);
            if (!Original.denxAlternateVisualSets.Contains(enumType))
            {
                Console.WriteLine("Can't set alternate visual set parameters for item " + IntType + " -> " + enType + " if alt set hasn't been created!");
            }
            Original.denxAlternateVisualSets[enumType].abBehindCharacter[0] = up;
            Original.denxAlternateVisualSets[enumType].abBehindCharacter[1] = right;
            Original.denxAlternateVisualSets[enumType].abBehindCharacter[2] = down;
            Original.denxAlternateVisualSets[enumType].abBehindCharacter[3] = left;
        }

        public void SetObstructionsForAltSet(int enType, bool top, bool sides, bool bottom)
        {
            dynamic enumType = Utils.GetEnumObject("SoG.ItemCodex+ItemTypes", enType);
            if (!Original.denxAlternateVisualSets.Contains(enumType))
            {
                Console.WriteLine("Can't set alternate visual set parameters for item " + IntType + " -> " + enType + " if alt set hasn't been created!");
            }
            Original.denxAlternateVisualSets[enumType].bObstructsTop = top;
            Original.denxAlternateVisualSets[enumType].bObstructsSides = sides;
            Original.denxAlternateVisualSets[enumType].bObstructsBottom = bottom;
        }

        public void SetRenderOffsetsForAltSet(int enType, Vector2 up, Vector2 right, Vector2 down, Vector2 left)
        {
            dynamic enumType = Utils.GetEnumObject("SoG.ItemCodex+ItemTypes", enType);
            if (!Original.denxAlternateVisualSets.Contains(enumType))
            {
                Console.WriteLine("Can't set alternate visual set parameters for item " + IntType + " -> " + enType + " if alt set hasn't been created!");
            }
            if (up != null) Original.denxAlternateVisualSets[enumType].av2RenderOffsets[0] = new Vector2(up.X, up.Y);
            if (right != null) Original.denxAlternateVisualSets[enumType].av2RenderOffsets[1] = new Vector2(right.X, right.Y);
            if (down != null) Original.denxAlternateVisualSets[enumType].av2RenderOffsets[2] = new Vector2(down.X, down.Y);
            if (left != null) Original.denxAlternateVisualSets[enumType].av2RenderOffsets[3] = new Vector2(left.X, left.Y);
        }

        private static bool OnGetHatInfoPrefix(ref dynamic __result, ref int enType)
        {
            if (ModItem.ValueIsVanillaItem(enType) || !ModItem.ValueIsModItem(enType))
            {
                // Borked - continue with original
                return true;
            }

            int lmao = enType;
            try 
            {
                __result = BaseScript.CustomItems.Find(info => (int)info.IntType == lmao && info.EquipInfo.GetType() == typeof(ModHat)).HatInfo.Original;
            }
            catch(Exception e)
            {
                Console.WriteLine("OnGetWeaponPrefix failed! Error: " + e.Message + ". Item that caused it: " + enType);
                throw;
            }
            // Return info stored in BaseScript and skip original code
            return false;
        }
    }

    public class ModWeapon : ModEquipment
    {
        public ModWeapon(object originalObject) : base(originalObject)
        {

        }

        private static bool OnGetWeaponInfoPrefix(ref dynamic __result, ref int enType)
        {
            if (ModItem.ValueIsVanillaItem(enType) || !ModItem.ValueIsModItem(enType))
            {
                // Borked - continue with original
                return true;
            }

            int lmao = enType;
            try
            {
                __result = BaseScript.CustomItems.Find(info => (int)info.IntType == lmao && info.EquipInfo.GetType() == typeof(ModWeapon)).WeaponInfo.Original;
            }
            catch(Exception e)
            {
                Console.WriteLine("OnGetWeaponPrefix failed! Error: " + e.Message + ". Item that caused it: " + enType);
                throw;
            }

            // Return info stored in BaseScript, skip original code
            return false;
        }
    }

}
