using Content.Shared.Roles.RoleCodeword;

namespace Content.Server.Traitor.Components;

[RegisterComponent]
public sealed partial class TraitorCodePaperComponent : Component
{
    [DataField]
    public string CodewordFaction = "Traitor";

    [DataField]
    public bool FakeCodewords = false;

    [DataField]
    public string CodewordGenerator = "Default";

    [DataField]
    public int CodewordAmount = 3;

    [DataField]
    public bool CodewordShowAll = false;
}
