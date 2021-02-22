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
using System.Reflection;
using StatEnum = SoG.EquipmentInfo.StatEnum;

// TO DO:
// Add price / blood price / arcade mod for items
// Add sorting (iLevel, sCategory, etc.)

namespace SoG.GrindScript
{
    
    /// <summary>
    /// Helper class that stores additional information for modded items.
    /// Used internally by GrindScript.
    /// </summary>
    public class ModItemData
    {
        public ItemCodex.ItemTypes enType;

        public Texture2D txItemShadow;

        public ContentManager xContentManager;

        public EquipmentInfo xEquipmentData;

        public ModItemData(ItemCodex.ItemTypes enType)
        {
            this.enType = enType;
        }
    }

    /// <summary> Contains methods for working with items. </summary>
    public static class ItemHelper
    {

        // Static setup methods

        /// <summary> Creates a new ItemDescription that can be used by the game. </summary>
        public static ItemDescription CreateItemDescription(string sName, string sDesc, string sDisplayImage, ContentManager xContent, string sShadow = "")
        {
            string sEntry = sName.Replace(" ", "");

            ItemDescription xDesc = new ItemDescription()
            {
                sFullName = sName,
                sDescription = sDesc,
                sNameLibraryHandle = sEntry + "_Name",
                sDescriptionLibraryHandle = sEntry + "_Description",
                sCategory = "",
                iInternalLevel = 1,
                iValue = 1,
                enType = ModLibrary.ItemTypesNext
            };

            ItemCodex.denxLoadedDescriptions[xDesc.enType] = xDesc;
            ModLibrary.ItemDetails.Add(xDesc.enType, new ModItemData(xDesc.enType));

            Ui.AddMiscText("Items", xDesc.sNameLibraryHandle, sName, MiscTextTypes.GenericItemName);
            Ui.AddMiscText("Items", xDesc.sDescriptionLibraryHandle, sDesc, MiscTextTypes.GenericItemDescription);

            xDesc.SetTextures(sDisplayImage, xContent, sShadow);

            return xDesc;
        }

        // ItemTypes extensions

        public static bool IsSoGItem(this ItemCodex.ItemTypes enType)
        {
            return Enum.IsDefined(typeof(ItemCodex.ItemTypes), enType);
        }

        public static bool IsModItem(this ItemCodex.ItemTypes enType)
        {
            return enType >= ModLibrary.ItemTypesStart && enType < ModLibrary.ItemTypesNext;
        }

        // ItemDescription extensions

        public static void AddCategories(this ItemDescription xDesc, params ItemCodex.ItemCategories[] p_enCategories)
        {
            foreach (var enCat in p_enCategories)
            {
                xDesc.lenCategory.Add(enCat);
            }
        }

        public static void RemoveCategories(this ItemDescription xDesc, params ItemCodex.ItemCategories[] p_enCategories)
        {
            foreach (var enCat in p_enCategories)
            {
                xDesc.lenCategory.Remove(enCat);
            }
        }


        /// <summary>
        /// Sets the pricing for the item.
        /// Setting the blood cost overrides the game's calculated HP cost for Shadier Merchant items.
        /// </summary>
        public static void SetPriceValues(this ItemDescription xDesc, int iValue, float fArcadeCostModifier = 1f, int iBloodCost = 0)
        {
            xDesc.iValue = iValue;
            xDesc.fArcadeModeCostModifier = fArcadeCostModifier;
            xDesc.iOverrideBloodValue = iBloodCost;
        }

        /// <summary>
        /// Sets the textures used by the item. 
        /// <para>The path to the texture is of format "Items/DropAppearance/[resource].xnb".</para>
        /// </summary>
        /// <remarks> If no shadow texture is specified, a default texture will be used instead. </remarks>
        public static void SetTextures(this ItemDescription xDesc, string sDisplayImage, ContentManager xContent, string sCustomShadow = "")
        {
            xDesc.txDisplayImage = xContent.Load<Texture2D>("Items/DropAppearance/" + sDisplayImage);

            if (sCustomShadow != "")
            {
                ModLibrary.ItemDetails[xDesc.enType].txItemShadow = xContent.Load<Texture2D>("Items/DropAppearance/" + sCustomShadow);
            }
            else
            {
                // Keep in mind that stuff doesn't get unloaded from the main Content Manager
                ModLibrary.ItemDetails[xDesc.enType].txItemShadow = Utils.GetTheGame().Content.Load<Texture2D>("Items/DropAppearance/hartass02");
            }
        }

