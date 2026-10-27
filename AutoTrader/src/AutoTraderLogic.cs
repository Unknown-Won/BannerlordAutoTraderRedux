using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.Core;
using TaleWorlds.Localization;

[assembly: InternalsVisibleTo("AutoTraderTests")]
namespace AutoTrader
{
    public class AutoTraderLogic
    {
        public bool IsTradingActive { get; set; }

        private readonly ILogicConnector _logicConnector;

        private AutoTraderLogicConnector Connector => (AutoTraderLogicConnector)_logicConnector;

        private float _availableInventoryCapacity;
        private int _availablePlayerGold;
        private int _mountSurplus;
        // Riding horses of each type (by name) that selling must leave in the party inventory
        private Dictionary<string, int> _keptMounts = new Dictionary<string, int>();
        private int _warMountCount;
        private bool _hardwoodResupplyPass;
        private bool _restockPass;
        private int _availableMerchantGold;

        private List<string> _soldItems;
        private List<string> _boughtItems;

        /// <summary>
        /// Prices of an item in the other markets in range. Calculated once per trade, since they don't change while we trade here.
        /// </summary>
        private class PriceInfo
        {
            // Averages used for trading (within the search radius)
            public float AverageBuyPrice;
            public float AverageSellPrice;
            // Averages over all markets, for the log
            public float GlobalBuyPrice;
            public float GlobalSellPrice;
            // Averages of the 5 best markets in range, this one included: highest sell prices (used for profit), lowest buy prices (for the log)
            public float TopSellPrice;
            public float TopBuyPrice;
            // For the log: how many markets each average counts, and the top 5 markets with their prices
            public int SellRangeCount;
            public int SellGlobalCount;
            public int BuyRangeCount;
            public int BuyGlobalCount;
            public string TopSellMarkets;
            public string TopBuyMarkets;
        }
        private Dictionary<string, PriceInfo> _priceCache = new Dictionary<string, PriceInfo>();

        private class BuyCandidate
        {
            public string Name;
            public object Element;
            public PriceInfo Prices;
            public int Amount;
            public int OwnAmount;
        }

        public AutoTraderLogic(ILogicConnector logicConnector)
        {
            AutoTraderHelpers.PrintDebugMessage("####### AutoTrader Initialization #######");
            _logicConnector = logicConnector;
            
        }

        public void OnApplicationTick()
        {
            Connector.TickInventoryDisplayRefresh();
        }

        /// <param name="sellFromBothRosters">In a port with Cargo Holds: also sell from the roster that is not traded with (party inventory or cargo hold) before trading</param>
        public void PerformAutoTrade(bool isCaravan = false, bool isPort = false, bool useCargoHold = false, bool sellFromBothRosters = false)
        {
            // Pick up debugMode changes made in the config file while the game is running
            AutoTraderConfig.ReloadDebugMode();
            AutoTraderHelpers.PrintDebugMessage("####### Performing AutoTrade #######");
            _logicConnector.IsCaravan = isCaravan;
            _logicConnector.IsPort = isPort;
            _logicConnector.UseCargoHold = useCargoHold;
            AutoTraderHelpers.PrintDebugMessage(" - isCaravan: " + isCaravan);
            AutoTraderHelpers.PrintDebugMessage(" - isPort: " + isPort);
            AutoTraderHelpers.PrintDebugMessage(" - useCargoHold: " + useCargoHold);
            int initialGold = _logicConnector.GetInitialGold();
            int troopWage = _logicConnector.GetTroopWage();
            _availablePlayerGold = initialGold - (AutoTraderConfig.KeepWagesValue * troopWage);
            AutoTraderHelpers.PrintDebugMessage(" - availablePlayerGold: " + _availablePlayerGold.ToString());

            _soldItems = new List<string>();
            _boughtItems = new List<string>();
            _priceCache = new Dictionary<string, PriceInfo>();
            _availableMerchantGold = _logicConnector.GetMerchantGold();

            // Set trading state
            IsTradingActive = true;
            try
            {
                bool sellOther = sellFromBothRosters && AutoTraderConfig.SellFromBothRostersValue;
                // Trading with the cargo hold leaves the party's food alone, so restock it on the side
                bool restockParty = sellFromBothRosters && useCargoHold && AutoTraderConfig.ResupplyValue;
                if (sellOther || restockParty)
                {
                    int merchantGold = _availableMerchantGold;
                    TradeOtherRoster(useCargoHold, isPort, sellOther, restockParty);
                    // That trade is only settled on Done, so the real trade can't spend what it earns,
                    // but must leave room for what it costs. Same for the merchant's gold.
                    int playerGain = _availablePlayerGold - (initialGold - (AutoTraderConfig.KeepWagesValue * troopWage));
                    _availablePlayerGold = initialGold - (AutoTraderConfig.KeepWagesValue * troopWage) + Math.Min(0, playerGain);
                    _availableMerchantGold = merchantGold + Math.Min(0, _availableMerchantGold - merchantGold);
                }
                UpdateAvailableInventoryCapacity();

                if (!_logicConnector.InitInventory())
                {
                    AutoTraderHelpers.PrintDebugMessage("Failed to initialize the inventory!");
                    _logicConnector.CancelBackgroundInventory();
                    return;
                }

                // Riding horses in the party inventory minus the men on foot (horses in the cargo hold mount nobody)
                _mountSurplus = useCargoHold ? 0 : _logicConnector.GetMountSurplus();
                _warMountCount = useCargoHold ? 0 : _logicConnector.GetWarMountCount();

                // Sell first so all buying can use the freed space
                Sell();

                AutoTraderHelpers.PrintDebugMessage("##### Restocking #####");
                _restockPass = true;
                try
                {
                    BuyProcess(RestockFilter);
                }
                finally
                {
                    _restockPass = false;
                }
                BuyHardwood();
                BuyMounts();
                BuyProcess(BuyFilter);
                // Buying gave the merchant gold, so sell again what it couldn't pay for before
                AutoTraderHelpers.PrintDebugMessage("##### Selling again after buying #####");
                Sell();
                //BuyHorses();
                Connector.BeginInventoryDisplayRefresh();
                Connector.RefreshUI();
            }
            catch (Exception e)
            {
                AutoTraderHelpers.PrintMessage("My Lord! Something terrible happened to our autotraders! The last we heard of them is:\n" + e.ToString());
            }
            finally
            {
                // Unset trading state
                IsTradingActive = false;
            }
        }

