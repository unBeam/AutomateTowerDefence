using System;
using UnityEngine;
using Zenject;

public class ObjectAnimation : MonoBehaviour
{
    [Inject] private VFXManager _VFXManager;
    [SerializeField] private Transform _VFXPosition;

    private void Start()
    {
        _VFXManager.PlayEffect(VFXKeys.MoveTo, transform);
    }
}
