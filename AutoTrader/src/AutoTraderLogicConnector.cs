using AutoTrader.Warsails;
using Helpers;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Inventory;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Roster;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.CampaignSystem.ViewModelCollection;
using TaleWorlds.CampaignSystem.ViewModelCollection.Inventory;
using TaleWorlds.Core;
using TaleWorlds.Core.ViewModelCollection.Information;
using TaleWorlds.Localization;

namespace AutoTrader
{
    class AutoTraderLogicConnector: ILogicConnector
    {

        public bool _isCaravan = false;
        public bool _isBuying = false;
        public bool _isPort = false;

        // True to trade against the fleet's hold from the Cargo Holds mod
        public bool _useCargoHold = false;

        private ItemRosterElement _currentItemRosterElement;
        private InventoryLogic _inventoryLogic;
        // Sale from the roster the trade screen doesn't show, settled when that screen closes
        private InventoryLogic _pendingBackgroundLogic;
        // Gold the merchant owes for the pending trade, held back from the screen's merchant gold
        private int _pendingPayout;
        // Trade total and label last written into the trade screen, so they are only rebuilt when something changed
        private int _shownTradeTotal = int.MinValue;
        private string _shownTradeLbl;
        private bool _pendingBackgroundCancelled;
        private int _inventoryDisplayRefreshTicks;

        bool ILogicConnector.IsCaravan { get { return _isCaravan; } set { _isCaravan = value; } }
        bool ILogicConnector.IsBuying { get { return _isBuying; } set { _isBuying = value; } }
        bool ILogicConnector.IsPort { get { return _isPort; } set { _isPort = value; } }
        bool ILogicConnector.UseCargoHold { get { return _useCargoHold; } set { _useCargoHold = value; } }

        public enum MerchantType
        {
            Town,
            Village,
            Caravan
        }

        public void SetCurrentElementById(int itemId)
        {
            _currentItemRosterElement = GetItemRoster()[itemId];
            AutoTraderHelpers.PrintDebugMessage(" - Current Item: " + _currentItemRosterElement.EquipmentElement.Item.Name.ToString());
        }

