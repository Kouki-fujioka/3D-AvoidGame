using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Unity.Gaame.UI
{
    public class MenuNavigation : MonoBehaviour
    {
        public Selectable DefaultSelection; // UI オブジェクト (初期選択)
        public bool ForceSelection = false; // 強制選択フラグ

        void Start()
        {
            EventSystem.current.SetSelectedGameObject(null);
        }

        void LateUpdate()
        {
            if (EventSystem.current.currentSelectedGameObject == null || ForceSelection)
            {
                EventSystem.current.SetSelectedGameObject(DefaultSelection.gameObject);
            }
        }

        void OnDisable()
        {
            if (ForceSelection && EventSystem.current.currentSelectedGameObject == DefaultSelection.gameObject)
            {
                EventSystem.current.SetSelectedGameObject(null);
            }
        }
    }
}
