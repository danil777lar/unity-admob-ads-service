using System;
using UnityEngine;

namespace Larje.Ads.AdMob
{
    [Serializable]
    public sealed class AdMobAdUnits
    {
        [SerializeField] private string banner;
        [SerializeField] private string interstitial;
        [SerializeField] private string rewarded;
        [SerializeField] private string appOpen;

        public string Banner => banner;
        public string Interstitial => interstitial;
        public string Rewarded => rewarded;
        public string AppOpen => appOpen;

        public AdMobAdUnits(string banner, string interstitial, string rewarded, string appOpen)
        {
            this.banner = banner;
            this.interstitial = interstitial;
            this.rewarded = rewarded;
            this.appOpen = appOpen;
        }
    }
}
