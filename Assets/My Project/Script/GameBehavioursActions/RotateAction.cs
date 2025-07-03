using Unity.Game.Player;
using UnityEngine;

namespace Unity.Game.Behaviours.Actions
{
    [RequireComponent(typeof(Collider))]
    [RequireComponent(typeof(Rigidbody))]
    public class RotateAction : MovementAction
    {
        [Header("ÉfÅ[É^")]
        [SerializeField, Tooltip("ç≈ëÂâÒì]äpìx")] int m_maxAngle = 360;

        int m_Angle;
        Collider m_Collider;

        enum State
        {
            Rotating,   // âÒì]íÜ
            WaitingToRotate // âÒì]ë“ã@íÜ
        }

        State m_State;  // âÒì]èÛë‘
        float m_Offset; // ó›êœâÒì]ó 

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
                    var delta = Mathf.Clamp(m_Angle / m_Time * m_CurrentTime, Mathf.Min(-m_Angle, m_Angle), Mathf.Max(-m_Angle, m_Angle)) - m_Offset;   // âÒì]ó 
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

        void OnCollisionEnter(Collision collision)
        {
            if (collision.gameObject.CompareTag("Player"))
            {
                var player = collision.gameObject.GetComponent<PlayerController>();

                if (player)
                {
                    var direction = (collision.transform.position - transform.position).normalized;
                    //player.Knockback(direction, 5.0f);
                }
            }
        }

        //public void Knockback(Vector3 direction, float force)
        //{
        //    direction.y = 0.5f;  // è≠ÇµïÇÇ©ÇπÇÈ
        //    velocity = direction.normalized * force;
        //}
    }
}
