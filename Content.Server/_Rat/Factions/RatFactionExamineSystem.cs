using Content.Shared._Rat.Factions;
using Content.Shared.Examine;
using Content.Shared.Humanoid;
using Content.Shared.Preferences;
using Robust.Shared.GameObjects;

namespace Content.Server._Rat.Factions;

public sealed class RatFactionExamineSystem : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<RatFactionComponent, ExaminedEvent>(OnExamined);
    }

    private void OnExamined(EntityUid uid, RatFactionComponent component, ExaminedEvent args)
    {
        if (string.IsNullOrEmpty(component.SubfactionName))
            return;

        args.PushMarkup(Loc.GetString("rat-faction-examine", ("faction", component.SubfactionName)));
    }
}
