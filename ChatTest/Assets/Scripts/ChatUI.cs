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

    [Header("平台配置")]
    public float androidKeyboardOffset = 300f; // 安卓键盘弹出时的偏移
    public float androidMessageFontSize = 16f; // 安卓消息字体大小

    private RectTransform scrollRectTransform;
    private float originalScrollRectY;

    private void Awake()
    {
        // 如果是Linux服务器，不需要UI
        if (Application.platform == RuntimePlatform.LinuxServer)
        {
            Destroy(gameObject);
            return;
        }

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
        AddSystemMessage($"聊天系统已就绪 | 平台: {Application.platform}");

        // 如果是安卓平台，应用特殊设置
        if (Application.platform == RuntimePlatform.Android)
        {
            ApplyAndroidOptimizations();
        }

        // 保存原始位置
        if (scrollRect != null)
        {
            scrollRectTransform = scrollRect.GetComponent<RectTransform>();
            originalScrollRectY = scrollRectTransform.anchoredPosition.y;
        }
    }

    // 应用安卓优化
    private void ApplyAndroidOptimizations()
    {
        Debug.Log("应用安卓平台UI优化");

        // 调整输入框大小
        if (messageInput != null)
        {
            // 安卓上输入框稍微大一些
            RectTransform inputRT = messageInput.GetComponent<RectTransform>();
            if (inputRT != null)
            {
                inputRT.sizeDelta = new Vector2(inputRT.sizeDelta.x, 60f);
            }
        }

        // 调整按钮大小
        if (sendButton != null)
        {
            RectTransform buttonRT = sendButton.GetComponent<RectTransform>();
            if (buttonRT != null)
            {
                buttonRT.sizeDelta = new Vector2(120f, 60f);
            }
        }
    }

    // 更新连接状态显示
    public void UpdateConnectionStatus(bool isConnected)
    {
        if (statusText == null) return;

        if (isConnected)
        {
            statusText.text = "状态: <color=green>已连接</color>";
            statusText.color = Color.green;

            if (connectButton != null)
            {
                Text buttonText = connectButton.GetComponentInChildren<Text>();
                if (buttonText != null) buttonText.text = "断开连接";
            }
        }
        else
        {
            statusText.text = "状态: <color=red>未连接</color>";
            statusText.color = Color.red;

            if (connectButton != null)
            {
                Text buttonText = connectButton.GetComponentInChildren<Text>();
                if (buttonText != null) buttonText.text = "连接服务器";
            }
        }

        // 控制输入框和发送按钮
        if (messageInput != null) messageInput.interactable = isConnected;
        if (sendButton != null) sendButton.interactable = isConnected;
    }

    // 添加普通消息
    public void AddMessage(string sender, string message, Color senderColor)
    {
        if (messageContent == null) return;

        if (messageContent.childCount >= maxMessages)
        {
            // 删除最旧的消息
            Destroy(messageContent.GetChild(0).gameObject);
        }

        // 创建新消息项
        if (messageItemPrefab == null)
        {
            Debug.LogError("消息预制体未设置！");
            return;
        }

        GameObject newMessage = Instantiate(messageItemPrefab, messageContent);
        Text textComponent = newMessage.GetComponent<Text>();

        if (textComponent == null)
        {
            Debug.LogError("消息预制体缺少Text组件！");
            return;
        }

        // 应用平台特定的字体大小
        if (Application.platform == RuntimePlatform.Android)
        {
            textComponent.fontSize = Mathf.RoundToInt(androidMessageFontSize);
        }

        // 格式化消息
        string colorHex = ColorUtility.ToHtmlStringRGB(senderColor);
        textComponent.text = $"<color=#{colorHex}>[{sender}]</color>: {message}";

        // 自动滚动到底部
        ScrollToBottom();
    }

    // 滚动到底部
    private void ScrollToBottom()
    {
        if (scrollRect != null)
        {
            Canvas.ForceUpdateCanvases();
            scrollRect.verticalNormalizedPosition = 0f;
        }
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
        if (messageContent == null) return;

        foreach (Transform child in messageContent)
        {
            Destroy(child.gameObject);
        }
    }

    // 安卓键盘事件处理
    private void Update()
    {
        // 安卓平台处理键盘弹出
        if (Application.platform == RuntimePlatform.Android)
        {
            HandleAndroidKeyboard();
        }
    }

    // 处理安卓键盘
    private void HandleAndroidKeyboard()
    {
        if (scrollRectTransform == null || messageInput == null) return;

        // 当输入框获得焦点时（键盘弹出），上移滚动视图
        if (messageInput.isFocused)
        {
            float targetY = originalScrollRectY + androidKeyboardOffset;
            scrollRectTransform.anchoredPosition = Vector2.Lerp(
                scrollRectTransform.anchoredPosition,
                new Vector2(scrollRectTransform.anchoredPosition.x, targetY),
                Time.deltaTime * 5f
            );
        }
        else
        {
            // 输入框失去焦点时，恢复位置
            scrollRectTransform.anchoredPosition = Vector2.Lerp(
                scrollRectTransform.anchoredPosition,
                new Vector2(scrollRectTransform.anchoredPosition.x, originalScrollRectY),
                Time.deltaTime * 5f
            );
        }
    }
}