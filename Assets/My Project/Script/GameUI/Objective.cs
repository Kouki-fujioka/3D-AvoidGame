using UnityEngine;
using UnityEngine.UI;

namespace Unity.Game.UI
{
    [RequireComponent(typeof(RectTransform))]

    public class Objective : MonoBehaviour
    {
        [Header("参照")]
        [SerializeField, Tooltip("勝敗条件タイトル")] TMPro.TextMeshProUGUI m_Title = default;
        [SerializeField, Tooltip("勝敗条件説明")] TMPro.TextMeshProUGUI m_Description = default;
        [SerializeField, Tooltip("勝敗条件進捗")] TMPro.TextMeshProUGUI m_Progress = default;
        [SerializeField, Tooltip("勝敗条件達成アイコン")] Image m_CompleteIcon = default;
        [SerializeField, Tooltip("アニメーションカーブ")] AnimationCurve m_MoveCurve = default;

        float m_Time;   // 経過時間
        const int s_Margin = 25;    // マージン
        const int s_Space = 4;  // スペース (タイトル ~ 進捗)
        RectTransform m_RectTransform;

        /// <summary>
        /// テキスト (タイトル, 説明, 進捗) を設定
        /// </summary>
        /// <param name="title"></param>
        /// <param name="description"></param>
        /// <param name="progress"></param>
        public void Initialize(string title, string description, string progress)
        {
            m_RectTransform = GetComponent<RectTransform>();

            // テキスト (進捗) を設定
            m_Progress.text = progress;
            m_Progress.ForceMeshUpdate();

            // テキスト (タイトル) を設定
            Vector4 margin = m_Title.margin;
            margin.z = 4 + (string.IsNullOrEmpty(progress) ? 0 : m_Progress.renderedWidth + s_Space);
            m_Title.margin = margin;
            m_Title.text = title;

            // テキスト (説明) を設定
            m_Description.text = description;
        }

        public void OnProgress(IObjective objective)
        {
            m_Progress.text = objective.GetProgress();  // テキスト (進捗) 更新

            if (objective.IsCompleted)
            {
                m_CompleteIcon.gameObject.SetActive(true);
                objective.OnProgress -= OnProgress; // リスナ解除
            }
        }

        void Update()
        {
            m_Time += Time.deltaTime;
            var moving = m_MoveCurve.Evaluate(m_Time);
            m_RectTransform.anchoredPosition = new Vector2((m_RectTransform.sizeDelta.x + s_Margin) * moving, m_RectTransform.anchoredPosition.y);  // スライドイン
        }
    }
}
