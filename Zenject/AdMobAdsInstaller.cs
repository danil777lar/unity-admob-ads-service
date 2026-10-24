using Larje.Core.Zenject;
using UnityEngine;
using Zenject;

namespace Larje.Ads.AdMob.Zenject
{
    [CreateAssetMenu(fileName = "AdMobAdsInstaller", menuName = "Larje/Ads/AdMob Ads Installer")]
    public sealed class AdMobAdsInstaller : ScriptableObjectInstaller<AdMobAdsInstaller>
    {
        [SerializeField] private AdMobAdsConfig config;

        public override void InstallBindings()
        {
            Container.BindInstance(config);
            Container.BindLarjeAdsProvider<AdMobAdsService>();
        }
    }
}
