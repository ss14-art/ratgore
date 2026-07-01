using Content.Shared._Rat.Factions;
using Robust.Shared.GameObjects;
using System;
using System.Collections.Generic;

namespace Content.Client._Rat.Factions;

public sealed class RatFactionSystem : EntitySystem
{
    private List<RatFactionInfo> _factionList = new();
    private string _selectedFaction = "";

    public event Action<List<RatFactionInfo>>? OnFactionsChanged;
    public event Action<string>? OnFactionSelected;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeNetworkEvent<FactionListMessage>(OnFactionListReceived);
    }

    public void RequestFactionList()
    {
        RaiseNetworkEvent(new RequestFactionListMessage());
    }

    private void OnFactionListReceived(FactionListMessage message)
    {
        SetFactions(message.Factions);
    }

    public void SetFactions(List<RatFactionInfo> factions)
    {
        _factionList = factions;
        OnFactionsChanged?.Invoke(factions);
    }

    public List<RatFactionInfo> GetAvailableFactions()
    {
        return _factionList;
    }

    public string GetSelectedFaction()
    {
        return _selectedFaction;
    }

    public void SelectFaction(string factionName)
    {
        _selectedFaction = factionName;
        OnFactionSelected?.Invoke(factionName);
    }
}
