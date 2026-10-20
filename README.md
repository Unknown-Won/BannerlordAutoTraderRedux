# AutoTrader Redux

![AutoTraderBanner](images/AutoTraderWarsails.png)

AutoTrader Redux is a continuation of the AutoTrader mod for Mount & Blade II: Bannerlord, updated for **Bannerlord v1.4.8** with War Sails and Cargo Holds support, smarter buying, horse and herd management, and a tabbed settings screen.

## Credits

AutoTrader was created by **[Eskalior](https://github.com/Eskalior)**. All credit for the original mod goes to them:

- Original source: [Eskalior/BannerlordAutoTrader](https://github.com/Eskalior/BannerlordAutoTrader)
- Original mod page: [AutoTrader on Nexus Mods](https://www.nexusmods.com/mountandblade2bannerlord/mods/135)
- Original Steam Workshop page: [AutoTrader](https://steamcommunity.com/sharedfiles/filedetails/?id=2875660601)

Redux is maintained by [Unknown-Won](https://github.com/Unknown-Won), building on Eskalior's code and the contributions to the original repository.

## What's new in Redux

The full list is in [CHANGELOG.md](CHANGELOG.md). Highlights:

**Ports, War Sails and Cargo Holds**
- Autotrade in ports uses fleet capacity automatically.
- Optional Cargo Holds support, with separate autotrade buttons for the cargo hold and the party inventory.
- Each port button can also sell from the other inventory first, settled when you press Done.
- Its own Ports settings tab: cargo hold space, per-good limits and minimum profit.

**Smarter buying and selling**
- Trades in a fixed order: sell, restock food, resupply hardwood, horses for troops on foot, then regular buying, then a second sell pass.
- Buys one unit at a time of whatever is most profitable right now, ranked by profit per 10 weight.
- Profit uses the average of the 5 best sell prices in range (straight-line distance), counting towns only, so villages no longer skew prices.
- New "Minimum profit per 10 weight" and "Buy anyway if profit per item is at least" sliders.
- Sells the best deals first and won't sell below what you paid unless no better market is in range.

**Horses and herding**
- Every animal purchase respects the game's own herd formula.
- Modded trade animals count as livestock.
- Trade riding horses, buy horses for troops on foot (optionally war mounts), and keep a minimum of war mounts and spare horses. The kept horses are always the cheapest ones.
- Caps on total herd % and on gold tied up in each horse type.

**Hardwood**
- Its own keep amount and price slider, separate from consumables.

**Settings screen**
- Split into General, Trading, Supplies & Horses and Ports tabs.
- Resupply toggles work independently of the trade toggles.

## Installation

Download the release from the AutoTrader Redux Nexus Mods page, or build from source (see [Contributing](#contributing) below).

AutoTrader Redux replaces the original AutoTrader. Remove the original `AutoTrader` folder from your `Modules` folder before installing.

## Contributing

Contributions are welcome! Please see [CONTRIBUTING.md](CONTRIBUTING.md) for detailed setup instructions.

**Quick Start:**

1. Clone this repository
2. Copy `AutoTrader/paths.props.user.template` to `AutoTrader/paths.props.user`
3. Edit `paths.props.user` and set your Bannerlord installation path
4. You are ready to build!

## License

MIT License - see [LICENSE.txt](LICENSE.txt) for details. The original copyright notice from Eskalior is kept as the license requires.
