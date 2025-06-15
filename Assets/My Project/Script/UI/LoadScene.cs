using UnityEngine;
using UnityEngine.SceneManagement;

namespace Unity.Game.UI
{
    public class LoadSceneButton : MonoBehaviour
    {
        public string sceneName = "";

        public void LoadScene()
        {
            SceneManager.LoadScene(sceneName);  // シーンをロード
        }

        public void LoadPreviousScene()
        {
            if (GameFlowManager.PreviousScene != null)  // 旧シーンが存在する場合
            {
                SceneManager.LoadScene(GameFlowManager.PreviousScene);  // 旧シーンをロード
            }
            else
            {
                LoadScene();    // シーンをロード
            }
        }
    }
}
