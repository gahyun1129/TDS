using System.IO;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PresetUIManager : MonoBehaviour
{
    [Header("핵심 데이터")]
    public LevelData levelData;

    [Header("패널 연결")]
    public GameObject presetListPanel;    // 전체 팝업 (목록 + 에디터)
    public GameObject editorPanel;        // 우측 에디터 영역 (초기엔 꺼둘 수도 있음)

    [Header("목록 UI")]
    public Transform listContent;         // ScrollView Content
    public GameObject presetButtonPrefab; // 프리셋 버튼 프리팹

    [Header("에디터 UI (저장/수정용)")]
    public TMP_InputField fileNameInput;  // 파일 이름 입력칸

    [Header("밸런스 조절 인풋 필드들")]
    public TMP_InputField hpInput;
    public TMP_InputField speedInput;
    public TMP_InputField timeLimitInput;
    public TMP_InputField enemyCountInput;
    // 필요한 만큼 더 추가...

    private string saveFolderPath;

    void Start()
    {
        // 1. 초기 설정
        if(InGameManager.Instance != null) 
            levelData = InGameManager.Instance.currentLevelData;
            
        saveFolderPath = Application.persistentDataPath;

        // 2. 시작할 때 목록만 보여주고 에디터는 숨김
        if (presetListPanel) presetListPanel.SetActive(false);
        if (editorPanel) editorPanel.SetActive(false);
    }

    // =================================================================
    // 1. 메인 버튼 기능 (열기/닫기/추가)
    // =================================================================

    public void OnClickOpenManager()
    {
        presetListPanel.SetActive(true);
        editorPanel.SetActive(false); // 에디터는 일단 숨김
        RefreshPresetList();
    }

    public void OnClickCloseManager()
    {
        presetListPanel.SetActive(false);
    }

    // [Add New] 버튼 클릭 시
    public void OnClickAddButton()
    {
        // 1. 에디터 열기
        editorPanel.SetActive(true);

        // 2. 이름 칸 비우기 (새로 저장해야 하니까)
        fileNameInput.text = "";
        fileNameInput.interactable = true; // 이름 수정 가능

        // 3. 현재 게임상의 데이터를 UI에 뿌려주기 (기본값으로 사용)
        UpdateUIFromData(); 
    }

    // =================================================================
    // 2. 프리셋 목록 로직
    // =================================================================

    private void RefreshPresetList()
    {
        foreach (Transform child in listContent) Destroy(child.gameObject);

        DirectoryInfo info = new DirectoryInfo(saveFolderPath);
        FileInfo[] fileInfo = info.GetFiles("*.json");

        foreach (FileInfo file in fileInfo)
        {
            GameObject btnObj = Instantiate(presetButtonPrefab, listContent);
            string fileNameOnly = Path.GetFileNameWithoutExtension(file.Name);

            // 버튼 텍스트
            TextMeshProUGUI btnText = btnObj.GetComponentInChildren<TextMeshProUGUI>();
            if (btnText != null) btnText.text = fileNameOnly;

            // 버튼 클릭 시 -> OnSelectPreset 호출
            Button btn = btnObj.GetComponent<Button>();
            btn.onClick.AddListener(() => OnSelectPreset(fileNameOnly));
        }
    }

    // 목록에 있는 버튼을 눌렀을 때
    private void OnSelectPreset(string fileName)
    {
        // 1. 해당 파일 데이터를 LevelData로 로드
        levelData.LoadFromFile(fileName);

        // 2. 에디터 열기
        editorPanel.SetActive(true);

        // 3. 이름 칸에 파일명 채우기
        fileNameInput.text = fileName;
        
        // 4. 로드된 데이터를 UI 인풋 필드에 뿌리기
        UpdateUIFromData();

        Debug.Log($"[{fileName}] 데이터를 불러와서 에디터에 표시했습니다.");
    }

    // =================================================================
    // 3. 저장(Save) 로직 (덮어쓰기 or 새로 만들기)
    // =================================================================

    public void OnClickSave()
    {
        string fileName = fileNameInput.text;

        if (string.IsNullOrWhiteSpace(fileName))
        {
            Debug.LogWarning("파일 이름을 입력해주세요.");
            return;
        }

        // 1. UI에 적힌 값들을 실제 LevelData 변수에 적용
        ApplyUIToData();

        // 2. 파일로 저장 (이미 있는 이름이면 덮어쓰기 됨)
        levelData.SaveToFile(fileName);

        // 3. 목록 새로고침 (새로 추가됐을 수 있으니)
        RefreshPresetList();

        Debug.Log($"[{fileName}] 저장 완료!");
    }

    // =================================================================
    // 4. 데이터 <-> UI 동기화 (노가다 구간)
    // =================================================================

    // Data -> UI (보여주기)
    private void UpdateUIFromData()
    {
        // float나 int를 string으로 변환해서 인풋 필드에 넣음
        if(hpInput) hpInput.text = levelData.enemyBase.maxHealth.ToString();
        if(speedInput) speedInput.text = levelData.enemyBase.speed.ToString();
        if(timeLimitInput) timeLimitInput.text = levelData.mission.timeLimit.ToString();
        
        // 여기에 필요한 변수들을 계속 추가하세요...
        // ex) if(projectileDamageInput) ...
    }

    // UI -> Data (적용하기)
    private void ApplyUIToData()
    {
        // 인풋 필드의 string을 파싱해서 데이터에 넣음
        if (hpInput && float.TryParse(hpInput.text, out float hp))
            levelData.enemyBase.maxHealth = hp;

        if (speedInput && float.TryParse(speedInput.text, out float speed))
            levelData.enemyBase.speed = speed;

        if (timeLimitInput && int.TryParse(timeLimitInput.text, out int time))
            levelData.mission.timeLimit = time;

        // 여기에 필요한 변수들을 계속 추가하세요...
    }
}