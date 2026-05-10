using Robust.Shared.Prototypes;

namespace Content.Shared.Body.Prototypes;

[Prototype("body")]
public sealed partial class BodyPrototype : IPrototype
{
    [IdDataField] public string ID { get; private set; } = default!;

    [DataField("name")]
    public string Name { get; private set; } = "";

    [DataField("root")] public string Root { get; private set; } = string.Empty;

    [DataField("slots")] public Dictionary<string, BodyPrototypeSlot> Slots { get; private set; } = new();

    private BodyPrototype() { }

    public BodyPrototype(string id, string name, string root, Dictionary<string, BodyPrototypeSlot> slots)
    {
        ID = id;
        Name = name;
        Root = root;
        Slots = slots;
    }
}

[DataRecord, DataDefinition]
public sealed partial record BodyPrototypeSlot
{
    [DataField("part")]
    public EntProtoId? Part { get; private set; }

    [DataField("connections")]
    public HashSet<string> Connections { get; private set; } = new();

    [DataField("organs")]
    public Dictionary<string, string> Organs { get; private set; } = new();

    public BodyPrototypeSlot() { }

    public BodyPrototypeSlot(EntProtoId? part, HashSet<string> connections, Dictionary<string, string> organs)
    {
        Part = part;
        Connections = connections;
        Organs = organs;
    }
}
