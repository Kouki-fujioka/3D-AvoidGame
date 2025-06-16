using UnityEngine;
//using System.Collections.Generic;
using Unity.Game.Player;

namespace Unity.Game.Behaviour
{
    [RequireComponent(typeof(Rigidbody))]

    public class Bullet : MonoBehaviour
    {
        [
            SerializeField,
            Range(0.0f, 1080.0f),
            Tooltip("The rotation speed in degrees per second.")
        ]
        float m_RotationSpeed = 0.0f;

        public bool Deadly { get; private set; } = true;

        Rigidbody m_RigidBody;
        CapsuleCollider m_Collider;
        ParticleSystem m_ParticleSystem;
        bool m_Rotate;
        Vector3 m_Rotation;
        //List<Collider> m_IgnoredColliders;
        bool m_Launched;

        //public void Init(Collider collider, float velocity, bool useGravity, float time)
        //{
        //    m_RigidBody.linearVelocity = transform.forward * velocity;
        //    m_RigidBody.useGravity = useGravity;
        //    m_IgnoredColliders = new List<Collider>();
        //    Physics.IgnoreCollision(m_Collider, collider, true);
        //    m_IgnoredColliders.Add(collider);
        //    Destroy(gameObject, time);
        //}

        public void Init(float velocity, bool useGravity, float time)
        {
            m_RigidBody.linearVelocity = transform.forward * velocity;
            m_RigidBody.useGravity = useGravity;
            //m_IgnoredColliders = new List<Collider>();
            //m_IgnoredColliders.Add(collider);
            Destroy(gameObject, time);
        }

        void Awake()
        {
            m_Collider = GetComponent<CapsuleCollider>();
            m_Collider.enabled = false;
            m_RigidBody = GetComponent<Rigidbody>();
            m_RigidBody.isKinematic = false;
            m_RigidBody.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
            m_ParticleSystem = GetComponent<ParticleSystem>();  // ParticleSystem コンポーネントを取得
            m_ParticleSystem.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);   // パーティクル停止

            if (m_RotationSpeed > 0.0f) // 回転速度 > 0
            {
                // Random.onUnitSphere : 方向ベクトル (ランダム)
                m_Rotation = Random.onUnitSphere * m_RotationSpeed; // 回転速度 (ランダム)
                m_Rotate = true;    // プロジェクトタイル回転
            }
        }

        void Update()
        {
            m_Collider.enabled = true;  // CapsuleCollider コンポーネントを有効化

            // Check if the projectile has been launched out of the firing Shoot Action.
            if (!m_Launched)    // プロジェクトタイル未発射の場合
            {
                // Assumes that the capsule collider is aligned with local forward axis in projectile.
                var c0 = transform.TransformPoint(m_Collider.center - Vector3.forward * (m_Collider.height * 0.5f - m_Collider.radius));    // カプセルコライダのワールド座標 (片端)
                var c1 = transform.TransformPoint(m_Collider.center + Vector3.forward * (m_Collider.height * 0.5f - m_Collider.radius));    // カプセルコライダのワールド座標 (片端)
                /*
                 * Physics.OverlapCapsule(Vector3 point0, Vector3 point1, float radius, int layerMask, QueryTriggerInteraction queryTriggerInteraction)
                 * point0 : カプセルコライダのワールド座標 (片端)
                 * point1 : カプセルコライダのワールド座標 (片端)
                 * radius : カプセルコライダの半径
                 * layerMask : 判定対象のレイヤ (インスペクから設定可能)
                 * queryTriggerInteraction : 判定対象のコライダ (トリガ機能, 衝突機能)
                 * カプセルコライダに接触している全てのコライダを返却
                */
                var colliders = Physics.OverlapCapsule(c0, c1, m_Collider.radius, Physics.AllLayers, QueryTriggerInteraction.Ignore);   // プロジェクトタイルに接触している全てのコライダを格納 (トリガ型コライダ無視)
                var collisions = false; // 非接触

                foreach (var collider in colliders) // Collider コンポーネント (サブクラス対象) を順次代入
                {
                    // Do not collide with self, minifigs, the connected bricks of the firing Shoot Action or colliders from other LEGOBehaviourColliders.
                    if (collider != m_Collider && !collider.GetComponent<PlayerController>() && !collider.GetComponent<LEGOBehaviourCollider>()) // 対象コライダの場合
                    {
                        collisions = true;  // 接触
                        break;
                    }
                }

                // Play launch particle effect when projectile is no longer colliding with anything.
                if (!collisions)    // 非接触時
                {
                    m_ParticleSystem.Play();    // パーティクル再生
                    m_Launched = true;  // プロジェクトタイル発射
                }
            }

            if (Deadly) // 致命傷時
            {
                if (m_Rotate)   // プロジェクトタイル回転時
                {
                    transform.Rotate(m_Rotation * Time.deltaTime);  // プロジェクトタイルを回転
                }
                else   // プロジェクトタイル非回転時
                {
                    /*
                     * Quaternion 構造体 : 回転 (ローカル空間 or ワールド空間) を格納する構造体 (演算 (+, -, /) 無意味)
                     * 
                     * transform.rotation : Quaternion 型
                     */
                    transform.rotation = Quaternion.LookRotation(m_RigidBody.linearVelocity);   // プロジェクトタイルを回転 (移動方向)
                }
            }
        }

        /*
         * MonoBehaviour.OnCollisionEnter(Collision collision)
         * other : 相手の Collision コンポーネント
         * ゲームオブジェクト (両方) : Collider コンポーネント, 衝突機能必須
         * ゲームオブジェクト (片方) : Rigidbody コンポーネント必須
         * 物理衝突発生時に 1 回だけ実行されるメソッド
         */
        void OnCollisionEnter(Collision collision)
        {
            // Check if the player was hit.
            if (Deadly && collision.collider.gameObject.CompareTag("Player"))   // 接触したコライダ（ゲームオブジェクト）のタグが Player の場合
            {
                // If the player is a minifig or a brick, do an explosion.
                var minifigController = collision.collider.GetComponent<MinifigController>();   // MinifigController コンポーネントを取得

                if (minifigController)  // MinifigController コンポーネントが存在する場合
                {
                    minifigController.Explode();    // プレイヤ (Player Minifig) を爆発
                }
                else   // MinifigController コンポーネントが存在しない場合
                {
                    var brick = collision.collider.GetComponentInParent<Brick>();   // Brick コンポーネントを取得

                    if (brick)  // Brick コンポーネントが存在する場合
                    {
                        BrickExploder.ExplodeConnectedBricks(brick);    // ブロック群を爆発
                    }
                }

                GameOverEvent evt = Events.GameOverEvent;   // インスタンスを取得
                evt.Win = false;    // アンクリア
                EventManager.Broadcast(evt);    // イベント (GameOverEvent) をブロードキャスト (コールバックメソッド (リスナ) 実行)
            }

            // Turn on gravity and make non-deadly.
            m_RigidBody.useGravity = true;  // 重力適用
            Deadly = false; // 非致命傷
        }
    }
}