        public int GetInitialGold()
        {
            // TODO: Use "MobileParty" instead since 1.3?
            AutoTraderHelpers.PrintDebugMessage(" - InitialGold: " + PartyBase.MainParty.Owner.Gold.ToString());
            return PartyBase.MainParty.Owner.Gold;
        }
        public int GetTroopWage()
        {
            // ToDo: Whole daily wage
            AutoTraderHelpers.PrintDebugMessage(" - TroopWage: " + PartyBase.MainParty.MobileParty.TotalWage.ToString());
            return PartyBase.MainParty.MobileParty.TotalWage;
        }
        public float GetCurrentWeight()
        {
            // In a port the fleet carries the cargo
            if (_isPort && WarsailsDetector.IsWarsailsDLCAvailable())
            {
                float fleetWeight = WarsailsHelper.GetFleetTotalWeightCarried(MobileParty.MainParty, true);
                
                if (fleetWeight > 0)
                {
                    AutoTraderHelpers.PrintDebugMessage(" - Using FleetTotalWeightCarried: " + fleetWeight.ToString());
                    return fleetWeight;
                }
            }

            // Fall back to regular weight calculation
            AutoTraderHelpers.PrintDebugMessage(" - CurrentWeight: " + MobileParty.MainParty.TotalWeightCarried.ToString());
            return MobileParty.MainParty.TotalWeightCarried;
        }
        public float GetInventoryCapacity()
        {
            // In a port the fleet carries the cargo
            if (_isPort && WarsailsDetector.IsWarsailsDLCAvailable())
            {
                int fleetCapacity = WarsailsHelper.GetFleetCargoCapacity(MobileParty.MainParty);

                if (fleetCapacity > 0)
                {
                    AutoTraderHelpers.PrintDebugMessage(" - Using FleetCargoCapacity: " + fleetCapacity.ToString());
                    return fleetCapacity;
                }
            }

            // Use regular inventory capacity
            AutoTraderHelpers.PrintDebugMessage(" - InventoryCapacity: " + PartyBase.MainParty.MobileParty.InventoryCapacity.ToString());
            return PartyBase.MainParty.MobileParty.InventoryCapacity;
        }
        /// <summary>
        /// The roster the player trades from: the fleet's cargo hold when trading through Cargo Holds, otherwise the party inventory.
        /// </summary>
        private ItemRoster GetPlayerRoster()
        {
            if (_useCargoHold)
            {
                var hold = CargoHoldsCompat.GetPlayerFleetHold();
                if (hold != null)
                    return hold;
            }
            return PartyBase.MainParty.ItemRoster;
        }
        public int GetPlayerItemRosterSize()
        {
            var result = GetPlayerRoster().Count;
            AutoTraderHelpers.PrintDebugMessage(" - PlayerItemRosterSize: " + result.ToString());
            return result;
        }
        public int GetNumPartyMembers()
        {
            // TODO: Use "MobileParty" instead since 1.3?
            AutoTraderHelpers.PrintDebugMessage(" - NumPartyMembers: " + PartyBase.MainParty.NumberOfAllMembers.ToString());
            return PartyBase.MainParty.NumberOfAllMembers;
        }
        public int GetNumLivestockAnimals()
        {
            int totalLivestock = 0;
            var itemRoster = PartyBase.MainParty?.ItemRoster;

            if (itemRoster != null)
            {
                foreach (var element in itemRoster)
                {
                    if (element.EquipmentElement.Item != null && AutoTraderHelpers.IsLivestock(element.EquipmentElement.Item))
                    {
                        totalLivestock += element.Amount;
                    }
                }
            }

            AutoTraderHelpers.PrintDebugMessage(" - NumLivestockAnimals (Dynamic): " + totalLivestock.ToString());
            return totalLivestock;
        }
        /// <summary>
        /// Herd size as the game calculates it for the herding speed penalty:
        /// pack animals + livestock + mounts beyond the number of men without a horse.
        /// </summary>
        public int GetHerdSize()
        {
            var party = PartyBase.MainParty;
            var roster = party.ItemRoster;
            int spareMounts = Math.Max(0, roster.NumberOfMounts - party.NumberOfMenWithoutHorse);
            int result = roster.NumberOfPackAnimals + roster.NumberOfLivestockAnimals + spareMounts;
            AutoTraderHelpers.PrintDebugMessage(" - HerdSize: " + result.ToString());
            return result;
        }
        /// <summary>
        /// Riding horses in the party inventory minus the men on foot who could ride them.
        /// Negative means some men on foot have no horse and the party misses the speed bonus.
        /// </summary>
        public int GetMountSurplus()
        {
            var party = PartyBase.MainParty;
            int result = party.ItemRoster.NumberOfMounts - party.NumberOfMenWithoutHorse;
            AutoTraderHelpers.PrintDebugMessage(" - MountSurplus: " + result.ToString());
            return result;
        }
        /// <summary>
        /// War mounts in the party inventory (needed to upgrade troops into mounted ones).
        /// </summary>
        public int GetWarMountCount()
        {
            int result = 0;
            foreach (ItemRosterElement element in PartyBase.MainParty.ItemRoster)
            {
                if (AutoTraderHelpers.IsWarMount(element.EquipmentElement.Item))
                    result += element.Amount;
            }
            AutoTraderHelpers.PrintDebugMessage(" - WarMountCount: " + result.ToString());
            return result;
        }
        public bool HasRoomInHerd()
        {
            var item = _currentItemRosterElement.EquipmentElement.Item;

            // Animals bought into the fleet's cargo hold stay on the ships and don't join the herd
            if (_useCargoHold)
                return true;

            // Only items with a HorseComponent join the herd
            if (item == null || !item.HasHorseComponent)
                return true;

            // A mount only adds to the herd once every man without a horse already has one
            var party = PartyBase.MainParty;
            if (!item.HorseComponent.IsPackAnimal && item.HorseComponent.IsMount
                && party.ItemRoster.NumberOfMounts < party.NumberOfMenWithoutHorse)
                return true;

            // Trade buying stops at the set % of the herding limit
            int herdLimit = GetNumPartyMembers() * AutoTraderConfig.MaxHerdTradePercentValue / 100;
            var result = GetHerdSize() < herdLimit;
            AutoTraderHelpers.PrintDebugMessage(" - HasRoomInHerd: " + result.ToString() + " (trade herd limit " + herdLimit.ToString() + ")");
            return result;
        }
        public int GetMerchantItemRosterSize()
        {
            if (_isCaravan)
            {
                AutoTraderHelpers.PrintDebugMessage(" - MerchantItemRosterSize (Caravan): " + MobileParty.ConversationParty.ItemRoster.Count.ToString());
                return MobileParty.ConversationParty.ItemRoster.Count;
            }
            AutoTraderHelpers.PrintDebugMessage(" - MerchantItemRosterSize (Town): " + Settlement.CurrentSettlement.ItemRoster.Count.ToString());
            return Settlement.CurrentSettlement.ItemRoster.Count;
        }
        public List<string> GetLocks()
        {
            var locksEnumerable = Campaign.Current.GetCampaignBehavior<IViewDataTracker>().GetInventoryLocks();
            if (locksEnumerable != null)
            {
                return locksEnumerable.ToList<string>();
            }
            return new List<string>();
        }

        public bool IsItemLocked()
        {
            var locks = GetLocks();
            var itemStringId = GetElementKey(_currentItemRosterElement.EquipmentElement);
            AutoTraderHelpers.PrintDebugMessage(" - IsLocked: " + locks.Contains(itemStringId).ToString());
            return locks.Contains(itemStringId);
        }

        /// <summary>
        /// Tells apart items that share a name, like a plain and a rusty sword. Same as the game's inventory lock id.
        /// </summary>
        private static string GetElementKey(EquipmentElement equipmentElement)
        {
            var key = equipmentElement.Item.StringId;
            if (equipmentElement.ItemModifier != null)
            {
                key += equipmentElement.ItemModifier.StringId;
            }
            return key;
        }
        public bool IsItemTradeGood()
        {
            AutoTraderHelpers.PrintDebugMessage(" - IsTradeGood: " + _currentItemRosterElement.EquipmentElement.Item.IsTradeGood.ToString());
            return _currentItemRosterElement.EquipmentElement.Item.IsTradeGood;
        }
        public int GetItemAmount()
        {
            try
            {
                AutoTraderHelpers.PrintDebugMessage(" - ItemAmount: " + _currentItemRosterElement.Amount.ToString());
                return _currentItemRosterElement.Amount;
            }
            catch (Exception e)
            {
                AutoTraderHelpers.PrintMessage("GetItemAmount crashed: " + e.ToString());
                return 0;
            }
        }
        public string GetItemName()
        {
            AutoTraderHelpers.PrintDebugMessage(" - ItemName: " + _currentItemRosterElement.EquipmentElement.Item.Name.ToString());
            return _currentItemRosterElement.EquipmentElement.Item.Name.ToString();
        }
        public string GetItemId()
        {
            AutoTraderHelpers.PrintDebugMessage(" - ItemId: " + _currentItemRosterElement.EquipmentElement.Item.StringId);
            return _currentItemRosterElement.EquipmentElement.Item.StringId;
        }
        public float GetItemWeight()
        {
            AutoTraderHelpers.PrintDebugMessage(" - ItemWeight: " + _currentItemRosterElement.EquipmentElement.Item.Weight.ToString());
            return _currentItemRosterElement.EquipmentElement.Item.Weight;
        }

