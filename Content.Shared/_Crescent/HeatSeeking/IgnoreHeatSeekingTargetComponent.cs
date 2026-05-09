namespace Content.Shared._Crescent.HeatSeeking;

/// <summary>
/// Marks entities (usually debris grids) that should never be selected by heat-seeking missiles.
/// </summary>
[RegisterComponent]
public sealed partial class IgnoreHeatSeekingTargetComponent : Component
{
}