        /// <summary>
        /// Trades with the roster the trade screen will not show (the party inventory when trading with the cargo hold, and the other way round):
        /// sells from it, and restocks the party's food when that roster is the party inventory.
        /// Runs without a trade screen and is settled when the real trade screen closes, so all other buying goes to the opened roster.
        /// </summary>
        private void TradeOtherRoster(bool useCargoHold, bool isPort, bool sell, bool restock)
        {
            _logicConnector.UseCargoHold = !useCargoHold;
            // The party inventory uses party capacity, the cargo hold fleet capacity
            _logicConnector.IsPort = _logicConnector.UseCargoHold;
            try
            {
                string rosterName = _logicConnector.UseCargoHold ? "cargo hold" : "party inventory";
                AutoTraderHelpers.PrintDebugMessage("##### Trading with the " + rosterName + " #####");
                if (!_logicConnector.InitBackgroundInventory())
                {
                    AutoTraderHelpers.PrintDebugMessage(" - could not open the other roster for trading (Cargo Holds missing or fleet not docked here)");
                    return;
                }

                int startGold = _availablePlayerGold;
                int soldGold = 0;
                _mountSurplus = _logicConnector.UseCargoHold ? 0 : _logicConnector.GetMountSurplus();
                _warMountCount = _logicConnector.UseCargoHold ? 0 : _logicConnector.GetWarMountCount();
                UpdateAvailableInventoryCapacity();
                try
                {
                    if (sell)
                        Sell();
                    soldGold = _availablePlayerGold - startGold;
                    if (restock && !_logicConnector.UseCargoHold)
                    {
                        AutoTraderHelpers.PrintDebugMessage("##### Restocking the party inventory #####");
                        _restockPass = true;
                        try
                        {
                            BuyProcess(RestockFilter);
                        }
                        finally
                        {
                            _restockPass = false;
                        }
                    }
                }
                finally
                {
                    // Settled when the trade screen's Done is pressed, undone on Cancel
                    _logicConnector.DeferBackgroundInventory();
                }

                int boughtGold = soldGold - (_availablePlayerGold - startGold);
                AutoTraderHelpers.PrintDebugMessage(" - " + rosterName + ": sold for " + soldGold.ToString() + ", bought for " + boughtGold.ToString() + " (settled on Done)");
            }
            finally
            {
                _logicConnector.UseCargoHold = useCargoHold;
                _logicConnector.IsPort = isPort;
            }
        }

        private void UpdateAvailableInventoryCapacity()
        {
            float currentWeight = _logicConnector.GetCurrentWeight();
            float inventoryCapacity = _logicConnector.GetInventoryCapacity();
           
            // Ports trade with the cargo hold, which has its own limit
            int useSpace = _logicConnector.IsPort ? AutoTraderConfig.PortUseInventorySpaceValue : AutoTraderConfig.UseInventorySpaceValue;
            _availableInventoryCapacity = inventoryCapacity * ((float)useSpace / 100.0f);
            _availableInventoryCapacity -= currentWeight;
            AutoTraderHelpers.PrintDebugMessage(" - actual availableInventoryCapacity: " + _availableInventoryCapacity.ToString());
        }

        private void Sell()
        {
            AutoTraderHelpers.PrintDebugMessage("##### Selling #####");
            _logicConnector.IsBuying = false;
            ReserveCheapestMounts();
            // Item key, prices elsewhere, price factor here.
            // By key, not name: a locked item can share its name with an unlocked one (e.g. a plain and a rusty sword).
            var itemSellList = new List<Tuple<string, PriceInfo, float>>();

            foreach (string itemKey in _logicConnector.GetPlayerItemRosterKeys())
            {
                AutoTraderHelpers.PrintDebugMessage("-------------------------------------------------------------------");
                _logicConnector.SetCurrentElementByKey(itemKey);
                AutoTraderHelpers.PrintDebugMessage(" - current item: " + _logicConnector.GetItemName());

                // Check if its filtered
                if (_logicConnector.IsItemFiltered(_boughtItems))
                {
                    AutoTraderHelpers.PrintDebugMessage(" - Item is filtered!");
                    continue;
                }

                int amount = _logicConnector.GetItemAmount();
                if (amount < 1)
                {
                    AutoTraderHelpers.PrintDebugMessage(" - skipping because amount is less than 1");
                    continue;
                }

                // Selling compares against what the item sells for elsewhere
                PriceInfo prices = GetPrices();
                float priceFactor = (float)_logicConnector.GetCostOfRosterElement() / prices.AverageSellPrice;
                itemSellList.Add(Tuple.Create(itemKey, prices, priceFactor));
            }

            // Best deals first, so a merchant short on gold spends it on them
            itemSellList.Sort((item1, item2) => item2.Item3.CompareTo(item1.Item3));

            AutoTraderHelpers.PrintDebugMessage("### Actual sell loop ###");
            foreach (var element in itemSellList)
            {
                AutoTraderHelpers.PrintDebugMessage("-------------------------------------------------------------------");
                _logicConnector.SetCurrentElementByKey(element.Item1);
                AutoTraderHelpers.PrintDebugMessage(" - current item: " + _logicConnector.GetItemName() + " (price factor " + element.Item3.ToString() + ")");
                if (_logicConnector.IsItemLocked())
                    continue;
                int amount = _logicConnector.GetItemAmount();

                // Sell items one by one
                bool canSell = false;
                do
                {
                    int buyout_price = 0;
                    canSell = CanSell(element.Item2, amount, out buyout_price);
                    if (canSell)
                    {
                        // Update members
                        ProcessTransaction(buyout_price);
                        amount--;
                    }

                } while (canSell && amount > 0);
            }
        }

        /// <summary>
        /// Picks the riding horses selling must keep: the war mounts to keep for troop upgrades, then one horse for each man on foot
        /// (when mounting troops) plus the spare horses to keep. The horses that sell for least here are kept, so the others can still be sold.
        /// </summary>
        private void ReserveCheapestMounts()
        {
            _keptMounts.Clear();
            if (_logicConnector.UseCargoHold)
                return;

            // Riding horse types in the party inventory, cheapest to sell here first
            var mounts = new List<MountStock>();
            int totalMounts = 0;
            foreach (string itemName in _logicConnector.GetPlayerItemRosterNames())
            {
                _logicConnector.SetCurrentElementByName(itemName);
                if (!_logicConnector.IsMount())
                    continue;
                int amount = _logicConnector.GetItemAmount();
                if (amount <= 0)
                    continue;
                mounts.Add(new MountStock { Name = itemName, Amount = amount, Value = _logicConnector.GetCostOfRosterElement(), IsWarMount = _logicConnector.IsWarMount() });
                totalMounts += amount;
            }
            mounts.Sort((a, b) => a.Value.CompareTo(b.Value));

            // War mounts for troop upgrades
            KeepCheapestMounts(mounts.Where(x => x.IsWarMount), AutoTraderConfig.KeepWarMountsValue, "for troop upgrades");

            // Men on foot and spare horses, from the horses not kept above
            int menOnFoot = Math.Max(0, totalMounts - _mountSurplus);
            int toKeep = 0;
            if (AutoTraderConfig.KeepSpareMountsValue > 0)
                toKeep = menOnFoot + AutoTraderConfig.KeepSpareMountsValue;
            else if (IsMountingTroops())
                toKeep = menOnFoot;
            KeepCheapestMounts(mounts, toKeep, "for men on foot and spare horses");
        }

