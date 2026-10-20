using System;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.ScreenSystem;

namespace AutoTrader
{
    class AutoTraderMenuViewModel : ViewModel
    {
		private string _configMenuText;
		private string _doneText;
		private string _cancelText;
		private string _lockHintText;
		private string _buyText;
		private string _sellText;
		private string _defaultText;
		private string _infText;

		private string _presetsText;
		private string _presetsDefaultText;
		private string _presetsSellerText;
		private string _presetsGlobalText;
		private string _presetsLocalText;

		private string _headerBaseText;
		private string _headerCustomText;
		private string _headerSpecialText;
		private string _headerTradeText;

		private string _useWeightedText;
		private string _simpleAIText;
		private string _buyThresholdText;
		private string _sellThresholdText;
		private string _maxCapacityText;
		private string _keepGrainsMinText;
		private string _keepGrainsMaxText;
		private string _keepConsumablesMinText;
		private string _keepConsumablesMaxText;
		private string _useInventorySpaceText;
		private string _keepHardwoodText;
		private string _hardwoodBuyThresholdText;
		private string _keepWagesText;
		private string _searchRadiusText;
		private string _weaponsArmorTierText;

		private string _useAltATText;
		private string _junkCattleText;
		private string _sellFromBothRostersText;
		private string _sellSmithingText;
		private string _keepSmeltingText;
		private string _resupplyHardwoodText;
		private string _mountTroopsText;
		private string _mountTroopsWarText;
		private string _tradeMountsText;
		private string _keepWarMountsText;
		private string _minBuyProfitText;
		private string _buyProfitOverrideText;
		private string _resupplyText;

		private string _tradeHorsesText;
		private string _tradeWeaponsText;
		private string _tradeArmorText;
		private string _tradeGoodsText;
		private string _tradeConsumablesText;
		private string _tradeLivestockText;


		private float _buyThresholdValue;
		private string _buyThresholdValueAsString;

		private float _sellThresholdValue;
		private string _sellThresholdValueAsString;

		private float _maxCapacityValue;
		private string _maxCapacityValueAsString;

		private float _keepGrainsMinValue;
		private float _keepGrainsMaxValue;
		private string _keepGrainsMinValueAsString;
		private string _keepGrainsMaxValueAsString;

		private float _keepConsumablesMinValue;
		private float _keepConsumablesMaxValue;
		private string _keepConsumablesMinValueAsString;
		private string _keepConsumablesMaxValueAsString;

		private float _useInventorySpaceValue;
		private string _useInventorySpaceValueAsString;

		private float _keepHardwoodValue;
		private string _keepHardwoodValueAsString;
		private float _hardwoodBuyThresholdValue;
		private string _hardwoodBuyThresholdValueAsString;

		private float _keepWagesValue;
		private string _keepWagesValueAsString;

		private float _searchRadiusValue;
		private string _searchRadiusValueAsString;

		private float _weaponsArmorTierValue;
		private string _weaponsArmorTierValueAsString;

		private bool _useWeightedValue;
		private bool _simpleAIValue;

		private bool _useAltATValue;
		private bool _junkCattleValue;
		private bool _sellFromBothRostersValue;
		private bool _sellSmithingValue;
		private bool _keepSmeltingValue;
		private bool _resupplyHardwoodValue;
		private bool _mountTroopsValue;
		private bool _mountTroopsWarValue;
		private bool _buyMountsValue;
		private bool _sellMountsValue;
		private float _keepWarMountsValue;
		private string _keepWarMountsValueAsString;
		private string _keepSpareMountsText;
		private string _maxHerdTradePercentText;
		private string _maxHorseGoldPerTypeText;
		private string _tabGeneralText;
		private string _tabTradingText;
		private string _tabSuppliesText;
		private string _headerPricesText;
		private string _headerGoldSpaceText;
		private string _headerOtherText;
		private string _headerBuyingText;
		private string _headerSellingText;
		private string _headerWhatToTradeText;
		private string _headerFoodText;
		private string _headerHardwoodText;
		private string _headerHorsesText;
		private string _headerResupplyText;
		private string _headerTroopHorsesText;
		private float _keepSpareMountsValue;
		private string _keepSpareMountsValueAsString;
		private float _maxHerdTradePercentValue;
		private string _maxHerdTradePercentValueAsString;
		private float _maxHorseGoldPerTypeValue;
		private string _maxHorseGoldPerTypeValueAsString;

		// Settings tabs
		private const int TabGeneral = 0;
		private const int TabTrading = 1;
		private const int TabSupplies = 2;
		private const int TabPorts = 3;
		private string _portUseInventorySpaceText;
		private string _portMaxCapacityText;
		private string _portMaxItemCountText;
		private string _tabPortsText;
		private string _headerPortsText;
		private float _portUseInventorySpaceValue;
		private string _portUseInventorySpaceValueAsString;
		private float _portMaxCapacityValue;
		private string _portMinBuyProfitText;
		private float _portMinBuyProfitValue;
		private string _portMinBuyProfitValueAsString;
		private string _portMaxCapacityValueAsString;
		// In steps of 10 items
		private float _portMaxItemCountValue;
		private string _portMaxItemCountValueAsString;
		private int _selectedTab = TabGeneral;
		private float _minBuyProfitValue;
		private string _minBuyProfitValueAsString;
		// Slider in hundreds of gold
		private float _buyProfitOverrideValue;
		private string _buyProfitOverrideValueAsString;
		private bool _resupplyValue;

		private bool _buyHorsesValue;
		private bool _sellHorsesValue;
		private bool _buyWeaponsValue;
		private bool _sellWeaponsValue;
		private bool _buyArmorValue;
		private bool _sellArmorValue;
		private bool _buyGoodsValue;
		private bool _sellGoodsValue;
		private bool _buyConsumablesValue;
		private bool _sellConsumablesValue;
		private bool _buyLivestockValue;
		private bool _sellLivestockValue;
		private bool _isWeightedActive;
		private bool _isSearchRadiusActive;

		private Action _closeMenuScreen;

