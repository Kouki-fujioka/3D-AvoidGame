using UnityEngine;

namespace Unity.LEGO.UI
{
    public class UIAnimator : MonoBehaviour
    {
        [Header("参照")]
        [SerializeField, Tooltip("条件達成アイコン表示用")] CanvasGroup m_CanvasGroup = default;
        [SerializeField, Tooltip("条件達成アイコン表示用")] AnimationCurve m_AlphaCurve = default;
        [SerializeField, Tooltip("条件達成アイコンスケール用")] RectTransform m_RectTransform = default;
        [SerializeField, Tooltip("条件達成アイコンスケール用")] AnimationCurve m_ScaleCurve = default;

        [Header("データ")]
        [SerializeField, Tooltip("スケール開始ディレイ")] float m_AnimationDelay = 0.0f;

        float m_Time;   // 経過時間

        void Update()
        {
            m_Time += Time.deltaTime;
            m_CanvasGroup.alpha = m_AlphaCurve.Evaluate(m_Time - m_AnimationDelay);
            var scale = m_ScaleCurve.Evaluate(m_Time - m_AnimationDelay);
            m_RectTransform.localScale = new Vector3(scale, scale, 1.0f);
        }
    }
}
