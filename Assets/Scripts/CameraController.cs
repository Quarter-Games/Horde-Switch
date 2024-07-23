using UnityEngine;

public class CameraController : MonoBehaviour
{
    [SerializeField] Camera _camera;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        SetCameraPosition();
    }

    [ContextMenu("Set Camera Position")]
    public void SetCameraPosition()
    {
        var ratio = 1/_camera.aspect;
        transform.localPosition = new Vector3(0, 0, -ratio*2f);

    }
}
