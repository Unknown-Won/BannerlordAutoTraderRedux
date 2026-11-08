using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using TaleWorlds.Core.ViewModelCollection.Selector;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace AutoTrader
{
	// Settings profiles on the General tab. Choosing a profile in the dropdown fills in the screen like a preset; Done applies it.
	partial class AutoTraderMenuViewModel
	{
		private class ProfileSetting
		{
			public string Key;
			public Func<string> Get;
			public Action<string> Set;
		}

		private List<ProfileSetting> _profileSettings;
		private SelectorVM<SelectorItemVM> _profileSelector;
		// Saved profile names; the dropdown shows "Choose a profile" first, so item i + 1 is _profileNames[i]
		private List<string> _profileNames = new List<string>();
		private bool _updatingProfileSelector;
		private string _chooseProfileText;
		private string _profileName = "";
		private string _profilesText;
		private string _profileNameHintText;
		private string _profileSaveText;
		private string _profileDeleteText;
		private string _noProfilesText;
		private string _typedValuesHintText;

		private void InitializeProfiles()
		{
			this._profilesText = new TextObject("{=ATHeaderProfiles}Profiles", null).ToString();
			this._profileNameHintText = new TextObject("{=ATProfileNameHint}Profile name", null).ToString();
			this._profileSaveText = new TextObject("{=ATProfileSave}Save", null).ToString();
			this._profileDeleteText = new TextObject("{=ATProfileDelete}Delete", null).ToString();
			this._noProfilesText = new TextObject("{=ATNoProfiles}Type a name and press Save to keep the settings on screen as a profile.", null).ToString();
			this._typedValuesHintText = new TextObject("{=ATTypedValuesHint}Click a number next to a slider to type a value, then press Enter or click elsewhere.", null).ToString();

			this._chooseProfileText = new TextObject("{=ATChooseProfile}Choose a profile", null).ToString();

			this._profileSelector = new SelectorVM<SelectorItemVM>(0, this.OnProfileSelected);
			this.RefreshProfileList();
		}

		// Every trading setting, by its name in AutoTraderConfig.xml and in config units.
		// The hotkey and debug mode aren't part of a profile.
		// simpleTradingAI comes first, because changing it resets the buy and sell thresholds.
		private List<ProfileSetting> ProfileSettings
		{
			get
			{
				if (this._profileSettings == null)
				{
					this._profileSettings = new List<ProfileSetting>
					{
						BoolSetting("simpleTradingAI", () => this.SimpleAIValue, v => this.SimpleAIValue = v),
						BoolSetting("useWeightedValue", () => this.UseWeightedValue, v => this.UseWeightedValue = v),
						NumberSetting("buyThresholdValue", () => this.BuyThresholdValue, v => this.BuyThresholdValue = v),
						NumberSetting("sellThresholdValue", () => this.SellThresholdValue, v => this.SellThresholdValue = v),
						NumberSetting("useInventorySpaceValue", () => this.UseInventorySpaceValue, v => this.UseInventorySpaceValue = v),
						NumberSetting("maxCapacityValue", () => this.MaxCapacityValue, v => this.MaxCapacityValue = v),
						NumberSetting("keepGrainsMinValue", () => this.KeepGrainsMinValue, v => this.KeepGrainsMinValue = v),
						NumberSetting("keepGrainsMaxValue", () => this.KeepGrainsMaxValue, v => this.KeepGrainsMaxValue = v),
						NumberSetting("keepConsumablesMinValue", () => this.KeepConsumablesMinValue, v => this.KeepConsumablesMinValue = v),
						NumberSetting("keepConsumablesMaxValue", () => this.KeepConsumablesMaxValue, v => this.KeepConsumablesMaxValue = v),
						NumberSetting("keepHardwoodValue", () => this.KeepHardwoodValue, v => this.KeepHardwoodValue = v),
						NumberSetting("hardwoodBuyThresholdValue", () => this.HardwoodBuyThresholdValue, v => this.HardwoodBuyThresholdValue = v),
						NumberSetting("keepWagesValue", () => this.KeepWagesValue, v => this.KeepWagesValue = v),
						NumberSetting("searchRadiusValue", () => this.SearchRadiusValue, v => this.SearchRadiusValue = v),
						NumberSetting("weaponsArmorTierValue", () => this.WeaponsArmorTierValue, v => this.WeaponsArmorTierValue = v),

						BoolSetting("junkCattleValue", () => this.JunkCattleValue, v => this.JunkCattleValue = v),
						BoolSetting("sellFromBothRostersValue", () => this.SellFromBothRostersValue, v => this.SellFromBothRostersValue = v),
						BoolSetting("resupplyValue", () => this.ResupplyValue, v => this.ResupplyValue = v),
						BoolSetting("sellSmithingValue", () => this.SellSmithingValue, v => this.SellSmithingValue = v),
						BoolSetting("keepSmeltingValue", () => this.KeepSmeltingValue, v => this.KeepSmeltingValue = v),
						BoolSetting("resupplyHardwoodValue", () => this.ResupplyHardwoodValue, v => this.ResupplyHardwoodValue = v),
						BoolSetting("mountTroopsValue", () => this.MountTroopsValue, v => this.MountTroopsValue = v),
						BoolSetting("mountTroopsWarValue", () => this.MountTroopsWarValue, v => this.MountTroopsWarValue = v),
						BoolSetting("buyMountsValue", () => this.BuyMountsValue, v => this.BuyMountsValue = v),
						BoolSetting("sellMountsValue", () => this.SellMountsValue, v => this.SellMountsValue = v),
						NumberSetting("keepWarMountsValue", () => this.KeepWarMountsValue, v => this.KeepWarMountsValue = v),
						NumberSetting("keepSpareMountsValue", () => this.KeepSpareMountsValue, v => this.KeepSpareMountsValue = v),
						NumberSetting("maxHerdTradePercentValue", () => this.MaxHerdTradePercentValue, v => this.MaxHerdTradePercentValue = v),
						NumberSetting("maxHorseGoldPerTypeValue", () => this.MaxHorseGoldPerTypeValue, v => this.MaxHorseGoldPerTypeValue = v, 5000f),
						NumberSetting("portUseInventorySpaceValue", () => this.PortUseInventorySpaceValue, v => this.PortUseInventorySpaceValue = v),
						NumberSetting("portMaxCapacityValue", () => this.PortMaxCapacityValue, v => this.PortMaxCapacityValue = v),
						NumberSetting("portMaxItemCountValue", () => this.PortMaxItemCountValue, v => this.PortMaxItemCountValue = v, 10f),
						NumberSetting("portMinBuyProfitValue", () => this.PortMinBuyProfitValue, v => this.PortMinBuyProfitValue = v),
						NumberSetting("minBuyProfitValue", () => this.MinBuyProfitValue, v => this.MinBuyProfitValue = v),
						NumberSetting("buyProfitOverrideValue", () => this.BuyProfitOverrideValue, v => this.BuyProfitOverrideValue = v, 100f),

						BoolSetting("buyHorsesValue", () => this.BuyHorsesValue, v => this.BuyHorsesValue = v),
						BoolSetting("sellHorsesValue", () => this.SellHorsesValue, v => this.SellHorsesValue = v),
						BoolSetting("buyArmorValue", () => this.BuyArmorValue, v => this.BuyArmorValue = v),
						BoolSetting("sellArmorValue", () => this.SellArmorValue, v => this.SellArmorValue = v),
						BoolSetting("buyWeaponsValue", () => this.BuyWeaponsValue, v => this.BuyWeaponsValue = v),
						BoolSetting("sellWeaponsValue", () => this.SellWeaponsValue, v => this.SellWeaponsValue = v),
						BoolSetting("buyGoodsValue", () => this.BuyGoodsValue, v => this.BuyGoodsValue = v),
						BoolSetting("sellGoodsValue", () => this.SellGoodsValue, v => this.SellGoodsValue = v),
						BoolSetting("buyConsumablesValue", () => this.BuyConsumablesValue, v => this.BuyConsumablesValue = v),
						BoolSetting("sellConsumablesValue", () => this.SellConsumablesValue, v => this.SellConsumablesValue = v),
						BoolSetting("buyLivestockValue", () => this.BuyLivestockValue, v => this.BuyLivestockValue = v),
						BoolSetting("sellLivestockValue", () => this.SellLivestockValue, v => this.SellLivestockValue = v),
					};
				}
				return this._profileSettings;
			}
		}

		private static ProfileSetting BoolSetting(string key, Func<bool> get, Action<bool> set)
		{
			return new ProfileSetting
			{
				Key = key,
				Get = () => get().ToString(),
				Set = text =>
				{
					bool value;
					if (bool.TryParse(text, out value))
						set(value);
				}
			};
		}

		// scale: config units per slider step, e.g. 5000 gold for the horse gold slider
		private static ProfileSetting NumberSetting(string key, Func<float> get, Action<float> set, float scale = 1f)
		{
			return new ProfileSetting
			{
				Key = key,
				Get = () => ((int)Math.Round(get() * scale)).ToString(CultureInfo.InvariantCulture),
				Set = text =>
				{
					int value;
					if (int.TryParse(text, NumberStyles.Integer, CultureInfo.InvariantCulture, out value))
						set((float)Math.Round(value / scale));
				}
			};
		}

		private void RefreshProfileList()
		{
			this._profileNames = AutoTraderProfiles.GetProfileNames();
			List<string> items = new List<string> { this._chooseProfileText };
			items.AddRange(this._profileNames);

			this._updatingProfileSelector = true;
			this._profileSelector.Refresh(items, this.GetSelectedProfileIndex(), this.OnProfileSelected);
			this._updatingProfileSelector = false;
			base.OnPropertyChanged("HasProfiles");
		}

		// Shows the profile named in the name box in the dropdown, or "Choose a profile"
		private void UpdateSelectedProfile()
		{
			if (this._profileSelector == null)
				return;

			this._updatingProfileSelector = true;
			this._profileSelector.SelectedIndex = this.GetSelectedProfileIndex();
			this._updatingProfileSelector = false;
		}

		private int GetSelectedProfileIndex()
		{
			string name = AutoTraderProfiles.CleanName(this._profileName);
			int index = this._profileNames.FindIndex(n => string.Equals(n, name, StringComparison.OrdinalIgnoreCase));
			return index + 1;
		}

		private void OnProfileSelected(SelectorVM<SelectorItemVM> selector)
		{
			if (this._updatingProfileSelector || selector.SelectedIndex <= 0 || selector.SelectedIndex > this._profileNames.Count)
				return;

			this.LoadProfile(this._profileNames[selector.SelectedIndex - 1]);
		}

		private void LoadProfile(string name)
		{
			Dictionary<string, string> settings = AutoTraderProfiles.Load(name);
			if (settings == null)
			{
				AutoTraderHelpers.PrintMessage(new TextObject("{=ATProfileLoadFailed}Couldn't read the profile \"{NAME}\".", null).SetTextVariable("NAME", name).ToString());
				this.RefreshProfileList();
				return;
			}

			// Drop numbers typed but not entered, so they don't override the profile
			this.RefreshValues();

			foreach (ProfileSetting setting in this.ProfileSettings)
			{
				string text;
				if (settings.TryGetValue(setting.Key, out text))
					setting.Set(text);
			}

			// Puts every slider back in range and shows the loaded values in the number boxes
			this.ApplyTypedValues();
			this.ProfileName = name;
		}

		private void ExecuteSaveProfile()
		{
			this.ApplyTypedValues();

			string name = AutoTraderProfiles.CleanName(this._profileName);
			if (name.Length == 0)
			{
				AutoTraderHelpers.PrintMessage(new TextObject("{=ATProfileNoName}Type a name for the profile first.", null).ToString());
				return;
			}

			string existing = AutoTraderProfiles.GetProfileNames().FirstOrDefault(n => string.Equals(n, name, StringComparison.OrdinalIgnoreCase));
			if (existing != null)
			{
				this.Confirm(
					new TextObject("{=ATProfileOverwriteTitle}Overwrite profile", null).ToString(),
					new TextObject("{=ATProfileOverwrite}Replace the profile \"{NAME}\" with the settings on screen?", null).SetTextVariable("NAME", existing).ToString(),
					() => this.SaveProfile(existing));
			}
			else
			{
				this.SaveProfile(name);
			}
		}

		private void SaveProfile(string name)
		{
			try
			{
				AutoTraderProfiles.Save(name, this.ProfileSettings.Select(s => new KeyValuePair<string, string>(s.Key, s.Get())));
				AutoTraderHelpers.PrintMessage(new TextObject("{=ATProfileSaved}Profile \"{NAME}\" saved.", null).SetTextVariable("NAME", name).ToString());
			}
			catch (Exception e)
			{
				AutoTraderHelpers.PrintMessage(new TextObject("{=ATProfileSaveFailed}Couldn't save the profile \"{NAME}\".", null).SetTextVariable("NAME", name).ToString() + " " + e.Message);
			}

			this.ProfileName = name;
			this.RefreshProfileList();
		}

		private void ExecuteDeleteProfile()
		{
			string name = AutoTraderProfiles.CleanName(this._profileName);
			string existing = AutoTraderProfiles.GetProfileNames().FirstOrDefault(n => string.Equals(n, name, StringComparison.OrdinalIgnoreCase));
			if (existing == null)
			{
				AutoTraderHelpers.PrintMessage(new TextObject("{=ATProfileDeleteNone}Choose a profile, or type its name, to delete it.", null).ToString());
				return;
			}

			this.Confirm(
				new TextObject("{=ATProfileDeleteTitle}Delete profile", null).ToString(),
				new TextObject("{=ATProfileDeleteConfirm}Delete the profile \"{NAME}\"?", null).SetTextVariable("NAME", existing).ToString(),
				() =>
				{
					try
					{
						AutoTraderProfiles.Delete(existing);
						AutoTraderHelpers.PrintMessage(new TextObject("{=ATProfileDeleted}Profile \"{NAME}\" deleted.", null).SetTextVariable("NAME", existing).ToString());
						this.ProfileName = "";
					}
					catch (Exception e)
					{
						AutoTraderHelpers.PrintMessage(e.Message);
					}
					this.RefreshProfileList();
				});
		}

		private void Confirm(string title, string text, Action onYes)
		{
			InformationManager.ShowInquiry(new InquiryData(title, text, true, true,
				new TextObject("{=ATYes}Yes", null).ToString(),
				new TextObject("{=ATNo}No", null).ToString(),
				onYes, null), false, false);
		}

		[DataSourceProperty]
		public SelectorVM<SelectorItemVM> ProfileSelector
		{
			get
			{
				return this._profileSelector;
			}
			set
			{
				if (value != this._profileSelector)
				{
					this._profileSelector = value;
					base.OnPropertyChanged("ProfileSelector");
				}
			}
		}

		[DataSourceProperty]
		public bool HasProfiles
		{
			get
			{
				return this._profileNames.Count > 0;
			}
		}

		[DataSourceProperty]
		public string ProfileName
		{
			get
			{
				return this._profileName;
			}
			set
			{
				if (value != this._profileName)
				{
					this._profileName = value ?? "";
					base.OnPropertyChanged("ProfileName");
					this.UpdateSelectedProfile();
				}
			}
		}

		[DataSourceProperty]
		public string ProfilesText
		{
			get
			{
				return this._profilesText;
			}
		}

		[DataSourceProperty]
		public string ProfileNameHintText
		{
			get
			{
				return this._profileNameHintText;
			}
		}

		[DataSourceProperty]
		public string ProfileSaveText
		{
			get
			{
				return this._profileSaveText;
			}
		}

		[DataSourceProperty]
		public string ProfileDeleteText
		{
			get
			{
				return this._profileDeleteText;
			}
		}

		[DataSourceProperty]
		public string NoProfilesText
		{
			get
			{
				return this._noProfilesText;
			}
		}
	}
}
