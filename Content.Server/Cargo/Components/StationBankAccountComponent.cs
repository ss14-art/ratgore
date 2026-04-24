using Content.Shared.Cargo;
using Content.Shared.Cargo.Prototypes;
using Robust.Shared.Prototypes;


namespace Content.Server.Cargo.Components;

/// <summary>
/// Added to the abstract representation of a station to track its money.
/// </summary>
[RegisterComponent, Access(typeof(SharedCargoSystem))]
public sealed partial class StationBankAccountComponent : Component
{
    [ViewVariables(VVAccess.ReadWrite), DataField("balance")]
    public int Balance = 2000;

    /// <summary>
    /// How much the bank balance goes up per second, every Delay period. Rounded down when multiplied.
    /// </summary>
    [ViewVariables(VVAccess.ReadWrite), DataField("increasePerSecond")]
    public int IncreasePerSecond = 1;

    [ViewVariables(VVAccess.ReadWrite), DataField("incomeDelay")]
    public TimeSpan IncomeDelay = TimeSpan.FromMinutes(1);

    [ViewVariables(VVAccess.ReadWrite)]
    public TimeSpan NextIncomeTime;

    [ViewVariables(VVAccess.ReadWrite)]
    public float RevenueDistribution = 0.5f;

    [DataField]
    public ProtoId<CargoAccountPrototype> PrimaryAccount = "market";
}
