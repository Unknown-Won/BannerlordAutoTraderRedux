using Helpers;
using System;
using System.Collections.Generic;
using System.Reflection;
using AutoTrader.Warsails;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.ComponentInterfaces;
using TaleWorlds.CampaignSystem.Conversation;
using TaleWorlds.CampaignSystem.Encounters;
using TaleWorlds.CampaignSystem.GameMenus;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Localization;

namespace AutoTrader
{
    public class TradeBehavior : CampaignBehaviorBase
    {
        private AutoTraderLogic _autoTraderLogic;

        public TradeBehavior(AutoTraderLogic autoTraderLogic)
        {
            _autoTraderLogic = autoTraderLogic;
        }

        public override void RegisterEvents()
        {
            CampaignEvents.OnSessionLaunchedEvent.AddNonSerializedListener(this, new Action<CampaignGameStarter>(this.OnGameLoaded));
            CampaignEvents.BeforeGameMenuOpenedEvent.AddNonSerializedListener(this, new Action<MenuCallbackArgs>(this.OnBeforeGameMenuOpened));
        }

        public override void SyncData(IDataStore dataStore)
        {
        }

        public void AutoTradeGoodsConsequence(MenuCallbackArgs args)
        {
            _autoTraderLogic.PerformAutoTrade(false);
        }

        public void AutoTradePortConsequence(MenuCallbackArgs args)
        {
            _autoTraderLogic.PerformAutoTrade(false, true);
        }

        public void AutoTradePortCargoHoldConsequence(MenuCallbackArgs args)
        {
            _autoTraderLogic.PerformAutoTrade(false, true, true, true);
        }

        public void AutoTradePortPartyInventoryConsequence(MenuCallbackArgs args)
        {
            _autoTraderLogic.PerformAutoTrade(false, false, false, true);
        }

        public void TradePortPartyInventoryConsequence(MenuCallbackArgs args)
        {
            InventoryScreenHelper.OpenScreenAsTrade(Settlement.CurrentSettlement.ItemRoster, Settlement.CurrentSettlement.Town,
                InventoryScreenHelper.InventoryCategoryType.None, null);
        }


        private void OnGameLoaded(CampaignGameStarter campaignGameStarter)
        {
            if (campaignGameStarter != null)
                this.AddDialogAndGameMenus(campaignGameStarter);
        }

        private void OnBeforeGameMenuOpened(MenuCallbackArgs args)
        {
            GameMenu menu = args?.MenuContext?.GameMenu;
            if (menu != null && menu.StringId == "port_menu")
                GroupPortTradeOptions(menu);
        }

        /// <summary>
        /// Moves the AutoTrader port options next to the port's own "Trade" option, so all trade buttons sit together.
        /// Runs each time the port menu opens, so options other mods add at any point are already there.
        /// </summary>
        private void GroupPortTradeOptions(GameMenu portMenu)
        {
            try
            {
                List<GameMenuOption> options = portMenu.MenuOptions as List<GameMenuOption>;
                if (options == null)
                    return;

                // Order: autotrade (cargo hold), autotrade (party inventory), Trade, Trade (party inventory)
                string[] beforeTradeIds = { "autotrade_port", "autotrade_port_cargo_hold", "autotrade_port_party" };
                string[] afterTradeIds = { "trade_port_party" };

                // The War Sails port "Trade" option (Cargo Holds reroutes it to the cargo hold)
                GameMenuOption tradeOption = options.Find(o => IsPortTradeOption(o, beforeTradeIds, afterTradeIds));
                if (tradeOption == null)
                {
                    AutoTraderHelpers.PrintDebugMessage("GroupPortTradeOptions: port trade option not found");
                    return;
                }

                // With Cargo Holds the port "Trade" opens the cargo hold, so label it like our buttons
                if (CargoHoldsCompat.IsAvailable)
                    RenameMenuOption(tradeOption, new TextObject("{=ATManualTradeCargoHold}Trade (cargo hold)", null));

                var before = new List<GameMenuOption>();
                foreach (string id in beforeTradeIds)
                {
                    GameMenuOption option = options.Find(o => o.IdString == id);
                    if (option != null)
                    {
                        options.Remove(option);
                        before.Add(option);
                    }
                }
                var after = new List<GameMenuOption>();
                foreach (string id in afterTradeIds)
                {
                    GameMenuOption option = options.Find(o => o.IdString == id);
                    if (option != null)
                    {
                        options.Remove(option);
                        after.Add(option);
                    }
                }

                int tradeIndex = options.IndexOf(tradeOption);
                options.InsertRange(tradeIndex + 1, after);
                options.InsertRange(tradeIndex, before);
            }
            catch (Exception ex)
            {
                AutoTraderHelpers.PrintDebugMessage("GroupPortTradeOptions: " + ex.Message);
            }
        }

