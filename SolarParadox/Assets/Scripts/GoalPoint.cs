using UnityEngine;

public class GoalPoint : MonoBehaviour
{
    public bool isComplete = false;

    // ฟังก์ชันนี้ของ Unity จะทำงานอัตโนมัติเมื่อมีอะไรมาชน/เหยียบทับ
    private void OnTriggerEnter2D(Collider2D other)
    {
        // ถ้าคนที่มาเหยียบคือ Player
        if (other.CompareTag("Player"))
        {
            isComplete = true;
            
        }
    }

}