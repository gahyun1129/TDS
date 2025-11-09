using UnityEngine;
using UnityEngine.UI;
using TMPro; // TextMeshPro를 사용해야 합니다

public class DamageText : MonoBehaviour
{
    public float moveSpeed = 50f;   // 위로 올라가는 속도 (픽셀/초)
    public float fadeOutTime = 0.5f; // 사라지는 데 걸리는 시간

    private Text textMesh;
    private float timer;
    private Color startColor;

    private void Awake()
    {
        // TextMeshProUGUI 컴포넌트를 미리 찾아둡니다.
        textMesh = GetComponent<Text>();
        if (textMesh != null)
        {
            startColor = textMesh.color;
        }
    }

    // 풀 매니저가 이 함수를 호출해 텍스트를 초기화합니다.
    public void ShowText(string text, Vector3 screenPosition)
    {
        // 텍스트 내용과 위치 설정
        textMesh.text = text;
        transform.position = screenPosition;

        // 타이머와 알파값(투명도) 초기화
        timer = fadeOutTime;
        textMesh.color = startColor; // 원래 색상으로 (알파 1)
    }

    // OnEnable은 SetActive(true)가 될 때마다 호출됩니다.
    private void OnEnable()
    {
        // OnEnable에서 초기화하면 ShowText에서 하던 일을 줄일 수 있습니다.
        // 여기서는 ShowText에서 이미 위치와 텍스트를 설정했다고 가정합니다.
        timer = fadeOutTime;
        if (textMesh != null)
        {
            textMesh.color = startColor;
        }
    }

    void Update()
    {
        if (timer > 0)
        {
            // 타이머 감소
            timer -= Time.deltaTime;

            // 1. 위로 이동
            transform.position += Vector3.up * moveSpeed * Time.deltaTime;

            // 2. 페이드 아웃 (알파값 조절)
            // (timer / fadeOutTime)은 1에서 0으로 점차 감소합니다.
            Color newColor = startColor;
            newColor.a = Mathf.Clamp01(timer / fadeOutTime);
            textMesh.color = newColor;

            // 타이머가 0 이하가 되면
            if (timer <= 0)
            {
                // 자신을 풀 매니저에게 반납합니다.
                ObjectPoolManager.Instance.ReturnToPool(this.gameObject);
            }
        }
    }
}