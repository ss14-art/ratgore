using System.Numerics;
using Content.Client.Pinpointer.UI;
using Content.Client.Shuttles.Systems;
using Content.Client.Shuttles.UI;
using Content.Client._Rat.SpaceEvents;
using Content.Shared.Shuttles.Components;
using Robust.Client.Graphics;
using Robust.Client.Input;
using Robust.Client.UserInterface;
using Robust.Client.UserInterface.Controls;
using Robust.Shared.Map;
using Robust.Shared.Map.Components;
using Robust.Shared.Timing;

namespace Content.Client._Rat.CrewMonitoring;

public sealed class RatCrewMonitorMapControl : BaseShuttleControl
{
    [Dependency] private readonly IInputManager _inputs = default!;
    private readonly SharedTransformSystem _xformSystem = default!;
    private readonly ShuttleSystem _shuttles = default!;
    private EmpZoneClientSystem _empZone = default!;

    public NetEntity? Focus;
    public Dictionary<NetEntity, string> LocalizedNames = new();
    public Dictionary<NetEntity, NavMapBlip> TrackedEntities = new();
    public Dictionary<NetEntity, byte> MobStates = new();
    public Dictionary<EntityCoordinates, (bool Visible, Color Color)> TrackedCoordinates = new();

    public EntityUid? MapUid;

    public event Action<NetEntity?>? TrackedEntitySelectedAction;

    private readonly List<(NetEntity entity, string name, Color color, EntityCoordinates coords)> _entityBlips = new();

    private Label _trackedEntityLabel;
    private PanelContainer _trackedEntityPanel;

    protected override bool Draggable => true;

    private static Color GetMobColor(byte mobState) => mobState switch
    {
        4 => Color.Red,       // Dead
        2 or 3 => Color.Orange, // Critical / SoftCritical
        _ => Color.LimeGreen,   // Alive, Invalid
    };

    public RatCrewMonitorMapControl() : base(256f, 8192f, 4096f)
    {
        IoCManager.InjectDependencies(this);
        _xformSystem = EntManager.System<SharedTransformSystem>();
        _shuttles = EntManager.System<ShuttleSystem>();
        _empZone = EntManager.System<EmpZoneClientSystem>();

        _trackedEntityLabel = new Label
        {
            Margin = new Thickness(10f, 8f),
            HorizontalAlignment = HAlignment.Center,
            VerticalAlignment = VAlignment.Center,
            Modulate = Color.White,
        };

        _trackedEntityPanel = new PanelContainer
        {
            PanelOverride = new StyleBoxFlat
            {
                BackgroundColor = Color.FromSrgb(new Color(30, 67, 30).WithAlpha(0.9f)),
            },
            Margin = new Thickness(5f, 10f),
            HorizontalAlignment = HAlignment.Left,
            VerticalAlignment = VAlignment.Bottom,
            Visible = false,
        };

        _trackedEntityPanel.AddChild(_trackedEntityLabel);
        AddChild(_trackedEntityPanel);
    }

    public void ForceNavMapUpdate()
    {
    }

    public void CenterToCoordinates(EntityCoordinates coordinates)
    {
        var mapCoords = coordinates.ToMap(EntManager, _xformSystem);
        TargetOffset = mapCoords.Position;
        Recentering = true;
    }

    protected override void Draw(DrawingHandleScreen handle)
    {
        base.Draw(handle);
        DrawRecenter();
        DrawBacking(handle);
        DrawCoordinateGrid(handle);
        DrawRatZones(handle);

        var mapId = MapUid != null && EntManager.TryGetComponent(MapUid.Value, out TransformComponent? mapXform)
            ? mapXform.MapID
            : MapId.Nullspace;

        if (mapId == MapId.Nullspace)
        {
            DrawNoSignal(handle);
            return;
        }

        var matty = Matrix3Helpers.CreateInverseTransform(Offset, Angle.Zero);

        DrawAllGrids(handle, matty);
        DrawBlips(handle, matty);
        DrawTrackedCoords(handle, matty);
        DrawMouseCoords(handle);
        DrawAzimuthScale(handle, Angle.Zero);
    }

    private void DrawCoordinateGrid(DrawingHandleScreen handle)
    {
        const float step = 500f;
        var gridColor = new Color(0.2f, 0.2f, 0.2f, 0.8f);
        var axisColor = new Color(0.4f, 0.4f, 0.4f, 1f);

        var screenH = PixelHeight;
        var screenW = PixelWidth;

        var startX = MathF.Floor((Offset.X - WorldRange) / step) * step;
        var endX = MathF.Ceiling((Offset.X + WorldRange) / step) * step;
        var startY = MathF.Floor((Offset.Y - WorldRange) / step) * step;
        var endY = MathF.Ceiling((Offset.Y + WorldRange) / step) * step;

        for (var wx = startX; wx <= endX; wx += step)
        {
            var sx = ScalePosition(new Vector2(wx - Offset.X, 0f)).X;
            var color = MathF.Abs(wx) < 0.1f ? axisColor : gridColor;
            handle.DrawLine(new Vector2(sx, 0), new Vector2(sx, screenH), color);
        }

        for (var wy = startY; wy <= endY; wy += step)
        {
            var sy = ScalePosition(new Vector2(0f, -(wy - Offset.Y))).Y;
            var color = MathF.Abs(wy) < 0.1f ? axisColor : gridColor;
            handle.DrawLine(new Vector2(0, sy), new Vector2(screenW, sy), color);
        }
    }

