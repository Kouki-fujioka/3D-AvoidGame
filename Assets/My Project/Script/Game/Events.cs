namespace Unity.Game
{
    public class Events
    {
        public static ObjectiveAdded ObjectiveAddedEvent = new ObjectiveAdded();
        public static OptionsMenuEvent OptionsMenuEvent = new OptionsMenuEvent();
        public static RoundShadowSettingEvent RoundShadowSettingEvent = new RoundShadowSettingEvent();
        public static LookSensitivityUpdateEvent LookSensitivityUpdateEvent = new LookSensitivityUpdateEvent();
        public static GameOverEvent GameOverEvent = new GameOverEvent();
    }

    public class ObjectiveAdded : GameEvent
    {
        public IObjective Objective;
    }

    public class OptionsMenuEvent : GameEvent
    {
        public bool Active;
    }

    public class RoundShadowSettingEvent : GameEvent
    {
        public bool Active;
    }

    public class LookSensitivityUpdateEvent : GameEvent
    {
        public float Value;
    }

    public class GameOverEvent : GameEvent
    {
        public bool Win;
        public bool Fall;
    }
}
