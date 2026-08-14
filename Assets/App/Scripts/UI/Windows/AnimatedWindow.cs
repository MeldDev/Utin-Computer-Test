using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;

namespace UtinComputerTest.UI.Windows
{
    public class AnimatedWindow : BaseWindow
    {
        [Header("Animation Settings")]
        [SerializeField] private RectTransform _container;
        [SerializeField] private CanvasGroup _canvasGroup;
        [SerializeField] private float _animationDuration = 0.5f;
        [SerializeField] private Ease _animationEase = Ease.OutBack;
        [SerializeField] private PanelAnimationDirection _direction = PanelAnimationDirection.Center;
        [SerializeField] private bool _useFade = true;

        private Vector2 _initialAnchoredPosition;
        private Vector2 _offscreenPosition;
        private bool _isInitialized;
        private Tween _positionTween;
        private Tween _scaleTween;
        private Tween _fadeTween;

        protected override void OnOpened()
        {
            PlayOpenAnimationAsync().Forget();
        }

        public override void Close()
        {
            if (!IsOpened)
            {
                return;
            }

            KillTweens(true);
            SetInteractable(false);
            PlayCloseAnimationAsync().Forget();
        }

        protected override void OnClosed()
        {
            KillTweens(false);
            _container.anchoredPosition = _offscreenPosition;
            _container.localScale = GetHiddenScale();
            _canvasGroup.alpha = _useFade ? 0f : 1f;
            SetInteractable(false);
        }

        protected virtual void OnCloseStarted()
        {
        }

        protected virtual void OnDestroy()
        {
            KillTweens(false);
        }

        private async UniTaskVoid PlayOpenAnimationAsync()
        {
            InitializePositions();
            KillTweens(false);

            _container.anchoredPosition = _offscreenPosition;
            _container.localScale = GetHiddenScale();
            _canvasGroup.alpha = _useFade ? 0f : 1f;
            SetInteractable(false);

            var sequence = DOTween.Sequence().SetUpdate(true);
            if (_useFade)
            {
                _fadeTween = _canvasGroup.DOFade(1f, _animationDuration).SetUpdate(true);
                sequence.Join(_fadeTween);
            }

            _positionTween = _container
                .DOAnchorPos(_initialAnchoredPosition, _animationDuration)
                .SetEase(_animationEase)
                .SetUpdate(true);
            sequence.Join(_positionTween);

            if (_direction == PanelAnimationDirection.Center)
            {
                _scaleTween = _container.DOScale(1f, _animationDuration).SetEase(_animationEase).SetUpdate(true);
                sequence.Join(_scaleTween);
            }

            await sequence.AsyncWaitForCompletion();

            if (!IsOpened)
            {
                return;
            }

            _container.anchoredPosition = _initialAnchoredPosition;
            _container.localScale = Vector3.one;
            _canvasGroup.alpha = 1f;
            SetInteractable(true);
        }

        private async UniTaskVoid PlayCloseAnimationAsync()
        {
            OnCloseStarted();
            var sequence = DOTween.Sequence().SetUpdate(true);

            if (_useFade)
            {
                _fadeTween = _canvasGroup.DOFade(0f, _animationDuration).SetUpdate(true);
                sequence.Join(_fadeTween);
            }

            _positionTween = _container
                .DOAnchorPos(_offscreenPosition, _animationDuration)
                .SetEase(_animationEase)
                .SetUpdate(true);
            sequence.Join(_positionTween);

            if (_direction == PanelAnimationDirection.Center)
            {
                _scaleTween = _container.DOScale(0.5f, _animationDuration).SetEase(_animationEase).SetUpdate(true);
                sequence.Join(_scaleTween);
            }

            await sequence.AsyncWaitForCompletion();
            base.Close();
        }

        private void InitializePositions()
        {
            if (_isInitialized)
            {
                return;
            }

            _initialAnchoredPosition = _container.anchoredPosition;
            _offscreenPosition = GetOffscreenPosition();
            _isInitialized = true;
        }

        private Vector2 GetOffscreenPosition()
        {
            var offscreenPosition = _initialAnchoredPosition;
            var containerRect = _container.rect;
            var parentRect = ((RectTransform)_container.parent).rect;

            switch (_direction)
            {
                case PanelAnimationDirection.Top:
                    offscreenPosition.y = parentRect.height + containerRect.height;
                    break;
                case PanelAnimationDirection.Bottom:
                    offscreenPosition.y = -parentRect.height - containerRect.height;
                    break;
                case PanelAnimationDirection.Left:
                    offscreenPosition.x = -parentRect.width - containerRect.width;
                    break;
                case PanelAnimationDirection.Right:
                    offscreenPosition.x = parentRect.width + containerRect.width;
                    break;
            }

            return offscreenPosition;
        }

        private Vector3 GetHiddenScale()
        {
            return _direction == PanelAnimationDirection.Center ? Vector3.one * 0.5f : Vector3.one;
        }

        private void SetInteractable(bool isInteractable)
        {
            _canvasGroup.interactable = isInteractable;
            _canvasGroup.blocksRaycasts = isInteractable;
        }

        private void KillTweens(bool complete)
        {
            if (complete)
            {
                _positionTween?.Complete();
                _scaleTween?.Complete();
                _fadeTween?.Complete();
            }

            _positionTween?.Kill();
            _scaleTween?.Kill();
            _fadeTween?.Kill();
            _positionTween = null;
            _scaleTween = null;
            _fadeTween = null;
        }
    }
}
