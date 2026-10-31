using System;
using System.Collections.Generic;
using System.Reflection;
using TaleWorlds.CampaignSystem.GameMenus;
using TaleWorlds.CampaignSystem.GameState;
using TaleWorlds.Localization;

namespace AutoTrader
{
    /// <summary>
    /// Adds an "Automatically trade wares" button next to every "Trade" option that another mod adds to a game menu,
    /// for example the castle market of Castle Market Towns or the black markets and hideout traders of Guilds of Calradia.
    /// A trade option is any menu option that shows the game's trade icon (LeaveType.Trade). The button runs the mod's own
    /// option, and when that opens a trade screen, autotrades on that screen. Options of the base game are left alone,
    /// since AutoTrader adds its own buttons for town, village and port trading.
    /// </summary>
    public class ModdedTradeMenus
    {
        private const string OptionIdPrefix = "autotrade_mod_";

        // Menus AutoTrader already handles itself (Cargo Holds reroutes the port's own Trade option)
        private static readonly HashSet<string> HandledMenus = new HashSet<string> { "port_menu", "naval_storyline_virtualport" };

        // Assemblies of the base game and its DLCs, whose trade options AutoTrader covers with its own buttons
        private static readonly string[] BaseGameAssemblyPrefixes = { "TaleWorlds.", "SandBox", "StoryMode", "NavalDLC" };

        // "menu/option" of options that turned out not to open a trade screen, so their button is hidden for this session
        private static readonly HashSet<string> NotTradeScreens = new HashSet<string>();

        // Options whose condition was already asked for the trade icon, so each is only asked once
        private static readonly HashSet<GameMenuOption> ProbedOptions = new HashSet<GameMenuOption>();

        private readonly AutoTraderLogic _autoTraderLogic;

        public ModdedTradeMenus(AutoTraderLogic autoTraderLogic)
        {
            _autoTraderLogic = autoTraderLogic;
        }

        /// <summary>
        /// Called each time a game menu opens, so options that mods add at any time are found.
        /// </summary>
        public void AddAutoTradeOptions(GameMenu menu, MenuContext menuContext)
        {
            if (menu == null || HandledMenus.Contains(menu.StringId))
                return;
            try
            {
                List<GameMenuOption> options = menu.MenuOptions as List<GameMenuOption>;
                if (options == null)
                    return;

                var tradeOptions = new List<GameMenuOption>();
                foreach (GameMenuOption option in options)
                {
                    if (IsModdedTradeOption(option, menuContext))
                        tradeOptions.Add(option);
                }

                foreach (GameMenuOption tradeOption in tradeOptions)
                {
                    string id = OptionIdPrefix + tradeOption.IdString;
                    if (options.Exists(o => o.IdString == id))
                        continue;

                    // With several trade options in one menu, say which one each button uses
                    TextObject text;
                    if (tradeOptions.Count > 1 && tradeOption.Text != null)
                    {
                        text = new TextObject("{=ATTradeWithOption}Automatically trade wares ({OPTION})", null);
                        text.SetTextVariable("OPTION", tradeOption.Text);
                    }
                    else
                    {
                        text = new TextObject("{=ATTrade}Automatically trade wares", null);
                    }

                    string key = GetKey(menu, tradeOption);
                    GameMenuOption autoTradeOption = new GameMenuOption(GameMenu.MenuAndOptionType.RegularMenuOption, id, text, text,
                        args => AutoTradeCondition(tradeOption, key, args),
                        args => AutoTradeConsequence(tradeOption, key, args),
                        false, false, tradeOption.RelatedObject);

                    // Right before the mod's own option, like the town and port buttons
                    options.Insert(options.IndexOf(tradeOption), autoTradeOption);
                    AutoTraderHelpers.PrintDebugMessage("Added an autotrade button for '" + key + "'");
                }
            }
            catch (Exception ex)
            {
                AutoTraderHelpers.PrintDebugMessage("AddAutoTradeOptions: " + ex.Message);
            }
        }

        private static string GetKey(GameMenu menu, GameMenuOption option)
        {
            return menu.StringId + "/" + option.IdString;
        }

        private static bool IsModdedTradeOption(GameMenuOption option, MenuContext menuContext)
        {
            if (option == null || option.IdString == null || option.IdString.StartsWith(OptionIdPrefix) || option.OnConsequence == null)
                return false;
            if (IsBaseGameOrOwnCode(option.OnConsequence))
                return false;

            // The game sets the leave type from the option's condition each time it shows the menu
            if (option.OptionLeaveType == GameMenuOption.LeaveType.Trade)
                return true;
            // Not shown yet: ask the condition once, which is where mods set the trade icon.
            // Once the game has shown the option, OptionLeaveType above tells.
            if (option.OnCondition == null || !ProbedOptions.Add(option))
                return false;
            try
            {
                var args = new MenuCallbackArgs(menuContext, option.Text);
                option.OnCondition(args);
                return args.optionLeaveType == GameMenuOption.LeaveType.Trade;
            }
            catch
            {
                // Some conditions need the menu to be set up first; the option is found the next time the menu opens
                return false;
            }
        }

        private static bool IsBaseGameOrOwnCode(Delegate handler)
        {
            Assembly assembly = handler.Method?.DeclaringType?.Assembly;
            if (assembly == null)
                return true;
            if (assembly == typeof(ModdedTradeMenus).Assembly)
                return true;
            string name = assembly.GetName().Name ?? "";
            foreach (string prefix in BaseGameAssemblyPrefixes)
            {
                if (name.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                    return true;
            }
            return false;
        }

        private static bool AutoTradeCondition(GameMenuOption tradeOption, string key, MenuCallbackArgs args)
        {
            if (NotTradeScreens.Contains(key))
                return false;

            // Shown, enabled and explained exactly like the mod's own option
            bool shown = true;
            try
            {
                if (tradeOption.OnCondition != null)
                    shown = tradeOption.OnCondition(args);
            }
            catch (Exception ex)
            {
                AutoTraderHelpers.PrintDebugMessage("AutoTradeCondition '" + key + "': " + ex.Message);
                shown = false;
            }
            args.optionLeaveType = GameMenuOption.LeaveType.Trade;
            return shown;
        }

        private void AutoTradeConsequence(GameMenuOption tradeOption, string key, MenuCallbackArgs args)
        {
            // Opens the mod's trade screen, with whatever the mod sets up for it
            tradeOption.OnConsequence(args);

            if (_autoTraderLogic.TryPerformAutoTradeOnTradeScreen())
                return;

            // Some options open the screen a moment later
            _autoTraderLogic.AutoTradeWhenTradeScreenOpens(() =>
            {
                NotTradeScreens.Add(key);
                AutoTraderHelpers.PrintMessage(new TextObject("{=ATNoTradeScreen}AutoTrader: this option did not open a trade screen, so its autotrade button is hidden for now.", null).ToString());
            });
        }
    }
}