        public bool IsWeaponDesignEmpty()
        {
            var result = _currentItemRosterElement.EquipmentElement.Item.WeaponDesign == null;
            AutoTraderHelpers.PrintDebugMessage(" - IsWeaponDesignEmpty: " + result.ToString());
            return result;
        }
        public bool IsPackAnimal()
        {
            var result = _currentItemRosterElement.EquipmentElement.Item.HorseComponent.IsPackAnimal;
            AutoTraderHelpers.PrintDebugMessage(" - IsPackAnimal: " + result.ToString());
            return result;
        }
        public bool IsItemGrain()
        {
            var result = _currentItemRosterElement.EquipmentElement.Item == DefaultItems.Grain;
            AutoTraderHelpers.PrintDebugMessage(" - IsItemGrain: " + result.ToString());
            return result;
        }
        public bool IsItemHardwood()
        {
            var result = _currentItemRosterElement.EquipmentElement.Item == DefaultItems.HardWood;
            AutoTraderHelpers.PrintDebugMessage(" - IsItemHardwood: " + result.ToString());
            return result;
        }
        public int GetPartyHardwoodIndex()
        {
            var result = GetPlayerRoster().FindIndexOfItem(DefaultItems.HardWood);
            AutoTraderHelpers.PrintDebugMessage(" - HardwoodIndex: " + result.ToString());
            return result;
        }
        public float GetRosterElementWeight()
        {
            var result = _currentItemRosterElement.GetRosterElementWeight();
            if (!_isBuying)
                AutoTraderHelpers.PrintDebugMessage("GetRosterElementWeigth: Not buying!");
            AutoTraderHelpers.PrintDebugMessage(" - RosterElementWeight: " + result.ToString());
            return result;
        }
        private MerchantType GetMerchantType()
        {
            MerchantType merchantType;
            if (_isCaravan)
            {
                merchantType = MerchantType.Caravan;

                // Make sure its opened through conversation
                if (MobileParty.ConversationParty == null)
                {
                    AutoTraderHelpers.PrintDebugMessage("Caravan trading but not through a conversation!");
                }
            }
            else
                merchantType = Settlement.CurrentSettlement.IsTown ? MerchantType.Town : MerchantType.Village;
            return merchantType;
        }

        public bool InitInventory()
        {
            var merchantType = GetMerchantType();
            if (_useCargoHold)
            {
                // Trade against the fleet's cargo hold instead of the party inventory
                if (merchantType != MerchantType.Town || !CargoHoldsCompat.OpenFleetTrade(Settlement.CurrentSettlement))
                    return false;
                AutoTraderHelpers.PrintDebugMessage(" - Trading with the Cargo Holds fleet hold");
            }
            else if (merchantType == MerchantType.Town)
            {
                InventoryScreenHelper.OpenScreenAsTrade(Settlement.CurrentSettlement.ItemRoster, Settlement.CurrentSettlement.Town,
                    InventoryScreenHelper.InventoryCategoryType.None, null);
            }
            else if (merchantType == MerchantType.Village)
            {
                InventoryScreenHelper.OpenScreenAsTrade(Settlement.CurrentSettlement.ItemRoster, Settlement.CurrentSettlement.Village, InventoryScreenHelper.InventoryCategoryType.None, null);
            }
            else if (merchantType == MerchantType.Caravan)
            {
                InventoryScreenHelper.OpenTradeWithCaravanOrAlleyParty(MobileParty.ConversationParty, InventoryScreenHelper.InventoryCategoryType.None);
            }
            else
            {
                return false;
            }

            if (!TryPrepareInventoryLogic())
                return false;

            AttachPendingBackgroundTrade();
            return true;
        }

        /// <summary>
        /// Trades the player roster (cargo hold or party inventory, per UseCargoHold) with the town without opening a trade screen.
        /// Used to sell from the roster the opened trade screen does not show. DeferBackgroundInventory hands it to the next trade screen.
        /// </summary>
        public bool InitBackgroundInventory()
        {
            // Both rosters are only reachable while the fleet is docked at this town
            if (GetMerchantType() != MerchantType.Town || !CargoHoldsCompat.IsAvailable
                || !CargoHoldsCompat.IsPlayerFleetDockedAt(Settlement.CurrentSettlement))
                return false;

            ItemRoster playerRoster = PartyBase.MainParty.ItemRoster;
            if (_useCargoHold)
            {
                playerRoster = CargoHoldsCompat.GetPlayerFleetHold();
                if (playerRoster == null)
                    return false;
            }

            Town town = Settlement.CurrentSettlement.Town;
            var logic = new InventoryLogic(town.Owner);
            // Same setup as InventoryScreenHelper.OpenScreenAsTrade, with the chosen roster on the player side
            logic.Initialize(Settlement.CurrentSettlement.ItemRoster, playerRoster, PartyBase.MainParty.MemberRoster, true, true,
                CharacterObject.PlayerCharacter, InventoryScreenHelper.InventoryCategoryType.None, town.MarketData, false,
                InventoryScreenHelper.InventoryMode.Trade, null, null, null);
            logic.SetInventoryListener(new TownInventoryListener(town));
            logic.TotalAmountChange = _ => { };
            _inventoryLogic = logic;
            return true;
        }

