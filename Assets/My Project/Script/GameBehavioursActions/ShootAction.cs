using UnityEngine;

namespace Unity.Game.Behaviours.Actions
{
    public class ShootAction : RepeatableAction
    {
        [Header("参照")]
        [SerializeField, Tooltip("発射オブジェクト")] GameObject m_Arrow = null;
        [SerializeField, Tooltip("発射ポジション")] Transform m_ShootPoint = null;

        [Header("データ")]
        [SerializeField, Range(1, 100), Tooltip("発射速度")] float m_Velocity = 25f;
        [SerializeField, Range(0, 100), Tooltip("The accuracy in percent.")] int m_Accuracy = 65;
        [SerializeField, Tooltip("発射オブジェクト消滅時間")] float m_Lifetime = 8f;
        [SerializeField, Tooltip("発射オブジェクト重力付与フラグ")] bool m_UseGravity = true;

        float m_Time;   // 経過時間
        bool m_HasFired;    // 発射フラグ

        protected override void OnValidate()
        {
            base.OnValidate();
            m_Lifetime = Mathf.Max(1.0f, m_Lifetime);
            m_Pause = Mathf.Max(0.25f, m_Pause);
        }

        protected void Awake()
        {
            EventManager.AddListener<GameOverEvent>(OnGameOver);    // GameOverEvent ブロードキャスト時に OnGameOver 実行
        }

        protected void Update()
        {
            if (m_Active)
            {
                m_Time += Time.deltaTime;

                if (!m_HasFired)
                {
                    Fire();
                    m_HasFired = true;
                }

                if (m_Time >= m_Pause)
                {
                    m_Time -= m_Pause;
                    m_HasFired = false;
                    m_Active = m_Repeat;
                }
            }
        }

        /// <summary>
        /// オブジェクト発射
        /// </summary>
        void Fire()
        {
            if (m_Arrow)
            {
                var accuracyToDegrees = 90.0f - 90.0f * m_Accuracy / 100.0f;    // 拡散角度
                var randomSpread = Random.insideUnitCircle * Mathf.Tan(accuracyToDegrees * Mathf.Deg2Rad * 0.5f);   // ？
                var projectilePosition = m_ShootPoint.position; // 発射地点
                var projectileRotation = m_ShootPoint.rotation * Quaternion.LookRotation(Vector3.forward + Vector3.right * randomSpread.x + Vector3.up * randomSpread.y);   // 発射角度
                var go = Instantiate(m_Arrow, projectilePosition, projectileRotation);
                var arrow = go.GetComponent<Arrow>();

                if (arrow)
                {
                    arrow.Init(m_Velocity, m_UseGravity, m_Lifetime);
                }

                PlayAudio();
            }
        }

        void OnGameOver(GameOverEvent evt)
        {
            m_Repeat = false;
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            EventManager.RemoveListener<GameOverEvent>(OnGameOver); // OnGameOver 登録解除
        }
    }
}
