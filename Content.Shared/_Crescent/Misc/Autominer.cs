using Robust.Shared.Serialization;

namespace Content.Shared._Crescent.Misc;

[Serializable, NetSerializable]
public enum AutominerUiKey : byte
{  
    Key
}

[Serializable, NetSerializable]
public sealed class AutominerBoundUserInterfaceState : BoundUserInterfaceState
{
    public TimeSpan CooldownEndTime;
    public bool IsActive;

    public AutominerBoundUserInterfaceState(TimeSpan cooldownEndTime, bool isActive)
    {
        CooldownEndTime = cooldownEndTime;
        IsActive = isActive;
    }
}

[Serializable, NetSerializable]
public sealed class AutominerStartMessage : BoundUserInterfaceMessage { }