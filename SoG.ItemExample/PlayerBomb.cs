using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SoG.GrindScript;
using Lidgren.Network;
using Microsoft.Xna.Framework.Content;

namespace SoG.ItemExample
{
	// This is mostly a Ctrl + C / V of Bomb
	// PlayerBomb two different, overlapped explosions: one that deals high damage and hits enemies, and one that deals low damage and targets players

	public class PlayerBomb : CrateArchetype
	{
		private StaticRenderComponent xShadowRC;

		private int iPreCounter;

		private int iCounter;

		private int iExplodeAt;

		private Vector2 v2MoveDir = Vector2.Zero;

		private Vector2 v2StartMoveDir;

		private int iFallAppearDelay;

		public float fExplosionRadius = 40f;

		private bool bFall;

		private bool bSilent;

		private bool bArcFall;

		private float fArcFall_MaxHeight = 10f;

		private Vector2 v2ArcFall_StartPos;

		private Vector2 v2ArcFall_EndPos;

		private int iArcFall_TravelTime;

		private int iArcFall_Counter;

		public bool bShareArcFallPos;

		private bool bExploded;

		public PlayerBomb(int p_iTimesToHitBeforeDestroy)
			: base(p_iTimesToHitBeforeDestroy)
		{
			xBaseStats = new BaseStats(this);
			xBaseStats.bAllowKnockback = false;
		}

		public override void OnNetworkInstruction(params float[] p_af)
		{
			if (p_af[0] == 1f)
			{
				CreateEffect();
				base.Destroy();
			}
			else if (p_af[0] == 2f)
			{
				v2ArcFall_EndPos = new Vector2(p_af[1], p_af[2]);
			}
		}
		
		private void CreateEffect()
		{
			Random rand = CAS.RandomInVisual;
			_ = (SortedAnimated)Utils.GetTheGame()._EffectMaster_AddEffect(new SortedAnimated(xTransform.v2Pos, SortedAnimated.SortedAnimatedEffects.ExplosionBombA));
			if (!bSilent)
			{
				Utils.GetTheGame().xSoundSystem.PlayCue("BombAxploda", xTransform.v2Pos);
			}
			for (int i = 0; i < 16; i++)
			{
				float fRotDir = (float)Math.PI * 2f * ((float)i / 16f);
				Vector2 v2Dir = Utility.RadiansToVector2(fRotDir);
				float fSpeedMod = 2f;
				float fMosMod = 0.2f;
				float fMosBas = 1f - fMosMod;
				if (i < 4)
				{
					fSpeedMod = 1f - fMosMod * ((float)i / 4f);
				}
				else if (i < 8)
				{
					fSpeedMod = fMosBas + fMosMod * ((float)(i - 4) / 4f);
				}
				else if (i < 12)
				{
					fSpeedMod = 1f - fMosMod * ((float)(i - 8) / 4f);
				}
				else if (i < 16)
				{
					fSpeedMod = fMosBas + fMosMod * ((float)(i - 12) / 4f);
				}
				rand.NextDouble();
				_Effect_MovingAnimated eff = Utils.GetTheGame()._EffectMaster_AddEffect(new _Effect_MovingAnimated(xTransform.v2Pos, SortedAnimated.SortedAnimatedEffects.Particle_DustCloudSmall01, v2Dir * 1.6f * fSpeedMod, 40, 20, 0.93f)) as _Effect_MovingAnimated;
				eff.xRenderComponent.fVirtualHeight += xRenderComponent.fVirtualHeight;
				(eff.xRenderComponent as AnimatedRenderComponent).fAnimationTimeWarp = 1.3f;
				eff.fRotationMod = (float)rand.NextDouble() * 0.5f - 0.25f;
				eff.xRenderComponent.cColor = Color.Brown;
			}
		}

