using Godot;
using VoidParley.core.player;

namespace VoidParley.network.player;

public partial class PlayerController : Node
{
    public Player Player {get; private set;}

    public override void _Ready()
    {
        Player = GetParent<Player>();
    }
}