using UnityEngine;
using Cinemachine;

namespace Unity.Game
{
    public class InputManager : MonoBehaviour
    {
        [Header("データ")]
        [SerializeField, Tooltip("基準値 (上下視点移動速度)")] float m_VerticalLookMinSensitivity = 0.5f;
        [SerializeField, Range(0.25f, 1.0f), Tooltip("ステップ幅 (上下視点移動速度)")] float m_VerticalLookSensitivityStep = 1.0f;
        [SerializeField, Tooltip("基準値 (左右視点移動速度)")] float m_HorizontalLookMinimumSensitivity = 50.0f;
        [SerializeField, Range(10.0f, 100.0f), Tooltip("ステップ幅 (左右視点移動速度)")] float m_HorizontalLookSensitivityStep = 100.0f;

        CinemachineFreeLook m_FreeLookCamera;

        void Awake()
        {
            m_FreeLookCamera = FindFirstObjectByType<CinemachineFreeLook>();
            EventManager.AddListener<LookSensitivityUpdateEvent>(OnLookSensitivityUpdate);  // LookSensitivityUpdateEvent ブロードキャスト時に OnLookSensitivityUpdate 実行
        }

        void OnLookSensitivityUpdate(LookSensitivityUpdateEvent evt)
        {
            if (m_FreeLookCamera)
            {
                // 視点移動速度設定
                m_FreeLookCamera.m_XAxis.m_MaxSpeed = m_HorizontalLookMinimumSensitivity + (m_HorizontalLookSensitivityStep * evt.Value);
                m_FreeLookCamera.m_YAxis.m_MaxSpeed = m_VerticalLookMinSensitivity + (m_VerticalLookSensitivityStep * evt.Value);
            }
        }

        void OnDestroy()
        {
            EventManager.RemoveListener<LookSensitivityUpdateEvent>(OnLookSensitivityUpdate);   // OnLookSensitivityUpdate 登録解除
        }
    }
}
