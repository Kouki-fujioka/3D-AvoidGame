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
        [SerializeField] TextAsset knowledgeBaseText;

        [Header("AI設定")]
        [SerializeField] string geminiModel = "gemini-2.5-flash"; // 2.5を使用

        // API URL (v1betaを使用)
        private const string BaseUrl = "https://generativelanguage.googleapis.com/v1beta/models/";

        // 内部変数
        private string currentApiKey = "";
        private const string PREFS_KEY_NAME = "PlayerGeminiApiKey";
        private bool isChatActive = false;
        private bool canOpenChat = true;

        public bool IsActive => isChatActive;

        void Start()
        {
            chatPanel.SetActive(false);
            if (inputField != null) inputField.onSubmit.AddListener(OnSubmitChat);

            // 保存されたキーを読み込む
            currentApiKey = PlayerPrefs.GetString(PREFS_KEY_NAME, "");
            UpdateInputPlaceholder();
        }

        void Update()
        {
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
                UpdateInputPlaceholder();
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

        // プレースホルダー更新
        void UpdateInputPlaceholder()
        {
            var placeholderText = inputField.placeholder as TextMeshProUGUI;
            if (placeholderText != null)
            {
                if (string.IsNullOrEmpty(currentApiKey))
                {
                    placeholderText.text = "Enter API Key...";
                    inputField.contentType = TMP_InputField.ContentType.Password;
                }
                else
                {
                    placeholderText.text = "Enter text...";
                    inputField.contentType = TMP_InputField.ContentType.Standard;
                }
                inputField.ForceLabelUpdate();
            }
        }

        void OnSubmitChat(string text)
        {
            // 空白チェック
            if (string.IsNullOrWhiteSpace(text))
            {
                ToggleChat(false);
                return;
            }

            // ★リセット機能
            if (text.Trim() == "/reset")
            {
                PlayerPrefs.DeleteKey(PREFS_KEY_NAME);
                PlayerPrefs.Save();
                currentApiKey = "";
                AddMessageToLog("★APIキーをリセットしました。", false);
                UpdateInputPlaceholder();
                inputField.text = "";
                inputField.ActivateInputField();
                return;
            }

            // ★APIキー保存処理（まだキーがない場合）
            if (string.IsNullOrEmpty(currentApiKey))
            {
                // 余計な文字を削除して保存
                string newKey = text.Trim().Replace("\n", "").Replace("\r", "").Replace(" ", "");
                if (!string.IsNullOrEmpty(newKey))
                {
                    currentApiKey = newKey;
                    PlayerPrefs.SetString(PREFS_KEY_NAME, currentApiKey);
                    PlayerPrefs.Save();
                    AddMessageToLog("APIキーを保存しました。", false);
                    UpdateInputPlaceholder();
                }
                inputField.text = "";
                inputField.ActivateInputField();
                return;
            }

            // 通常チャット処理
            AddMessageToLog(text, true);
            // 2. ★修正：入力を一時的に無効化する（ロック）
            inputField.text = "";           // テキストを消す
            inputField.interactable = false; // ★入力を禁止にする
            // プレースホルダーを「考え中...」に変えてユーザーに知らせる
            var placeholder = inputField.placeholder as TextMeshProUGUI;
            if (placeholder != null) placeholder.text = "AI is thinking...";
            StartCoroutine(CallGeminiSmart(text));
            //inputField.text = "";
            //inputField.ActivateInputField();
        }

        void AddMessageToLog(string text, bool isUser)
        {
            GameObject prefabToUse = isUser ? userMessagePrefab : systemMessagePrefab;
            GameObject newMsg = Instantiate(prefabToUse, historyContent);

            // 親ではなく、子（Bubbleの中）にあるテキストを探す
            var tmp = newMsg.GetComponentInChildren<TextMeshProUGUI>();
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

        // ★ChatManager2.cs をベースにした通信処理
        IEnumerator CallGeminiSmart(string userMessage)
        {
            string contextData = knowledgeBaseText != null ? knowledgeBaseText.text : "";

            // プロンプト（ChatManager2準拠）
            string prompt = $@"
あなたはゲーム内のアシスタントAIです。
以下の【Context（資料）】を読み、ユーザーの【Question（質問）】に答えてください。
### ルール
1. もし質問の答えが【Context】の中にある情報で説明できるなら、その情報を使って回答してください。
2. もし質問が【Context】の内容と無関係、あるいは【Context】に答えがない場合は、【Context】を無視して、あなたの一般的な知識で回答してください。
3. 回答は自然な日本語で行ってください。

【Context】
{contextData}

【Question】
{userMessage}
";

            // ★JSON作成：ChatManager2をベースにしつつ、Gemini 2.5対応のため "role": "user" を追加
            // （2.5系はrole指定がないと400エラーになることがあるため、ここだけは安全策をとります）
            string jsonBody = $@"
            {{
                ""contents"": [
                    {{
                        ""role"": ""user"",
                        ""parts"": [
                            {{ ""text"": ""{EscapeJson(prompt)}"" }}
                        ]
                    }}
                ],
                ""generationConfig"": {{
                    ""temperature"": 1
                }}
            }}";

            byte[] rawData = Encoding.UTF8.GetBytes(jsonBody);
            string url = $"{BaseUrl}{geminiModel}:generateContent?key={currentApiKey}";

            // ChatManager2と同じ通信設定
            using (UnityWebRequest request = new UnityWebRequest(url, "POST"))
            {
                request.uploadHandler = new UploadHandlerRaw(rawData);
                request.downloadHandler = new DownloadHandlerBuffer();
                request.SetRequestHeader("Content-Type", "application/json");

                yield return request.SendWebRequest();

                if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
                {
                    string errorDetails = request.downloadHandler.text;
                    Debug.LogError($"通信エラー: {errorDetails}");

                    // エラー時のキー削除判定
                    if (errorDetails.Contains("API_KEY_INVALID") || errorDetails.Contains("API key not valid") || request.responseCode == 403)
                    {
                        AddMessageToLog("APIキーが無効です。/reset と入力して再設定してください。", false);
                        PlayerPrefs.DeleteKey(PREFS_KEY_NAME);
                        currentApiKey = "";
                        UpdateInputPlaceholder();
                    }
                    else
                    {
                        AddMessageToLog($"エラー({request.responseCode})が発生しました。", false);
                    }
                }
                else
                {
                    // ★ChatManager2.cs のExtractAnswerを使って回答を抽出
                    string jsonResponse = request.downloadHandler.text;
                    Debug.Log("受信データ: " + jsonResponse); // 確認用ログ
                    string answer = ExtractAnswer(jsonResponse);
                    AddMessageToLog(answer, false);
                }
            }
            // ★修正：通信が終わったら（成功でも失敗でも）ここを通る
            // 入力ロックを解除する
            inputField.interactable = true;

            // プレースホルダーを「Enter text...」に戻す
            UpdateInputPlaceholder();

            // 再び入力できるようにカーソルを合わせる
            inputField.ActivateInputField();
        }

        // ★ChatManager2.cs からそのまま移植したエスケープ処理
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

        // ★ChatManager2.cs からそのまま移植した抽出処理（これが動いていたロジック）
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