        /// <summary> Spawns an item at the target player's position. </summary>
        public static Item SpawnItem(this ItemDescription xDesc, PlayerView xTarget)
        {
            PlayerEntity xEntity = xTarget.xEntity;

            return xDesc.SpawnItem(xEntity.xTransform.v2Pos, xEntity.xRenderComponent.fVirtualHeight, xEntity.xCollisionComponent.ibitCurrentColliderLayer);
        }

        /// <summary> Spawns an item at the target location. </summary>
        public static Item SpawnItem(this ItemDescription xDesc, Vector2 v2Pos, float fVirtualHeight, int iColliderLayer)
        {
            Vector2 v2ThrowDirection = Utility.RandomizeVector2Direction(CAS.RandomInLogic);

            return Utils.GetTheGame()._EntityMaster_AddItem(xDesc.enType, v2Pos, fVirtualHeight, iColliderLayer, v2ThrowDirection);
        }

        /// <summary> Creates an EquipmentInfo for the given modded item. </summary>
        /// <returns> Either a new or existing object of type T. If item has an existing object with an incompatible type, returns null. </returns>
        /// <remarks> This is a shorthand that should be used only for accessories, shoes and armor. </remarks>
        /// <exception cref="ArgumentException"></exception>
        public static EquipmentInfo CreateEquipmentInfo(this ItemDescription xDesc)
        {
            if(!xDesc.enType.IsModItem())
            {
                throw new ArgumentException("Provided enType is not a mod item.");
            }

            return CreateInfo<EquipmentInfo>(xDesc, "", null);
        }

        /// <summary> 
        /// Creates a new EquipmentInfo derived object that can be used by the game. 
        /// For shields, the paths to the textures are similar to the SoG paths, except "Shields/" substring is removed.
        /// </summary>
        /// <returns> Either a new or existing object of type T. If item has an existing object with an incompatible type, returns null. </returns>
        /// <exception cref="ArgumentException"></exception>
        public static T CreateInfo<T>(this ItemDescription xDesc, string sResource, ContentManager xContent) where T : EquipmentInfo
        {
            if (!xDesc.enType.IsModItem())
            {
                throw new ArgumentException("Provided enType is not a mod item.");
            }

            EquipmentInfo xInfo;

            if (ModLibrary.ItemDetails.ContainsKey(xDesc.enType))
            {
                xInfo = ModLibrary.ItemDetails[xDesc.enType].xEquipmentData;
                if (xInfo != null)
                {
                    if (xInfo.GetType() == typeof(T))
                    {
                        return xInfo as T;
                    }
                    else
                    {
                        return null;
                    }
                }
            }

            switch (typeof(T).Name)
            {
                case "EquipmentInfo":
                    xInfo = new EquipmentInfo(sResource, xDesc.enType);
                    break;

                case "FacegearInfo":
                    FacegearInfo xFacegear = (FacegearInfo)(xInfo = new FacegearInfo(xDesc.enType));
                    xFacegear.SetResource(sResource, xContent);
                    break;

                case "HatInfo":
                    HatInfo xHat = (HatInfo)(xInfo = new HatInfo(xDesc.enType));
                    xHat.SetResource(sResource, xContent);
                    break;

                case "WeaponInfo":
                    WeaponInfo xWeapon = (WeaponInfo)(xInfo = new WeaponInfo("", xDesc.enType, WeaponInfo.WeaponCategory.OneHanded));
                    xWeapon.SetResource(sResource, xContent);
                    xWeapon.SetWeaponType(WeaponInfo.WeaponCategory.OneHanded, false);
                    break;

                default:
                    throw new ArgumentException("Received a non-vanilla Info type! Preposterous!");
            }

            ModLibrary.ItemDetails[xDesc.enType].xEquipmentData = xInfo;

            return xInfo as T;
        }

