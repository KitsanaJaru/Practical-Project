using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PlayerController : MonoBehaviour
{
    [Header("Settings")]
    public float moveSpeed = 0.2f;
    public float zoomSpeed = 0.3f;
    public LayerMask obstacleLayer;
    public Camera mainCamera;

    [Header("Animations")]
    public float frameRate = 0.03f; // ความเร็วในการสับขา (วิ/เฟรม)
    public Sprite[] walkUpFrames;   //
    public Sprite[] walkDownFrames; //
    public Sprite[] walkSideFrames;

    private Sprite[] currentAnim; // อนิเมชั่นที่กำลังเล่นอยู่
    private int currentFrame = 0; // ลำดับภาพปัจจุบัน
    private float animTimer = 0f; // ตัวนับเวลาสลับภาพ

    private SpriteRenderer sr;
    private bool isMoving = false;
    private Stack<WorldState> worldHistory = new Stack<WorldState>();

    private struct WorldState
    {
        public Vector3 cameraReturnPos;
        public float cameraReturnSize;
        public Vector3 boxEntryPos; 
    }

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        // ตั้งค่าเริ่มต้นให้หันหน้าลง
        if (walkDownFrames != null && walkDownFrames.Length > 0)
        {
            currentAnim = walkDownFrames;
            sr.sprite = currentAnim[0];
        }
    }

    public Vector3 GetReturnBoxPosition()
    {
        if (worldHistory.Count > 0) return worldHistory.Peek().boxEntryPos;
        return Vector3.zero;
    }

    void Update()
    {
        // ระบบเล่นอนิเมชั่น (ทำทุกๆ เฟรมของเกม)
        if (isMoving && currentAnim != null && currentAnim.Length > 0)
        {
            // ถ้ายำลังเดินอยู่ ให้สับขา
            animTimer += Time.deltaTime;
            if (animTimer >= frameRate)
            {
                animTimer -= frameRate; // รีเซ็ตเวลา
                currentFrame = (currentFrame + 1) % currentAnim.Length; // เลื่อนไปรูปถัดไป วนลูป
                sr.sprite = currentAnim[currentFrame]; // แสดงผลรูปใหม่
            }
        }
        else if (!isMoving && currentAnim != null && currentAnim.Length > 0)
        {
            // ถ้ายืนนิ่งๆ ให้กลับไปท่ายืน (รูปแรกสุดในอาเรย์เสมอ)
            sr.sprite = currentAnim[0];
            currentFrame = 0;
            animTimer = 0f;
        }

        // ระบบรับคำสั่งเดิน
        if (isMoving) return;
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");
        Vector3 dir = Vector3.zero;
        if (h != 0) dir = new Vector3(h, 0, 0);
        else if (v != 0) dir = new Vector3(0, v, 0);
        if (dir != Vector3.zero) TryMove(dir);

        // ระบบ Undo (กด Z เพื่อย้อนกลับ 1 ก้าว)
        if (Input.GetKeyDown(KeyCode.Z)) PerformUndo();
    }

