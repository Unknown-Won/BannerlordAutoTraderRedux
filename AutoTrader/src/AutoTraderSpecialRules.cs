
namespace AutoTrader
{
    public static class AutoTraderSpecialRules
    {
        // Capacity
        public static bool CheckBuyMaxCapacityRule(ILogicConnector logicConnector, int baseCapacity, int amount)
        {
            // Checks if we have the item too often
            var rosterWeight = logicConnector.GetItemWeight() * amount;
            AutoTraderHelpers.PrintDebugMessage("- weight in roster: " + rosterWeight.ToString());
            int maxCapacity = logicConnector.IsPort ? AutoTraderConfig.PortMaxCapacityValue : AutoTraderConfig.MaxCapacityValue;
            if (rosterWeight >= (float)baseCapacity * ((float)maxCapacity / 100f))
            {
                return false;
            }

            // Ports: also at most this many of one good, whichever limit is lower
            if (logicConnector.IsPort && AutoTraderConfig.PortMaxItemCountValue < AutoTraderConfig.PortMaxItemCountInf
                && amount >= AutoTraderConfig.PortMaxItemCountValue)
            {
                AutoTraderHelpers.PrintDebugMessage("- at the port maximum of " + AutoTraderConfig.PortMaxItemCountValue.ToString() + " of one good");
                return false;
            }
            return true;
        }

        // Cattle
        public static bool CheckBuyCattleCondition(ILogicConnector logicConnector)
        {
            return AutoTraderConfig.BuyLivestockValue && logicConnector.IsLivestock();
        }

        public static bool CheckBuyCattleRule(ILogicConnector logicConnector)
        {
            return logicConnector.HasRoomInHerd();
        }

        // Horses
        public static bool CheckBuyHorsesRules(ILogicConnector logicConnector, int buyoutPrice, int availablePlayerGold)
        {
            if (logicConnector.IsPackAnimal() && AutoTraderConfig.BuyHorsesValue)
            {
                // Buy pack horses rule
                if (logicConnector.HasRoomInHerd() && buyoutPrice * 2 < availablePlayerGold)
                    return true;

                // TODO: Add max herding setting
            }
            return false;
        }

        // Resupply hardwood
        public static bool CheckBuyResupplyHardwoodRule(ILogicConnector logicConnector, int currentAmount)
        {
            if (AutoTraderConfig.ResupplyHardwoodValue && logicConnector.IsItemHardwood())
            {
                // The maximum slider value means "keep all hardwood"
                if (AutoTraderConfig.KeepHardwoodValue >= AutoTraderConfig.MaxKeepHardwoodValue)
                    return true;
                return currentAmount < AutoTraderConfig.KeepHardwoodValue;
            }
            return false;
        }

        /// <summary>
        /// Resupply
        /// </summary>
        /// <param name="itemId"></param>
        /// <returns></returns>
        public static bool CheckBuyConsumablesRules(ILogicConnector logicConnector, int currentAmount)
        {
            if (AutoTraderConfig.ResupplyValue)
                return CheckBuyResupplyRule(logicConnector, currentAmount);
            return false;
        }

        /// <returns>
        /// True if the given item is below the restock value
        /// False if not
        /// </returns>
        public static bool CheckBuyResupplyRule(ILogicConnector logicConnector, int currentAmount)
        {
            // Find item stack in current inventory

            // Resupply grain
            if (logicConnector.IsItemGrain())
            {
                if (currentAmount < AutoTraderConfig.KeepGrainsMinValue)
                {
                    return true;
                }
                return false;
            }
            else
            {
                if (currentAmount < AutoTraderConfig.KeepConsumablesMinValue)
                {
                    return true;
                }
                return false;
            }
        }

    }
}
