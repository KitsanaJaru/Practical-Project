using UnityEngine;
using System.Collections;

public class CameraShake : MonoBehaviour
{
    public static CameraShake Instance;
    
    private bool isShaking = false;
    private Vector3 originalPos;

    void Awake()
    {
        Instance = this;
    }

    public void TriggerShake(float duration = 0.1f, float magnitude = 0.05f)
    {
        // ถ้ากดรัวๆ ตอนที่มันยังสั่นไม่เสร็จ ให้ดึงกล้องกลับมาที่เดิมก่อนเลย
        if (isShaking)
        {
            StopAllCoroutines();
            transform.position = originalPos; 
        }
        
        StartCoroutine(DoShake(duration, magnitude));
    }

    private IEnumerator DoShake(float duration, float magnitude)
    {
        isShaking = true;
        // จำตำแหน่งก่อนเริ่มสั่นของจริง
        originalPos = transform.position; 
        float elapsed = 0.0f;

        while (elapsed < duration)
        {
            // สุ่มขยับแค่ X กับ Y (ไม่ยุ่งกับ Z เพราะเดี๋ยวกล้องพัง)
            float x = originalPos.x + Random.Range(-1f, 1f) * magnitude;
            float y = originalPos.y + Random.Range(-1f, 1f) * magnitude;

            transform.position = new Vector3(x, y, originalPos.z);
            elapsed += Time.deltaTime;
            
            yield return null;
        }

        // พอสั่นเสร็จ จับยัดกลับตำแหน่งเดิมเป๊ะๆ ทันที
        transform.position = originalPos;
        isShaking = false;
    }
}