using System.Collections.Generic;
using HietakissaUtils.Commands;
using System.Collections;
using HietakissaUtils;
using UnityEngine.UI;
using UnityEngine;
using System.Text;
using System.Linq;
using System;
using TMPro;
using JetBrains.Annotations;

public class ConsoleController : MonoBehaviour
{
    public static ConsoleController Instance;

    #region Base Console Variables
    [Header("Base Console")]
    [SerializeField] bool showConsole = false;
    [SerializeField] KeyCode toggleConsole = KeyCode.BackQuote;

    [SerializeField] CursorSettings openSettings;
    [SerializeField] CursorSettings closeSettings;

    List<DebugCommandBase> matches = new List<DebugCommandBase>();
    List<DebugCommandBase> hiddenCommands = new List<DebugCommandBase>();

    int lastMatchCount;
    int selectionIndex;

    string input;
    #endregion
    #region Debug Console Variables
    [Header("Debug Console")]
    [SerializeField] bool enableDebugConsole = false;
    [SerializeField] int consoleHistory = 100;

    [SerializeField] bool autoScroll = true;
    int commandCount;
    bool shouldUpdate;
    bool paused;

    List<LogMessage> logMessages = new List<LogMessage>();
    int normalLogs, warningLogs, errorLogs;

    List<string> availableTags = new List<string>();
    string selectedTag = "all";
    int lastTagIndex;

    List<TMP_Dropdown.OptionData> data = new List<TMP_Dropdown.OptionData>();
    TMP_Dropdown dropdown;
    #endregion

    [Header("FPS Update Settings")]
    [SerializeField] float fpsUpdateDelay;
    float delayTime;

    Color hietakissaLogColor = new Color(0.77f, 0.63f, 0.87f);

    [Header("Object References")]
    [SerializeField] GameObject textLine;
    [SerializeField] TMP_InputField inputField;
    [SerializeField] RectTransform debugLog;

    #region Random References
    Scrollbar commandPreviewScroll;

    RectTransform commandPreviewScrollRect;
    RectTransform commandPreviewContent;

    Scrollbar debugLogScroll;
    RectTransform debugLogContent;

    RectTransform extras;

    TextMeshProUGUI pauseButtonText;
    TextMeshProUGUI autoscrollButtonText;

    TextMeshProUGUI fpsText;
    TextMeshProUGUI logText;


    RectTransform child;
    #endregion

    void Awake()
    {
        Instance = this;

        #region Find UI Elements

        inputField = transform.Find("InputField").GetComponent<TMP_InputField>();
        commandPreviewScroll = transform.Find("InputField").Find("Command Preview Scroll").Find("Scrollbar Vertical").GetComponent<Scrollbar>();

        commandPreviewScrollRect = (RectTransform)transform.Find("InputField").Find("Command Preview Scroll");
        commandPreviewContent = (RectTransform)commandPreviewScrollRect.Find("Viewport").Find("Content");

        debugLogScroll = transform.Find("Debug Console").Find("Debug Console Scroll").Find("Scrollbar Vertical").GetComponent<Scrollbar>();
        debugLogContent = (RectTransform)transform.Find("Debug Console").Find("Debug Console Scroll").Find("Viewport").Find("Content");
        
        extras = (RectTransform)transform.Find("Debug Console").Find("Debug Console Scroll").Find("Pause Button").Find("Extras");

        pauseButtonText = transform.Find("Debug Console").Find("Debug Console Scroll").Find("Pause Button").Find("Text (TMP)").GetComponent<TextMeshProUGUI>();
        autoscrollButtonText = extras.Find("Autoscroll Button").Find("Text (TMP)").GetComponent<TextMeshProUGUI>();

        debugLog = (RectTransform)transform.Find("Debug Console");

        fpsText = transform.Find("Debug Console").Find("FPS Text").GetComponent<TextMeshProUGUI>();
        logText = extras.Find("Background").Find("Log Text").GetComponent<TextMeshProUGUI>();

        dropdown = extras.Find("Dropdown").GetComponent<TMP_Dropdown>();

        #endregion

        CommandSystem.AddCommand(new DebugCommand("toggle_debug_console", () => { enableDebugConsole = !enableDebugConsole; }, true));

        Application.logMessageReceived += LogReceived;
    }

    void Start()
    {
        CommandSystem.commandList.Sort();

        commandCount = CommandSystem.commandList.Count;
        for (int i = 0; i < commandCount; i++)
        {
            if (CommandSystem.commandList[i].hidden)
            {
                hiddenCommands.Add(CommandSystem.commandList[i]);
                continue;
            }
            Instantiate(textLine, commandPreviewContent);
        }

        commandCount -= hiddenCommands.Count;
        commandPreviewScroll.value = 1;

        for (int i = 0; i < consoleHistory; i++)
        {
            Instantiate(textLine, debugLogContent);
        }

        UpdateDebugConsole();

        UpdateInput("");
    }

