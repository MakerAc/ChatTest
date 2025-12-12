using Mirror;
using UnityEngine;

public class ChatNetworkManager : NetworkManager
{
    [Header("服务器设置")]
    [Tooltip("客户端连接地址（公网IP）")]
    public string clientConnectAddress = "47.97.57.104"; // 改为您的公网IP

    [Tooltip("服务器监听端口")]
    public int serverPort = 7777;

    [Header("服务器模式配置")]
    [Tooltip("是否为专用服务器模式")]
    public bool isDedicatedServer = false;

    [Tooltip("服务器监听地址（专用服务器用）")]
    public string serverListenAddress = "0.0.0.0";

    [Header("平台配置")]
    [Tooltip("是否启用平台特定优化")]
    public bool enablePlatformOptimization = true;

    // 当前平台
    private RuntimePlatform currentPlatform;

    public override void Start()
    {
        base.Start();

        // 记录当前平台
        currentPlatform = Application.platform;
        Debug.Log($"当前运行平台: {currentPlatform}");

        // 根据平台和模式设置网络地址
        if (isDedicatedServer || IsCommandLineServer())
        {
            // 服务器模式：监听所有地址
            networkAddress = serverListenAddress;
            Debug.Log($"服务器模式启动，监听地址: {serverListenAddress}:{serverPort}");
        }
        else
        {
            // 客户端模式：连接服务器地址
            networkAddress = clientConnectAddress;
            Debug.Log($"客户端模式，连接地址: {clientConnectAddress}:{serverPort}");
        }

        // 应用平台优化
        ApplyPlatformOptimization();

        // 自动启动服务器（如果是服务器模式）
        AutoStartServer();
    }

    // 应用平台优化
    private void ApplyPlatformOptimization()
    {
        if (!enablePlatformOptimization) return;

        switch (currentPlatform)
        {
            case RuntimePlatform.Android:
                // 安卓平台优化
                Debug.Log("应用安卓平台优化");
                // 可以调整帧率、网络参数等
                Application.targetFrameRate = 60;
                break;

            case RuntimePlatform.LinuxServer:
            case RuntimePlatform.LinuxPlayer:
                // Linux服务器优化
                Debug.Log("应用Linux服务器优化");
                // 服务器模式下可以设置更高性能
                QualitySettings.vSyncCount = 0;
                Application.targetFrameRate = 60;
                break;

            default:
                break;
        }
    }

    // 自动启动服务器
    private void AutoStartServer()
    {
        // 如果是Linux服务器平台，自动启动服务器
        if (currentPlatform == RuntimePlatform.LinuxServer ||
            isDedicatedServer ||
            IsCommandLineServer())
        {
            Debug.Log("以服务器模式启动...");
            StartServer();

            // 显示服务器信息
            LogServerInfo();
        }
    }

    // 记录服务器信息
    private void LogServerInfo()
    {
        Debug.Log("========================================");
        Debug.Log("    聊天服务器已启动");
        Debug.Log($"    服务器IP: {GetPublicIP()}");
        Debug.Log($"    监听端口: {serverPort}");
        Debug.Log($"    启动时间: {System.DateTime.Now}");
        Debug.Log("========================================");
    }

    // 获取公网IP
    private string GetPublicIP()
    {
        return "47.97.57.104"; // 您的公网IP
    }

    // 检查命令行参数
    private bool IsCommandLineServer()
    {
        // 检查是否通过命令行以服务器模式启动
        string[] args = System.Environment.GetCommandLineArgs();
        foreach (string arg in args)
        {
            if (arg.ToLower() == "-server" || arg.ToLower() == "-batchmode")
                return true;
        }
        return false;
    }

    // 客户端连接成功
    public override void OnClientConnect()
    {
        base.OnClientConnect();
        Debug.Log($"成功连接到服务器: {clientConnectAddress}:{serverPort}");

        // 通知UI更新状态
        UpdateConnectionStatus(true);
    }

    // 客户端断开连接
    public override void OnClientDisconnect()
    {
        base.OnClientDisconnect();
        Debug.Log("与服务器断开连接");
        
        // 通知UI更新状态
        UpdateConnectionStatus(false);
    }

    // 更新连接状态
    private void UpdateConnectionStatus(bool connected)
    {
        // 如果不是服务器模式，更新UI状态
        if (!isDedicatedServer && currentPlatform != RuntimePlatform.LinuxServer)
        {
            ChatUI.Instance?.UpdateConnectionStatus(connected);
        }
    }

    // 服务器启动成功
    public override void OnStartServer()
    {
        base.OnStartServer();
        Debug.Log($"服务器已启动，监听端口: {serverPort}");
    }

    // 玩家加入时生成玩家预制体
    public override void OnServerAddPlayer(NetworkConnectionToClient conn)
    {
        if (playerPrefab == null)
        {
            Debug.LogError("PlayerPrefab 未设置！");
            return;
        }

        GameObject player = Instantiate(playerPrefab);
        NetworkServer.AddPlayerForConnection(conn, player);

        Debug.Log($"新玩家加入: {conn.address}");

        // 发送欢迎消息
        SendWelcomeMessage(conn);
    }

    // 发送欢迎消息
    private void SendWelcomeMessage(NetworkConnectionToClient conn)
    {
        // 创建一个简单的欢迎消息
        // 这里可以扩展为RPC调用
        Debug.Log($"欢迎玩家 {conn.connectionId} 加入聊天室");
    }
}