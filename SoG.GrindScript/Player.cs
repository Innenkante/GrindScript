using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SoG.GrindScript
{
    
    public abstract class ConvertedObject
    {
        protected dynamic _originalType;

        protected ConvertedObject(object originalType)
        {
            _originalType = originalType;
        }
    }

    public class Inventory : ConvertedObject 
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

    public class Equipment : ConvertedObject
    {
        public Equipment(object originalType) : base(originalType)
        {
        }
    }

    public class Journal : ConvertedObject
    {
        public Journal(object originalType) : base(originalType)
        {
        }

    }

    public class Player : ConvertedObject
    {
        public Inventory Inventory;
        public Equipment Equipment;
        public Journal Journal;
        

        public Player(object originalType) : base(originalType)
        {
            Inventory = new Inventory(_originalType.xInventory);
            Equipment = new Equipment(_originalType.xEquipment);
            Journal = new Journal(_originalType.xJournalInfo);
        }

        /* Player Level Members */
        public ushort Level
        {
            get { return _originalType.xViewStats.iLevel; }
            set { _originalType.xViewStats.iLevel = value;  }
        }

        public ushort SilverSkillPoints
        {
            get { return _originalType.xViewStats.iSkillPointsSilver; }
            set { _originalType.xViewStats.iSkillPointsSilver = value; }
        }

        public ushort GoldSkillPoints
        {
            get { return _originalType.xViewStats.iSkillPointsGold; }
            set { _originalType.xViewStats.iSkillPointsGold = value; }
        }

        public ushort TalentPoints
        {
            get { return _originalType.xViewStats.iTalentPoints; }
            set { _originalType.xViewStats.iTalentPoints = value; }
        }

        public void SetSkillLevel(SpellTypes type, byte level)
        {
            var spellTypes = Utils.GetGameType("SoG.SpellCodex").GetDeclaredNestedType("SpellTypes");
            var spellGameType = Enum.Parse(spellTypes, type.ToString());

            Utils.GetGameType("SoG.PlayerViewStats")
                 .GetPublicInstanceOverloadedMethods("SetSkillLevel").Single()
                 .Invoke((object) _originalType.xViewStats, new[] { spellGameType, level });
        }

        /* Player Stat Members */
        public int Health
        {
            get { return _originalType.xEntity.xBaseStats.iHP; }
            set { _originalType.xEntity.BaseStats.iHP = value; }
        }

        public int EP
        {
            get { return _originalType.xEntity.xBaseStats.iEP; }
            set { _originalType.xEntity.BaseStats.iEP = value; }
        }

        public string Name
        {
            get { return _originalType.sNetworkNickname; }
        }
    }
}