        private class MountStock
        {
            public string Name;
            public int Amount;
            public float Value;
            public bool IsWarMount;
        }

        /// <param name="mounts">Cheapest first</param>
        private void KeepCheapestMounts(IEnumerable<MountStock> mounts, int toKeep, string reason)
        {
            foreach (var mount in mounts)
            {
                if (toKeep <= 0)
                    break;
                int kept;
                _keptMounts.TryGetValue(mount.Name, out kept);
                int keep = Math.Min(mount.Amount - kept, toKeep);
                if (keep <= 0)
                    continue;
                _keptMounts[mount.Name] = kept + keep;
                toKeep -= keep;
                AutoTraderHelpers.PrintDebugMessage(" - keeping " + keep.ToString() + " " + mount.Name + " " + reason);
            }
        }

        private void BuyProcess(Func<bool> filterFunc)
        {
            AutoTraderHelpers.PrintDebugMessage("##### Executing buy process #####");

            _logicConnector.IsBuying = true;
            var candidates = new List<BuyCandidate>();

            // Loop through all items of merchant
            for (int itemId = 0; itemId < _logicConnector.GetMerchantItemRosterSize(); itemId++)
            {
                AutoTraderHelpers.PrintDebugMessage("-------------------------------------------------------------------");
                AutoTraderHelpers.PrintDebugMessage(" - current item ID: " + itemId.ToString());
                _logicConnector.SetCurrentElementById(itemId);

                // Price info is logged before filtering, so skipped items show it too
                int buyoutPrice = _logicConnector.GetItemPrice();
                PriceInfo prices = GetPrices();
                // Expected profit per item when sold elsewhere
                float profit = GetExpectedSellPrice(prices) - (float)buyoutPrice;
                AutoTraderHelpers.PrintDebugMessage(" - profit per item: " + profit.ToString());
                AutoTraderHelpers.PrintDebugMessage(" - profit per 10 weight: " + GetProfitPerTenWeight(profit).ToString());

                if (filterFunc())
                {
                    AutoTraderHelpers.PrintDebugMessage(" - skipping because item is filtered");
                    continue;
                }

                AutoTraderHelpers.PrintDebugMessage(" --> adding to buy list!");
                candidates.Add(new BuyCandidate
                {
                    Name = _logicConnector.GetItemName(),
                    Element = _logicConnector.GetCurrentElement(),
                    Prices = prices,
                    Amount = _logicConnector.GetItemAmount(),
                    OwnAmount = _logicConnector.GetItemAmountInPlayerRoster()
                });
            }

            // Buy one unit at a time, always of the item that is most profitable right now:
            // prices rise with every unit bought, and gold and space run out as we go
            AutoTraderHelpers.PrintDebugMessage("### Actual buy loop ###");
            while (candidates.Count > 0)
            {
                BuyCandidate best = null;
                float bestScore = 0.0f;
                foreach (var candidate in candidates)
                {
                    _logicConnector.SetCurrentElement(candidate.Element);
                    int price = _logicConnector.GetCostOfRosterElement();
                    float score = GetBuyScore(GetExpectedSellPrice(candidate.Prices) - (float)price, price);
                    if (best == null || score > bestScore)
                    {
                        best = candidate;
                        bestScore = score;
                    }
                }

                AutoTraderHelpers.PrintDebugMessage("-------------------------------------------------------------------");
                AutoTraderHelpers.PrintDebugMessage(" - current item Name: " + best.Name + " (score " + bestScore.ToString() + ")");
                PrintPrices(best.Prices);
                _logicConnector.SetCurrentElementByName(best.Name);

                int buyout_price = 0;
                if (best.Amount > 0 && CanBuy(best.Prices, best.Amount, best.OwnAmount, out buyout_price))
                {
                    ProcessTransaction(buyout_price);
                    best.Amount--;
                    best.OwnAmount++;
                    if (best.Amount <= 0)
                        candidates.Remove(best);
                }
                else
                {
                    // Once an item fails a check it stays out for this pass
                    candidates.Remove(best);
                }
            }
        }

        /// <summary>
        /// Profit per share of whichever this item would use up first: inventory space or gold.
        /// When space is short this ranks by profit per weight; when gold is short, by profit per denar spent.
        /// </summary>
        private float GetBuyScore(float profit, int price)
        {
            float goldShare = _availablePlayerGold > 0 ? (float)price / (float)_availablePlayerGold : float.MaxValue;
            float spaceShare = 0.0f;
            // Horses and livestock don't use inventory space
            if (!_logicConnector.IsHorse() && !_logicConnector.IsLivestock())
            {
                float weight = Math.Max(_logicConnector.GetItemWeight(), 0.1f);
                spaceShare = _availableInventoryCapacity > 0 ? weight / _availableInventoryCapacity : float.MaxValue;
            }
            float share = Math.Max(Math.Max(goldShare, spaceShare), 0.000001f);
            return profit / share;
        }

        private static float GetExpectedSellPrice(PriceInfo prices)
        {
            return prices.TopSellPrice;
        }

