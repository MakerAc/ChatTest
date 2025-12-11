// Scripts/Network/ChatPlayer.cs
using Mirror;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ChatPlayer : NetworkBehaviour
{
    [SyncVar(hook = nameof(OnPlayerNameChanged))]
    public string playerName = "匿名玩家";

    [SyncVar]
    public Color playerColor = Color.white;

    [Header("UI引用")]
    [SerializeField] private InputField inputField;
    [SerializeField] private Button sendButton;
    [SerializeField] private Button connectButton;

    private ChatUI chatUI;

    // 客户端开始时调用
    public override void OnStartLocalPlayer()
    {
        base.OnStartLocalPlayer();

        // 获取UI引用
        chatUI = ChatUI.Instance;
        if (chatUI == null)
        {
            Debug.LogError("未找到ChatUI实例!");
            return;
        }

        // 获取UI组件
        inputField = chatUI.messageInput;
        sendButton = chatUI.sendButton;
        connectButton = chatUI.connectButton;

        // 绑定按钮事件
        sendButton.onClick.AddListener(SendMessage);
        connectButton.onClick.AddListener(ToggleConnection);

        // 输入框回车发送
        inputField.onSubmit.AddListener((text) => SendMessage());

        // 设置玩家名称
        CmdSetPlayerName($"玩家{Random.Range(1000, 9999)}");

        // 设置随机颜色
        CmdSetPlayerColor(new Color(
            Random.Range(0.5f, 1f),
            Random.Range(0.5f, 1f),
            Random.Range(0.5f, 1f)
        ));
    }

    // 发送消息
    public void SendMessage()
    {
        if (string.IsNullOrWhiteSpace(inputField.text))
            return;

        string message = inputField.text.Trim();
        CmdSendMessageToAll(message);

        // 清空输入框并聚焦
        inputField.text = "";
        inputField.Select();
        inputField.ActivateInputField();
    }

    // 切换连接状态
    private void ToggleConnection()
    {
        NetworkManager nm = NetworkManager.singleton;

        if (nm.mode == NetworkManagerMode.Offline)
        {
            // 连接服务器
            nm.StartClient();
        }
        else if (nm.mode == NetworkManagerMode.ClientOnly)
        {
            // 断开连接
            nm.StopClient();
        }
    }

    // 服务器端：设置玩家名称
    [Command]
    private void CmdSetPlayerName(string name)
    {
        playerName = name;
    }

    // 服务器端：设置玩家颜色
    [Command]
    private void CmdSetPlayerColor(Color color)
    {
        playerColor = color;
    }

    // 服务器端：发送消息给所有人
    [Command]
    private void CmdSendMessageToAll(string message)
    {
        if (string.IsNullOrWhiteSpace(message))
            return;

        // 记录日志
        Debug.Log($"[聊天] {playerName}: {message}");

        // 广播消息
        RpcReceiveMessage(playerName, message, playerColor);
    }

    // 客户端：接收消息
    [ClientRpc]
    private void RpcReceiveMessage(string sender, string message, Color color)
    {
        // 在UI中显示消息
        ChatUI.Instance.AddMessage(sender, message, color);
    }

    // 玩家名称变化时的回调
    private void OnPlayerNameChanged(string oldName, string newName)
    {
        Debug.Log($"玩家名称变更: {oldName} -> {newName}");
    }

    // 清理
    private void OnDestroy()
    {
        if (isLocalPlayer)
        {
            sendButton?.onClick.RemoveListener(SendMessage);
            connectButton?.onClick.RemoveListener(ToggleConnection);
        }
    }
}