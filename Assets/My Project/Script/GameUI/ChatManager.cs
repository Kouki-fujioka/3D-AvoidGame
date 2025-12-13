using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

namespace Unity.Game.UI
{
    public class ChatManager : MonoBehaviour
    {
        [Header("UI参照")]
        [SerializeField] GameObject chatPanel;
        [SerializeField] TMP_InputField inputField;
        [SerializeField] Transform historyContent;
        [SerializeField] ScrollRect scrollRect;

        [Header("プレハブ設定")]
        // ★変更点：プレハブを2つ登録できるようにする
        [SerializeField] GameObject userMessagePrefab;   // 自分の質問用（左寄せ）
        [SerializeField] GameObject systemMessagePrefab; // システム回答用（右寄せ）

        [Header("データ")]
        [SerializeField] TextAsset knowledgeBaseText;

        private bool isChatActive = false;
        private string[] knowledgeChunks;

        void Start()
        {
            chatPanel.SetActive(false);
            if (knowledgeBaseText != null)
            {
                knowledgeChunks = knowledgeBaseText.text
                    .Split(new[] { '\n', '\r' }, System.StringSplitOptions.RemoveEmptyEntries);
            }
            inputField.onSubmit.AddListener(OnSubmitChat);
        }

        void Update()
        {
            if (Input.GetKeyDown(KeyCode.Return))
            {
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
                ToggleChat(false); // 空でEnterなら閉じる
                return;
            }

            // 1. 自分の質問を表示（isUser = true）
            // "You:" などの接頭辞はUIで左右分かれるので消してもOKです
            AddMessageToLog(text, true);

            // 2. 回答を検索して表示（isUser = false）
            string answer = SearchAnswer(text);
            AddMessageToLog(answer, false);

            inputField.text = "";
            inputField.ActivateInputField();
        }

        string SearchAnswer(string query)
        {
            if (knowledgeChunks == null || knowledgeChunks.Length == 0) return "データベースがありません。";

            var keywords = query.Split(new[] { ' ', '　' }, System.StringSplitOptions.RemoveEmptyEntries);
            string bestMatch = null;
            int maxScore = 0;

            foreach (var line in knowledgeChunks)
            {
                int score = 0;
                foreach (var keyword in keywords)
                    if (line.Contains(keyword)) score++;

                if (score > maxScore)
                {
                    maxScore = score;
                    bestMatch = line;
                }
            }
            return maxScore > 0 ? bestMatch : "関連する情報が見つかりませんでした。";
        }

        //// ★変更点：isUserフラグを追加
        void AddMessageToLog(string text, bool isUser)
        {
            // フラグによって使うプレハブを変える
            GameObject prefabToUse = isUser ? userMessagePrefab : systemMessagePrefab;

            GameObject newMsg = Instantiate(prefabToUse, historyContent);

            var tmp = newMsg.GetComponent<TextMeshProUGUI>();
            if (tmp != null)
            {
                tmp.text = text;
            }

            LayoutRebuilder.ForceRebuildLayoutImmediate(newMsg.GetComponent<RectTransform>());

            if (historyContent != null)
            {
                LayoutRebuilder.ForceRebuildLayoutImmediate(historyContent.GetComponent<RectTransform>());
            }

            StartCoroutine(ScrollToBottom());
        }

        System.Collections.IEnumerator ScrollToBottom()
        {
            yield return new WaitForEndOfFrame();
            scrollRect.verticalNormalizedPosition = 0f;
        }
    }
}