        /// <summary>
        /// Buys the cheapest riding horses until every man on foot has one, for the party speed bonus.
        /// Regular horses and war mounts each have their own toggle; noble mounts are never bought for this.
        /// Mounts for men on foot don't join the herd, so this never causes the herding penalty.
        /// </summary>
        private void BuyMounts()
        {
            if (!IsMountingTroops() || _logicConnector.UseCargoHold || _mountSurplus >= 0)
                return;

            AutoTraderHelpers.PrintDebugMessage("##### Buying mounts for troops on foot #####");
            _logicConnector.IsBuying = true;

            var mountList = new List<KeyValuePair<string, int>>();
            for (int itemId = 0; itemId < _logicConnector.GetMerchantItemRosterSize(); itemId++)
            {
                _logicConnector.SetCurrentElementById(itemId);
                if (!_logicConnector.IsMount() || _logicConnector.IsNobleMount() || _logicConnector.GetItemAmount() <= 0)
                    continue;
                if (_logicConnector.IsWarMount() ? !AutoTraderConfig.MountTroopsWarValue : !AutoTraderConfig.MountTroopsValue)
                    continue;
                mountList.Add(new KeyValuePair<string, int>(_logicConnector.GetItemName(), _logicConnector.GetItemPrice()));
            }

            // Cheapest first
            mountList.Sort((pair1, pair2) => pair1.Value.CompareTo(pair2.Value));

            foreach (var element in mountList)
            {
                if (_mountSurplus >= 0)
                    break;

                AutoTraderHelpers.PrintDebugMessage("-------------------------------------------------------------------");
                AutoTraderHelpers.PrintDebugMessage(" - current mount: " + element.Key);
                _logicConnector.SetCurrentElementByName(element.Key);
                int amount = _logicConnector.GetItemAmount();

                while (_mountSurplus < 0 && amount > 0)
                {
                    int buyoutPrice = _logicConnector.GetCostOfRosterElement();
                    if (!CheckBasicBuyRequirements(amount, buyoutPrice))
                        break;
                    ProcessTransaction(buyoutPrice);
                    amount--;
                }
            }
        }

        /// <summary>
        /// Tops hardwood up to its keep amount before regular buying, so other goods can't use up the inventory space first.
        /// </summary>
        private void BuyHardwood()
        {
            if (!AutoTraderConfig.ResupplyHardwoodValue)
                return;

            AutoTraderHelpers.PrintDebugMessage("##### Resupplying hardwood #####");
            _hardwoodResupplyPass = true;
            try
            {
                BuyProcess(HardwoodFilter);
            }
            finally
            {
                _hardwoodResupplyPass = false;
            }
        }

        private bool HardwoodFilter()
        {
            // Returns true if filtered
            if (!_logicConnector.IsItemHardwood())
                return true;
            if (_logicConnector.GetItemAmount() <= 0)
            {
                AutoTraderHelpers.PrintDebugMessage(" - skipping because out of stock");
                return true;
            }
            // Resupply ignores the "Trade Goods" buy toggle, but never buys back what was just sold
            if (_soldItems.Exists(x => x == _logicConnector.GetItemName()))
            {
                AutoTraderHelpers.PrintDebugMessage(" - skipping because it was sold in this trade");
                return true;
            }
            return false;
        }

        private static bool IsMountingTroops()
        {
            return AutoTraderConfig.MountTroopsValue || AutoTraderConfig.MountTroopsWarValue;
        }

        private bool RestockFilter()
        {
            // Returns true if filtered
            if (!AutoTraderConfig.ResupplyValue)
            {
                AutoTraderHelpers.PrintDebugMessage("- Skipping resupply due to config");
                return true;
            }
            if (_logicConnector.UseCargoHold)
            {
                // Food in the cargo hold does not count as food supply
                AutoTraderHelpers.PrintDebugMessage("- Skipping resupply because we trade with the cargo hold");
                return true;
            }
            if (_logicConnector.GetItemAmount() <= 0)
            {
                AutoTraderHelpers.PrintDebugMessage(" - skipping because out of stock");
                return true;
            }
            if (!_logicConnector.IsConsumable())
            {
                AutoTraderHelpers.PrintDebugMessage(" - skipping because it is not a consumable");
                return true;
            }
            if (AutoTraderSpecialRules.CheckBuyResupplyRule(_logicConnector, _logicConnector.GetItemAmount()))
            {
                AutoTraderHelpers.PrintDebugMessage(" - skipping because of the resupply rule");
                return true; ;
            }
            return false;
        }

        private bool BuyFilter()
        {
            var result = _logicConnector.IsItemFiltered(_soldItems);
            if(result)
                AutoTraderHelpers.PrintDebugMessage(" - skipping because item is filtered");
            return result;
        }

        //private bool BuyHorseFilter()
        //{
        //    if (!_logicConnector.IsHorse())
        //    {
        //        AutoTraderHelpers.PrintDebugMessage(" - skipping because item is not horse");
        //        continue;
        //    }
        //}

        //private void BuyHorses()
        //{
        //    AutoTraderHelpers.PrintDebugMessage("### Buying Horses ###");
        //    _logicConnector.IsBuying = true;

        //    // Loop through items
        //    for (int itemId = 0; itemId < _logicConnector.GetMerchantItemRosterSize(); itemId++)
        //    {
        //        AutoTraderHelpers.PrintDebugMessage("-------------------------------------------------------------------");
        //        AutoTraderHelpers.PrintDebugMessage(" - current item ID: " + itemId.ToString());
        //        _logicConnector.SetCurrentElementById(itemId);
        //        // Check if its filtered
        //        if (!_logicConnector.IsHorse())
        //        {
        //            AutoTraderHelpers.PrintDebugMessage(" - skipping because item is not horse");
        //            continue;
        //        }

        //        int amount = _logicConnector.GetItemAmount();
        //        if (amount < 1)
        //        {
        //            AutoTraderHelpers.PrintDebugMessage(" - skipping because amount is less than 1");
        //            continue;
        //        }

        //        float averagePrice = GetAveragePrice();
        //        int buyoutPrice = 0;

        //        bool canBuy = false;
        //        do
        //        {
        //            canBuy = CanBuy(averagePrice, 1, out buyoutPrice);
        //            if (canBuy)
        //            {
        //                ProcessTransaction(buyoutPrice);
        //                amount -= 1;
        //            }
        //        } while (canBuy && amount > 0);
        //    }
        //}

        internal bool SimpleWorthCheck(int value, int buyoutPrice)
        {
            AutoTraderHelpers.PrintDebugMessage("### Simple Worth Check ###");
            /// Checks if the value is below or above threshold 
            var result = false;
            if (!_logicConnector.IsBuying && buyoutPrice >= ((float)AutoTraderConfig.SellThresholdValue / 100.0f) * (float)value)
            {
                result = true;
            }
            else if (_logicConnector.IsBuying && buyoutPrice < ((float)AutoTraderConfig.BuyThresholdValue / 100.0f) * (float)value)
            {
                result = true;
            }
            AutoTraderHelpers.PrintDebugMessage("- isItemWorth: " + result.ToString());
            return result;
        }

