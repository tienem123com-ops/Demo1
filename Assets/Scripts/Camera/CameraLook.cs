using UnityEngine;

public class CameraLook : MonoBehaviour
{
    [Header("Camera Control")]
    [SerializeField] private Transform _cameraTarget;
    [SerializeField] private float _sensitivity = 2f;

    private float _xRotation;
    private float _yRotation;

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
    }

    private void LateUpdate()
    {
        UpdateRotation();
    }

    private void UpdateRotation()
    {
        float mouseX = Input.GetAxis("Mouse X") * _sensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * _sensitivity;

        _yRotation += mouseX;
        _xRotation += mouseY;

        if (_cameraTarget != null)
            _cameraTarget.rotation = Quaternion.Euler(-_xRotation, _yRotation, 0);

        transform.rotation = Quaternion.Euler(0, _yRotation, 0);
    }
}