        /// <summary> Returns the EquipmentInfo object associated with a modded item. </summary>
        /// <returns> An existing object of type T if the item has associated EquipmentInfo, or null otherwise. </returns>
        /// <exception cref="ArgumentException"></exception>
        public static T Info<T>(this ItemDescription xDesc) where T: EquipmentInfo
        {
            if(!xDesc.enType.IsModItem())
            {
                throw new ArgumentException("Provided enType is not a mod item.");
            }

            return ModLibrary.ItemDetails[xDesc.enType].xEquipmentData as T;
        }

        // EquipmentInfo extension

        /// <summary> Sets stats for a modded item. </summary>
        /// <remarks> Protip: specify only the stats you want to change with calls like xInfo.SetStats(HP: 1234, ASPD: 42, CritDMG: 45).</remarks>
        public static void SetStats(this EquipmentInfo xInfo, int HP = 0, int EP = 0, int ATK = 0, int MATK = 0, int DEF = 0, int ASPD = 0, int CSPD = 0, int Crit = 0, int CritDMG = 0, int ShldHP = 0, int EPRegen = 0, int ShldRegen = 0)
        {
            var xDict = xInfo.deniStatChanges;

            StatEnum[] stat = { 
                StatEnum.HP, StatEnum.EP, StatEnum.ATK, StatEnum.MATK, StatEnum.DEF, StatEnum.ASPD, StatEnum.CSPD,
                StatEnum.Crit, StatEnum.CritDMG, StatEnum.ShldHP, StatEnum.EPRegen, StatEnum.ShldRegen
            };
            int[] statValue = { HP, EP, ATK, MATK, DEF, ASPD, CSPD, Crit, CritDMG, ShldHP, EPRegen, ShldRegen };

            for (int i = 0; i < statValue.Length; i++)
            {
                if (statValue[i] == 0) xDict.Remove(stat[i]);
                else xDict.Add(stat[i], statValue[i]);
            }
        }

        public static void AddSpecialEffect(this EquipmentInfo xInfo, EquipmentInfo.SpecialEffect enEffect)
        {
            xInfo.lenSpecialEffects.Add(enEffect);
        }

        public static void RemoveSpecialEffect(this EquipmentInfo xInfo, EquipmentInfo.SpecialEffect enEffect)
        {
            xInfo.lenSpecialEffects.Remove(enEffect);
        }

        // FacegearInfo extensions

        public static void SetOverHair(this FacegearInfo xInfo, bool bUp, bool bRight, bool bDown, bool bLeft)
        {
            xInfo.abOverHair[0] = bUp;
            xInfo.abOverHair[1] = bRight;
            xInfo.abOverHair[2] = bDown;
            xInfo.abOverHair[3] = bLeft;
        }

        public static void SetOverHat(this FacegearInfo xInfo, bool bUp, bool bRight, bool bDown, bool bLeft)
        {
            xInfo.abOverHat[0] = bUp;
            xInfo.abOverHat[1] = bRight;
            xInfo.abOverHat[2] = bDown;
            xInfo.abOverHat[3] = bLeft;
        }

        public static void SetRenderOffsets(this FacegearInfo xInfo, Vector2 v2Up, Vector2 v2Right, Vector2 v2Down, Vector2 v2Left)
        {
            if (v2Up != null) xInfo.av2RenderOffsets[0] = new Vector2(v2Up.X, v2Up.Y);
            if (v2Right != null) xInfo.av2RenderOffsets[1] = new Vector2(v2Right.X, v2Right.Y);
            if (v2Down != null) xInfo.av2RenderOffsets[2] = new Vector2(v2Down.X, v2Down.Y);
            if (v2Left != null) xInfo.av2RenderOffsets[3] = new Vector2(v2Left.X, v2Left.Y);
        }

