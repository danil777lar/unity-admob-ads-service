using System.Threading;
using Cysharp.Threading.Tasks;
using GoogleMobileAds.Api;

namespace Larje.Ads.AdMob
{
    public sealed class AdMobRewarded : AdMobFullscreenAd
    {
        private bool _earned;
        private RewardedAd _ad;
        private UniTaskCompletionSource<bool> _completion;

        public override bool Ready => _ad != null && _ad.CanShowAd();

        public AdMobRewarded(string adUnitId, float retryDelay) : base(adUnitId, retryDelay)
        {
        }

        public UniTask<bool> ShowAsync(CancellationToken cancellationToken)
        {
            if (!Ready)
            {
                return UniTask.FromResult(false);
            }

            _earned = false;
            _completion = new UniTaskCompletionSource<bool>();
            _ad.Show(OnRewardEarned);
            return _completion.Task.AttachExternalCancellation(cancellationToken);
        }

        protected override void LoadInternal(string adUnitId, AdRequest request)
        {
            RewardedAd.Load(adUnitId, request, OnLoaded);
        }

        protected override void DestroyAd()
        {
            if (_ad != null)
            {
                _ad.OnAdFullScreenContentClosed -= OnClosed;
                _ad.OnAdFullScreenContentFailed -= OnFailed;
                _ad.Destroy();
                _ad = null;
            }
        }

        private void OnLoaded(RewardedAd ad, LoadAdError error)
        {
            if (error == null && ad != null)
            {
                _ad = ad;
                _ad.OnAdFullScreenContentClosed += OnClosed;
                _ad.OnAdFullScreenContentFailed += OnFailed;
            }

            CompleteLoad(error);
        }

        private void OnRewardEarned(Reward reward)
        {
            _earned = true;
        }

        private void OnClosed()
        {
            _completion?.TrySetResult(_earned);
            _completion = null;
            CompleteShow(true);
        }

        private void OnFailed(AdError error)
        {
            _completion?.TrySetResult(false);
            _completion = null;
            CompleteShow(false);
        }
    }
}
