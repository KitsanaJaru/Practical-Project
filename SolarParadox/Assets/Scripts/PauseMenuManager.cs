using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenuManager : MonoBehaviour
{
    [Header("UI Pause Menu")]
    public GameObject pauseMenuPanel; // ลาก Panel หน้าจอ Pause มาใส่
    
    [Header("Manager Settings")]
    public MainMenuManager mainMenuManager;
    public PlayerController player;   // ลาก Player มาใส่ (เพื่อล็อกไม่ให้แอบเดินตอนพอส)

    private bool isPaused = false;

    void Start()
    {
        // เริ่มด่านมา ต้องซ่อนหน้า Pause ไว้เสมอ
        if (pauseMenuPanel != null)
        {
            pauseMenuPanel.SetActive(false);
        }
    }

    void Update()
    {
        // ตรวจจับว่าผู้เล่นกดปุ่ม ESC หรือไม่
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isPaused)
            {
                ResumeGame(); // ถ้ามัน Pause อยู่ -> ให้กลับไปเล่นต่อ
            }
            else
            {
                PauseGame(); // ถ้ามันเล่นอยู่ -> ให้ Pause
            }
        }
    }

    // ฟังก์ชันสั่งหยุดเกม
    public void PauseGame()
    {
        pauseMenuPanel.SetActive(true); // โชว์หน้าต่าง UI Pause
        Time.timeScale = 0f;            // แช่แข็งเวลาในเกม
        isPaused = true;
        
        // กันเหนียว: ปิดสคริปต์ Player ไม่ให้รับคำสั่งเดินตอนหน้าจอพอสบังอยู่
        if (player != null) player.enabled = false; 
    }

    // ฟังก์ชันสั่งเล่นต่อ (ให้ปุ่ม "Resume" เรียกใช้)
    public void ResumeGame()
    {
        pauseMenuPanel.SetActive(false); // ซ่อนหน้าต่าง UI Pause
        Time.timeScale = 1f;             // ให้เวลาเดินปกติ 100%
        isPaused = false;

        // เปิดให้ Player กลับมาขยับได้
        if (player != null) player.enabled = true; 
    }

    // ฟังก์ชันกลับหน้าแรก (ให้ปุ่ม "Main Menu" เรียกใช้)
    public void GoToMainMenu()
    {

        Time.timeScale = 1f; 

        pauseMenuPanel.SetActive(false);
        
        if (mainMenuManager != null)
        {
            mainMenuManager.ReturnToMainMenuSequence();
        }
    }
}