using System.Collections.Generic;
using UnityEngine;

namespace Unity.Game.Behaviour.Action
{
    public abstract class ObjectiveAction : Action
    {
        public abstract ObjectiveConfiguration GetDefaultObjectiveConfiguration(Trigger trigger);

        [SerializeField]
        List<ObjectiveConfiguration> m_ObjectiveConfigurations = new List<ObjectiveConfiguration>();

        [SerializeField]
        List<Trigger> m_Triggers = new List<Trigger>();

        public override void Activate()
        {
            PlayAudio(spatial: false, destroyWithAction: false);
            base.Activate();
            m_Active = false;   // “®ì”ñŽÀs
        }

        protected override void Start()
        {
            base.Start();
            ObjectiveConfiguration objectiveConfiguration;
            var targetingTriggers = GetTargetingTriggers();

            if (targetingTriggers.Count == 0)
            {
                objectiveConfiguration = GetDefaultObjectiveConfiguration(null);
                AddObjective(null, objectiveConfiguration.Title, objectiveConfiguration.Description, objectiveConfiguration.ProgressType, objectiveConfiguration.Lose, objectiveConfiguration.Hidden);
            }

            foreach (var trigger in targetingTriggers)
            {
                var triggerIndex = m_Triggers.IndexOf(trigger);

                if (triggerIndex >= 0)
                {
                    objectiveConfiguration = m_ObjectiveConfigurations[triggerIndex];
                }
                else
                {
                    objectiveConfiguration = GetDefaultObjectiveConfiguration(trigger);
                }

                AddObjective(trigger, objectiveConfiguration.Title, objectiveConfiguration.Description, objectiveConfiguration.ProgressType, objectiveConfiguration.Lose, objectiveConfiguration.Hidden);
            }
        }

        void AddObjective(Trigger trigger, string title, string description, ObjectiveProgressType progressType, bool lose, bool hidden)
        {
            var objective = gameObject.AddComponent<Objective>();
            objective.m_Trigger = trigger;
            objective.m_Title = title;
            objective.m_Description = description;
            objective.m_ProgressType = progressType;
            objective.m_Lose = lose;
            objective.m_Hidden = hidden;
        }
    }
}
