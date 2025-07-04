using UnityEngine;

namespace Unity.Game.Behaviours.Actions
{
    [RequireComponent(typeof(Collider))]
    [RequireComponent(typeof(Rigidbody))]
    public class RotateAction : MovementAction
    {
        [Header("ƒf[ƒ^")]
        [SerializeField, Tooltip("Å‘å‰ñ“]Šp“x")] int m_maxAngle = 360;

        int m_Angle;
        Collider m_Collider;

        enum State
        {
            Rotating,   // ‰ñ“]’†
            WaitingToRotate // ‰ñ“]‘Ò‹@’†
        }

        State m_State;  // ‰ñ“]ó‘Ô
        float m_Offset; // —İÏ‰ñ“]—Ê

        protected void Reset()
        {
            m_Time = 5.0f;
            m_Pause = 0.0f;
        }

        protected override void OnValidate()
        {
            base.OnValidate();
            m_Angle = Random.Range(-m_maxAngle, m_maxAngle);
        }

        void Awake()
        {
            m_Collider = GetComponent<Collider>();
        }

        void FixedUpdate()
        {
            if (m_Active)
            {
                m_CurrentTime += Time.fixedDeltaTime;

                if (m_State == State.Rotating)
                {
                    var delta = Mathf.Clamp(m_Angle / m_Time * m_CurrentTime, Mathf.Min(-m_Angle, m_Angle), Mathf.Max(-m_Angle, m_Angle)) - m_Offset;   // ‰ñ“]—Ê
                    transform.Rotate(Vector3.up, delta, Space.World);
                    m_Offset += delta;

                    if (m_CurrentTime >= m_Time)
                    {
                        m_Offset = 0.0f;
                        m_CurrentTime -= m_Time;
                        m_State = State.WaitingToRotate;
                    }
                }

                if (m_State == State.WaitingToRotate)
                {
                    if (m_CurrentTime >= m_Pause)
                    {
                        m_CurrentTime -= m_Pause;
                        m_State = State.Rotating;
                        m_Active = m_Repeat;
                        m_Angle = Random.Range(-m_maxAngle, m_maxAngle);
                    }
                }
            }
        }

        void OnTriggerEnter(Collider other)
        {
            if (other.gameObject.CompareTag("Player"))
            {
                GameOverEvent evt = Events.GameOverEvent;
                evt.Win = false;
                evt.Fall = false;
                EventManager.Broadcast(evt);    // GameOverEvent ƒuƒ[ƒhƒLƒƒƒXƒg
            }
        }
    }
}
