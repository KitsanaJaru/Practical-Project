using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{
    [Header("UI Panels")]
    public GameObject mainMenuPanel;    // ลาก Panel ที่มีปุ่ม (เริ่ม, เลือกเลเวล, ออก) มาใส่
    public GameObject levelSelectPanel; // ลาก Panel ที่มีปุ่มด่าน 1, 2 มาใส่

    [Header("Camera & Zoom Settings")]
    public Camera mainCamera;
    public Transform targetPlanet;
    public float zoomSpeed = 2f;
    public float targetZoomSize = 2f;

    [Header("Game Start")]
    public PlayerController player;

    private Vector3 initialCameraPosition; // จดจำตำแหน่งกล้องตอนเมนู
    private float initialCameraSize;     // จดจำขนาดกล้องตอนเมนู

    void Start()
    {
        // 1. จดจำค่าเริ่มต้นของกล้องไว้ก่อน เพื่อซูมกลับมา
        initialCameraPosition = mainCamera.transform.position;
        initialCameraSize = mainCamera.orthographicSize;

        // เริ่มเกมมา ให้โชว์หน้าเมนูหลัก และซ่อนหน้าเลือกด่านเอาไว้ก่อน
        if (mainMenuPanel != null) mainMenuPanel.SetActive(true);
        if (levelSelectPanel != null) levelSelectPanel.SetActive(false);

        // ล็อกตัวละครไว้ก่อนตอนอยู่หน้าเมนู
        if (player != null) player.enabled = false; 

        Time.timeScale = 0f;
    }

    // ฟังก์ชันสำหรับผูกกับปุ่มต่างๆ (UI Buttons)
    // 1. ผูกกับปุ่ม "เริ่มเกม"
    public void StartGameSequence()
    {
        StartCoroutine(ZoomIntoPlanetRoutine());
    }

    // 2. ผูกกับปุ่ม "กลับเมนูหลัก"
    public void ReturnToMainMenuSequence()
    {
        StartCoroutine(ZoomOutToPlanetRoutine());
    }

    // 3. ผูกกับปุ่ม "เลือกเลเวล"
    public void OpenLevelSelect()
    {
        mainMenuPanel.SetActive(false);    // ซ่อนหน้าเมนูหลัก
        levelSelectPanel.SetActive(true);  // โชว์หน้าเลือกด่าน
    }

    // 4. ผูกกับปุ่ม "กลับ" ในหน้าเลือกด่าน
    public void CloseLevelSelect()
    {
        levelSelectPanel.SetActive(false); // ซ่อนหน้าเลือกด่าน
        mainMenuPanel.SetActive(true);     // โชว์หน้าเมนูหลัก
    }

    // 5. ผูกกับปุ่ม "เลือกเลเวล"
    public void SelectLevel(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }

    // 6. ผูกกับปุ่ม "ออกเกม"
    public void QuitGame()
    {
        Debug.Log("ออกจากเกมแล้ว!");
        Application.Quit();
    }

    // ระบบซูมกล้อง: ซูมเข้าไปที่ดาวเคราะห์และเริ่มเกม
    private IEnumerator ZoomIntoPlanetRoutine()
    {
        // ปิด UI ทุกหน้าทิ้งไปเลยตอนเริ่มซูม
        if (mainMenuPanel != null) mainMenuPanel.SetActive(false);
        if (levelSelectPanel != null) levelSelectPanel.SetActive(false);

        Time.timeScale = 1f;

        Vector3 startPos = mainCamera.transform.position;
        Vector3 endPos = new Vector3(targetPlanet.position.x, targetPlanet.position.y, -10f); 
        
        float startSize = mainCamera.orthographicSize;
        float t = 0f;

        while (t < 1f)
        {
            t += Time.unscaledDeltaTime * zoomSpeed;
            
            float smoothT = Mathf.SmoothStep(0f, 1f, t);
            mainCamera.transform.position = Vector3.Lerp(startPos, endPos, smoothT);
            mainCamera.orthographicSize = Mathf.Lerp(startSize, targetZoomSize, smoothT);
            
            yield return null;
        }

        if (player != null) player.enabled = true;

    }

    // ระบบซูมกล้อง: ซูมออกกลับมาเมนูหลัก
    private IEnumerator ZoomOutToPlanetRoutine()
    {
        // 1. ล็อกตัวละคร และซ่อน UI ทุกหน้า
        if (player != null) player.enabled = false;
        if (mainMenuPanel != null) mainMenuPanel.SetActive(false);
        if (levelSelectPanel != null) levelSelectPanel.SetActive(false);

        Time.timeScale = 1f;

        // 2. ตั้งค่าเริ่มต้น/สิ้นสุด (สลับฝั่งจาก ZoomIn)
        Vector3 startPos = mainCamera.transform.position;
        Vector3 endPos = initialCameraPosition; // กลับไปที่ตำแหน่งเริ่มต้นที่จดจำไว้

        float startSize = mainCamera.orthographicSize;
        float endSize = initialCameraSize;     // กลับไปที่ขนาดเริ่มต้นที่จดจำไว้
        
        float t = 0f;

        // 3. เริ่มพุ่ง
        while (t < 1f)
        {
            t += Time.unscaledDeltaTime * zoomSpeed;
            
            // ค่อยๆ เลื่อนกล้องและซูมเข้าไป (ใช้ SmoothStep ให้มันดูนุ่มนวลขึ้นตอนเริ่มและจบ)
            float smoothT = Mathf.SmoothStep(0f, 1f, t);
            mainCamera.transform.position = Vector3.Lerp(startPos, endPos, smoothT);
            mainCamera.orthographicSize = Mathf.Lerp(startSize, endSize, smoothT);
            
            yield return null;
        }

        Time.timeScale = 0f;

        // 4. ซูมเสร็จแล้ว เปิด UI หน้าเมนูหลักขึ้นมาใหม่
        if (mainMenuPanel != null) mainMenuPanel.SetActive(true);
        
    }
}