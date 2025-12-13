using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using TMPro;
using System.Collections;
using System.Text;

namespace Unity.Game.UI
{
    public class ChatManager : MonoBehaviour
    {
        [Header("UI参照")]
        [SerializeField] GameObject chatPanel;
        [SerializeField] TMP_InputField inputField;
        [SerializeField] Transform historyContent;
        [SerializeField] ScrollRect scrollRect;
        [SerializeField] MenuManager menuManager;

        [Header("プレハブ設定")]
        [SerializeField] GameObject userMessagePrefab;
        [SerializeField] GameObject systemMessagePrefab;

        [Header("データ")]
        [SerializeField] TextAsset knowledgeBaseText; // ゲームの知識データ

        [Header("Gemini設定")]
        [SerializeField] string geminiApiKey = "ここにAPIキー";
        [SerializeField] string geminiModel = "gemini-1.5-flash";

        private bool isChatActive = false;
        private bool canOpenChat = true;

        // API URL
        private const string BaseUrl = "https://generativelanguage.googleapis.com/v1beta/models/";

        public bool IsActive => isChatActive;

        void Start()
        {
            chatPanel.SetActive(false);
            inputField.onSubmit.AddListener(OnSubmitChat);
        }

        void Update()
        {
            // メニュー中のガード処理
            if (Time.timeScale == 0f && !isChatActive)
            {
                canOpenChat = false;
                return;
            }
            if (!canOpenChat)
            {
                canOpenChat = true;
                return;
            }

            if (Input.GetKeyDown(KeyCode.Return))
            {
                if (menuManager != null && menuManager.IsActive) return;

                if (!isChatActive) ToggleChat(true);
                else if (!inputField.isFocused) inputField.ActivateInputField();
            }

            if (isChatActive && Input.GetKeyDown(KeyCode.Escape)) ToggleChat(false);
        }

        void ToggleChat(bool active)
        {
            isChatActive = active;
            chatPanel.SetActive(active);

            if (active)
            {
                Time.timeScale = 0f;
#if !UNITY_EDITOR
                Cursor.lockState = CursorLockMode.None;
#endif
                Cursor.visible = true;
                inputField.text = "";
                inputField.ActivateInputField();
            }
            else
            {
                Time.timeScale = 1f;
#if !UNITY_EDITOR
                Cursor.lockState = CursorLockMode.Locked;
#endif
                Cursor.visible = false;
            }
        }

        void OnSubmitChat(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                ToggleChat(false);
                return;
            }

            // 1. 自分の質問を表示
            AddMessageToLog(text, true);

            // 2. Geminiに「テキスト全文」と「質問」を丸投げする
            // （簡易検索は削除し、AIに判断を委ねる）
            StartCoroutine(CallGeminiSmart(text));

            inputField.text = "";
            inputField.ActivateInputField();
        }

        void AddMessageToLog(string text, bool isUser)
        {
            GameObject prefabToUse = isUser ? userMessagePrefab : systemMessagePrefab;
            GameObject newMsg = Instantiate(prefabToUse, historyContent);

            var tmp = newMsg.GetComponent<TextMeshProUGUI>();
            if (tmp != null) tmp.text = text;

            StartCoroutine(UpdateLayoutAndScroll(newMsg));
        }

        IEnumerator UpdateLayoutAndScroll(GameObject msgObj)
        {
            yield return null;
            LayoutRebuilder.ForceRebuildLayoutImmediate(msgObj.GetComponent<RectTransform>());
            if (historyContent != null)
                LayoutRebuilder.ForceRebuildLayoutImmediate(historyContent.GetComponent<RectTransform>());
            yield return null;
            scrollRect.verticalNormalizedPosition = 0f;
        }

        // ★ここが核心部分：賢いGemini呼び出し
        IEnumerator CallGeminiSmart(string userMessage)
        {
            // txtの中身を取得（なければ空文字）
            string contextData = knowledgeBaseText != null ? knowledgeBaseText.text : "";

            // プロンプト作成：AIへの条件付き命令
            // 「Contextを優先しろ。でも載ってないなら普通に答えろ」という指示
            string prompt = $@"
あなたはゲーム内のアシスタントAIです。
以下の【Context（資料）】を読み、ユーザーの【Question（質問）】に答えてください。

### ルール
1. もし質問の答えが【Context】の中にある情報で説明できるなら、その情報を使って回答してください。
2. もし質問が【Context】の内容と無関係、あるいは【Context】に答えがない場合は、【Context】を無視して、あなたの一般的な知識で回答してください。
3. 回答は自然な日本語で行ってください。「資料によると」などの前置きは不要です。

【Context】
{contextData}

【Question】
{userMessage}
";

            // JSON作成
            string jsonBody = "{\"contents\":[{\"parts\":[{\"text\":\"" + EscapeJson(prompt) + "\"}]}]}";
            byte[] rawData = Encoding.UTF8.GetBytes(jsonBody);

            string url = $"{BaseUrl}{geminiModel}:generateContent?key={geminiApiKey}";

            using (UnityWebRequest request = new UnityWebRequest(url, "POST"))
            {
                request.uploadHandler = new UploadHandlerRaw(rawData);
                request.downloadHandler = new DownloadHandlerBuffer();
                request.SetRequestHeader("Content-Type", "application/json");

                yield return request.SendWebRequest();

                if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
                {
                    Debug.LogError("通信エラー: " + request.error);
                    AddMessageToLog("通信エラーが発生しました。", false);
                }
                else
                {
                    string jsonResponse = request.downloadHandler.text;
                    string answer = ExtractAnswer(jsonResponse);
                    AddMessageToLog(answer, false);
                }
            }
        }

        // JSONエスケープ
        private string EscapeJson(string str)
        {
            if (str == null) return "";
            return str
                .Replace("\\", "\\\\")
                .Replace("\"", "\\\"")
                .Replace("\n", "\\n")
                .Replace("\r", "")
                .Replace("\t", "\\t");
        }

        // 回答抽出
        private string ExtractAnswer(string json)
        {
            string searchKey = "\"text\": \"";
            int startIndex = json.IndexOf(searchKey);
            if (startIndex == -1) return "エラー：回答が見つかりませんでした。";

            startIndex += searchKey.Length;

            int endIndex = startIndex;
            bool isEscaped = false;

            for (int i = startIndex; i < json.Length; i++)
            {
                if (json[i] == '\\') isEscaped = !isEscaped;
                else if (json[i] == '"' && !isEscaped)
                {
                    endIndex = i;
                    break;
                }
                else isEscaped = false;
            }

            string result = json.Substring(startIndex, endIndex - startIndex);
            return result.Replace("\\n", "\n").Replace("\\\"", "\"").Replace("\\t", "\t").Replace("\\\\", "\\");
        }
    }
}
