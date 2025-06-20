using UnityEngine;

namespace Unity.Game.UI
{
    public class GameQuit : MonoBehaviour
    {
        public void EndGame()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else   // ゲーム実行中 (ビルド後)
            Application.Quit(); // アプリケーション終了
#endif
        }
    }
}
