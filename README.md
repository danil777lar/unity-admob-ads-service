# Larje Ads AdMob

`com.larje.ads-admob`: Google AdMob provider for the `IAdsService` from `com.larje.core`: anchored adaptive banner, interstitial with a cooldown, rewarded ads as `UniTask<bool>`, app open ads on start and on resume, retrying loads and the core no-ads mode.

## Install

UPM cannot resolve git dependencies, so add core, UniTask and this package to `Packages/manifest.json`:

```json
"com.cysharp.unitask": "https://github.com/Cysharp/UniTask.git?path=src/UniTask/Assets/Plugins/UniTask#2.5.10",
"com.larje.core": "https://github.com/danil777lar/unity-gameplay-core-services.git",
"com.larje.zenject": "https://github.com/danil777lar/unity-zenject-upm.git",
"com.larje.ads-admob": "https://github.com/danil777lar/unity-admob-ads-service.git"
```

Runtime code compiles only when `LARJE_ADMOB` is defined, so the package is safe to install before the SDK. The Zenject installer additionally needs `com.larje.zenject` (`LARJE_ZENJECT`).

### SDK

Google Mobile Ads Unity plugin 9+ (verified with 11.6.0). Either:

- UPM through OpenUPM: add a scoped registry `https://package.openupm.com` with scope `com.google.ads.mobile` and the dependency `"com.google.ads.mobile": "11.6.0"`. `com.unity.external-dependency-manager` comes from the Unity registry.
- Or import the `.unitypackage` into `Assets`. The `GoogleMobileAds` DLL is detected and `LARJE_ADMOB` is added to the scripting defines automatically.

Then set the AdMob app ids in `Assets > Google Mobile Ads > Settings`. Android builds crash on start without an app id.

## Setup

1. Create `AdMobAdsConfig` (`Create > Larje > Ads > AdMob Ads Config`). It starts with Google test ad unit ids for Android and iOS; replace them before release.
2. Create `AdMobAdsInstaller` (`Create > Larje > Ads > AdMob Ads Installer`), assign the config and add it to the `ProjectContext` scriptable object installers next to `LarjeCoreInstaller`. The order does not matter: the provider replaces the simulated `IAdsService`.
3. Use `IAdsService` from game code as usual.

## Config

| Group | Fields |
|---|---|
| Ad Units | `android` and `ios` ad unit ids (banner, interstitial, rewarded, app open). Empty id disables that format. |
| Interstitial | `interstitialCooldown` seconds after any interstitial or rewarded ad closes. |
| Banner | `useBanner`, `bannerPosition`. The banner is hidden while no-ads mode is on and shown again when it is turned off. |
| App Open | `showAppOpenOnStart` (waits up to `appOpenStartTimeout` for the ad to load), `showAppOpenOnResume`. App open ads expire after 4 hours and are reloaded. |
| Loading | `retryDelay` between failed loads, `disableInEditor`. |

## Behavior

- `ShowRewardedAsync` returns `true` only when the reward was earned; cancelling the token throws `OperationCanceledException`.
- Only one fullscreen ad is shown at a time; app open ads do not reset the interstitial cooldown.
- Ad events are raised on the Unity main thread (`MobileAds.RaiseAdEventsOnUnityMainThread`).
- `BannerHeight` is in pixels (`BannerView.GetHeightInPixels`), 0 while the banner is hidden or not loaded.
- The placement argument is accepted for API compatibility and ignored.

## Status

Compiles against Google Mobile Ads 11.6.0 in Unity 6000.4.9f1 (Standalone target). Not tested on a device yet.
