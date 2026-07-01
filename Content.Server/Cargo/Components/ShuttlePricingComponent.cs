using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;
using Robust.Shared.Timing;

namespace Content.Server.Cargo.Components;

[RegisterComponent]
public sealed partial class ShuttlePricingComponent : Component
{
    [DataField]
    public int BasePrice = 30000;

    [DataField]
    public int CurrentPrice = 30000;

    [DataField]
    public int MinPrice = 15000;

    [DataField]
    public int MaxPrice = 60000;

    [DataField]
    public TimeSpan UpdateInterval = TimeSpan.FromSeconds(600);

    [DataField(customTypeSerializer: typeof(TimeOffsetSerializer))]
    public TimeSpan NextUpdate = TimeSpan.Zero;

    [DataField]
    public List<int> PriceHistory = new();
}