    void OnDisable()
    {
        Application.logMessageReceived -= LogReceived;
    }

    void LogReceived(string condition, string stackTrace, LogType type)
    {
        if (consoleHistory == 0) return;

        if (paused) return;

        if (type == LogType.Error) errorLogs++;
        else if (type == LogType.Warning) warningLogs++;
        else normalLogs++;

        while (logMessages.Count >= consoleHistory) logMessages.RemoveAt(0);
        logMessages.Add(new LogMessage(condition, type, new string[] { "normal" }, GetColorFromLogType(type)));

        shouldUpdate = true;
    }

    void LateUpdate()
    {
        if (Input.GetKeyDown(toggleConsole))
        {
            showConsole = !showConsole;

            if (!showConsole)
            {
                //Cursor.lockState = CursorLockMode.Locked;
                //Cursor.visible = false;

                Cursor.lockState = openSettings.cursorLockMode;
                Cursor.visible = openSettings.visible;

                inputField.gameObject.SetActive(false);
            }
            else
            {
                //Cursor.lockState = CursorLockMode.None;
                //Cursor.visible = true;

                Cursor.lockState = closeSettings.cursorLockMode;
                Cursor.visible = closeSettings.visible;

                inputField.gameObject.SetActive(true);
            }
        }

        if (showConsole)
        {
            PreviewNavigation();
            SetInputFieldText(input, false);
            if (Input.GetKeyDown(KeyCode.Return)) ProcessInput();
        }

        if (enableDebugConsole)
        {
            debugLog.gameObject.SetActive(true);
            UpdateFPSText();
        }
        else debugLog.gameObject.SetActive(false);

        if (shouldUpdate) UpdateDebugConsole();
    }

    #region Base Console

    public void UpdateInput(string input)
    {
        if (Input.GetKeyDown(toggleConsole)) return;

        matches.Clear();
        this.input = input;

        string[] inputCommand = input.Split(" ");

        foreach (DebugCommandBase command in CommandSystem.commandList)
        {
            if (command.hidden) continue;
            if (command.commandName.StartsWith(inputCommand[0]) && CommandInputMatchCommand(command, inputCommand)) matches.Add(command);
        }

        foreach (DebugCommandBase command in CommandSystem.commandList)
        {
            if (command.hidden) continue;
            if (command.commandName.Contains(inputCommand[0]) && !matches.Contains(command) && CommandInputMatchCommand(command, inputCommand)) matches.Add(command);
        }

        int matchCount = matches.Count;
        int maxMatchIndex = Mathf.Min(matchCount, commandCount) - 1;

        for (int i = 0; i < commandCount; i++)
        {
            Transform child = commandPreviewContent.GetChild(i);
            if (i <= maxMatchIndex)
            {
                child.gameObject.SetActive(true);
                TextMeshProUGUI text = child.GetComponent<TextMeshProUGUI>();
                text.text = TextFromCommand(matches[i]);

                if (selectionIndex == i) text.color = Color.yellow;
                else text.color = Color.white;
            }
            else child.gameObject.SetActive(false);
        }

        int scrollHeight = Mathf.Min(200, matchCount * 40);
        commandPreviewScrollRect.anchoredPosition = commandPreviewScrollRect.anchoredPosition.SetY(scrollHeight);
        commandPreviewScrollRect.sizeDelta = commandPreviewScrollRect.sizeDelta.SetY(scrollHeight);

        StartCoroutine(UpdateScroll());

        bool CommandInputMatchCommand(DebugCommandBase command, string[] inputCommand)
        {
            int typesLength = command.types.Length;

            if (inputCommand.Length - 1 > typesLength) return false;
            if (inputCommand.Length - 1 < typesLength) return true;

            for (int t = 0, emptyTypes = 0; t < typesLength; t++)
            {
                if (emptyTypes == 1) return false;

                Variable variable = TypeToVariable(command.types[t]);

                if (inputCommand[t + 1] == "")
                {
                    emptyTypes++;
                    continue;
                }
                if (variable == Variable.Int && !int.TryParse(inputCommand[t + 1], out int i)) return false;
                if (variable == Variable.Bool && !bool.TryParse(inputCommand[t + 1], out bool b)) return false;
                if (variable == Variable.Float && !float.TryParse(inputCommand[t + 1].Replace(".", ","), out float f)) return false;
            }

            return true;
        }

        string TextFromCommand(DebugCommandBase command)
        {
            if (command.types.Length == 0) return command.commandName;

            StringBuilder sb = new StringBuilder();

            sb.Append($"{command.commandName}<");
            for (int i = 0; i < command.types.Length; i++)
            {
                sb.Append($"{TypeToVariable(command.types[i])}{(i == command.types.Length - 1 ? "" : ",")}");
            }
            sb.Append(">");

            return sb.ToString();
        }

        Variable TypeToVariable(Type type)
        {
            if (type == typeof(int)) return Variable.Int;
            if (type == typeof(bool)) return Variable.Bool;
            if (type == typeof(float)) return Variable.Float;
            if (type == typeof(string)) return Variable.String;
            return Variable.VariableNotDefined;
        }
    }

