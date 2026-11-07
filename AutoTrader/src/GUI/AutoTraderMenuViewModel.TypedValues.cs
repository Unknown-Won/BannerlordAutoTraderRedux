using System;
using System.Collections.Generic;
using System.Globalization;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace AutoTrader
{
	// Number boxes next to the sliders. Each box edits the slider's ...ValueAsString text directly.
	// Typed text is applied on Enter, on Done, when switching tabs and before presets and profiles.
	partial class AutoTraderMenuViewModel
	{
		private class SliderInput
		{
			public Func<string> GetText;
			public Func<string> Format;
			public Func<float> GetValue;
			public Action<float> SetValue;
			public Func<float> Min;
			public Func<float> Max;
			// Gold or items per slider step, e.g. 5000 for the horse gold slider
			public float Scale = 1.0f;
			// The slider's maximum shows Inf or All, so typing those words selects it
			public bool MaxIsUnlimited;
			// 0 shows Off, so typing "off" selects it
			public bool ZeroIsOff;
		}

		private List<SliderInput> _sliderInputs;

		private List<SliderInput> SliderInputs
		{
			get
			{
				if (this._sliderInputs == null)
					this._sliderInputs = this.CreateSliderInputs();
				return this._sliderInputs;
			}
		}

		// Order matters: a slider whose range depends on another slider comes after it
		private List<SliderInput> CreateSliderInputs()
		{
			return new List<SliderInput>
			{
				new SliderInput { GetText = () => this.SearchRadiusValueAsString, Format = this.GetSearchRadiusAsString, GetValue = () => this.SearchRadiusValue, SetValue = v => this.SearchRadiusValue = v, Min = () => 80f, Max = () => 1000f, MaxIsUnlimited = true },
				new SliderInput { GetText = () => this.KeepWagesValueAsString, Format = () => this.KeepWagesValue.ToString(), GetValue = () => this.KeepWagesValue, SetValue = v => this.KeepWagesValue = v, Min = () => 0f, Max = () => 50f },
				new SliderInput { GetText = () => this.UseInventorySpaceValueAsString, Format = () => this.UseInventorySpaceValue.ToString(), GetValue = () => this.UseInventorySpaceValue, SetValue = v => this.UseInventorySpaceValue = v, Min = () => 0f, Max = () => 150f },
				new SliderInput { GetText = () => this.MaxCapacityValueAsString, Format = () => this.MaxCapacityValue.ToString(), GetValue = () => this.MaxCapacityValue, SetValue = v => this.MaxCapacityValue = v, Min = () => 0f, Max = () => this.UseInventorySpaceValue },
				new SliderInput { GetText = () => this.BuyThresholdValueAsString, Format = () => this.BuyThresholdValue.ToString(), GetValue = () => this.BuyThresholdValue, SetValue = v => this.BuyThresholdValue = v, Min = () => 0f, Max = () => 200f },
				new SliderInput { GetText = () => this.MinBuyProfitValueAsString, Format = () => this.MinBuyProfitValue.ToString(), GetValue = () => this.MinBuyProfitValue, SetValue = v => this.MinBuyProfitValue = v, Min = () => 0f, Max = () => 100f },
				new SliderInput { GetText = () => this.BuyProfitOverrideValueAsString, Format = this.GetBuyProfitOverrideAsString, GetValue = () => this.BuyProfitOverrideValue, SetValue = v => this.BuyProfitOverrideValue = v, Min = () => 0f, Max = () => 100f, Scale = 100f, ZeroIsOff = true },
				new SliderInput { GetText = () => this.SellThresholdValueAsString, Format = () => this.SellThresholdValue.ToString(), GetValue = () => this.SellThresholdValue, SetValue = v => this.SellThresholdValue = v, Min = () => 0f, Max = () => 200f },
				new SliderInput { GetText = () => this.WeaponsArmorTierValueAsString, Format = () => this.WeaponsArmorTierValue.ToString(), GetValue = () => this.WeaponsArmorTierValue, SetValue = v => this.WeaponsArmorTierValue = v, Min = () => 0f, Max = () => 6f },
				new SliderInput { GetText = () => this.KeepGrainsMinValueAsString, Format = () => this.KeepGrainsMinValue.ToString(), GetValue = () => this.KeepGrainsMinValue, SetValue = v => this.KeepGrainsMinValue = v, Min = () => 0f, Max = () => 999f },
				new SliderInput { GetText = () => this.KeepGrainsMaxValueAsString, Format = this.GetKeepGrainsMaxAsString, GetValue = () => this.KeepGrainsMaxValue, SetValue = v => this.KeepGrainsMaxValue = v, Min = () => this.KeepGrainsMinValue, Max = () => 1000f, MaxIsUnlimited = true },
				new SliderInput { GetText = () => this.KeepConsumablesMinValueAsString, Format = () => this.KeepConsumablesMinValue.ToString(), GetValue = () => this.KeepConsumablesMinValue, SetValue = v => this.KeepConsumablesMinValue = v, Min = () => 0f, Max = () => 499f },
				new SliderInput { GetText = () => this.KeepConsumablesMaxValueAsString, Format = () => this.KeepConsumablesMaxValue.ToString(), GetValue = () => this.KeepConsumablesMaxValue, SetValue = v => this.KeepConsumablesMaxValue = v, Min = () => this.KeepConsumablesMinValue, Max = () => 500f },
				new SliderInput { GetText = () => this.KeepHardwoodValueAsString, Format = this.GetKeepHardwoodValueAsString, GetValue = () => this.KeepHardwoodValue, SetValue = v => this.KeepHardwoodValue = v, Min = () => 0f, Max = () => 500f, MaxIsUnlimited = true },
				new SliderInput { GetText = () => this.HardwoodBuyThresholdValueAsString, Format = () => this.HardwoodBuyThresholdValue.ToString(), GetValue = () => this.HardwoodBuyThresholdValue, SetValue = v => this.HardwoodBuyThresholdValue = v, Min = () => 0f, Max = () => 200f },
				new SliderInput { GetText = () => this.KeepWarMountsValueAsString, Format = () => this.KeepWarMountsValue.ToString(), GetValue = () => this.KeepWarMountsValue, SetValue = v => this.KeepWarMountsValue = v, Min = () => 0f, Max = () => 50f },
				new SliderInput { GetText = () => this.KeepSpareMountsValueAsString, Format = () => this.KeepSpareMountsValue.ToString(), GetValue = () => this.KeepSpareMountsValue, SetValue = v => this.KeepSpareMountsValue = v, Min = () => 0f, Max = () => 100f },
				new SliderInput { GetText = () => this.MaxHerdTradePercentValueAsString, Format = () => this.MaxHerdTradePercentValue.ToString(), GetValue = () => this.MaxHerdTradePercentValue, SetValue = v => this.MaxHerdTradePercentValue = v, Min = () => 0f, Max = () => 150f },
				new SliderInput { GetText = () => this.MaxHorseGoldPerTypeValueAsString, Format = this.GetMaxHorseGoldPerTypeAsString, GetValue = () => this.MaxHorseGoldPerTypeValue, SetValue = v => this.MaxHorseGoldPerTypeValue = v, Min = () => 0f, Max = () => 41f, Scale = 5000f, MaxIsUnlimited = true },
				new SliderInput { GetText = () => this.PortUseInventorySpaceValueAsString, Format = () => this.PortUseInventorySpaceValue.ToString(), GetValue = () => this.PortUseInventorySpaceValue, SetValue = v => this.PortUseInventorySpaceValue = v, Min = () => 0f, Max = () => 150f },
				new SliderInput { GetText = () => this.PortMaxCapacityValueAsString, Format = () => this.PortMaxCapacityValue.ToString(), GetValue = () => this.PortMaxCapacityValue, SetValue = v => this.PortMaxCapacityValue = v, Min = () => 0f, Max = () => this.PortUseInventorySpaceValue },
				new SliderInput { GetText = () => this.PortMaxItemCountValueAsString, Format = this.GetPortMaxItemCountAsString, GetValue = () => this.PortMaxItemCountValue, SetValue = v => this.PortMaxItemCountValue = v, Min = () => 0f, Max = () => 101f, Scale = 10f, MaxIsUnlimited = true },
				new SliderInput { GetText = () => this.PortMinBuyProfitValueAsString, Format = () => this.PortMinBuyProfitValue.ToString(), GetValue = () => this.PortMinBuyProfitValue, SetValue = v => this.PortMinBuyProfitValue = v, Min = () => 0f, Max = () => 100f },
			};
		}

		// Enter in any number box
		private void ExecuteApplyTypedValues()
		{
			this.ApplyTypedValues();
		}

		/// <summary>
		/// Moves each slider to the number typed in its box, then shows every box's value again.
		/// Boxes whose text wasn't changed are left alone; text that isn't a number is reset.
		/// </summary>
		private void ApplyTypedValues()
		{
			foreach (SliderInput input in this.SliderInputs)
			{
				string text = input.GetText();
				if (text == null || text == input.Format())
					continue;

				float typed;
				if (this.TryParseTypedValue(input, text, out typed))
					input.SetValue(typed);
			}

			// A lowered "inventory space" or raised minimum can push a dependent slider out of its range
			foreach (SliderInput input in this.SliderInputs)
			{
				float value = input.GetValue();
				float clamped = Math.Max(input.Min(), Math.Min(input.Max(), value));
				if (clamped != value)
					input.SetValue(clamped);
			}

			this.RefreshValues();
		}

		private bool TryParseTypedValue(SliderInput input, string text, out float sliderValue)
		{
			sliderValue = 0f;
			string word = text.Trim();
			if (word.Length == 0)
				return false;

			if (input.MaxIsUnlimited && (IsWord(word, "Inf") || IsWord(word, this.InfText) || IsWord(word, "All") || IsWord(word, new TextObject("{=ATAll}All", null).ToString())))
			{
				sliderValue = input.Max();
				return true;
			}
			if (input.ZeroIsOff && (IsWord(word, "Off")))
			{
				sliderValue = 0f;
				return true;
			}

			// Whole numbers only; allow "75,000", "75 000", "75.000" and a trailing "%"
			string digits = word.TrimEnd('%').Replace(",", "").Replace(".", "").Replace(" ", "").Replace("'", "");
			long number;
			if (!long.TryParse(digits, NumberStyles.Integer, CultureInfo.InvariantCulture, out number))
				return false;

			double steps = Math.Round(number / (double)input.Scale, MidpointRounding.AwayFromZero);
			sliderValue = (float)Math.Max(input.Min(), Math.Min(input.Max(), steps));
			return true;
		}

		private static bool IsWord(string text, string word)
		{
			return !string.IsNullOrEmpty(word) && string.Equals(text, word.Trim(), StringComparison.OrdinalIgnoreCase);
		}

		private string GetSearchRadiusAsString()
		{
			if (this.SearchRadiusValue > 999.0f)
				return this.InfText;
			return this.SearchRadiusValue.ToString();
		}

		private string GetKeepGrainsMaxAsString()
		{
			if (this.KeepGrainsMaxValue >= AutoTraderConfig.MaxKeepGrainsValue)
				return new TextObject("{=ATAll}All", null).ToString();
			return this.KeepGrainsMaxValue.ToString();
		}

		[DataSourceProperty]
		public string TypedValuesHintText
		{
			get
			{
				return this._typedValuesHintText;
			}
		}
	}
}
