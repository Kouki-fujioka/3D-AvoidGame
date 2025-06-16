using Unity.Game;

namespace Unity.Game.Behaviour.Action
{
    public class WinAction : ObjectiveAction
    {
        public override ObjectiveConfiguration GetDefaultObjectiveConfiguration(Trigger trigger)
        {
            ObjectiveConfiguration result = new ObjectiveConfiguration();

            if (trigger)
            {
                var triggerType = trigger.GetType();

                if (triggerType == typeof(TimerTrigger))   // TimerTrigger 型の場合
                {
                    result.Title = "Survive";   // タイトルを設定
                    result.Description = "Hang in there!";  // 説明を設定
                    result.ProgressType = ObjectiveProgressType.Time;   // 進捗を設定
                }
                else
                {
                    result.Title = "Complete the Objective";    // タイトルを設定
                    result.Description = "Just do it!"; // 説明を設定
                }
            }
            else
            {
                result.Title = "Easy as Pie!";
                result.Description = "Connect a Trigger Brick to the Win Brick to make this objective more challenging.";
            }

            return result;
        }
    }
}
