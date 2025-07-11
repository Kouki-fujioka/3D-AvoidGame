using UnityEngine;

namespace Unity.Game.Behaviours.Triggers
{
    public class TimerTrigger : Trigger
    {
        [Header("データ")]
        [SerializeField, Tooltip("トリガ起動時間 (制限時間)")] float m_Time = 10.0f;

        float m_CurrentTime;    // 現経過時間
        int m_PreviousProgress; // 旧経過時間

        void OnValidate()
        {
            m_Time = Mathf.Max(0.0f, m_Time);
        }

        void Start()
        {
            Goal = Mathf.FloorToInt(m_Time);
        }

        void Update()
        {
            m_CurrentTime += Time.deltaTime;

            if (!m_AlreadyTriggered)
            {
                Progress = Mathf.FloorToInt(m_CurrentTime);
            }

            if (m_CurrentTime >= m_Time)
            {
                ConditionMet();
                m_CurrentTime -= m_Time;
            }
            else
            {
                if (m_PreviousProgress != Progress)
                {
                    OnProgress?.Invoke();   // テキスト (進捗), フラグ (m_UpdateStatus) 更新
                }
            }

            m_PreviousProgress = Progress;
        }
    }
}