		public override void Destroy()
		{
			if (bExploded)
			{
				return;
			}
			bExploded = true;
			base.Destroy();
			CreateEffect();
			if (CAS.NetworkRole != NetworkHelperInterface.NetworkRole.Client)
			{
				_EnemySpells_ArbitraryCircularExplosion xSpell = (_EnemySpells_ArbitraryCircularExplosion)Utils.GetTheGame()._EntityMaster_AddSpellInstance(SpellCodex.SpellTypes._EnemySkill_ArbitraryCircularDamage, this, xTransform.v2Pos, false);
				xSpell.xAttackPhasePlayer.xStats.sAttackHandle = "Bomb";
				xSpell.xAttackPhasePlayer.xStats.iBreakingPower = 10;
				xSpell.xAttackPhasePlayer.xStats.iBaseDamage = 1600;
				xSpell.xAttackPhasePlayer.lenLayers.Add(Collider.ColliderLayers.Enemies);
				xSpell.xAttackPhasePlayer.lenLayers.Remove(Collider.ColliderLayers.Players);

				xSpell.xAttackPhasePlayer.xStats.fKnockBack = 40f;
				(xSpell.xAttackPhasePlayer.lxCurrentColliders[0] as SphereCollider).fRadius = fExplosionRadius;

				_EnemySpells_ArbitraryCircularExplosion xSpell2 = (_EnemySpells_ArbitraryCircularExplosion)Utils.GetTheGame()._EntityMaster_AddSpellInstance(SpellCodex.SpellTypes._EnemySkill_ArbitraryCircularDamage, this, xTransform.v2Pos, false);
				xSpell2.xAttackPhasePlayer.xStats.sAttackHandle = "Bomb";
				xSpell2.xAttackPhasePlayer.xStats.iBreakingPower = 10;
				xSpell2.xAttackPhasePlayer.xStats.iBaseDamage = 100;
				xSpell2.xAttackPhasePlayer.lenLayers.Clear();
				xSpell2.xAttackPhasePlayer.lenLayers.Add(Collider.ColliderLayers.Players);

				xSpell2.xAttackPhasePlayer.xStats.fKnockBack = 40f;
				(xSpell2.xAttackPhasePlayer.lxCurrentColliders[0] as SphereCollider).fRadius = fExplosionRadius;

			}
			SendNetworkInstruction(1f);
		}

		public void SetInfo_Bounce(Vector2 v2DirSpeed, int iExplodeAt)
		{
			xCollisionComponent.SetPropertyInGroup(CollisionComponent.ColliderGroup.All, CollisionComponent.ColliderProperty.Ghost);
			xCollisionComponent.DeactivateGroup(CollisionComponent.ColliderGroup.Combat);
			v2StartMoveDir = (v2MoveDir = v2DirSpeed);
			this.iExplodeAt = iExplodeAt;
			if (Utility.ConvertV2DirectionToClosestAnimationDirection(v2DirSpeed) < 2)
			{
				xRenderComponent.SwitchAnimation(0, Animation.CancelOptions.RestartIfPlaying);
			}
			else
			{
				xRenderComponent.SwitchAnimation(1, Animation.CancelOptions.RestartIfPlaying);
			}
			if (CAS.NetworkRole == NetworkHelperInterface.NetworkRole.Server)
			{
				OutMessage om = Utils.GetTheGame()._Network_CreateMessage();
				om.Write(byte.MaxValue);
				om.Write((byte)14);
				om.Write((byte)0);
				om.Write(iID);
				om.Write(xTransform.v2Pos.X);
				om.Write(xTransform.v2Pos.Y);
				om.Write(v2DirSpeed.X);
				om.Write(v2DirSpeed.Y);
				om.Write(xRenderComponent.fVirtualHeight);
				om.Write(xCollisionComponent.ibitCurrentColliderLayer);
				om.Write(iExplodeAt);
				Utils.GetTheGame()._Network_SendMessage(om, 13);
			}
		}

		public void SetInfo_Fall(int iExplodeAt, int iStartFall = 100)
		{
			bFall = true;
			xShadowRC = new StaticRenderComponent(CAS.RegionContent.Load<Texture2D>("Sprites/Monster/Temple/Marauder/Jump/Shadow"), xTransform);
			xShadowRC.fAlpha = 0f;
			xShadowRC.fScale = 0f;
			Utils.GetTheGame().xRenderMaster.RegisterBelowSorted(xShadowRC);
			xCollisionComponent.SetPropertyInGroup(CollisionComponent.ColliderGroup.All, CollisionComponent.ColliderProperty.Ghost);
			xCollisionComponent.DeactivateGroup(CollisionComponent.ColliderGroup.Combat);
			xRenderComponent.v2OffsetRenderPos.Y -= 100f;
			xRenderComponent.fAlpha = 0f;
			iFallAppearDelay = iStartFall;
			this.iExplodeAt = iExplodeAt;
			if (CAS.RandomInVisual.Next(2) == 0)
			{
				xRenderComponent.SwitchAnimation(2);
			}
			else
			{
				xRenderComponent.SwitchAnimation(3);
			}
			if (CAS.NetworkRole == NetworkHelperInterface.NetworkRole.Server)
			{
				OutMessage om = Utils.GetTheGame()._Network_CreateMessage();
				om.Write(byte.MaxValue);
				om.Write((byte)14);
				om.Write((byte)1);
				om.Write(iID);
				om.Write(xTransform.v2Pos.X);
				om.Write(xTransform.v2Pos.Y);
				om.Write(xRenderComponent.fVirtualHeight);
				om.Write(xCollisionComponent.ibitCurrentColliderLayer);
				om.Write(iExplodeAt);
				Utils.GetTheGame()._Network_SendMessage(om, 13);
			}
		}