        /// <summary>
        /// Keeps the background trade pending until the next trade screen closes: Done pays it out, Cancel puts the items back.
        /// </summary>
        public void DeferBackgroundInventory()
        {
            CancelBackgroundInventory();
            _pendingBackgroundLogic = _inventoryLogic;
            _pendingBackgroundCancelled = false;
            _inventoryLogic = null;
        }

        /// <summary>
        /// Undoes a background trade that was never paid out.
        /// </summary>
        public void CancelBackgroundInventory()
        {
            CompletePendingBackgroundTrade(true);
        }

        /// <summary>
        /// Ties the pending background trade to the trade screen that just opened.
        /// </summary>
        private void AttachPendingBackgroundTrade()
        {
            var state = InventoryScreenHelper.GetActiveInventoryState();
            if (_pendingBackgroundLogic == null || state == null || _inventoryLogic == null)
                return;

            // Cancel resets the screen's trade with fromCancel set, then closes it like Done
            _inventoryLogic.AfterReset += (logic, fromCancel) =>
            {
                if (fromCancel)
                    _pendingBackgroundCancelled = true;
            };

            // Runs when the screen closes, after its own trade went through (also after Cancel)
            Action previous = state.DoneLogicExtrasDelegate;
            state.DoneLogicExtrasDelegate = () =>
            {
                previous?.Invoke();
                CompletePendingBackgroundTrade(_pendingBackgroundCancelled);
            };

            // Show the merchant's gold as if the pending sale were already paid, like a normal trade.
            // The screen's own trade can then only spend what is left, and pays through the same listener.
            int pendingPayout = Math.Max(0, -_pendingBackgroundLogic.TotalAmount);
            _pendingPayout = 0;
            if (pendingPayout > 0 && _inventoryLogic.InventoryListener != null)
            {
                _inventoryLogic.SetInventoryListener(new OffsetInventoryListener(_inventoryLogic.InventoryListener, pendingPayout));
                _pendingPayout = pendingPayout;
            }
            _shownTradeTotal = int.MinValue;

            // List the pending trade in the transaction tooltip (hover over Done)
            _tooltipPendingLogic = _pendingBackgroundLogic;
            _tooltipVisibleName = GetRosterName(_useCargoHold);
            _tooltipPendingName = GetRosterName(!_useCargoHold);
            HookInventoryTooltip();
        }

        private static string GetRosterName(bool cargoHold)
        {
            return cargoHold ? new TextObject("{=ATCargoHold}Cargo hold", null).ToString()
                : new TextObject("{=ATPartyInventory}Party inventory", null).ToString();
        }

        // The game's transaction tooltip, wrapped so it also lists the pending trade
        private static Action<PropertyBasedTooltipVM, object[]> _originalInventoryTooltip;
        private static InventoryLogic _tooltipPendingLogic;
        private static string _tooltipVisibleName;
        private static string _tooltipPendingName;

        /// <summary>
        /// Replaces the tooltip registered for InventoryLogic with one that calls it and then adds the pending trade.
        /// Checked each time, since the game may register its own again (for example after loading a save).
        /// </summary>
        private static void HookInventoryTooltip()
        {
            try
            {
                FieldInfo registryField = typeof(TaleWorlds.Library.InformationManager).GetField("_registeredTypes", BindingFlags.NonPublic | BindingFlags.Static);
                IDictionary registry = registryField?.GetValue(null) as IDictionary;
                if (registry == null || !registry.Contains(typeof(InventoryLogic)))
                {
                    AutoTraderHelpers.PrintDebugMessage(" - no trade tooltip registered to extend");
                    return;
                }

                object entry = registry[typeof(InventoryLogic)];
                Delegate current = entry.GetType().GetField("OnRefreshData")?.GetValue(entry) as Delegate;
                string movieName = entry.GetType().GetField("MovieName")?.GetValue(entry) as string;
                Action<PropertyBasedTooltipVM, object[]> ours = RefreshInventoryTooltip;
                if (current == null || movieName == null || current.Method == ours.Method)
                    return;

                _originalInventoryTooltip = current as Action<PropertyBasedTooltipVM, object[]> ?? ((tooltip, args) => current.DynamicInvoke(tooltip, args));
                TaleWorlds.Library.InformationManager.RegisterTooltip<InventoryLogic, PropertyBasedTooltipVM>(ours, movieName);
            }
            catch (Exception e)
            {
                AutoTraderHelpers.PrintDebugMessage("Could not extend the trade tooltip: " + e.Message);
            }
        }

