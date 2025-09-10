using UnityEngine;

[ConfigSection("PlayerMove")]
[CreateAssetMenu(fileName = "PlayerMoveConfig", menuName = "Configs/PlayerMove")]
public class PlayerMoveConfigSO : LiveConfigSO
{
    [SerializeField, Min(0f)] private float _moveSpeed = 5f;
    [SerializeField] private string _version = "local";

    public float MoveSpeed => _moveSpeed;
    public string Version  => _version;
}