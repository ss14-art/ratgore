using Content.Shared.Customization.Systems;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.Manager;

using Content.Shared.Roles;

namespace Content.Shared.Clothing.Loadouts.Prototypes;


[Prototype]
public sealed partial class LoadoutPrototype : IPrototype, IEquipmentLoadout
{
    public string GetGear(string slot) => string.Empty;
    public List<EntProtoId> Inhand => new();
    public Dictionary<string, List<EntProtoId>> Storage => new();

    /// Formatted like "Loadout[Department/ShortHeadName][CommonClothingSlot][SimplifiedClothingId]", example: "LoadoutScienceOuterLabcoatSeniorResearcher"
    [IdDataField]
    public string ID { get; set; } = default!;

    [DataField]
    public ProtoId<LoadoutCategoryPrototype> Category = "Uncategorized";

    [DataField(required: true)]
    public List<ProtoId<EntityPrototype>> Items = new();

    /// Components to give each item on spawn
    [DataField]
    public ComponentRegistry Components = new();

    [DataField]
    public int Cost = 1;

    /// <summary>
    ///     How many item group selections this uses. Defaulted to 1:1, but can be any number.
    /// </summary>
    [DataField]
    public int Slots = 1;

    /// Should this item override other items in the same slot
    [DataField]
    public bool Exclusive;

    [DataField]
    public bool CustomName = true;

    [DataField]
    public bool CustomDescription = true;

    [DataField]
    public bool CustomColorTint = false;

    [DataField]
    public bool CanBeHeirloom = false;

    [DataField]
    public List<CharacterRequirement> Requirements = new();

    [DataField]
    public string GuideEntry { get; set; } = "";

    [DataField(serverOnly: true)]
    public LoadoutFunction[] Functions { get; private set; } = Array.Empty<LoadoutFunction>();
}

/// This serves as a hook for loadout functions to modify one or more entities upon spawning in.
[ImplicitDataDefinitionForInheritors]
public abstract partial class LoadoutFunction
{
    public abstract void OnPlayerSpawn(
        EntityUid character,
        EntityUid loadoutEntity,
        IComponentFactory factory,
        IEntityManager entityManager,
        ISerializationManager serializationManager);
}
