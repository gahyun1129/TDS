using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class StageInfoPopUp : PopupBase
{
    [Header("Weak Point Toggles")]
    [SerializeField] private Toggle[] weakPointToggles = new Toggle[8];
    
    private LevelData levelData;
    private List<int> selectedToggleIndices = new List<int>();
    private int maxWeakPoints = 0;

    void Start()
    {
        levelData = InGameManager.Instance.currentLevelData;
        
        // weakPointToggles가 null이거나 비어있으면 실행하지 않음
        if (weakPointToggles == null || weakPointToggles.Length == 0)
        {
            return;
        }
        
        // 토글 리스너 등록 및 초기 비활성화
        for (int i = 0; i < weakPointToggles.Length; i++)
        {
            if (weakPointToggles[i] == null) continue; // null 체크
            
            int index = i; // 클로저 문제 방지
            weakPointToggles[i].onValueChanged.AddListener((isOn) => OnToggleChanged(index, isOn));
            weakPointToggles[i].gameObject.SetActive(false); // 초기에는 비활성화
        }
    }

    public void OnClickedStageStart()
    {
        // 약점 개수가 설정되어 있고, 선택된 토글이 부족한 경우 랜덤으로 채우기
        if (maxWeakPoints > 0 && selectedToggleIndices.Count < maxWeakPoints)
        {
            FillRemainingTogglesRandomly();
        }
        
        // Enemy Controller에 선택된 약점 전달
        EnemyController enemyController = FindObjectOfType<EnemyController>();
        if (enemyController != null)
        {
            enemyController.SetActiveWeakPoints(selectedToggleIndices.ToArray());
        }
        
        InGameManager.Instance.GameStart();
        Close();
    }

    public void OnEndEnemyHealth(string text)
    {
        levelData.enemyBase.maxHealth = float.Parse(text);
    }

    public void OnEndTime(string text)
    {
        levelData.mission.timeLimit = int.Parse(text);
    }

    public void OnEndCount(string text)
    {
        levelData.mission.countLimit = int.Parse(text);
    }
    
    public void OnEndWeak(string text)
    {
        levelData.mission.weakPointCount = int.Parse(text);
        maxWeakPoints = levelData.mission.weakPointCount;
        
        // 약점 개수가 변경되면 토글 초기화
        ResetToggles();
    }

    private void OnToggleChanged(int index, bool isOn)
    {
        if (isOn)
        {
            // 토글이 켜질 때
            // maxWeakPoints가 0이면 제한 없이 선택 가능 (약점 개수 미설정 상태)
            if (maxWeakPoints == 0 || selectedToggleIndices.Count < maxWeakPoints)
            {
                // 아직 최대 개수에 도달하지 않았으면 추가
                if (!selectedToggleIndices.Contains(index))
                {
                    selectedToggleIndices.Add(index);
                }
            }
            else
            {
                // 최대 개수에 도달했으면 가장 먼저 선택한 토글을 끄고 새로운 토글 추가
                int firstIndex = selectedToggleIndices[0];
                selectedToggleIndices.RemoveAt(0);
                
                // 첫 번째 토글 끄기 (리스너 일시 제거하여 무한 루프 방지)
                weakPointToggles[firstIndex].onValueChanged.RemoveAllListeners();
                weakPointToggles[firstIndex].isOn = false;
                int capturedFirstIndex = firstIndex;
                weakPointToggles[firstIndex].onValueChanged.AddListener((value) => OnToggleChanged(capturedFirstIndex, value));
                
                selectedToggleIndices.Add(index);
            }
        }
        else
        {
            // 토글이 꺼질 때
            selectedToggleIndices.Remove(index);
        }
        
        // 토글 변경 시마다 Enemy Controller 업데이트
        UpdateEnemyWeakPoints();
    }

    private void ResetToggles()
    {
        selectedToggleIndices.Clear();
        
        // 모든 토글 초기화 및 활성화 상태 설정
        for (int i = 0; i < weakPointToggles.Length; i++)
        {
            weakPointToggles[i].onValueChanged.RemoveAllListeners();
            weakPointToggles[i].isOn = false; // 체크 해제
            int index = i;
            weakPointToggles[i].onValueChanged.AddListener((isOn) => OnToggleChanged(index, isOn));
            
            // maxWeakPoints가 설정되면 토글 활성화, 아니면 비활성화
            if (maxWeakPoints > 0)
            {
                weakPointToggles[i].gameObject.SetActive(true);
            }
            else
            {
                weakPointToggles[i].gameObject.SetActive(false);
            }
        }
    }

    private void FillRemainingTogglesRandomly()
    {
        int needed = maxWeakPoints - selectedToggleIndices.Count;
        
        // 선택되지 않은 인덱스 찾기
        List<int> availableIndices = new List<int>();
        for (int i = 0; i < weakPointToggles.Length; i++)
        {
            if (!selectedToggleIndices.Contains(i))
            {
                availableIndices.Add(i);
            }
        }
        
        // 랜덤으로 필요한 만큼 선택
        for (int i = 0; i < needed && availableIndices.Count > 0; i++)
        {
            int randomIndex = Random.Range(0, availableIndices.Count);
            int toggleIndex = availableIndices[randomIndex];
            availableIndices.RemoveAt(randomIndex);
            
            // 토글 켜기
            weakPointToggles[toggleIndex].isOn = true;
        }
    }

    private void UpdateEnemyWeakPoints()
    {
        // Enemy Controller 찾기
        EnemyController enemyController = FindObjectOfType<EnemyController>();
        if (enemyController != null)
        {
            // 현재 선택된 토글 인덱스를 Enemy Controller에 전달
            enemyController.SetActiveWeakPoints(selectedToggleIndices.ToArray());
        }
    }
}
