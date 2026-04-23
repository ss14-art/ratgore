using Content.Shared.Roles.RoleCodeword;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;
using Content.Shared.Dataset;
using Content.Shared.NPC.Prototypes;

namespace Content.Server.Roles.RoleCodeword;

public sealed class RoleCodewordSystem : SharedRoleCodewordSystem
{
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly IPrototypeManager _prototype = default!;

    public string[] GetCodewords(ProtoId<NpcFactionPrototype> faction)
    {
        var query = EntityQueryEnumerator<RoleCodewordComponent>();
        while (query.MoveNext(out _, out var component))
        {
            if (component.RoleCodewords.TryGetValue(faction, out var data))
            {
                return data.Codewords.ToArray();
            }
        }

        return GenerateCodewords(faction);
    }

    public string[] GenerateCodewords(ProtoId<NpcFactionPrototype> faction)
    {
        var adjectives = _prototype.Index<DatasetPrototype>("adjectives").Values;
        var verbs = _prototype.Index<DatasetPrototype>("verbs").Values;

        var codewords = new List<string>
        {
            _random.Pick(adjectives) + " " + _random.Pick(verbs),
            _random.Pick(adjectives) + " " + _random.Pick(verbs),
            _random.Pick(adjectives) + " " + _random.Pick(verbs)
        };

        var query = EntityQueryEnumerator<RoleCodewordComponent>();
        if (query.MoveNext(out var uid, out var component))
        {
            component.RoleCodewords[faction] = new CodewordsData(Color.Red, codewords);
            Dirty(uid, component);
        }
        else
        {
            var newComp = new RoleCodewordComponent();
            newComp.RoleCodewords[faction] = new CodewordsData(Color.Red, codewords);
            EntityManager.AddComponent(EntityManager.CreateEntityUninitialized(null), newComp);
        }

        return codewords.ToArray();
    }
}
