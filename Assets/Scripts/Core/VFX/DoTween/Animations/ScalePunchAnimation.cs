using DG.Tweening;
using UnityEngine;

[CreateAssetMenu(fileName = "ScalePunchAnimation", menuName = "Configs/VFX/Animations/Scale Punch")]
public class ScalePunchAnimation : VFXAnimation
{
    [SerializeField]
    private float _duration = 0.5f;

    [SerializeField]
    private Vector3 _punch = Vector3.one * 0.2f;

    [SerializeField]
    private int _vibrato = 5;

    [SerializeField]
    private float _elasticity = 1f;

    public override Sequence CreateSequence(Transform target, IVFXParameters parameters = null)
    {
        return DOTween.Sequence()
            .Append(target.DOPunchScale(_punch, _duration, _vibrato, _elasticity));
    }
}