        private static void RefreshInventoryTooltip(PropertyBasedTooltipVM tooltip, object[] args)
        {
            InventoryLogic pending = _tooltipPendingLogic;
            if (pending != null)
                tooltip.AddProperty("", _tooltipVisibleName, 0, TooltipProperty.TooltipPropertyFlags.Title);

            _originalInventoryTooltip?.Invoke(tooltip, args);
            if (pending == null)
                return;

            tooltip.AddProperty("", "", 0, TooltipProperty.TooltipPropertyFlags.DefaultSeperator);
            tooltip.AddProperty("", _tooltipPendingName, 0, TooltipProperty.TooltipPropertyFlags.Title);
            foreach (var sold in pending.GetSoldItems())
            {
                tooltip.AddColoredProperty(sold.Item1.EquipmentElement.GetModifiedItemName().ToString() + " x " + sold.Item1.Amount.ToString(),
                    "+" + Math.Abs(sold.Item2).ToString(), UIColors.PositiveIndicator, 0, TooltipProperty.TooltipPropertyFlags.None);
            }
            foreach (var bought in pending.GetBoughtItems())
            {
                tooltip.AddColoredProperty(bought.Item1.EquipmentElement.GetModifiedItemName().ToString() + " x " + bought.Item1.Amount.ToString(),
                    "-" + Math.Abs(bought.Item2).ToString(), UIColors.NegativeIndicator, 0, TooltipProperty.TooltipPropertyFlags.None);
            }
        }

        /// <summary>
        /// Merchant gold source that holds back gold already owed for the pending trade.
        /// </summary>
        private class OffsetInventoryListener : InventoryListener
        {
            private readonly InventoryListener _inner;
            private readonly int _offset;

            public OffsetInventoryListener(InventoryListener inner, int offset)
            {
                _inner = inner;
                _offset = offset;
            }

            public override int GetGold()
            {
                return _inner.GetGold() - _offset;
            }

            public override void SetGold(int gold)
            {
                _inner.SetGold(gold + _offset);
            }

            public override PartyBase GetOppositeParty()
            {
                return _inner.GetOppositeParty();
            }

            public override TextObject GetTraderName()
            {
                return _inner.GetTraderName();
            }

            public override void OnTransaction()
            {
                _inner.OnTransaction();
            }
        }

        private void CompletePendingBackgroundTrade(bool cancel)
        {
            InventoryLogic logic = _pendingBackgroundLogic;
            _pendingBackgroundLogic = null;
            _pendingBackgroundCancelled = false;
            _tooltipPendingLogic = null;
            if (logic == null)
                return;

            bool committed = false;
            if (!cancel)
            {
                committed = logic.DoneLogic();
            }
            if (!committed)
            {
                // Put the items back, since nothing was paid for them
                logic.Reset(true);
            }
            AutoTraderHelpers.PrintDebugMessage(" - Sale from the other roster " + (committed ? "paid out" : "undone"));
        }

        /// <summary>
        /// Gold source for a background trade with a town, like the game's own merchant listener.
        /// </summary>
        private class TownInventoryListener : InventoryListener
        {
            private readonly Town _town;

            public TownInventoryListener(Town town)
            {
                _town = town;
            }

            public override int GetGold()
            {
                return _town.Gold;
            }

            public override void SetGold(int gold)
            {
                _town.ChangeGold(gold - _town.Gold);
            }

            public override PartyBase GetOppositeParty()
            {
                return _town.Owner;
            }

            public override TextObject GetTraderName()
            {
                return _town.Owner.Name;
            }

            public override void OnTransaction()
            {
            }
        }

        /// <summary>
        /// In 1.4.6, TransactionDebt invokes TotalAmountChange without a null-check.
        /// Autotrading runs before the trade UI wires its handler, so we provide a temporary no-op.
        /// </summary>
        private bool TryPrepareInventoryLogic()
        {
            _inventoryLogic = InventoryScreenHelper.GetActiveInventoryState()?.InventoryLogic;
            if (_inventoryLogic == null)
            {
                AutoTraderHelpers.PrintDebugMessage("Failed to get active inventory logic!");
                return false;
            }

            if (_inventoryLogic.TotalAmountChange == null)
            {
                _inventoryLogic.TotalAmountChange = _ => { };
            }
            return true;
        }

        internal void BeginInventoryDisplayRefresh()
        {
            _inventoryDisplayRefreshTicks = 15;
        }

        internal void TickInventoryDisplayRefresh()
        {
            // A pending sale whose trade screen closed without settling it is undone
            if (_pendingBackgroundLogic != null && InventoryScreenHelper.GetActiveInventoryState() == null)
            {
                CancelBackgroundInventory();
            }

            UpdatePendingTradeDisplay();

            if (_inventoryDisplayRefreshTicks <= 0)
            {
                return;
            }

            if (InventoryScreenHelper.GetActiveInventoryState() == null)
            {
                _inventoryDisplayRefreshTicks = 0;
                return;
            }

            _inventoryLogic = InventoryScreenHelper.GetActiveInventoryState()?.InventoryLogic ?? _inventoryLogic;
            _inventoryLogic?.TotalAmountChange?.Invoke(_inventoryLogic.TotalAmount);
            _inventoryDisplayRefreshTicks--;
            UpdatePendingTradeDisplay();
        }

