﻿using Stride.BepuPhysics.Configurations;
using Stride.Core;
using Stride.Engine.Design;
using Stride.Games;

namespace Stride.BepuPhysics
{
    internal static class BepuServicesHelper
    {
        public static void LoadBepuServices(IServiceRegistry services)
        {
            var bepuConfig = services.GetService<BepuConfiguration>();
            if (bepuConfig == null)
            {
                var gameSettings = services.GetService<IGameSettingsService>();
                if (gameSettings != null)
                    bepuConfig = gameSettings.Settings.Configurations.Get<BepuConfiguration>();
                else
                    bepuConfig = new();

                if (bepuConfig.BepuSimulations.Count == 0)
                {
                    bepuConfig.BepuSimulations.Add(new BepuSimulation());
                }
                services.AddService(bepuConfig);
            }

            var bepuShapeCacheSys = services.GetService<BepuShapeCacheSystem>();
            if (bepuShapeCacheSys == null)
            {
                services.AddService(new BepuShapeCacheSystem(services));
            }

            var gameSystems = services.GetService<IGameSystemCollection>();
            if (gameSystems != null)
            {
                if (!gameSystems.Any(e => e is BepuPhysicsGameSystem))
                {
                    gameSystems.Add(new BepuPhysicsGameSystem(services));
                }
            }
        }
    }
}