        private static bool _renameFailureReported = false;

        private static void RenameMenuOption(GameMenuOption option, TextObject text)
        {
            try
            {
                // GameMenuOption.Text has a non-public setter
                var setter = typeof(GameMenuOption).GetProperty("Text", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)?.GetSetMethod(true);
                if (setter != null)
                    setter.Invoke(option, new object[] { text });

                // Fall back to the compiler-generated backing field
                if (option.Text != text)
                {
                    var field = typeof(GameMenuOption).GetField("<Text>k__BackingField", BindingFlags.NonPublic | BindingFlags.Instance);
                    if (field != null)
                        field.SetValue(option, text);
                }

                if (option.Text != text)
                    throw new InvalidOperationException("Text could not be set");
            }
            catch (Exception ex)
            {
                if (!_renameFailureReported)
                {
                    _renameFailureReported = true;
                    AutoTraderHelpers.PrintMessage("AutoTrader: could not rename the port Trade option: " + ex.Message);
                }
            }
        }

        private static bool IsPortTradeOption(GameMenuOption option, string[] beforeTradeIds, string[] afterTradeIds)
        {
            if (option == null || Array.IndexOf(beforeTradeIds, option.IdString) >= 0 || Array.IndexOf(afterTradeIds, option.IdString) >= 0)
                return false;
            if (option.OnConsequence != null && option.OnConsequence.Method.Name == "trade_on_consequence")
                return true;
            return option.IdString != null && option.IdString.IndexOf("trade", StringComparison.OrdinalIgnoreCase) >= 0
                && option.OptionLeaveType == GameMenuOption.LeaveType.Trade;
        }

        private bool AutoTradeGoodsCondition(MenuCallbackArgs args)
        {
            bool shouldBeDisabled;
            TextObject disabledText;
            bool canPlayerDo = Campaign.Current.Models.SettlementAccessModel.CanMainHeroDoSettlementAction(Settlement.CurrentSettlement, SettlementAccessModel.SettlementAction.Trade, out shouldBeDisabled, out disabledText);
            args.optionLeaveType = GameMenuOption.LeaveType.Trade;
            return MenuHelper.SetOptionProperties(args, canPlayerDo, shouldBeDisabled, disabledText);
        }

        private bool AutoTradeGoodsVillageCondition(MenuCallbackArgs args)
        {
            args.optionLeaveType = GameMenuOption.LeaveType.Trade;
            return MenuHelper.SetOptionProperties(args, true, false, new TextObject("", null));
        }

        private bool AutoTradeGoodsCaravanCondition()
        {
            return true;
        }

        private void AutoTradeGoodsCaravanConsequence()
        {
            _autoTraderLogic.PerformAutoTrade(true);
            PlayerEncounter.LeaveEncounter = true;
        }

