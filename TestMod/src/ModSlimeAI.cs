using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using SoG.Modding;
using SoG.Modding.Extensions;

namespace SoG.Modding.TestMod
{
    // This is pretty much a Ctrl+C / V of SlimeAI
    // TO DO: Make the ModSlime act in a different way

    public class ModSlimeAI : Behaviours.EnemyBehaviour
	{
		// For ease of use, we use some global variables that are set by FeatureExample.cs
		// I suggest you find a better alternative in your own code, though

		internal static Dictionary<int, Animation> ClassAnimationPrototypes = new Dictionary<int, Animation>();

		internal static TestMod Mod = null;

		private int iCounter;

		private int iNextHardUpdate;

		private Item xItemLockedOnto;

		public float fFurthestAllowedDistance = 130f;

		private float fAttackDistance = 30f;

		private bool bInited;

		private bool bTotallyMad;

		private Vector2 v2RandMoveDir = Vector2.Zero;

		public ModSlimeAI(Enemy xEnemy)
		{
			xOwner = xEnemy;
			if (Globals.Game.xStateMaster.enGameMode == StateMaster.GameModes.RogueLike)
			{
				fFurthestAllowedDistance = 600f;
			}
		}

		public override void MakeAgressive()
		{
			fFurthestAllowedDistance = 1000f;
		}

		public void Move(Vector2 v2MoveDir)
		{
			if (xOwner.xRenderComponent.GetCurrentAnimation().iID == 1)
			{
				int iFrame = xOwner.xRenderComponent.GetCurrentAnimation().iRenderedFrame;
				if (Utility.IsWithinRange(iFrame, 2, 6))
				{
					v2MoveDir = Utility.Normalize(v2MoveDir);
					xOwner.xTransform.v2Pos += v2MoveDir * xOwner.xBaseStats.fMovementSpeed;
				}
			}
		}

		public void TryMove(Vector2 v2MoveDir)
		{
			iNextHardUpdate = 56;
			v2RandMoveDir = v2MoveDir;
		}

		public override void ToBeCaught()
		{
			IEffect mos = Globals.Game._EffectMaster_AddEffect(new SortedAnimated(Vector2.Zero, SortedAnimated.SortedAnimatedEffects._GUIEffects_Emote_Exclamation));
			mos.xRenderComponent.xTransform = xOwner.xTransform;
			mos.xRenderComponent.v2OffsetRenderPos.Y = -20f;
		}

		public override void EndCatch()
		{
		}

		public override void OnAttackHit(AttackCollisionData xAtColData, BaseStats xBaseStats)
		{
			if (!xAtColData.bShielded && Globals.Game.bEffectsOn)
			{
				IEffect kaktus = Globals.Game._EffectMaster_AddEffect(new SortedAnimated(xAtColData.v2PointOfImpact, SortedAnimated.SortedAnimatedEffects._HitEffect_Pang1));
				kaktus.xRenderComponent.fVirtualHeight += xOwner.xRenderComponent.fVirtualHeight;
				kaktus.xRenderComponent.xTransform = xBaseStats.xOwner.xTransform;
				kaktus.xRenderComponent.v2OffsetRenderPos = new Vector2(0f, -10f);
			}
		}

		public void AddHitEffect()
		{
			if (!Globals.Game.bEffectsOn)
			{
				return;
			}
			Random knark = Globals.Game.randomInVisual;
			Vector2 v2Orig = xOwner.xTransform.v2Pos + new Vector2(0f, -3f);
			for (int i = 0; i < 6; i++)
			{
				Vector2 v2RandDir = new Vector2(-1f + (float)knark.NextDouble() * 2f, -1f + (float)knark.NextDouble() * 2f);
				if (v2RandDir == Vector2.Zero)
				{
					v2RandDir = new Vector2(1f, 0f);
				}
				v2RandDir = v2RandDir.SafeNormalized();
				SortedAnimated.SortedAnimatedEffects enEff = SortedAnimated.SortedAnimatedEffects._HitEffect_SlimeParticle;
				if (xOwner.enType == EnemyCodex.EnemyTypes.RedSlime)
				{
					enEff = SortedAnimated.SortedAnimatedEffects._HitEffect_RedSlimeParticle;
				}
				float fRandGrade = (float)knark.NextDouble();
				SortedAnimated eff = Globals.Game._EffectMaster_AddEffect(new _Effect_MovingAnimated(v2Orig, enEff, v2RandDir * (0.5f + (float)Globals.Game.randomInVisual.NextDouble() * 2f), 35 + (int)(25f * fRandGrade), 25, 0.9f)) as SortedAnimated;
				eff.xRenderComponent.fVirtualHeight += xOwner.xRenderComponent.fVirtualHeight;
				(eff.xRenderComponent as AnimatedRenderComponent).fAnimationTimeWarp = 0.7f + fRandGrade * 0.6f;
			}
		}

