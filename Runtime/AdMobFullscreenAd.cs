using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using GoogleMobileAds.Api;
using UnityEngine;

namespace Larje.Ads.AdMob
{
    public abstract class AdMobFullscreenAd : IDisposable
    {
        private bool _loading;
        private readonly float _retryDelay;
        private readonly string _adUnitId;
        private readonly CancellationTokenSource _lifetime = new CancellationTokenSource();

        public abstract bool Ready { get; }

        public event Action<bool> Finished;

        protected AdMobFullscreenAd(string adUnitId, float retryDelay)
        {
            _adUnitId = adUnitId;
            _retryDelay = retryDelay;
        }

        public void Load()
        {
            if (_loading || Ready || string.IsNullOrEmpty(_adUnitId) || _lifetime.IsCancellationRequested)
            {
                return;
            }

            _loading = true;
            LoadInternal(_adUnitId, new AdRequest());
        }

        public void Dispose()
        {
            _lifetime.Cancel();
            _lifetime.Dispose();
            DestroyAd();
        }

        protected abstract void LoadInternal(string adUnitId, AdRequest request);
        protected abstract void DestroyAd();

        protected void CompleteLoad(LoadAdError error)
        {
            _loading = false;
            if (error != null)
            {
                Debug.LogWarning($"{GetType().Name}: load failed for '{_adUnitId}': {error.GetMessage()}");
                RetryAsync().Forget();
            }
        }

        protected void CompleteShow(bool shown)
        {
            DestroyAd();
            Finished?.Invoke(shown);
            Load();
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