		public AutoTraderMenuViewModel(Action closeMenuScreen)
        {
			this._closeMenuScreen = closeMenuScreen;

			this._configMenuText = new TextObject("{=ATOptions}AutoTrader Configuration", null).ToString();
			this._doneText = new TextObject("{=ATDone}Done", null).ToString();
			this._cancelText = new TextObject("{=ATCancel}Cancel", null).ToString();
			this._lockHintText = new TextObject("{=ATLockHint}Note: Lock items to prevent them from being sold.", null).ToString();
			this._buyText = new TextObject("{=ATBuy}Buy", null).ToString();
			this._sellText = new TextObject("{=ATSell}Sell", null).ToString();
			this._defaultText = new TextObject("{=ATDefault}Default: ", null).ToString();
			this._infText = new TextObject("{=ATInf}Inf", null).ToString();

			this._presetsText = new TextObject("{=ATPresets}Presets", null).ToString();
			this._presetsDefaultText = new TextObject("{=ATPreDefault}Default", null).ToString();
			this._presetsSellerText = new TextObject("{=ATPreSeller}Loot Seller", null).ToString();
			this._presetsGlobalText = new TextObject("{=ATPreGlobal}Global Trade", null).ToString();
			this._presetsLocalText = new TextObject("{=ATPreLocal}Local Trade", null).ToString();

			this._headerBaseText = new TextObject("{=ATHeaderBase}Basic Options", null).ToString();
			this._headerCustomText = new TextObject("{=ATHeaderCustom}User Preferences", null).ToString();
			this._headerSpecialText = new TextObject("{=ATHeaderSpecial}Special Options", null).ToString();
			this._headerTradeText = new TextObject("{=ATHeaderTrade}Buy / Sell Options", null).ToString();

			this._useWeightedText = new TextObject("{=ATWeightedRadius}Weight average price by distance", null).ToString();
			this._simpleAIText = new TextObject("{=ATSimpleAI}Green/red-based trading (improves with trade rumors)", null).ToString();
			this._buyThresholdText = new TextObject("{=ATBuyThreshold}Buy under X% of the average sell price", null).ToString();
			this._sellThresholdText = new TextObject("{=ATSellThreshold}Sell above X% of the average sell price", null).ToString();
			this._maxCapacityText = new TextObject("{=ATMaxCapacity}Maximum % of inventory filled by the same good", null).ToString();
			this._keepGrainsMinText = new TextObject("{=ATKeepGrainsMin}Minimum amount of grain to keep", null).ToString();
			this._keepGrainsMaxText = new TextObject("{=ATKeepGrainsMax}Maximum amount of grain to keep", null).ToString();
			this._keepConsumablesMinText = new TextObject("{=ATKeepConsumablesMin}Minimum amount of other consumables to keep", null).ToString();
			this._keepConsumablesMaxText = new TextObject("{=ATKeepConsumablesMax}Maximum amount of other consumables to keep", null).ToString();
			this._useInventorySpaceText = new TextObject("{=ATUseInventorySpace}Total % of inventory space the mod may use", null).ToString();
			this._keepHardwoodText = new TextObject("{=ATKeepHardwood}Amount of hardwood to keep", null).ToString();
			this._hardwoodBuyThresholdText = new TextObject("{=ATHardwoodBuyThreshold}Buy hardwood to keep under % of the average buying price", null).ToString();
			this._keepWagesText = new TextObject("{=ATKeepWages}Keep enough gold for X days of troop wages", null).ToString();
			this._searchRadiusText = new TextObject("{=ATSearchRadius}Average price search radius", null).ToString();
			this._weaponsArmorTierText = new TextObject("{=ATWeaponsArmorTier}Sell weapons and armor of tier X and below", null).ToString();

			this._useAltATText = new TextObject("{=ATAltAT}Use hotkey <Alt + A + T> instead of <Alt + A>", null).ToString();
			this._junkCattleText = new TextObject("{=ATJunkCattle}Always sell all cattle", null).ToString();
			this._sellFromBothRostersText = new TextObject("{=ATSellFromBothRosters}Ports: also sell from the other inventory", null).ToString();
			this._sellSmithingText = new TextObject("{=ATSellSmithing}Sell charcoal and ingots", null).ToString();
			this._keepSmeltingText = new TextObject("{=ATKeepSmelting}Keep smeltable weapons", null).ToString();
			this._resupplyHardwoodText = new TextObject("{=ATResupplyHardwood}Resupply hardwood to 'hardwood to keep' value", null).ToString();
			this._mountTroopsText = new TextObject("{=ATMountTroops}Buy and keep riding horses for troops on foot", null).ToString();
			this._mountTroopsWarText = new TextObject("{=ATMountTroopsWar}Buy and keep war mounts for troops on foot", null).ToString();
			this._tradeMountsText = new TextObject("{=ATTradeMounts}Trade Riding Horses", null).ToString();
			this._keepWarMountsText = new TextObject("{=ATKeepWarMounts}Minimum war mounts to keep", null).ToString();
			this._keepSpareMountsText = new TextObject("{=ATKeepSpareMounts}Minimum spare riding horses to keep", null).ToString();
			this._maxHerdTradePercentText = new TextObject("{=ATMaxHerdTradePercent}Total % of herd size the mod may use", null).ToString();
			this._maxHorseGoldPerTypeText = new TextObject("{=ATMaxHorseGoldPerType}Maximum gold in each horse type for trading", null).ToString();
			this._tabGeneralText = new TextObject("{=ATTabGeneral}General", null).ToString();
			this._tabTradingText = new TextObject("{=ATTabTrading}Trading", null).ToString();
			this._tabSuppliesText = new TextObject("{=ATTabSupplies}Supplies & Horses", null).ToString();
			this._portUseInventorySpaceText = new TextObject("{=ATPortUseInventorySpace}Total % of cargo hold space the mod may use", null).ToString();
			this._portMaxCapacityText = new TextObject("{=ATPortMaxCapacity}Maximum % of cargo hold filled by the same good", null).ToString();
			this._portMaxItemCountText = new TextObject("{=ATPortMaxItemCount}Maximum amount of the same good in the cargo hold", null).ToString();
			this._portMinBuyProfitText = new TextObject("{=ATPortMinBuyProfit}Minimum profit per 10 weight to buy in the cargo hold", null).ToString();
			this._tabPortsText = new TextObject("{=ATTabPorts}Ports", null).ToString();
			this._headerPortsText = new TextObject("{=ATHeaderPorts}Ports", null).ToString();
			this._headerPricesText = new TextObject("{=ATHeaderPrices}Prices", null).ToString();
			this._headerGoldSpaceText = new TextObject("{=ATHeaderGoldSpace}Gold and Space", null).ToString();
			this._headerOtherText = new TextObject("{=ATHeaderOther}Hotkey", null).ToString();
			this._headerBuyingText = new TextObject("{=ATHeaderBuying}Buying", null).ToString();
			this._headerSellingText = new TextObject("{=ATHeaderSelling}Selling", null).ToString();
			this._headerWhatToTradeText = new TextObject("{=ATHeaderWhatToTrade}What to Trade", null).ToString();
			this._headerFoodText = new TextObject("{=ATHeaderFood}Food", null).ToString();
			this._headerHardwoodText = new TextObject("{=ATHeaderHardwood}Hardwood", null).ToString();
			this._headerHorsesText = new TextObject("{=ATHeaderHorses}Horses and Herd", null).ToString();
			this._headerResupplyText = new TextObject("{=ATHeaderResupply}Resupply", null).ToString();
			this._headerTroopHorsesText = new TextObject("{=ATHeaderTroopHorses}Horses for Troops", null).ToString();
			this._minBuyProfitText = new TextObject("{=ATMinBuyProfit}Minimum profit per 10 weight to buy", null).ToString();
			this._buyProfitOverrideText = new TextObject("{=ATBuyProfitOverride}Buy anyway if profit per item is at least", null).ToString();
			this._resupplyText = new TextObject("{=ATResupply}Resupply food to 'keep consumables' value", null).ToString();

			this._tradeHorsesText = new TextObject("{=ATTradeHorses}Trade Pack Horses (TBD)", null).ToString();
			this._tradeWeaponsText = new TextObject("{=ATTradeWeapons}Trade Weapons (sells below Tier X)", null).ToString();
			this._tradeArmorText = new TextObject("{=ATTradeArmor}Trade Armor (sells below Tier X)", null).ToString();
			this._tradeGoodsText = new TextObject("{=ATTradeGoods}Trade Goods", null).ToString();
			this._tradeConsumablesText = new TextObject("{=ATTradeConsumables}Trade Consumables", null).ToString();
			this._tradeLivestockText = new TextObject("{=ATTradeLivestock}Trade Livestock", null).ToString();

			this._useWeightedValue = AutoTraderConfig.UseWeightedValue;
			this._simpleAIValue = AutoTraderConfig.SimpleTradingAI;
			this._buyThresholdValue = AutoTraderConfig.BuyThresholdValue;
			this._sellThresholdValue = AutoTraderConfig.SellThresholdValue;
			this._maxCapacityValue = AutoTraderConfig.MaxCapacityValue;
			this._keepGrainsMinValue = AutoTraderConfig.KeepGrainsMinValue;
			this._keepGrainsMaxValue = AutoTraderConfig.KeepGrainsMaxValue;
			this._keepConsumablesMinValue = AutoTraderConfig.KeepConsumablesMinValue;
			this._keepConsumablesMaxValue = AutoTraderConfig.KeepConsumablesMaxValue;
			this._useInventorySpaceValue = AutoTraderConfig.UseInventorySpaceValue;
			this._keepHardwoodValue = AutoTraderConfig.KeepHardwoodValue;
			this._hardwoodBuyThresholdValue = AutoTraderConfig.HardwoodBuyThresholdValue;
			this._keepWagesValue = AutoTraderConfig.KeepWagesValue;
			this._searchRadiusValue = AutoTraderConfig.SearchRadiusValue;
			this._weaponsArmorTierValue = AutoTraderConfig.WeaponsArmorTierValue;

			this._buyHorsesValue = AutoTraderConfig.BuyHorsesValue;
			this._buyWeaponsValue = AutoTraderConfig.BuyWeaponsValue;
			this._buyArmorValue = AutoTraderConfig.BuyArmorValue;
			this._buyGoodsValue = AutoTraderConfig.BuyGoodsValue;
			this._buyConsumablesValue = AutoTraderConfig.BuyConsumablesValue;
			this._buyLivestockValue = AutoTraderConfig.BuyLivestockValue;

			this._useAltATValue = AutoTraderConfig.UseAltATValue;
			this._junkCattleValue = AutoTraderConfig.JunkCattleValue;
			this._sellFromBothRostersValue = AutoTraderConfig.SellFromBothRostersValue;
			this._sellSmithingValue = AutoTraderConfig.SellSmithingValue;
			this._keepSmeltingValue = AutoTraderConfig.KeepSmeltingValue;
			this._resupplyHardwoodValue = AutoTraderConfig.ResupplyHardwoodValue;
			this._mountTroopsValue = AutoTraderConfig.MountTroopsValue;
			this._mountTroopsWarValue = AutoTraderConfig.MountTroopsWarValue;
			this._buyMountsValue = AutoTraderConfig.BuyMountsValue;
			this._sellMountsValue = AutoTraderConfig.SellMountsValue;
			this._keepWarMountsValue = AutoTraderConfig.KeepWarMountsValue;
			this._keepSpareMountsValue = AutoTraderConfig.KeepSpareMountsValue;
			this._maxHerdTradePercentValue = AutoTraderConfig.MaxHerdTradePercentValue;
			this._maxHorseGoldPerTypeValue = AutoTraderConfig.MaxHorseGoldPerTypeValue / 5000;
			this._portUseInventorySpaceValue = AutoTraderConfig.PortUseInventorySpaceValue;
			this._portMaxCapacityValue = AutoTraderConfig.PortMaxCapacityValue;
			this._portMinBuyProfitValue = AutoTraderConfig.PortMinBuyProfitValue;
			this._portMaxItemCountValue = AutoTraderConfig.PortMaxItemCountValue / 10;
			this._minBuyProfitValue = AutoTraderConfig.MinBuyProfitValue;
			this._buyProfitOverrideValue = AutoTraderConfig.BuyProfitOverrideValue / 100.0f;
			this._resupplyValue = AutoTraderConfig.ResupplyValue;

			this._sellHorsesValue = AutoTraderConfig.SellHorsesValue;
			this._sellWeaponsValue = AutoTraderConfig.SellWeaponsValue;
			this._sellArmorValue = AutoTraderConfig.SellArmorValue;
			this._sellGoodsValue = AutoTraderConfig.SellGoodsValue;
			this._sellConsumablesValue = AutoTraderConfig.SellConsumablesValue;
			this._sellLivestockValue = AutoTraderConfig.SellLivestockValue;

			this._isWeightedActive = !this._simpleAIValue;
			this._isSearchRadiusActive = (!this._simpleAIValue && !this._useWeightedValue);

			this.RefreshValues();
		}