        private void ProcessTransaction(int buyoutPrice)
        {
            AutoTraderHelpers.PrintDebugMessage("### Processing transaction ###");
            // Update available gold
            _availablePlayerGold += _logicConnector.IsBuying ? -buyoutPrice : buyoutPrice;
            _availableMerchantGold += _logicConnector.IsBuying ? buyoutPrice : -buyoutPrice;

            // Mark the item
            string name = _logicConnector.GetItemName();
            if (_logicConnector.IsBuying)
            {
                if (!_boughtItems.Exists(x => x == name))
                    _boughtItems.Add(name);
            } else
            {
                if (!_soldItems.Exists(x => x == name))
                    _soldItems.Add(name);
            }

            // Track riding horses in the party inventory
            if (!_logicConnector.UseCargoHold && _logicConnector.IsMount())
            {
                _mountSurplus += _logicConnector.IsBuying ? 1 : -1;
                if (_logicConnector.IsWarMount())
                    _warMountCount += _logicConnector.IsBuying ? 1 : -1;
            }

            // Update available weight
            UpdateAvailableInventoryCapacity();

            _logicConnector.TransferItem();
        }

        /// <param name="prices">"Buy under X%" compares against the average sell price elsewhere, the hardwood % against the average buy price,
        /// and the profit checks use the expected sell price (best or average)</param>
        private bool CanBuy(PriceInfo prices, int amount, int ownAmount, out int buyoutPrice)
        {
            AutoTraderHelpers.PrintDebugMessage("### Can buy check ###");
            float averageSellPrice = GetExpectedSellPrice(prices);
            // TODO: Refactor, too many return statements

            // Retrieve price
            buyoutPrice = _logicConnector.GetCostOfRosterElement();

            // Special Rules
            // Horses
            if (_logicConnector.IsHorse())
            {
                // Most gold in one horse type to own for trading, counting the one we'd buy at the current buy price
                if (AutoTraderConfig.MaxHorseGoldPerTypeValue < AutoTraderConfig.MaxHorseGoldPerTypeInf && (long)(ownAmount + 1) * buyoutPrice > AutoTraderConfig.MaxHorseGoldPerTypeValue)
                {
                    AutoTraderHelpers.PrintDebugMessage("- do not buy because " + (ownAmount + 1).ToString() + " of this horse would be worth " + ((long)(ownAmount + 1) * buyoutPrice).ToString() + ", the maximum per type is " + AutoTraderConfig.MaxHorseGoldPerTypeValue.ToString());
                    return false;
                }

                // Riding horses: normal trading under the buy price rules
                if (_logicConnector.IsMount())
                {
                    if (AutoTraderConfig.BuyMountsValue && IsBuyPriceOrProfitAcceptable(prices, buyoutPrice) && IsBuyProfitAcceptable(averageSellPrice, buyoutPrice))
                    {
                        AutoTraderHelpers.PrintDebugMessage("- buying riding horse");
                        return CheckBasicBuyRequirements(amount, buyoutPrice);
                    }
                    return false;
                }

                if (AutoTraderSpecialRules.CheckBuyHorsesRules(_logicConnector, buyoutPrice, _availablePlayerGold))
                    return CheckBasicBuyRequirements(amount, buyoutPrice);
                AutoTraderHelpers.PrintDebugMessage(" - do not buy because we need no additional horses");
                return false; // buy no other horses
            }

            // Hardwood: resupply up to its own keep amount, ignoring the "same good" capacity limit.
            // Above the keep amount it is traded like any other good.
            if (AutoTraderSpecialRules.CheckBuyResupplyHardwoodRule(_logicConnector, ownAmount))
            {
                // Hardwood to keep has its own price threshold, which also applies with green/red-based trading
                float priceFactor = (float)buyoutPrice / prices.AverageBuyPrice;
                AutoTraderHelpers.PrintDebugMessage(" - price factor: " + priceFactor.ToString());
                if (priceFactor > (float)AutoTraderConfig.HardwoodBuyThresholdValue / 100.0f)
                {
                    AutoTraderHelpers.PrintDebugMessage(" - do not buy hardwood to keep because priceFactor is above threshold (" + priceFactor.ToString() + " > " + ((float)AutoTraderConfig.HardwoodBuyThresholdValue / 100.0f).ToString() + ")");
                    return false;
                }
                AutoTraderHelpers.PrintDebugMessage("- buying hardwood to resupply");
                return CheckBasicBuyRequirements(amount, buyoutPrice);
            }
            if (_hardwoodResupplyPass)
            {
                // The resupply pass only fills up to the keep amount; the rest is bought with the other goods
                AutoTraderHelpers.PrintDebugMessage("- hardwood keep amount reached");
                return false;
            }

            // Consumables: never buy past the maximum, in the cargo hold as well
            if (_logicConnector.IsConsumable())
            {
                int maxAmount = _logicConnector.IsItemGrain() ? AutoTraderConfig.KeepGrainsMaxValue : AutoTraderConfig.KeepConsumablesMaxValue;

                if (ownAmount >= maxAmount)
                {
                    AutoTraderHelpers.PrintDebugMessage(" --> not buying because we have enough");
                    return false;
                }
            }
            // Food minimums (food in the cargo hold is plain cargo, not food supply)
            if (_logicConnector.IsConsumable() && !_logicConnector.UseCargoHold)
            {
                if (AutoTraderSpecialRules.CheckBuyConsumablesRules(_logicConnector, ownAmount))
                    return CheckBasicBuyRequirements(amount, buyoutPrice);
                if (_restockPass)
                {
                    // The restock pass only fills up to the keep minimum; the rest is bought with the other goods
                    AutoTraderHelpers.PrintDebugMessage("- food keep minimum reached");
                    return false;
                }
            }

            // Price niveau: pay at most X% of what it typically sells for elsewhere, or make enough gold per item
            if (!IsBuyPriceOrProfitAcceptable(prices, buyoutPrice))
                return false;
            if (!IsBuyProfitAcceptable(averageSellPrice, buyoutPrice))
                return false;

            // Specials rules after price check
            // Check if we have enough cattle
            if (AutoTraderSpecialRules.CheckBuyCattleCondition(_logicConnector))
            {
                if (AutoTraderSpecialRules.CheckBuyCattleRule(_logicConnector))
                    return CheckBasicBuyRequirements(amount, buyoutPrice);
                else
                {
                    AutoTraderHelpers.PrintDebugMessage("- do not buy because we need no more cattle");
                    return false; // Don't buy more cattle   
                }
            }

            // Check weight
            if (AutoTraderSpecialRules.CheckBuyMaxCapacityRule(_logicConnector, (int)_logicConnector.GetInventoryCapacity(), ownAmount))
                return CheckBasicBuyRequirements(amount, buyoutPrice);
            else
            {
                AutoTraderHelpers.PrintDebugMessage("- do not buy because we are at capacity");
                return false;
            }
        }

