using Microsoft.Xna.Framework;

namespace SoG.Modding.Extensions
{
    public static class ItemExtension
    {
        /// <summary>
        /// Spawns an item at the target PlayerView's position.
        /// </summary>
        public static Item SpawnItem(this ItemCodex.ItemTypes enType, PlayerView xTarget)
        {
            PlayerEntity xEntity = xTarget.xEntity;

            return enType.SpawnItem(xEntity.xTransform.v2Pos, xEntity.xRenderComponent.fVirtualHeight, xEntity.xCollisionComponent.ibitCurrentColliderLayer);
        }

        /// <summary>
        /// Spawns an item at the target position.
        /// </summary>
        public static Item SpawnItem(this ItemCodex.ItemTypes enType, Vector2 v2Pos, float fVirtualHeight, int iColliderLayer)
        {
            Vector2 v2ThrowDirection = Utility.RandomizeVector2Direction(CAS.RandomInLogic);

            return Globals.Game._EntityMaster_AddItem(enType, v2Pos, fVirtualHeight, iColliderLayer, v2ThrowDirection);
        }
    }
}