        public static void SetBehindCharacter(this FacegearInfo xInfo, bool bUp, bool bRight, bool bDown, bool bLeft)
        {
            xInfo.abBehindCharacter[0] = bUp;
            xInfo.abBehindCharacter[1] = bRight;
            xInfo.abBehindCharacter[2] = bDown;
            xInfo.abBehindCharacter[3] = bLeft;
        }

        /// <summary>
        /// Sets resources used by the FacegearInfo.
        /// <para>The path to the textures are of format "Sprites/Equipment/Facegear/[resource]/[Up, Right, Down or Left].xnb"</para>
        /// </summary>
        public static void SetResource(this FacegearInfo xInfo, string sResource, ContentManager xContent)
        {
            string sHatPath = "Sprites/Equipment/Facegear/" + sResource + "/";

            try
            {
                xInfo.atxTextures[0] = xContent.Load<Texture2D>(sHatPath + "Up");
                xInfo.atxTextures[1] = xContent.Load<Texture2D>(sHatPath + "Right");
                xInfo.atxTextures[2] = xContent.Load<Texture2D>(sHatPath + "Down");
                xInfo.atxTextures[3] = xContent.Load<Texture2D>(sHatPath + "Left");
            }
            catch (Exception e)
            {
                Console.WriteLine("Facegear texture load failed. Exception: " + e.Message);
                xInfo.atxTextures[0] = RenderMaster.txNullTex;
                xInfo.atxTextures[1] = RenderMaster.txNullTex;
                xInfo.atxTextures[2] = RenderMaster.txNullTex;
                xInfo.atxTextures[3] = RenderMaster.txNullTex;
            }
        }

        // HatInfo extensions

        public static void SetUnderHair(this HatInfo.VisualSet xSet, bool bUp, bool bRight, bool bDown, bool bLeft)
        {
            xSet.abUnderHair[0] = bUp;
            xSet.abUnderHair[1] = bRight;
            xSet.abUnderHair[2] = bDown;
            xSet.abUnderHair[3] = bLeft;
        }

        public static void SetBehindCharacter(this HatInfo.VisualSet xSet, bool bUp, bool bRight, bool bDown, bool bLeft)
        {
            xSet.abBehindCharacter[0] = bUp;
            xSet.abBehindCharacter[1] = bRight;
            xSet.abBehindCharacter[2] = bDown;
            xSet.abBehindCharacter[3] = bLeft;
        }

        public static void SetObstructions(this HatInfo.VisualSet xSet, bool bTop, bool bSides, bool bBottom)
        {
            xSet.bObstructsTop = bTop;
            xSet.bObstructsSides = bSides;
            xSet.bObstructsBottom = bBottom;
        }

        public static void SetRenderOffsets(this HatInfo.VisualSet xSet, Vector2 v2Up, Vector2 v2Right, Vector2 v2Down, Vector2 v2Left)
        {
            if (v2Up != null) xSet.av2RenderOffsets[0] = new Vector2(v2Up.X, v2Up.Y);
            if (v2Right != null) xSet.av2RenderOffsets[1] = new Vector2(v2Right.X, v2Right.Y);
            if (v2Down != null) xSet.av2RenderOffsets[2] = new Vector2(v2Down.X, v2Down.Y);
            if (v2Left != null) xSet.av2RenderOffsets[3] = new Vector2(v2Left.X, v2Left.Y);
        }

