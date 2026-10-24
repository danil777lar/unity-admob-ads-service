using System;
using GoogleMobileAds.Api;
using UnityEngine;

namespace Larje.Ads.AdMob
{
    public sealed class AdMobAppOpen : AdMobFullscreenAd
    {
        private static readonly TimeSpan s_lifetime = TimeSpan.FromHours(4);

        private float _loadedAt;
        private AppOpenAd _ad;

        public override bool Ready => _ad != null && _ad.CanShowAd() && !Expired;

        private bool Expired => Time.realtimeSinceStartup - _loadedAt > s_lifetime.TotalSeconds;

        public AdMobAppOpen(string adUnitId, float retryDelay) : base(adUnitId, retryDelay)
        {
        }

        public bool Show()
        {
            if (_ad != null && Expired)
            {
                DestroyAd();
                Load();
                return false;
            }

            if (!Ready)
            {
                return false;
            }

            _ad.Show();
            return true;
        }

        protected override void LoadInternal(string adUnitId, AdRequest request)
        {
            AppOpenAd.Load(adUnitId, request, OnLoaded);
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

        private void OnLoaded(AppOpenAd ad, LoadAdError error)
        {
            if (error == null && ad != null)
            {
                _ad = ad;
                _loadedAt = Time.realtimeSinceStartup;
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
