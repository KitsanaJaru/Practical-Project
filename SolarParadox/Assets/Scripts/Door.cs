using UnityEngine;

public class Door : MonoBehaviour
{
    // ลากปุ่มที่ต้องใช้เปิดประตูนี้มาใส่ (รองรับหลายปุ่มก็ได้)
    public PressurePlate[] requiredPlates;
    
    private BoxCollider2D col;
    private SpriteRenderer sr;

    void Start()
    {
        col = GetComponent<BoxCollider2D>();
        sr = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        if (CheckAllPlates())
        {
            OpenDoor();
        }
        else
        {
            CloseDoor();
        }
    }

    bool CheckAllPlates()
    {
        foreach (var plate in requiredPlates)
        {
            if (!plate.isPressed) return false;
        }
        return true;
    }

    void OpenDoor()
    {
        col.enabled = false; // ปิดคอลไลเดอร์ให้เดินผ่านได้
        sr.enabled = false;  // ซ่อนตัวประตู
    }

    void CloseDoor()
    {
        col.enabled = true;
        sr.enabled = true;
    }
}