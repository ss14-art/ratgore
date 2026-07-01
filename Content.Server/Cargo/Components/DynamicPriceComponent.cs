namespace Content.Server.Cargo.Components;

[RegisterComponent]
public sealed partial class DynamicPriceComponent : Component
{
    [DataField]
    public double BasePrice;

    [DataField]
    public float PriceMultiplier = 1.0f;

    [DataField]
    public float MinMultiplier = 0.5f;

    [DataField]
    public float MaxMultiplier = 2.0f;

    [DataField]
    public float RecoveryRate = 0.01f;

    [DataField]
    public float PriceDeviation;

    [DataField]
    public float LastUpdateTimer;

    [DataField]
    public float UpdateInterval = 300f;
}
