using Content.Shared.Clothing.Components;
using Content.Shared.Clothing;
using Content.Shared.Clothing.Components;
using Content.Shared.Clothing.EntitySystems;
using Content.Shared.Foldable;
using Content.Shared.Item;
using Robust.Client.GameObjects;

namespace Content.Client.Clothing;

public sealed class FlippableClothingVisualizerSystem : VisualizerSystem<FoldableClothingComponent>
{
    [Dependency] private readonly SharedItemSystem _itemSys = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<FoldableClothingComponent, GetEquipmentVisualsEvent>(OnGetVisuals, after: new[] { typeof(ClothingSystem) });
        SubscribeLocalEvent<FoldableClothingComponent, FoldedEvent>(OnFolded);
    }

    private void OnFolded(Entity<FoldableClothingComponent> ent, ref FoldedEvent args)
    {
        _itemSys.VisualsChanged(ent);
    }

    private void OnGetVisuals(Entity<FoldableClothingComponent> ent, ref GetEquipmentVisualsEvent args)
    {
        if (!TryComp(ent, out SpriteComponent? sprite) ||
            !TryComp(ent, out ClothingComponent? clothing))
            return;

        if (clothing.MappedLayer == null ||
            !AppearanceSystem.TryGetData<bool>(ent, FoldableSystem.FoldedVisuals.State, out var folding) ||
            !sprite.LayerMapTryGet(folding ? ent.Comp.FoldingLayer : ent.Comp.UnfoldingLayer, out var idx))
            return;

        // add each layer to the visuals
        var spriteLayer = sprite[idx];
        foreach (var layer in args.Layers)
        {
            if (layer.Item1 != clothing.MappedLayer)
                continue;

            layer.Item2.Scale = spriteLayer.Scale;
        }
    }
}
