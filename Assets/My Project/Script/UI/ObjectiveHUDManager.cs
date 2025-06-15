using UnityEngine;
using UnityEngine.UI;

namespace Unity.Game.UI
{
    public class ObjectiveHUDManager : MonoBehaviour
    {
        [Header("参照")]
        [SerializeField, Tooltip("条件 (勝利, 敗北) 表示用パネル")] RectTransform m_ObjectivePanel = default;
        [SerializeField, Tooltip("勝利条件")] GameObject m_WinObjectivePrefab = default;
        [SerializeField, Tooltip("敗北条件")] GameObject m_LoseObjectivePrefab = default;

        const int s_TopMargin = 10;
        const int s_Space = 10;
        float m_NextY;

        void Awake()
        {
            EventManager.AddListener<ObjectiveAdded>(OnObjectiveAdded); // リスナ追加
            EventManager.AddListener<GameOverEvent>(OnGameOver);    // リスナ追加
        }

        void OnObjectiveAdded(ObjectiveAdded evt)   // コールバックメソッド
        {
            if(!evt.Objective.m_Hidden) // 表示時
            {
                GameObject go = Instantiate(evt.Objective.m_Lose ? m_LoseObjectivePrefab : m_WinObjectivePrefab, m_ObjectivePanel); // クローン (敗北条件 or 勝利条件)
                Objective objective = go.GetComponent<Objective>();
                objective.Initialize(evt.Objective.m_Title, evt.Objective.m_Description, evt.Objective.GetProgress());
                LayoutRebuilder.ForceRebuildLayoutImmediate(objective.GetComponent<RectTransform>());
                RectTransform rectTransform = go.GetComponent<RectTransform>();
                rectTransform.anchoredPosition = new Vector2(0, m_NextY - s_TopMargin);
                m_NextY -= rectTransform.sizeDelta.y + s_Space;
                evt.Objective.OnProgress += objective.OnProgress;
            }
        }

        void OnGameOver(GameOverEvent evt)  // コールバックメソッド
        {
            EventManager.RemoveListener<ObjectiveAdded>(OnObjectiveAdded);  // リスナ解除
            EventManager.RemoveListener<GameOverEvent>(OnGameOver); // リスナ解除
        }

        void OnDestroy()
        {
            EventManager.RemoveListener<ObjectiveAdded>(OnObjectiveAdded);  // リスナ解除
            EventManager.RemoveListener<GameOverEvent>(OnGameOver); // リスナ解除
        }
    }
}
