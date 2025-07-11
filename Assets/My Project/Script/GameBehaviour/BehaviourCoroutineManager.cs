using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Unity.Game.Behaviours
{
    public class BehaviourCoroutineManager : MonoBehaviour
    {
        static BehaviourCoroutineManager m_Instance;    // シングルトンパターン
        static readonly Dictionary<Object, Dictionary<string, Coroutine>> s_ExistingCoroutines = new Dictionary<Object, Dictionary<string, Coroutine>>();

        /// <summary>
        /// 未登録コルーチン実行, 登録済みコルーチン停止
        /// </summary>
        /// <param name="owner"></param>
        /// <param name="key"></param>
        /// <param name="coroutine"></param>
        /// <param name="stopExisting"></param>
        public static void StartCoroutine(Object owner, string key, IEnumerator coroutine, bool stopExisting = false)
        {
            if (m_Instance)
            {
                if (stopExisting && s_ExistingCoroutines.ContainsKey(owner))
                {
                    if (s_ExistingCoroutines[owner].ContainsKey(key))
                    {
                        m_Instance.StopCoroutine(s_ExistingCoroutines[owner][key]);
                    }
                }

                if (s_ExistingCoroutines.ContainsKey(owner))
                {
                    s_ExistingCoroutines[owner].Remove(key);
                }

                if (!s_ExistingCoroutines.ContainsKey(owner))
                {
                    s_ExistingCoroutines.Add(owner, new Dictionary<string, Coroutine>());
                }

                s_ExistingCoroutines[owner].Add(key, m_Instance.StartCoroutine(coroutine));
            }
        }

        void Awake()
        {
            // シングルトンパターン再現
            if (m_Instance && m_Instance != this)
            {
                Destroy(this);
            }
            else
            {
                m_Instance = this;
            }
        }
    }
}