        /// <summary>
        /// Sets resources used by the HatInfo.
        /// <para>The path to the textures are of format "Sprites/Equipment/Hats/[resource]/[Up, Right, Down or Left].xnb"</para>
        /// </summary>
        public static void SetResource(this HatInfo xInfo, string sResource, ContentManager xContent)
        {
            string sHatPath = "Sprites/Equipment/Hats/" + sResource + "/";

            try
            {
                xInfo.xDefaultSet.atxTextures[0] = xContent.Load<Texture2D>(sHatPath + "Up");
                xInfo.xDefaultSet.atxTextures[1] = xContent.Load<Texture2D>(sHatPath + "Right");
                xInfo.xDefaultSet.atxTextures[2] = xContent.Load<Texture2D>(sHatPath + "Down");
                xInfo.xDefaultSet.atxTextures[3] = xContent.Load<Texture2D>(sHatPath + "Left");

                xInfo.sResourceName = sResource;
            }
            catch (Exception e)
            {
                Console.WriteLine("Facegear texture load failed. Exception: " + e.Message);
                xInfo.xDefaultSet.atxTextures[0] = RenderMaster.txNullTex;
                xInfo.xDefaultSet.atxTextures[1] = RenderMaster.txNullTex;
                xInfo.xDefaultSet.atxTextures[2] = RenderMaster.txNullTex;
                xInfo.xDefaultSet.atxTextures[3] = RenderMaster.txNullTex;

                xInfo.sResourceName = "";
            }
        }

        /// <summary>
        /// Creates an alternative style for the HatInfo's item, which is activated while wearing the enCombo item.
        /// <para>The paths to the texture are of format "Sprites/Equipment/Hats/[resource]/[Up, Right, Down or Left].xnb"</para>
        /// </summary>
        /// <remarks>You can set resource to something like "[hatResource]/[comboResource]/" to keep the textures for default and alternate set in the same hat folder.</remarks>
        public static void AddAltSet(this HatInfo xInfo, ItemCodex.ItemTypes enCombo, string sResource, ContentManager xContent)
        {
            if (!xInfo.denxAlternateVisualSets.ContainsKey(enCombo))
            {
                xInfo.denxAlternateVisualSets[enCombo] = new HatInfo.VisualSet();
            }

            string sHatPath = "Sprites/Equipment/Hats/" + sResource + "/";
            HatInfo.VisualSet xAlt = xInfo.denxAlternateVisualSets[enCombo];

            try
            {
                xAlt.atxTextures[0] = xContent.Load<Texture2D>(sHatPath + "Up");
                xAlt.atxTextures[1] = xContent.Load<Texture2D>(sHatPath + "Right");
                xAlt.atxTextures[2] = xContent.Load<Texture2D>(sHatPath + "Down");
                xAlt.atxTextures[3] = xContent.Load<Texture2D>(sHatPath + "Left");

                xInfo.sResourceName = sResource;
            }
            catch (Exception e)
            {
                Console.WriteLine("Failed to load a texture in AddAltSet.");
                Console.WriteLine("Exception: " + e.Message);
                xAlt.atxTextures[0] = RenderMaster.txNullTex;
                xAlt.atxTextures[1] = RenderMaster.txNullTex;
                xAlt.atxTextures[2] = RenderMaster.txNullTex;
                xAlt.atxTextures[3] = RenderMaster.txNullTex;

                xInfo.sResourceName = "";
            }
        }

        // WeaponInfo extensions

        /// <summary>
        /// Sets resources used by the WeaponInfo.
        /// <para>
        /// The texture paths are similar to SoG.WeaponAssetLoader's OneHandedDictionaryFill and TwoHandedDictionaryFill paths, 
        /// except with "Weapons/" substring removed from the path. 
        /// </para>
        /// </summary>
        /// <remarks> The resulting resources aren't mamanged by xContent, but by a ContentManager from SoG. </remarks>
        public static void SetResource(this WeaponInfo xInfo, string sResource, ContentManager xContent)
        {
            // Weapon resources aren't loaded in the WeaponInfo. Storing the information is required.
            xInfo.sResourceName = sResource;
            ModLibrary.ItemDetails[xInfo.enItemType].xContentManager = xContent;
        }

