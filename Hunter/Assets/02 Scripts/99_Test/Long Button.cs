using UnityEngine;
using UnityEngine.EventSystems; // UI 이벤트 필수 네임스페이스
using UnityEngine.Events;       // 인스펙터 연결용

public class LongButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    // 인스펙터에서 연결할 이벤트 (여기에 함수를 등록하세요)
    public UnityEvent onLongPress;

    private bool isPressed = false;

    // 옵션: 너무 빨리 실행되면 안 되니까 쿨타임 두기 (0.1초마다 실행)
    // 0이면 매 프레임(1초에 60번) 실행됨
    public float interval = 0.1f; 
    private float timer = 0f;

    // 1. 버튼을 누르는 순간 실행
    public void OnPointerDown(PointerEventData eventData)
    {
        isPressed = true;
        onLongPress.Invoke();
        timer = 0f; // 누르자마자 바로 한 번 실행하고 싶으면 여기서 invoke 하거나 타이머 조절
    }

    // 2. 손을 떼는 순간 실행
    public void OnPointerUp(PointerEventData eventData)
    {
        isPressed = false;
    }

    // 3. 누르고 있는 동안 계속 체크
    void Update()
    {
        if (isPressed)
        {
            timer += Time.deltaTime;
            
            // 설정한 시간 간격마다 실행
            if (timer >= interval)
            {
                onLongPress.Invoke(); // 연결된 함수 실행!
                timer = 0f;           // 타이머 초기화
            }
        }
    }
}