        private void AddDialogAndGameMenus(CampaignGameStarter campaignGameStarter)
        {
            if (campaignGameStarter != null)
            {
                campaignGameStarter.AddGameMenuOption("town", "autotrader_town", new TextObject("{=ATTrade}Automatically trade wares", null).ToString(),
                    new GameMenuOption.OnConditionDelegate(this.AutoTradeGoodsCondition),
                    new GameMenuOption.OnConsequenceDelegate(this.AutoTradeGoodsConsequence), false, 4, false);
                campaignGameStarter.AddGameMenuOption("village", "autotrader_village", new TextObject("{=ATTrade}Automatically trade wares", null).ToString(),
                    new GameMenuOption.OnConditionDelegate(this.AutoTradeGoodsVillageCondition),
                    new GameMenuOption.OnConsequenceDelegate(this.AutoTradeGoodsConsequence), false, 4, false);

                // Caravan
                campaignGameStarter.AddPlayerLine("caravan_buy_products", "caravan_talk",
                    "close_window", "{=ATCaravan}I'd like to inspect your wares. (Autotrade)", 
                    new ConversationSentence.OnConditionDelegate(this.AutoTradeGoodsCaravanCondition),
                    new ConversationSentence.OnConsequenceDelegate(this.AutoTradeGoodsCaravanConsequence), 100, null, null);

                // Naval Campaign
                campaignGameStarter.AddGameMenuOption("naval_storyline_virtualport", "autotrade_naval_campaign", "{=ATTrade}Automatically trade wares", 
                    new GameMenuOption.OnConditionDelegate(this.AutoTradeNavalCampaignCondition), new GameMenuOption.OnConsequenceDelegate(this.AutoTradePortConsequence), false, 4, false, null);

                // Ports
                campaignGameStarter.AddGameMenuOption("port_menu", "autotrade_port", "{=ATTrade}Automatically trade wares",
                    new GameMenuOption.OnConditionDelegate(this.AutoTradePortCondition), new GameMenuOption.OnConsequenceDelegate(this.AutoTradePortConsequence), false, 4, false, null);

                // Ports with Cargo Holds: trade with the fleet's hold or with the party inventory
                campaignGameStarter.AddGameMenuOption("port_menu", "autotrade_port_cargo_hold", "{=ATTradeCargoHold}Automatically trade wares (cargo hold)",
                    new GameMenuOption.OnConditionDelegate(this.AutoTradePortCargoHoldCondition), new GameMenuOption.OnConsequenceDelegate(this.AutoTradePortCargoHoldConsequence), false, 4, false, null);
                campaignGameStarter.AddGameMenuOption("port_menu", "autotrade_port_party", "{=ATTradePartyInventory}Automatically trade wares (party inventory)",
                    new GameMenuOption.OnConditionDelegate(this.PortPartyInventoryCondition), new GameMenuOption.OnConsequenceDelegate(this.AutoTradePortPartyInventoryConsequence), false, 4, false, null);
                campaignGameStarter.AddGameMenuOption("port_menu", "trade_port_party", "{=ATManualTradePartyInventory}Trade (party inventory)",
                    new GameMenuOption.OnConditionDelegate(this.PortPartyInventoryCondition), new GameMenuOption.OnConsequenceDelegate(this.TradePortPartyInventoryConsequence), false, 4, false, null);
            }
        }

        private bool AutoTradeNavalCampaignCondition(MenuCallbackArgs args)
        {
            Settlement currentSettlement = Settlement.CurrentSettlement;
            if (currentSettlement == null || currentSettlement.IsUnderSiege)
            {
                return false;
            }

            bool shouldBeDisabled;
            TextObject disabledText;
            bool canPlayerDo = Campaign.Current.Models.SettlementAccessModel.CanMainHeroDoSettlementAction(Settlement.CurrentSettlement, SettlementAccessModel.SettlementAction.Trade, out shouldBeDisabled, out disabledText);
            args.optionLeaveType = GameMenuOption.LeaveType.Trade;
            return MenuHelper.SetOptionProperties(args, canPlayerDo, shouldBeDisabled, disabledText);
        }

        private bool AutoTradePortCondition(MenuCallbackArgs args)
        {
            // With Cargo Holds the port gets separate cargo hold / party inventory buttons instead
            if (CargoHoldsCompat.IsAvailable)
                return false;
            return AutoTradeNavalCampaignCondition(args);
        }

        private bool AutoTradePortCargoHoldCondition(MenuCallbackArgs args)
        {
            if (!CargoHoldsCompat.IsAvailable || !AutoTradeNavalCampaignCondition(args) || !Settlement.CurrentSettlement.IsTown)
                return false;

            // The cargo hold is only reachable while the fleet is docked here (same rule as Cargo Holds' own trade button)
            if (!CargoHoldsCompat.IsPlayerFleetDockedAt(Settlement.CurrentSettlement))
            {
                args.IsEnabled = false;
                args.Tooltip = new TextObject("{=ATFleetNotDocked}Your fleet is not docked at this port.", null);
            }
            return true;
        }

        private bool PortPartyInventoryCondition(MenuCallbackArgs args)
        {
            if (!CargoHoldsCompat.IsAvailable || !AutoTradeNavalCampaignCondition(args))
                return false;
            return Settlement.CurrentSettlement.IsTown;
        }

    }
}
