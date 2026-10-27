using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.Core;

namespace AutoTrader
{
    /// <summary>
    /// This interface allows to exchange the actual source that provides the data for the autotrading logic.
    /// Initially this was done to make unittesting possible but right now it has only one implementation.
    /// </summary>
    public interface ILogicConnector
    {
        bool IsCaravan { get; set; }
        bool IsBuying { get; set; }
        bool IsPort { get; set; }
        bool UseCargoHold { get; set; }

        void SetCurrentElementById(int itemId);
        void SetCurrentElementByName(string itemName);
        void SetCurrentElementByKey(string itemKey);
        object GetCurrentElement();
        void SetCurrentElement(object element);
        int GetInitialGold();
        int GetTroopWage();
        float GetCurrentWeight();
        float GetInventoryCapacity();
        int GetPlayerItemRosterSize();
        List<string> GetPlayerItemRosterNames();
        List<string> GetPlayerItemRosterKeys();
        int GetNumPartyMembers();
        int GetNumLivestockAnimals();
        int GetHerdSize();
        bool HasRoomInHerd();
        int GetMountSurplus();
        int GetWarMountCount();
        int GetMerchantItemRosterSize();
        List<string> GetLocks();
        bool IsItemLocked();
        bool IsItemTradeGood();
        int GetItemAmount();
        int GetItemAmountInPlayerRoster();
        string GetItemName();
        string GetItemId();
        float GetItemWeight();
        bool IsWeaponDesignEmpty();
        bool IsPackAnimal();
        bool IsItemGrain();
        bool IsItemHardwood();
        int GetPartyHardwoodIndex();
        float GetRosterElementWeight();
        bool InitInventory();
        bool InitBackgroundInventory();
        void DeferBackgroundInventory();
        void CancelBackgroundInventory();
        int GetMerchantGold();
        bool IsItemTierLowerThan(ItemObject.ItemTiers tier);

        bool IsItemFiltered(List<string> doneItems = null);
        void TransferItem();
        int GetProjectedProfit(int buyoutPrice);
        int GetItemPrice();
        float GetAveragePriceFallback();
        int GetCostOfRosterElement();
        float GetAveragePriceFactorItemCategory();

        /// Towns
        int GetTownListSize();
        string GetTownName(int townId);
        string GetVillageName(int villageId);
        int GetTownItemStock(int townId);
        int GetVillageItemStock(int villageId);
        bool IsTownInRange(int townId, out float actualDistance);
        bool IsCurrentTown(int townId);
        float GetTownItemPrice(int townId, bool isSelling);
        float GetCurrentTownPriceFactor();

        /// Villages
        int GetVillageListSize();
        bool IsVillageInRange(int villageId, out float actualDistance);
        bool IsCurrentVillage(int townId);
        float GetVillageItemPrice(int townId, bool isSelling);

        /// Helper Wrapper
        bool IsArmor();
        bool IsWeapon();
        bool IsHorse();
        bool IsMount();
        bool IsWarMount();
        bool IsNobleMount();
        bool IsConsumable();
        bool IsLivestock();

    }
}
