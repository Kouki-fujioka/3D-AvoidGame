using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Unity.Game.Behaviours.Triggers;

#if UNITY_EDITOR
using UnityEditor.SceneManagement;
#endif

namespace Unity.Game.Behaviours.Actions
{
    public abstract class Action : MonoBehaviour
    {
        [SerializeField, Tooltip("The audio clip used by the Behaviour.")] protected AudioClip m_Audio;
        [SerializeField, Range(0.0f, 1.0f), Tooltip("The volume of the audio.")] protected float m_AudioVolume = 1.0f;

        protected bool m_Active;
        readonly List<AudioSource> m_AudioSourcesToDestroy = new List<AudioSource>();
        const float k_AudioRange = 80.0f;

        public virtual void Activate()
        {
            if (!m_Active)  // ìÆçÏîÒé¿çséû
            {
                m_Active = true;    // ìÆçÏé¿çs
            }
        }

        public List<Trigger> GetTargetingTriggers()
        {
            var result = new List<Trigger>();

#if UNITY_EDITOR
            var triggers = StageUtility.GetStageHandle(gameObject).FindComponentsOfType<Trigger>();
#else
            var triggers = FindObjectsOfType<Trigger>();
#endif

            foreach (var trigger in triggers)
            {
                if (trigger.GetTargetedActions().Contains(this))
                {
                    result.Add(trigger);
                }
            }

            return result;
        }

        protected virtual void Start()
        {
            if (GetTargetingTriggers().Count == 0)
            {
                Activate();
            }
        }

        protected AudioSource PlayAudio(bool loop = false, bool spatial = true, bool moveWithScope = true, bool scopeDeterminesDistance = true, bool destroyWithAction = true, float pitch = 1.0f)
        {
            if (m_Audio)
            {
                GameObject go = new GameObject("Audio");

                if (moveWithScope)
                {
                    go.transform.parent = transform;
                }

                go.transform.position = transform.position;
                AudioSource audioSource = go.AddComponent<AudioSource>();
                audioSource.clip = m_Audio;
                audioSource.loop = loop;
                audioSource.pitch = pitch;
                audioSource.volume = m_AudioVolume;
                audioSource.dopplerLevel = 0.0f;

                if (spatial)
                {
                    audioSource.spatialBlend = 1.0f;
                    var minDistance = scopeDeterminesDistance ? Mathf.Max(5.0f, transform.localScale.x) : 5.0f;
                    var maxDistance = minDistance + k_AudioRange;
                    var minRatio = minDistance / maxDistance;
                    audioSource.maxDistance = maxDistance;
                    audioSource.rolloffMode = AudioRolloffMode.Custom;
                    audioSource.SetCustomCurve(AudioSourceCurveType.CustomRolloff, new AnimationCurve(new Keyframe[] { new Keyframe(minRatio, 1.0f, 0.0f, -3.0f), new Keyframe(1.0f, 0.0f, 0.0f, 0.0f) }));
                }

                audioSource.Play();

                if (!loop)
                {
                    BehaviourCoroutineManager.StartCoroutine(this, "Audio", DoDestroyAudio(audioSource));
                }

                if (destroyWithAction)
                {
                    m_AudioSourcesToDestroy.Add(audioSource);
                }

                return audioSource;
            }
            else
            {
                return null;
            }
        }

        protected virtual void OnDestroy()
        {
            foreach (AudioSource audioSource in m_AudioSourcesToDestroy)
            {
                Destroy(audioSource.gameObject);
            }
        }

        IEnumerator DoDestroyAudio(AudioSource audioSource)
        {
            yield return new WaitForSeconds(audioSource.clip.length);
            m_AudioSourcesToDestroy.Remove(audioSource);

            if (audioSource)
            {
                Destroy(audioSource.gameObject);
            }
        }
    }
}
