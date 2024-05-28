using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(DataBaseSynchronizedScribtableObject),true)]
public class DataBaseSynchronizedScribtableObject_Editor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        if (GUILayout.Button("Pull"))
        {
            DataBaseSynchronizedScribtableObject myScript = (DataBaseSynchronizedScribtableObject)target;
            myScript.Pull();
        }
    }
}
