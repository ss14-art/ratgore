using Content.Shared.FixedPoint;
using Robust.Shared.Map;

namespace Content.Server.Chemistry.Components
{
    [RegisterComponent]
    public sealed partial class VaporComponent : Component
    {
        public const string SolutionName = "vapor";

        [DataField("transferAmount")]
        public FixedPoint2 TransferAmount = FixedPoint2.New(0.5);

        public float ReactTimer;
        [DataField("active")]
        public bool Active;

        public TileRef? PreviousTileRef;

        [DataField]
        public float TransferAmountPercentage = 0.5f;

        [DataField]
        public float MinimumTransferAmount = 0.1f;
    }
}
