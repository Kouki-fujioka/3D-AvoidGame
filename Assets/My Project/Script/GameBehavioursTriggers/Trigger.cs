using UnityEngine;
using System.Collections.Generic;
using Unity.Game.Behaviours.Actions;

namespace Unity.Game.Behaviours.Triggers
{
    public abstract class Trigger : MonoBehaviour
    {
        public System.Action OnProgress;
        public System.Action OnActivate;

        public int Progress;
        public int Goal;

        [SerializeField, Tooltip("The list of actions to trigger."), NonReorderable] protected List<Action> m_SpecificTargetActions = new List<Action>();
        [SerializeField, Tooltip("Trigger continuously.")] protected bool m_Repeat = true;

        protected HashSet<Action> m_TargetedActions = new HashSet<Action>();
        protected bool m_AlreadyTriggered;

        protected virtual void Awake()
        {
            m_TargetedActions = GetTargetedActions();
        }

        public HashSet<Action> GetTargetedActions()
        {
            var result = new HashSet<Action>();
            result.UnionWith(m_SpecificTargetActions);
            return result;
        }

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

                OnActivate?.Invoke();
                m_AlreadyTriggered = true;
            }
        }
    }
}