		public void SetInfo_ArcFall(Vector2 p_v2StartPos, Vector2 p_v2EndPos, float p_fMaxHeight, int p_iTravelTime, int iExplodeAt)
		{
			bNetworkSynchEnabled = false;
			this.iExplodeAt = iExplodeAt;
			bArcFall = true;
			v2ArcFall_StartPos = p_v2StartPos;
			v2ArcFall_EndPos = p_v2EndPos;
			fArcFall_MaxHeight = p_fMaxHeight;
			iArcFall_TravelTime = p_iTravelTime;
			xShadowRC = new StaticRenderComponent(CAS.RegionContent.Load<Texture2D>("Sprites/Monster/Temple/Marauder/Jump/Shadow"), xTransform);
			xShadowRC.fAlpha = 0f;
			xShadowRC.fScale = 0f;
			Utils.GetTheGame().xRenderMaster.RegisterBelowSorted(xShadowRC);
			xCollisionComponent.SetPropertyInGroup(CollisionComponent.ColliderGroup.All, CollisionComponent.ColliderProperty.Ghost);
			xCollisionComponent.DeactivateGroup(CollisionComponent.ColliderGroup.Combat);
			float fTotalTailMovement = 29.2f;
			float fSpeed = Vector2.Distance(v2ArcFall_StartPos, v2ArcFall_EndPos) / (float)iArcFall_TravelTime;
			v2StartMoveDir = Utility.Normalize(v2ArcFall_EndPos - v2ArcFall_StartPos) * fSpeed;
			if (Utility.ConvertV2DirectionToClosestAnimationDirection(v2StartMoveDir) < 2)
			{
				xRenderComponent.SwitchAnimation(6);
			}
			else
			{
				xRenderComponent.SwitchAnimation(7);
			}
			v2ArcFall_EndPos += Utility.Normalize(v2ArcFall_StartPos - v2ArcFall_EndPos) * fSpeed * fTotalTailMovement;
			iArcFall_TravelTime -= (int)fTotalTailMovement;
		}

