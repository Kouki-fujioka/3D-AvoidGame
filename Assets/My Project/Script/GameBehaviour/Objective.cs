using System;
using UnityEngine;
using Unity.Game.Behaviours.Triggers;

namespace Unity.Game.Behaviours
{
    public class Objective : MonoBehaviour, IObjective
    {
        public Trigger m_Trigger;
        public string m_Title { get; set; } // 勝敗条件タイトル
        public string m_Description { get; set; }   // 勝敗条件説明
        public ObjectiveProgressType m_ProgressType { get; set; }   // 勝敗条件進捗タイプ
        public bool m_Lose { get; set; }    // 敗北条件フラグ
        public bool m_Hidden { get; set; }  // 勝敗条件表示フラグ
        public bool IsCompleted { get; private set; }   // 勝敗条件達成フラグ
        public Action<IObjective> OnProgress { get; set; }  // デリゲート

        /// <summary>
        /// 進捗を返却
        /// </summary>
        /// <returns></returns>
        public string GetProgress()
        {
            switch(m_ProgressType)
            {
                case ObjectiveProgressType.None:
                    {
                        return string.Empty;
                    }

                case ObjectiveProgressType.Amount:
                    {
                        return m_Trigger.Progress + "/" + m_Trigger.Goal;
                    }

                case ObjectiveProgressType.Time:
                    {
                        var seconds = m_Trigger.Goal - m_Trigger.Progress;
                        var minutes = seconds / 60;
                        seconds -= 60 * minutes;

                        if (minutes > 0)
                        {
                            return minutes.ToString() + ":" + seconds.ToString("D2");
                        }
                        else
                        {
                            return seconds.ToString();
                        }
                    }
            }

            return string.Empty;
        }

        void Start()
        {
            ObjectiveAdded evt = Events.ObjectiveAddedEvent;
            evt.Objective = this;
            EventManager.Broadcast(evt);    // ObjectiveAdded ブロードキャスト

            if (m_Trigger)
            {
                m_Trigger.OnProgress += Progress;   // リスナ登録
                m_Trigger.OnActivate += Activate;   // リスナ登録
            }
            else
            {
                Activate();
            }
        }

        /// <summary>
        /// テキスト (進捗), フラグ (m_UpdateStatus) 更新
        /// </summary>
        void Progress()
        {
            OnProgress?.Invoke(this);
        }

        /// <summary>
        /// テキスト (進捗), フラグ (IsCompleted, m_UpdateStatus) 更新
        /// </summary>
        void Activate()
        {
            if (IsCompleted)
            {
                return;
            }
            else
            {
                IsCompleted = true;
                OnProgress?.Invoke(this);
            }
        }
    }
}
