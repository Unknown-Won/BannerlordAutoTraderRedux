# AutoTrader changelog (2026-10-14)

Target game version is now **Bannerlord v1.4.8** (was v1.4.6).

## Fixes
- **Locked items (2026-10-27):** items locked in the inventory are no longer sold when an unlocked item with the same name is also carried (for example a locked sword and a rusty copy of it). Selling now tells such items apart by their modifier.
- **Resupply toggles are independent (2026-10-16):** "Resupply" (food) and "Resupply hardwood" no longer switch "Trade Consumables" or "Trade Goods" buying on, and turning those off no longer turns resupply off. Resupply tops up food and hardwood to their keep amounts even when buying consumables or goods for trade is off.
- **Settings menu order:** "Basic Options" and "Buy / Sell Options" now sit at the top of their columns. The rows below them keep their previous order.
- **Settings menu fit:** shorter slider and checkbox rows, so the bottom option is no longer hidden behind the Cancel button.
- **Herding limit:** every animal purchase now uses the game's own herd formula (pack animals + livestock + mounts beyond your men on foot). It stops buying before the herd exceeds your party size.
- **Modded livestock:** any trade animal (item type Animal), including ones added by other mods, now counts as livestock.
- **Version warning:** the in-game "AutoTrader for v1.4.6" warning no longer shows on v1.4.8.
- **Pack horse sell toggle:** it no longer sells riding horses by mistake.

## New defaults (2026-10-20)
These defaults apply to new settings files.
- Buy under X% of the average sell price: 75 (was 90)
- Sell above X% of the average sell price: 90 (was 100)
- Green/red-based trading: off (was on)
- Maximum % of inventory filled by the same good: 20 (was 15)
- Maximum % of cargo hold filled by the same good: 30 (was 15)
- Maximum amount of grain to keep: 50 (was 100)
- Maximum amount of other consumables to keep: 50 (was 20)
- Keep enough gold for X days of troop wages: 7 (was 3)
- Sell charcoal and ingots: on
- Keep smeltable weapons: on
- Resupply hardwood: on
- Trade Riding Horses, buy and sell: on
- Trade Pack Horses, sell: on
- Trade Weapons, buy: on
- Trade Armor, buy: on
- Trade Livestock, buy: on
- Minimum spare riding horses to keep: 5 (was 0)

## Ports, War Sails and Cargo Holds
- **Trade total includes the other inventory (2026-10-16):** in a port, "You will get/pay X" and your gold on the trade screen now include the pending trade from the other inventory, so they show the full amount Done will pay out.
- **Fleet capacity:** autotrade in a port uses fleet capacity automatically. The "Use fleet capacity" toggle was removed.
- **Cargo Holds support:** optional, found at runtime and declared as an optional dependency in SubModule.xml.
- **Port buttons with Cargo Holds:** the port menu offers these, grouped together:
  - Automatically trade wares (cargo hold)
  - Automatically trade wares (party inventory)
  - Trade (cargo hold): the War Sails "Trade" button, renamed
  - Trade (party inventory)
- **Cargo hold trades:**
  - Use the fleet's hold and fleet capacity.
  - Are greyed out if the fleet isn't docked.
  - Animals bought into the hold don't count toward the herd.
  - Food resupply and the food keep amounts are skipped, because food in the hold isn't food supply.
- **Selling from both (2026-10-15):** both port autotrade buttons first sell from the other roster (the party inventory for the cargo hold button, the cargo hold for the party inventory button), then trade as before. All buying still goes into the roster whose button you pressed. That first sale runs without a trade screen and is only paid out when you press Done on the trade screen; Cancel puts the items back. Until then its gold can't be spent on buying. It only runs while the fleet is docked at that town, and can be turned off with the new "Ports: also sell from the other inventory" toggle (on by default).
- **Party food from the cargo hold button (2026-10-15):** "Automatically trade wares (cargo hold)" also buys food into the party inventory up to the food keep minimums (if Resupply is on). Like the sale from the other inventory, it is settled on Done and undone on Cancel. On the trade screen, the merchant's gold already counts the side trade, and the transaction tooltip (hover over Done) lists the screen's own trade and the side trade under separate "Party inventory" and "Cargo hold" headings, with a divider between them.
- **Without Cargo Holds:** ports keep the single autotrade button, using fleet capacity.