		public override void Update()
		{
			base.Update();
			if (bArcFall)
			{
				iArcFall_Counter++;
				if (CAS.NetworkRole == NetworkHelperInterface.NetworkRole.Server && iArcFall_Counter == iArcFall_TravelTime / 2 && bShareArcFallPos)
				{
					SendNetworkInstruction(2f, v2ArcFall_EndPos.X, v2ArcFall_EndPos.Y);
				}
				float fGrade = (float)iArcFall_Counter / (float)iArcFall_TravelTime;
				float fHeight = Utility.Percent_RealAndragrad(fGrade) * fArcFall_MaxHeight;
				xTransform.v2Pos = Vector2.Lerp(v2ArcFall_StartPos, v2ArcFall_EndPos, fGrade);
				xRenderComponent.v2OffsetRenderPos.Y = 0f - fHeight;
				if (xRenderComponent.fVirtualHeight > 0f)
				{
					xRenderComponent.fVirtualHeight -= 1f;
				}
				xShadowRC.fAlpha = 0.4f;
				xShadowRC.fScale = 1f - (0f - xRenderComponent.v2OffsetRenderPos.Y) / 130f;
				if (xShadowRC.fScale < 0.4f)
				{
					xShadowRC.fScale = 0.4f;
				}
				if (iArcFall_Counter == iArcFall_TravelTime)
				{
					bArcFall = false;
					xRenderComponent.fVirtualHeight = 0f;
					Utils.GetTheGame().xSoundSystem.PlayCue("MimicMedium_BombLand", xTransform);
					if (Utility.ConvertV2DirectionToClosestAnimationDirection(v2StartMoveDir) < 2)
					{
						xRenderComponent.SwitchAnimation(0);
					}
					else
					{
						xRenderComponent.SwitchAnimation(1);
					}
					xRenderComponent.v2OffsetRenderPos.Y = 0f;
					xCollisionComponent.SetPropertyInGroup(CollisionComponent.ColliderGroup.All, CollisionComponent.ColliderProperty.NonGhost);
					xCollisionComponent.ActivateGroup(CollisionComponent.ColliderGroup.Combat);
					xShadowRC.Unregister();
					xShadowRC = null;
					v2MoveDir = v2StartMoveDir;
				}
				return;
			}
			if (bFall)
			{
				if (xShadowRC == null || xShadowRC.fAlpha > 0.6f)
				{
					if (xRenderComponent.v2OffsetRenderPos.Y < 0f)
					{
						xRenderComponent.v2OffsetRenderPos.Y += 4f;
						if (xRenderComponent.v2OffsetRenderPos.Y >= 0f)
						{
							xRenderComponent.v2OffsetRenderPos.Y = 0f;
							xCollisionComponent.SetPropertyInGroup(CollisionComponent.ColliderGroup.All, CollisionComponent.ColliderProperty.NonGhost);
							xCollisionComponent.ActivateGroup(CollisionComponent.ColliderGroup.Movement);
							xCollisionComponent.ActivateGroup(CollisionComponent.ColliderGroup.Combat);
							xShadowRC.Unregister();
							xShadowRC = null;
							bFall = false;
						}
					}
					if (xRenderComponent.fAlpha < 1f)
					{
						xRenderComponent.fAlpha += 0.04f;
						if (xRenderComponent.fAlpha > 1f)
						{
							xRenderComponent.fAlpha = 1f;
						}
					}
				}
				if (xShadowRC != null)
				{
					float fMos = (float)iCounter / (float)iFallAppearDelay;
					if (fMos > 1f)
					{
						fMos = 1f;
					}
					if (fMos < 0f)
					{
						fMos = 0f;
					}
					xShadowRC.fAlpha = fMos * 0.7f;
					xShadowRC.fScale = fMos;
				}
			}
			if (xRenderComponent.iActiveAnimation > 1)
			{
				iCounter++;
			}
			else
			{
				iPreCounter++;
				if (iPreCounter == 20)
				{
					v2MoveDir = v2StartMoveDir * 0.6f;
					xCollisionComponent.SetPropertyInGroup(CollisionComponent.ColliderGroup.Movement, CollisionComponent.ColliderProperty.NonGhost);
					xCollisionComponent.ActivateGroup(CollisionComponent.ColliderGroup.Combat);
				}
				else if (iPreCounter == 28)
				{
					v2MoveDir = v2StartMoveDir * 0.3f;
				}
				else if (iPreCounter == 36)
				{
					v2MoveDir = v2StartMoveDir * 0.15f;
				}
				else if (iPreCounter == 44)
				{
					v2MoveDir = v2StartMoveDir * 0.1f;
				}
				else if (iPreCounter == 52)
				{
					v2MoveDir = v2StartMoveDir * 0f;
				}
			}
			xTransform.v2Pos += v2MoveDir;
			if (iCounter >= iExplodeAt - 90 && iCounter >= 52)
			{
				xRenderComponent.SwitchAnimation((ushort)(xRenderComponent.GetCurrentAnimation().byAnimationDirection + 4), Animation.CancelOptions.IgnoreIfPlaying);
				if (iCounter <= 52)
				{
					int iTotalTicks = 93;
					xRenderComponent.fAnimationTimeWarp = 1f + (float)iCounter / (float)iTotalTicks;
				}
			}
			if (iCounter == iExplodeAt)
			{
				Destroy();
			}
		}

		public override void OnHitByAttack(AttackCollisionData xAtColData, AttackPhase xAtPhase)
		{
			if ((!(xAtPhase.xStats.sAttackHandle != "Bomb") || CAS.DifficultySetting >= 3) && iCounter >= 30)
			{
				if (iCounter < iExplodeAt - 5)
				{
					iCounter = iExplodeAt - 5;
				}
				_ = iTimesHit;
				_ = iTimesToHitBeforeDestroy;
			}
		}

