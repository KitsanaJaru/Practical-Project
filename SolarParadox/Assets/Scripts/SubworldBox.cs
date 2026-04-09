using UnityEngine;

public class SubworldBox : MonoBehaviour
{
    [Header("ขนาดด่านย่อย (กี่ช่อง)")]
    public int worldWidth = 5;
    public int worldHeight = 5;
    public Transform worldCenter;

    [Header("ทิศทางที่เข้าได้")]
    public bool enterFromUp = true;
    public bool enterFromDown = true;
    public bool enterFromLeft = true;
    public bool enterFromRight = true;

    [Header("ตำแหน่งกล้องในโลกย่อย")]
    public Vector3 cameraPosition;
    public float targetCameraSize = 5f;

    public bool CanEnterFrom(Vector3 direction)
    {
        if (direction == Vector3.up && enterFromDown) return true;
        if (direction == Vector3.down && enterFromUp) return true;
        if (direction == Vector3.left && enterFromRight) return true;
        if (direction == Vector3.right && enterFromLeft) return true;
        return false;
    }

    public Vector3 GetEdgeSpawnPosition(Vector3 entryDirection)
    {
        Vector3 center = worldCenter.position;
        // คำนวณขอบโดยใช้เลขจำนวนเต็มของขนาดโลก
        float offsetX = worldWidth / 2 + 1;
        float offsetY = worldHeight / 2 + 1;

        // ถ้าดันขวา (เข้าซ้าย) -> โผล่ขอบซ้าย (-offsetX)
        if (entryDirection == Vector3.right) return new Vector3(center.x - offsetX, center.y, 0);
        // ถ้าดันซ้าย (เข้าขวา) -> โผล่ขอบขวา (+offsetX)
        if (entryDirection == Vector3.left) return new Vector3(center.x + offsetX, center.y, 0);
        // ถ้าดันขึ้น (เข้าล่าง) -> โผล่ขอบล่าง (-offsetY)
        if (entryDirection == Vector3.up) return new Vector3(center.x, center.y - offsetY, 0);
        // ถ้าดันลง (เข้าบน) -> โผล่ขอบบน (+offsetY)
        if (entryDirection == Vector3.down) return new Vector3(center.x, center.y + offsetY, 0);

        return center;
    }
}