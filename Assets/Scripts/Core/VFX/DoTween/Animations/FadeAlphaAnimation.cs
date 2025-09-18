using DG.Tweening;
using UnityEngine;

[CreateAssetMenu(fileName = "FadeAlphaAnimation", menuName = "Configs/VFX/Animations/Fade Alpha")]
public class FadeAlphaAnimation : VFXAnimation
{
    [SerializeField] private float _endAlpha = 0f;
    [SerializeField] private float _duration = 0.5f;

    public override Sequence CreateSequence(Transform target, IVFXParameters parameters = null)
    {
        var seq = DOTween.Sequence();
        var cg = target.GetComponent<CanvasGroup>();
        if (cg != null)
            seq.Append(cg.DOFade(_endAlpha, _duration));
        return seq;
    }
}