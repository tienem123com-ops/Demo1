using UnityEngine;

[System.Serializable]
public class ActionWindow
{
    public string actionName; // Ví dụ: "Hitbox", "ComboInput", "DashCancel"
    [Range(0, 1)] public float startTime; // Bắt đầu tại 20% animation
    [Range(0, 1)] public float endTime;   // Kết thúc tại 50% animation

    public bool IsInside(float normalizedTime) 
        => normalizedTime >= startTime && normalizedTime <= endTime;
}