## Hardwood
- **New slider "Amount of hardwood to keep":** default 30, maximum Inf. The resupply toggle now uses it instead of the consumables value.
- **New slider "Buy hardwood to keep under % of the average price":** default 110, maximum 200.
- **Below the keep amount:**
  - Buys only under the hardwood % slider above, even with green/red trading on.
  - Respects "Total % of inventory space".
  - Ignores "Maximum % filled by the same good".
- **Above the keep amount:** hardwood trades like any other good.
- **Inf:** buys all hardwood that passes the price check.
- **Selling:** never sells below the keep amount.
- **Where it counts:** each trade counts only the inventory it trades with (cargo hold or party).

## Horses
- **Trade Pack Horses** (renamed from "Trade Horses (buys only carry horses)"): unchanged, and both toggles still do nothing.
- **Trade Riding Horses (new, buy and sell):**
  - Trades any riding horse under the normal buy/sell price rules.
  - Respects the herding limit.
  - On by default (2026-10-20).
- **Buy and keep riding horses for troops on foot (new, on by default):**
  - Buys the cheapest regular horses until every man on foot has one, for the speed bonus.
  - Ignores the buy price threshold but keeps your wage reserve.
- **Allow war mounts for troops on foot (new, off by default):** lets the option above buy war mounts too. Noble mounts are never bought for this.
- **Selling with either option above on:** a riding horse is never sold if that would leave a man on foot.
- **Minimum war mounts to keep (new slider, 0 to 50, default 0):** stops war mounts from being sold below that count, for troop upgrades. It never buys any.
- **Cargo hold trades:** none of the horse rules above apply.
- **New slider "Maximum herd size for trading, % of the herding limit"** (0 to 100, default 100): trade buying of pack animals, livestock and spare riding horses stops once the herd reaches this % of the herding limit (party size). Horses bought for men on foot don't count toward the herd, as in the game, so they are still bought (2026-10-16).
- **New slider "Maximum gold in each horse type for trading"** (0 to 200,000 in steps of 5,000, then Inf; default 75,000): a horse (riding horse or pack animal) isn't bought if the ones you'd then own of that type would be worth more than this at the current buy price. Cheap horses can be bought in bulk while expensive ones like chargers stop after a few. It counts every horse of that type in the inventory being traded, including ones your men on foot ride. Livestock isn't limited. (Replaced the count-based "Maximum horses of each type" slider on 2026-10-16.)
- **AveragePriceFallback log line:** now only printed when no market has a price and the item's base value is used instead.
- **New slider "Minimum spare riding horses to keep"** (0 to 100, default 5): riding horses beyond the men on foot aren't sold below this number, so troops hired later have horses. It never buys horses.
- **Kept horses are the cheapest (2026-10-16):** the riding horses kept for men on foot and as spare horses are now the ones that sell for least in the current market, so expensive horses like a Charger can still be sold. Before, the first horses checked were sold and the rest kept. "Minimum war mounts to keep" works the same way: it keeps the cheapest war mounts. Those are set aside first, so they don't also count as horses for men on foot or as spare horses.