        /// <summary>
        /// The trade screen only counts its own trade in "You will get/pay X" and in the player's gold.
        /// Adds the pending trade from the other roster, the way SPInventoryVM.OnTotalAmountChange builds them.
        /// </summary>
        private void UpdatePendingTradeDisplay()
        {
            if (_pendingBackgroundLogic == null)
                return;
            InventoryLogic logic = InventoryScreenHelper.GetActiveInventoryState()?.InventoryLogic;
            SPInventoryVM vm = GetInventoryVM();
            if (logic == null || vm == null)
                return;

            // Negative: the player receives gold
            int total = logic.TotalAmount + _pendingBackgroundLogic.TotalAmount;
            int playerGold = Hero.MainHero.Gold - total;
            if (total == _shownTradeTotal && vm.TradeLbl == _shownTradeLbl && vm.RightInventoryOwnerGold == playerGold)
                return;

            // The merchant can't pay out more gold than it has
            int merchantGold = (logic.InventoryListener != null ? logic.InventoryListener.GetGold() : 0) + _pendingPayout;
            MBTextManager.SetTextVariable("PAY_OR_GET", total < 0 ? 1 : 0);
            MBTextManager.SetTextVariable("TRADE_AMOUNT", Math.Abs(Math.Min(-total, merchantGold)));
            vm.TradeLbl = total != 0 ? GameTexts.FindText("str_inventory_trade_label", null).ToString() : "";
            vm.RightInventoryOwnerGold = playerGold;
            _shownTradeTotal = total;
            _shownTradeLbl = vm.TradeLbl;
        }

        // Field of the inventory screen that holds its view model, looked up once per screen type
        private static Type _inventoryScreenType;
        private static FieldInfo _inventoryVMField;

        private static SPInventoryVM GetInventoryVM()
        {
            var screen = TaleWorlds.ScreenSystem.ScreenManager.TopScreen;
            if (screen == null)
                return null;
            Type type = screen.GetType();
            if (type != _inventoryScreenType)
            {
                _inventoryScreenType = type;
                _inventoryVMField = type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                    .FirstOrDefault(f => typeof(SPInventoryVM).IsAssignableFrom(f.FieldType));
            }
            return _inventoryVMField?.GetValue(screen) as SPInventoryVM;
        }

        public void RefreshUI()
        {
            try
            {
                var topScreen = TaleWorlds.ScreenSystem.ScreenManager.TopScreen;
                if (topScreen != null)
                {
                    var vmProperty = topScreen.GetType().GetProperty("DataSource")
                        ?? topScreen.GetType().GetProperty("ViewModel");
                    var vmField = topScreen.GetType().GetField("_dataSource", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);

                    object vm = vmProperty?.GetValue(topScreen) ?? vmField?.GetValue(topScreen);
                    if (vm != null)
                    {
                        var refreshMethod = vm.GetType().GetMethod("RefreshInformation", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance)
                            ?? vm.GetType().GetMethod("RefreshValues", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);

                        refreshMethod?.Invoke(vm, null);
                    }
                }
            }
            catch
            {
                // Safe fallback if not in inventory screen context
            }
        }

        public int GetMerchantGold()
        {
            var result = 0;
            var merchantType = GetMerchantType();
            switch (merchantType)
            {
                case MerchantType.Town:
                    result = Settlement.CurrentSettlement.Town.Gold;
                    break;
                case MerchantType.Village:
                    result = Settlement.CurrentSettlement.Village.Gold;
                    break;
                case MerchantType.Caravan:
                    result = MobileParty.ConversationParty.PartyTradeGold;
                    break;
            }
            AutoTraderHelpers.PrintDebugMessage(" - MerchantGold: " + result.ToString());
            return result;
        }

        private ItemRoster GetItemRoster()
        {
            if (_isBuying)
            {
                if (GetMerchantType() == MerchantType.Caravan)
                    return MobileParty.ConversationParty.ItemRoster;
                return Settlement.CurrentSettlement.ItemRoster;
                
            } else
                return GetPlayerRoster();
        }

        public bool IsItemTierLowerThan(ItemObject.ItemTiers tier)
        {
            var result = _currentItemRosterElement.EquipmentElement.Item.Tier < tier;
            AutoTraderHelpers.PrintDebugMessage(" - IsItemTierLowerThan: " + result.ToString());
            return result;
        }

