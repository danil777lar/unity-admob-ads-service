using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using GoogleMobileAds.Api;
using UnityEngine;

namespace Larje.Ads.AdMob
{
    public sealed class AdMobBanner : IDisposable
    {
        private bool _loaded;
        private bool _visible;
        private readonly float _retryDelay;
        private readonly string _adUnitId;
        private readonly AdPosition _position;
        private readonly CancellationTokenSource _lifetime = new CancellationTokenSource();
        private BannerView _view;

        public bool Showing => _loaded && _visible;
        public float Height => Showing ? _view.GetHeightInPixels() : 0f;

        public event Action Shown;
        public event Action Hidden;

        public AdMobBanner(string adUnitId, AdPosition position, float retryDelay)
        {
            _adUnitId = adUnitId;
            _position = position;
            _retryDelay = retryDelay;
        }

        public void Load()
        {
            if (string.IsNullOrEmpty(_adUnitId) || _lifetime.IsCancellationRequested)
            {
                return;
            }

            DestroyView();
            AdSize size = AdSize.GetCurrentOrientationAnchoredAdaptiveBannerAdSizeWithWidth(AdSize.FullWidth);
            _view = new BannerView(_adUnitId, size, _position);
            _view.OnBannerAdLoaded += OnLoaded;
            _view.OnBannerAdLoadFailed += OnLoadFailed;
            _view.LoadAd(new AdRequest());
        }

        public void Show()
        {
            bool wasShowing = Showing;
            _visible = true;
            if (_loaded)
            {
                _view.Show();
                if (!wasShowing)
                {
                    Shown?.Invoke();
                }
            }
        }

        public void Hide()
        {
            bool wasShowing = Showing;
            _visible = false;
            _view?.Hide();
            if (wasShowing)
            {
                Hidden?.Invoke();
            }
        }

        public void Dispose()
        {
            _lifetime.Cancel();
            _lifetime.Dispose();
            DestroyView();
        }

        private void OnLoaded()
        {
            bool wasLoaded = _loaded;
            _loaded = true;
            if (!_visible)
            {
                _view.Hide();
            }
            else if (!wasLoaded)
            {
                Shown?.Invoke();
            }
        }

        private void OnLoadFailed(LoadAdError error)
        {
            Debug.LogWarning($"AdMobBanner: load failed for '{_adUnitId}': {error.GetMessage()}");
            if (!_loaded)
            {
                RetryAsync().Forget();
            }
        }

        private void DestroyView()
        {
            if (_view != null)
            {
                _view.OnBannerAdLoaded -= OnLoaded;
                _view.OnBannerAdLoadFailed -= OnLoadFailed;
                _view.Destroy();
                _view = null;
            }

            _loaded = false;
        }

        private async UniTaskVoid RetryAsync()
        {
            bool cancelled = await UniTask.Delay(TimeSpan.FromSeconds(_retryDelay), DelayType.Realtime, PlayerLoopTiming.Update, _lifetime.Token).SuppressCancellationThrow();
            if (!cancelled)
            {
                Load();
            }
        }
    }
}
