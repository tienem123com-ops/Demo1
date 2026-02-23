using UnityEngine;

public class CameraLook : MonoBehaviour
{
    [Header("Camera Control")]
    public Transform cameraTarget; // Cái Empty Object ngang vai Player
    public float sensitivity = 2f;
    private float xRotation;
    private float yRotation;

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked; // Khóa chuột để tránh lệch tâm
    }

    // Trong PlayerController.cs hoặc script điều khiển xoay
    void LateUpdate()
    {
        // Tính toán góc xoay dựa trên Mouse Input
        float mouseX = Input.GetAxis("Mouse X") * sensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * sensitivity;
        yRotation += mouseX;
        xRotation += mouseY;
        // Xoay Target để Camera bám theo
        cameraTarget.rotation = Quaternion.Euler(-xRotation, yRotation, 0);

        // Ép người Player xoay theo hướng ngang
        transform.rotation = Quaternion.Euler(0, yRotation, 0);
    }
}