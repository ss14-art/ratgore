using Robust.Shared.Configuration;

namespace Content.Shared._Scp.ScpCCVars;

[CVarDefs]
public sealed partial class ScpCCVars
{
    /**
     * Shader
     */

    /// <summary>
    /// Выключен ли шейдер зернистости?
    /// </summary>
    public static readonly CVarDef<bool> GrainToggleOverlay =
        CVarDef.Create("shader.grain_toggle_overlay", true, CVar.CLIENTONLY | CVar.ARCHIVE);

    /// <summary>
    /// Сила шейдера зернистости
    /// </summary>
    public static readonly CVarDef<int> GrainStrength =
        CVarDef.Create("shader.grain_strength", 140, CVar.CLIENTONLY | CVar.ARCHIVE);

    /// <summary>
    /// Будет ли использовать альтернативный метод просчета сущностей для поля зрения
    /// </summary>
    /*
     * Свечение лампочек
     */

    /// <summary>
    /// Будет ли использоваться эффект свечения у лампочек?
    /// Отвечает за главный рубильник для двух опций настройки.
    /// </summary>
    public static readonly CVarDef<bool> LightBloomEnable =
        CVarDef.Create("scp.light_bloom_enable", true, CVar.CLIENTONLY | CVar.ARCHIVE);

    /// <summary>
    /// Будет ли отображаться конус у эффекта свечения лампочек?
    /// </summary>
    public static readonly CVarDef<bool> LightBloomConeEnable =
        CVarDef.Create("scp.light_bloom_cone_enable", true, CVar.CLIENTONLY | CVar.ARCHIVE);

    /// <summary>
    /// При включении не рисуется на невидимых для игрока позициях эффект, что увеличивает производительность ТОЛЬКО на слабых видеокартах.
    /// В остальных случаях снижает FPS из-за проверок на видимость. Поэтому это опционально.
    /// </summary>
    public static readonly CVarDef<bool> LightBloomOptimizations =
        CVarDef.Create("scp.light_bloom_optimizations", false, CVar.CLIENTONLY | CVar.ARCHIVE);

    /// <summary>
    /// Определяет силу эффекта свечения.
    /// Чем выше сила, тем сильнее эффект. Отображается в процентах от 0% до 100%
    /// </summary>
    public static readonly CVarDef<float> LightBloomStrength =
        CVarDef.Create("scp.light_bloom_strength", 0.5f, CVar.CLIENTONLY | CVar.ARCHIVE);
}