        public bool IsItemFiltered(List<string> doneItems=null)
        {
            var itemRosterElement = _currentItemRosterElement;
            ItemObject itemObject = itemRosterElement.EquipmentElement.Item;

            // Filter by amount
            if (_currentItemRosterElement.Amount <=0)
            {
                AutoTraderHelpers.PrintDebugMessage(" - filtered because out of stock");
                return true;
            }
            // Filter by lock (locks only protect our own items from being sold)
            if (!_isBuying && IsItemLocked())
                return true;

            // Check if already bought / sold
            if (doneItems != null && doneItems.Exists(x => x == itemRosterElement.EquipmentElement.Item.Name.ToString()))
                return true;

            // Riding horses have their own buy/sell toggles
            if (AutoTraderHelpers.IsRidingHorse(itemObject))
                return !(_isBuying ? AutoTraderConfig.BuyMountsValue : AutoTraderConfig.SellMountsValue);

            // Exclude pack animals when buying for now
            if (AutoTraderHelpers.IsHorse(itemObject) && _isBuying)
                return true;

            // Filter by type
            if (!_isBuying && AutoTraderHelpers.IsSmithingMaterial(itemObject))
            {
                AutoTraderHelpers.PrintDebugMessage(" - is smithing material");
                return AutoTraderConfig.SellSmithingValue ? false : true;
            }   
            if (AutoTraderHelpers.IsHorse(itemObject) && !(_isBuying ? AutoTraderConfig.BuyHorsesValue : AutoTraderConfig.SellHorsesValue))
                return true;
            if (AutoTraderHelpers.IsArmor(itemObject) && !(_isBuying ? AutoTraderConfig.BuyArmorValue : AutoTraderConfig.SellArmorValue))
                return true;
            if (AutoTraderHelpers.IsWeapon(itemObject) && !(_isBuying ? AutoTraderConfig.BuyWeaponsValue : AutoTraderConfig.SellWeaponsValue))
                return true;
            if (AutoTraderHelpers.IsLivestock(itemObject) && !(_isBuying ? AutoTraderConfig.BuyLivestockValue : AutoTraderConfig.SellLivestockValue))
                return true;
            if (AutoTraderHelpers.IsTradeGood(itemObject) && !(_isBuying ? AutoTraderConfig.BuyGoodsValue : AutoTraderConfig.SellGoodsValue))
            {
                if (!AutoTraderHelpers.IsConsumable(itemObject))
                    return true;
            }
            if (AutoTraderHelpers.IsConsumable(itemObject) && !(_isBuying ? AutoTraderConfig.BuyConsumablesValue : AutoTraderConfig.SellConsumablesValue))
                return true;

            return false;
        }

        public void TransferItem()
        {
            // Generate command
            TransferCommand transferCommand = TransferCommand.Transfer(1,
                _isBuying ? InventoryLogic.InventorySide.OtherInventory : InventoryLogic.InventorySide.PlayerInventory,
                _isBuying ? InventoryLogic.InventorySide.PlayerInventory : InventoryLogic.InventorySide.OtherInventory,
                _currentItemRosterElement, EquipmentIndex.None, EquipmentIndex.None, CharacterObject.PlayerCharacter);
            _inventoryLogic.AddTransferCommand(transferCommand);
            AutoTraderHelpers.PrintDebugMessage(" - Transfer of item " + GetItemName() + " complete! (" + (_isBuying? "Buy" : "Sell") + ")");
        }

        public int GetProjectedProfit(int buyoutPrice)
        {
            IPlayerTradeBehavior campaignBehavior = Campaign.Current.GetCampaignBehavior<IPlayerTradeBehavior>();
            var result = campaignBehavior.GetProjectedProfit(_currentItemRosterElement, buyoutPrice);
            AutoTraderHelpers.PrintDebugMessage(" - ProjectedProfit: " + result.ToString());
            return result;
        }

        public int GetItemPrice()
        {
            var result = _inventoryLogic.GetItemPrice(_currentItemRosterElement.EquipmentElement, _isBuying);
            AutoTraderHelpers.PrintDebugMessage(" - ItemPrice: " + result.ToString());
            return result;
        }

        public float GetAveragePriceFallback()
        {
            return _currentItemRosterElement.EquipmentElement.Item.Value;
        }

        public int GetCostOfRosterElement()
        {
            var result = _inventoryLogic.GetCostOfItemRosterElement(_currentItemRosterElement, _isBuying ? InventoryLogic.InventorySide.OtherInventory : InventoryLogic.InventorySide.PlayerInventory);
            AutoTraderHelpers.PrintDebugMessage(" - CostOfElement: " + result.ToString());
            return result;
        }
        public float GetAveragePriceFactorItemCategory()
        {
            var result = _inventoryLogic.GetAveragePriceFactorItemCategory(_currentItemRosterElement.EquipmentElement.Item.ItemCategory);
            AutoTraderHelpers.PrintDebugMessage(" - AveragePriceFactorItemCategory: " + result.ToString());
            return result;
        }
        
        /// Towns
        public int GetTownListSize()
        {
            var result = Town.AllTowns.Count();
            return result;
        }

        private Town GetTownById(int townId)
        {
            return Town.AllTowns.ElementAt(townId);
        }

        public string GetTownName(int townId)
        {
            return GetTownById(townId).Name.ToString();
        }

        public string GetVillageName(int villageId)
        {
            return GetVillageById(villageId).Name.ToString();
        }

        public int GetTownItemStock(int townId)
        {
            return GetTownById(townId).Settlement.ItemRoster.GetItemNumber(_currentItemRosterElement.EquipmentElement.Item);
        }

        public int GetVillageItemStock(int villageId)
        {
            return GetVillageById(villageId).Settlement.ItemRoster.GetItemNumber(_currentItemRosterElement.EquipmentElement.Item);
        }

        public bool IsTownInRange(int townId, out float actualDistance)
        {
            var town = GetTownById(townId);
            actualDistance = GetDistanceTo(town.Settlement);
            return actualDistance < (float)AutoTraderConfig.SearchRadiusValue;
        }

