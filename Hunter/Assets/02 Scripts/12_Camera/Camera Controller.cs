using Cinemachine;
using DG.Tweening;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    #region scene singleton
    public static CameraController Instance { get; private set; }
    private CinemachineImpulseSource _impulseSource;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        _impulseSource = GetComponent<CinemachineImpulseSource>();
        Instance = this;
    }

    #endregion

    [Header("Cameras")]
    public GameObject dogCameraObj;
    public GameObject enemyCameraObj;
    public GameObject mainCarmeraObj;
    public GameObject tutorialCameraObj;

    private CinemachineVirtualCamera _dogCamLegacy;
    private CinemachineVirtualCamera _enemyCamLegacy;
    private CinemachineVirtualCamera _mainCamLegacy;
    private CinemachineVirtualCamera _tutorialLegacy;

    void Start()
    {
        if (dogCameraObj) _dogCamLegacy = dogCameraObj.GetComponent<CinemachineVirtualCamera>();
        if (enemyCameraObj) _enemyCamLegacy = enemyCameraObj.GetComponent<CinemachineVirtualCamera>();
        if (mainCarmeraObj) _mainCamLegacy = mainCarmeraObj.GetComponent<CinemachineVirtualCamera>();
        if (tutorialCameraObj) _tutorialLegacy = tutorialCameraObj.GetComponent<CinemachineVirtualCamera>();
    }
    
    [ContextMenu("test camera shaking")]
    public void Shake()
    {
        Shake(1f);
    }

    public void Shake(float force = 1.0f, float duration = 1.0f)
    {
        if (_impulseSource != null)
        {
            _impulseSource.GenerateImpulse(force);
            PostProcessingContorller.Instance.ProcessMotionBlur(duration).Forget();
        }
    }

    private void SetPriority(CinemachineVirtualCamera cam, int score)
    {
        if (cam != null) cam.Priority = score;
    }
    
    public void ShowDogCamera()
    {
        SetPriority(_dogCamLegacy, 100);
        SetPriority(_enemyCamLegacy, 0);
        SetPriority(_tutorialLegacy, 0);
        SetPriority(_mainCamLegacy, 0);
    }

    public void ShowEnemyCamera()
    {
        SetPriority(_dogCamLegacy, 0);
        SetPriority(_enemyCamLegacy, 100);
        SetPriority(_tutorialLegacy, 0);
        SetPriority(_mainCamLegacy, 0);
    }

    public void ShowMainCamera()
    {
        mainCarmeraObj.SetActive(true);

        SetPriority(_dogCamLegacy, 0);
        SetPriority(_enemyCamLegacy, 0);
        SetPriority(_tutorialLegacy, 0);
        SetPriority(_mainCamLegacy, 100);
    }
    
    public void ShowTutorialCamera()
    {
        SetPriority(_dogCamLegacy, 0);
        SetPriority(_enemyCamLegacy, 0);
        SetPriority(_tutorialLegacy, 100);
        SetPriority(_mainCamLegacy, 0);
    }

    

}

