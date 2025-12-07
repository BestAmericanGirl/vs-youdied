using System;
using ConfigLib;
using Vintagestory.API.Client;
using Vintagestory.API.Common;

namespace YouDied
{
    public class YouDiedModSystem : ModSystem
    {
        public static event Action SettingsChanged;
        private YouDiedRenderer youDiedRenderer = null!;

        public override void StartClientSide(ICoreClientAPI api)
        {
            try
            {
                YouDiedConfig.Instance = api.LoadModConfig<YouDiedConfig>(YouDiedConfig.ConfigName) ?? new YouDiedConfig();
                api.StoreModConfig(YouDiedConfig.Instance, YouDiedConfig.ConfigName);
            }
            catch (Exception) { YouDiedConfig.Instance = new YouDiedConfig(); }

            if (api.ModLoader.IsModEnabled("configlib"))
            {
                SubscribeToConfigChange(api);
            }

            youDiedRenderer = new YouDiedRenderer(api);

            api.Event.RegisterRenderer(youDiedRenderer, EnumRenderStage.Ortho);

            api.Event.PlayerDeath += (IClientPlayer player) =>
            {
                // Only fire effect if we're the one dying
                if (api.World.Player.Entity != player.Entity)
                {
                    return;
                }
                youDiedRenderer.TriggerDeath();
            };

            Mod.Logger.Notification("YouDied: Client loaded.");
        }

        public override void AssetsFinalize(ICoreAPI api)
        {
            if (api.World.Side != EnumAppSide.Client)
            {
                return;
            }

            youDiedRenderer.LoadSound();
        }

        private void SubscribeToConfigChange(ICoreAPI api)
        {
            ConfigLibModSystem system = api.ModLoader.GetModSystem<ConfigLibModSystem>();

            system.SettingChanged += (domain, config, setting) =>
            {
                if (domain != "youdied") return;
                setting.AssignSettingValue(YouDiedConfig.Instance);
                SettingsChanged?.Invoke();
            };
            system.ConfigsLoaded += () =>
            {
                system.GetConfig("youdied")?.AssignSettingsValues(YouDiedConfig.Instance);
            };
        }
    }
}
