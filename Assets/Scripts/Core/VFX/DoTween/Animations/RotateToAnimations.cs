using DG.Tweening;
using UnityEngine;

[CreateAssetMenu(fileName = "RotateToAnimation", menuName = "Configs/VFX/Animations/Rotate To")]
public class RotateToAnimation : VFXAnimation
{
    [SerializeField] private Vector3 _endEulerAngles;
    [SerializeField] private float _duration = 1f;

    public override Sequence CreateSequence(Transform target, IVFXParameters parameters = null)
    {
        return DOTween.Sequence()
            .Append(target.DORotate(_endEulerAngles, _duration).SetEase(Ease.InOutQuad));
    }
}