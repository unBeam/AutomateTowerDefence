using DG.Tweening;
using UnityEngine;

[CreateAssetMenu(fileName = "MoveToAnimation", menuName = "Configs/VFX/Animations/Move To")]
public class MoveToAnimation : VFXAnimation
{
    [SerializeField] private Vector3 _endPosition;
    [SerializeField] private float _duration = 1f;
    [SerializeField] private bool _isRelative;

    public override Sequence CreateSequence(Transform target)
    {
        Vector3 dest = _isRelative
            ? target.position + _endPosition
            : _endPosition;
        return DOTween.Sequence()
            .Append(target.DOMove(dest, _duration).SetEase(Ease.InOutSine));
    }
}