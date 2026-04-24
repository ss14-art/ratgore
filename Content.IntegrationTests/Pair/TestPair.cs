#nullable enable
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Content.Client.IoC;
using Content.Client.Parallax.Managers;
using Content.IntegrationTests.Tests.Destructible;
using Content.IntegrationTests.Tests.DeviceNetwork;
using Content.Server.GameTicking;
using Content.Shared.CCVar;
using Content.Shared.Players;
using Robust.Shared.ContentPack;
using Robust.Shared.GameObjects;
using Robust.Shared.IoC;
using Robust.Shared.Log;
using Robust.Shared.Network;
using Robust.Shared.Reflection;
using Robust.Shared.Testing;
using Robust.Shared.Utility;
using Robust.UnitTesting;

namespace Content.IntegrationTests.Pair;

/// <summary>
/// This object wraps a pooled server+client pair.
/// </summary>
public sealed partial class TestPair : RobustIntegrationTest.TestPair
{
    private List<NetUserId> _modifiedProfiles = new();

    public ContentPlayerData? PlayerData => Player?.Data.ContentData();

    protected override async Task Initialize()
    {
        await base.Initialize();

        // Prevent info log spam in some tests (particularly SpawnAndDeleteAllEntitiesOnDifferentMaps)
        Server.System<SharedMapSystem>().Log.Level = LogLevel.Warning;
        Client.EntMan.EntitySysManager.SystemLoaded += (_, e) =>
        {
            if (e.System is SharedMapSystem map)
                map.Log.Level = LogLevel.Warning;
        };

        var settings = (PoolSettings)Settings;
        if (!settings.DummyTicker)
        {
            var gameTicker = Server.System<GameTicker>();
            await Server.WaitPost(() => gameTicker.RestartRound());
        }
    }

    public override async Task RevertModifiedCvars()
    {
        // I just love order dependent cvars
        // I.e., cvars that when changed automatically cause others to also change.
        var modified = ModifiedServerCvars.TryGetValue(CCVars.PanicBunkerEnabled.Name, out var panik);

        await base.RevertModifiedCvars();

        if (!modified)
            return;

        await Server.WaitPost(() => Server.CfgMan.SetCVar(CCVars.PanicBunkerEnabled.Name, panik!));
        ClearModifiedCvars();
    }

    protected override async Task ApplySettings(IIntegrationInstance instance, PairSettings n)
    {
        var next = (PoolSettings)n;
        await base.ApplySettings(instance, next);
        var cfg = instance.CfgMan;
        await instance.WaitPost(() =>
        {
            if (cfg.IsCVarRegistered(CCVars.GameDummyTicker.Name))
                cfg.SetCVar(CCVars.GameDummyTicker, next.DummyTicker);

            if (cfg.IsCVarRegistered(CCVars.GameLobbyEnabled.Name))
                cfg.SetCVar(CCVars.GameLobbyEnabled, next.InLobby);

            if (cfg.IsCVarRegistered(CCVars.GameMap.Name))
                cfg.SetCVar(CCVars.GameMap, next.Map);

            if (cfg.IsCVarRegistered(CCVars.AdminLogsEnabled.Name))
                cfg.SetCVar(CCVars.AdminLogsEnabled, next.AdminLogsEnabled);
        });
    }

    protected override RobustIntegrationTest.ClientIntegrationOptions ClientOptions()
    {
        var opts = base.ClientOptions();

        opts.LoadTestAssembly = false;
        opts.ContentStart = true;
        opts.FailureLogLevel = LogLevel.Warning;
        opts.Options = new()
        {
            LoadConfigAndUserData = false,
        };

        opts.BeforeStart += () =>
        {
            IoCManager.Resolve<IModLoader>().SetModuleBaseCallbacks(new ClientModuleTestingCallbacks
                {
                    ClientBeforeIoC = () => IoCManager.Register<IParallaxManager, DummyParallaxManager>(true)
                });
        };

        opts.BeforeRegisterComponents += () =>
        {
            var refl = IoCManager.Resolve<IReflectionManager>();
            var modLoader = IoCManager.Resolve<IModLoader>();

            var loaded = refl.Assemblies.Select(a => a.GetName().Name).ToHashSet();
            var goobAsms = PoolManager.Client.Concat(PoolManager.Shared)
                .Where(a => a.GetName().Name!.Contains("Goobstation"))
                .ToArray();

            var toLoadReflection = goobAsms
                .Where(a => !loaded.Contains(a.GetName().Name))
                .ToArray();

            refl.LoadAssemblies(toLoadReflection);

            foreach (var asm in goobAsms)
            {
                var iocClasses = asm.GetTypes().Where(t => t.Name.Contains("Goob") && t.Name.Contains("IoC")).ToArray();
                foreach (var iocClass in iocClasses)
                {
                    var registerMethod = iocClass.GetMethod("Register", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
                    if (registerMethod != null)
                    {
                        registerMethod.Invoke(null, null);
                    }
                }
            }

            var addMethod = modLoader.GetType().GetMethod("AddModWithoutInit", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (addMethod != null)
            {
                foreach (var asm in goobAsms)
                {
                    if (!modLoader.LoadedModules.Contains(asm))
                        addMethod.Invoke(modLoader, new object[] { asm });
                }
            }
        };

        return opts;
    }

    protected override RobustIntegrationTest.ServerIntegrationOptions ServerOptions()
    {
        var opts = base.ServerOptions();

        opts.LoadTestAssembly = false;
        opts.ContentStart = true;
        opts.Options = new()
        {
            LoadConfigAndUserData = false,
        };

        opts.BeforeStart += () =>
        {
            // Server-only systems (i.e., systems that subscribe to events with server-only components)
            // There's probably a better way to do this.
            var entSysMan = IoCManager.Resolve<IEntitySystemManager>();
            entSysMan.LoadExtraSystemType<DeviceNetworkTestSystem>();
            entSysMan.LoadExtraSystemType<TestDestructibleListenerSystem>();
        };

        opts.BeforeRegisterComponents += () =>
        {
            var refl = IoCManager.Resolve<IReflectionManager>();
            var modLoader = IoCManager.Resolve<IModLoader>();

            var loaded = refl.Assemblies.Select(a => a.GetName().Name).ToHashSet();
            var goobAsms = PoolManager.Server.Concat(PoolManager.Shared)
                .Where(a => a.GetName().Name!.Contains("Goobstation"))
                .ToArray();

            var toLoadReflection = goobAsms
                .Where(a => !loaded.Contains(a.GetName().Name))
                .ToArray();

            refl.LoadAssemblies(toLoadReflection);

            foreach (var asm in goobAsms)
            {
                var iocClasses = asm.GetTypes().Where(t => t.Name.Contains("Goob") && t.Name.Contains("IoC")).ToArray();
                foreach (var iocClass in iocClasses)
                {
                    var registerMethod = iocClass.GetMethod("Register", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
                    if (registerMethod != null)
                    {
                        registerMethod.Invoke(null, null);
                    }
                }
            }

            var addMethod = modLoader.GetType().GetMethod("AddModWithoutInit", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (addMethod != null)
            {
                foreach (var asm in goobAsms)
                {
                    if (!modLoader.LoadedModules.Contains(asm))
                        addMethod.Invoke(modLoader, new object[] { asm });
                }
            }
        };

        return opts;
    }

    public async ValueTask<TestMapData> LoadTestMap(ValueType testMapPath)
    {
        throw new NotImplementedException();
    }
}
