using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace SoG.GrindScript
{
    public class CustomItem : ConvertedObject
    {
        public int Id { get; set; }

        public int EnType
        {
            get => (int)_originalObject.enType;
            set => _originalObject.enType = Enum.ToObject(Utils.GetGameType("SoG.ItemCodex+ItemTypes"), value);
        }

        public bool IsVanillaItem 
            => Enum.IsDefined(Utils.GetGameType("SoG.ItemCodex+ItemTypes"), EnType);

        public CustomItem(object originalObject) : base(originalObject)
        {
            
        }

        public static CustomItem AddCustomItemTo(LocalGame game, string name, string description, string appearance, int value)
        {
            int newId = 1;
            if (BaseScript.CustomItems.Count > 0)
                newId = BaseScript.CustomItems.Count + 1;


            dynamic newItem = Utils.GetGameType("SoG.ItemDescription").GetConstructor(Type.EmptyTypes).Invoke(null);

            Ui.AddMiscTextTo(game, "Items", name.Trim() + "_Name", name, MiscTextTypes.GenericItemName);
            Ui.AddMiscTextTo(game,"Items", description.Trim() + "_Description", description, MiscTextTypes.GenericItemDescription);

            newItem.sFullName = name;
            newItem.txDisplayImage = game.GetRegionContentManager().Load<Texture2D>(appearance);
            newItem.lenCategory.Add((dynamic)Enum.ToObject(ModCodex.SoGType.ItemCategories, 0));
            newItem.sNameLibraryHandle = name.Trim() + "_Name";
            newItem.sDescriptionLibraryHandle = description.Trim() + "_Description";
            newItem.sCategory = "Misc";
            newItem.iInternalLevel = 1;
            newItem.iValue = value;

            dynamic items = Utils.GetGameType("SoG.ItemCodex").GetPublicStaticField("denxLoadedDescriptions");


            newItem.enType = (dynamic)Enum.ToObject(Utils.GetGameType("SoG.ItemCodex+ItemTypes"), 400000 + newId);


            items[newItem.enType] = newItem;

            Console.WriteLine("Added the custom item called " + name + " with the id" + newId + "...");

            var customItem = new CustomItem(newItem) {Id = newId};

            return customItem;
        }

        public void SpawnOn(LocalGame game,Player player)
        {
            var function = Utils.GetGameType("SoG.Game1").GetDeclaredMethods("_EntityMaster_AddItem").First();
            function.Invoke(game.GetUnderlayingGame(), new[] { EnType, player.Original.xEntity.xTransform.v2Pos, player.Original.xEntity.xRenderComponent.fVirtualHeight, player.Original.xEntity.xCollisionComponent.ibitCurrentColliderLayer, Vector2.Zero });
        }
    }
}