        public override void RefreshValues()
        {
            base.RefreshValues();
			this.BuyThresholdValueAsString = this.BuyThresholdValue.ToString();
			this.SellThresholdValueAsString = this.SellThresholdValue.ToString();
			this.MaxCapacityValueAsString = this.MaxCapacityValue.ToString();
			this.KeepGrainsMinValueAsString = this.KeepGrainsMinValue.ToString();
			if (this.KeepGrainsMaxValue >= AutoTraderConfig.MaxKeepGrainsValue)
				this.KeepGrainsMaxValueAsString = new TextObject("{=ATAll}All", null).ToString();
			else
				this.KeepGrainsMaxValueAsString = this.KeepGrainsMaxValue.ToString();
			this.KeepConsumablesMinValueAsString = this.KeepConsumablesMinValue.ToString();
			this.KeepConsumablesMaxValueAsString = this.KeepConsumablesMaxValue.ToString();
			this.KeepHardwoodValueAsString = this.GetKeepHardwoodValueAsString();
			this.HardwoodBuyThresholdValueAsString = this.HardwoodBuyThresholdValue.ToString();
			this.UseInventorySpaceValueAsString = this.UseInventorySpaceValue.ToString();
			this.KeepWagesValueAsString = this.KeepWagesValue.ToString();
			this.KeepWarMountsValueAsString = this.KeepWarMountsValue.ToString();
			this.KeepSpareMountsValueAsString = this.KeepSpareMountsValue.ToString();
			this.MaxHerdTradePercentValueAsString = this.MaxHerdTradePercentValue.ToString();
			this.MaxHorseGoldPerTypeValueAsString = GetMaxHorseGoldPerTypeAsString();
			this.PortUseInventorySpaceValueAsString = this.PortUseInventorySpaceValue.ToString();
			this.PortMaxCapacityValueAsString = this.PortMaxCapacityValue.ToString();
			this.PortMinBuyProfitValueAsString = this.PortMinBuyProfitValue.ToString();
			this.PortMaxItemCountValueAsString = GetPortMaxItemCountAsString();
			this.MinBuyProfitValueAsString = this.MinBuyProfitValue.ToString();
			this.BuyProfitOverrideValueAsString = GetBuyProfitOverrideAsString();
			if (this.SearchRadiusValue > 999.0f)
				this.SearchRadiusValueAsString = this.InfText;
			else
				this.SearchRadiusValueAsString = this.SearchRadiusValue.ToString();
			this.WeaponsArmorTierValueAsString = this.WeaponsArmorTierValue.ToString();

			this.IsWeightedActive = (!this._simpleAIValue);
			this.IsSearchRadiusActive = (!this._simpleAIValue && !this._useWeightedValue);
		}

		private void ExecuteDone()
		{
			AutoTraderConfig.UseWeightedValue = this._useWeightedValue;
			AutoTraderConfig.SimpleTradingAI = this._simpleAIValue;
			AutoTraderConfig.BuyThresholdValue = (int)this.BuyThresholdValue;
			AutoTraderConfig.SellThresholdValue = (int)this.SellThresholdValue;
			AutoTraderConfig.MaxCapacityValue = (int)this.MaxCapacityValue;
			AutoTraderConfig.KeepGrainsMinValue = (int)this.KeepGrainsMinValue;
			AutoTraderConfig.KeepGrainsMaxValue = (int)this.KeepGrainsMaxValue;
			AutoTraderConfig.KeepConsumablesMinValue = (int)this.KeepConsumablesMinValue;
			AutoTraderConfig.KeepConsumablesMaxValue = (int)this.KeepConsumablesMaxValue;
			AutoTraderConfig.UseInventorySpaceValue = (int)this.UseInventorySpaceValue;
			AutoTraderConfig.KeepHardwoodValue = (int)this.KeepHardwoodValue;
			AutoTraderConfig.HardwoodBuyThresholdValue = (int)this.HardwoodBuyThresholdValue;
			AutoTraderConfig.KeepWagesValue = (int)this.KeepWagesValue;
			AutoTraderConfig.SearchRadiusValue = (int)this.SearchRadiusValue;
			AutoTraderConfig.WeaponsArmorTierValue = (int)this.WeaponsArmorTierValue;

			AutoTraderConfig.BuyHorsesValue = this.BuyHorsesValue;
			AutoTraderConfig.BuyArmorValue = this.BuyArmorValue;
			AutoTraderConfig.BuyWeaponsValue = this.BuyWeaponsValue;
			AutoTraderConfig.BuyGoodsValue = this.BuyGoodsValue;
			AutoTraderConfig.BuyConsumablesValue = this.BuyConsumablesValue;
			AutoTraderConfig.BuyLivestockValue = this.BuyLivestockValue;

			AutoTraderConfig.UseAltATValue = this.UseAltATValue;
			AutoTraderConfig.JunkCattleValue = this.JunkCattleValue;
			AutoTraderConfig.SellFromBothRostersValue = this.SellFromBothRostersValue;
			AutoTraderConfig.SellSmithingValue = this.SellSmithingValue;
			AutoTraderConfig.KeepSmeltingValue = this.KeepSmeltingValue;
			AutoTraderConfig.ResupplyHardwoodValue = this.ResupplyHardwoodValue;
			AutoTraderConfig.MountTroopsValue = this.MountTroopsValue;
			AutoTraderConfig.MountTroopsWarValue = this.MountTroopsWarValue;
			AutoTraderConfig.BuyMountsValue = this.BuyMountsValue;
			AutoTraderConfig.SellMountsValue = this.SellMountsValue;
			AutoTraderConfig.KeepWarMountsValue = (int)this.KeepWarMountsValue;
			AutoTraderConfig.KeepSpareMountsValue = (int)this.KeepSpareMountsValue;
			AutoTraderConfig.MaxHerdTradePercentValue = (int)this.MaxHerdTradePercentValue;
			AutoTraderConfig.MaxHorseGoldPerTypeValue = (int)this.MaxHorseGoldPerTypeValue * 5000;
			AutoTraderConfig.PortUseInventorySpaceValue = (int)this.PortUseInventorySpaceValue;
			AutoTraderConfig.PortMaxCapacityValue = (int)this.PortMaxCapacityValue;
			AutoTraderConfig.PortMinBuyProfitValue = (int)this.PortMinBuyProfitValue;
			AutoTraderConfig.PortMaxItemCountValue = (int)this.PortMaxItemCountValue * 10;
			AutoTraderConfig.MinBuyProfitValue = (int)this.MinBuyProfitValue;
			AutoTraderConfig.BuyProfitOverrideValue = (int)Math.Round(this.BuyProfitOverrideValue * 100.0f);
			AutoTraderConfig.ResupplyValue = this.ResupplyValue;

			AutoTraderConfig.SellHorsesValue = this.SellHorsesValue;
			AutoTraderConfig.SellArmorValue = this.SellArmorValue;
			AutoTraderConfig.SellWeaponsValue = this.SellWeaponsValue;
			AutoTraderConfig.SellGoodsValue = this.SellGoodsValue;
			AutoTraderConfig.SellConsumablesValue = this.SellConsumablesValue;
			AutoTraderConfig.SellLivestockValue = this.SellLivestockValue;

			AutoTraderConfig.Save();

			if (this._closeMenuScreen != null)
				this._closeMenuScreen();
			else
				ScreenManager.PopScreen();
		}

