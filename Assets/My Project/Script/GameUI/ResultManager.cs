using UnityEngine;
using DG.Tweening;

namespace Unity.Game.UI
{
    public class ResultManager : MonoBehaviour
    {
        [Header("動かす対象")]
        [SerializeField] private RectTransform titleText;
        [SerializeField] private CanvasGroup buttonContainer;

        [Header("演出")]
        [SerializeField] private AudioSource audioSource;
        [SerializeField] private ParticleSystem confetti;

        void Awake()
        {
            // Nullチェックを追加（安全のため）
            if (titleText != null) titleText.localScale = Vector3.zero;
            if (buttonContainer != null)
            {
                buttonContainer.alpha = 0f;
                buttonContainer.transform.localScale = new Vector3(0.8f, 0.8f, 1f);
            }
        }

        void Start()
        {
            if (titleText == null || buttonContainer == null) return;

            Sequence seq = DOTween.Sequence();

            // ★追加：このSequenceをGameObjectに紐付ける
            seq.SetLink(gameObject);

            seq.SetUpdate(true);
            seq.PrependInterval(0.5f);

            seq.Append(titleText.DOScale(1f, 0.5f).SetEase(Ease.OutBack));
            seq.Join(DOVirtual.DelayedCall(0, () =>
            {
                if (audioSource != null) audioSource.Play();
                if (confetti != null) confetti.Play();
            }));

            //// A. タイトル演出
            //seq.Append(titleText.DOScale(1f, 0.5f).SetEase(Ease.OutBack));

            //// ★追加：音＆紙吹雪
            //seq.AppendCallback(() =>
            //{
            //    if (audioSource != null) audioSource.Play();
            //    if (confetti != null) confetti.Play();
            //});

            // B. ボタン演出
            float delay = 0.1f;
            seq.Insert(0.5f + delay, buttonContainer.DOFade(1f, 0.5f));
            seq.Insert(0.5f + delay, buttonContainer.transform.DOScale(1f, 0.5f).SetEase(Ease.OutCubic));

            seq.OnComplete(() =>
            {
                // ★追加：無限ループするアニメーションにも必ずSetLinkをつける
                // これを忘れると、オブジェクトが消えた後にエラーが出ます
                if (titleText != null)
                {
                    titleText.DOScale(1.05f, 1.0f)
                        .SetEase(Ease.InOutSine)
                        .SetLoops(-1, LoopType.Yoyo)
                        .SetLink(titleText.gameObject);
                }
            });
        }

        // ★追加：念のための安全策（オブジェクト破棄時に全Tweenを停止）
        private void OnDestroy()
        {
            DOTween.Kill(gameObject);
        }
    }
}
