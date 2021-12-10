using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using SoG.Modding.Utils;
using SpellDescriptions;
using Watchers;

namespace SoG.Modding.Addons
{
    [HarmonyPatch]
    [HarmonyPriority(Priority.LowerThanNormal)]
    internal class Patch_GetSkillLevelReplacer
    {
        [HarmonyTargetMethods]
        public static IEnumerable<MethodBase> TargetMethods()
        {
            List<MethodBase> perceivedLevelMethods = new List<MethodBase>()
            {
                AccessTools.Method(typeof(Game1), "_Potion_ActivatePotion_OLD"),
                AccessTools.Method(typeof(Game1), "_Item_PickUp", new[] { typeof(ItemCodex.ItemTypes), typeof(PlayerView), typeof(ushort), typeof(bool) }),
                AccessTools.Method(typeof(Game1), "_Enemy_DropLoot"),
                AccessTools.Method(typeof(Game1), "_Enemy_HandleDeath"),
                AccessTools.Method(typeof(Game1), "_Enemy_DecideArbitraryCrit"),
                AccessTools.Method(typeof(Game1), "_CollisionMaster_ResolveAttackCollision"),
                AccessTools.Method(typeof(Game1), "_Input_AttackButtonPressed"),
                AccessTools.Method(typeof(Game1), "__Input_ActivateSpellslot"),
                AccessTools.Method(typeof(Game1), "_Loading_LoadCharacterFromFile"),
                AccessTools.Method(typeof(Game1), "_Network_ParseServerMessage"),
                AccessTools.Method(typeof(Game1), "_Player_SpellOrSkillCast"),
                AccessTools.Method(typeof(Game1), "_Player_ActivatePotion"),
                AccessTools.Method(typeof(Game1), "_Player_EmpowerAttackStatsWithBasicAttackEffects"),
                AccessTools.Method(typeof(Game1), "_Player_GetCurrentMaxHPPenalty"),
                AccessTools.Method(typeof(Game1), "_Player_SetCurrentMaxHPPenalty"),
                AccessTools.Constructor(typeof(FrostyFriendDescription)),
                AccessTools.Method(typeof(_Skills_SpinActivation), "SuccessfulActivation"),
                AccessTools.Method(typeof(SpellCharge), "Update"),
                AccessTools.Method(typeof(SpellCharge), "GetCostMod"),
                AccessTools.Method(typeof(SpellCharge), "Enter"),
                AccessTools.Method(typeof(SpellCharge), "Release"),
                AccessTools.Method(typeof(_Skills_ShadowCloneActivation), "SuccessfulActivation"),
                AccessTools.Method(typeof(_Skills_OverheadActivation), "SuccessfulActivation"),
                AccessTools.Method(typeof(DeathMarkSpellCharge), "ExecuteSpellCallback"),
                AccessTools.Method(typeof(_Spells_ProtectionActivation), "SuccessfulActivation"),
                AccessTools.Method(typeof(SpinSpellCharge), "ExecuteSpellCallback"),
                AccessTools.Method(typeof(IceNovaSpellCharge), "ExecuteSpellCallback"),
                AccessTools.Method(typeof(_Spells_BerserkActivation), "SuccessfulActivation"),
                AccessTools.Method(typeof(TauntSpellCharge), "ExecuteSpellCallback"),
                AccessTools.Method(typeof(_Spells_HasteActivation), "SuccessfulActivation"),
                AccessTools.Method(typeof(SpawnSpellNextFrame_FromArrow), "Update"),
                AccessTools.Method(typeof(CreateSpellNextFrameWatcher), "Update"),
                AccessTools.Method(typeof(_Spells_EarthSpikeInstance), "OnAttackHit"),
                AccessTools.Method(typeof(SmashSpellCharge), "ExecuteSpellCallback"),
                AccessTools.Method(typeof(ShortswingSpellCharge), "CommonStabFlab"),
                AccessTools.Method(typeof(_Spells_CloudInstance), "Update"),
                AccessTools.Method(typeof(_Spells_BuffATKActivation), "SuccessfulActivation"),
                AccessTools.Method(typeof(_Spells_IceSpikeActivation), "SuccessfulActivation"),
                AccessTools.Method(typeof(PlayerRenderComponent), "ParseInstruction"),
                AccessTools.Method(typeof(OverheadSpellCharge), "ExecuteSpellCallback"),
                AccessTools.Method(typeof(_Skills_SmashActivation), "SuccessfulActivation"),
                AccessTools.Method(typeof(BuffDEFSpellCharge), "Release"),
                AccessTools.Method(typeof(BuffDEFSpellCharge), "CastBuff"),
                AccessTools.Method(typeof(_Spells_EarthSpikeActivation), "SuccessfulActivation"),
                AccessTools.Method(typeof(_Spells_MeteorActivation), "SuccessfulActivation"),
                AccessTools.Method(typeof(_Spells_ShortswingInstance), "OnAttackHit"),
                AccessTools.Method(typeof(_Spells_Wand_OneHandBasicInstance), "OnAttackHit"),
                AccessTools.Method(typeof(_Spells_DeathMarkActivation), "SuccessfulActivation"),
                AccessTools.Method(typeof(StasisSpellCharge), "ExecuteSpellCallback"),
                AccessTools.Method(typeof(_Spells_ArrowActivation), "AnimationActivationCallback"),
                AccessTools.Method(typeof(StingerSpellCharge), "ExecuteSpellCallback"),
                AccessTools.Method(typeof(MillionStabsSpellCharge), "ExecuteSpellCallback"),
                AccessTools.Method(typeof(_Spells_InsectSwarmActivation), "SuccessfulActivation"),
                AccessTools.Method(typeof(_Spells_CloudActivation), "SuccessfulActivation"),
                AccessTools.Method(typeof(BarrierSpellCharge), "Release"),
                AccessTools.Method(typeof(BarrierSpellCharge), "CastBuff"),
                AccessTools.Method(typeof(BlinkSpellCharge), "OnAnimationCallback"),
                AccessTools.Method(typeof(_Spells_BlinkActivation), "ComparePrerequisites"),
                AccessTools.Method(typeof(_Spells_BlinkActivation), "SuccessfulActivation"),
                AccessTools.Method(typeof(_Spells_FireballActivation), "SuccessfulActivation"),
                AccessTools.Method(typeof(_Skills_SpiritSlashActivation), "SuccessfulActivation"),
                AccessTools.Method(typeof(_Spells_FrostyFriendActivation), "SuccessfulActivation"),
                AccessTools.Method(typeof(AttackStats), "ApplyFireBonusDamage"),
                AccessTools.Method(typeof(AttackStats), "ApplyEarthBonusDamage"),
                AccessTools.Method(typeof(AttackStats), "ApplyWindBonusDamage"),
                AccessTools.Method(typeof(AttackStats), "ApplyIceBonusDamage"),
                AccessTools.Method(typeof(AttackStats), "ApplyPhysicalBonusDamage"),
                AccessTools.Method(typeof(RogueLikeMode.Chaos_UpgradePlate), "Activate"),
                AccessTools.Method(typeof(_Skills_SpiritSlashInstance), "DoSlash"),
                AccessTools.Method(typeof(_Spells_WindSliceActivation), "SuccessfulActivation"),
                AccessTools.Method(typeof(LoodChest), "Update"),
                AccessTools.Method(typeof(_Skills_StingerActivation), "SuccessfulActivation"),
                AccessTools.Method(typeof(_Spells_HealActivation), "SuccessfulActivation"),
                AccessTools.Method(typeof(FireballSpellCharge), "ExecuteSpellCallback"),
                AccessTools.Method(typeof(_Spells_FocusActivation), "SuccessfulActivation"),
                AccessTools.Method(typeof(IceSpikesSpellCharge), "ExecuteSpellCallback"),
                AccessTools.Method(typeof(_Spells_ChainLightningActivation), "SuccessfulActivation"),
                AccessTools.Method(typeof(BuffSPDSpellCharge), "Release"),
                AccessTools.Method(typeof(BuffSPDSpellCharge), "CastBuff"),
                AccessTools.Method(typeof(BuffATKSpellCharge), "Release"),
                AccessTools.Method(typeof(BuffATKSpellCharge), "CastBuff"),
                AccessTools.Method(typeof(_Spells_IceNovaInstance), "CreateArbitraryFrostNova"),
                AccessTools.Method(typeof(_Spells_IceNovaInstance), "Update"),
                AccessTools.Method(typeof(_Spells_IceNovaActivation), "SuccessfulActivation"),
                AccessTools.Method(typeof(_Spells_TauntActivation), "SuccessfulActivation"),
                AccessTools.Method(typeof(_Spells_BuffSPDActivation), "SuccessfulActivation"),
                AccessTools.Method(typeof(_Spells_ThrowActivation), "SuccessfulActivation"),
                AccessTools.Method(typeof(_Spells_FrostyFriendInstance), "Init"),
                AccessTools.Method(typeof(_Spells_FrostyFriendInstance), "Update"),
                AccessTools.Method(typeof(_Spells_StaticTouchActivation), "SuccessfulActivation"),
                AccessTools.Method(typeof(_Spells_BuffDEFActivation), "SuccessfulActivation"),
                AccessTools.Method(typeof(_Spells_PlantActivation), "SuccessfulActivation"),
                AccessTools.Method(typeof(ThrowSpellCharge), "ExecuteSpellCallback"),
                AccessTools.Method(typeof(_Skills_ShortswingActivation), "SuccessfulActivation"),
                AccessTools.Method(typeof(_Spells_Wand_TwoHandBasicInstance), "OnAttackHit"),
                AccessTools.Method(typeof(_Spells_BarrierActivation), "SuccessfulActivation"),
                AccessTools.Method(typeof(_Skills_StabActivation), "SuccessfulActivation"),
                AccessTools.Method(typeof(_Spells_FlamethrowerActivation), "SuccessfulActivation"),
                AccessTools.Method(typeof(_Spells_StasisActivation), "SuccessfulActivation"),
                AccessTools.Method(typeof(PlayerEntity), "Update"),
                AccessTools.Method(typeof(PlayerEntity), "FireWandProjectile"),
                AccessTools.Method(typeof(PlayerEntity), "OnAttackHit"),
                AccessTools.Method(typeof(PlayerEntity), "OnHitByAttack"),
                AccessTools.Property(typeof(PlayerEntity), "BowStretchTime").GetGetMethod(),
                AccessTools.Property(typeof(PlayerEntity), "MovementModInCharge").GetGetMethod(),
                AccessTools.Property(typeof(PlayerEntity), "MovementModInShield").GetGetMethod(),
                AccessTools.Method(typeof(_Skills_ShadowCloneInstance), "Update"),
                AccessTools.Method(typeof(_Spells_StaticTouchDamage), "Detonate")
            };

            List<MethodBase> trueLevelMethods = new List<MethodBase>()
            {
                AccessTools.Method(typeof(PlayerViewStats), "CanLevelSkill"),
                AccessTools.Method(typeof(PlayerViewStats), "DeductSkillLevelCost"),
                AccessTools.Method(typeof(Game1), "_InGameMenu_RenderSkills"),
                AccessTools.Method(typeof(Game1), "_InGameMenu_RenderSkills"),
                AccessTools.Method(typeof(Game1), "_InGameMenu_RenderSkills_RenderTalent"),
                AccessTools.Method(typeof(Game1), "_InGameMenu_RenderSkills_RenderSkillFrame"),
                AccessTools.Method(typeof(Game1), "_InGameMenu_RenderSpellDetails"),
                AccessTools.Method(typeof(Game1), "_InGameMenu_RenderPopupUpgrade"),
                AccessTools.Method(typeof(Game1), "_InGameMenu_RenderPopupRefund"),
                AccessTools.Method(typeof(Game1), "_Skill_LevelUp", new[] { typeof(PlayerView), typeof(SpellCodex.SpellTypes), typeof(bool), typeof(int) }),
                AccessTools.Method(typeof(Game1), "_InGameMenu_Input"),
                AccessTools.Method(typeof(Game1), "_InGameMenu_Skills_SetBasePopup"),
                AccessTools.Method(typeof(SpellDescription), "IsMaxed"),
                AccessTools.Method(typeof(Replay), "PlaybackSpellLeveled"),
                AccessTools.Method(typeof(ShopMenu), "QuickLevelInterface"),
            };

            return trueLevelMethods;
        }

        /// <summary>
        /// This transpiler replaces GetSkillLevel with GetTrueSkillLevel.
        /// Since we patch GetSkillLevel to return the modified skill level,
        /// we need to replace it with GetTrueSkillLevel in methods that should use the true level instead.
        /// </summary>
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> UseTrueSkillLevelTranspiler(IEnumerable<CodeInstruction> code)
        {
            var codeList = code.ToList();

            var from = AccessTools.Method(typeof(PlayerViewStats), nameof(PlayerViewStats.GetSkillLevel));
            var to = AccessTools.Method(typeof(Patch_GetSkillLevelReplacer), nameof(Patch_GetSkillLevelReplacer.GetTrueSkillLevel));

            return codeList.MethodReplacer(from, to);
        }

        private static byte GetTrueSkillLevel(PlayerViewStats stats, SpellCodex.SpellTypes spell)
        {
            return ModGoodies.TheMod.GetTrueSkillLevel(stats, spell);
        }
    }
}