## Buying (2026-10-15)
- **Trade order:** selling runs first so all buying can use the freed space. Then food up to its keep minimum, hardwood up to its keep amount, horses for troops on foot, and regular buying sorted by profit per 10 weight, so light goods compete fairly with heavy ones.
- **Locked items:** locks now only stop selling, so locked hardwood is resupplied.
- **New slider "Minimum profit per 10 weight to buy"** (0 to 100, default 10): regular buying skips items whose profit (average price minus buy price) per 10 weight is below it. Horses and livestock count as weighing 10. Food, hardwood and horses bought to reach their keep amounts ignore it.
- **Average prices split:** profit (buy order and minimum profit) now uses the average SELL price elsewhere minus the buy price here. "Buy under X% of the average sell price" compares against the average SELL price elsewhere (renamed), the hardwood % against the average BUY price; "Sell above X%" compares against the average SELL price elsewhere. Previously all of these used one average of both.
- **Smarter buying:** buys one unit at a time, always of the item that is most profitable right now, since prices rise as you buy. Items are ranked by profit per share of whichever runs out first for them, inventory space or gold.
- **New slider "Buy anyway if profit per item is at least"** (0 to 10,000 gold in steps of 100, default 1,000, 0 = off): an item over the "Buy under X%" limit is still bought if its profit per item (expected sell price minus buy price) reaches this amount, so expensive goods and horses with a big gold profit aren't skipped. Applies to regular buying and riding horses. The minimum profit per 10 weight still applies.
- **Food minimum when selling:** selling now stops at the minimum to keep instead of one below it.
- **Debug log:** the buy loop's "expected sell price" line is now "highest sell price in range".
- **Top 5 sell prices in range (2026-10-16):** profit always uses the average sell price of the 5 best markets in range, this one included, instead of the average or the single best market, whose price drops fast if it has a shortage. The "best nearby sell price" toggle was removed.
- **Labels:** "Sell above X% of the average sell price", "Buy anyway if profit per item is at least", "Trade Pack Horses (TBD)".
- **Villages left out of price averages:** a village prices goods exactly like the town it trades with, so counting villages repeated each town's price once per village (the top 5 for Hunter was one town and its villages). Only towns count now, plus the village you are trading in. The log lists the top 5 sell and buy markets with their price and distance.
- **Buy prices use stock:** the buy price averages (in range, global and top 5) only count towns that have the item in stock, since you can't buy anywhere else. Sell prices count every town. Villages other than the one you trade in are never counted.
- **Food maximums in the cargo hold (2026-10-16):** cargo hold trades no longer buy grain or other consumables past "Maximum amount of grain to keep" and "Maximum amount of other consumables to keep", counting what is in the hold. The minimums still only apply to the party inventory.
- **"Minimum profit per 10 weight to buy" default:** now 40 (was 10).
- **Range is a straight line (2026-10-16):** the search radius is now measured in a straight line from the party, like the game's map distance, instead of along land paths. Towns across the sea were left out or counted as far away before.
- **Debug log prices:** each price is on its own line (average in range, global average, top 5 in range) with the number of markets counted, and the top 5 lines list the towns with their price (and stock for buying), also when the prices come from the cache.
- **Second sell pass:** after buying, AutoTrader sells once more, since the merchant now has the gold from what you bought.
- **Debug log prices:** for each item it logs the buy and sell averages in range, the global averages over all markets, and the top 5 averages in range (highest sell prices, lowest buy prices).
- **Selling:** best deals (highest price compared to elsewhere) are sold first, so a merchant short on gold spends it on them. Goods are not sold below what you paid, unless no other market in range pays more.
- **Faster trades:** each item's prices elsewhere are worked out once per trade instead of once per pass. They are stored by item ID, not name, so two different items with the same name (such as two "Hare" items) no longer share prices. The debug log now shows each item's ID.
- **Debug mode hot reload:** `debugMode` in AutoTraderConfig.xml is re-read before every autotrade, so it can be switched while the game runs.
- **Debug log:** each item's price, average price and profit are logged even when it is skipped.

## Settings screen (2026-10-16)
- **Tabs:** the settings are split into three tabs. General has prices, gold and space, ports and the hotkey. Trading has the buy and sell toggles and the buying and selling sliders. Supplies & Horses has food, hardwood, resupply, horses and herd. Presets, Cancel and Done stay on every tab.
- **Ports tab:** holds "Ports: also sell from the other inventory" and three cargo hold settings, used whenever you trade with the cargo hold: "Total % of cargo hold space the mod may use" (0 to 150, default 90), "Maximum % of cargo hold filled by the same good" (default 30) and "Maximum amount of the same good in the cargo hold" (0 to 1,000 in steps of 10, default Inf). The good stops at whichever of the last two is lower.
- **Herd setting:** renamed "Total % of herd size the mod may use", 0 to 150 like the inventory space setting, default 90.
- **Horses of each type:** default 10; "Inf" is now at the right end of the slider. Its saved setting was renamed, so an older "No limit" (0) isn't read as "buy none".
- **Label:** "Buy and keep war mounts for troops on foot".
- **Minimum profit per 10 weight:** the default is now 50. The Ports tab has its own "Minimum profit per 10 weight to buy in the cargo hold" (default 40), used whenever you trade with the cargo hold.
- **Cargo hold wording and defaults:** the Ports tab settings say "cargo hold" instead of "fleet inventory". New defaults: "Amount of hardwood to keep" 100, "Total % of cargo hold space the mod may use" 100, "Maximum amount of the same good in the cargo hold" 300.
- **Label:** "Buy hardwood to keep under % of the average buying price".

## Build setup
- **`paths.props.user`:** added and pointed at your game install.
- **Deploy folder:** the build copies the mod to `AutoTrader\Deploy\AutoTrader` instead of the game's Modules folder.
- **Solution config:** the "Any CPU" configurations now build the x64 settings.

## Untested in game
- Port button grouping and the "Trade (cargo hold)" rename.
- All horse options.
- Herd check while buying several horses in one trade.
