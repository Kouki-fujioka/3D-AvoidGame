using UnityEngine;
using UnityEngine.UI;

namespace Unity.Game.UI
{
    [RequireComponent(typeof(RectTransform))]

    public class Objective : MonoBehaviour
    {
        [Header("参照")]
        [SerializeField, Tooltip("条件 (勝利, 敗北) タイトル")] TMPro.TextMeshProUGUI m_Title = default;
        [SerializeField, Tooltip("条件 (勝利, 敗北) 説明")] TMPro.TextMeshProUGUI m_Description = default;
        [SerializeField, Tooltip("条件 (勝利, 敗北) 進捗")] TMPro.TextMeshProUGUI m_Progress = default;
        [SerializeField, Tooltip("条件 (勝利, 敗北) 達成アイコン")] Image m_CompleteIcon = default;
        [SerializeField, Tooltip("アニメーションカーブ")] AnimationCurve m_MoveCurve = default;

        float m_Time;
        const int s_Margin = 25;
        const int s_Space = 4;
        RectTransform m_RectTransform;

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
            m_Progress.text = objective.GetProgress();

            if (objective.IsCompleted)  // 条件 (勝利, 敗北) 達成時
            {
                m_CompleteIcon.gameObject.SetActive(true);  // アイコン表示
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