		private void ExecuteTabGeneral()
		{
			this.SelectTab(TabGeneral);
		}

		private void ExecuteTabTrading()
		{
			this.SelectTab(TabTrading);
		}

		private void ExecuteTabSupplies()
		{
			this.SelectTab(TabSupplies);
		}

		private void ExecuteTabPorts()
		{
			this.SelectTab(TabPorts);
		}

		private void SelectTab(int tab)
		{
			this._selectedTab = tab;
			base.OnPropertyChanged("IsGeneralTab");
			base.OnPropertyChanged("IsTradingTab");
			base.OnPropertyChanged("IsSuppliesTab");
			base.OnPropertyChanged("IsPortsTab");
		}

		private string GetMaxHorseGoldPerTypeAsString()
		{
			if (this.MaxHorseGoldPerTypeValue * 5000 >= AutoTraderConfig.MaxHorseGoldPerTypeInf)
				return this.InfText;
			return (this.MaxHorseGoldPerTypeValue * 5000).ToString();
		}

		private string GetPortMaxItemCountAsString()
		{
			if (this.PortMaxItemCountValue * 10 >= AutoTraderConfig.PortMaxItemCountInf)
				return this.InfText;
			return (this.PortMaxItemCountValue * 10).ToString();
		}

		private void ExecuteCancel()
		{
			if (this._closeMenuScreen != null)
				this._closeMenuScreen();
			else
				ScreenManager.PopScreen();
		}

		private void ExecutePresetDefault()
		{
			this.SimpleAIValue = false;

			this.BuyThresholdValue = 75.0f;
			this.SellThresholdValue = 90.0f;
			this.UseWeightedValue = false;
			this.MaxCapacityValue = 20.0f;
			this.KeepGrainsMinValue = 10.0f;
			this.KeepGrainsMaxValue = 50.0f;
			this.KeepConsumablesMinValue = 4.0f;
			this.KeepConsumablesMaxValue = 50.0f;
			this.UseInventorySpaceValue = 90.0f;
			this.KeepHardwoodValue = 100.0f;
			this.HardwoodBuyThresholdValue = 110.0f;
			this.KeepWagesValue = 7.0f;
			this.SearchRadiusValue = 300.0f;
			this.WeaponsArmorTierValue = 2.0f;

			this.JunkCattleValue = false;
			this.SellFromBothRostersValue = true;
			this.SellSmithingValue = true;
			this.KeepSmeltingValue = true;
			this.ResupplyHardwoodValue = true;
			this.MountTroopsValue = true;
			this.MountTroopsWarValue = false;
			this.KeepWarMountsValue = 0.0f;
			this.KeepSpareMountsValue = 5.0f;
			this.MaxHerdTradePercentValue = 90.0f;
			this.MaxHorseGoldPerTypeValue = 15.0f;
			this.PortUseInventorySpaceValue = 100.0f;
			this.PortMaxCapacityValue = 30.0f;
			this.PortMinBuyProfitValue = 40.0f;
			this.PortMaxItemCountValue = 30.0f;
			this.MinBuyProfitValue = 50.0f;
			this.BuyProfitOverrideValue = 10.0f;
			this.ResupplyValue = true;

			this.BuyHorsesValue = true;
			this.SellHorsesValue = true;
			this.BuyMountsValue = true;
			this.SellMountsValue = true;
			this.BuyWeaponsValue = true;
			this.SellWeaponsValue = true;
			this.BuyArmorValue = true;
			this.SellArmorValue = true;
			this.BuyGoodsValue = true;
			this.SellGoodsValue = true;
			this.BuyConsumablesValue = true;
			this.SellConsumablesValue = true;
			this.BuyLivestockValue = true;
			this.SellLivestockValue = true;

			RefreshValues();
		}

		private void ExecutePresetSeller()
		{
			this.WeaponsArmorTierValue = 3.0f;
			this.SellSmithingValue = false;
			this.KeepSmeltingValue = false;
			this.BuyHorsesValue = false;
			this.SellHorsesValue = true;
			this.BuyMountsValue = false;
			this.SellMountsValue = true;
			this.BuyWeaponsValue = false;
			this.SellWeaponsValue = true;
			this.BuyArmorValue = false;
			this.SellArmorValue = true;
			this.BuyGoodsValue = false;
			this.SellGoodsValue = false;
			this.BuyConsumablesValue = false;
			this.SellConsumablesValue = false;
			this.BuyLivestockValue = false;
			this.SellLivestockValue = false;
			this.JunkCattleValue = true;

			RefreshValues();
		}

		private void ExecutePresetGlobal()
		{
			this.SimpleAIValue = false;

			this.SearchRadiusValue = 1000.0f;
			this.UseWeightedValue = false;

			this.BuyHorsesValue = true;
			this.BuyGoodsValue = true;
			this.SellGoodsValue = true;
			this.BuyConsumablesValue = true;
			this.SellConsumablesValue = true;

			RefreshValues();
		}

		private void ExecutePresetLocal()
		{
			this.SimpleAIValue = false;

			this.SearchRadiusValue = 300.0f;
			this.UseWeightedValue = true;

			this.BuyHorsesValue = true;
			this.BuyGoodsValue = true;
			this.SellGoodsValue = true;
			this.BuyConsumablesValue = true;
			this.SellConsumablesValue = true;

			RefreshValues();
		}

		[DataSourceProperty]
		public string UseWeightedText
		{
			get
			{
				return this._useWeightedText;
			}
		}

		[DataSourceProperty]
		public string ResupplyHardwoodText
		{
			get
			{
				return this._resupplyHardwoodText;
			}
		}

		[DataSourceProperty]
		public string MountTroopsWarText
		{
			get
			{
				return this._mountTroopsWarText;
			}
		}

		[DataSourceProperty]
		public string TradeMountsText
		{
			get
			{
				return this._tradeMountsText;
			}
		}

		[DataSourceProperty]
		public string KeepWarMountsText
		{
			get
			{
				return this._keepWarMountsText;
			}
		}

		[DataSourceProperty]
		public string BuyProfitOverrideText
		{
			get
			{
				return this._buyProfitOverrideText;
			}
		}

		[DataSourceProperty]
		public float BuyProfitOverrideValue
		{
			get
			{
				return this._buyProfitOverrideValue;
			}
			set
			{
				if (value != this._buyProfitOverrideValue)
				{
					this._buyProfitOverrideValue = value;
					this.BuyProfitOverrideValueAsString = GetBuyProfitOverrideAsString();
					base.OnPropertyChanged("BuyProfitOverrideValue");
				}
			}
		}

		[DataSourceProperty]
		public string BuyProfitOverrideValueAsString
		{
			get
			{
				return this._buyProfitOverrideValueAsString;
			}
			set
			{
				if (value != this._buyProfitOverrideValueAsString)
				{
					this._buyProfitOverrideValueAsString = value;
					base.OnPropertyChanged("BuyProfitOverrideValueAsString");
				}
			}
		}

		private string GetBuyProfitOverrideAsString()
		{
			int gold = (int)Math.Round(this._buyProfitOverrideValue * 100.0f);
			return gold > 0 ? gold.ToString() : "Off";
		}

		[DataSourceProperty]
		public string MinBuyProfitText
		{
			get
			{
				return this._minBuyProfitText;
			}
		}

		[DataSourceProperty]
		public string MountTroopsText
		{
			get
			{
				return this._mountTroopsText;
			}
		}

		[DataSourceProperty]
		public string SimpleAIText
		{
			get
			{
				return this._simpleAIText;
			}
		}

		[DataSourceProperty]
		public string ResupplyText
		{
			get
			{
				return this._resupplyText;
			}
		}

		[DataSourceProperty]
		public string InfText
		{
			get
			{
				return this._infText;
			}
		}

		[DataSourceProperty]
		public string ConfigMenuText
		{
			get
			{
				return this._configMenuText;
			}
		}

		[DataSourceProperty]
		public string DoneText
		{
			get
			{
				return this._doneText;
			}
		}

		[DataSourceProperty]
		public string CancelText
		{
			get
			{
				return this._cancelText;
			}
		}

		[DataSourceProperty]
		public string LockHintText
		{
			get
			{
				return this._lockHintText;
			}
		}

		[DataSourceProperty]
		public string BuyText
		{
			get
			{
				return this._buyText;
			}
		}

		[DataSourceProperty]
		public string SellText
		{
			get
			{
				return this._sellText;
			}
		}

		[DataSourceProperty]
		public string DefaultText
		{
			get
			{
				return this._defaultText;
			}
		}

		[DataSourceProperty]
		public string PresetsText
		{
			get
			{
				return this._presetsText;
			}
		}

