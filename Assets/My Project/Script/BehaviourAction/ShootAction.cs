using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace Unity.Game.Behaviour.Action
{
    public class ShootAction : RepeatableAction
    {
        [
            SerializeField,
            Tooltip("The projectile to launch.")
        ]
        GameObject m_Bullet = null;

        [SerializeField]
        Transform m_ShootPoint = null;

        [
            SerializeField,
            Range(1, 100),
            Tooltip("The velocity of the projectiles.")
        ]
        float m_Velocity = 25f;

        [
            SerializeField,
            Range(0, 100),
            Tooltip("The accuracy in percent.")
        ]
        int m_Accuracy = 90;

        [
            SerializeField,
            Tooltip("The time in seconds before projectiles disappears.")
        ]
        float m_Lifetime = 2f;

        [
            SerializeField,
            Tooltip("Projectiles are affected by gravity.")
        ]
        bool m_UseGravity = true;

        float m_Time;
        bool m_HasFired;

        protected override void OnValidate()
        {
            base.OnValidate();
            m_Lifetime = Mathf.Max(1.0f, m_Lifetime);   // プロジェクトタイル消滅時間
            m_Pause = Mathf.Max(0.25f, m_Pause);    // リピート間隔を調整
        }

        protected void Update()
        {
            if (m_Active)   // 動作実行時
            {
                m_Time += Time.deltaTime;   // 前フレームからの経過時間 (秒) を加算

                if (!m_HasFired)    // プロジェクトタイル発射可能時
                {
                    Fire(); // プロジェクトタイル発射
                    m_HasFired = true;  // プロジェクトタイル発射不可能
                }

                if (m_Time >= m_Pause) // リピート間隔経過後
                {
                    m_Time -= m_Pause;  // 経過時間をリセット
                    m_HasFired = false; // プロジェクトタイル発射可能
                    m_Active = m_Repeat;    // 動作実行
                }
            }
        }

        void Fire()
        {
            if (m_Bullet)
            {
                var accuracyToDegrees = 90.0f - 90.0f * m_Accuracy / 100.0f;    // 拡散角度を計算
                var randomSpread = Random.insideUnitCircle * Mathf.Tan(accuracyToDegrees * Mathf.Deg2Rad * 0.5f);   // ？
                var projectilePosition = m_ShootPoint.position; // 発射地点を格納
                var projectileRotation = m_ShootPoint.rotation; // 発射角度を格納
                var go = Instantiate(m_Bullet, projectilePosition, projectileRotation);
                var projectile = go.GetComponent<Bullet>();

                if (projectile)
                {
                    projectile.Init(m_Velocity, m_UseGravity, m_Lifetime);
                }

                PlayAudio();
            }
        }
    }
}
