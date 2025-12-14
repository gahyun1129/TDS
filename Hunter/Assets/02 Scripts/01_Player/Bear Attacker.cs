using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using DG.Tweening;

public class BearAttacker : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject dotPrefab;
    [SerializeField] private Transform throwPoint;
    [SerializeField] private SpearProjectile projectilePrefab;
    [SerializeField] private LayerMask obstacleLayer;

    private List<GameObject> dots;
    private List<SpriteRenderer> dotRenderers;
    private List<Vector3> pathPoints = new List<Vector3>();

    private bool isAiming = false;
    private bool canShoot = false;
    
    private Vector3 startInputPos;
    private Vector3 currentInputPos;
    private Vector3 lastProcessedInputPos; // 마지막으로 처리된 마우스 위치 저장용
    private Vector3 currentSmoothedVelocity;
    
    // 목표로 하는 속도 (Lerp를 위해 따로 저장)
    private Vector3 targetVelocity; 

    private LevelData levelData;

    private void Start()
    {
        levelData = InGameManager.Instance.currentLevelData;
        RefreshDotPool(); 
        HideDots();
    }

    public void DoAiming()
    {
        if (IsPointerOverUI()) return;

        isAiming = true;
        canShoot = false;
        startInputPos = Input.mousePosition;
        lastProcessedInputPos = startInputPos; // 초기화
        currentSmoothedVelocity = Vector3.zero;
        targetVelocity = Vector3.zero; // 초기화
        
        RefreshDotPool(); 
        HideDots();
    }

    public void UpdateAiming()
    {
        if (!isAiming) return;

        currentInputPos = Input.mousePosition;

        // --- 1. 발사 취소 조건 체크 ---
        float dragDistance = Vector3.Distance(startInputPos, currentInputPos);
        if (dragDistance < levelData.projectile.minDragDistance)
        {
            HideDots();
            canShoot = false;
            // 거리가 너무 가까우면 아예 계산을 멈춤 (각도가 미친듯이 도는 것 방지)
            return; 
        }

        // --- 2. 입력 데드존 (떨림 방지 핵심) ---
        // 마우스가 'inputDeadzone' 픽셀보다 적게 움직였으면, 
        // 목표값을 갱신하지 않고 기존 목표를 유지합니다. (슉슉거림 방지)
        if (Vector3.Distance(currentInputPos, lastProcessedInputPos) > levelData.projectile.inputDeadzone)
        {
            // 의미 있게 움직였을 때만 목표치 재계산
            targetVelocity = CalculateClampedVelocity(startInputPos, currentInputPos);
            lastProcessedInputPos = currentInputPos; // 위치 갱신
        }

        // --- 3. 앵글/파워 불가 체크 ---
        if (targetVelocity == Vector3.zero)
        {
            HideDots();
            canShoot = false;
        }
        else
        {
            // --- 4. 부드럽게 따라가기 (Smoothing) ---
            // targetVelocity는 계단식으로 변하더라도, Lerp가 부드럽게 이어줍니다.
            // * throwData.aimSmoothing 값을 낮추면 더 묵직해집니다.
            currentSmoothedVelocity = Vector3.Lerp(currentSmoothedVelocity, targetVelocity, Time.unscaledDeltaTime * levelData.projectile.aimSmoothing);

            if (currentSmoothedVelocity.magnitude >= levelData.projectile.minPower)
            {
                SimulateTrajectory(currentSmoothedVelocity * levelData.projectile.initialSpeedBoost);
                canShoot = true;
            }
            else
            {
                HideDots();
                canShoot = false;
            }
        }
    }

    public void CancelAiming()
    {
        isAiming = false;
        canShoot = false;
        HideDots();
    }

    public bool DoFire()
    {
        if (!isAiming) return false;

        isAiming = false;
        HideDots();

        if (canShoot && currentSmoothedVelocity.magnitude >= levelData.projectile.minPower)
        {
            Vector3 finalVelocity = currentSmoothedVelocity * levelData.projectile.initialSpeedBoost;
            
            SpearProjectile spear = SpearPoolManager.Instance.GetSpear();

            spear.transform.position = throwPoint.position; 
            
            spear.Launch(throwPoint.position, finalVelocity, levelData.projectile.gravity, levelData.projectile.fallGravityMultiplier);
            
            canShoot = false;
            return true;
        }
        
        canShoot = false;
        return false;
    }

    private Vector3 CalculateClampedVelocity(Vector3 start, Vector3 current)
    {
        Vector3 direction = start - current;

        // // ★ Clamp 느낌: 드래그 거리를 제한
        direction = Vector3.ClampMagnitude(direction, levelData.projectile.maxDragRadius);

        float rawMagnitude = direction.magnitude * 0.01f * levelData.projectile.sensitivity;
        float finalPower = Mathf.Clamp(rawMagnitude * levelData.projectile.powerMultiplier, 0, levelData.projectile.maxPower);

        float angleRad = Mathf.Atan2(direction.y, direction.x);
        float angleDeg = angleRad * Mathf.Rad2Deg;

        if (direction.x < 0)
        {
            if (direction.y < 0) return Vector3.zero;
            else angleDeg = levelData.projectile.maxAngle;
        }

        angleDeg = Mathf.Clamp(angleDeg, levelData.projectile.minAngle, levelData.projectile.maxAngle);
        float clampedRad = angleDeg * Mathf.Deg2Rad;
        Vector3 finalDir = new Vector3(Mathf.Cos(clampedRad), Mathf.Sin(clampedRad), 0);

        return finalDir * finalPower;
    }

    // (SimulateTrajectory, RefreshDotPool 등 아래쪽 코드는 이전과 동일하므로 생략하지 않고 그대로 둡니다)
    private void SimulateTrajectory(Vector3 startVelocity)
    {
        RefreshDotPool();
        pathPoints.Clear();
        Vector3 tempPos = throwPoint.position;
        Vector3 tempVel = startVelocity;
        pathPoints.Add(tempPos);

        int simulationSteps = levelData.projectile.trajectoryDotCount + 2; 

        for (int i = 0; i < simulationSteps; i++)
        {
            float timeStep = levelData.projectile.dotSpacing;
            float currentGravity = (tempVel.y < 0) ? levelData.projectile.gravity * levelData.projectile.fallGravityMultiplier : levelData.projectile.gravity;

            tempVel.y += currentGravity * timeStep;
            Vector3 nextPos = tempPos + (tempVel * timeStep);

            RaycastHit2D hit = Physics2D.Linecast(tempPos, nextPos, obstacleLayer);
            
            if (hit.collider != null)
            {
                pathPoints.Add(hit.point);
                break; 
            }
            else
            {
                pathPoints.Add(nextPos);
                tempPos = nextPos;
            }
        }

        float progress = (Time.time * levelData.projectile.flowSpeed) % 1f;

        for (int i = 0; i < dots.Count; i++)
        {
            if (i < pathPoints.Count - 1)
            {
                Vector3 start = pathPoints[i];
                Vector3 end = pathPoints[i + 1];
                Vector3 flowPos = Vector3.Lerp(start, end, progress);
                
                dots[i].transform.position = flowPos;
                dots[i].SetActive(true);

                float alphaRatio = 1f - ((float)i / levelData.projectile.trajectoryDotCount);
                Color c = dotRenderers[i].color;
                c.a = Mathf.Lerp(levelData.projectile.minAlpha, 1f, alphaRatio);
                dotRenderers[i].color = c;

                float scale = Mathf.Lerp(0.2f, 0.05f, 1f - alphaRatio);
                dots[i].transform.localScale = Vector3.one * scale;
            }
            else
            {
                dots[i].SetActive(false);
            }
        }
    }

    private void RefreshDotPool()
    {
        if (dots == null)
        {
            dots = new List<GameObject>();
            dotRenderers = new List<SpriteRenderer>();
        }
        int targetCount = levelData.projectile.trajectoryDotCount;
        while (dots.Count < targetCount)
        {
            GameObject dot = Instantiate(dotPrefab, transform);
            dot.SetActive(false);
            dots.Add(dot);
            dotRenderers.Add(dot.GetComponent<SpriteRenderer>());
        }
        for (int i = targetCount; i < dots.Count; i++)
            if (dots[i].activeSelf) dots[i].SetActive(false);
    }
    private bool IsPointerOverUI()
    {
        if (EventSystem.current == null) return false;
        if (EventSystem.current.IsPointerOverGameObject()) return true;
        if (Input.touchCount > 0 && EventSystem.current.IsPointerOverGameObject(Input.GetTouch(0).fingerId)) return true;
        return false;
    }
    public void HideDots() { foreach (var dot in dots) dot.SetActive(false); }
}
