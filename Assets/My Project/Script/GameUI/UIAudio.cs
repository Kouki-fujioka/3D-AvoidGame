using UnityEngine;

namespace Unity.Game.UI
{
    public class UIAudio : MonoBehaviour
    {
        static AudioSource s_AudioSource;

        void Awake()
        {
            if (!s_AudioSource)
            {
                var go = new GameObject("UI Audio");
                s_AudioSource = go.AddComponent<AudioSource>();
                DontDestroyOnLoad(go);
            }
        }

        public void Play(AudioClip audioClip)
        {
            s_AudioSource.PlayOneShot(audioClip);
        }
    }
}
