using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoG.GrindScript
{
    public class Dialogue
    {
        public static void AddDialogueLineTo(LocalGame localGame, string line)
        {
            var game = localGame.GetUnderlayingGame();

			game.xInGameMenu.TrueExit();
            game.xShopMenu.ExitShop();
            game.xInput_Game.MenuButton.bPressed = false;

            game._Input_StopAllMovement();

            bool bInDialogueBefore = game.xDialogueSystem.bInDialogue;
            game.xDialogueSystem.SetCustomLineDialogue(line);
            if (bInDialogueBefore)
            {
                game.xDialogueSystem.iFadeCount = 0;
            }
            game.xInput_Game.Action.bDown = true;
        }
    }
}
