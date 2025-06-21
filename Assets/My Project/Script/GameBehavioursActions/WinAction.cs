using Unity.Game.Behaviours.Triggers;

namespace Unity.Game.Behaviours.Actions
{
    public class WinAction : ObjectiveAction
    {
        /// <summary>
        /// 勝敗条件 (タイトル, 説明, 進捗タイプ) を設定
        /// </summary>
        /// <param name="trigger"></param>
        /// <returns></returns>
        public override ObjectiveConfiguration GetDefaultObjectiveConfiguration(Trigger trigger)
        {
            ObjectiveConfiguration result = new ObjectiveConfiguration();

            if (trigger)
            {
                var triggerType = trigger.GetType();

                if (triggerType == typeof(TimerTrigger))
                {
                    result.Title = "Survive";
                    result.Description = "Hang in there!";
                    result.ProgressType = ObjectiveProgressType.Time;
                }
                else
                {
                    result.Title = "Complete the Objective";
                    result.Description = "Just do it!";
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
