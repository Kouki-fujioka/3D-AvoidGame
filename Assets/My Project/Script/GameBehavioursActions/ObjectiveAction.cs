using UnityEngine;
using System.Collections.Generic;
using Unity.Game.Behaviours.Triggers;

namespace Unity.Game.Behaviours.Actions
{
    public abstract class ObjectiveAction : Action
    {
        public abstract ObjectiveConfiguration GetDefaultObjectiveConfiguration(Trigger trigger);

        [Header("参照")]
        [SerializeField, Tooltip("勝敗条件 (タイトル, 説明等)")] List<ObjectiveConfiguration> m_ObjectiveConfigurations = new List<ObjectiveConfiguration>();
        [SerializeField, Tooltip("対象トリガ")] List<Trigger> m_Triggers = new List<Trigger>();

        public override void Activate()
        {
            PlayAudio(spatial: false, destroyWithAction: false);
            base.Activate();
            m_Active = false;
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

        /// <summary>
        /// 勝敗条件 (タイトル, 説明等) を設定
        /// </summary>
        /// <param name="trigger"></param>
        /// <param name="title"></param>
        /// <param name="description"></param>
        /// <param name="progressType"></param>
        /// <param name="lose"></param>
        /// <param name="hidden"></param>
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
