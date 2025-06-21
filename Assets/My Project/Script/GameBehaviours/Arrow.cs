using UnityEngine;

namespace Unity.Game.Behaviours
{
    [RequireComponent(typeof(Rigidbody))]

    public class Arrow : MonoBehaviour
    {
        [Header("データ")]
        [SerializeField, Range(0.0f, 1080.0f), Tooltip("発射オブジェクト回転速度")] float m_RotationSpeed = 0.0f;

        public bool Deadly { get; private set; } = true;    // 即死フラグ
        bool m_Launched;    // 発射フラグ
        bool m_Rotate;  // 回転フラグ
        Vector3 m_Rotation; // 回転量
        Rigidbody m_RigidBody;
        CapsuleCollider m_Collider;
        ParticleSystem m_ParticleSystem;

        /// <summary>
        /// 発射オブジェクト詳細設定
        /// </summary>
        /// <param name="velocity"></param>
        /// <param name="useGravity"></param>
        /// <param name="time"></param>
        public void Init(float velocity, bool useGravity, float time)
        {
            m_RigidBody.linearVelocity = transform.forward * velocity;
            m_RigidBody.useGravity = useGravity;
            Destroy(gameObject, time);
        }

        void Awake()
        {
            m_Collider = GetComponent<CapsuleCollider>();
            m_Collider.enabled = false;
            m_RigidBody = GetComponent<Rigidbody>();
            m_RigidBody.isKinematic = false;
            m_RigidBody.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
            m_ParticleSystem = GetComponent<ParticleSystem>();
            m_ParticleSystem.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

            if (m_RotationSpeed > 0.0f)
            {
                m_Rotation = Random.onUnitSphere * m_RotationSpeed;
                m_Rotate = true;
            }
        }

        void Update()
        {
            m_Collider.enabled = true;

            if (!m_Launched)
            {
                var c0 = transform.TransformPoint(m_Collider.center - Vector3.forward * (m_Collider.height * 0.5f - m_Collider.radius));
                var c1 = transform.TransformPoint(m_Collider.center + Vector3.forward * (m_Collider.height * 0.5f - m_Collider.radius));
                var colliders = Physics.OverlapCapsule(c0, c1, m_Collider.radius, Physics.AllLayers, QueryTriggerInteraction.Ignore);
                var collisions = false; // 衝突フラグ

                foreach (var collider in colliders)
                {
                    if (collider != m_Collider)
                    {
                        collisions = true;
                        break;
                    }
                }

                if (!collisions)
                {
                    m_ParticleSystem.Play();
                    m_Launched = true;
                }
            }

            if (Deadly)
            {
                if (m_Rotate)
                {
                    transform.Rotate(m_Rotation * Time.deltaTime);
                }
                else
                {
                    transform.rotation = Quaternion.LookRotation(m_RigidBody.linearVelocity);   // 回転 (移動方向)
                }
            }
        }

        void OnCollisionEnter(Collision collision)
        {
            if (Deadly && collision.collider.gameObject.CompareTag("Player"))
            {
                GameOverEvent evt = Events.GameOverEvent;
                evt.Win = false;
                EventManager.Broadcast(evt);    // GameOverEvent ブロードキャスト
            }

            m_RigidBody.useGravity = true;
            Deadly = false;
        }
    }
}
