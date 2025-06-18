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

        [SerializeField, Tooltip("Trigger actions on connected bricks.\nor\nTrigger a list of specific actions.")] protected Action m_Target;
        [SerializeField, Tooltip("The list of actions to trigger."), NonReorderable] protected List<Action> m_SpecificTargetActions = new List<Action>();
        [SerializeField, Tooltip("Trigger continuously.")] protected bool m_Repeat = true;

        protected bool m_AlreadyTriggered;

        protected void ConditionMet()
        {
            if (m_Repeat || !m_AlreadyTriggered)
            {
                List<Action> winAndLoseActions = new List<Action>();

                if (m_Target)
                {
                    m_Target.Activate();

                    if (m_Target is ObjectiveAction)
                    {
                        winAndLoseActions.Add(m_Target);
                    }
                }

                OnActivate?.Invoke();
                m_AlreadyTriggered = true;
            }
        }
    }
}
