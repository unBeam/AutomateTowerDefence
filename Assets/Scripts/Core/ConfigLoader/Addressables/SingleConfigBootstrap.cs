// Assets/Scripts/Core/ConfigLoader/Addressables/SingleConfigBootstrap.cs
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;

public class SingleConfigBootstrap : MonoBehaviour
{
    [Header("Addressables Keys")]
    [SerializeField] private string _playerMoveKey = "cfg.player_move";
    [SerializeField] private string _pricesKey     = "cfg.prices";
    [SerializeField] private string _audioCfgKey   = "cfg.audio";

    [Header("Remote")]
    [SerializeField] private string _configUrl = "https://testconfigs-c3b83.web.app/configs/Config.json";

    [Header("UI")]
    [SerializeField] private LoadingScreenStopwatch _loading;

    private IAddressablesLoader _addr;
    private IRemoteTextProvider _remote;

    private PlayerMoveConfigSO _pm;
    private PricesConfigSO _prices;
    private AudioConfigSO _audioCfg;

    private AudioRegistry _audioReg;
    private AudioHub _audioHub;

    private async void Awake()
    {
        if (_loading != null) _loading.ShowAndStart();

        Debug.Log($"[BOOT] platform={Application.platform} dev={Debug.isDebugBuild} internetAccess=Require");

        _addr   = new AddressablesLoader();
        _remote = new UnityWebRequestTextProvider(10);

        Debug.Log("[BOOT] Addressables.InitializeAsync...");
        await Addressables.InitializeAsync().Task;
        Debug.Log("[BOOT] Addressables.InitializeAsync OK");

        await LoadScriptables();
        RegisterInConfigHub();

        _audioReg = new AudioRegistry(_addr);
        if (_audioCfg != null)
        {
            await _audioReg.LoadDefaults(_audioCfg);
            Debug.Log("[BOOT][AUDIO] defaults loaded from AudioConfigSO");
        }

        _audioHub = FindObjectOfType<AudioHub>(true);
        if (_audioHub != null)
        {
            _audioHub.Construct(_audioReg);
            Debug.Log("[BOOT][AUDIO] AudioHub constructed with AudioRegistry");
        }
        else
        {
            Debug.LogWarning("[BOOT][AUDIO] AudioHub not found in scene.");
        }

        var flat = await FetchFlatJson(_configUrl);
        if (flat != null)
        {
            ConfigAutoApplier.Apply(flat, GetTargets(), "Version");

            await _audioReg.Apply(flat);
            Debug.Log("[BOOT][AUDIO] overrides applied from flat JSON");
        }
        else
        {
            Debug.LogWarning("[BOOT] flat == null (stay on defaults)");
        }

        Debug.Log($"[BOOT][RESULT] PM v={_pm?.Version} speed={_pm?.MoveSpeed}");
        Debug.Log($"[BOOT][RESULT] PR v={_prices?.Version} coin={_prices?.CoinPrice} gem={_prices?.GemPrice}");
        ConfigInitGate.MarkReady();

        if (_loading != null)
        {
            float s = _loading.StopAndHide();
            Debug.Log($"[Loading] init finished in {s:0.000}s");
        }
    }

    private async UniTask LoadScriptables()
    {
        Debug.Log("[BOOT] Loading SO via Addressables...");

        _pm = await _addr.Load<PlayerMoveConfigSO>(_playerMoveKey);
        if (_pm == null) Debug.LogError($"[BOOT] Failed to load PlayerMoveConfigSO by key '{_playerMoveKey}'");

        _prices = await _addr.Load<PricesConfigSO>(_pricesKey);
        if (_prices == null) Debug.LogError($"[BOOT] Failed to load PricesConfigSO by key '{_pricesKey}'");

        if (!string.IsNullOrEmpty(_audioCfgKey))
        {
            _audioCfg = await _addr.Load<AudioConfigSO>(_audioCfgKey);
            if (_audioCfg == null)
                Debug.LogWarning($"[BOOT] AudioConfigSO not found by key '{_audioCfgKey}' (optional).");
        }

        Debug.Log($"[BOOT] Loaded: pm={_pm?.name}/{_pm?.GetInstanceID()} prices={_prices?.name}/{_prices?.GetInstanceID()} audioCfg={_audioCfg?.name}/{_audioCfg?.GetInstanceID()}");
    }

    private void RegisterInConfigHub()
    {
        if (_pm != null)     ConfigHub.Set("PlayerMove", _pm);
        if (_prices != null) ConfigHub.Set("Prices", _prices);
        if (_audioCfg != null) ConfigHub.Set("Audio", _audioCfg);
    }

    private IEnumerable<LiveConfigSO> GetTargets()
    {
        if (_pm != null)     yield return _pm;
        if (_prices != null) yield return _prices;
      
    }

    private async UniTask<Dictionary<string, object>> FetchFlatJson(string url)
    {
        if (string.IsNullOrEmpty(url)) return null;

        string busted = UrlUtil.WithCacheBuster(url);
        Debug.Log("[BOOT] GET " + busted);

        string txt = await _remote.Fetch(busted);
        if (string.IsNullOrEmpty(txt))
        {
            Debug.LogWarning("[BOOT] fetch empty: " + busted);
            return null;
        }

        object parsed = MiniJson.Deserialize(txt);
        var dict = parsed as Dictionary<string, object>;
        if (dict != null)
        {
            Debug.Log($"[BOOT] flat entries = {dict.Count}");
            int n = 0;
            foreach (var kv in dict)
            {
                if (n++ < 20) Debug.Log($"[BOOT]  {kv.Key} = {kv.Value}");
                else break;
            }
        }
        else
        {
            Debug.LogWarning("[BOOT] invalid JSON root (not object).");
        }

        return dict;
    }
}
