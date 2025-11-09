using UnityEngine;

public class BackButtonHandler : MonoBehaviour
{
    [Header("아웃 패널")]
    [SerializeField] private GameObject quitConfirmationPanel;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (!quitConfirmationPanel.activeSelf)
            {
                ShowQuitConfirmation();
            }
            else
            {
                CancelQuit();
            }
        }
    }

    public void ShowQuitConfirmation()
    {
        if (quitConfirmationPanel != null)
        {
            TimeManager.Instance.Pause();
            quitConfirmationPanel.SetActive(true);
        }
    }

    public void ConfirmQuit()
    {
        
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false; // 에디터에서 게임 중지
#else
        Application.Quit(); // 실제 빌드(Android) 시 종료
#endif
    }

    public void CancelQuit()
    {
        if (quitConfirmationPanel != null)
        {
            quitConfirmationPanel.SetActive(false);
            TimeManager.Instance.Resume();
        }
    }
}