    private static void DrawFilledRing(DrawingHandleScreen handle, Vector2 center,
        float innerRadius, float outerRadius, Color fillColor, Color outlineColor, int segments = 64)
    {
        var verts = new Vector2[segments * 6];
        for (int i = 0; i < segments; i++)
        {
            float a0 = MathF.Tau * i / segments;
            float a1 = MathF.Tau * (i + 1) / segments;
            var d0 = new Vector2(MathF.Cos(a0), MathF.Sin(a0));
            var d1 = new Vector2(MathF.Cos(a1), MathF.Sin(a1));
            verts[i * 6 + 0] = center + d0 * innerRadius;
            verts[i * 6 + 1] = center + d0 * outerRadius;
            verts[i * 6 + 2] = center + d1 * outerRadius;
            verts[i * 6 + 3] = center + d0 * innerRadius;
            verts[i * 6 + 4] = center + d1 * outerRadius;
            verts[i * 6 + 5] = center + d1 * innerRadius;
        }
        handle.DrawPrimitives(DrawPrimitiveTopology.TriangleList, verts, fillColor);

        var outerVerts = new Vector2[segments];
        var innerVerts = new Vector2[segments];
        for (int i = 0; i < segments; i++)
        {
            float a = MathF.Tau * i / segments;
            var dir = new Vector2(MathF.Cos(a), MathF.Sin(a));
            outerVerts[i] = center + dir * outerRadius;
            innerVerts[i] = center + dir * innerRadius;
        }
        handle.DrawPrimitives(DrawPrimitiveTopology.LineLoop, outerVerts, outlineColor);
        handle.DrawPrimitives(DrawPrimitiveTopology.LineLoop, innerVerts, outlineColor);
    }

    private void DrawRatZones(DrawingHandleScreen handle)
    {
        var matty = Matrix3Helpers.CreateInverseTransform(Offset, Angle.Zero);
        var worldOrigin = Vector2.Transform(Vector2.Zero, matty);
        worldOrigin = worldOrigin with { Y = -worldOrigin.Y };
        var screenOrigin = ScalePosition(worldOrigin);

        handle.DrawCircle(screenOrigin, 500f * MinimapScale, new Color(1f, 0f, 0f, 0.03f));
        handle.DrawCircle(screenOrigin, 500f * MinimapScale, new Color(1f, 0f, 0f, 0.2f), filled: false);

        DrawFilledRing(handle, screenOrigin,
            4000f * MinimapScale, 4500f * MinimapScale,
            new Color(0f, 1f, 0f, 0.03f), new Color(0f, 1f, 0f, 0.2f));

        DrawFilledRing(handle, screenOrigin,
            10000f * MinimapScale, 20000f * MinimapScale,
            new Color(1f, 0f, 0f, 0.01f), new Color(1f, 0f, 0f, 0.1f));

        foreach (var (_, (center, radius)) in _empZone.ActiveZones)
        {
            var empRelPos = Vector2.Transform(center, matty);
            empRelPos = empRelPos with { Y = -empRelPos.Y };
            var empScreenPos = ScalePosition(empRelPos);
            var empScreenRadius = radius * MinimapScale;

            handle.DrawCircle(empScreenPos, empScreenRadius, new Color(0f, 0.8f, 1f, 0.03f));
            handle.DrawCircle(empScreenPos, empScreenRadius, new Color(0f, 0.8f, 1f, 0.2f), filled: false);
        }
    }

    private void DrawAllGrids(DrawingHandleScreen handle, Matrix3x2 matty)
    {
        var gridQuery = EntManager.EntityQueryEnumerator<MapGridComponent, TransformComponent>();

        while (gridQuery.MoveNext(out var gridUid, out var gridComp, out var gridXform))
        {
            if (gridXform.MapID == MapId.Nullspace)
                continue;

            IFFComponent? iffComp = null;

            if (EntManager.TryGetComponent(gridUid, out iffComp) && (iffComp.Flags & IFFFlags.Hide) != 0)
                continue;

            var gridColor = _shuttles.GetIFFColor(gridUid, self: false, component: iffComp);

            var worldMatrix = _xformSystem.GetWorldMatrix(gridUid);
            var matrix = Matrix3x2.Multiply(worldMatrix, matty);
            DrawGrid(handle, matrix, (gridUid, gridComp), gridColor, 0.15f);

            var label = _shuttles.GetIFFLabel(gridUid, self: false, component: iffComp);

            if (string.IsNullOrEmpty(label))
                continue;

            var gridPos = _xformSystem.GetWorldPosition(gridUid);
            var relativePos = Vector2.Transform(gridPos, matty);
            relativePos = relativePos with { Y = -relativePos.Y };
            var screenPos = ScalePosition(relativePos);

            var textSize = handle.GetDimensions(Font, label, 1f);
            handle.DrawString(Font, screenPos + new Vector2(-textSize.X / 2f, -8f), label, gridColor);
        }
    }