		public static PlayerBomb InstanceBuilder(ContentManager xContent)
        {
			string ENVIRON_PATH = "Sprites/Environment/";

			PlayerBomb xEn = new PlayerBomb(1);
			xEn.bInstantiateInNetwork = false;
			xEn.bNetworkSynchEnabled = true;
			xEn.xRenderComponent.xOwnerObject = xEn;
			xEn.enHitEffect = SortedAnimated.SortedAnimatedEffects.None;
			xEn.bDestroyOnHit = true;
			xEn.xRenderComponent.dixAnimations.Add(0, new Animation(0, 0, xContent.Load<Texture2D>(ENVIRON_PATH + "Traps/Bomb/Roll"), new Vector2(15f, 31f), 4, 17, 31, 35, 0, 0, 20, Animation.LoopSettings.Clamp, Animation.CancelOptions.IgnoreIfPlaying, true, true, new AnimationInstruction(new AnimInsCriteria(AnimInsCriteria.Criteria.TriggerAtEnd), new AnimInsEvent(AnimInsEvent.EventType.PlayAnimation, 2f))));
			xEn.xRenderComponent.dixAnimations.Add(1, new Animation(1, 1, xContent.Load<Texture2D>(ENVIRON_PATH + "Traps/Bomb/Roll"), new Vector2(16f, 31f), 4, 17, 31, 35, 0, 0, 20, Animation.LoopSettings.Clamp, Animation.CancelOptions.IgnoreIfPlaying, true, true, new AnimationInstruction(new AnimInsCriteria(AnimInsCriteria.Criteria.TriggerAtEnd), new AnimInsEvent(AnimInsEvent.EventType.PlayAnimation, 3f))));
			xEn.xRenderComponent.dixAnimations[1].enSpriteEffect = SpriteEffects.FlipHorizontally;
			xEn.xRenderComponent.dixAnimations.Add(2, new Animation(2, 0, xContent.Load<Texture2D>(ENVIRON_PATH + "Traps/Bomb/Bomb"), new Vector2(15f, 18f), 4, 3, 31, 22, 0, 0, 20, Animation.LoopSettings.Looping, Animation.CancelOptions.IgnoreIfPlaying, true, true));
			xEn.xRenderComponent.dixAnimations.Add(3, new Animation(3, 1, xContent.Load<Texture2D>(ENVIRON_PATH + "Traps/Bomb/Bomb"), new Vector2(16f, 18f), 4, 3, 31, 22, 0, 0, 20, Animation.LoopSettings.Looping, Animation.CancelOptions.IgnoreIfPlaying, true, true));
			xEn.xRenderComponent.dixAnimations[3].enSpriteEffect = SpriteEffects.FlipHorizontally;
			xEn.xRenderComponent.dixAnimations.Add(4, new Animation(4, 0, xContent.Load<Texture2D>(ENVIRON_PATH + "Traps/Bomb/Ticking"), new Vector2(15f, 18f), 4, 3, 31, 22, 0, 0, 20, Animation.LoopSettings.Looping, Animation.CancelOptions.IgnoreIfPlaying, true, true));
			xEn.xRenderComponent.dixAnimations.Add(5, new Animation(5, 1, xContent.Load<Texture2D>(ENVIRON_PATH + "Traps/Bomb/Ticking"), new Vector2(16f, 18f), 4, 3, 31, 22, 0, 0, 20, Animation.LoopSettings.Looping, Animation.CancelOptions.IgnoreIfPlaying, true, true));
			xEn.xRenderComponent.dixAnimations[5].enSpriteEffect = SpriteEffects.FlipHorizontally;
			xEn.xRenderComponent.dixAnimations.Add(6, new Animation(6, 0, xContent.Load<Texture2D>(ENVIRON_PATH + "Traps/Bomb/InAir"), new Vector2(15f, 21f), 4, 5, 31, 35, 0, 0, 20, Animation.LoopSettings.Looping, Animation.CancelOptions.IgnoreIfPlaying, true, true));
			xEn.xRenderComponent.dixAnimations.Add(7, new Animation(7, 1, xContent.Load<Texture2D>(ENVIRON_PATH + "Traps/Bomb/InAir"), new Vector2(16f, 21f), 4, 5, 31, 35, 0, 0, 20, Animation.LoopSettings.Looping, Animation.CancelOptions.IgnoreIfPlaying, true, true));
			xEn.xRenderComponent.dixAnimations[7].enSpriteEffect = SpriteEffects.FlipHorizontally;
			float fWeight = 10f + 5f * (float)CAS.RandomInLogic.NextDouble();
			if (CAS.GameMode == StateMaster.GameModes.RogueLike)
			{
				fWeight = 2f + 2f * (float)CAS.RandomInLogic.NextDouble();
			}
			xEn.xCollisionComponent.xMovementCollider = new BoxCollider(15, 6, 0f, new Vector2(0f, 0f), xEn.xTransform, fWeight, xEn);
			xEn.xCollisionComponent.xMovementCollider.bIsLarge = false;
			Utils.GetTheGame().xCollisionMaster.RegisterMovementCollider(xEn.xCollisionComponent.xMovementCollider);
			xEn.xCollisionComponent.AddHitboxCollider(new BoxCollider(15, 6, 0f, new Vector2(0f, 0f), xEn.xTransform, 10f, xEn), Collider.ColliderLayers.DynamicEnvironment);
			xEn.xCollisionComponent.xMovementCollider.bStaticKnockback = false;

			return xEn;
		}
	}
}
