// Scripts/UI/ChatUI.cs
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ChatUI : MonoBehaviour
{
    public static ChatUI Instance { get; private set; }

    [Header("UI引用")]
    public Text statusText;
    public InputField messageInput;
    public Button sendButton;
    public Button connectButton;
    public ScrollRect scrollRect;
    public Transform messageContent;

    [Header("预制体")]
    public GameObject messageItemPrefab;

    [Header("设置")]
    public int maxMessages = 100; // 最大消息数量
    public Color systemColor = Color.yellow;
    public Color errorColor = Color.red;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        // 初始化状态
        UpdateConnectionStatus(false);

        // 添加系统消息
        AddSystemMessage("聊天系统已就绪");
    }

    // 更新连接状态显示
    public void UpdateConnectionStatus(bool isConnected)
    {
        if (isConnected)
        {
            statusText.text = "状态: <color=green>已连接</color>";
            statusText.color = Color.green;
            connectButton.GetComponentInChildren<Text>().text = "断开连接";
        }
        else
        {
            statusText.text = "状态: <color=red>未连接</color>";
            statusText.color = Color.red;
            connectButton.GetComponentInChildren<Text>().text = "连接服务器";
        }

        // 控制输入框和发送按钮
        messageInput.interactable = isConnected;
        sendButton.interactable = isConnected;
    }

    // 添加普通消息
    public void AddMessage(string sender, string message, Color senderColor)
    {
        if (messageContent.childCount >= maxMessages)
        {
            // 删除最旧的消息
            Destroy(messageContent.GetChild(0).gameObject);
        }

        // 创建新消息项
        GameObject newMessage = Instantiate(messageItemPrefab, messageContent);
        Text textComponent = newMessage.GetComponent<Text>();

        // 格式化消息
        string colorHex = ColorUtility.ToHtmlStringRGB(senderColor);
        textComponent.text = $"<color=#{colorHex}>[{sender}]</color>: {message}";

        // 自动滚动到底部
        Canvas.ForceUpdateCanvases();
        scrollRect.verticalNormalizedPosition = 0f;
    }

    // 添加系统消息
    public void AddSystemMessage(string message)
    {
        AddMessage("系统", message, systemColor);
    }

    // 添加错误消息
    public void AddErrorMessage(string message)
    {
        AddMessage("错误", message, errorColor);
    }

    // 清空聊天记录
    public void ClearMessages()
    {
        foreach (Transform child in messageContent)
        {
            Destroy(child.gameObject);
        }
    }
}