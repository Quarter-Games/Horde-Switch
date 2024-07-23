using UnityEngine;

public static class VFXManager
{
    public static void PlayVFX(GameObject vfxPrefab, Vector3 position, Quaternion rotation, Transform parent = null)
    {
        if (vfxPrefab == null) return;
        GameObject vfx = GameObject.Instantiate(vfxPrefab, position, rotation);
        if (parent != null)
        {
            vfx.transform.SetParent(parent);
        }
    }
}
