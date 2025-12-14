using System.Collections.Generic;
using UnityEngine;

public class DetectDefenseZone : MonoBehaviour
{
    [SerializeField] private EnemyController enemyController;

    [Header("Layers")]
    [SerializeField] private LayerMask weaponLayer;

    // 이미 체크된 충돌체 추적
    private HashSet<Collider2D> checkedCollisions = new HashSet<Collider2D>();
    
    private Camera mainCamera;

    void Start()
    {
        mainCamera = Camera.main;
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (!InGameManager.Instance.isGameActive) return;

        Debug.Log("걸리긴 하나");
        // 이미 체크된 충돌체는 무시
        if (checkedCollisions.Contains(collision))
        {
            return;
        }

        int hitLayer = 1 << collision.gameObject.layer;
        if ((hitLayer & weaponLayer) != 0)
        {
            // 충돌 위치를 뷰포트 좌표로 변환
            Vector3 worldPos = collision.transform.position;
            Vector3 viewportPos = mainCamera.WorldToViewportPoint(worldPos);

            // 뷰포트 Y 좌표 0.7 이상이면 상단(true), 아니면 하단(false)
            bool isHighDefense = viewportPos.y >= 0.7f;

            enemyController.TriggerDefense(isHighDefense);

            // 이 충돌체를 체크 완료 목록에 추가
            checkedCollisions.Add(collision);
        }
    }

    void OnTriggerExit2D(Collider2D collision)
    {
        // 충돌체가 트리거를 벗어나면 체크 목록에서 제거 (재사용 가능하게)
        if (checkedCollisions.Contains(collision))
        {
            checkedCollisions.Remove(collision);
        }
    }

    // 게임 재시작 등을 위한 리셋 메서드
    public void ResetCheckedCollisions()
    {
        checkedCollisions.Clear();
    }
}
