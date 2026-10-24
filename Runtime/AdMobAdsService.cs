using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using GoogleMobileAds.Api;
using GoogleMobileAds.Common;
using Larje.Core;
using Larje.Core.Monetization;
using Larje.Core.Saves;
using UnityEngine;

namespace Larje.Ads.AdMob
{
    public sealed class AdMobAdsService : AdsServiceBase, IInitializableService, IDisposable
    {
        private bool _initialized;
        private bool _fullscreenShowing;
        private float _lastFullscreenTime = float.MinValue;
        private readonly AdMobAdsConfig _config;
        private readonly CancellationTokenSource _lifetime = new CancellationTokenSource();
        private AdMobBanner _banner;
        private AdMobInterstitial _interstitial;
        private AdMobRewarded _rewarded;
        private AdMobAppOpen _appOpen;

        public override bool Initialized => _initialized;
        public override bool InterstitialAvailable => _interstitial != null && _interstitial.Ready && CooldownPassed;
        public override bool RewardedAvailable => _rewarded != null && _rewarded.Ready;
        public override bool BannerShowing => _banner != null && _banner.Showing;
        public override float BannerHeight => _banner != null ? _banner.Height : 0f;

        private bool CooldownPassed => Time.realtimeSinceStartup - _lastFullscreenTime >= _config.InterstitialCooldown;

        public AdMobAdsService(AdMobAdsConfig config, ISaveService saves) : base(saves)
        {
            _config = config;
        }

        public void Initialize()
        {
            if (_config.DisableInEditor && Application.isEditor)
            {
                Debug.Log("AdMobAdsService: disabled in the editor");
                return;
            }

            MobileAds.RaiseAdEventsOnUnityMainThread = true;
            MobileAds.Initialize(OnMobileAdsInitialized);
        }

        public void Dispose()
        {
            _lifetime.Cancel();
            _lifetime.Dispose();
            AppStateEventNotifier.AppStateChanged -= OnAppStateChanged;
            NoAdsChanged -= OnNoAdsChanged;
            _banner?.Dispose();
            _interstitial?.Dispose();
            _rewarded?.Dispose();
            _appOpen?.Dispose();
        }

        protected override void ShowInterstitialInternal(string placement)
        {
            if (_fullscreenShowing || !InterstitialAvailable)
            {
                return;
            }

            _fullscreenShowing = _interstitial.Show();
        }

        protected override UniTask<bool> ShowRewardedInternal(string placement, CancellationToken cancellationToken)
        {
            if (_fullscreenShowing || !RewardedAvailable)
            {
                return UniTask.FromResult(false);
            }

            _fullscreenShowing = true;
            return _rewarded.ShowAsync(cancellationToken);
        }

        protected override bool ShowAppOpenInternal()
        {
            if (_fullscreenShowing || _appOpen == null)
            {
                return false;
            }

            _fullscreenShowing = _appOpen.Show();
            return _fullscreenShowing;
        }

        protected override void HideBanner()
        {
            _banner?.Hide();
        }

        private void OnMobileAdsInitialized(InitializationStatus status)
        {
            SetupAsync().Forget();
        }

        private async UniTaskVoid SetupAsync()
        {
            await UniTask.SwitchToMainThread();
            if (_lifetime.IsCancellationRequested)
            {
                return;
            }

            AdMobAdUnits units = _config.CurrentUnits;
            _interstitial = new AdMobInterstitial(units.Interstitial, _config.RetryDelay);
            _interstitial.Finished += OnFullscreenFinished;
            _interstitial.Load();

            _rewarded = new AdMobRewarded(units.Rewarded, _config.RetryDelay);
            _rewarded.Finished += OnFullscreenFinished;
            _rewarded.Load();

            _appOpen = new AdMobAppOpen(units.AppOpen, _config.RetryDelay);
            _appOpen.Finished += OnAppOpenFinished;
            _appOpen.Load();

            if (_config.UseBanner)
            {
                _banner = new AdMobBanner(units.Banner, _config.BannerPosition, _config.RetryDelay);
                _banner.Shown += NotifyBannerShown;
                _banner.Hidden += NotifyBannerHidden;
                _banner.Load();
                if (!NoAds)
                {
                    _banner.Show();
                }
                else
                {
                    _banner.Hide();
                }
            }

            NoAdsChanged += OnNoAdsChanged;
            AppStateEventNotifier.AppStateChanged += OnAppStateChanged;
            _initialized = true;
            Debug.Log("AdMobAdsService: initialized");

            if (_config.ShowAppOpenOnStart)
            {
                ShowAppOpenWhenReadyAsync(_lifetime.Token).Forget();
            }
        }

        private async UniTaskVoid ShowAppOpenWhenReadyAsync(CancellationToken cancellationToken)
        {
            float deadline = Time.realtimeSinceStartup + _config.AppOpenStartTimeout;
            while (!_appOpen.Ready && Time.realtimeSinceStartup < deadline)
            {
                bool cancelled = await UniTask.Yield(PlayerLoopTiming.Update, cancellationToken).SuppressCancellationThrow();
                if (cancelled)
                {
                    return;
                }
            }

            ShowAppOpen();
        }

        private void OnFullscreenFinished(bool shown)
        {
            _fullscreenShowing = false;
            _lastFullscreenTime = Time.realtimeSinceStartup;
        }

        private void OnAppOpenFinished(bool shown)
        {
            _fullscreenShowing = false;
        }

        private void OnNoAdsChanged(bool active)
        {
            if (!active && _banner != null)
            {
                _banner.Show();
            }
        }

        private void OnAppStateChanged(AppState state)
        {
            if (state == AppState.Foreground && _config.ShowAppOpenOnResume)
            {
                ShowAppOpenOnResumeAsync().Forget();
            }
        }

        private async UniTaskVoid ShowAppOpenOnResumeAsync()
        {
            await UniTask.SwitchToMainThread();
            if (!_lifetime.IsCancellationRequested)
            {
                ShowAppOpen();
            }
        }
    }
}
