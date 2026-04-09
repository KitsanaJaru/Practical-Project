using UnityEngine;

public class PressurePlate : MonoBehaviour
{
    public bool isPressed = false;
    public Color activeColor = Color.green; // สีตอนโดนทับ
    public Color inactiveColor = Color.red; // สีปกติ
    
    private SpriteRenderer sr;

    void Start()
    {
        sr = GetComponent<SpriteRenderer>();
        sr.color = inactiveColor;
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        // เช็คตำแหน่งว่าทับกันสนิทไหม (Grid Snap)
        if (Vector2.Distance(transform.position, other.transform.position) < 0.1f)
        {
            if (!isPressed)
            {
                isPressed = true;
                sr.color = activeColor;
                Debug.Log("ปุ่มถูกกด");
            }
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        isPressed = false;
        sr.color = inactiveColor;
    }
}