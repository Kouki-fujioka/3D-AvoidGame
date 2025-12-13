using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Unity.Game.UI
{
    public class MenuManager : MonoBehaviour
    {
        [Header("参照")]
        [SerializeField, Tooltip("チャットマネージャー")] ChatManager m_ChatManager;
        [SerializeField, Tooltip("メニュー画面")] GameObject m_Menu = default;
        [SerializeField, Tooltip("操作説明画面")] GameObject m_Controls = default;
        [SerializeField, Tooltip("丸影用トグル")] Toggle m_ShadowToggle = default;
        [SerializeField, Tooltip("FPS 用トグル")] Toggle m_FrameRateCounterToggle = default;
        [SerializeField, Tooltip("視点移動速度用スライダ")] Slider m_Sensitivity = default;

        [Header("データ")]
        [SerializeField, Tooltip("丸影使用フラグ")] bool m_UseRoundShadow = true;

        FrameRateCounter m_FrameRateCounter;
        public bool IsActive => m_Menu.activeSelf;  // アクティブ状態

        void Start()
        {
            PlayerPrefs.SetInt("Dev_UseRoundShadow", m_UseRoundShadow ? 1 : 0);
            m_FrameRateCounter = FindFirstObjectByType<FrameRateCounter>();

            if (m_FrameRateCounter == null)
            {
                Debug.LogError("FrameRateCounter is null");
            }

            m_FrameRateCounterToggle.SetIsOnWithoutNotify(m_FrameRateCounter.IsActive);
            m_FrameRateCounterToggle.onValueChanged.AddListener(OnFrameRateCounterChanged);

            if (m_UseRoundShadow)
            {
                bool isRoundShadowActive = PlayerPrefs.GetInt("RoundShadow", 1) == 1;
                QualitySettings.shadows = ShadowQuality.Disable;    // 影無効
                m_ShadowToggle.SetIsOnWithoutNotify(isRoundShadowActive);
                m_ShadowToggle.onValueChanged.AddListener(OnRoundShadowChanged);
                RoundShadowSettingEvent roundShadowSettingEvent = Events.RoundShadowSettingEvent;
                roundShadowSettingEvent.Active = isRoundShadowActive;
                EventManager.Broadcast(roundShadowSettingEvent);
            }
            else
            {
                RoundShadowSettingEvent roundShadowSettingEvent = Events.RoundShadowSettingEvent;
                roundShadowSettingEvent.Active = false;
                EventManager.Broadcast(roundShadowSettingEvent);
                bool isNormalShadowActive = PlayerPrefs.GetInt("NormalShadow", 1) == 1;
                QualitySettings.shadows = isNormalShadowActive ? ShadowQuality.All : ShadowQuality.Disable;
                m_ShadowToggle.SetIsOnWithoutNotify(isNormalShadowActive);
                m_ShadowToggle.onValueChanged.AddListener(OnNormalShadowChanged);
            }

            var defaultSensitivity = PlayerPrefs.GetFloat("Sensitivity", 5.0f);
            m_Sensitivity.SetValueWithoutNotify(defaultSensitivity);
            m_Sensitivity.onValueChanged.AddListener(OnSensitivityChanged);
            OnSensitivityChanged(defaultSensitivity);
            m_Menu.SetActive(false);
        }

        void OnFrameRateCounterChanged(bool value)
        {
            m_FrameRateCounter.Show(value);
        }

        void OnNormalShadowChanged(bool value)
        {
            PlayerPrefs.SetInt("NormalShadow", value ? 1 : 0);
            PlayerPrefs.Save();
            QualitySettings.shadows = value ? ShadowQuality.All : ShadowQuality.Disable;
        }

        void OnRoundShadowChanged(bool value)
        {
            PlayerPrefs.SetInt("RoundShadow", value ? 1 : 0);
            PlayerPrefs.Save();
            RoundShadowSettingEvent roundShadowSettingEvent = Events.RoundShadowSettingEvent;
            roundShadowSettingEvent.Active = value;
            EventManager.Broadcast(roundShadowSettingEvent);
        }

        void OnSensitivityChanged(float sensitivity)
        {
            PlayerPrefs.SetFloat("Sensitivity", sensitivity);
            PlayerPrefs.Save();
            LookSensitivityUpdateEvent lookSensitivityUpdateEvent = Events.LookSensitivityUpdateEvent;
            lookSensitivityUpdateEvent.Value = sensitivity;
            EventManager.Broadcast(lookSensitivityUpdateEvent); // LookSensitivityUpdateEvent ブロードキャスト
        }

        public void CloseMenu() // ボタン (DoneButton) 押下時
        {
            SetMenuActivation(false);
        }

        public void ToggleMenu()
        {
            SetMenuActivation(!(m_Menu.activeSelf || m_Controls.activeSelf));
        }

        void SetMenuActivation(bool active)
        {
#if !UNITY_EDITOR
            Cursor.lockState = active ? CursorLockMode.None : CursorLockMode.Locked;
#endif
            m_Menu.SetActive(active);
            m_Controls.SetActive(false);

            if (m_Menu.activeSelf)
            {
                Time.timeScale = 0f;
                EventSystem.current.SetSelectedGameObject(null);
            }
            else
            {
                Time.timeScale = 1f;
            }

            OptionsMenuEvent evt = Events.OptionsMenuEvent;
            evt.Active = active;
            EventManager.Broadcast(evt);    // OptionsMenuEvent ブロードキャスト
        }

        void Update()
        {
            if (Input.GetButtonDown("Menu"))    // キー (TAB) 押下時
            {
                if (m_ChatManager != null && m_ChatManager.IsActive)
                {
                    return;
                }

                ToggleMenu();
            }
        }
    }
}