		[DataSourceProperty]
		public string PresetsDefaultText
		{
			get
			{
				return this._presetsDefaultText;
			}
		}

		[DataSourceProperty]
		public string PresetsSellerText
		{
			get
			{
				return this._presetsSellerText;
			}
		}

		[DataSourceProperty]
		public string PresetsGlobalText
		{
			get
			{
				return this._presetsGlobalText;
			}
		}

		[DataSourceProperty]
		public string PresetsLocalText
		{
			get
			{
				return this._presetsLocalText;
			}
		}

		[DataSourceProperty]
		public string HeaderBaseText
		{
			get
			{
				return this._headerBaseText;
			}
		}

		[DataSourceProperty]
		public string HeaderCustomText
		{
			get
			{
				return this._headerCustomText;
			}
		}

		[DataSourceProperty]
		public string HeaderSpecialText
		{
			get
			{
				return this._headerSpecialText;
			}
		}

		[DataSourceProperty]
		public string HeaderTradeText
		{
			get
			{
				return this._headerTradeText;
			}
		}

		[DataSourceProperty]
		public string BuyThresholdText
		{
			get
			{
				return this._buyThresholdText;
			}
		}

		[DataSourceProperty]
		public string SellThresholdText
		{
			get
			{
				return this._sellThresholdText;
			}
		}

		[DataSourceProperty]
		public string MaxCapacityText
		{
			get
			{
				return this._maxCapacityText;
			}
		}

		[DataSourceProperty]
		public string UseInventorySpaceText
		{
			get
			{
				return this._useInventorySpaceText;
			}
		}

		[DataSourceProperty]
		public string KeepHardwoodText
		{
			get
			{
				return this._keepHardwoodText;
			}
		}

		[DataSourceProperty]
		public string KeepWagesText
		{
			get
			{
				return this._keepWagesText;
			}
		}

		[DataSourceProperty]
		public string SearchRadiusText
		{
			get
			{
				return this._searchRadiusText;
			}
		}

		[DataSourceProperty]
		public string WeaponsArmorTierText
		{
			get
			{
				return this._weaponsArmorTierText;
			}
		}

		[DataSourceProperty]
		public string UseAltATText
		{
			get
			{
				return this._useAltATText;
			}
		}

		[DataSourceProperty]
		public string JunkCattleText
		{
			get
			{
				return this._junkCattleText;
			}
		}

		[DataSourceProperty]
		public string SellFromBothRostersText
		{
			get
			{
				return this._sellFromBothRostersText;
			}
		}

		[DataSourceProperty]
		public bool SellFromBothRostersValue
		{
			get
			{
				return this._sellFromBothRostersValue;
			}
			set
			{
				if (value != this._sellFromBothRostersValue)
				{
					this._sellFromBothRostersValue = value;
					base.OnPropertyChanged("SellFromBothRostersValue");
				}
			}
		}

		[DataSourceProperty]
		public string SellSmithingText
		{
			get
			{
				return this._sellSmithingText;
			}
		}

		[DataSourceProperty]
		public string KeepSmeltingText
		{
			get
			{
				return this._keepSmeltingText;
			}
		}

		[DataSourceProperty]
		public string TradeHorsesText
		{
			get
			{
				return this._tradeHorsesText;
			}
		}

		[DataSourceProperty]
		public string TradeWeaponsText
		{
			get
			{
				return this._tradeWeaponsText;
			}
		}

		[DataSourceProperty]
		public string TradeArmorText
		{
			get
			{
				return this._tradeArmorText;
			}
		}

		[DataSourceProperty]
		public string TradeGoodsText
		{
			get
			{
				return this._tradeGoodsText;
			}
		}

		[DataSourceProperty]
		public string TradeConsumablesText
		{
			get
			{
				return this._tradeConsumablesText;
			}
		}

		[DataSourceProperty]
		public string TradeLivestockText
		{
			get
			{
				return this._tradeLivestockText;
			}
		}

		[DataSourceProperty]
		public bool IsSearchRadiusActive
		{
			get
			{
				return this._isSearchRadiusActive;
			}
			set
			{
				if (value != this._isSearchRadiusActive)
				{
					this._isSearchRadiusActive = value;
					base.OnPropertyChanged("IsSearchRadiusActive");
				}
			}
		}

		[DataSourceProperty]
		public bool IsWeightedActive
		{
			get
			{
				return this._isWeightedActive;
			}
			set
			{
				if (value != this._isWeightedActive)
				{
					this._isWeightedActive = value;
					RefreshValues();
					base.OnPropertyChanged("IsWeightedActive");
				}
			}
		}

		[DataSourceProperty]
		public bool UseWeightedValue
		{
			get
			{
				return this._useWeightedValue;
			}
			set
			{
				if (value != this._useWeightedValue)
				{
					this._useWeightedValue = value;
					RefreshValues();
					base.OnPropertyChanged("UseWeightedValue");
				}
			}
		}

		[DataSourceProperty]
		public bool SimpleAIValue
		{
			get
			{
				return this._simpleAIValue;
			}
			set
			{
				if (value != this._simpleAIValue)
				{
					this._simpleAIValue = value;
					RefreshValues();

					if (this._simpleAIValue)
					{
						this.SellThresholdValue = 125.0f;
						this.BuyThresholdValue = 80.0f;
					}
					else
					{
						this.SellThresholdValue = 95.0f;
						this.BuyThresholdValue = 70.0f;
					}

					base.OnPropertyChanged("SimpleAIValue");
				}
			}
		}

		[DataSourceProperty]
		public bool UseAltATValue
		{
			get
			{
				return this._useAltATValue;
			}
			set
			{
				if (value != this._useAltATValue)
				{
					this._useAltATValue = value;
					base.OnPropertyChanged("UseAltATValue");
				}
			}
		}

		[DataSourceProperty]
		public bool JunkCattleValue
		{
			get
			{
				return this._junkCattleValue;
			}
			set
			{
				if (value != this._junkCattleValue)
				{
					this._junkCattleValue = value;
                    if (value)
                    {
						this.BuyLivestockValue = false;
						this.SellLivestockValue = true;
                    }
					base.OnPropertyChanged("JunkCattleValue");
				}
			}
		}

		[DataSourceProperty]
		public bool SellSmithingValue
		{
			get
			{
				return this._sellSmithingValue;
			}
			set
			{
				if (value != this._sellSmithingValue)
				{
					this._sellSmithingValue = value;
					base.OnPropertyChanged("SellSmithingValue");
				}
			}
		}

		[DataSourceProperty]
		public bool KeepSmeltingValue
		{
			get
			{
				return this._keepSmeltingValue;
			}
			set
			{
				if (value != this._keepSmeltingValue)
				{
					this._keepSmeltingValue = value;
					base.OnPropertyChanged("KeepSmeltingValue");
				}
			}
		}

		[DataSourceProperty]
		public bool MountTroopsWarValue
		{
			get
			{
				return this._mountTroopsWarValue;
			}
			set
			{
				if (value != this._mountTroopsWarValue)
				{
					this._mountTroopsWarValue = value;
					base.OnPropertyChanged("MountTroopsWarValue");
				}
			}
		}

		[DataSourceProperty]
		public bool BuyMountsValue
		{
			get
			{
				return this._buyMountsValue;
			}
			set
			{
				if (value != this._buyMountsValue)
				{
					this._buyMountsValue = value;
					base.OnPropertyChanged("BuyMountsValue");
				}
			}
		}

		[DataSourceProperty]
		public bool SellMountsValue
		{
			get
			{
				return this._sellMountsValue;
			}
			set
			{
				if (value != this._sellMountsValue)
				{
					this._sellMountsValue = value;
					base.OnPropertyChanged("SellMountsValue");
				}
			}
		}

		[DataSourceProperty]
		public float KeepWarMountsValue
		{
			get
			{
				return this._keepWarMountsValue;
			}
			set
			{
				if (value != this._keepWarMountsValue)
				{
					this._keepWarMountsValue = value;
					this.KeepWarMountsValueAsString = this.KeepWarMountsValue.ToString();
					base.OnPropertyChanged("KeepWarMountsValue");
				}
			}
		}

		[DataSourceProperty]
		public string KeepWarMountsValueAsString
		{
			get
			{
				return this._keepWarMountsValueAsString;
			}
			set
			{
				if (value != this._keepWarMountsValueAsString)
				{
					this._keepWarMountsValueAsString = value;
					base.OnPropertyChanged("KeepWarMountsValueAsString");
				}
			}
		}

		[DataSourceProperty]
		public string KeepSpareMountsText
		{
			get
			{
				return this._keepSpareMountsText;
			}
		}

		[DataSourceProperty]
		public string MaxHerdTradePercentText
		{
			get
			{
				return this._maxHerdTradePercentText;
			}
		}

		[DataSourceProperty]
		public string MaxHorseGoldPerTypeText
		{
			get
			{
				return this._maxHorseGoldPerTypeText;
			}
		}

