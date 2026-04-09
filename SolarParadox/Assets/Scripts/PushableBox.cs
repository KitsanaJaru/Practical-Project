using UnityEngine;
using System.Collections;

public class PushableBox : MonoBehaviour
{
    public LayerMask obstacleLayer;
    public float moveSpeed = 0.2f;
    private bool isMoving = false;

    public bool TryPush(Vector3 direction)
    {
        if (isMoving) return false;
        Vector3 targetPos = transform.position + direction;

        // 1. เช็คว่าดันไปชนทางออก (Exit) ไหม
        Collider2D hitExit = Physics2D.OverlapCircle(targetPos, 0.1f);
        if (hitExit != null && hitExit.CompareTag("Exit"))
        {
            StartCoroutine(ExitSubworldRoutine(direction));
            return true;
        }

        // 2. เช็คกำแพง หรือ กล่องใบอื่น
        Collider2D obstacle = Physics2D.OverlapCircle(targetPos, 0.1f, obstacleLayer);
        
        if (obstacle == null)
        {
            // ถ้าทางว่าง ดันไปเลย
            StartCoroutine(MoveRoutine(targetPos));
            return true;
        }
        else if (obstacle.CompareTag("Box"))
        {
            PushableBox nextBox = obstacle.GetComponent<PushableBox>();
            SubworldBox subworldBehind = obstacle.GetComponent<SubworldBox>();
            
            // 1. ลองสั่งให้กล่องข้างหน้าเดินก่อน
            if (nextBox != null && nextBox.TryPush(direction)) 
            {
                // ถ้ามันเดินได้ แปลว่าทางโล่ง เราก็แค่เดินตามตูดมันไป
                StartCoroutine(MoveRoutine(targetPos));
                return true;
            }
            // 2. ถ้ามันเดินไม่ได้ (TryPush ส่งกลับมาเป็น false เพราะติดกำแพง) ค่อยเช็คว่ามุดได้ไหม
            else if (subworldBehind != null && subworldBehind.CanEnterFrom(direction))
            {
                StartCoroutine(EnterSubworldRoutine(subworldBehind, direction));
                return true;
            }
        }
        
        // ถ้าเป็นกำแพงเฉยๆ หรือดันไม่ไป+มุดไม่ได้ ก็ยืนนิ่งๆ
        return false;
    }

    private IEnumerator ExitSubworldRoutine(Vector3 direction)
    {
        isMoving = true;
        PlayerController player = FindFirstObjectByType<PlayerController>();
        Vector3 returnPos = player.GetReturnBoxPosition();

        if (returnPos == Vector3.zero) { isMoving = false; yield break; }

        float t = 0f;
        Vector3 startScale = transform.localScale;
        
        while (t < 1f)
        {
            t += Time.deltaTime / 0.2f;
            transform.localScale = Vector3.Lerp(startScale, Vector3.zero, t);
            yield return null;
        }

        transform.position = returnPos;
        Vector3 finalPosInMainWorld = returnPos + direction;
        Collider2D blocker = Physics2D.OverlapCircle(finalPosInMainWorld, 0.1f, obstacleLayer);
        
        if (blocker != null && blocker.CompareTag("Box")) 
        {
            blocker.GetComponent<PushableBox>().TryPush(direction);
        }

        t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / 0.15f;
            transform.localScale = Vector3.Lerp(Vector3.zero, startScale, t);
            yield return null;
        }
        transform.localScale = startScale;

        yield return StartCoroutine(MoveRoutine(finalPosInMainWorld));
        isMoving = false;
    }

    public IEnumerator EnterSubworldRoutine(SubworldBox subworld, Vector3 direction)
    {
        isMoving = true;
        Vector3 startScale = transform.localScale;
        Vector3 startPos = transform.position;
        Vector3 centerPos = subworld.transform.position;
        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / 0.2f;
            transform.position = Vector3.Lerp(startPos, centerPos, t);
            transform.localScale = Vector3.Lerp(startScale, Vector3.zero, t);
            yield return null;
        }
        transform.position = subworld.GetEdgeSpawnPosition(direction);
        transform.localScale = startScale;
        
        yield return StartCoroutine(MoveRoutine(transform.position + direction));
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
}