        /// <summary>
        /// Sets the Weapon type for a WeaponInfo, as well as the default damage modifiers.
        /// </summary>
        public static void SetWeaponType(this WeaponInfo xInfo, WeaponInfo.WeaponCategory enHands, bool bMagic)
        {
            xInfo.enWeaponCategory = enHands;
            xInfo.enAutoAttackSpell = WeaponInfo.AutoAttackSpell.None;
            xInfo.sWeaponCategory = "Bow";

            if (enHands == WeaponInfo.WeaponCategory.OneHanded)
            {
                if(bMagic)
                {
                    xInfo.enAutoAttackSpell = WeaponInfo.AutoAttackSpell.Generic1H;
                }
                xInfo.iDamageMultiplier = 90;
                xInfo.sWeaponCategory = "OneHanded";
            }
            else if (enHands == WeaponInfo.WeaponCategory.TwoHanded)
            {
                if (bMagic)
                {
                    xInfo.enAutoAttackSpell = WeaponInfo.AutoAttackSpell.Generic2H;
                }
                xInfo.iDamageMultiplier = 125;
                xInfo.sWeaponCategory = "TwoHanded";
            }
        }

        // Utility

        /// <summary> 
        /// Returns a fresh SpecialEffect value.
        /// The value returned is unique, so you can use it in custom items without worrying about conflicts, as long as you store it in a variable somewhere.
        /// </summary>
        /// <remarks> You'll have to implement the effect yourself via patches or callbacks. </remarks>
        /// <returns> A new EquipmentInfo.SpecialEffect value. </returns>
        private static EquipmentInfo.SpecialEffect AddNewSpecialEffect()
        {
            EquipmentInfo.SpecialEffect effect = ModLibrary.SpecialEffectsNext;
            ModLibrary.SpecialEffectsCount++;
            return effect;
        }

        // 
        // Harmony Library Patches - For Get- methods
        //

        /// <summary> Patches GetItemInstance so that SoG can create item instances with a correct shadow texture. </summary>
        private static bool GetItemInstance_PrefixPatch(ref Item __result, ItemCodex.ItemTypes enType)
        {
            if (!enType.IsModItem())
            {
                return true; // Executes original method
            }

            ItemDescription xDesc = ItemCodex.denxLoadedDescriptions[enType];

            __result = new Item()
            {
                enType = enType,
                sFullName = xDesc.sFullName,
                bGiveToServer = xDesc.lenCategory.Contains(ItemCodex.ItemCategories.GrantToServer)
            };

            __result.xRenderComponent.txTexture = xDesc.txDisplayImage;
            __result.xCollisionComponent.xMovementCollider = new SphereCollider(10f, Vector2.Zero, __result.xTransform, 1f, __result) { bCollideWithFlat = true };
            __result.xRenderComponent.txShadowTexture = ModLibrary.ItemDetails[enType].txItemShadow ?? Utils.GetTheGame().xLevelMaster.contRegionContent.Load<Texture2D>("Items/DropAppearance/hartass02");

            return false; // Skips original method
        }

        /// <summary> Patches GetEquipmentInfo so that SoG can use modded EquipmentInfos. </summary>
        private static bool GetEquipmentInfo_PrefixPatch(ref EquipmentInfo __result, ItemCodex.ItemTypes enType)
        {
            if (enType.IsModItem())
            {
                __result = ModLibrary.ItemDetails[enType].xEquipmentData;
                return false; // Skip original method
            }
            return true; // Execute original method
        }

        /// <summary> Patches GetFacegearInfo so that SoG can use modded FacegearInfos. </summary>
        private static bool GetFacegearInfo_PrefixPatch(ref FacegearInfo __result, ItemCodex.ItemTypes enType)
        {
            if (enType.IsModItem())
            {
                __result = ModLibrary.ItemDetails[enType].xEquipmentData as FacegearInfo;
                return false; // Skip original method
            }
            return true; // Execute original method
        }

        /// <summary> Patches GetHatInfo so that SoG can use modded HatInfos. </summary>
        private static bool GetHatInfo_PrefixPatch(ref HatInfo __result, ItemCodex.ItemTypes enType)
        {
            if (enType.IsModItem())
            {
                __result = ModLibrary.ItemDetails[enType].xEquipmentData as HatInfo;
                return false; // Skip original method
            }
            return true; // Execute original method
        }

        /// <summary> Patches GetWeaponInfo so that SoG can use modded WeaponInfos. </summary>
        private static bool GetWeaponInfo_PrefixPatch(ref WeaponInfo __result, ItemCodex.ItemTypes enType)
        {
            if (enType.IsModItem())
            {
                __result = ModLibrary.ItemDetails[enType].xEquipmentData as WeaponInfo;
                return false; // Skip original method
            }
            return true; // Execute original method
        }

