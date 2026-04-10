// hud.offer_mode_indicators_point_show -> Port From SS14 Corvax-Next

using Robust.Shared.Configuration;

namespace Content.Shared._Forge.ForgeVars;

/// <summary>
///     Corvax modules console variables
/// </summary>
[CVarDefs]
// ReSharper disable once InconsistentNaming
public sealed class ForgeVars
{
    public static readonly CVarDef<string> DiscordApiUrl =
        CVarDef.Create("jerry.discord_api_url", "", CVar.CONFIDENTIAL | CVar.SERVERONLY);

    public static readonly CVarDef<bool> DiscordAuthEnabled =
        CVarDef.Create("jerry.discord_auth_enabled", false, CVar.CONFIDENTIAL | CVar.SERVERONLY);

    public static readonly CVarDef<string> DiscordGuildID =
        CVarDef.Create("jerry.discord_guildId", "1318776836599320657", CVar.CONFIDENTIAL | CVar.SERVERONLY);

    public static readonly CVarDef<string> ApiKey =
        CVarDef.Create("jerry.discord_apikey", "", CVar.CONFIDENTIAL | CVar.SERVERONLY);



    /// <summary>
    ///     Nudge entities that stay in hard contact with static geometry for a long time (anti-stuck).
    /// </summary>
    public static readonly CVarDef<bool> AutoUnstuckEnabled =
        CVarDef.Create("forge.physics.auto_unstuck", true, CVar.SERVERONLY);

    /// <summary>
    ///     Seconds of continuous static hard contact before a nudge is applied.
    /// </summary>
    public static readonly CVarDef<float> AutoUnstuckAfterSeconds =
        CVarDef.Create("forge.physics.auto_unstuck_after", 10f, CVar.SERVERONLY);

    /// <summary>
    ///     World-space displacement applied when unsticking (meters).
    /// </summary>
    public static readonly CVarDef<float> AutoUnstuckNudge =
        CVarDef.Create("forge.physics.auto_unstuck_nudge", 2f, CVar.SERVERONLY);

}
