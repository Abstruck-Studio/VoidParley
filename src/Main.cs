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
        // 设置本地玩家标识（示例：使用设备名+时间戳）
        Player.LocalId = $"{OS.GetName()}_{Time.GetUnixTimeFromSystem()}";

        // 创建 UI 控件
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

        // 玩家列表显示
        _playerList = new ItemList();
        _playerList.SizeFlagsVertical = Control.SizeFlags.ExpandFill;
        container.AddChild(_playerList);

        // 定时刷新玩家列表（避免频繁修改其他类）
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

    private void RefreshPlayerList()
    {
        // 需要 PlayerManager 提供公共方法： public List<PlayerData> GetPlayers() => _players.Values.ToList();
        // 同时需在文件顶部添加 using System.Linq;
        List<PlayerData> players = PlayerManager.Instance.GetPlayers();
        
        _playerList.Clear();
        foreach (var p in players)
        {
            string isLocal = (p.UniqueId == NetworkManager.Instance.LocalPlayerId) ? " (自己)" : "";
            _playerList.AddItem($"{p.Id} [{p.UniqueId}]{isLocal}");
        }
    }
}
