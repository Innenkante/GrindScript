using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoG.GrindScript
{
    public class Ui
    {
        public static void AddMiscTextTo(LocalGame game, string category, string entry, string text, MiscTextTypes textType = MiscTextTypes.Default)
        {
            try
            {
                dynamic textEntry = Utils.GetGameType("SoG.MiscText").GetConstructor(Type.EmptyTypes).Invoke(null);
                game.GetUnderlayingGame().xMiscTextGod_Default.dsxTextCollections[category].dsxTexts[entry] = textEntry;

                // Set extra params

                // No support for color tags yet...
                textEntry.sUnparsedFullLine = textEntry.sUnparsedBaseLine = text;

                switch (textType)
                {
                    case MiscTextTypes.GenericItemName:
                        textEntry.iMaxWidth = 170;
                        textEntry.iMaxHeight = 19;
                        textEntry.enFontType = (dynamic)Enum.ToObject(Utils.GetGameType("SoG.FontManager").GetDeclaredNestedType("FontType"), (int)FontType.Bold8Spacing1); //Bold8Spacing1
                        break;
                    case MiscTextTypes.GenericItemDescription:
                        // "You screwed up but we're putting in random values anyway" case
                        textEntry.iMaxWidth = 200;
                        textEntry.iMaxHeight = 100;
                        textEntry.enFontType = (dynamic)Enum.ToObject(Utils.GetGameType("SoG.FontManager").GetDeclaredNestedType("FontType"), (int)FontType.Reg7); //Reg 7
                        break;
                    case MiscTextTypes.Default:
                    default:
                        // "You screwed up but we're putting in random values anyway" case
                        textEntry.iMaxWidth = 200;
                        textEntry.iMaxHeight = 100;
                        textEntry.enFontType = (dynamic)Enum.ToObject(Utils.GetGameType("SoG.FontManager").GetDeclaredNestedType("FontType"), (int)FontType.Reg7); //Reg 7
                        break;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Couldn't add misc text " + category + ":" + entry + ":" + text);
                Console.WriteLine("Reason: " + e);
            }
        }
    }
}