        /// <summary>
        /// Profit scaled to 10 units of inventory weight, so light goods are compared fairly with heavy ones.
        /// Horses and livestock don't use inventory capacity, so they count as weighing 10 (profit per item).
        /// </summary>
        private float GetProfitPerTenWeight(float profit)
        {
            float weight = 10.0f;
            if (!_logicConnector.IsHorse() && !_logicConnector.IsLivestock())
                weight = Math.Max(_logicConnector.GetItemWeight(), 0.1f);
            return profit * 10.0f / weight;
        }

        /// <summary>
        /// Regular buying skips items whose profit per 10 weight (average price minus buy price, scaled by weight) is below the minimum profit setting.
        /// Food, hardwood and mounts bought to reach their keep amounts don't use this check.
        /// </summary>
        private bool IsBuyProfitAcceptable(float averageSellPrice, int buyoutPrice)
        {
            float weightedProfit = GetProfitPerTenWeight(averageSellPrice - (float)buyoutPrice);
            // The cargo hold has its own minimum
            int minProfit = _logicConnector.IsPort ? AutoTraderConfig.PortMinBuyProfitValue : AutoTraderConfig.MinBuyProfitValue;
            if (weightedProfit < minProfit)
            {
                AutoTraderHelpers.PrintDebugMessage("- do not buy because profit per 10 weight " + weightedProfit.ToString() + " is below the minimum of " + minProfit.ToString());
                return false;
            }
            return true;
        }

        /// <summary>
        /// "Buy under X%" check, which expensive items can also pass by making at least the set gold profit per item.
        /// </summary>
        private bool IsBuyPriceOrProfitAcceptable(PriceInfo prices, int buyoutPrice)
        {
            if (IsBuyPriceAcceptable(prices.AverageSellPrice, buyoutPrice))
                return true;

            float profit = GetExpectedSellPrice(prices) - (float)buyoutPrice;
            if (AutoTraderConfig.BuyProfitOverrideValue > 0 && profit >= AutoTraderConfig.BuyProfitOverrideValue)
            {
                AutoTraderHelpers.PrintDebugMessage("- buying anyway because profit per item " + profit.ToString() + " is at least " + AutoTraderConfig.BuyProfitOverrideValue.ToString());
                return true;
            }
            return false;
        }

        private bool IsBuyPriceAcceptable(float averagePrice, int buyoutPrice)
        {
            if (AutoTraderConfig.SimpleTradingAI)
            {
                // Case: buying
                float averagePriceFactorItemCategory = _logicConnector.GetAveragePriceFactorItemCategory();
                AutoTraderHelpers.PrintDebugMessage(" - average price factor item category: " + averagePriceFactorItemCategory.ToString());

                if (averagePriceFactorItemCategory != -99.0)
                {
                    float priceFactor = _logicConnector.GetCurrentTownPriceFactor();
                    AutoTraderHelpers.PrintDebugMessage(" - price factor: " + priceFactor.ToString());
                    if (priceFactor > averagePriceFactorItemCategory * 0.8f)
                    {
                        AutoTraderHelpers.PrintDebugMessage(" - do not buy because the price factor is too bad (" + priceFactor.ToString() + " > " + (averagePriceFactorItemCategory * 0.8f).ToString() + ")");
                        return false;
                    }
                        
                } else {
                    AutoTraderHelpers.PrintDebugMessage("- no average price found for " + _logicConnector.GetItemName());
                    return false;
                }
            }
            else
            {
                // Check threshold
                float priceFactor = (float)buyoutPrice / averagePrice;
                AutoTraderHelpers.PrintDebugMessage(" - price factor: " + priceFactor.ToString());
                if (priceFactor > (float)AutoTraderConfig.BuyThresholdValue / 100.0f)
                {
                    AutoTraderHelpers.PrintDebugMessage(" - do not buy because priceFactor is above threshold (" + priceFactor.ToString() + " > " + ((float)AutoTraderConfig.BuyThresholdValue / 100.0f).ToString() + ")");
                    return false;
                }
            }
            return true;
        }