        /// <summary>
        /// Straight-line distance from the party to the settlement, so towns across the sea count the same as towns over land.
        /// </summary>
        private float GetDistanceTo(Settlement settlement)
        {
            return MobileParty.MainParty.GetPosition2D.Distance(settlement.GetPosition2D);
        }
        public bool IsCurrentTown(int townId)
        {
            if (_isCaravan)
                return false;
            var town = GetTownById(townId);
            var result = Settlement.CurrentSettlement.IsTown && town == Settlement.CurrentSettlement.Town;
            return result;
        }
        public float GetTownItemPrice(int townId, bool isSelling)
        {
            var town = GetTownById(townId);
            var result = town.MarketData.GetPrice(_currentItemRosterElement.EquipmentElement.Item, PartyBase.MainParty.MobileParty, isSelling);
            return result;
        }
        public float GetCurrentTownPriceFactor()
        {
            Town town = Settlement.CurrentSettlement.IsVillage ? Settlement.CurrentSettlement.Village.Bound.Town : Settlement.CurrentSettlement.Town;
            var result = town.MarketData.GetPriceFactor(_currentItemRosterElement.EquipmentElement.Item.ItemCategory);
            AutoTraderHelpers.PrintDebugMessage(" - CurrentTownPriceFactor: " + result.ToString());
            return result;
        }

        /// Villages
        public int GetVillageListSize()
        {
            var result = Village.All.Count();
            return result;
        }

        private Village GetVillageById(int villageId)
        {
            return Village.All.ElementAt(villageId);
        }

        public bool IsVillageInRange(int villageId, out float actualDistance)
        {
            var village = GetVillageById(villageId);
            actualDistance = GetDistanceTo(village.Settlement);
            return actualDistance < (float)AutoTraderConfig.SearchRadiusValue;
        }
        public bool IsCurrentVillage(int townId)
        {
            if (_isCaravan)
                return false;
            var village = GetVillageById(townId);
            var result = Settlement.CurrentSettlement.IsVillage && village == Settlement.CurrentSettlement.Village;
            return result;
        }
        public float GetVillageItemPrice(int townId, bool isSelling)
        {
            var town = GetVillageById(townId);
            var result = town.MarketData.GetPrice(_currentItemRosterElement.EquipmentElement.Item, PartyBase.MainParty.MobileParty, isSelling, null);
            return result;
        }

        /// Helper Wrapper
        public bool IsArmor()
        {
            var itemRosterElement = _currentItemRosterElement;
            return AutoTraderHelpers.IsArmor(itemRosterElement.EquipmentElement.Item);
        }
        public bool IsWeapon()
        {
            var itemRosterElement = _currentItemRosterElement;
            return AutoTraderHelpers.IsWeapon(itemRosterElement.EquipmentElement.Item);
        }
        public bool IsMount()
        {
            return AutoTraderHelpers.IsRidingHorse(_currentItemRosterElement.EquipmentElement.Item);
        }
        public bool IsWarMount()
        {
            return AutoTraderHelpers.IsWarMount(_currentItemRosterElement.EquipmentElement.Item);
        }
        public bool IsNobleMount()
        {
            return AutoTraderHelpers.IsNobleMount(_currentItemRosterElement.EquipmentElement.Item);
        }
        public bool IsHorse()
        {
            var itemRosterElement = _currentItemRosterElement;
            return AutoTraderHelpers.IsHorse(itemRosterElement.EquipmentElement.Item);
        }
        public bool IsConsumable()
        {
            var itemRosterElement = _currentItemRosterElement;
            return AutoTraderHelpers.IsConsumable(itemRosterElement.EquipmentElement.Item);
        }
        public bool IsLivestock()
        {
            var itemRosterElement = _currentItemRosterElement;
            return AutoTraderHelpers.IsLivestock(itemRosterElement.EquipmentElement.Item);
        }

        /// <summary>
        /// The current roster element as an opaque handle, to switch back to it without searching the roster by name.
        /// </summary>
        public object GetCurrentElement()
        {
            return _currentItemRosterElement;
        }

        public void SetCurrentElement(object element)
        {
            _currentItemRosterElement = (ItemRosterElement)element;
        }

        public void SetCurrentElementByName(string itemName)
        {
            bool found = false;
            foreach (ItemRosterElement element in GetItemRoster())
            {
                if (element.EquipmentElement.Item.Name.ToString().Equals(itemName))
                {
                    found = true;
                    AutoTraderHelpers.PrintDebugMessage("Found by name: " + element.EquipmentElement.Item.Name.ToString() + " == " + itemName);
                    _currentItemRosterElement = element;
                    break;
                }
            }
            if (!found)
            {
                AutoTraderHelpers.PrintDebugMessage("!!!!! Could not find element with name : " + itemName + " !!!!!");
            }
        }

        public List<string> GetPlayerItemRosterNames()
        {
            return GetPlayerRoster().Select(x => x.EquipmentElement.Item.Name.ToString()).ToList();
        }

        public void SetCurrentElementByKey(string itemKey)
        {
            foreach (ItemRosterElement element in GetItemRoster())
            {
                if (GetElementKey(element.EquipmentElement) == itemKey)
                {
                    _currentItemRosterElement = element;
                    return;
                }
            }
            AutoTraderHelpers.PrintDebugMessage("!!!!! Could not find element with key : " + itemKey + " !!!!!");
        }

        public List<string> GetPlayerItemRosterKeys()
        {
            return GetPlayerRoster().Select(x => GetElementKey(x.EquipmentElement)).ToList();
        }

        public int GetItemAmountInPlayerRoster()
        {
            int amount = 0;
            foreach (ItemRosterElement element in GetPlayerRoster())
            {
                if (element.EquipmentElement.Item.Name.ToString().Equals(GetItemName()))
                {
                    amount = element.Amount;
                    break;
                }
            }
            AutoTraderHelpers.PrintDebugMessage("- amount in own inventory: " + amount.ToString());
            return amount;
        }
    }
}
