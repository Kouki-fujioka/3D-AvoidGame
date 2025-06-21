using UnityEngine;
using System.Collections.Generic;
using Unity.Game.Behaviours.Actions;

namespace Unity.Game.Behaviours.Triggers
{
    public abstract class Trigger : MonoBehaviour
    {
        [Header("参照")]
        [SerializeField, Tooltip("トリガ対象アクション"), NonReorderable] protected List<Action> m_SpecificTargetActions = new List<Action>();

        [Header("データ")]
        [SerializeField, Tooltip("トリガリピートフラグ")] protected bool m_Repeat = true;

        public System.Action OnProgress;    // デリゲート
        public System.Action OnActivate;    // デリゲート
        public int Progress;    // 経過時間 (進捗)
        public int Goal;    // トリガ起動時間 (制限時間)
        protected HashSet<Action> m_TargetedActions = new HashSet<Action>();    // トリガ対象アクション
        protected bool m_AlreadyTriggered;  // トリガ起動フラグ

        protected virtual void Awake()
        {
            m_TargetedActions = GetTargetedActions();
        }

        /// <summary>
        /// トリガ対象アクションを返却
        /// </summary>
        /// <returns></returns>
        public HashSet<Action> GetTargetedActions()
        {
            var result = new HashSet<Action>();
            result.UnionWith(m_SpecificTargetActions);
            return result;
        }

        /// <summary>
        /// トリガ起動
        /// </summary>
        protected void ConditionMet()
        {
            if (m_Repeat || !m_AlreadyTriggered)
            {
                List<Action> winAndLoseActions = new List<Action>();

                foreach (var action in m_TargetedActions)
                {
                    if (action)
                    {
                        action.Activate();

                        if (action is ObjectiveAction)
                        {
                            winAndLoseActions.Add(action);
                        }
                    }
                }

                foreach (var action in winAndLoseActions)
                {
                    m_TargetedActions.Remove(action);
                }

                OnActivate?.Invoke();   // テキスト (進捗), フラグ (IsCompleted, m_UpdateStatus) 更新
                m_AlreadyTriggered = true;
            }
        }
    }
}
