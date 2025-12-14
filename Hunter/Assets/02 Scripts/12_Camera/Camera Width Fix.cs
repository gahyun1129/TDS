using UnityEngine;

// ▼ 이 줄이 핵심입니다! 에디터 상에서도 스크립트를 돌아가게 해줍니다.
[ExecuteAlways] 
public class CameraWidthFix : MonoBehaviour
{
    [Tooltip("게임에서 고정적으로 보여주고 싶은 가로 길이 (유닛 단위)")]
    public float targetWidth = 20.0f; 

    private Camera _camera;

    void Update()
    {
        // 카메라 컴포넌트를 매번 가져오면 무거우니 없을 때만 가져오기
        if (_camera == null)
        {
            _camera = GetComponent<Camera>();
        }

        if (_camera != null)
        {
            // 현재 게임 뷰의 화면 비율 (가로 / 세로)
            // 에디터에서는 'Game' 탭의 해상도 설정에 따라 계산됩니다.
            float currentAspectRatio = (float)Screen.width / (float)Screen.height;

            // 0으로 나누는 에러 방지
            if (currentAspectRatio > 0)
            {
                // Orthographic Size 실시간 적용
                _camera.orthographicSize = targetWidth / currentAspectRatio / 2.0f;
            }
        }
    }
}