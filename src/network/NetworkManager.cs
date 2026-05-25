using System.Text.Json;
using Godot;
using VoidParley.core.player;

namespace VoidParley.network;

public partial class NetworkManager : Node
{
    public static NetworkManager Instance { get; private set; }
    public static bool IsConnectedToServer { get; private set; } = false;
    public long LocalPlayerId => Multiplayer.GetUniqueId();

    private ENetMultiplayerPeer _peer;

    public override void _EnterTree()
    {
        if (null != Instance)
        {
            QueueFree();
            return;
        }
        Instance = this;
    }

    public override void _Ready()
    {
        _peer = new ENetMultiplayerPeer();
        // Client
        Multiplayer.ConnectedToServer += OnConnectedToServer;
        Multiplayer.PeerDisconnected += OnPeerDisconnected_Client;
        
        // Server
        Multiplayer.PeerDisconnected += OnPeerDisconnected_Server;
    }

    public override void _ExitTree()
    {
        // Client
        Multiplayer.ConnectedToServer -= OnConnectedToServer;
        Multiplayer.PeerDisconnected -= OnPeerDisconnected_Client;
        
        // Server
        Multiplayer.PeerDisconnected -= OnPeerDisconnected_Server;
    }
    
    public bool CreateServer(int port, int maxPlayers)
    {
        if (IsConnectedToServer)
        {
            Disconnect();
        }
        
        _peer = new ENetMultiplayerPeer();
        Error error = _peer.CreateServer(port, maxPlayers);
        if (Error.Ok != error)
        {
            GD.PrintErr($"Failed to create server: {error}");
            return false;
        }

        Multiplayer.MultiplayerPeer = _peer;
        IsConnectedToServer = true;
        PlayerManager.Instance.JoinPlayer(new PlayerData(LocalPlayerId, Player.LocalId));
        
        return true;
    }

    public bool CreateClient(string ip, int port)
    {
        if (IsConnectedToServer)
        {
            Disconnect();
        }
        
        _peer = new ENetMultiplayerPeer();
        Error error = _peer.CreateClient(ip, port);
        if (Error.Ok != error)
        {
            GD.PrintErr($"Failed to create client: {error}");
            return false;
        }
        Multiplayer.MultiplayerPeer = _peer;
        return true;
    }

    public void Disconnect()
    {
        if (null != _peer)
        {
            _peer.Close();
            _peer.Dispose();
            _peer = null;
        }
        
        Multiplayer.MultiplayerPeer = null;
        IsConnectedToServer = false;
    }

    // <== Events ==>
    // === === Client === ===
    private void OnConnectedToServer()
    {
        IsConnectedToServer = true;
        PlayerData data = new PlayerData(LocalPlayerId, Player.LocalId);
        RpcId(1, MethodName.ReceivePlayerInfoFromClient, JsonSerializer.Serialize(data));
    }

    private void OnPeerDisconnected_Client(long id)
    {
        if (Multiplayer.IsServer()) return;
        PlayerManager.Instance.ClearPlayers();
    }
    
    // === === Server === === 
    private void OnPeerDisconnected_Server(long id)
    {
        if (!Multiplayer.IsServer()) return;
        PlayerManager.Instance.LeavePlayer(id);
    }
    
    // <== Rpc ==>
    // === === Server === === 
    [Rpc(MultiplayerApi.RpcMode.AnyPeer, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    public void ReceivePlayerInfoFromClient(string playerInfo)
    {
        PlayerData data = JsonSerializer.Deserialize<PlayerData>(playerInfo);
        long senderId = Multiplayer.GetRemoteSenderId();
        if (senderId == data.UniqueId)
        {
            PlayerManager.Instance.JoinPlayer(data);
        }
    }
    
}