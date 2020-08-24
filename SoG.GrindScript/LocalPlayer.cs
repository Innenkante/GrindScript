using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoG.GrindScript
{
    
    public abstract class ConvertedType
    {
        protected dynamic _originalType;

        protected ConvertedType(object originalType)
        {
            _originalType = originalType;
        }
    }

    public class Inventory : ConvertedType 
    {
        public Inventory(object orignalType) : base(orignalType)
        {
        }

        public void AddItem(ItemTypes type, int amount)
            => _originalType.AddItem(type, amount);

        public int GetItemAmount(ItemTypes type)
            => _originalType.GetAmount(type);

        public void SetItemAmount(ItemTypes type, int amount)
            => _originalType.SetItemAmount(type, amount);

        public void AddMoney(int amount)
            => _originalType.AddMoney(amount);

        public int GetMoneyAmount()
            => _originalType.GetMoney();

        public void SetMoneyAmount(int amount)
            => _originalType.SetMoney(amount);

    }

    public class Equipment : ConvertedType
    {
        public Equipment(object originalType) : base(originalType)
        {
        }
    }

    public class Journal : ConvertedType
    {
        public Journal(object originalType) : base(originalType)
        {
        }

    }


    public class LocalPlayer : ConvertedType
    {
        public Inventory Inventory;
        public Equipment Equipment;
        public Journal Journal;
        

        public LocalPlayer(object originalType) : base(originalType)
        {
            Inventory = new Inventory(_originalType.xInventory);
            Equipment = new Equipment(_originalType.xEquipment);
            Journal = new Journal(_originalType.xJournalInfo);
        }

        public int GetHealth()
            => _originalType.xEntity.xBaseStats.iHP;

        public int GetEP()
            => _originalType.xEntity.xBaseStats.iEP;

        public int SetHealth(int health)
            => _originalType.xEntity.BaseStats.iHP = health;

        public int SetEP(int ep)
            => _originalType.xEntity.BaseStats.iEP = ep;

        public string GetName()
            => _originalType.sNetworkNickname;

    }
}
