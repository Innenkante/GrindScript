using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Content;

namespace SoG.GrindScript
{
    /// <summary>
    /// Helper class that stores additional information for modded dynamic environments.
    /// Used internally by GrindScript.
    /// </summary>
    public class ModDynEnvData
    {
        public DynamicEnvironmentCodex.ObjectTypes enType;

        public DynEnvBuilderPrototype InstanceBuilder;

        public ContentManager xContent;

        public ModDynEnvData(DynamicEnvironmentCodex.ObjectTypes enType)
        {
            this.enType = enType;
        }
    }

    /// <summary> Contains methods for working with dynamic environments (Crates, Bushes, Bombs, Doors, etc.). </summary>
    public static class DynEnvHelper
    {
        // Static methods

        /// <summary> 
        /// Allocates a new DynamicEnvironmentCodex.ObjectTypes value, and sets the delegate used to build dynamic environments of that type. 
        /// <para> The delegate is where you instantiate your dynamic environment object and return it, so that it can be used by SoG. </para>
        /// <para> You can either use an existing class, or make your own class that derives from DynamicEnvironment (for example, public MyAmazingBarrel : DynamicEnvironment { ... }) </para>
        /// </summary>
        public static DynamicEnvironmentCodex.ObjectTypes CreateDynamicEnvironmentEntry(ContentManager xContent, DynEnvBuilderPrototype xBuilder)
        {
            DynamicEnvironmentCodex.ObjectTypes enNext = ModLibrary.DynEnvTypeNext;
            ModLibrary.DynEnvDetails.Add(enNext, new ModDynEnvData(enNext) { InstanceBuilder = xBuilder, xContent = xContent });
            return enNext;
        }

        // DynamicEnvironmentCodex.ObjectTypes extensions

        public static bool IsSoGEnv(this DynamicEnvironmentCodex.ObjectTypes enType)
        {
            return Enum.IsDefined(typeof(DynamicEnvironmentCodex.ObjectTypes), enType);
        }

        public static bool IsModEnv(this DynamicEnvironmentCodex.ObjectTypes enType)
        {
            return enType >= ModLibrary.DynEnvTypeStart && enType < ModLibrary.DynEnvTypeNext;
        }

        //
        // Harmony Patches
        //

        /// <summary> Patches GetObjectInstance so that it can create instances of modded Dynamic Environments. </summary>
        private static bool GetObjectInstance_PrefixPatch(ref DynamicEnvironment __result, DynamicEnvironmentCodex.ObjectTypes enType, Level.WorldRegion enOverrideRegion)
        {
            if(!enType.IsModEnv())
            {
                return true; // Execute original methods
            }

            ModDynEnvData xData = ModLibrary.DynEnvDetails[enType];

            __result = xData.InstanceBuilder(xData.xContent);

            Console.WriteLine("Lmaooo" + (__result == null));

            Console.WriteLine("Lmaooo" + __result.GetType().ToString());

            __result.enType = enType;
            __result.bNetworkSynchEnabled = false;

            Console.WriteLine("Lmaooo" + (__result == null));

            if (__result.xRenderComponent != null)
            {
                __result.xRenderComponent.GetCurrentAnimation().Reset();
            }

            Console.WriteLine("Lmaooo" + (__result == null));

            return false; // Skip original method
        }
    }
}
