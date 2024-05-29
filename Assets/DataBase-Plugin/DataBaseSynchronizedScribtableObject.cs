using UnityEngine;

abstract public class DataBaseSynchronizedScribtableObject : ScriptableObject
{
#if UNITY_EDITOR
    public abstract void Pull();
#endif
}