		[DataSourceProperty]
		public string TabGeneralText
		{
			get
			{
				return this._tabGeneralText;
			}
		}

		[DataSourceProperty]
		public string TabTradingText
		{
			get
			{
				return this._tabTradingText;
			}
		}

		[DataSourceProperty]
		public string TabSuppliesText
		{
			get
			{
				return this._tabSuppliesText;
			}
		}

		[DataSourceProperty]
		public string HeaderPricesText
		{
			get
			{
				return this._headerPricesText;
			}
		}

		[DataSourceProperty]
		public string HeaderGoldSpaceText
		{
			get
			{
				return this._headerGoldSpaceText;
			}
		}

		[DataSourceProperty]
		public string HeaderOtherText
		{
			get
			{
				return this._headerOtherText;
			}
		}

		[DataSourceProperty]
		public string HeaderBuyingText
		{
			get
			{
				return this._headerBuyingText;
			}
		}

		[DataSourceProperty]
		public string HeaderSellingText
		{
			get
			{
				return this._headerSellingText;
			}
		}

		[DataSourceProperty]
		public string HeaderWhatToTradeText
		{
			get
			{
				return this._headerWhatToTradeText;
			}
		}

		[DataSourceProperty]
		public string HeaderFoodText
		{
			get
			{
				return this._headerFoodText;
			}
		}

		[DataSourceProperty]
		public string HeaderHardwoodText
		{
			get
			{
				return this._headerHardwoodText;
			}
		}

		[DataSourceProperty]
		public string HeaderHorsesText
		{
			get
			{
				return this._headerHorsesText;
			}
		}

		[DataSourceProperty]
		public string HeaderResupplyText
		{
			get
			{
				return this._headerResupplyText;
			}
		}

		[DataSourceProperty]
		public string HeaderTroopHorsesText
		{
			get
			{
				return this._headerTroopHorsesText;
			}
		}

		[DataSourceProperty]
		public float KeepSpareMountsValue
		{
			get
			{
				return this._keepSpareMountsValue;
			}
			set
			{
				if (value != this._keepSpareMountsValue)
				{
					this._keepSpareMountsValue = value;
					this.KeepSpareMountsValueAsString = this.KeepSpareMountsValue.ToString();
					base.OnPropertyChanged("KeepSpareMountsValue");
				}
			}
		}

		[DataSourceProperty]
		public string KeepSpareMountsValueAsString
		{
			get
			{
				return this._keepSpareMountsValueAsString;
			}
			set
			{
				if (value != this._keepSpareMountsValueAsString)
				{
					this._keepSpareMountsValueAsString = value;
					base.OnPropertyChanged("KeepSpareMountsValueAsString");
				}
			}
		}

		[DataSourceProperty]
		public float MaxHerdTradePercentValue
		{
			get
			{
				return this._maxHerdTradePercentValue;
			}
			set
			{
				if (value != this._maxHerdTradePercentValue)
				{
					this._maxHerdTradePercentValue = value;
					this.MaxHerdTradePercentValueAsString = this.MaxHerdTradePercentValue.ToString();
					base.OnPropertyChanged("MaxHerdTradePercentValue");
				}
			}
		}

		[DataSourceProperty]
		public string MaxHerdTradePercentValueAsString
		{
			get
			{
				return this._maxHerdTradePercentValueAsString;
			}
			set
			{
				if (value != this._maxHerdTradePercentValueAsString)
				{
					this._maxHerdTradePercentValueAsString = value;
					base.OnPropertyChanged("MaxHerdTradePercentValueAsString");
				}
			}
		}

		[DataSourceProperty]
		public float MaxHorseGoldPerTypeValue
		{
			get
			{
				return this._maxHorseGoldPerTypeValue;
			}
			set
			{
				if (value != this._maxHorseGoldPerTypeValue)
				{
					this._maxHorseGoldPerTypeValue = value;
					this.MaxHorseGoldPerTypeValueAsString = GetMaxHorseGoldPerTypeAsString();
					base.OnPropertyChanged("MaxHorseGoldPerTypeValue");
				}
			}
		}

		[DataSourceProperty]
		public string MaxHorseGoldPerTypeValueAsString
		{
			get
			{
				return this._maxHorseGoldPerTypeValueAsString;
			}
			set
			{
				if (value != this._maxHorseGoldPerTypeValueAsString)
				{
					this._maxHorseGoldPerTypeValueAsString = value;
					base.OnPropertyChanged("MaxHorseGoldPerTypeValueAsString");
				}
			}
		}

		[DataSourceProperty]
		public bool IsGeneralTab
		{
			get
			{
				return this._selectedTab == TabGeneral;
			}
		}

		[DataSourceProperty]
		public bool IsTradingTab
		{
			get
			{
				return this._selectedTab == TabTrading;
			}
		}

		[DataSourceProperty]
		public bool IsSuppliesTab
		{
			get
			{
				return this._selectedTab == TabSupplies;
			}
		}

		[DataSourceProperty]
		public string PortUseInventorySpaceText
		{
			get
			{
				return this._portUseInventorySpaceText;
			}
		}

		[DataSourceProperty]
		public string PortMaxCapacityText
		{
			get
			{
				return this._portMaxCapacityText;
			}
		}

		[DataSourceProperty]
		public string PortMaxItemCountText
		{
			get
			{
				return this._portMaxItemCountText;
			}
		}

		[DataSourceProperty]
		public string TabPortsText
		{
			get
			{
				return this._tabPortsText;
			}
		}

		[DataSourceProperty]
		public string HeaderPortsText
		{
			get
			{
				return this._headerPortsText;
			}
		}

		[DataSourceProperty]
		public float PortUseInventorySpaceValue
		{
			get
			{
				return this._portUseInventorySpaceValue;
			}
			set
			{
				if (value != this._portUseInventorySpaceValue)
				{
					this._portUseInventorySpaceValue = value;
					this.PortUseInventorySpaceValueAsString = this.PortUseInventorySpaceValue.ToString();
					base.OnPropertyChanged("PortUseInventorySpaceValue");
				}
			}
		}

		[DataSourceProperty]
		public string PortUseInventorySpaceValueAsString
		{
			get
			{
				return this._portUseInventorySpaceValueAsString;
			}
			set
			{
				if (value != this._portUseInventorySpaceValueAsString)
				{
					this._portUseInventorySpaceValueAsString = value;
					base.OnPropertyChanged("PortUseInventorySpaceValueAsString");
				}
			}
		}

		[DataSourceProperty]
		public float PortMaxCapacityValue
		{
			get
			{
				return this._portMaxCapacityValue;
			}
			set
			{
				if (value != this._portMaxCapacityValue)
				{
					this._portMaxCapacityValue = value;
					this.PortMaxCapacityValueAsString = this.PortMaxCapacityValue.ToString();
					base.OnPropertyChanged("PortMaxCapacityValue");
				}
			}
		}

		[DataSourceProperty]
		public string PortMaxCapacityValueAsString
		{
			get
			{
				return this._portMaxCapacityValueAsString;
			}
			set
			{
				if (value != this._portMaxCapacityValueAsString)
				{
					this._portMaxCapacityValueAsString = value;
					base.OnPropertyChanged("PortMaxCapacityValueAsString");
				}
			}
		}

		[DataSourceProperty]
		public float PortMaxItemCountValue
		{
			get
			{
				return this._portMaxItemCountValue;
			}
			set
			{
				if (value != this._portMaxItemCountValue)
				{
					this._portMaxItemCountValue = value;
					this.PortMaxItemCountValueAsString = GetPortMaxItemCountAsString();
					base.OnPropertyChanged("PortMaxItemCountValue");
				}
			}
		}

		[DataSourceProperty]
		public string PortMaxItemCountValueAsString
		{
			get
			{
				return this._portMaxItemCountValueAsString;
			}
			set
			{
				if (value != this._portMaxItemCountValueAsString)
				{
					this._portMaxItemCountValueAsString = value;
					base.OnPropertyChanged("PortMaxItemCountValueAsString");
				}
			}
		}

		[DataSourceProperty]
		public string PortMinBuyProfitText
		{
			get
			{
				return this._portMinBuyProfitText;
			}
		}

		[DataSourceProperty]
		public float PortMinBuyProfitValue
		{
			get
			{
				return this._portMinBuyProfitValue;
			}
			set
			{
				if (value != this._portMinBuyProfitValue)
				{
					this._portMinBuyProfitValue = value;
					this.PortMinBuyProfitValueAsString = this.PortMinBuyProfitValue.ToString();
					base.OnPropertyChanged("PortMinBuyProfitValue");
				}
			}
		}