        private bool CanSell(PriceInfo prices, int amount, out int buyoutPrice)
        {
            AutoTraderHelpers.PrintDebugMessage("### Can sell check ###");
            float averagePrice = prices.AverageSellPrice;
            // Retrieve price
            buyoutPrice = _logicConnector.GetCostOfRosterElement();

            // Sell all Armor and Weapons
            if (_logicConnector.IsArmor())
            {
                if (_logicConnector.IsItemTierLowerThan((ItemObject.ItemTiers)AutoTraderConfig.WeaponsArmorTierValue))
                {
                    return CheckBasicSellRequirements(amount, buyoutPrice);
                }
                else
                {
                    AutoTraderHelpers.PrintDebugMessage("- do not sell because tier is too high");
                    return false;
                }
            } else if (_logicConnector.IsWeapon())
            {
                // Keep handmade weapons
                if (AutoTraderConfig.KeepSmeltingValue && !_logicConnector.IsWeaponDesignEmpty())
                {
                    AutoTraderHelpers.PrintDebugMessage("- do not sell because its crafted");
                    return false;
                }

                if (_logicConnector.IsItemTierLowerThan((ItemObject.ItemTiers)AutoTraderConfig.WeaponsArmorTierValue))
                {
                    return CheckBasicSellRequirements(amount, buyoutPrice);
                }
                else
                {
                    AutoTraderHelpers.PrintDebugMessage("- do not sell because tier is too high");
                    return false;
                }
            }
                
            // Special horse rule
            if (_logicConnector.IsHorse())
            {
                // Keep the cheapest war mounts for troop upgrades and riding horses for men on foot and as spare horses (see ReserveCheapestMounts)
                int keptMounts;
                if (!_logicConnector.UseCargoHold && _logicConnector.IsMount()
                    && _keptMounts.TryGetValue(_logicConnector.GetItemName(), out keptMounts) && amount <= keptMounts)
                {
                    AutoTraderHelpers.PrintDebugMessage("- do not sell because we keep " + keptMounts.ToString() + " of this horse for troop upgrades, men on foot and spare horses");
                    return false;
                }


                if (_logicConnector.IsPackAnimal() && AutoTraderConfig.SellHorsesValue)
                {
                    AutoTraderHelpers.PrintDebugMessage("- do not sell because its a pack animal");
                    return false;
                }
            }

            // Check amounts to keep (food in the cargo hold is plain cargo, not food supply)
            if (_logicConnector.IsConsumable() && !_logicConnector.UseCargoHold)
            {
                int maxAmountToKeep = _logicConnector.IsItemGrain() ?
                    AutoTraderConfig.KeepGrainsMaxValue : AutoTraderConfig.KeepConsumablesMaxValue;
                int minAmountToKeep = _logicConnector.IsItemGrain() ?
                    AutoTraderConfig.KeepGrainsMinValue : AutoTraderConfig.KeepConsumablesMinValue;

                // Selling this one must leave at least the minimum
                if (amount <= minAmountToKeep)
                {
                    AutoTraderHelpers.PrintDebugMessage("- do not sell because we dont have enough of this consumable");
                    return false;
                }
                if (maxAmountToKeep < amount)
                {
                    AutoTraderHelpers.PrintDebugMessage("- selling because we have too much of this consumable");
                    return CheckBasicSellRequirements(amount, buyoutPrice);
                }
            }

            // Livestock
            if (_logicConnector.IsLivestock())
            {
                // Sell if its treated as junk
                if (AutoTraderConfig.JunkCattleValue)
                    return CheckBasicSellRequirements(amount, buyoutPrice);
            }

            // Special hardwood rule
            if (AutoTraderSpecialRules.CheckBuyResupplyHardwoodRule(_logicConnector, amount))
            {
                AutoTraderHelpers.PrintDebugMessage("- do not sell because we dont have enough hardwood");
                return false;                
            }

            // Don't sell below what we paid, unless no other market in range pays more than here
            int paidPrice = buyoutPrice - _logicConnector.GetProjectedProfit(buyoutPrice);
            if (paidPrice > 0 && buyoutPrice < paidPrice && prices.TopSellPrice > buyoutPrice)
            {
                AutoTraderHelpers.PrintDebugMessage("- do not sell below the price we paid (" + buyoutPrice.ToString() + " < " + paidPrice.ToString() + ")");
                return false;
            }

            if (AutoTraderConfig.SimpleTradingAI)
            {
                int weighted_profit = buyoutPrice - _logicConnector.GetProjectedProfit(buyoutPrice);
                AutoTraderHelpers.PrintDebugMessage(" - weighted_profit: " + weighted_profit.ToString());
                // Check case of no trade rumours
                if (weighted_profit == 0)
                {
                    AutoTraderHelpers.PrintDebugMessage(" - no trade rumours!");
                    float priceFactor = (float)buyoutPrice / averagePrice;
                    if (priceFactor <= 1.2f)
                    {
                        AutoTraderHelpers.PrintDebugMessage("- do not sell because price factor is too low: " + priceFactor.ToString());
                        return false;
                    }

                }
                else if (weighted_profit > buyoutPrice * 0.8f)
                {
                    AutoTraderHelpers.PrintDebugMessage("- do not sell because weighted profit (" + weighted_profit.ToString() + ") is higher than " + (buyoutPrice * 0.8f).ToString());
                    return false;
                }
                    
            }
            else
            {
                // Check threshold
                float priceFactor = (float)buyoutPrice / averagePrice;
                if (priceFactor < (float)AutoTraderConfig.SellThresholdValue / 100.0f)
                {
                    AutoTraderHelpers.PrintDebugMessage("- do not sell because price factor is less than sell threshold: " + priceFactor.ToString());
                    return false;
                }
                    
            }
            
            return CheckBasicSellRequirements(amount, buyoutPrice);
        }

        private bool CheckBasicBuyRequirements(int amount, int price)
        {
            AutoTraderHelpers.PrintDebugMessage("### Checking basic buy requirements ###");
            if (price >= _availablePlayerGold)
            {
                AutoTraderHelpers.PrintDebugMessage("- do not buy because not enough gold: " + price.ToString() + " >= " + _availablePlayerGold.ToString());
                return false;
            }

            if (amount <= 0)
            {
                AutoTraderHelpers.PrintDebugMessage("- do not buy because not enough stock");
                return false;
            }


            // Never push the party over the herding limit, whatever rule led here
            if (!_logicConnector.HasRoomInHerd())
            {
                AutoTraderHelpers.PrintDebugMessage("- do not buy because the herd would exceed the party size");
                return false;
            }

            // ToDo: Only for carry horses? Nop, also for livestock.
            if (!_logicConnector.IsHorse() && !_logicConnector.IsLivestock() && _logicConnector.GetItemWeight() > _availableInventoryCapacity)
            {
                AutoTraderHelpers.PrintDebugMessage("- do not buy because not enough capacity: " + _logicConnector.GetItemWeight().ToString() + " > " + _availableInventoryCapacity.ToString());
                return false;
            }

            AutoTraderHelpers.PrintDebugMessage("- fulfills basic buy requirements");
            return true;
        }

        private bool CheckBasicSellRequirements(int amount, int price)
        {
            AutoTraderHelpers.PrintDebugMessage("### Checking basic sell requirements ###");
            if (price >= _availableMerchantGold)
            {
                AutoTraderHelpers.PrintDebugMessage(" - not selling because not enough merchant gold");
                return false;
            }
                

            if (amount <= 0)
            {
                AutoTraderHelpers.PrintDebugMessage(" - not selling because not enough stock");
                return false;
            }

            AutoTraderHelpers.PrintDebugMessage("- fulfills basic sell requirements");
            return true;
        }

        /// <summary>
        /// Averages the item's prices in the towns (and villages for trade goods) within the search radius.
        /// The buy price is what we would pay there; the sell price is what we would get for it there.
        /// </summary>
        private PriceInfo GetPrices()
        {
            // Keyed by the item ID, since different items can share a display name
            string itemId = _logicConnector.GetItemId();
            PriceInfo prices;
            if (_priceCache.TryGetValue(itemId, out prices))
            {
                AutoTraderHelpers.PrintDebugMessage(" - cached prices:");
                PrintPrices(prices);
                return prices;
            }

            prices = new PriceInfo();
            GetAveragePrices(prices);
            _priceCache[itemId] = prices;
            return prices;
        }

        // Number of markets the top sell / buy averages are taken over
        private const int TopMarketCount = 5;

        private class MarketPrice
        {
            public string Name;
            public int Stock;
            public float SellPrice;
            public float BuyPrice;
            public float Distance;
            public bool IsInRange;
            public bool IsCurrent;
        }

