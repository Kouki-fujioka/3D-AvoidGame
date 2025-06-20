using System;

namespace Unity.Game.Behaviours
{
    [Serializable]
    public class ObjectiveConfiguration
    {
        public string Title = "Title";
        public string Description = "Description";
        public ObjectiveProgressType ProgressType;
        public bool Lose;
        public bool Hidden;
    }
}
