using Microsoft.Xna.Framework.Content;
using SoG.Modding.Utils;
using System;

namespace SoG.Modding.Extensions
{
    public static class AnimationExtension
    {
        /// <summary>
        /// Creates a new Animation with the same parameters as the given argument.
        /// The returned animations is reset for a fresh playback.
        /// </summary>
        public static Animation CleanClone(this Animation anim)
        {
            if (anim == null)
                throw new NullReferenceException("Animation is null");

            Animation clone = new Animation(0, 0, RenderMaster.txNullTex, default)
            {
                iID = anim.iID,
                iTicksPerFrame = anim.iTicksPerFrame,
                iEndFrame = anim.iEndFrame,
                bReversePlayback = anim.bReversePlayback,
                fInnateTimeWarp = anim.fInnateTimeWarp,
                iLogicFrame = default,
                iPreviousFrame = default,
                iRenderedFrame = default,
                iLoops = default,
                iCellWidth = anim.iCellWidth,
                iCellHeight = anim.iCellHeight,
                iCellRenderWidth = anim.iCellRenderWidth,
                iCellRenderHeight = anim.iCellRenderHeight,
                iOffsetXOverride = anim.iOffsetXOverride,
                iOffsetYOverride = anim.iOffsetYOverride,
                iOffsetX = anim.iOffsetX,
                iOffsetY = anim.iOffsetY,
                iFramesPerRow = anim.iFramesPerRow,
                bDisableAnimationMovement = anim.bDisableAnimationMovement,
                enCancelOptions = anim.enCancelOptions,
                byAnimationDirection = anim.byAnimationDirection,
                v2PositionOffset = anim.v2PositionOffset,
                recCurrentFrame = default,
                txTexture = anim.txTexture,
                txTwilightTexture = anim.txTwilightTexture,
                enLoopSettings = anim.enLoopSettings,
                iMaximumClientPrediction = anim.iMaximumClientPrediction,
                bIgnoreSentTicks = anim.bIgnoreSentTicks,
                bOverrideRotation = anim.bOverrideRotation,
                fRotation = anim.fRotation,
                bIsMoveCancellable = anim.bIsMoveCancellable,
                bIsSpellCancellable = anim.bIsSpellCancellable,
                bIsAttackCancellable = anim.bIsAttackCancellable,
                bIsFlashCancellable = anim.bIsFlashCancellable,
                enSpriteEffect = anim.enSpriteEffect,
                bAtEnd = default,
                iLoopResets = default
            };

            if (anim.lxAnimationInstructions != null)
            {
                anim.lxAnimationInstructions.ForEach(x => clone.lxAnimationInstructions.Add(x.Clone()));
            }

            anim.Reset();

            return clone;
        }

        /// <summary>
        /// Loads this animation's texture from the given path and content manager.
        /// </summary>
        public static Animation LoadTexture(this Animation anim, ContentManager manager, string path)
        {
            AssetUtils.TryLoadTexture(path, manager, out anim.txTexture);
            return anim;
        }

        /// <summary>
        /// Loads this animation's twilight texture from the given path and content manager.
        /// </summary>
        public static Animation LoadTwilightTexture(this Animation anim, ContentManager manager, string path)
        {
            AssetUtils.TryLoadTexture(path, manager, out anim.txTwilightTexture);
            return anim;
        }

        /// <summary>
        /// Sets this animation's instructions to the given animation instructions.
        /// </summary>
        public static Animation SetInstructions(this Animation anim, params AnimationInstruction[] instructions)
        {
            anim.lxAnimationInstructions.Clear();
            anim.lxAnimationInstructions.AddRange(instructions);
            return anim;
        }
    }
}
