using UnityEngine;
using Unity.Game.Player;

namespace Unity.Game.Behaviours
{
    [RequireComponent(typeof(Rigidbody))]

    public class Arrow : MonoBehaviour
    {
        [SerializeField, Range(0.0f, 1080.0f), Tooltip("The rotation speed in degrees per second.")] float m_RotationSpeed = 0.0f;

        public bool Deadly { get; private set; } = true;
        Rigidbody m_RigidBody;
        CapsuleCollider m_Collider;
        ParticleSystem m_ParticleSystem;
        bool m_Rotate;
        Vector3 m_Rotation;
        bool m_Launched;

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
                var collisions = false;

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
                    transform.rotation = Quaternion.LookRotation(m_RigidBody.linearVelocity);
                }
            }
        }

        void OnCollisionEnter(Collision collision)
        {
            if (Deadly && collision.collider.gameObject.CompareTag("Player"))
            {
                var playerController = collision.collider.GetComponent<PlayerController>();

                //if (playerController)
                //{
                //    playerController.Explode();    // プレイヤ (Player Minifig) を爆発
                //}
                //else
                //{
                //    var brick = collision.collider.GetComponentInParent<Brick>();

                //    if (brick)
                //    {
                //        BrickExploder.ExplodeConnectedBricks(brick);    // ブロック群を爆発
                //    }
                //}

                GameOverEvent evt = Events.GameOverEvent;
                evt.Win = false;
                EventManager.Broadcast(evt);
            }

            m_RigidBody.useGravity = true;
            Deadly = false;
        }
    }
}