private void TryMove(Vector3 direction)
    {
        UpdateCharacterVisual(direction);

        Vector3 targetPos = transform.position + direction;
        Collider2D exitHit = Physics2D.OverlapCircle(targetPos, 0.1f);

        if (exitHit != null && exitHit.CompareTag("Exit"))
        {
            SaveSnapshot();
            StartCoroutine(ExitSubworldRoutine(direction));
            return;
        }

        Collider2D hit = Physics2D.OverlapCircle(targetPos, 0.1f, obstacleLayer);
        if (hit == null)
        {
            SaveSnapshot();
            StartCoroutine(MoveRoutine(targetPos));
        }

        else if (hit.CompareTag("Box"))
        {
            PushableBox box = hit.GetComponent<PushableBox>();
            SubworldBox subworld = hit.GetComponent<SubworldBox>();

            if (box != null && box.TryPush(direction))
            {
                SaveSnapshot();
                StartCoroutine(MoveRoutine(targetPos));
            }
            else if (subworld != null && subworld.CanEnterFrom(direction))
            {
                SaveSnapshot();
                StartCoroutine(EnterSubworldRoutine(subworld, direction));
            }
            else
            {
                if (CameraShake.Instance != null) CameraShake.Instance.TriggerShake();
            }
        }
        else
        {
            if (CameraShake.Instance != null) CameraShake.Instance.TriggerShake();
        }
    }

    private void UpdateCharacterVisual(Vector3 moveDir)
    {
        if (sr == null) return;

        if (moveDir == Vector3.up)
        {
            currentAnim = walkUpFrames;
            sr.flipX = false;
        }
        else if (moveDir == Vector3.down)
        {
            currentAnim = walkDownFrames;
            sr.flipX = false;
        }
        else if (moveDir == Vector3.right)
        {
            currentAnim = walkSideFrames;
            sr.flipX = false;
        }
        else if (moveDir == Vector3.left)
        {
            currentAnim = walkSideFrames;
            sr.flipX = true; // เดินซ้าย พลิกรูปหันข้างเอา
        }

        // บังคับให้เปลี่ยนเป็นรูปแรกของทิศนั้น "ทันที"
        if (currentAnim != null && currentAnim.Length > 0)
        {
            currentFrame = 0; // รีเซ็ตเฟรมกลับไปรูปแรก
            animTimer = 0f;   // รีเซ็ตเวลาอนิเมชั่น
            sr.sprite = currentAnim[0]; // แปะรูปลงไปเดี๋ยวนั้นเลย จะได้ไม่กระพริบ
        }
    }

    private IEnumerator EnterSubworldRoutine(SubworldBox subworld, Vector3 direction)
    {
        isMoving = true;
        worldHistory.Push(new WorldState {
            cameraReturnPos = mainCamera.transform.position,
            cameraReturnSize = mainCamera.orthographicSize,
            boxEntryPos = subworld.transform.position 
        });

        float t = 0f;
        Vector3 startScale = transform.localScale;
        Vector3 startPos = transform.position;
        Vector3 centerPos = subworld.transform.position;
        Vector3 startCamPos = mainCamera.transform.position;

        while (t < 1f)
        {
            t += Time.deltaTime / zoomSpeed;
            transform.position = Vector3.Lerp(startPos, centerPos, t);
            transform.localScale = Vector3.Lerp(startScale, Vector3.zero, t);
            mainCamera.transform.position = Vector3.Lerp(startCamPos, new Vector3(centerPos.x, centerPos.y, -10f), t);
            mainCamera.orthographicSize = Mathf.Lerp(worldHistory.Peek().cameraReturnSize, 0.05f, t);
            yield return null;
        }

        Vector3 spawnPos = subworld.GetEdgeSpawnPosition(direction);
        Vector3 firstStepInside = spawnPos + direction;

        Collider2D boxAtSpawn = Physics2D.OverlapCircle(spawnPos, 0.1f, obstacleLayer);
        if (boxAtSpawn != null && boxAtSpawn.CompareTag("Box")) boxAtSpawn.GetComponent<PushableBox>().TryPush(direction);
        
        Collider2D boxAtStep = Physics2D.OverlapCircle(firstStepInside, 0.1f, obstacleLayer);
        if (boxAtStep != null && boxAtStep.CompareTag("Box")) boxAtStep.GetComponent<PushableBox>().TryPush(direction);

        transform.position = spawnPos;
        transform.localScale = startScale;
        mainCamera.transform.position = new Vector3(subworld.cameraPosition.x, subworld.cameraPosition.y, -10f);
        mainCamera.orthographicSize = subworld.targetCameraSize;

        yield return StartCoroutine(MoveRoutine(firstStepInside));
        isMoving = false;
    }

    private IEnumerator ExitSubworldRoutine(Vector3 exitDirection)
    {
        isMoving = true;
        WorldState prev = worldHistory.Pop();

        // 1. ตัดกล้องไปโลกข้างนอกแบบเต็มตา "ทันที" ไม่ต้องรอซูมแล้ว
        mainCamera.transform.position = prev.cameraReturnPos;
        mainCamera.orthographicSize = prev.cameraReturnSize;
        
        // เซ็ตตัวละครให้ไปอยู่โลกหลัก แต่ย่อส่วนเหลือ 0 ไว้ก่อน
        Vector3 originalScale = transform.localScale;
        transform.localScale = Vector3.zero;
        transform.position = prev.boxEntryPos;

        // 2. อนิเมชั่นขยายตัวละครป๊อปอัปออกจากกล่อง (แต่ตาเรามองเห็นทั้งฉากแล้ว)
        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / 0.15f;
            transform.localScale = Vector3.Lerp(Vector3.zero, originalScale, t);
            yield return null;
        }
        transform.localScale = originalScale;

        // 3. เช็คพื้นที่หน้ากล่องก่อนก้าวเท้าออกมา
        Vector3 exitStepPos = prev.boxEntryPos + exitDirection;
        Collider2D blocker = Physics2D.OverlapCircle(exitStepPos, 0.1f, obstacleLayer);
        
        // ถ้านายดันกล่องออกมาก่อนหน้านี้ มันจะขวางอยู่ ก็ถีบมันไปเลย
        if (blocker != null && blocker.CompareTag("Box"))
        {
            blocker.GetComponent<PushableBox>().TryPush(exitDirection);
        }

        // 4. ก้าวเท้าพ้นกล่องออกมา 1 ช่อง
        yield return StartCoroutine(MoveRoutine(exitStepPos));
        
        isMoving = false;
    }

    private IEnumerator MoveRoutine(Vector3 targetPos)
    {
        isMoving = true;
        Vector3 startPos = transform.position;
        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / moveSpeed;
            transform.position = Vector3.Lerp(startPos, targetPos, t);
            yield return null;
        }
        transform.position = targetPos;
        isMoving = false;
    }

    // โครงสร้างข้อมูลสำหรับเก็บสถานะ 1 ก้าว
    private struct GameSnapshot
    {
        public Vector3 playerPos;
        public Vector3 playerScale;
        public List<Vector3> allBoxesPos; // ตำแหน่งกล่องทุกใบในฉาก
        public List<WorldState> historyCopy; // ประวัติมิติที่ซ้อนอยู่ (Copy ของ worldHistory)
        public float camSize;
        public Vector3 camPos;
    }

    // Stack สำหรับเก็บประวัติการเดิน (ย้อนได้หลายก้าวตามที่ต้องการ)
    private Stack<GameSnapshot> undoStack = new Stack<GameSnapshot>();

    private void SaveSnapshot()
    {
        // สร้าง Snapshot ใหม่
        GameSnapshot snapshot = new GameSnapshot();
        snapshot.playerPos = transform.position;
        snapshot.playerScale = transform.localScale;
        snapshot.camSize = mainCamera.orthographicSize;
        snapshot.camPos = mainCamera.transform.position;
        
        // ก๊อปปี้ประวัติมิติ (Stack) ออกมาเป็น List เพื่อบันทึก
        snapshot.historyCopy = new List<WorldState>(worldHistory.ToArray());

        // บันทึกตำแหน่งกล่องทุกใบที่มี Tag ว่า Box ในขณะนั้น
        snapshot.allBoxesPos = new List<Vector3>();
        GameObject[] boxes = GameObject.FindGameObjectsWithTag("Box");
        foreach (GameObject box in boxes)
        {
            snapshot.allBoxesPos.Add(box.transform.position);
        }

        // เก็บเข้า Stack ย้อนกลับ
        undoStack.Push(snapshot);
    }

    private void PerformUndo()
    {
        if (undoStack.Count == 0 || isMoving) return; // ถ้าไม่มีอะไรให้ย้อน หรือกำลังขยับอยู่ ให้ข้ามไป

        // ดึงข้อมูลล่าสุดออกมา
        GameSnapshot lastState = undoStack.Pop();

        // 1. คืนค่าผู้เล่น
        transform.position = lastState.playerPos;
        transform.localScale = lastState.playerScale;

        // 2. คืนค่ากล่อง (ต้องเรียงลำดับให้ตรงกับตอนเก็บ)
        GameObject[] boxes = GameObject.FindGameObjectsWithTag("Box");
        for (int i = 0; i < boxes.Length; i++)
        {
            if (i < lastState.allBoxesPos.Count)
                boxes[i].transform.position = lastState.allBoxesPos[i];
        }

        // 3. คืนค่ามิติ (ล้างอันเก่าแล้วใส่ประวัติที่บันทึกไว้กลับเข้าไป)
        worldHistory.Clear();
        // ต้องกลับลำดับ List ก่อน Push กลับเข้า Stack
        List<WorldState> reversedHistory = new List<WorldState>(lastState.historyCopy);
        reversedHistory.Reverse();
        foreach (var ws in reversedHistory) worldHistory.Push(ws);

        // 4. คืนค่ากล้อง
        mainCamera.orthographicSize = lastState.camSize;
        mainCamera.transform.position = lastState.camPos;
        
        Debug.Log("Undo สำเร็จ! เหลือประวัติอีก: " + undoStack.Count + " ก้าว");
    }
}