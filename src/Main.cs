using System.Collections.Generic;
using Godot;
using VoidParley.core.player;
using VoidParley.network;

namespace VoidParley;

public partial class Main : Node2D
{
    private LineEdit _ipInput;
    private LineEdit _portInput;
    private ItemList _playerList;
    private Timer _refreshTimer;

    public override void _Ready()
    {
        Player.LocalId = $"{OS.GetName()}_{Time.GetUnixTimeFromSystem()}";

        var container = new VBoxContainer();
        container.Size = new Vector2(400, 300);
        container.Position = new Vector2(50, 50);
        AddChild(container);

        // 创建服务器按钮
        var hostButton = new Button();
        hostButton.Text = "创建服务器 (端口 8910)";
        hostButton.Pressed += OnHostPressed;
        container.AddChild(hostButton);

        // 客户端连接区域
        var clientPanel = new HBoxContainer();
        container.AddChild(clientPanel);

        _ipInput = new LineEdit();
        _ipInput.PlaceholderText = "服务器 IP";
        _ipInput.Text = "127.0.0.1";
        clientPanel.AddChild(_ipInput);

        _portInput = new LineEdit();
        _portInput.PlaceholderText = "端口";
        _portInput.Text = "8910";
        clientPanel.AddChild(_portInput);

        var joinButton = new Button();
        joinButton.Text = "加入";
        joinButton.Pressed += OnJoinPressed;
        clientPanel.AddChild(joinButton);

        // 🟢 新增：断开连接按钮
        var disconnectButton = new Button();
        disconnectButton.Text = "断开连接";
        disconnectButton.Pressed += OnDisconnectPressed;
        container.AddChild(disconnectButton);

        // 玩家列表显示
        _playerList = new ItemList();
        _playerList.SizeFlagsVertical = Control.SizeFlags.ExpandFill;
        container.AddChild(_playerList);

        _refreshTimer = new Timer();
        _refreshTimer.WaitTime = 0.5;
        _refreshTimer.Timeout += RefreshPlayerList;
        AddChild(_refreshTimer);
        _refreshTimer.Start();
    }

    private void OnHostPressed()
    {
        int port = 8910;
        bool success = NetworkManager.Instance.CreateServer(port, 4);
        if (success)
            GD.Print("服务器创建成功");
        else
            GD.PrintErr("服务器创建失败");
    }

    private void OnJoinPressed()
    {
        string ip = _ipInput.Text;
        if (!int.TryParse(_portInput.Text, out int port))
        {
            GD.PrintErr("端口无效");
            return;
        }
        bool success = NetworkManager.Instance.CreateClient(ip, port);
        if (success)
            GD.Print("正在连接服务器...");
        else
            GD.PrintErr("连接失败");
    }

    // 🟢 新增：断开连接处理
    private void OnDisconnectPressed()
    {
        if (!NetworkManager.IsConnectedToServer)
        {
            GD.Print("未连接到任何服务器，无需断开");
            return;
        }
        
        NetworkManager.Instance.Disconnect();
        PlayerManager.Instance.ClearPlayers();  // 清空本地玩家缓存
        _playerList.Clear();                    // 立即清空 UI
        GD.Print("已断开连接");
    }

    private void RefreshPlayerList()
    {
        // 如果未连接，PlayerManager 可能为 null，需增加保护
        if (PlayerManager.Instance == null || !NetworkManager.IsConnectedToServer)
        {
            // 可选：清空列表避免显示旧数据
            if (_playerList.ItemCount > 0) _playerList.Clear();
            return;
        }

        List<PlayerData> players = PlayerManager.Instance.GetPlayers();
        _playerList.Clear();
        foreach (var p in players)
        {
            string isLocal = (p.UniqueId == NetworkManager.Instance.LocalPlayerId) ? " (自己)" : "";
            _playerList.AddItem($"{p.Id} [{p.UniqueId}]{isLocal}");
        }
    }
}