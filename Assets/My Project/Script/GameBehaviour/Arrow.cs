using UnityEngine;
using Unity.Game.UI;
using UnityEngine.Rendering;

namespace Unity.Game.Behaviours
{
    [RequireComponent(typeof(Rigidbody))]

    public class Arrow : MonoBehaviour
    {
        [Header("参照")]
        [SerializeField, Tooltip("丸影")] RoundShadow m_RoundShadow;

        [Header("データ")]
        [SerializeField, Range(0.0f, 1080.0f), Tooltip("発射オブジェクト回転速度")] float m_RotationSpeed = 0.0f;
        [SerializeField, Tooltip("地面探索最大距離")] float m_ShadowRayLength = 30.0f;

        public bool Deadly { get; private set; } = true;    // 即死フラグ
        bool m_DevUseRoundShadow;   // 丸影使用フラグ
        bool m_RoundShadowEnabled;  // 丸影表示フラグ
        bool m_Launched;    // 発射フラグ
        bool m_Rotate;  // 回転フラグ
        Vector3 m_Rotation; // 回転量
        MeshRenderer m_MeshRenderer;
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

        /// <summary>
        /// 影表示設定
        /// </summary>
        void SetupShadowMode()
        {
            if (m_DevUseRoundShadow)    // 丸影使用時
            {
                if (m_MeshRenderer)
                {
                    m_MeshRenderer.shadowCastingMode = ShadowCastingMode.Off;
                }
            }
            else   // 影使用時
            {
                if (m_MeshRenderer)
                {
                    m_MeshRenderer.shadowCastingMode = ShadowCastingMode.On;    // 全体設定 (QualitySettings.shadows) オフ → 影非描画, 全体設定 (QualitySettings.shadows) オン → 影描画
                }
            }
        }

        /// <summary>
        /// 丸影距離計測
        /// </summary>
        void UpdateRoundShadow()
        {
            if (!m_RoundShadow) return;

            if (!m_DevUseRoundShadow)
            {
                if (m_RoundShadow.gameObject.activeSelf)
                {
                    m_RoundShadow.gameObject.SetActive(false);
                }

                return;
            }

            if (!m_RoundShadowEnabled)
            {
                if (m_RoundShadow.gameObject.activeSelf)
                {
                    m_RoundShadow.gameObject.SetActive(false);
                }

                return;
            }

            if (!Deadly) return;

            Ray ray = new Ray(transform.position, Vector3.down);

            if (Physics.Raycast(ray, out RaycastHit hit, m_ShadowRayLength, Physics.AllLayers, QueryTriggerInteraction.Ignore))
            {
                float height = transform.position.y - hit.point.y;  // 距離 (矢 ~ 地面)
                m_RoundShadow.UpdateShadow(height, hit.point, hit.normal);  // 丸影更新
            }
            else
            {
                m_RoundShadow.gameObject.SetActive(false);
            }
        }

        void OnRoundShadowSetting(RoundShadowSettingEvent evt)
        {
            m_RoundShadowEnabled = evt.Active;

            if (m_RoundShadow && !m_RoundShadowEnabled)
            {
                m_RoundShadow.gameObject.SetActive(evt.Active);
            }
        }

        void Awake()
        {
            m_MeshRenderer = GetComponentInChildren<MeshRenderer>();
            m_DevUseRoundShadow = PlayerPrefs.GetInt("Dev_UseRoundShadow", 1) == 1;
            m_RoundShadowEnabled = PlayerPrefs.GetInt("RoundShadow", 1) == 1;
            EventManager.AddListener<RoundShadowSettingEvent>(OnRoundShadowSetting);
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

            SetupShadowMode();
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

            UpdateRoundShadow();
        }

        void OnCollisionEnter(Collision collision)
        {
            if (!collision.gameObject.CompareTag("Player") && !collision.gameObject.CompareTag("Ground")) return;

            if (Deadly && collision.collider.gameObject.CompareTag("Player"))
            {
                GameOverEvent evt = Events.GameOverEvent;
                evt.Win = false;
                evt.Fall = false;
                EventManager.Broadcast(evt);    // GameOverEvent ブロードキャスト
            }

            m_RigidBody.useGravity = true;
            Deadly = false;

            if (m_RoundShadow)
            {
                m_RoundShadow.gameObject.SetActive(false);
            }
        }

        void OnDestroy()
        {
            EventManager.RemoveListener<RoundShadowSettingEvent>(OnRoundShadowSetting);
        }
    }
}
