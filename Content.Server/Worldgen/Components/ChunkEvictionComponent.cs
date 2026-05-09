using Robust.Shared.GameObjects;

namespace Content.Server.Worldgen.Components;

/// <summary>
///     Marks a world chunk for delayed eviction (deletion) after it has been unloaded.
/// </summary>
[RegisterComponent]
public sealed partial class ChunkEvictionComponent : Component
{
    /// <summary>
    ///     The real time at which this chunk should be evicted.
    /// </summary>
    public TimeSpan EvictAt;
}