		[DataSourceProperty]
		public string PortMinBuyProfitValueAsString
		{
			get
			{
				return this._portMinBuyProfitValueAsString;
			}
			set
			{
				if (value != this._portMinBuyProfitValueAsString)
				{
					this._portMinBuyProfitValueAsString = value;
					base.OnPropertyChanged("PortMinBuyProfitValueAsString");
				}
			}
		}

		[DataSourceProperty]
		public bool IsPortsTab
		{
			get
			{
				return this._selectedTab == TabPorts;
			}
		}

		[DataSourceProperty]
		public float MinBuyProfitValue
		{
			get
			{
				return this._minBuyProfitValue;
			}
			set
			{
				if (value != this._minBuyProfitValue)
				{
					this._minBuyProfitValue = value;
					this.MinBuyProfitValueAsString = this.MinBuyProfitValue.ToString();
					base.OnPropertyChanged("MinBuyProfitValue");
				}
			}
		}

		[DataSourceProperty]
		public string MinBuyProfitValueAsString
		{
			get
			{
				return this._minBuyProfitValueAsString;
			}
			set
			{
				if (value != this._minBuyProfitValueAsString)
				{
					this._minBuyProfitValueAsString = value;
					base.OnPropertyChanged("MinBuyProfitValueAsString");
				}
			}
		}

		[DataSourceProperty]
		public bool MountTroopsValue
		{
			get
			{
				return this._mountTroopsValue;
			}
			set
			{
				if (value != this._mountTroopsValue)
				{
					this._mountTroopsValue = value;
					base.OnPropertyChanged("MountTroopsValue");
				}
			}
		}

		[DataSourceProperty]
		public bool ResupplyHardwoodValue
		{
			get
			{
				return this._resupplyHardwoodValue;
			}
			set
			{
				if (value != this._resupplyHardwoodValue)
				{
					this._resupplyHardwoodValue = value;
					base.OnPropertyChanged("ResupplyHardwoodValue");
				}
			}
		}

		[DataSourceProperty]
		public bool ResupplyValue
		{
			get
			{
				return this._resupplyValue;
			}
			set
			{
				if (value != this._resupplyValue)
				{
					this._resupplyValue = value;
					base.OnPropertyChanged("ResupplyValue");
				}
			}
		}

        [DataSourceProperty]
        public bool BuyHorsesValue
        {
            get
            {
                return this._buyHorsesValue;
            }
            set
            {
                if (value != this._buyHorsesValue)
                {
                    this._buyHorsesValue = value;
                    base.OnPropertyChanged("BuyHorsesValue");
                }
            }
        }

        [DataSourceProperty]
        public bool BuyWeaponsValue
        {
            get
            {
                return this._buyWeaponsValue;
            }
            set
            {
                if (value != this._buyWeaponsValue)
                {
                    this._buyWeaponsValue = value;
                    base.OnPropertyChanged("BuyWeaponsValue");
                }
            }
        }

        [DataSourceProperty]
        public bool BuyArmorValue
        {
            get
            {
                return this._buyArmorValue;
            }
            set
            {
                if (value != this._buyArmorValue)
                {
                    this._buyArmorValue = value;
                    base.OnPropertyChanged("BuyArmorValue");
                }
            }
        }

        [DataSourceProperty]
        public bool BuyGoodsValue
        {
            get
            {
                return this._buyGoodsValue;
            }
            set
            {
                if (value != this._buyGoodsValue)
                {
                    this._buyGoodsValue = value;
                    base.OnPropertyChanged("BuyGoodsValue");
                }
            }
        }

        [DataSourceProperty]
        public bool BuyConsumablesValue
        {
            get
            {
                return this._buyConsumablesValue;
            }
            set
            {
                if (value != this._buyConsumablesValue)
                {
                    this._buyConsumablesValue = value;
                    base.OnPropertyChanged("BuyConsumablesValue");
                }
            }
        }

        [DataSourceProperty]
        public bool BuyLivestockValue
        {
            get
            {
                return this._buyLivestockValue;
            }
            set
            {
                if (value != this._buyLivestockValue)
                {
                    this._buyLivestockValue = value;
                    if (value)
                        this.JunkCattleValue = false;
                    base.OnPropertyChanged("BuyLivestockValue");
                }
            }
        }

        [DataSourceProperty]
        public bool SellHorsesValue
        {
            get
            {
                return this._sellHorsesValue;
            }
            set
            {
                if (value != this._sellHorsesValue)
                {
                    this._sellHorsesValue = value;
                    base.OnPropertyChanged("SellHorsesValue");
                }
            }
        }

        [DataSourceProperty]
        public bool SellWeaponsValue
        {
            get
            {
                return this._sellWeaponsValue;
            }
            set
            {
                if (value != this._sellWeaponsValue)
                {
                    this._sellWeaponsValue = value;
                    base.OnPropertyChanged("SellWeaponsValue");
                }
            }
        }

        [DataSourceProperty]
        public bool SellArmorValue
        {
            get
            {
                return this._sellArmorValue;
            }
            set
            {
                if (value != this._sellArmorValue)
                {
                    this._sellArmorValue = value;
                    base.OnPropertyChanged("SellArmorValue");
                }
            }
        }

        [DataSourceProperty]
        public bool SellGoodsValue
        {
            get
            {
                return this._sellGoodsValue;
            }
            set
            {
                if (value != this._sellGoodsValue)
                {
                    this._sellGoodsValue = value;
                    base.OnPropertyChanged("SellGoodsValue");
                }
            }
        }

        [DataSourceProperty]
        public bool SellConsumablesValue
        {
            get
            {
                return this._sellConsumablesValue;
            }
            set
            {
                if (value != this._sellConsumablesValue)
                {
                    this._sellConsumablesValue = value;
                    base.OnPropertyChanged("SellConsumablesValue");
                }
            }
        }

        [DataSourceProperty]
        public bool SellLivestockValue
        {
            get
            {
                return this._sellLivestockValue;
            }
            set
            {
                if (value != this._sellLivestockValue)
                {
                    this._sellLivestockValue = value;
                    base.OnPropertyChanged("SellLivestockValue");
                }
            }
        }

		[DataSourceProperty]
		public string KeepGrainsMinText
		{
			get
			{
				return this._keepGrainsMinText;
			}
		}

		[DataSourceProperty]
		public string KeepGrainsMaxText
		{
			get
			{
				return this._keepGrainsMaxText;
			}
		}

		[DataSourceProperty]
		public string KeepConsumablesMinText
		{
			get
			{
				return this._keepConsumablesMinText;
			}
		}

		[DataSourceProperty]
		public string KeepConsumablesMaxText
		{
			get
			{
				return this._keepConsumablesMaxText;
			}
		}


		[DataSourceProperty]
		public float BuyThresholdValue
		{
			get
			{
				return this._buyThresholdValue;
			}
			set
			{
				if (value != this._buyThresholdValue)
				{
					this._buyThresholdValue = value;
					this.BuyThresholdValueAsString = this.BuyThresholdValue.ToString();
					base.OnPropertyChanged("BuyThresholdValue");
				}
			}
		}

		[DataSourceProperty]
		public string BuyThresholdValueAsString
		{
			get
			{
				return this._buyThresholdValueAsString;
			}
			set
			{
				if (value != this._buyThresholdValueAsString)
				{
					this._buyThresholdValueAsString = value;
					base.OnPropertyChanged("BuyThresholdValueAsString");
				}
			}
		}

		[DataSourceProperty]
		public float SellThresholdValue
		{
			get
			{
				return this._sellThresholdValue;
			}
			set
			{
				if (value != this._sellThresholdValue)
				{
					this._sellThresholdValue = value;
					this.SellThresholdValueAsString = this.SellThresholdValue.ToString();
					base.OnPropertyChanged("SellThresholdValue");
				}
			}
		}

		[DataSourceProperty]
		public string SellThresholdValueAsString
		{
			get
			{
				return this._sellThresholdValueAsString;
			}
			set
			{
				if (value != this._sellThresholdValueAsString)
				{
					this._sellThresholdValueAsString = value;
					base.OnPropertyChanged("SellThresholdValueAsString");
				}
			}
		}

		[DataSourceProperty]
		public float MaxCapacityValue
		{
			get
			{
				return this._maxCapacityValue;
			}
			set
			{
				if (value != this._maxCapacityValue)
				{
					this._maxCapacityValue = value;
					this.MaxCapacityValueAsString = this.MaxCapacityValue.ToString();
					base.OnPropertyChanged("MaxCapacityValue");
				}
			}
		}

		[DataSourceProperty]
		public string MaxCapacityValueAsString
		{
			get
			{
				return this._maxCapacityValueAsString;
			}
			set
			{
				if (value != this._maxCapacityValueAsString)
				{
					this._maxCapacityValueAsString = value;
					base.OnPropertyChanged("MaxCapacityValueAsString");
				}
			}
		}

