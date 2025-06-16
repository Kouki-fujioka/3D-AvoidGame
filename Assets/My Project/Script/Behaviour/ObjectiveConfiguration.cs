using System;

namespace Unity.Game.Behaviour
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
