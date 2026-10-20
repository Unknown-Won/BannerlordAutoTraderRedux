using System;
using System.Reflection;
using TaleWorlds.CampaignSystem.Roster;
using TaleWorlds.CampaignSystem.Settlements;

namespace AutoTrader.Warsails
{
    /// <summary>
    /// Optional integration with the Cargo Holds mod (WarSailsCargoHolds).
    /// Everything is resolved through reflection so AutoTrader keeps working when the mod is not installed.
    /// </summary>
    public static class CargoHoldsCompat
    {
        private static bool _resolved = false;
        private static MethodInfo _getPlayerFleetHold;
        private static MethodInfo _isPlayerFleetDockedAt;
        private static MethodInfo _openFleetTrade;

        private static void Resolve()
        {
            if (_resolved)
                return;
            _resolved = true;

            try
            {
                foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
                {
                    var bridgeType = assembly.GetType("WarSailsCargoHolds.CaravaneerBridge");
                    var rerouterType = assembly.GetType("WarSailsCargoHolds.TradeRerouter");
                    if (bridgeType == null || rerouterType == null)
                        continue;

                    _getPlayerFleetHold = bridgeType.GetMethod("GetPlayerFleetHold", BindingFlags.Public | BindingFlags.Static);
                    _isPlayerFleetDockedAt = bridgeType.GetMethod("IsPlayerFleetDockedAt", BindingFlags.Public | BindingFlags.Static);
                    _openFleetTrade = rerouterType.GetMethod("OpenFleetTrade", BindingFlags.Public | BindingFlags.Static);
                    break;
                }
            }
            catch (Exception ex)
            {
                AutoTraderHelpers.PrintDebugMessage("CargoHoldsCompat: Error resolving Cargo Holds: " + ex.Message);
            }

            AutoTraderHelpers.PrintDebugMessage("CargoHoldsCompat: Cargo Holds available: " + IsAvailable.ToString());
        }

        /// <summary>
        /// True if the Cargo Holds mod is loaded and exposes everything AutoTrader needs.
        /// </summary>
        public static bool IsAvailable
        {
            get
            {
                Resolve();
                return _getPlayerFleetHold != null && _isPlayerFleetDockedAt != null && _openFleetTrade != null;
            }
        }

        /// <summary>
        /// Gets the player's fleet cargo hold, or null if Cargo Holds is not available.
        /// </summary>
        public static ItemRoster GetPlayerFleetHold()
        {
            if (!IsAvailable)
                return null;

            try
            {
                return _getPlayerFleetHold.Invoke(null, null) as ItemRoster;
            }
            catch (Exception ex)
            {
                AutoTraderHelpers.PrintDebugMessage("CargoHoldsCompat: Error getting fleet hold: " + ex.Message);
                return null;
            }
        }

        /// <summary>
        /// True if the player's fleet is docked at the given settlement.
        /// </summary>
        public static bool IsPlayerFleetDockedAt(Settlement settlement)
        {
            if (!IsAvailable || settlement == null)
                return false;

            try
            {
                return (bool)_isPlayerFleetDockedAt.Invoke(null, new object[] { settlement });
            }
            catch (Exception ex)
            {
                AutoTraderHelpers.PrintDebugMessage("CargoHoldsCompat: Error checking docked fleet: " + ex.Message);
                return false;
            }
        }

        /// <summary>
        /// Opens the trade screen between the settlement and the fleet's cargo hold.
        /// </summary>
        /// <returns>True if the trade screen was opened</returns>
        public static bool OpenFleetTrade(Settlement settlement)
        {
            if (!IsAvailable || settlement == null)
                return false;

            try
            {
                _openFleetTrade.Invoke(null, new object[] { settlement });
                return true;
            }
            catch (Exception ex)
            {
                AutoTraderHelpers.PrintDebugMessage("CargoHoldsCompat: Error opening fleet trade: " + ex.Message);
                return false;
            }
        }
    }
}
