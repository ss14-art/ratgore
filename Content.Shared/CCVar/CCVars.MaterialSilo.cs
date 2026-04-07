using Robust.Shared.Configuration;

namespace Content.Shared.CCVar;

public static partial class CCVars
{
    /// <summary>
    ///     Is ore material enabled.
    /// </summary>
    public static readonly CVarDef<bool> SiloEnabled =
        CVarDef.Create("silo.silo_enabled", true, CVar.SERVER | CVar.REPLICATED);
}