		public override void InstructionsFromServer(byte byInstructionID, params float[] afParams)
		{
			switch (byInstructionID)
			{
				case 0:
					AddHitEffect();
					break;
				case 1:
					{
						ushort iID = (ushort)afParams[0];
						int iDuration = 420;
						if (bIsElite)
						{
							iDuration = 450;
						}
						if (xOwner.enType == EnemyCodex.EnemyTypes.RedSlime)
						{
							SlowSlime mos2 = Globals.Game._EntityMaster_AddDynamicEnvironment(iID, DynamicEnvironmentCodex.ObjectTypes.CloudEffect_BossPapaSlime_SlowSlimeSmall_Red, xOwner.xTransform.v2Pos, 0f, 0) as SlowSlime;
							mos2.iDeath = iDuration;
						}
						else if (xOwner.enType == EnemyCodex.EnemyTypes.GreenSlime)
						{
							SlowSlime mos = Globals.Game._EntityMaster_AddDynamicEnvironment(iID, DynamicEnvironmentCodex.ObjectTypes.CloudEffect_BossPapaSlime_SlowSlimeSmall, xOwner.xTransform.v2Pos, 0f, 0) as SlowSlime;
							mos.iDeath = iDuration;
						}
						break;
					}
			}
		}

		public override void OnHitByAttack(AttackCollisionData xAtColData, AttackPhase xAtPhase)
		{
			base.OnHitByAttack(xAtColData, xAtPhase);
			fFurthestAllowedDistance = 500f;
			AddHitEffect();
			if (Globals.Game.xNetworkInfo.enCurrentRole == NetworkHelperInterface.NetworkRole.Server)
			{
				SendClientInstruction(0);
			}
		}

		public override void OnAnimationCallback(AnimationInstruction xIns)
		{
			if (xIns.xEvent.afAlterableValues[0] != 0f || CAS.NetworkRole == NetworkHelperInterface.NetworkRole.Client)
			{
				return;
			}
			int iDuration = 420;
			if (bIsElite)
			{
				iDuration = 450;
			}
			if (xOwner.enType == EnemyCodex.EnemyTypes.RedSlime)
			{
				SlowSlime mos2 = Globals.Game._EntityMaster_AddDynamicEnvironment(DynamicEnvironmentCodex.ObjectTypes.CloudEffect_BossPapaSlime_SlowSlimeSmall_Red, xOwner.xTransform.v2Pos) as SlowSlime;
				if (bIsElite)
				{
					mos2.fMoveSpeedDebuff = Math.Max(0.15f - (float)(CAS.DifficultySetting + 1) * 0.02f, 0.07f);
				}
				mos2.iDeath = iDuration;
				SendClientInstruction(1, (int)mos2.iID);
			}
			else if (xOwner.enType == EnemyCodex.EnemyTypes.GreenSlime)
			{
				SlowSlime mos = Globals.Game._EntityMaster_AddDynamicEnvironment(DynamicEnvironmentCodex.ObjectTypes.CloudEffect_BossPapaSlime_SlowSlimeSmall, xOwner.xTransform.v2Pos) as SlowSlime;
				if (CAS.CurrentZone == Level.ZoneEnum.GhostShip_FXTopLeftHouse)
				{
					mos.iDeath = 120;
				}
				if (bIsElite)
				{
					mos.fMoveSpeedDebuff = Math.Max(0.32f - (float)(CAS.DifficultySetting + 1) * 0.04f, 0.13f);
				}
				mos.iDeath = iDuration;
				SendClientInstruction(1, (int)mos.iID);
			}
		}

		public override void OnTaunted(WorldActor xTauntSource)
		{
			base.OnTaunted(xTauntSource);
			bTotallyMad = true;
			xPlayerLockedOnto = xTauntSource;
			if (fFurthestAllowedDistance < 200f)
			{
				fFurthestAllowedDistance = 200f;
			}
		}

