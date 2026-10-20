using System;
using System.IO;
using System.Text;
using System.Xml;
using TaleWorlds.Engine;
using TaleWorlds.Library;

namespace AutoTrader
{
    public static class AutoTraderConfig
    {
        private static PlatformFilePath _configFile;

        public static string AutoTraderGameVersion { get; } = "v1.4.8";

        public static int MaxKeepGrainsValue { get; set; } = 500;
        public static int MaxKeepHardwoodValue { get; } = 500;

        public static int BuyThresholdValue { get; set; } = 75;
        public static int SellThresholdValue { get; set; } = 90;
        public static bool SimpleTradingAI { get; set; } = false;
        public static bool UseWeightedValue { get; set; } = false;
        public static int MaxCapacityValue { get; set; } = 20;
        public static int KeepGrainsMinValue { get; set; } = 10;
        public static int KeepGrainsMaxValue { get; set; } = 50;
        public static int KeepConsumablesMinValue { get; set; } = 4;
        public static int KeepConsumablesMaxValue { get; set; } = 50;
        public static int UseInventorySpaceValue { get; set; } = 90;
        public static int KeepHardwoodValue { get; set; } = 100;
        public static int HardwoodBuyThresholdValue { get; set; } = 110;
        public static int KeepWagesValue { get; set; } = 7;
        public static int SearchRadiusValue { get; set; } = 300;
        public static int WeaponsArmorTierValue { get; set; } = 2;

        public static bool UseAltATValue { get; set; } = false;
        public static bool SellSmithingValue { get; set; } = true;
        public static bool KeepSmeltingValue { get; set; } = true;
        public static bool ResupplyHardwoodValue { get; set; } = true;
        public static bool MountTroopsValue { get; set; } = true;
        public static bool MountTroopsWarValue { get; set; } = false;
        public static bool BuyMountsValue { get; set; } = true;
        public static bool SellMountsValue { get; set; } = true;
        public static int KeepWarMountsValue { get; set; } = 0;
        // Riding horses beyond the men on foot that are never sold, for troops hired later
        public static int KeepSpareMountsValue { get; set; } = 5;
        // Herd size trade buying may reach, as % of the herding limit (party size). Horses for men on foot don't count.
        public static int MaxHerdTradePercentValue { get; set; } = 90;
        // Most horses of one type to own for trading; the slider's maximum means no limit
        public static int MaxHorseGoldPerTypeValue { get; set; } = 75000;
        public static int MaxHorseGoldPerTypeInf { get; } = 205000;
        // Port (cargo hold) equivalents of the inventory space settings
        public static int PortUseInventorySpaceValue { get; set; } = 100;
        public static int PortMaxCapacityValue { get; set; } = 30;
        // Most of one good in the cargo hold, in steps of 10; the slider's maximum means no limit
        public static int PortMaxItemCountValue { get; set; } = 300;
        public static int PortMaxItemCountInf { get; } = 1010;
        public static int MinBuyProfitValue { get; set; } = 50;
        // Minimum profit per 10 weight when trading with the cargo hold
        public static int PortMinBuyProfitValue { get; set; } = 40;
        // Gold profit per item that lets an item past "Buy under X%" (0 = off)
        public static int BuyProfitOverrideValue { get; set; } = 1000;
        public static bool ResupplyValue { get; set; } = true;
        public static bool JunkCattleValue { get; set; } = false;
        public static bool SellFromBothRostersValue { get; set; } = true;

        public static bool BuyHorsesValue { get; set; } = true;
        public static bool SellHorsesValue { get; set; } = true;
        public static bool BuyWeaponsValue { get; set; } = true;
        public static bool SellWeaponsValue { get; set; } = true;
        public static bool BuyArmorValue { get; set; } = true;
        public static bool SellArmorValue { get; set; } = true;
        public static bool BuyGoodsValue { get; set; } = true;
        public static bool SellGoodsValue { get; set; } = true;
        public static bool BuyConsumablesValue { get; set; } = true;
        public static bool SellConsumablesValue { get; set; } = true;
        public static bool BuyLivestockValue { get; set; } = true;
        public static bool SellLivestockValue { get; set; } = true;

        public static int Version { get; set; } = 2;
        public static bool DebugMode { get; set; } = false;

