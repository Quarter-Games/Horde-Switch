using System.Collections;
using UnityEngine;

public static class VFXManager
{
    public static void PlayVFX(ParticleSystem vfxPrefab, Vector3 position, Quaternion rotation, Transform parent = null)
    {
        if (vfxPrefab == null) return;
        ParticleSystem vfx = GameObject.Instantiate(vfxPrefab, position, rotation);
        if (parent != null)
        {
            vfx.transform.SetParent(parent);
        }
        DestroyVFX(vfx);
    }
    private static IEnumerator DestroyVFX(ParticleSystem vfx)
    {
        yield return new WaitForSeconds(vfx.main.duration);
        Debug.Log("Destroying VFX");

    }
}
