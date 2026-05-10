using Robust.Shared.Serialization;

namespace Content.Shared.Construction;

[Serializable, NetSerializable]
public enum MachineUpgradeScalingType : byte
{
    Linear,
    Exponential
}