		public override void OnUpdate()
		{
			if (!bInited)
			{
				bInited = true;
			}
			if (v2RandMoveDir != Vector2.Zero && iNextHardUpdate > 0 && xOwner.xRenderComponent.GetCurrentAnimation().bIsMoveCancellable)
			{
				Move(v2RandMoveDir);
			}
			iCounter++;
			iNextHardUpdate--;
			if (xCatchMode != null)
			{
				if (xCatchMode.enPhase == CatchMode.Phase.WaitingForBaitedEnemy)
				{
					float fDistance2 = 50000f;
					WorldActor xClosestPlayer2 = xCatchMode.xView.xEntity;
					Vector2 v2TargetPosition = ((!(xClosestPlayer2.xTransform.v2Pos.X >= xOwner.xTransform.v2Pos.X)) ? (xClosestPlayer2.xTransform.v2Pos + new Vector2(30f, 0f)) : (xClosestPlayer2.xTransform.v2Pos - new Vector2(30f, 0f)));
					fDistance2 = Vector2.Distance(v2TargetPosition, xOwner.xTransform.v2Pos);
					if (xClosestPlayer2 != null && iNextHardUpdate <= 0)
					{
						if (fDistance2 < 5f)
						{
							xCatchMode.EnterMiniGame();
							xOwner.xRenderComponent.SwitchAnimation(0, Animation.CancelOptions.UseAnimationDefault);
							v2RandMoveDir = Vector2.Zero;
						}
						else
						{
							iNextHardUpdate = 40;
							v2RandMoveDir = v2TargetPosition - xOwner.xTransform.v2Pos;
							v2RandMoveDir = v2RandMoveDir.SafeNormalized();
							if (fDistance2 < 18f)
							{
								v2RandMoveDir *= fDistance2 / 18f;
							}
							xOwner.xRenderComponent.SwitchAnimation(1, Animation.CancelOptions.IgnoreIfPlaying);
						}
					}
				}
				xOwner.xTransform.v2ServerPos = xOwner.xTransform.v2Pos;
				return;
			}
			if ((!(v2RandMoveDir != Vector2.Zero) || iNextHardUpdate <= 0 || !xOwner.xRenderComponent.GetCurrentAnimation().bIsMoveCancellable) && xOwner.xRenderComponent.GetCurrentAnimation().bIsMoveCancellable)
			{
				if (xPlayerLockedOnto != null && bTotallyMad)
				{
					float fDistance3 = Vector2.Distance(xPlayerLockedOnto.GetBelievedPosition(), xOwner.xTransform.v2Pos);
					Vector2 v2Dir = xPlayerLockedOnto.GetBelievedPosition() - xOwner.xTransform.v2Pos;
					v2Dir = v2Dir.SafeNormalized();
					xOwner.xRenderComponent.SwitchAnimation(1, Animation.CancelOptions.UseAnimationDefault);
					if (fDistance3 > fAttackDistance * 2.2f)
					{
						TryMove(v2Dir);
						xOwner.xRenderComponent.SwitchAnimation(1, Animation.CancelOptions.UseAnimationDefault);
					}
					else if (fDistance3 > fAttackDistance * 1.25f)
					{
						Vector2 v2LineDir = Utility.FindClosestStraightLine(xOwner.xTransform.v2Pos, xPlayerLockedOnto.GetBelievedPosition());
						if (v2LineDir != Vector2.Zero)
						{
							v2LineDir = Utility.Normalize(v2LineDir);
						}
						v2Dir += v2LineDir;
						v2Dir = v2Dir.SafeNormalized();
						TryMove(v2Dir);
						xOwner.xRenderComponent.SwitchAnimation(1, Animation.CancelOptions.UseAnimationDefault);
					}
					else
					{
						v2Dir = Utility.FindClosestStraightLine(xOwner.xTransform.v2Pos, xPlayerLockedOnto.GetBelievedPosition());
						v2Dir = v2Dir.SafeNormalized();
						if (v2Dir != Vector2.Zero)
						{
							if (v2Dir.X != 0f)
							{
								float fDist3 = MathHelper.Distance(xOwner.xTransform.v2Pos.X, xPlayerLockedOnto.GetBelievedPosition().X);
								if (fDist3 < xOwner.xBaseStats.fMovementSpeed * 6f * 4f)
								{
									v2Dir *= fDist3 / (xOwner.xBaseStats.fMovementSpeed * 6f * 4f);
								}
							}
							else
							{
								float fDist2 = MathHelper.Distance(xOwner.xTransform.v2Pos.Y, xPlayerLockedOnto.GetBelievedPosition().Y);
								if (fDist2 < xOwner.xBaseStats.fMovementSpeed * 6f * 4f)
								{
									v2Dir *= fDist2 / (xOwner.xBaseStats.fMovementSpeed * 6f * 4f);
								}
							}
						}
						TryMove(v2Dir);
					}
					if (fDistance3 <= fAttackDistance * 1.3f)
					{
						if (MathHelper.Distance(xOwner.xTransform.v2Pos.X, xPlayerLockedOnto.GetBelievedPosition().X) < 8f)
						{
							if (xOwner.xTransform.v2Pos.Y < xPlayerLockedOnto.GetBelievedPosition().Y)
							{
								Globals.Game._Enemy_EnterAttackAnimation(xOwner, 6);
								bTotallyMad = false;
								v2RandMoveDir = Vector2.Zero;
								iNextHardUpdate = 1;
							}
							else
							{
								Globals.Game._Enemy_EnterAttackAnimation(xOwner, 4);
								bTotallyMad = false;
								v2RandMoveDir = Vector2.Zero;
								iNextHardUpdate = 1;
							}
						}
						else if (MathHelper.Distance(xOwner.xTransform.v2Pos.Y, xPlayerLockedOnto.GetBelievedPosition().Y) < 8f)
						{
							if (xOwner.xTransform.v2Pos.X < xPlayerLockedOnto.GetBelievedPosition().X)
							{
								Globals.Game._Enemy_EnterAttackAnimation(xOwner, 5);
								bTotallyMad = false;
								v2RandMoveDir = Vector2.Zero;
								iNextHardUpdate = 1;
							}
							else
							{
								Globals.Game._Enemy_EnterAttackAnimation(xOwner, 7);
								bTotallyMad = false;
								v2RandMoveDir = Vector2.Zero;
								iNextHardUpdate = 1;
							}
						}
					}
					if (fDistance3 > fFurthestAllowedDistance)
					{
						xPlayerLockedOnto = null;
					}
				}
				else
				{
					xOwner.xRenderComponent.SwitchAnimation(0, Animation.CancelOptions.UseAnimationDefault);
				}
				if (iNextHardUpdate <= 0)
				{
					while (lv2PathFindingPositions.Count > 0)
					{
						if (Vector2.Distance(lv2PathFindingPositions[0], xOwner.xTransform.v2Pos) < 15f)
						{
							lv2PathFindingPositions.RemoveAt(0);
							continue;
						}
						iNextHardUpdate = 40;
						v2RandMoveDir = lv2PathFindingPositions[0] - xOwner.xTransform.v2Pos;
						v2RandMoveDir = v2RandMoveDir.SafeNormalized();
						xOwner.xRenderComponent.SwitchAnimation(1, Animation.CancelOptions.IgnoreIfPlaying);
						return;
					}
					iNextHardUpdate = 10;
					Game1 Lord = Globals.Game;
					float fClosest = 50000f;
					Item xClosestItem = null;
					WorldActor xClosestPlayer = null;
					List<WorldActor> lx = Lord._Enemy_GetTargetList(this);
					foreach (WorldActor xActor in lx)
					{
						float fDist = Vector2.Distance(xActor.xTransform.v2Pos, xOwner.xTransform.v2Pos);
						if (fDist < fFurthestAllowedDistance && fDist < fClosest)
						{
							xClosestPlayer = xActor;
							fClosest = fDist;
						}
					}
					if (xTauntSource != null && Vector2.Distance(xTauntSource.xTransform.v2Pos, xOwner.xTransform.v2Pos) < fFurthestAllowedDistance)
					{
						xClosestPlayer = xTauntSource;
					}
					Random knark = Globals.Game.randomInLogic;
					if (xPlayerLockedOnto == null && xClosestPlayer != null)
					{
						bTotallyMad = true;
					}
					else if (xClosestPlayer != null && knark.Next(4) == 0)
					{
						bTotallyMad = true;
					}
					if (xClosestPlayer != null)
					{
						xPlayerLockedOnto = xClosestPlayer;
						xItemLockedOnto = null;
						float fDistance = Vector2.Distance(xPlayerLockedOnto.GetBelievedPosition(), xOwner.xTransform.v2Pos);
						if (!bTotallyMad)
						{
							if (fDistance <= 30f)
							{
								bTotallyMad = true;
							}
							else if (knark.Next(3) == 0)
							{
								iNextHardUpdate = 56;
								Vector2 v2RealDir = xPlayerLockedOnto.GetBelievedPosition() - xOwner.xTransform.v2Pos;
								v2RealDir = v2RealDir.SafeNormalized();
								v2RandMoveDir = new Vector2(((float)knark.NextDouble() / 2f + 0.5f) * v2RealDir.X, ((float)knark.NextDouble() / 2f + 0.5f) * v2RealDir.Y);
								v2RandMoveDir = v2RandMoveDir.SafeNormalized();
								xOwner.xRenderComponent.SwitchAnimation(1, Animation.CancelOptions.IgnoreIfPlaying);
							}
							else
							{
								xOwner.xRenderComponent.SwitchAnimation(0, Animation.CancelOptions.IgnoreIfPlaying);
								v2RandMoveDir = Vector2.Zero;
							}
						}
					}
					else if (xClosestItem != null)
					{
						xItemLockedOnto = xClosestItem;
						xPlayerLockedOnto = null;
					}
					else
					{
						Rectangle levelRec = Globals.Game.xLevelMaster.xCurrentLevel.recCurrentBounds;
						if (!levelRec.Contains((int)xOwner.xTransform.v2Pos.X, (int)xOwner.xTransform.v2Pos.Y))
						{
							iNextHardUpdate = 70 + knark.Next(40);
							v2RandMoveDir = new Vector2(levelRec.Center.X, levelRec.Center.Y) - xOwner.xTransform.v2Pos;
							v2RandMoveDir = v2RandMoveDir.SafeNormalized();
						}
						else if (knark.Next(20) == 1)
						{
							iNextHardUpdate = 120;
							if (xClosestPlayer != null)
							{
								iNextHardUpdate = 40;
							}
							v2RandMoveDir = new Vector2(-1f + (float)knark.NextDouble() * 2f, -1f + (float)knark.NextDouble() * 2f);
							v2RandMoveDir = v2RandMoveDir.SafeNormalized();
						}
						else
						{
							iNextHardUpdate = 10;
							v2RandMoveDir = Vector2.Zero;
						}
					}
				}
			}
			xOwner.xTransform.v2ServerPos = xOwner.xTransform.v2Pos;
		}

