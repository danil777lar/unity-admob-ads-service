using GoogleMobileAds.Api;

namespace Larje.Ads.AdMob
{
    public sealed class AdMobInterstitial : AdMobFullscreenAd
    {
        private InterstitialAd _ad;

        public override bool Ready => _ad != null && _ad.CanShowAd();

        public AdMobInterstitial(string adUnitId, float retryDelay) : base(adUnitId, retryDelay)
        {
        }

        public bool Show()
        {
            if (!Ready)
            {
                return false;
            }

            _ad.Show();
            return true;
        }

        protected override void LoadInternal(string adUnitId, AdRequest request)
        {
            InterstitialAd.Load(adUnitId, request, OnLoaded);
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

        private void OnLoaded(InterstitialAd ad, LoadAdError error)
        {
            if (error == null && ad != null)
            {
                _ad = ad;
                _ad.OnAdFullScreenContentClosed += OnClosed;
                _ad.OnAdFullScreenContentFailed += OnFailed;
            }

            CompleteLoad(error);
        }

        private void OnClosed()
        {
            CompleteShow(true);
        }

        private void OnFailed(AdError error)
        {
            CompleteShow(false);
        }
    }
}
