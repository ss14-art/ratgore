using Content.Shared._Rat.CrewMonitoring;
using Robust.Client.UserInterface;

namespace Content.Client._Rat.CrewMonitoring;

public sealed class RatCrewMonitorBoundUserInterface : BoundUserInterface
{
    private RatCrewMonitorWindow? _window;

    public RatCrewMonitorBoundUserInterface(EntityUid owner, Enum uiKey) : base(owner, uiKey)
    {
    }

    protected override void Open()
    {
        base.Open();

        EntityUid? mapUid = null;
        var stationName = string.Empty;

        if (EntMan.TryGetComponent<TransformComponent>(Owner, out var xform))
        {
            if (xform.GridUid != null)
            {
                mapUid = xform.GridUid.Value;
                if (EntMan.TryGetComponent<MetaDataComponent>(mapUid, out var metaData))
                    stationName = metaData.EntityName;
            }
            else if (xform.MapUid != null)
            {
                mapUid = xform.MapUid.Value;
            }
        }

        _window = this.CreateWindow<RatCrewMonitorWindow>();
        _window.Set(stationName, mapUid);
    }

    protected override void UpdateState(BoundUserInterfaceState state)
    {
        base.UpdateState(state);

        switch (state)
        {
            case RatCrewMonitorState st:
                EntMan.TryGetComponent<TransformComponent>(Owner, out var xform);
                _window?.ShowSensors(st.Sensors, Owner, xform?.Coordinates);
                break;
        }
    }

    protected override void Dispose(bool disposing)
    {
        _window?.Dispose();
        _window = null;
        base.Dispose(disposing);
    }
}
