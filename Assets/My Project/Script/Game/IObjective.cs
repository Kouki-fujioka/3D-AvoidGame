using System;

namespace Unity.Game
{
    public interface IObjective
    {
        string m_Title { get; }
        string m_Description { get; }
        ObjectiveProgressType m_ProgressType { get; }
        bool m_Lose { get; }
        bool m_Hidden { get; }
        bool IsCompleted { get; }
        Action<IObjective> OnProgress { get; set; }
        string GetProgress();
    }
}
