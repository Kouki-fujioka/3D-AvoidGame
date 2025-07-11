using UnityEngine;

namespace Unity.Game.Behaviours.Actions
{
    public abstract class MovementAction : RepeatableAction
    {
        [Header("データ")]
        [SerializeField, Tooltip("アクション完了時間")] protected float m_Time = 5.0f;
        [SerializeField, Tooltip("衝突時停止フラグ")] protected bool m_Collide = true;

        protected float m_CurrentTime;  // 経過時間

        protected override void OnValidate()
        {
            base.OnValidate();
            m_Time = Mathf.Max(0.0f, m_Time);
        }

        protected override void Start()
        {
            base.Start();   // フラグ (m_Active) を更新
            transform.localRotation = Quaternion.Euler(Vector3Int.RoundToInt(transform.localRotation.eulerAngles));
        }
    }
}
