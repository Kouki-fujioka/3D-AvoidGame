using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
using Cinemachine;

namespace Unity.Game
{
    public class GameFlowManager : MonoBehaviour
    {
        [Header("参照")]
        [SerializeField, Tooltip("BGM")] AudioClip m_AudioClip;

        [Header("データ")]
        [SerializeField, Tooltip("ゲーム勝利時にロードするシーン名")] string m_WinScene = "Menu Win";
        [SerializeField, Tooltip("ゲーム勝利時にロードするシーンへの遷移時間")] float m_WinSceneDelay = 6.0f;    // 勝利アニメーション時間確保
        [SerializeField, Tooltip("ゲーム敗北時にロードするシーン名")] string m_LoseScene = "Menu Lose";
        [SerializeField, Tooltip("ゲーム敗北時にロードするシーンへの遷移時間")] float m_LoseSceneDelay = 3.0f;   // 敗北アニメーション時間確保
        [SerializeField, Tooltip("ゲーム開始時にカメラ操作を無効にする時間")] float m_StartGameLockedControllerTime = 0.3f;

        AudioSource m_AudioSource;
        CinemachineFreeLook m_FreeLookCamera;
        public static string PreviousScene { get; private set; }    // 現シーン名
        public bool GameIsEnding { get; private set; }  // ゲーム終了フラグ
        float m_GameOverSceneTime;  // シーン遷移時間 (勝利 or 敗北)
        string m_GameOverSceneToLoad;   // シーン名 (勝利 or 敗北)
        string m_ControllerAxisXName;   // カメラ x 軸名
        string m_ControllerAxisYName;   // カメラ y 軸名

        private void Awake()
        {
            EventManager.AddListener<GameOverEvent>(OnGameOver);    // GameOverEvent ブロードキャスト時に OnGameOver 実行
            m_AudioSource = GetComponent<AudioSource>();
            m_AudioSource.clip = m_AudioClip;
            m_AudioSource.loop = true;
            m_FreeLookCamera = FindFirstObjectByType<CinemachineFreeLook>();

#if !UNITY_EDITOR
            Cursor.lockState = CursorLockMode.Locked;
#endif

            if (m_FreeLookCamera)
            {
                // カメラ操作無効
                m_ControllerAxisXName = m_FreeLookCamera.m_XAxis.m_InputAxisName;
                m_ControllerAxisYName = m_FreeLookCamera.m_YAxis.m_InputAxisName;
                m_FreeLookCamera.m_XAxis.m_InputAxisName = "";
                m_FreeLookCamera.m_YAxis.m_InputAxisName = "";
            }
        }
        
        void Start()
        {
            m_AudioSource.Play();
            StartCoroutine(StartGameLockControll());
        }

        /// <summary>
        /// ゲーム開始から一定時間経過後にカメラ操作を有効
        /// </summary>
        /// <returns></returns>
        IEnumerator StartGameLockControll()
        {
            while (m_StartGameLockedControllerTime > 0.0f)
            {
                m_StartGameLockedControllerTime -= Time.deltaTime;

                if (m_StartGameLockedControllerTime <= 0.0f)
                {
                    if (m_FreeLookCamera)
                    {
                        m_FreeLookCamera.m_XAxis.m_InputAxisName = m_ControllerAxisXName;
                        m_FreeLookCamera.m_YAxis.m_InputAxisName = m_ControllerAxisYName;
                    }
                }

                yield return new WaitForEndOfFrame();
            }
        }

        void Update()
        {
            if (GameIsEnding)
            {
                if (Time.time >= m_GameOverSceneTime)
                {

#if !UNITY_EDITOR
            Cursor.lockState = CursorLockMode.None;
#endif

                    PreviousScene = SceneManager.GetActiveScene().name;
                    SceneManager.LoadScene(m_GameOverSceneToLoad);
                }
            }
        }

        void OnGameOver(GameOverEvent evt)
        {
            if (!GameIsEnding)
            {
                GameIsEnding = true;
                m_AudioSource.Stop();

                if (evt.Win)
                {
                    m_GameOverSceneToLoad = m_WinScene;
                    m_GameOverSceneTime = Time.time + m_WinSceneDelay;
                    StartCoroutine(ZoomInOnPlayer());
                }
                else
                {
                    m_GameOverSceneToLoad = m_LoseScene;
                    m_GameOverSceneTime = Time.time + m_LoseSceneDelay;

                    if (m_FreeLookCamera)
                    {
                        m_FreeLookCamera.Follow = null;
                    }
                }
            }
        }

        /// <summary>
        /// プレイヤをズーム
        /// </summary>
        /// <returns></returns>
        IEnumerator ZoomInOnPlayer()
        {
            if (m_FreeLookCamera)
            {
                // カメラ操作無効
                m_FreeLookCamera.m_XAxis.m_InputAxisValue = 0.0f;
                m_FreeLookCamera.m_YAxis.m_InputAxisValue = 0.0f;
                m_FreeLookCamera.m_XAxis.m_InputAxisName = "";
                m_FreeLookCamera.m_YAxis.m_InputAxisName = "";
                var zoomFactor = 1.0f;  // ズーム率 (1 ~ 0.3)
                float middleRigZoomFactor = m_FreeLookCamera.m_Orbits[1].m_Radius;  // 距離 (カメラ ~ プレイヤ)

                while (zoomFactor > 0.3f)
                {
                    m_FreeLookCamera.m_YAxis.Value = Mathf.Lerp(m_FreeLookCamera.m_YAxis.Value, 0.6f, 3.0f * Time.deltaTime);
                    zoomFactor -= 0.1f * Time.deltaTime;
                    m_FreeLookCamera.m_Orbits[1].m_Radius = middleRigZoomFactor * zoomFactor;   // ズーム
                    yield return new WaitForEndOfFrame();
                }
            }
        }

        void OnDestroy()
        {
            EventManager.RemoveListener<GameOverEvent>(OnGameOver); // OnGameOver 登録解除
        }
    }
}
