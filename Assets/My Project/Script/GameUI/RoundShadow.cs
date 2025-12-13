using UnityEngine;

namespace Unity.Game.UI
{
    public class RoundShadow : MonoBehaviour
    {
        [SerializeField, Tooltip("丸影最小サイズ")] float m_MinScale = 0.1f;
        [SerializeField, Tooltip("丸影最大サイズ")] float m_MaxScale = 1.5f;
        [SerializeField, Tooltip("丸影サイズ変更開始距離")] float m_MaxHeight = 30.0f;

        bool m_Visible; // 丸影表示フラグ

        /// <summary>
        /// 丸影更新
        /// </summary>
        /// <param name="height"></param>
        /// <param name="hitPos"></param>
        /// <param name="normal"></param>
        public void UpdateShadow(float height, Vector3 hitPos, Vector3 normal)
        {
            if (!m_Visible)
            {
                gameObject.SetActive(false);
                return;
            }

            gameObject.SetActive(true);
            transform.position = hitPos + normal * 0.01f;
            transform.rotation = Quaternion.FromToRotation(Vector3.up, normal);
            float t = Mathf.Clamp01(1f - height / m_MaxHeight);
            float scale = Mathf.Lerp(m_MinScale, m_MaxScale, t);
            transform.localScale = Vector3.one * scale;
        }

        void OnSettingChanged(RoundShadowSettingEvent evt)
        {
            m_Visible = evt.Active;
        }

        void Awake()
        {
            m_Visible = PlayerPrefs.GetInt("RoundShadow", 1) == 1;
            EventManager.AddListener<RoundShadowSettingEvent>(OnSettingChanged);
        }

        void OnDestroy()
        {
            EventManager.RemoveListener<RoundShadowSettingEvent>(OnSettingChanged);
        }
    }
}