    public void ProcessInput()
    {
        string[] properties = input.Replace(".", ",").Split(" ");
        int length = properties.Length - 1;

        bool foundValidCommand = false;

        foreach (DebugCommandBase commandBase in matches)
        {
            if (CheckIfValidCommand(commandBase))
            {
                foundValidCommand = true;
                break;
            }
        }

        if (!foundValidCommand)
        {
            foreach (DebugCommandBase commandBase in hiddenCommands)
            {
                if (CheckIfValidCommand(commandBase))
                {
                    foundValidCommand = true;
                    break;
                }
            }
        }

        if (!foundValidCommand) CommandNotValid();

        SetInputFieldText("");
        UpdateInput("");

        bool CheckIfValidCommand(DebugCommandBase commandBase)
        {
            if (properties[0] == commandBase.commandName)
            {
                if (commandBase as DebugCommand != null)
                {
                    (commandBase as DebugCommand).Execute();
                    return true;
                }
                else if (commandBase.TryExecute(length, properties))
                {
                    return true;
                }
            }
            return false;
        }

        void CommandNotValid()
        {
            Debug.Log("Command parameters not valid");
        }
    }

    void PreviewNavigation()
    {
        if (lastMatchCount != matches.Count) selectionIndex = 0;

        bool input = false;
        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            selectionIndex--;
            input = true;
        }
        else if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            selectionIndex++;
            input = true;
        }

        if (selectionIndex < 0) selectionIndex = matches.Count - 1;
        else if (matches.Count > 0) selectionIndex %= matches.Count;

        if (Input.GetKeyDown(KeyCode.Tab) && matches.Count > 0)
        {
            this.input = matches[selectionIndex].commandName;

            string commandName = matches[selectionIndex].commandName;
            selectionIndex = 0;
            SetInputFieldText(commandName);
            UpdateInput(this.input);
        }
        else if (input)
        {
            UpdateInput(this.input);
            SetInputFieldText(this.input, true);
        }
        lastMatchCount = matches.Count;
    }

    void SetInputFieldText(string text, bool setCaret = true)
    {
        inputField.text = text;
        inputField.ActivateInputField();
        if (setCaret) inputField.caretPosition = text.Length;
    }

    IEnumerator UpdateScroll()
    {
        yield return new WaitForEndOfFrame();

        float scrollPercentage;
        if (selectionIndex <= 4 || matches.Count <= 5) scrollPercentage = 1;
        else scrollPercentage = Maf.FlipOne((selectionIndex - 4) / (float)(matches.Count - 5));

        commandPreviewScroll.value = scrollPercentage;
    }

    #endregion
    #region Debug console

    void UpdateDebugConsole()
    {
        shouldUpdate = false;

        availableTags.Clear();

        int messageCount = logMessages.Count;
        int maxMessageIndex = Mathf.Min(messageCount, consoleHistory) - 1;
        for (int i = 0; i < consoleHistory; i++)
        {
            child = (RectTransform)debugLogContent.GetChild(i);
            if (i <= maxMessageIndex)
            {
                LogMessage log = logMessages[i];

                foreach (string tag in log.tags) if (!availableTags.Contains(tag)) availableTags.Add(tag);

                if (!log.tags.Contains(selectedTag))
                {
                    child.gameObject.SetActive(false);
                    continue;
                }

                child.gameObject.SetActive(true);

                string logText = TextFromLog(log);
                int height = logText.Split('\n').Length * 40;

                child.sizeDelta = new Vector2(child.sizeDelta.x, height);

                TextMeshProUGUI text = child.GetComponent<TextMeshProUGUI>();
                text.text = logText;
                text.color = log.color;
            }
            else child.gameObject.SetActive(false);
        }

        if (messageCount != 0)
        {
            ShowExtras();
        }
        else HideExtras();

        if (autoScroll) StartCoroutine(SetDebugConsoleScroll());

        string TextFromLog(LogMessage log)
        {
            return $"{log.timeString} - {log.content}";
        }

        void ShowExtras()
        {
            extras.gameObject.SetActive(true);

            logText.text = $"<color=\"white\">{normalLogs} <color=\"yellow\">{warningLogs} <color=\"red\">{errorLogs}";

            availableTags.Sort();

            dropdown.ClearOptions();
            data.Clear();

            foreach (string option in availableTags) data.Add(new TMP_Dropdown.OptionData(option));

            dropdown.AddOptions(data);
            dropdown.value = lastTagIndex;
        }

        void HideExtras()
        {
            extras.gameObject.SetActive(false);
        }
    }

    void UpdateFPSText()
    {
        delayTime += Time.deltaTime;

        if (delayTime >= fpsUpdateDelay)
        {
            fpsText.text = $"{(int)(1 / Time.smoothDeltaTime)} ({Application.targetFrameRate})";
            delayTime -= fpsUpdateDelay;
        }
    }

    public void TogglePause()
    {
        paused = !paused;

        pauseButtonText.text = paused ? "Resume" : "Pause";
    }

    public void ClearLogs()
    {
        logMessages.Clear();

        normalLogs = 0;
        warningLogs = 0;
        errorLogs = 0;

        shouldUpdate = true;
    }

    public void ToggleAutoScroll()
    {
        autoScroll = !autoScroll;

        autoscrollButtonText.text = autoScroll ? "Autoscroll On" : "Autoscroll Off";

        if (autoScroll) SetDebugConsoleScrolll();
    }

    public void ExportLogs()
    {
        StringBuilder sb = new StringBuilder();

        int logMessagesCount = logMessages.Count;

        sb.AppendLine($"Logs: {normalLogs}, Warnings: {warningLogs}, Errors: {errorLogs}");
        for (int i = 0; i < logMessagesCount; i++)
        {
            sb.AppendLine($"{logMessages[i].timeString} {logMessages[i].type.ToString().ToUpper()} {logMessages[i].content}");
        }

        string logsText = sb.ToString();

        System.IO.File.WriteAllText($"{Application.dataPath}/log.txt", logsText);

        Log($"Log output to '{Application.dataPath}/log.txt'", hietakissaLogColor, "HietakissaUtils");
    }

    public void SetLogHeight(int height)
    {
        debugLog.sizeDelta = new Vector2(debugLog.sizeDelta.x, (height + 1) * 40);
    }

    public void SetselectedTag(int tagIndex)
    {
        selectedTag = availableTags[tagIndex];
        lastTagIndex = tagIndex;
        shouldUpdate = true;
    }

    IEnumerator SetDebugConsoleScroll()
    {
        yield return new WaitForEndOfFrame();
        debugLogScroll.value = 0;
    }

    void SetDebugConsoleScrolll()
    {
        debugLogScroll.value = 0;
    }

    public void Log(string message, params string[] tags)
    {
        if (consoleHistory == 0) return;

        if (paused) return;

        TryLog(new LogMessage(message, LogType.Log, tags, Color.white));
    }

    public void Log(string message, Color color, params string[] tags)
    {
        if (consoleHistory == 0) return;

        if (paused) return;

        TryLog(new LogMessage(message, LogType.Log, tags, color));
    }

    void TryLog(LogMessage message)
    {
        normalLogs++;

        while (logMessages.Count >= consoleHistory) logMessages.RemoveAt(0);
        logMessages.Add(message);

        message.tags.Add("custom");

        shouldUpdate = true;
    }

    #endregion

    class LogMessage
    {
        public string timeString;
        public string content;
        public LogType type;
        public List<string> tags;
        public Color color;

        public LogMessage(string content, LogType type, string[] tags, Color color)
        {
            DateTime time = DateTime.Now;
            timeString = $"[{Time.frameCount} - {time.Hour.ToString("00")}:{time.Minute.ToString("00")}:{time.Second.ToString("00")}]";
            this.content = content;
            this.type = type;
            this.tags = tags.ToList();

            if (!this.tags.Contains("all")) this.tags.Add("all");

            this.color = color;
        }
    }

    [Serializable]
    class CursorSettings
    {
        public CursorLockMode cursorLockMode;
        public bool visible;
    }

    Color GetColorFromLogType(LogType type)
    {
        if (type == LogType.Error) return Color.red;
        if (type == LogType.Warning) return Color.yellow;
        else return Color.white;
    }

    enum Variable
    {
        Int,
        Bool,
        Float,
        String,
        VariableNotDefined
    }
}