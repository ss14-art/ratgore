namespace Content.Server.Singularity.Components
{
    /// <summary>
    /// Overrides exactly how much energy this object gives to a singularity.
    /// </summary>
    [RegisterComponent]
    public sealed partial class SinguloFoodComponent : Component
    {
        [ViewVariables(VVAccess.ReadWrite)]
        [DataField("energy")]
        public float Energy { get; set; } = 1f;

        [ViewVariables(VVAccess.ReadWrite)]
        [DataField("energyFactor")]
        public float EnergyFactor { get; set; } = 1f;
    }
}
