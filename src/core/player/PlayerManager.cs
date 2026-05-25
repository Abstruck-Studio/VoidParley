using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using Godot;

namespace VoidParley.core.player;

public partial class PlayerManager : Node
{
    public static PlayerManager Instance { get; private set; }
    
    private Dictionary<long, PlayerData> _players;

    public override void _EnterTree()
    {
        if (null != Instance)
        {
            QueueFree();
            return;
        }
        Instance = this;
        
        _players = new Dictionary<long, PlayerData>();
    }
    
    public List<PlayerData> GetPlayers()
    {
        return _players.Values.ToList();
    }

    // === === Server === ===
    public void JoinPlayer(PlayerData player)
    {
        _players[player.UniqueId] = player;
        Rpc(MethodName.SendPlayerListToClient, JsonSerializer.Serialize(_players.Values));
    }

    public void LeavePlayer(long uniqueId)
    {
        _players.Remove(uniqueId);
        Rpc(MethodName.SendPlayerListToClient, JsonSerializer.Serialize(_players.Values));
    }

    // === === Client === ===
    public void ClearPlayers()
    {
        _players.Clear();
    }
    
    // <== Rpc ==>
    // === === Client === === 
    [Rpc(MultiplayerApi.RpcMode.Authority, CallLocal = false, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    public void SendPlayerListToClient(string playerList)
    {
        List<PlayerData>? players = JsonSerializer.Deserialize<List<PlayerData>>(playerList);

        foreach (PlayerData p in players)
        {
            _players[p.UniqueId] = p;
        }
    }
}