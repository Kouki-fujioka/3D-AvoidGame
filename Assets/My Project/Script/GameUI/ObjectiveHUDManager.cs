using UnityEngine;
using UnityEngine.UI;

namespace Unity.Game.UI
{
    public class ObjectiveHUDManager : MonoBehaviour
    {
        [Header("参照")]
        [SerializeField, Tooltip("勝敗条件表示用パネル")] RectTransform m_ObjectivePanel = default;
        [SerializeField, Tooltip("勝利条件")] GameObject m_WinObjectivePrefab = default;
        [SerializeField, Tooltip("敗北条件")] GameObject m_LoseObjectivePrefab = default;

        const int s_TopMargin = 10; // マージン
        const int s_Space = 10; // スペース
        float m_NextY;  // y 座標

        void Awake()
        {
            EventManager.AddListener<ObjectiveAdded>(OnObjectiveAdded); // ObjectiveAdded ブロードキャスト時に OnObjectiveAdded 実行
            EventManager.AddListener<GameOverEvent>(OnGameOver);    // GameOverEvent ブロードキャスト時に OnGameOver 実行
        }

        void OnObjectiveAdded(ObjectiveAdded evt)
        {
            if(!evt.Objective.m_Hidden)
            {
                GameObject go = Instantiate(evt.Objective.m_Lose ? m_LoseObjectivePrefab : m_WinObjectivePrefab, m_ObjectivePanel); // クローン (敗北条件 or 勝利条件)
                Objective objective = go.GetComponent<Objective>();
                objective.Initialize(evt.Objective.m_Title, evt.Objective.m_Description, evt.Objective.GetProgress());
                LayoutRebuilder.ForceRebuildLayoutImmediate(objective.GetComponent<RectTransform>());
                RectTransform rectTransform = go.GetComponent<RectTransform>();
                rectTransform.anchoredPosition = new Vector2(0, m_NextY - s_TopMargin);
                m_NextY -= rectTransform.sizeDelta.y + s_Space;
                evt.Objective.OnProgress += objective.OnProgress;   // リスナ登録
            }
        }

        void OnGameOver(GameOverEvent evt)
        {
            EventManager.RemoveListener<ObjectiveAdded>(OnObjectiveAdded);  // OnObjectiveAdded 登録解除
            EventManager.RemoveListener<GameOverEvent>(OnGameOver); // OnGameOver 登録解除
        }

        void OnDestroy()
        {
            EventManager.RemoveListener<ObjectiveAdded>(OnObjectiveAdded);  // OnObjectiveAdded 登録解除
            EventManager.RemoveListener<GameOverEvent>(OnGameOver); // OnGameOver 登録解除
        }
    }
}
