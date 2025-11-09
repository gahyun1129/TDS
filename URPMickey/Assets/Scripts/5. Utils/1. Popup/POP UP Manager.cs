// PopupManager.cs
using System.Collections.Generic;
using UnityEngine;

public class PopupManager : MonoBehaviour
{
    #region Singleton
    public static PopupManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            Initialize();
        }
        else
        {
            Destroy(gameObject);
        }
    }
    #endregion

    [System.Serializable]
    public class PopupPair
    {
        public PopupType type;
        public PopupBase prefab; // PopupBase 스크립트를 직접 참조
    }



    [Header("Popup Settings")]
    [Tooltip("팝업 프리팹들을 여기에 등록합니다.")]
    [SerializeField] private List<PopupPair> popupPrefabs = new List<PopupPair>();

    [Header("Sorting Order Settings")]
    [Tooltip("팝업이 시작될 기본 Sorting Order")]
    [SerializeField] private int baseSortingOrder = 100;
    [Tooltip("팝업이 겹칠 때마다 더해질 Order 값 (팝업 내부 UI용 여유값)")]
    [SerializeField] private int sortingOrderStep = 10;

    // 생성된 팝업 인스턴스 캐시
    private Dictionary<PopupType, PopupBase> _popupInstances = new Dictionary<PopupType, PopupBase>();

    // '현재 열려있는' 팝업들의 순서 목록 (Sorting Order 관리를 위해)
    private List<PopupBase> _openedPopups = new List<PopupBase>();

    private void Initialize()
    {
        _popupInstances.Clear();
        _openedPopups.Clear();
    }

    /// <summary>
    /// 팝업을 찾아 열어줍니다. (팩토리 메소드)
    /// </summary>
    public PopupBase ShowPopup(PopupType type)
    {
        if (type == PopupType.None) return null;

        PopupBase popupInstance;

        // 1. 캐시에서 팝업 찾기
        if (!_popupInstances.TryGetValue(type, out popupInstance))
        {
            // 2. 캐시에 없으면 프리팹 목록에서 찾아 생성
            PopupBase prefabToLoad = popupPrefabs.Find(p => p.type == type)?.prefab;
            if (prefabToLoad == null)
            {
                Debug.LogError($"[PopupManager] {type}에 해당하는 팝업 프리팹이 등록되지 않았습니다.");
                return null;
            }

            // 3. 팝업 매니저(DontDestroyOnLoad)의 자식으로 생성
            popupInstance = Instantiate(prefabToLoad, transform);
            popupInstance.name = $"{type}_Popup";
            popupInstance.SetManager(this);

            // 4. 캐시에 등록
            _popupInstances.Add(type, popupInstance);
        }

        // 5. 이미 열린 팝업인지 확인 후, 열린 목록에 추가/관리
        if (_openedPopups.Contains(popupInstance))
        {
            // 이미 열려있다면, 리스트에서 제거했다가 다시 맨 뒤에 추가 (최상위로 올리기)
            _openedPopups.Remove(popupInstance);
        }
        _openedPopups.Add(popupInstance);

        // 6. Sorting Order 재정렬 및 팝업 열기
        ReorderPopups();
        popupInstance.Open();
        
        return popupInstance;
    }

    /// <summary>
    /// 특정 타입의 팝업을 닫습니다.
    /// </summary>
    public void ClosePopup(PopupType type)
    {
        if (_popupInstances.TryGetValue(type, out PopupBase popupInstance))
        {
            if (popupInstance.gameObject.activeSelf)
            {
                popupInstance.Close(); // Close()가 애니메이션 후 NotifyPopupClosed()를 호출
            }
        }
    }

    /// <summary>
    /// 현재 열려있는 모든 팝업을 닫습니다.
    /// </summary>
    public void CloseAllPopups()
    {
        // 리스트를 복사해서 순회 (원본 리스트가 NotifyPopupClosed에 의해 변경되기 때문)
        List<PopupBase> activePopups = new List<PopupBase>(_openedPopups);
        foreach (var popup in activePopups)
        {
            popup.Close();
        }
    }

    /// <summary>
    /// (PopupBase가 호출) 팝업이 닫기 애니메이션을 완료했을 때 호출됩니다.
    /// </summary>
    public void NotifyPopupClosed(PopupBase popup)
    {
        if (_openedPopups.Contains(popup))
        {
            _openedPopups.Remove(popup);
            ReorderPopups(); // 팝업이 닫혔으니 Sorting Order 재정렬
        }
    }

    /// <summary>
    /// 열려있는 팝업 목록(_openedPopups)을 기준으로 Sorting Order를 재설정합니다.
    /// </summary>
    private void ReorderPopups()
    {
        int currentOrder = baseSortingOrder;
        for (int i = 0; i < _openedPopups.Count; i++)
        {
            _openedPopups[i].SetSortingOrder(currentOrder);
            currentOrder += sortingOrderStep; // 다음 팝업은 더 높은 Order를 가짐
        }
    }
}