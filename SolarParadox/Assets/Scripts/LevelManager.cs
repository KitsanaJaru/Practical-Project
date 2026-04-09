using UnityEngine;
using UnityEngine.SceneManagement; // จำเป็นสำหรับการรีเซ็ตและเปลี่ยนด่าน

public class LevelManager : MonoBehaviour
{
    // ลากจุด Goal ทั้งหมดในด่านมาใส่ใน Array นี้ที่ Inspector
    public GoalPoint[] allGoals;
    private bool levelFinished = false;

    void Update()
    {
        // 1. ระบบรีเซ็ต: กด R เพื่อเริ่มด่านใหม่ (ยัดไว้ตรงนี้เลย เช็คได้ตลอดเวลา)
        if (Input.GetKeyDown(KeyCode.R))
        {
            ResetLevel();
        }

        if (levelFinished) return;

        // 2. ระบบชนะ: เช็คเป้าหมาย
        if (CheckAllGoals())
        {
            levelFinished = true;
            
            // หน่วงเวลา 1 วินาทีก่อนเปลี่ยนด่าน จะได้ไม่ดูตัดฉับเกินไป
            Invoke("LoadNextLevel", 1f); 
        }
    }

    bool CheckAllGoals()
    {
        if (allGoals.Length == 0) return false;

        foreach (GoalPoint goal in allGoals)
        {
            if (!goal.isComplete) return false;
        }

        return true; 
    }

    // ฟังก์ชันรีเซ็ตด่าน (โหลด Scene เดิมซ้ำ)
    void ResetLevel()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    // ฟังก์ชันเปลี่ยนไปด่านถัดไป
    void LoadNextLevel()
    {
        int nextSceneIndex = SceneManager.GetActiveScene().buildIndex + 1;
        
        // เช็คว่าใน Build Settings มีด่านต่อไปให้โหลดไหม
        if (nextSceneIndex < SceneManager.sceneCountInBuildSettings)
        {
            SceneManager.LoadScene(nextSceneIndex);
        }
        else
        {
            Debug.Log("จบเกมแล้ว! ไม่มีด่านต่อไปแล้ว");
        }
    }
}