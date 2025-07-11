using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Unity.Game.UI
{
    public class MenuManager : MonoBehaviour
    {
        [Header("参照")]
        [SerializeField, Tooltip("メニュー画面")] GameObject m_Menu = default;
        [SerializeField, Tooltip("操作説明画面")] GameObject m_Controls = default;
        [SerializeField, Tooltip("影用トグル")] Toggle m_ShadowsToggle = default;
        [SerializeField, Tooltip("FPS 用トグル")] Toggle m_FrameRateCounterToggle = default;
        [SerializeField, Tooltip("視点移動速度用スライダ")] Slider m_Sensitivity = default;

        FrameRateCounter m_FrameRateCounter;

        void Start()
        {
            m_FrameRateCounter = FindFirstObjectByType<FrameRateCounter>();

            if (m_FrameRateCounter == null)
            {
                Debug.LogError("FrameRateCounter is null");
            }

            m_Menu.SetActive(false);
            m_ShadowsToggle.SetIsOnWithoutNotify(QualitySettings.shadows != ShadowQuality.Disable);
            m_ShadowsToggle.onValueChanged.AddListener(OnShadowsChanged);
            m_FrameRateCounterToggle.SetIsOnWithoutNotify(m_FrameRateCounter.IsActive);
            m_FrameRateCounterToggle.onValueChanged.AddListener(OnFrameRateCounterChanged);
            var defaultSensitivity = PlayerPrefs.GetFloat("Sensitivity", 5.0f);
            m_Sensitivity.SetValueWithoutNotify(defaultSensitivity);
            m_Sensitivity.onValueChanged.AddListener(OnSensitivityChanged);
            OnSensitivityChanged(defaultSensitivity);
        }

        void OnShadowsChanged(bool value)
        {
            QualitySettings.shadows = value ? ShadowQuality.All : ShadowQuality.Disable;
        }

        void OnFrameRateCounterChanged(bool value)
        {
            m_FrameRateCounter.Show(value);
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
                ToggleMenu();
            }
        }
    }
}
