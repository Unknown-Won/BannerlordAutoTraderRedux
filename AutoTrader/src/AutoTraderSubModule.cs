using TaleWorlds.Core;
using TaleWorlds.MountAndBlade;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Localization;
using TaleWorlds.CampaignSystem.GameState;
using TaleWorlds.Library;
using TaleWorlds.InputSystem;
using AutoTrader.GUI;

namespace AutoTrader
{
    public class AutoTraderSubModule : MBSubModuleBase
    {
		private AutoTraderLogic _autoTraderLogic;

		public AutoTraderSubModule()
		{
			//var harmony = new Harmony("eskalior.autotrader");
			//harmony.PatchAll(typeof(AutoTraderSubModule).Assembly);
		}

		protected override void OnSubModuleLoad()
		{
			base.OnSubModuleLoad();
			AutoTraderConfig.Initialize();
			AutoTraderHelpers.Initialize();
			_autoTraderLogic = new AutoTraderLogic(new AutoTraderLogicConnector());
		}

		public override void OnGameInitializationFinished(Game game)
		{
			string hotkey = AutoTraderConfig.UseAltATValue ? "<ALT + A + T>" : "<ALT + A>";
			AutoTraderHelpers.PrintMessage(new TextObject("{=ATStartup}Thanks for using AutoTrader! Press {HOTKEY} to open the settings menu", null)
				.SetTextVariable("HOTKEY", hotkey).ToString());

			string fullVersion = ApplicationVersion.FromParametersFile(null).ToString();
			string[] versionParts = fullVersion.Split('.');
			string version = versionParts.Length >= 3 
				? $"{versionParts[0]}.{versionParts[1]}.{versionParts[2]}" 
				: fullVersion;

			if (!version.Equals(AutoTraderConfig.AutoTraderGameVersion)){
				AutoTraderHelpers.PrintMessage(new TextObject("{=ATVersionMismatch}You are using AutoTrader for {MOD_GAME_VERSION} with Bannerlord {GAME_VERSION}. If you encounter issues please check the mod page for a fitting version.", null)
					.SetTextVariable("MOD_GAME_VERSION", AutoTraderConfig.AutoTraderGameVersion)
					.SetTextVariable("GAME_VERSION", version).ToString());
			}

			if (AutoTraderConfig.DebugMode)
			{
				AutoTraderHelpers.PrintMessage("WARNING: AutoTrader debug mode is active. This will make autotrading drastically slower.");
			}
		}

		private void OpenSettingsMenu()
		{
			Game.Current.GameStateManager.PushState(Game.Current.GameStateManager.CreateState<AutoTraderState>(), 0);
		}


		protected override void OnApplicationTick(float dt)
		{
			if (Game.Current != null)
			{
				_autoTraderLogic.OnApplicationTick();

				if (!_autoTraderLogic.IsTradingActive
					&& Input.IsKeyDown(InputKey.LeftAlt)
					&& Input.IsKeyDown(InputKey.A)
					&& (!AutoTraderConfig.UseAltATValue || Input.IsKeyDown(InputKey.T))
					&& Game.Current.GameStateManager.ActiveState.GetType() == typeof(MapState)
					&& Game.Current.GameStateManager.ActiveState.IsMenuState == false
					&& Game.Current.GameStateManager.ActiveState.GetType() != typeof(MissionState))
				{
					OpenSettingsMenu();
				}
			}
		}

		protected override void InitializeGameStarter(Game game, IGameStarter gameStarterObject)
		{
			if (game.GameType is Campaign)
			{
				CampaignGameStarter gameInitializer = (CampaignGameStarter)gameStarterObject;
				this.AddBehaviors(gameInitializer);
			}
		}

		private void AddBehaviors(CampaignGameStarter gameStarterObject)
		{
			gameStarterObject.AddBehavior(new TradeBehavior(_autoTraderLogic));
		}

	}
}
