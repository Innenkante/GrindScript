using Microsoft.Xna.Framework.Content;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoG.GrindScript
{
    class ModContent
    {
		public const bool UseModContentManagers = true;

		// This function overwrites _Animations_GetAnimationSet. The original function will never run
		// Determines mod to use based on input resource:
		// sResource of format "ModContent/<mod>/<resource>" will load the "<resource>" as if the original
		public bool On_Animations_GetAnimationSetPrefix(dynamic xPlayerView, string sAnimation, string sDirection, bool bWithWeapon, bool bCustomHat, bool bWithShield, bool bWeaponOnTop, dynamic __result)
        {
			__result = Utils.ConstructObject("SoG.PlayerAnimationTextureSet");
			__result.bWeaponOnTop = bWeaponOnTop;
			string sAttackPath = "";
			__result.txBase = RenderMaster.contPlayerStuff.Load<Texture2D>("Sprites/Heroes/" + sAttackPath + sAnimation + "/" + sDirection);
			if (bCustomHat && xPlayerView.xEquipment.xHat != null && xPlayerView.xEquipment.xHat.sResourceName != "")
			{
				ContentManager Content = null;
				if (ModItem.ValueIsModItem((int)xPlayerView.xEquipment.xHat.enItemType))
				{
					
					Content = 
				}
				else
				{
					Content = Utils.GetGameType("SoG.RenderMaster").GetPublicStaticField("contPlayerStuff");
				}
				if()
				__result.txHat = RenderMaster.contPlayerStuff.Load<Texture2D>("Sprites/Heroes/" + sAttackPath + sAnimation + "/Hats/" + xPlayerView.xEquipment.xHat.sResourceName + "/" + sDirection);
				__result.txHat = RenderMaster.contPlayerStuff.Load<Texture2D>("Sprites/Heroes/" + sAttackPath + sAnimation + "/Facegear/" + xPlayerView.xEquipment.xFacegear.sResourceName + "/" + sDirection);
				__result.txHair = RenderMaster.contPlayerStuff.Load<Texture2D>("Sprites/Heroes/" + sAttackPath + sAnimation + "/Hairdos/" + xPlayerView.xEquipment.xHairdo.sResourceName + "/" + sDirection);
			}
			if (bWithShield && xPlayerView.xEquipment.DisplayShield != null && xPlayerView.xEquipment.DisplayShield.sResourceName != "")
			{
				__result.txShield = RenderMaster.contPlayerStuff.Load<Texture2D>("Sprites/Heroes/" + sAttackPath + sAnimation + "/Shields/" + xPlayerView.xEquipment.DisplayShield.sResourceName + "/" + sDirection);
			}
			if (bWithWeapon)
			{
				__result.txWeapon = RenderMaster.txNullTex;
			}
			return false;
		}
    }
}