        /// <summary>
        /// Works out the item's prices in the towns and the village we trade in:
        /// the average used for trading (within the search radius, or over all markets weighted by distance or with an infinite radius),
        /// the global average over all markets, and the average of the 5 best markets in range (this one included).
        /// Sell prices count every town, since you can sell anywhere. Buy prices only count towns that have the item in stock.
        /// </summary>
        private void GetAveragePrices(PriceInfo prices)
        {
            AutoTraderHelpers.PrintDebugMessage("### Getting average price ###");
            var markets = new List<MarketPrice>();
            float actualDistance = 0;

            for (int townId = 0; townId < _logicConnector.GetTownListSize(); townId++)
            {
                try
                {
                    var market = new MarketPrice();
                    market.Name = _logicConnector.GetTownName(townId);
                    market.IsInRange = _logicConnector.IsTownInRange(townId, out actualDistance);
                    market.Distance = actualDistance;
                    market.IsCurrent = _logicConnector.IsCurrentTown(townId);
                    market.Stock = _logicConnector.GetTownItemStock(townId);
                    market.SellPrice = _logicConnector.GetTownItemPrice(townId, true);
                    market.BuyPrice = _logicConnector.GetTownItemPrice(townId, false);
                    markets.Add(market);
                }
                catch (Exception e)
                {
                    AutoTraderHelpers.PrintDebugMessage("ERROR: Could not retrieve average price for town: " + e.ToString());
                }
            }

            // A village prices goods like the town it trades with, so other villages would only repeat their town's price.
            // Only the village we trade in counts.
            if (_logicConnector.IsItemTradeGood())
            {
                for (int villageId = 0; villageId < _logicConnector.GetVillageListSize(); villageId++)
                {
                    try
                    {
                        if (!_logicConnector.IsCurrentVillage(villageId))
                            continue;
                        var market = new MarketPrice();
                        market.IsCurrent = true;
                        market.Stock = _logicConnector.GetVillageItemStock(villageId);
                        market.Name = _logicConnector.GetVillageName(villageId);
                        market.IsInRange = _logicConnector.IsVillageInRange(villageId, out actualDistance);
                        market.Distance = actualDistance;
                        market.SellPrice = _logicConnector.GetVillageItemPrice(villageId, true);
                        market.BuyPrice = _logicConnector.GetVillageItemPrice(villageId, false);
                        markets.Add(market);
                    }
                    catch (Exception e)
                    {
                        AutoTraderHelpers.PrintDebugMessage("ERROR: Could not retrieve average price for village: " + e.ToString());
                    }
                }
            }

            float fallback = _logicConnector.GetAveragePriceFallback();
            var inRange = markets.Where(x => x.IsInRange || x.IsCurrent).ToList();

            // Sell prices: every market
            var sellMarkets = markets;
            int sellRangeCount;
            prices.AverageSellPrice = GetTradingAverage(sellMarkets, x => x.SellPrice, fallback, out sellRangeCount);
            prices.GlobalSellPrice = sellMarkets.Count > 0 ? sellMarkets.Average(x => x.SellPrice) : fallback;
            var topSell = inRange.OrderByDescending(x => x.SellPrice).Take(TopMarketCount).ToList();
            prices.TopSellPrice = topSell.Count > 0 ? topSell.Average(x => x.SellPrice) : prices.AverageSellPrice;

            // Buy prices: only markets that have the item in stock, since you can't buy anywhere else.
            // If no market has it, all markets are used so the averages still mean something.
            var buyMarkets = markets.Where(x => x.Stock > 0).ToList();
            if (buyMarkets.Count == 0)
                buyMarkets = markets;
            int buyRangeCount;
            prices.AverageBuyPrice = GetTradingAverage(buyMarkets, x => x.BuyPrice, fallback, out buyRangeCount);
            prices.GlobalBuyPrice = buyMarkets.Count > 0 ? buyMarkets.Average(x => x.BuyPrice) : fallback;
            var topBuy = inRange.Where(x => x.Stock > 0).OrderBy(x => x.BuyPrice).Take(TopMarketCount).ToList();
            prices.TopBuyPrice = topBuy.Count > 0 ? topBuy.Average(x => x.BuyPrice) : prices.AverageBuyPrice;

            prices.SellRangeCount = sellRangeCount;
            prices.SellGlobalCount = sellMarkets.Count;
            prices.BuyRangeCount = buyRangeCount;
            prices.BuyGlobalCount = buyMarkets.Count;
            prices.TopSellMarkets = string.Join(", ", topSell.Select(x => x.Name + " " + x.SellPrice.ToString()));
            prices.TopBuyMarkets = string.Join(", ", topBuy.Select(x => x.Name + " " + x.BuyPrice.ToString() + " (stock " + x.Stock.ToString() + ")"));
            PrintPrices(prices);
        }

        /// <summary>
        /// Average used for trading: the markets within the search radius, or all other markets weighted by distance,
        /// or all markets with an infinite radius. count is the number of markets used.
        /// </summary>
        private float GetTradingAverage(List<MarketPrice> markets, Func<MarketPrice, float> price, float fallback, out int count)
        {
            float total = 0;
            float weights = 0.0f;
            count = 0;
            foreach (MarketPrice market in markets)
            {
                float weight = 1.0f;
                if (AutoTraderConfig.UseWeightedValue)
                {
                    // Every other market, weighted by distance
                    if (market.IsCurrent)
                        continue;
                    weight = 1.0f / Math.Max(market.Distance, 0.01f);
                }
                else if (!market.IsInRange && AutoTraderConfig.SearchRadiusValue <= 999)
                {
                    continue;
                }
                total += price(market) * weight;
                weights += weight;
                count++;
            }
            if (weights > 0 && total > 0)
                return total / weights;
            AutoTraderHelpers.PrintDebugMessage(" - no market prices, using AveragePriceFallback: " + fallback.ToString());
            return fallback;
        }

        private void PrintPrices(PriceInfo prices)
        {
            AutoTraderHelpers.PrintDebugMessage(" - buy price, average in range: " + prices.AverageBuyPrice.ToString() + " (" + prices.BuyRangeCount.ToString() + " markets with stock)");
            AutoTraderHelpers.PrintDebugMessage(" - buy price, global average: " + prices.GlobalBuyPrice.ToString() + " (" + prices.BuyGlobalCount.ToString() + " markets with stock)");
            AutoTraderHelpers.PrintDebugMessage(" - buy price, top " + TopMarketCount.ToString() + " in range: " + prices.TopBuyPrice.ToString() + ": " + prices.TopBuyMarkets);
            AutoTraderHelpers.PrintDebugMessage(" - sell price, average in range: " + prices.AverageSellPrice.ToString() + " (" + prices.SellRangeCount.ToString() + " markets)");
            AutoTraderHelpers.PrintDebugMessage(" - sell price, global average: " + prices.GlobalSellPrice.ToString() + " (" + prices.SellGlobalCount.ToString() + " markets)");
            AutoTraderHelpers.PrintDebugMessage(" - sell price, top " + TopMarketCount.ToString() + " in range: " + prices.TopSellPrice.ToString() + ": " + prices.TopSellMarkets);
        }

    }
}
