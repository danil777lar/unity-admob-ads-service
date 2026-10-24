using Larje.Core.Editor.Defines;
using UnityEditor;

namespace Larje.Ads.AdMob.Editor.Defines
{
    [InitializeOnLoad]
    internal static class AdMobDefines
    {
        static AdMobDefines()
        {
            OptionalDependencyDefines.Register("GoogleMobileAds", "LARJE_ADMOB");
        }
    }
}
