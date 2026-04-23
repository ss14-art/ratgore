using Content.Shared.Store;
using Robust.Shared.Prototypes;

namespace Content.Server.GameTicking.Rules.Components;

[RegisterComponent]
public sealed partial class ChangelingRuleComponent : Component
{
    public readonly List<EntityUid> ChangelingMinds = new();

    [DataField]
    public List<ProtoId<StoreCategoryPrototype>> StoreCategories = new()
    {
        "ChangelingAbilityCombat",
        "ChangelingAbilitySting",
        "ChangelingAbilityUtility"
    };
}
