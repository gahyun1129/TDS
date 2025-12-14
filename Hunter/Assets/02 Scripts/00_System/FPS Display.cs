using UnityEngine;

public class FPSDisplay : MonoBehaviour
{
    [Range(10, 100)]
    public int fontSize = 30; // 폰트 크기 조절용
    public Color color = Color.green; // 글자 색상

    float deltaTime = 0.0f;

    void Update()
    {
        // 프레임 측정 (부드럽게 보간)
        deltaTime += (Time.unscaledDeltaTime - deltaTime) * 0.1f;
    }

    void OnGUI()
    {
        int w = Screen.width, h = Screen.height;

        GUIStyle style = new GUIStyle();

        // 위치 및 크기 설정
        Rect rect = new Rect(20, 20, w, h * 2 / 100);
        
        style.alignment = TextAnchor.UpperLeft;
        style.fontSize = fontSize;
        style.normal.textColor = color;

        // FPS 계산
        float msec = deltaTime * 1000.0f;
        float fps = 1.0f / deltaTime;
        
        // 출력 텍스트: "ms (fps)" 형태
        string text = string.Format("{0:0.0} ms ({1:0.} fps)", msec, fps);
        
        GUI.Label(rect, text, style);
    }
}