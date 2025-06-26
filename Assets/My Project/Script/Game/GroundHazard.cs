using UnityEngine;

namespace Unity.Game
{
    public class GroundHazard : MonoBehaviour
    {
        void OnTriggerEnter(Collider other)
        {
            if (other.gameObject.CompareTag("Player"))
            {
                GameOverEvent evt = Events.GameOverEvent;
                evt.Win = false;
                evt.Fall = true;
                EventManager.Broadcast(evt);    // GameOverEvent ブロードキャスト
            }
        }
    }
}