		public static void InstanceBuilder(Enemy xEn)
		{
			string MONSTER_PATH = "Sprites/Monster/";

			// We load textures only when an enemy is needed
			string[] arrTex = new string[]
			{
					MONSTER_PATH + "Pillar Mountains/Slime/Idle/Up", // 0
                    MONSTER_PATH + "Pillar Mountains/Slime/Run/Up",
					MONSTER_PATH + "Pillar Mountains/Slime/Damage/Up",
					MONSTER_PATH + "Pillar Mountains/Slime/Damage/Reverse",
					MONSTER_PATH + "Pillar Mountains/Slime/Attack/Up",
					MONSTER_PATH + "Pillar Mountains/Slime/Attack/Right", // 5
                    MONSTER_PATH + "Pillar Mountains/Slime/Attack/Down",
					MONSTER_PATH + "Pillar Mountains/Slime/Attack/Left",
					MONSTER_PATH + "Pillar Mountains/Slime/Fall/Impact" // 8
			};

			xEn.xBaseStats.fMovementSpeed = 4f;
			xEn.xBehaviour = new ModSlimeAI(xEn);
			xEn.sAttackPhaseCategory = "GreenSlime";
			xEn.lenAssociatedEffects.Add(SortedAnimated.SortedAnimatedEffects._HitEffect_SlimeParticle);
			xEn.enDeathEffect = SortedAnimated.SortedAnimatedEffects.SlimeDeath;
			xEn.liAnimationsWithAttackphase.Add(4);
			xEn.liAnimationsWithAttackphase.Add(5);
			xEn.liAnimationsWithAttackphase.Add(6);
			xEn.liAnimationsWithAttackphase.Add(7);

			xEn.xRenderComponent.dixAnimations.Add(0, ClassAnimationPrototypes[0].CleanClone().LoadTexture(Globals.Game.Content, arrTex[0]));
			xEn.xRenderComponent.dixAnimations.Add(1, ClassAnimationPrototypes[1].CleanClone().LoadTexture(Globals.Game.Content, arrTex[1]));
			xEn.xRenderComponent.dixAnimations.Add(2, ClassAnimationPrototypes[2].CleanClone().LoadTexture(Globals.Game.Content, arrTex[2]));
			xEn.xRenderComponent.dixAnimations.Add(3, ClassAnimationPrototypes[3].CleanClone().LoadTexture(Globals.Game.Content, arrTex[3]));
			xEn.xRenderComponent.dixAnimations.Add(4, ClassAnimationPrototypes[4].CleanClone().LoadTexture(Globals.Game.Content, arrTex[4]));
			xEn.xRenderComponent.dixAnimations.Add(5, ClassAnimationPrototypes[5].CleanClone().LoadTexture(Globals.Game.Content, arrTex[5]));
			xEn.xRenderComponent.dixAnimations.Add(6, ClassAnimationPrototypes[6].CleanClone().LoadTexture(Globals.Game.Content, arrTex[6]));
			xEn.xRenderComponent.dixAnimations.Add(7, ClassAnimationPrototypes[7].CleanClone().LoadTexture(Globals.Game.Content, arrTex[7]));
			xEn.xRenderComponent.dixAnimations.Add(8, ClassAnimationPrototypes[8].CleanClone().LoadTexture(Globals.Game.Content, arrTex[8]));

			xEn.xRenderComponent.dixAnimations.Add(40000, ClassAnimationPrototypes[9].CleanClone().LoadTexture(Globals.Game.Content, arrTex[2]));
			xEn.xRenderComponent.dixAnimations.Add(40001, ClassAnimationPrototypes[10].CleanClone().LoadTexture(Globals.Game.Content, arrTex[2]));
			xEn.xRenderComponent.dixAnimations.Add(40002, ClassAnimationPrototypes[11].CleanClone().LoadTexture(Globals.Game.Content, arrTex[2]));
			xEn.xRenderComponent.dixAnimations.Add(40003, ClassAnimationPrototypes[12].CleanClone().LoadTexture(Globals.Game.Content, arrTex[2]));
			xEn.xRenderComponent.dixAnimations.Add(40004, ClassAnimationPrototypes[13].CleanClone().LoadTexture(Globals.Game.Content, arrTex[3]));
			xEn.xRenderComponent.dixAnimations.Add(40005, ClassAnimationPrototypes[14].CleanClone().LoadTexture(Globals.Game.Content, arrTex[3]));
			xEn.xRenderComponent.dixAnimations.Add(40006, ClassAnimationPrototypes[15].CleanClone().LoadTexture(Globals.Game.Content, arrTex[3]));
			xEn.xRenderComponent.dixAnimations.Add(40007, ClassAnimationPrototypes[16].CleanClone().LoadTexture(Globals.Game.Content, arrTex[3]));

			xEn.aiHitAnimation[0] = 2;
			xEn.aiHitAnimation[1] = 2;
			xEn.aiHitAnimation[2] = 2;
			xEn.aiHitAnimation[3] = 2;
			xEn.xBaseStats.enSize = BaseStats.BodySize.Small;
			xEn.v2DamageNumberPosOffset = new Vector2(-2f, -2f);
			xEn.iOverrideDamageNumberWidth = 14;
			xEn.xBaseStats.bCanBeKnockedUp = true;

			SortedAnimated.PreLoadMany(SortedAnimated.SortedAnimatedEffects._HitEffect_SlimeParticle, SortedAnimated.SortedAnimatedEffects.SlimeDeath);
			Globals.Game.xRenderMaster.RegisterSortedRenderComponent(xEn.xRenderComponent);
			xEn.xCollisionComponent.xMovementCollider = new SphereCollider(5f, Vector2.Zero, xEn.xTransform, 9f + (float)CAS.RandomInLogic.NextDouble(), xEn);
			xEn.xCollisionComponent.xMovementCollider.bIsLarge = false;
			xEn.xCollisionComponent.AddHitboxCollider(new SphereCollider(8f, Vector2.Zero, xEn.xTransform, 0f, xEn), Collider.ColliderLayers.Enemies);
		}

		public static void DifficultyScaler(Enemy xEn)
		{
			xEn.xBaseStats.iBaseATK = 69 + GameSessionData.iBaseDifficulty * 42;
		}
	}
}