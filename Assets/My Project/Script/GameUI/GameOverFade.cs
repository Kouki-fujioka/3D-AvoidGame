using UnityEngine;

namespace Unity.Game.UI
{
    public class GameOverFade : MonoBehaviour
    {
        [Header("参照")]
        [SerializeField, Tooltip("キャンバスグループ (ゲーム終了後に画面をフェードアウト)")] CanvasGroup m_CanvasGroup = default;

        [Header("データ")]
        [SerializeField, Tooltip("勝利してからフェードアウト開始までの時間")] float m_WinDelay = 4.0f;
        [SerializeField, Tooltip("敗北してからフェードアウト開始までの時間")] float m_LoseDelay = 2.0f;
        [SerializeField, Tooltip("フェードアウト完了までの時間")] float m_Duration = 1.0f;

        float m_Time;
        bool m_GameOver;
        bool m_Won;

        void Start()
        {
            EventManager.AddListener<GameOverEvent>(OnGameOver);    // GameOverEvent ブロードキャスト時に OnGameOver 実行
        }

        void Update()
        {
            if (m_GameOver)
            {
                m_Time += Time.deltaTime;

                if (m_Won)
                {
                    m_CanvasGroup.alpha = Mathf.Clamp01((m_Time - m_WinDelay) / m_Duration);
                }
                else
                {
                    m_CanvasGroup.alpha = Mathf.Clamp01((m_Time - m_LoseDelay) / m_Duration);
                }
            }
        }

        void OnGameOver(GameOverEvent evt)
        {
            if (!m_GameOver)
            {
                m_CanvasGroup.gameObject.SetActive(true);
                m_GameOver = true;
                m_Won = evt.Win;
            }
        }

        void OnDestroy()
        {
            EventManager.RemoveListener<GameOverEvent>(OnGameOver); // OnGameOver 登録解除
        }
    }
}
