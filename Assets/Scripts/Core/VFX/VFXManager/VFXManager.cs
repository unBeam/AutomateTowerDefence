using DG.Tweening;
using UnityEngine;
using Zenject;

public class VFXManager : MonoBehaviour, IVFXManager
{
    [Inject] private VFXConfig _config;

    public void PlayEffect(string key, Transform target)
    {
        if (target == null) return;

        VFXEntry entry = _config.GetEntry(key);
        if (entry == null) return;

        if (entry.ParticlePrefab != null)
        {
            //Instantiate(entry.ParticlePrefab, target.position, target.rotation);
            Instantiate(entry.ParticlePrefab, target.position, entry.ParticlePrefab.transform.rotation);
        }

        if (entry.TweenAnimation != null)
        {
            entry.TweenAnimation.CreateSequence(target).Play();
        }
    }
    
    public void PlayEffect(string key, Transform target, IVFXParameters parameters)
    {
        if (target == null) return;

        VFXEntry entry = _config.GetEntry(key);
        if (entry == null) return;

        if (entry.ParticlePrefab != null)
        {
            Instantiate(entry.ParticlePrefab, target.position, target.rotation);
        }

        if (entry.TweenAnimation != null)
        {
            entry.TweenAnimation.CreateSequence(target, parameters).Play();
        }
    }

    public void PlayEffect(string key, Vector3 position)
    {
        VFXEntry entry = _config.GetEntry(key);
        if (entry == null) return;

        if (entry.ParticlePrefab != null)
        {
            Instantiate(entry.ParticlePrefab, position, Quaternion.identity);
        }
    }
}