using System;

namespace Unity.Game.Behaviours
{
    [Serializable]
    public class ObjectiveConfiguration
    {
        public string Title = "Title";  // 勝敗条件タイトル
        public string Description = "Description";  // 勝敗条件説明
        public ObjectiveProgressType ProgressType;  // 勝敗条件進捗タイプ
        public bool Lose;   // 敗北条件フラグ
        public bool Hidden; // 勝敗条件表示フラグ
    }
}