		[DataSourceProperty]
		public float KeepGrainsMinValue
		{
			get
			{
				return this._keepGrainsMinValue;
			}
			set
			{
				if (value != this._keepGrainsMinValue)
				{
					this._keepGrainsMinValue = value;
					this.KeepGrainsMinValueAsString = this.KeepGrainsMinValue.ToString();
					base.OnPropertyChanged("KeepGrainsMinValue");
				}
			}
		}

		[DataSourceProperty]
		public float KeepGrainsMaxValue
		{
			get
			{
				return this._keepGrainsMaxValue;
			}
			set
			{
				if (value != this._keepGrainsMaxValue)
				{
					this._keepGrainsMaxValue = value;
					if (this._keepGrainsMaxValue >= AutoTraderConfig.MaxKeepGrainsValue)
						this.KeepGrainsMaxValueAsString = new TextObject("{=ATAll}All", null).ToString();
					else
						this.KeepGrainsMaxValueAsString = this.KeepGrainsMaxValue.ToString();
					base.OnPropertyChanged("KeepGrainsMaxValue");
				}
			}
		}

		[DataSourceProperty]
		public string KeepGrainsMinValueAsString
		{
			get
			{
				return this._keepGrainsMinValueAsString;
			}
			set
			{
				if (value != this._keepGrainsMinValueAsString)
				{
					this._keepGrainsMinValueAsString = value;
					base.OnPropertyChanged("KeepGrainsMinValueAsString");
				}
			}
		}

		[DataSourceProperty]
		public string KeepGrainsMaxValueAsString
		{
			get
			{
				return this._keepGrainsMaxValueAsString;
			}
			set
			{
				if (value != this._keepGrainsMaxValueAsString)
				{
					this._keepGrainsMaxValueAsString = value;
					base.OnPropertyChanged("KeepGrainsMaxValueAsString");
				}
			}
		}

		[DataSourceProperty]
		public float KeepConsumablesMinValue
		{
			get
			{
				return this._keepConsumablesMinValue;
			}
			set
			{
				if (value != this._keepConsumablesMinValue)
				{
					this._keepConsumablesMinValue = value;
					this.KeepConsumablesMinValueAsString = this.KeepConsumablesMinValue.ToString();
					base.OnPropertyChanged("KeepConsumablesMinValue");
				}
			}
		}

		[DataSourceProperty]
		public float KeepConsumablesMaxValue
		{
			get
			{
				return this._keepConsumablesMaxValue;
			}
			set
			{
				if (value != this._keepConsumablesMaxValue)
				{
					this._keepConsumablesMaxValue = value;
					this.KeepConsumablesMaxValueAsString = this.KeepConsumablesMaxValue.ToString();
					base.OnPropertyChanged("KeepConsumablesMaxValue");
				}
			}
		}

		[DataSourceProperty]
		public string KeepConsumablesMinValueAsString
		{
			get
			{
				return this._keepConsumablesMinValueAsString;
			}
			set
			{
				if (value != this._keepConsumablesMinValueAsString)
				{
					this._keepConsumablesMinValueAsString = value;
					base.OnPropertyChanged("KeepConsumablesMinValueAsString");
				}
			}
		}

		[DataSourceProperty]
		public string KeepConsumablesMaxValueAsString
		{
			get
			{
				return this._keepConsumablesMaxValueAsString;
			}
			set
			{
				if (value != this._keepConsumablesMaxValueAsString)
				{
					this._keepConsumablesMaxValueAsString = value;
					base.OnPropertyChanged("KeepConsumablesMaxValueAsString");
				}
			}
		}

		[DataSourceProperty]
		public float UseInventorySpaceValue
		{
			get
			{
				return this._useInventorySpaceValue;
			}
			set
			{
				if (value != this._useInventorySpaceValue)
				{
					this._useInventorySpaceValue = value;
					this.UseInventorySpaceValueAsString = this.UseInventorySpaceValue.ToString();
					base.OnPropertyChanged("UseInventorySpaceValue");
				}
			}
		}

		[DataSourceProperty]
		public string UseInventorySpaceValueAsString
		{
			get
			{
				return this._useInventorySpaceValueAsString;
			}
			set
			{
				if (value != this._useInventorySpaceValueAsString)
				{
					this._useInventorySpaceValueAsString = value;
					base.OnPropertyChanged("UseInventorySpaceValueAsString");
				}
			}
		}

		[DataSourceProperty]
		public float KeepHardwoodValue
		{
			get
			{
				return this._keepHardwoodValue;
			}
			set
			{
				if (value != this._keepHardwoodValue)
				{
					this._keepHardwoodValue = value;
					this.KeepHardwoodValueAsString = this.GetKeepHardwoodValueAsString();
					base.OnPropertyChanged("KeepHardwoodValue");
				}
			}
		}

		[DataSourceProperty]
		public string KeepHardwoodValueAsString
		{
			get
			{
				return this._keepHardwoodValueAsString;
			}
			set
			{
				if (value != this._keepHardwoodValueAsString)
				{
					this._keepHardwoodValueAsString = value;
					base.OnPropertyChanged("KeepHardwoodValueAsString");
				}
			}
		}

		[DataSourceProperty]
		public string HardwoodBuyThresholdText
		{
			get
			{
				return this._hardwoodBuyThresholdText;
			}
		}

		[DataSourceProperty]
		public float HardwoodBuyThresholdValue
		{
			get
			{
				return this._hardwoodBuyThresholdValue;
			}
			set
			{
				if (value != this._hardwoodBuyThresholdValue)
				{
					this._hardwoodBuyThresholdValue = value;
					this.HardwoodBuyThresholdValueAsString = this.HardwoodBuyThresholdValue.ToString();
					base.OnPropertyChanged("HardwoodBuyThresholdValue");
				}
			}
		}

		[DataSourceProperty]
		public string HardwoodBuyThresholdValueAsString
		{
			get
			{
				return this._hardwoodBuyThresholdValueAsString;
			}
			set
			{
				if (value != this._hardwoodBuyThresholdValueAsString)
				{
					this._hardwoodBuyThresholdValueAsString = value;
					base.OnPropertyChanged("HardwoodBuyThresholdValueAsString");
				}
			}
		}

		private string GetKeepHardwoodValueAsString()
		{
			if (this.KeepHardwoodValue >= AutoTraderConfig.MaxKeepHardwoodValue)
				return this.InfText;
			return this.KeepHardwoodValue.ToString();
		}

		[DataSourceProperty]
		public float KeepWagesValue
		{
			get
			{
				return this._keepWagesValue;
			}
			set
			{
				if (value != this._keepWagesValue)
				{
					this._keepWagesValue = value;
					this.KeepWagesValueAsString = this.KeepWagesValue.ToString();
					base.OnPropertyChanged("KeepWagesValue");
				}
			}
		}

		[DataSourceProperty]
		public string KeepWagesValueAsString
		{
			get
			{
				return this._keepWagesValueAsString;
			}
			set
			{
				if (value != this._keepWagesValueAsString)
				{
					this._keepWagesValueAsString = value;
					base.OnPropertyChanged("KeepWagesValueAsString");
				}
			}
		}

		[DataSourceProperty]
		public float SearchRadiusValue
		{
			get
			{
				return this._searchRadiusValue;
			}
			set
			{
				if (value != this._searchRadiusValue)
				{
					this._searchRadiusValue = value;
					if (this._searchRadiusValue > 999.0f)
						this.SearchRadiusValueAsString = this.InfText;
					else
						this.SearchRadiusValueAsString = this.SearchRadiusValue.ToString();
					base.OnPropertyChanged("SearchRadiusValue");
				}
			}
		}

		[DataSourceProperty]
		public string SearchRadiusValueAsString
		{
			get
			{
				return this._searchRadiusValueAsString;
			}
			set
			{
				if (value != this._searchRadiusValueAsString)
				{
					this._searchRadiusValueAsString = value;
					base.OnPropertyChanged("SearchRadiusValueAsString");
				}
			}
		}

		[DataSourceProperty]
		public float WeaponsArmorTierValue
		{
			get
			{
				return this._weaponsArmorTierValue;
			}
			set
			{
				if (value != this._weaponsArmorTierValue)
				{
					this._weaponsArmorTierValue = value;
					this.WeaponsArmorTierValueAsString = this.WeaponsArmorTierValue.ToString();
					base.OnPropertyChanged("WeaponsArmorTierValue");
				}
			}
		}

		[DataSourceProperty]
		public string WeaponsArmorTierValueAsString
		{
			get
			{
				return this._weaponsArmorTierValueAsString;
			}
			set
			{
				if (value != this._weaponsArmorTierValueAsString)
				{
					this._weaponsArmorTierValueAsString = value;
					base.OnPropertyChanged("WeaponsArmorTierValueAsString");
				}
			}
		}


	}
}