    private void DrawBlips(DrawingHandleScreen handle, Matrix3x2 matty)
    {
        var realTime = Timing.RealTime;
        var blinkFreq = 1f / 1f;
        var lit = realTime.TotalSeconds % blinkFreq > blinkFreq / 2f;

        _entityBlips.Clear();

        foreach (var (netEnt, blip) in TrackedEntities)
        {
            if (blip.Blinks && !lit)
                continue;

            var coords = blip.Coordinates;

            if (!coords.IsValid(EntManager))
                continue;

            var mapPos = coords.ToMap(EntManager, _xformSystem);

            if (mapPos.MapId == MapId.Nullspace)
                continue;

            var relativePos = Vector2.Transform(mapPos.Position, matty);
            relativePos = relativePos with { Y = -relativePos.Y };
            var screenPos = ScalePosition(relativePos);

            if (blip.Texture != null)
            {
                var scale = 0.075f * float.Sqrt(MinimapScale) * blip.Scale;
                var offset = new Vector2(scale * blip.Texture.Width, scale * blip.Texture.Height);
                handle.DrawTextureRect(blip.Texture, new UIBox2(screenPos - offset, screenPos + offset), blip.Color);
            }
            else
            {
                handle.DrawCircle(screenPos, float.Sqrt(MinimapScale) * 2f, blip.Color);
            }

            if (LocalizedNames.TryGetValue(netEnt, out var name) && netEnt == Focus)
            {
                _entityBlips.Add((netEnt, name, blip.Color, coords));
            }
        }

        foreach (var (entity, name, color, coords) in _entityBlips)
        {
            var mapPos = coords.ToMap(EntManager, _xformSystem);
            var relativePos = Vector2.Transform(mapPos.Position, matty);
            relativePos = relativePos with { Y = -relativePos.Y };
            var screenPos = ScalePosition(relativePos);

            var textDimensions = handle.GetDimensions(Font, name, 1f);
            handle.DrawString(Font, screenPos + new Vector2(-textDimensions.X / 2f, 8f), name, Color.FromSrgb(color));
        }
    }

    private void DrawTrackedCoords(DrawingHandleScreen handle, Matrix3x2 matty)
    {
        var realTime = Timing.RealTime;
        var blinkFreq = 1f / 1f;
        var lit = realTime.TotalSeconds % blinkFreq > blinkFreq / 2f;

        foreach (var (coord, value) in TrackedCoordinates)
        {
            if (!lit && value.Visible)
                continue;

            var mapPos = coord.ToMap(EntManager, _xformSystem);

            if (mapPos.MapId == MapId.Nullspace)
                continue;

            var relativePos = Vector2.Transform(mapPos.Position, matty);
            relativePos = relativePos with { Y = -relativePos.Y };
            var screenPos = ScalePosition(relativePos);

            handle.DrawCircle(screenPos, float.Sqrt(MinimapScale) * 2f, value.Color);
        }
    }

    private void DrawMouseCoords(DrawingHandleScreen handle)
    {
        var mousePos = _inputs.MouseScreenPosition;
        var mouseLocalPos = GetLocalPosition(mousePos);

        if (mousePos.Window != WindowId.Invalid && PixelRect.Contains(mouseLocalPos.Floored()))
        {
            var mapOffset = InverseMapPosition(mouseLocalPos);
            var coordsText = $"{mapOffset.X:0.0}, {mapOffset.Y:0.0}";
            DrawData(handle, coordsText);
        }
    }

    private Vector2 GetLocalPosition(ScreenCoordinates screenPos)
    {
        return screenPos.Position - GlobalPixelPosition;
    }

    protected override void FrameUpdate(FrameEventArgs args)
    {
        base.FrameUpdate(args);

        if (Focus == null)
        {
            _trackedEntityLabel.Text = string.Empty;
            _trackedEntityPanel.Visible = false;
            return;
        }

        foreach (var (netEntity, blip) in TrackedEntities)
        {
            if (netEntity != Focus)
                continue;

            if (!LocalizedNames.TryGetValue(netEntity, out var name))
                name = "Unknown";

            var worldPos = blip.Coordinates.ToMap(EntManager, _xformSystem);
            var message = name + "\nLocation: [x = " + MathF.Round(worldPos.Position.X) + ", y = " + MathF.Round(worldPos.Position.Y) + "]";

            _trackedEntityLabel.Text = message;
            _trackedEntityPanel.Visible = true;
            
            var mobColor = MobStates.TryGetValue(netEntity, out var mobState)
                ? GetMobColor(mobState)
                : Color.White;
            _trackedEntityLabel.Modulate = mobColor;

            return;
        }

        _trackedEntityLabel.Text = string.Empty;
        _trackedEntityPanel.Visible = false;
    }
}
