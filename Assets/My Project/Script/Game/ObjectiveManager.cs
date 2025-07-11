using System.Collections.Generic;
using UnityEngine;

namespace Unity.Game
{
    public class ObjectiveManager : MonoBehaviour
    {
        List<IObjective> m_Objectives;  // 勝敗条件 (タイトル, 説明, 進捗等) リスト
        bool m_UpdateStatus;    // 勝敗状況更新フラグ
        bool m_Won; // 勝利フラグ
        bool m_Lost;    // 敗北フラグ

        protected void Awake()
        {
            m_Objectives = new List<IObjective>();
            EventManager.AddListener<ObjectiveAdded>(OnObjectiveAdded); // ObjectiveAdded ブロードキャスト時に OnObjectiveAdded 実行
        }

        void OnObjectiveAdded(ObjectiveAdded evt)
        {
            m_Objectives.Add(evt.Objective);    // 勝敗条件 (タイトル, 説明, 進捗等) を追加
            evt.Objective.OnProgress += OnProgress;
            m_UpdateStatus = true;
            m_Won = false;
            m_Lost = false;
        }

        /// <summary>
        /// フラグ (m_UpdateStatus) 更新
        /// </summary>
        /// <param name="_"></param>
        public void OnProgress(IObjective _)
        {
            m_UpdateStatus = true;
        }

        /// <summary>
        /// 勝敗状況更新
        /// </summary>
        void UpdateGameStatus()
        {
            m_Won = m_Objectives.Exists(objective => !objective.m_Lose);    // 勝利条件有 → true

            foreach (IObjective objective in m_Objectives)
            {
                // | m_Won (更新前) | IsCompleted | m_Lose | m_Won (更新後) |
                // ----------------------------------------------------------
                // |      true      |    true     |  true  |      true      |   敗北条件達成 → 勝利条件無関係 → true (維持)
                // |      true      |    true     |  false |      true      |   勝利条件達成 → true
                // |      true      |    false    |  true  |      true      |   敗北条件未達成 → 勝利条件無関係 → true (維持)
                // |      true      |    false    |  false |      false     |   勝利条件未達成 → false
                // |      false     |    true     |  true  |      false     |   勝利条件無 → false
                // |      false     |    true     |  false |      false     |   勝利条件無 → false
                // |      false     |    false    |  true  |      false     |   勝利条件無 → false
                // |      false     |    false    |  false |      false     |   勝利条件無 → false
                m_Won &= (objective.IsCompleted || objective.m_Lose);
                // | m_Lost (更新前) | IsCompleted | m_Lose | m_Lost (更新後) |
                // ------------------------------------------------------------
                // |      false      |    true     |  true  |      true       | 敗北条件達成 → true
                // |      false      |    true     |  false |      false      | 勝利条件達成 → 敗北条件無関係 → false (維持)
                // |      false      |    false    |  true  |      false      | 敗北条件未達成 → false
                // |      false      |    false    |  false |      false      | 勝利条件未達成 → 敗北条件無関係 → false (維持)
                m_Lost |= (objective.IsCompleted && objective.m_Lose);
            }

            m_UpdateStatus = false;
        }

        void Update()
        {
            if (m_Won || m_Lost)    // ゲーム終了
            {
                Events.GameOverEvent.Win = m_Won || !m_Lost;
                Events.GameOverEvent.Fall = false;
                EventManager.Broadcast(Events.GameOverEvent);   // GameOverEvent ブロードキャスト
                Destroy(this);
            }

            if (m_UpdateStatus)
            {
                UpdateGameStatus();
            }
        }

        void OnDestroy()
        {
            EventManager.RemoveListener<ObjectiveAdded>(OnObjectiveAdded);  // OnObjectiveAdded 登録解除
        }
    }
}