        /// <summary> 
        /// Patches LoadBatch so that SoG can load weapon textures from paths other than "Content/". 
        /// <para> Also shortens the texture paths by removing the "Weapons/" substring. </para>
        /// </summary>
        private static bool LoadBatch_PrefixOverwrite(ref Dictionary<ushort, string> dis, WeaponAssets.WeaponContentManager __instance)
        {
            bool bShortenedPath = false;
            if (__instance.enType.IsModItem())
            {
                bShortenedPath = true;
                try
                {
                    __instance.contWeaponContent.RootDirectory = ModLibrary.ItemDetails[__instance.enType].xContentManager.RootDirectory;
                }
                catch
                {
                    Console.WriteLine("LoadBatch: couldn't set RootDirectory for item " + __instance.enType);
                }
            }
            

            foreach (KeyValuePair<ushort, string> kvp in dis)
            {
                string sPath = kvp.Value;
                if(bShortenedPath)
                {
                    sPath = sPath.Replace("Weapons/", "");
                }

                try
                {
                    __instance.ditxWeaponTextures.Add(kvp.Key, __instance.contWeaponContent.Load<Texture2D>(sPath));
                }
                catch
                {
                    Utils.GetTheGame().Log("Failed to load weapon texture at: " + __instance.contWeaponContent.RootDirectory + "/" + sPath);
                    Console.WriteLine("LoadBatch: failed to load weapon texture for item " + __instance.enType + " at " + __instance.contWeaponContent.RootDirectory + "/" + sPath);
                    __instance.ditxWeaponTextures[kvp.Key] = RenderMaster.txNullTex;
                }
            }

            return false; // Never executes the original
        }

        /// <summary> 
        /// Patches _Animations_GetAnimationSet so that SoG can load shield textures from paths other than "Content/". 
        /// <para> Also shortens the texture paths by removing the "Shields/" substring. </para>
        /// </summary>
        private static bool _Animations_GetAnimationSet_PrefixOverwrite(PlayerView xPlayerView, string sAnimation, string sDirection, bool bWithWeapon, bool bCustomHat, bool bWithShield, bool bWeaponOnTop, ref PlayerAnimationTextureSet __result)
        {
            __result = new PlayerAnimationTextureSet()
            {
                bWeaponOnTop = bWeaponOnTop
            };
            string sAttackPath = "";

            ContentManager VanillaContent = RenderMaster.contPlayerStuff;

            __result.txBase = VanillaContent.Load<Texture2D>("Sprites/Heroes/" + sAttackPath + sAnimation + "/" + sDirection);

            // Skipped code which isn't used in vanilla

            try
            {
                if (bWithShield && xPlayerView.xEquipment.DisplayShield != null && xPlayerView.xEquipment.DisplayShield.sResourceName != "")
                {
                    ItemCodex.ItemTypes enType = xPlayerView.xEquipment.DisplayShield.enItemType;

                    if (enType.IsModItem())
                    {
                        // Shortened path for mod loaders
                        __result.txShield = ModLibrary.ItemDetails[enType].xContentManager.Load<Texture2D>("Sprites/Heroes/" + sAttackPath + sAnimation + "/" + xPlayerView.xEquipment.DisplayShield.sResourceName + "/" + sDirection);
                    }
                    else
                    {
                        __result.txShield = VanillaContent.Load<Texture2D>("Sprites/Heroes/" + sAttackPath + sAnimation + "/Shields/" + xPlayerView.xEquipment.DisplayShield.sResourceName + "/" + sDirection);
                    }
                }
            }
            catch(Exception e)
            {
                Console.WriteLine("Shield texture load failed! Exception: " + e.Message);
                __result.txShield = RenderMaster.txNullTex;
            }
            
            if (bWithWeapon)
            {
                __result.txWeapon = RenderMaster.txNullTex;
            }

            return false; // Never executes the original
        }
    }

}