using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using System.Security.Policy;

namespace SoG.GrindScript
{
    public class ModCodex
    {
        public static class SoGType // Vanilla non-nested types
        {
            //public static readonly Type _ATemplate = Utils.GetGameType("SoG.");

            public static readonly Type ItemCategories = Utils.GetGameType("SoG.ItemCodex+ItemCategories");

            public static readonly Type ItemTypes = Utils.GetGameType("SoG.ItemCodex+ItemTypes");

            public static readonly Type EquipmentInfo = Utils.GetGameType("SoG.EquipmentInfo");

            static SoGType()
            {
                ItemCategories = Utils.GetGameType("SoG.ItemCodex+ItemCategories");
                ItemTypes = Utils.GetGameType("SoG.ItemCodex+ItemTypes");
                EquipmentInfo = Utils.GetGameType("SoG.EquipmentInfo");
            }

            
        }

        public class AttackStats
        {
            // Just holds types for now


        }

        

        public class ItemDescription : ConvertedObject
        {
            public static readonly ConstructorInfo _ctor;

            static ItemDescription()
            {
                _ctor = Utils.GetGameType("SoG.ItemDescription").GetConstructor(Type.EmptyTypes);
            }

            public ItemDescription(object originalObject) : base(originalObject)
            {
                // Some fields are the same for all items
            }

            public ItemDescription() : base(_ctor.Invoke(null)) 
            { 
                // Some fields are the same for all items
            }

            public int EnType
            {
                get => (int)_originalObject.enType;
                set => _originalObject.enType = Enum.ToObject(SoGType.ItemTypes, value);
            }

            public bool IsVanillaItem => Enum.IsDefined(SoGType.ItemTypes, EnType);
        }

        public class EquipmentInfo: ConvertedObject
        {
            public static readonly Type StatEnum;

            public static readonly Type SpecialEffect;

            public static readonly ConstructorInfo _ctor;

            public static readonly ConstructorInfo _ctorInit;

            static EquipmentInfo()
            {
                StatEnum = Utils.GetGameType("SoG.EquipmentInfo+StatEnum");
                SpecialEffect = Utils.GetGameType("SoG.EquipmentInfo+SpecialEffect");
                _ctor = Utils.GetGameType("SoG.EquipmentInfo").GetConstructor(Type.EmptyTypes);
                _ctorInit = Utils.GetGameType("SoG.EquipmentInfo").GetConstructor(new Type[] { typeof(string), SoGType.ItemTypes });
            }

            public EquipmentInfo(object originalObject) : base(originalObject) { }

            public EquipmentInfo(string sResourceName, int enItemType) : base(_ctorInit.Invoke(new object[] { sResourceName, Enum.ToObject(SoGType.ItemTypes, enItemType) })) { }
        }

        public class WeaponInfo: EquipmentInfo
        {
            public static readonly Type AutoAttackSpell;

            public static readonly Type WeaponCategory;

            new public static readonly ConstructorInfo _ctorInit;

            static WeaponInfo()
            {
                AutoAttackSpell = Utils.GetGameType("SoG.WeaponInfo+AutoAttackSpell");
                WeaponCategory = Utils.GetGameType("SoG.WeaponInfo+WeaponCategory");
                _ctorInit = Utils.GetGameType("SoG.WeaponInfo").GetConstructor(new Type[] { typeof(string), SoGType.ItemTypes, WeaponCategory, typeof(string) });
            }

            public WeaponInfo(object originalObject) : base(originalObject) { }

            public WeaponInfo(string sResourceName, int enItemType, int enWeaponCategory, string sPalette) : base(_ctorInit.Invoke(new object[] { sResourceName, Enum.ToObject(SoGType.ItemTypes, enItemType), Enum.ToObject(WeaponCategory, enWeaponCategory), sPalette })) { }


        }
    }
}
