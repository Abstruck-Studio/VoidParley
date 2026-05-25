using System.Collections.Generic;
using Godot;
using VoidParley.core.model;

namespace VoidParley.core.player;

public partial class Player(PlayerData playerData) : Node
{
    public PlayerData PlayerData { get; set; } = playerData;
    
    public static string LocalId;
    
    private List<CardModel> _cards = new List<CardModel>();
    public network.player.PlayerController Controller = new network.player.PlayerController();

    public override void _EnterTree()
    {
        AddChild(Controller);
    }

    internal void AddCard(CardModel card)
    {
        _cards.Add(card);
    }

    internal void RemoveCard(CardModel card)
    {
        _cards.Remove(card);
    }

    internal void ClearCards()
    {
        _cards.Clear();
    }
}