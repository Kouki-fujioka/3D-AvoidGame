namespace Unity.Game
{
    public class Events
    {
        public static OptionsMenuEvent OptionsMenuEvent = new OptionsMenuEvent();
        public static ObjectiveAdded ObjectiveAddedEvent = new ObjectiveAdded();
        public static GameOverEvent GameOverEvent = new GameOverEvent();
        public static LookSensitivityUpdateEvent LookSensitivityUpdateEvent = new LookSensitivityUpdateEvent();
    }

    public class OptionsMenuEvent : GameEvent
    {
        public bool Active;
    }

    public class ObjectiveAdded : GameEvent
    {
        public IObjective Objective;
    }

    public class GameOverEvent : GameEvent
    {
        public bool Win;
        public bool Fall;
    }

    public class LookSensitivityUpdateEvent : GameEvent
    {
        public float Value;
    }
}
