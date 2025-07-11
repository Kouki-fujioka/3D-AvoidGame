using System;

namespace Unity.Game
{
    public interface IObjective
    {
        string m_Title { get; } // 勝敗条件タイトル
        string m_Description { get; }   // 勝敗条件説明
        ObjectiveProgressType m_ProgressType { get; }   // 勝敗条件進捗タイプ
        bool m_Lose { get; }    // 敗北条件フラグ
        bool m_Hidden { get; }  // 勝敗条件表示フラグ
        bool IsCompleted { get; }   // 勝敗条件達成フラグ
        Action<IObjective> OnProgress { get; set; } // デリゲート
        string GetProgress();   // 進捗
    }
}
