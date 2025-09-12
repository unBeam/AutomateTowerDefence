using UnityEngine;

public interface IVFXManager
{
    void PlayEffect(string key, Transform target);
    void PlayEffect(string key, Transform target, IVFXParameters parameters);
    void PlayEffect(string key, Vector3 position);
}