using Content.Shared.Medical.SuitSensor;
using Robust.Shared.Serialization;

namespace Content.Shared._Rat.CrewMonitoring;

[Serializable, NetSerializable]
public sealed class RatCrewMonitorState : BoundUserInterfaceState
{
    public List<SuitSensorStatus> Sensors;

    public RatCrewMonitorState(List<SuitSensorStatus> sensors)
    {
        Sensors = sensors;
    }
}
