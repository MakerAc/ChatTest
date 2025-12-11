// Scripts/Network/ChatNetworkManager.cs
using Mirror;
using UnityEngine;

public class ChatNetworkManager : NetworkManager
{
    [Header("聊天服务器设置")]
    [Tooltip("服务器IP地址")]
    public string serverAddress = "127.0.0.1";
    [Tooltip("服务器端口")]
    public int serverPort = 7777;


    public override void Start()
    {
        base.Start();

        // 设置网络地址
        networkAddress = serverAddress;
    }

    // 客户端连接成功
    public override void OnClientConnect()
    {
        base.OnClientConnect();
        Debug.Log("成功连接到服务器!");

        // 通知UI更新状态
        ChatUI.Instance.UpdateConnectionStatus(true);
    }

    // 客户端断开连接
    public override void OnClientDisconnect()
    {
        base.OnClientDisconnect();
        Debug.Log("与服务器断开连接");

        // 通知UI更新状态
        ChatUI.Instance.UpdateConnectionStatus(false);
    }

    // 玩家加入时生成玩家预制体
    public override void OnServerAddPlayer(NetworkConnectionToClient conn)
    {
        GameObject player = Instantiate(playerPrefab);
        NetworkServer.AddPlayerForConnection(conn, player);

        Debug.Log($"新玩家加入: {conn.address}");
    }
}