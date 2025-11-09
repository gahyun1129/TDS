using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class OutGameUIInterect : MonoBehaviour
{
    [Header("스테이지 관련")]
    [SerializeField] private Text best_stage_text;

    void Start()
    {
        best_stage_text.text = GameDataManager.Instance.GetPlayerData().highestWave + " M";
    }

    // -------------------------------------------------------------------
    // 버튼 인터랙트 함수
    // -------------------------------------------------------------------

    public void InGameStart()
    {
        SceneChanger.Instance.LoadSceneByName("In-Game");
    }

    public void Test()
    {
        Debug.Log("게임 종료 버튼 클릭! Confirm 팝업을 엽니다.");

        // 1. 팝업을 엽니다. ShowPopup은 'PopupBase' 타입을 반환합니다.
        PopupBase popup = PopupManager.Instance.ShowPopup(PopupType.Confirm);

        // 2. 반환된 팝업을 'ConfirmPopup'으로 형변환(Casting)합니다.
        //    (as 키워드를 사용하면 형변환 실패 시 null이 됩니다)
        ConfirmPopup confirmPopup = popup as ConfirmPopup;

        // 3. 형변환이 성공했는지 확인합니다.
        if (confirmPopup != null)
        {
            // 4. ConfirmPopup에만 있는 'Setup' 메소드를 호출하여
            //    필요한 데이터(메시지, 확인 버튼 클릭 시 동작)를 전달합니다.
            confirmPopup.Setup(
                "정말 게임을 종료하시겠습니까?",   // 팝업에 표시될 메시지
                () =>
                {                           // '확인' 버튼을 눌렀을 때 실행될 동작 (람다식)
                    Debug.Log("확인 버튼이 눌렸습니다. 게임을 종료합니다.");
                    Application.Quit(); // (에디터에서는 EditorApplication.isPlaying = false;)
                }
            );
        }
        else
        {
            Debug.LogError("ConfirmPopup으로 형변환에 실패했습니다. PopupManager에 프리팹이 잘못 등록되었거나, ConfirmPopup.cs 스크립트가 프리팹에 없는지 확인하세요.");
        }
    }
    
    public void OnClicekdStatBtn()
    {
        Debug.Log("스탯 버튼 클릭! Stat Display 팝업을 엽니다.");

        PopupBase popup = PopupManager.Instance.ShowPopup(PopupType.Stat);

        statDisplayPopup statDisplayPopup = popup as statDisplayPopup;

        if ( statDisplayPopup != null)
        {
            statDisplayPopup.DisplayPopup();
        }
    }
}
