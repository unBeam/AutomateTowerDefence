using UnityEngine;

[ConfigSection("Prices")]
[CreateAssetMenu(fileName = "PricesConfig", menuName = "Configs/Prices")]
public class PricesConfigSO : LiveConfigSO
{
    [SerializeField, Min(0)] private int _coinPrice = 100;
    [SerializeField, Min(0)] private int _gemPrice  = 5;
    [SerializeField] private string _version = "local";

    public int CoinPrice => _coinPrice;
    public int GemPrice  => _gemPrice;
    public string Version => _version;
}