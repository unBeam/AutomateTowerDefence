using UnityEngine;

[CreateAssetMenu(fileName = "GameplayConfig", menuName = "Configs/Gameplay")]
public class GameplayConfigSO : ScriptableObject
{
    [Min(0f)]
    public float CubeSpeed = 5f;
    public string Version = "1.0.0";
}