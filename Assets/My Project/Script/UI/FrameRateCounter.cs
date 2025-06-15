using UnityEngine;
using TMPro;

namespace Unity.Game.UI
{
    public class FrameRateCounter : MonoBehaviour
    {
        [Header("参照")]
        [SerializeField, Tooltip("FPS テキスト")] TextMeshProUGUI m_TextMeshProUGUI = default;

        [Header("データ")]
        [SerializeField, Tooltip("FPS 計測時間")] float m_PollingTime = 0.5f;

        public bool IsActive => m_TextMeshProUGUI.gameObject.activeSelf;    // アクティブ状態を格納
        float m_Time;   // 経過時間
        int m_FrameCount;   // フレーム数

        public void Show(bool show)
        {
            m_TextMeshProUGUI.gameObject.SetActive(show);   // アクティブ状態変更
        }

        void Update()
        {
            m_Time += Time.deltaTime;
            m_FrameCount++;

            if(m_Time >= m_PollingTime) // FPS 計測時間経過後
            {
                // テキスト (FPS) 設定
                int FPS = Mathf.RoundToInt(m_FrameCount / m_Time);
                m_TextMeshProUGUI.text = "FPS : " + FPS.ToString();

                // リセット
                m_Time -= m_PollingTime;
                m_FrameCount = 0;
            }
        }
    }
}
