using DG.Tweening;
using UnityEngine;

public abstract class VFXAnimation : ScriptableObject
{
    public abstract Sequence CreateSequence(Transform target, IVFXParameters parameters = null);
}