using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace SoG.GrindScript
{
    /// <summary> Utility class that can be used to setup Animations easier. </summary>
    public class AnimationPrototype : Animation
    {
        private ContentManager xContent;

        private string sTexturePath;

        private string sTwilightTexturePath;

        /// <summary> 
        /// Creates a new prototype with minimal information. 
        /// Remaining information should be specified with respective method calls (SetResource(), etc...)
        /// </summary>
        /// <remarks> The textures are not loaded right away, but when Clone() is called. </remarks>
        public AnimationPrototype(ContentManager xContent, ushort iID, byte byAnimationDirection) : base(iID, byAnimationDirection, RenderMaster.txNullTex, new Vector2(0f, 0f))
        {
            this.xContent = xContent;
        }

        /// <summary> Creates a new prototype with a constructor format similar to Animation. </summary>
        /// <remarks> The textures are not loaded right away, but when Clone() is called. </remarks>
        public AnimationPrototype(ContentManager xContent, ushort p_iID, byte p_byAnimationDirection, string sTexture, Vector2 p_v2PositionOffset, int p_iTicksPerFrame, int p_iEndFrame, int p_iCellWidth, int p_iCellHeight, int p_iOffsetX, int p_iOffsetY, int p_iFramesPerRow, LoopSettings p_enLoopSettings, CancelOptions p_enDefaultCancelOptions, bool p_bIsMoveCancellable, bool p_bIsSpellCancellable, params AnimationInstruction[] p_axAnimationInstructions)
            : base(p_iID, p_byAnimationDirection, RenderMaster.txNullTex, p_v2PositionOffset, p_iTicksPerFrame, p_iEndFrame, p_iCellWidth, p_iCellHeight, p_iOffsetX, p_iOffsetY, p_iFramesPerRow, p_enLoopSettings, p_enDefaultCancelOptions, p_bIsMoveCancellable, p_bIsSpellCancellable, p_axAnimationInstructions)
        {
            sTexturePath = sTexture;
            this.xContent = xContent;
        }

        /// <summary> 
        /// Sets the resource data used by the Animation.
        /// A Twilight texture can be provided, but is optional.
        /// </summary>
        public void SetResource(string sTexturePath, int iTicksPerFrame, int iEndFrame, int iCellWidth, int iCellHeight, int iFramesPerRow, string sTwilightPath = "")
        {
            this.iTicksPerFrame = iTicksPerFrame;
            this.sTexturePath = sTexturePath;
            this.iEndFrame = iEndFrame;
            this.iCellRenderWidth = (this.iCellHeight = iCellWidth);
            this.iCellRenderHeight = (this.iCellHeight = iCellHeight);
            this.iFramesPerRow = iFramesPerRow;
            this.sTwilightTexturePath = sTwilightPath;
        }

        /// <summary> 
        /// Sets the Animation's render offset.
        /// This will offset the animation relative to its position.
        /// </summary>
        public void SetOffset(Vector2 v2Offset)
        {
            v2PositionOffset = v2Offset;
        }

        /// <summary> Sets various settings, such as looping and actions that can cancel the Animation. </summary>
        public void SetOptions(Animation.LoopSettings enLooping, Animation.CancelOptions enCancel, bool bSpellCancellable, bool bMoveCancellable)
        {
            enLoopSettings = enLooping;
            enCancelOptions = enCancel;
            bIsSpellCancellable = bSpellCancellable;
            bIsAttackCancellable = (bIsMoveCancellable = bMoveCancellable);
        }

        /// <summary> Sets various settings, such as looping and actions that can cancel the Animation. </summary>
        public void SetOptions(Animation.LoopSettings enLooping, Animation.CancelOptions enCancel, bool bSpellCancellable, bool bMoveCancellable, bool bAttackCancellable)
        {
            enLoopSettings = enLooping;
            enCancelOptions = enCancel;
            bIsSpellCancellable = bSpellCancellable;
            bIsMoveCancellable = bMoveCancellable;
            bIsAttackCancellable = bAttackCancellable;
        }

        /// <summary>
        /// Sets the Animation's instructions.
        /// These are used to execute actions based on certain criteria.
        /// </summary>
        public void SetInstructions(params AnimationInstruction[] p_xInstructions)
        {
            lxAnimationInstructions = new List<AnimationInstruction>(p_xInstructions);
        }

        /// <summary> Creates a new Animation, which is a copy of this AnimationPrototype. </summary>
        /// <remarks> 
        /// Cloning will load the textures stored in sTexturePath and (if not empty) sTwilightPath,
        /// using the previously provided Content Manager. 
        /// </remarks>
        /// <returns> An Animation with identical data to the AnimationPrototype. </returns>
        public Animation Clone()
        {
            return new Animation(iID, byAnimationDirection, xContent.Load<Texture2D>(sTexturePath), v2PositionOffset)
            {
                iTicksPerFrame = this.iTicksPerFrame,
                iEndFrame = this.iEndFrame,
                bReversePlayback = this.bReversePlayback,
                fInnateTimeWarp = this.fInnateTimeWarp,
                iLogicFrame = this.iLogicFrame,
                iPreviousFrame = this.iPreviousFrame,
                iRenderedFrame = this.iRenderedFrame,
                iLoops = this.iLoops,
                iCellWidth = this.iCellWidth,
                iCellHeight = this.iCellHeight,
                iCellRenderWidth = this.iCellRenderWidth,
                iCellRenderHeight = this.iCellRenderHeight,
                iOffsetXOverride = this.iOffsetXOverride,
                iOffsetYOverride = this.iOffsetYOverride,
                iOffsetX = this.iOffsetX,
                iOffsetY = this.iOffsetY,
                iFramesPerRow = this.iFramesPerRow,
                bDisableAnimationMovement = this.bDisableAnimationMovement,
                enCancelOptions = this.enCancelOptions,
                recCurrentFrame = new Rectangle(this.recCurrentFrame.X, this.recCurrentFrame.Y, this.recCurrentFrame.Width, this.recCurrentFrame.Height),
                txTwilightTexture = sTwilightTexturePath != "" ? xContent.Load<Texture2D>(sTwilightTexturePath) : this.txTwilightTexture,
                enLoopSettings = this.enLoopSettings,
                iMaximumClientPrediction = this.iMaximumClientPrediction,
                bIgnoreSentTicks = this.bIgnoreSentTicks,
                bOverrideRotation = this.bOverrideRotation,
                fRotation = this.fRotation,
                bIsMoveCancellable = this.bIsMoveCancellable,
                bIsSpellCancellable = this.bIsSpellCancellable,
                bIsAttackCancellable = this.bIsAttackCancellable,
                bIsFlashCancellable = this.bIsFlashCancellable,
                enSpriteEffect = this.enSpriteEffect,
                lxAnimationInstructions = new List<AnimationInstruction>(this.lxAnimationInstructions)
            };
        }
    }
}