        public static void Initialize()
        {
            // Get the config file path
            AutoTraderConfig._configFile = new PlatformFilePath(EngineFilePaths.ConfigsPath, "AutoTraderConfig.xml");

            // If it does not exist, create it by an initial save
            if (!FileHelper.FileExists(AutoTraderConfig._configFile))
            {
                AutoTraderConfig.Save();
            }
            else
            {
                // Read it
                var content = FileHelper.GetFileContentString(AutoTraderConfig._configFile);
                var stringReader = new System.IO.StringReader(content);
                XmlTextReader textReader = new XmlTextReader(stringReader);

                bool found_version = false;
                while (textReader.Read())
                {
                    if (textReader.IsStartElement())
                    {
                        if (textReader.Name == "version")
                        {
                            found_version = true;
                        }
                    }
                }

                textReader.Close();

                if (!found_version)
                {
                    // Reset
                    AutoTraderConfig.Save();
                }

                content = FileHelper.GetFileContentString(AutoTraderConfig._configFile);
                stringReader = new System.IO.StringReader(content);
                textReader = new XmlTextReader(stringReader);

                while (textReader.Read())
                {
                    if (textReader.IsStartElement())
                    {
                        if (textReader.Name == "buyThresholdValue")
                        {
                            AutoTraderConfig.BuyThresholdValue = Int32.Parse(textReader.ReadString());
                        }
                        else if (textReader.Name == "simpleTradingAI")
                        {
                            AutoTraderConfig.SimpleTradingAI = Boolean.Parse(textReader.ReadString());
                        }
                        else if (textReader.Name == "useWeightedValue")
                        {
                            AutoTraderConfig.UseWeightedValue = Boolean.Parse(textReader.ReadString());
                        }
                        else if (textReader.Name == "sellThresholdValue")
                        {
                            AutoTraderConfig.SellThresholdValue = Int32.Parse(textReader.ReadString());
                        }
                        else if (textReader.Name == "maxCapacityValue")
                        {
                            AutoTraderConfig.MaxCapacityValue = Int32.Parse(textReader.ReadString());
                        }
                        else if (textReader.Name == "keepGrainsMinValue")
                        {
                            AutoTraderConfig.KeepGrainsMinValue = Int32.Parse(textReader.ReadString());
                        }
                        else if (textReader.Name == "keepGrainsMaxValue")
                        {
                            AutoTraderConfig.KeepGrainsMaxValue = Int32.Parse(textReader.ReadString());
                        }
                        else if (textReader.Name == "keepConsumablesMinValue")
                        {
                            AutoTraderConfig.KeepConsumablesMinValue = Int32.Parse(textReader.ReadString());
                        }
                        else if (textReader.Name == "keepConsumablesMaxValue")
                        {
                            AutoTraderConfig.KeepConsumablesMaxValue = Int32.Parse(textReader.ReadString());
                        }
                        else if (textReader.Name == "useInventorySpaceValue")
                        {
                            AutoTraderConfig.UseInventorySpaceValue = Int32.Parse(textReader.ReadString());
                        }
                        else if (textReader.Name == "keepHardwoodValue")
                        {
                            AutoTraderConfig.KeepHardwoodValue = Int32.Parse(textReader.ReadString());
                        }
                        else if (textReader.Name == "hardwoodBuyThresholdValue")
                        {
                            AutoTraderConfig.HardwoodBuyThresholdValue = Int32.Parse(textReader.ReadString());
                        }
                        else if (textReader.Name == "searchRadiusValue")
                        {
                            AutoTraderConfig.SearchRadiusValue = Int32.Parse(textReader.ReadString());
                        }
                        else if (textReader.Name == "weaponsArmorTierValue")
                        {
                            AutoTraderConfig.WeaponsArmorTierValue = Int32.Parse(textReader.ReadString());
                        }
                        else if (textReader.Name == "keepWagesValue")
                        {
                            AutoTraderConfig.KeepWagesValue = Int32.Parse(textReader.ReadString());
                        }
                        else if (textReader.Name == "resupplyValue")
                        {
                            AutoTraderConfig.ResupplyValue = Boolean.Parse(textReader.ReadString());
                        }
                        else if (textReader.Name == "useAltATValue")
                        {
                            AutoTraderConfig.UseAltATValue = Boolean.Parse(textReader.ReadString());
                        }
                        else if (textReader.Name == "junkCattleValue")
                        {
                            AutoTraderConfig.JunkCattleValue = Boolean.Parse(textReader.ReadString());
                        }
                        else if (textReader.Name == "sellFromBothRostersValue")
                        {
                            AutoTraderConfig.SellFromBothRostersValue = Boolean.Parse(textReader.ReadString());
                        }
                        else if (textReader.Name == "sellSmithingValue")
                        {
                            AutoTraderConfig.SellSmithingValue = Boolean.Parse(textReader.ReadString());
                        }
                        else if (textReader.Name == "keepSmeltingValue")
                        {
                            AutoTraderConfig.KeepSmeltingValue = Boolean.Parse(textReader.ReadString());
                        }
                        else if (textReader.Name == "resupplyHardwoodValue")
                        {
                            AutoTraderConfig.ResupplyHardwoodValue = Boolean.Parse(textReader.ReadString());
                        }
                        else if (textReader.Name == "mountTroopsValue")
                        {
                            AutoTraderConfig.MountTroopsValue = Boolean.Parse(textReader.ReadString());
                        }
                        else if (textReader.Name == "mountTroopsWarValue")
                        {
                            AutoTraderConfig.MountTroopsWarValue = Boolean.Parse(textReader.ReadString());
                        }
                        else if (textReader.Name == "buyMountsValue")
                        {
                            AutoTraderConfig.BuyMountsValue = Boolean.Parse(textReader.ReadString());
                        }
                        else if (textReader.Name == "sellMountsValue")
                        {
                            AutoTraderConfig.SellMountsValue = Boolean.Parse(textReader.ReadString());
                        }
                        else if (textReader.Name == "keepWarMountsValue")
                        {
                            AutoTraderConfig.KeepWarMountsValue = Int32.Parse(textReader.ReadString());
                        }
                        else if (textReader.Name == "keepSpareMountsValue")
                        {
                            AutoTraderConfig.KeepSpareMountsValue = Int32.Parse(textReader.ReadString());
                        }
                        else if (textReader.Name == "maxHerdTradePercentValue")
                        {
                            AutoTraderConfig.MaxHerdTradePercentValue = Int32.Parse(textReader.ReadString());
                        }
                        else if (textReader.Name == "maxHorseGoldPerTypeValue")
                        {
                            AutoTraderConfig.MaxHorseGoldPerTypeValue = Int32.Parse(textReader.ReadString());
                        }
                        else if (textReader.Name == "portUseInventorySpaceValue")
                        {
                            AutoTraderConfig.PortUseInventorySpaceValue = Int32.Parse(textReader.ReadString());
                        }
                        else if (textReader.Name == "portMaxCapacityValue")
                        {
                            AutoTraderConfig.PortMaxCapacityValue = Int32.Parse(textReader.ReadString());
                        }
                        else if (textReader.Name == "portMaxItemCountValue")
                        {
                            AutoTraderConfig.PortMaxItemCountValue = Int32.Parse(textReader.ReadString());
                        }
                        else if (textReader.Name == "portMinBuyProfitValue")
                        {
                            AutoTraderConfig.PortMinBuyProfitValue = Int32.Parse(textReader.ReadString());
                        }
                        else if (textReader.Name == "minBuyProfitValue")
                        {
                            AutoTraderConfig.MinBuyProfitValue = Int32.Parse(textReader.ReadString());
                        }
                        else if (textReader.Name == "buyProfitOverrideValue")
                        {
                            AutoTraderConfig.BuyProfitOverrideValue = Int32.Parse(textReader.ReadString());
                        }
                        else if (textReader.Name == "buyHorsesValue")
                        {
                            AutoTraderConfig.BuyHorsesValue = Boolean.Parse(textReader.ReadString());
                        }
                        else if (textReader.Name == "sellHorsesValue")
                        {
                            AutoTraderConfig.SellHorsesValue = Boolean.Parse(textReader.ReadString());
                        }
                        else if (textReader.Name == "buyArmorValue")
                        {
                            AutoTraderConfig.BuyArmorValue = Boolean.Parse(textReader.ReadString());
                        }
                        else if (textReader.Name == "sellArmorValue")
                        {
                            AutoTraderConfig.SellArmorValue = Boolean.Parse(textReader.ReadString());
                        }
                        else if (textReader.Name == "buyWeaponsValue")
                        {
                            AutoTraderConfig.BuyWeaponsValue = Boolean.Parse(textReader.ReadString());
                        }
                        else if (textReader.Name == "sellWeaponsValue")
                        {
                            AutoTraderConfig.SellWeaponsValue = Boolean.Parse(textReader.ReadString());
                        }
                        else if (textReader.Name == "buyGoodsValue")
                        {
                            AutoTraderConfig.BuyGoodsValue = Boolean.Parse(textReader.ReadString());
                        }
                        else if (textReader.Name == "sellGoodsValue")
                        {
                            AutoTraderConfig.SellGoodsValue = Boolean.Parse(textReader.ReadString());
                        }
                        else if (textReader.Name == "buyConsumablesValue")
                        {
                            AutoTraderConfig.BuyConsumablesValue = Boolean.Parse(textReader.ReadString());
                        }
                        else if (textReader.Name == "sellConsumablesValue")
                        {
                            AutoTraderConfig.SellConsumablesValue = Boolean.Parse(textReader.ReadString());
                        }
                        else if (textReader.Name == "buyLivestockValue")
                        {
                            AutoTraderConfig.BuyLivestockValue = Boolean.Parse(textReader.ReadString());
                        }
                        else if (textReader.Name == "sellLivestockValue")
                        {
                            AutoTraderConfig.SellLivestockValue = Boolean.Parse(textReader.ReadString());
                        }
                        else if (textReader.Name == "debugMode")
                        {
                            AutoTraderConfig.DebugMode = Boolean.Parse(textReader.ReadString());
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Re-reads only debugMode from the config file, so it can be switched on or off while the game is running.
        /// </summary>
        public static void ReloadDebugMode()
        {
            try
            {
                if (!FileHelper.FileExists(AutoTraderConfig._configFile))
                    return;

                var content = FileHelper.GetFileContentString(AutoTraderConfig._configFile);
                XmlTextReader textReader = new XmlTextReader(new System.IO.StringReader(content));
                while (textReader.Read())
                {
                    if (textReader.IsStartElement() && textReader.Name == "debugMode")
                    {
                        bool debugMode = Boolean.Parse(textReader.ReadString());
                        if (debugMode != AutoTraderConfig.DebugMode)
                        {
                            AutoTraderConfig.DebugMode = debugMode;
                            AutoTraderHelpers.PrintMessage("AutoTrader debug mode " + (debugMode ? "enabled" : "disabled") + " from the config file.");
                        }
                        break;
                    }
                }
                textReader.Close();
            }
            catch (Exception)
            {
                // Keep the current value if the file is being edited or is invalid
            }
        }

        public static void Save()
        {
            // Keep a debugMode value that was edited in the file while the game was running
            AutoTraderConfig.ReloadDebugMode();

            // Open writer and write settings
            XmlDocument xmlDocument = new XmlDocument();
            XmlWriterSettings xmlWriterSettings = new XmlWriterSettings();
            xmlWriterSettings.Indent = true;
            xmlWriterSettings.IndentChars = "  ";
            xmlWriterSettings.NewLineChars = "\r\n";
            xmlWriterSettings.NewLineHandling = NewLineHandling.Replace;
            StringBuilder sb = new StringBuilder();

            //using (XmlTextWriter textWriter = XmlWriter.Create(xmlDocument.CreateNavigator().AppendChild(), xmlWriterSettings))
            using (XmlWriter textWriter = XmlWriter.Create(sb, xmlWriterSettings))
            {
                textWriter.WriteStartElement("config");

                textWriter.WriteElementString("version", AutoTraderConfig.Version.ToString());

                textWriter.WriteElementString("simpleTradingAI", AutoTraderConfig.SimpleTradingAI.ToString());
                textWriter.WriteElementString("useWeightedValue", AutoTraderConfig.UseWeightedValue.ToString());
                textWriter.WriteElementString("buyThresholdValue", AutoTraderConfig.BuyThresholdValue.ToString());
                textWriter.WriteElementString("sellThresholdValue", AutoTraderConfig.SellThresholdValue.ToString());
                textWriter.WriteElementString("maxCapacityValue", AutoTraderConfig.MaxCapacityValue.ToString());
                textWriter.WriteElementString("keepGrainsMinValue", AutoTraderConfig.KeepGrainsMinValue.ToString());
                textWriter.WriteElementString("keepGrainsMaxValue", AutoTraderConfig.KeepGrainsMaxValue.ToString());
                textWriter.WriteElementString("keepConsumablesMinValue", AutoTraderConfig.KeepConsumablesMinValue.ToString());
                textWriter.WriteElementString("keepConsumablesMaxValue", AutoTraderConfig.KeepConsumablesMaxValue.ToString());
                textWriter.WriteElementString("useInventorySpaceValue", AutoTraderConfig.UseInventorySpaceValue.ToString());
                textWriter.WriteElementString("keepHardwoodValue", AutoTraderConfig.KeepHardwoodValue.ToString());
                textWriter.WriteElementString("hardwoodBuyThresholdValue", AutoTraderConfig.HardwoodBuyThresholdValue.ToString());
                textWriter.WriteElementString("keepWagesValue", AutoTraderConfig.KeepWagesValue.ToString());
                textWriter.WriteElementString("searchRadiusValue", AutoTraderConfig.SearchRadiusValue.ToString());
                textWriter.WriteElementString("weaponsArmorTierValue", AutoTraderConfig.WeaponsArmorTierValue.ToString());

                textWriter.WriteElementString("useAltATValue", AutoTraderConfig.UseAltATValue.ToString());
                textWriter.WriteElementString("junkCattleValue", AutoTraderConfig.JunkCattleValue.ToString());
                textWriter.WriteElementString("sellFromBothRostersValue", AutoTraderConfig.SellFromBothRostersValue.ToString());
                textWriter.WriteElementString("resupplyValue", AutoTraderConfig.ResupplyValue.ToString());
                textWriter.WriteElementString("sellSmithingValue", AutoTraderConfig.SellSmithingValue.ToString());
                textWriter.WriteElementString("keepSmeltingValue", AutoTraderConfig.KeepSmeltingValue.ToString());
                textWriter.WriteElementString("resupplyHardwoodValue", AutoTraderConfig.ResupplyHardwoodValue.ToString());
                textWriter.WriteElementString("mountTroopsValue", AutoTraderConfig.MountTroopsValue.ToString());
                textWriter.WriteElementString("mountTroopsWarValue", AutoTraderConfig.MountTroopsWarValue.ToString());
                textWriter.WriteElementString("buyMountsValue", AutoTraderConfig.BuyMountsValue.ToString());
                textWriter.WriteElementString("sellMountsValue", AutoTraderConfig.SellMountsValue.ToString());
                textWriter.WriteElementString("keepWarMountsValue", AutoTraderConfig.KeepWarMountsValue.ToString());
                textWriter.WriteElementString("keepSpareMountsValue", AutoTraderConfig.KeepSpareMountsValue.ToString());
                textWriter.WriteElementString("maxHerdTradePercentValue", AutoTraderConfig.MaxHerdTradePercentValue.ToString());
                textWriter.WriteElementString("maxHorseGoldPerTypeValue", AutoTraderConfig.MaxHorseGoldPerTypeValue.ToString());
                textWriter.WriteElementString("portUseInventorySpaceValue", AutoTraderConfig.PortUseInventorySpaceValue.ToString());
                textWriter.WriteElementString("portMaxCapacityValue", AutoTraderConfig.PortMaxCapacityValue.ToString());
                textWriter.WriteElementString("portMaxItemCountValue", AutoTraderConfig.PortMaxItemCountValue.ToString());
                textWriter.WriteElementString("portMinBuyProfitValue", AutoTraderConfig.PortMinBuyProfitValue.ToString());
                textWriter.WriteElementString("minBuyProfitValue", AutoTraderConfig.MinBuyProfitValue.ToString());
                textWriter.WriteElementString("buyProfitOverrideValue", AutoTraderConfig.BuyProfitOverrideValue.ToString());

                textWriter.WriteElementString("buyHorsesValue", AutoTraderConfig.BuyHorsesValue.ToString());
                textWriter.WriteElementString("sellHorsesValue", AutoTraderConfig.SellHorsesValue.ToString());
                textWriter.WriteElementString("buyArmorValue", AutoTraderConfig.BuyArmorValue.ToString());
                textWriter.WriteElementString("sellArmorValue", AutoTraderConfig.SellArmorValue.ToString());
                textWriter.WriteElementString("buyWeaponsValue", AutoTraderConfig.BuyWeaponsValue.ToString());
                textWriter.WriteElementString("sellWeaponsValue", AutoTraderConfig.SellWeaponsValue.ToString());
                textWriter.WriteElementString("buyGoodsValue", AutoTraderConfig.BuyGoodsValue.ToString());
                textWriter.WriteElementString("sellGoodsValue", AutoTraderConfig.SellGoodsValue.ToString());
                textWriter.WriteElementString("buyConsumablesValue", AutoTraderConfig.BuyConsumablesValue.ToString());
                textWriter.WriteElementString("sellConsumablesValue", AutoTraderConfig.SellConsumablesValue.ToString());
                textWriter.WriteElementString("buyLivestockValue", AutoTraderConfig.BuyLivestockValue.ToString());
                textWriter.WriteElementString("sellLivestockValue", AutoTraderConfig.SellLivestockValue.ToString());

                textWriter.WriteElementString("debugMode", AutoTraderConfig.DebugMode.ToString());

                textWriter.WriteEndElement();
            }

            FileHelper.SaveFileString(AutoTraderConfig._configFile, sb.ToString());
        }
    }
}
