using UnityEngine;

namespace Unity.Game.Behaviours.Actions
{
    public abstract class RepeatableAction : Action
    {
        [Header("データ")]
        [SerializeField, Tooltip("リピート間隔")] protected float m_Pause = 0.25f;
        [SerializeField, Tooltip("リピートフラグ")] protected bool m_Repeat = true;

        protected virtual void OnValidate()
        {
            m_Pause = Mathf.Max(0.0f, m_Pause);
        }
    }
}
