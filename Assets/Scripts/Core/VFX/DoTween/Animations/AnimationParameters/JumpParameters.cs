using UnityEngine;

public struct JumpParams : IVFXParameters
{
    public float? JumpPower;
    public int? NumJumps;
    public float? Duration;
    public Vector3? EndOffset;
    public bool? IsRelative;
}