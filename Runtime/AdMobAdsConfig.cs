using GoogleMobileAds.Api;
using UnityEngine;

namespace Larje.Ads.AdMob
{
    [CreateAssetMenu(fileName = "AdMobAdsConfig", menuName = "Larje/Ads/AdMob Ads Config")]
    public sealed class AdMobAdsConfig : ScriptableObject
    {
        [Header("Ad Units")]
        [SerializeField] private AdMobAdUnits android = new AdMobAdUnits(
            "ca-app-pub-3940256099942544/9214589741",
            "ca-app-pub-3940256099942544/1033173712",
            "ca-app-pub-3940256099942544/5224354917",
            "ca-app-pub-3940256099942544/9257395921");
        [SerializeField] private AdMobAdUnits ios = new AdMobAdUnits(
            "ca-app-pub-3940256099942544/2435281174",
            "ca-app-pub-3940256099942544/4411468910",
            "ca-app-pub-3940256099942544/1712485313",
            "ca-app-pub-3940256099942544/5575463023");

        [Header("Interstitial")]
        [SerializeField, Min(0f)] private float interstitialCooldown = 20f;

        [Header("Banner")]
        [SerializeField] private bool useBanner = true;
        [SerializeField] private AdPosition bannerPosition = AdPosition.Bottom;

        [Header("App Open")]
        [SerializeField] private bool showAppOpenOnStart;
        [SerializeField] private bool showAppOpenOnResume;
        [SerializeField, Min(0f)] private float appOpenStartTimeout = 5f;

        [Header("Loading")]
        [SerializeField, Min(1f)] private float retryDelay = 15f;
        [SerializeField] private bool disableInEditor;

        public float InterstitialCooldown => interstitialCooldown;
        public bool UseBanner => useBanner;
        public AdPosition BannerPosition => bannerPosition;
        public bool ShowAppOpenOnStart => showAppOpenOnStart;
        public bool ShowAppOpenOnResume => showAppOpenOnResume;
        public float AppOpenStartTimeout => appOpenStartTimeout;
        public float RetryDelay => retryDelay;
        public bool DisableInEditor => disableInEditor;

        public AdMobAdUnits CurrentUnits
        {
            get
            {
#if UNITY_IOS
                return ios;
#else
                return android;
#endif
            }
        }
    }
}
