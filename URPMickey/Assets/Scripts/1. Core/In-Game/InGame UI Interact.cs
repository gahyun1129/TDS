using System.Collections;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class InGameUIInteract : MonoBehaviour
{
    public static InGameUIInteract Instance { get; private set; }

    [Header("시간")]
    [SerializeField] private Text time_text;
    [SerializeField] private Slider time_slider;
    private float duration;

    [Header("스테이지")]
    [SerializeField] private Text stage_text;
    [SerializeField] private Text time_scale_text;

    [Header("카운트 다운")]
    [SerializeField] private Text count_down_text;

    [Header("플레이어 체력")]
    [SerializeField] private Slider HP_slider;
    [SerializeField] private Text HP_text;

    [Header("플레이어 마나")]
    [SerializeField] private Slider Mana_slider;
    [SerializeField] private Text Mana_text;

    [Header("웨이브 끝 패널")]
    [SerializeField] private GameObject end_panel;
    [SerializeField] private GameObject next_stage_button;
    [SerializeField] private Text end_text;

    private GameObject player;
    public SkillManager skillManager;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }

        BattleManager.Instance.OnGameReady += ReadyToStart;
        BattleManager.Instance.OnGameStart += GameStart;
        BattleManager.Instance.OnGameEnd += GameEnd;

        player = BattleManager.Instance.Player;

        InGamePlayerStat.Instance.OnUpdateHP += UpdateHPUI;
        
        InGamePlayerStat.Instance.OnUpdateMana += UpdateManaUI;

    }

    void OnDestroy()
    {
        BattleManager.Instance.OnGameReady -= ReadyToStart;
        BattleManager.Instance.OnGameStart -= GameStart;
        BattleManager.Instance.OnGameEnd -= GameEnd;

        InGamePlayerStat.Instance.OnUpdateHP -= UpdateHPUI;
        
        InGamePlayerStat.Instance.OnUpdateMana -= UpdateManaUI;
    }

    public void UpdateStageText()
    {
        stage_text.text = GameManager.Instance.STAGE + " M";
    }

    public void UpdateHPUI(float HP, float maxHP)
    {
        HP_slider.value = HP / maxHP;
        HP_text.text = math.ceil(HP).ToString();
    }

    public void UpdateManaUI(float Mana, float MaxMana)
    {
        Mana_slider.value = Mana / MaxMana;
        Mana_text.text = math.ceil(Mana).ToString();
    }

    public void ReadyToStart(int stage)
    {
        UpdateStageText();
        next_stage_button.SetActive(false);
        StartCoroutine(CountDown());
    }

    public void GameStart()
    {
        duration = WaveManager.Instance.WaveTime;
        StartCoroutine(IUpdateWaveTime());
    }

    public void GameEnd(bool _isWin)
    {
        if (_isWin)
        {
            PopupBase popup = PopupManager.Instance.ShowPopup(PopupType.Skill);

            SkillPopup skillPopup = popup as SkillPopup;

            skillPopup.Setup(BattleManager.Instance.Player.GetComponent<SkillManager>().equippedSkills[0]);
            skillPopup.TurnOnNextButton(() =>
                {
                    BattleManager.Instance.GameReady();
                    PopupManager.Instance.ClosePopup(PopupType.Skill);
                });
            //end_text.text = "승리";
            //next_stage_button.SetActive(true);
        }
        else
        {
            end_panel.SetActive(true);
            end_text.text = "패배";
        }
    }

    private IEnumerator IUpdateWaveTime()
    {
        while (WaveManager.Instance.gameTime < duration)
        {
            if (BattleManager.Instance.CurGameState == GameState.GameOver)
            {
                yield return null;
                continue;
            }

            time_slider.value = (duration - WaveManager.Instance.gameTime) / duration;
            time_text.text = math.ceil(duration - WaveManager.Instance.gameTime).ToString();

            yield return new WaitForSeconds(0.05f);
        }
        time_slider.value = 0;
        time_text.text = 0f.ToString();
    }

    private IEnumerator CountDown()
    {
        count_down_text.gameObject.SetActive(true);

        count_down_text.text = "3";

        yield return new WaitForSeconds(1f);

        count_down_text.text = "2";

        yield return new WaitForSeconds(1f);

        count_down_text.text = "1";

        yield return new WaitForSeconds(1f);

        count_down_text.text = "START!";

        yield return new WaitForSeconds(1f);

        count_down_text.gameObject.SetActive(false);

        BattleManager.Instance.GameStart();
    }

    // -------------------------------------------------------------------
    // 버튼 인터랙트 함수
    // -------------------------------------------------------------------

    public void TimeFasterButton()
    {
        float value = TimeManager.Instance.ChangeTimeScale(1f);
        time_scale_text.text = "X " + value + ".0";
    }

    public void TimeSlowerButton()
    {
        float value = TimeManager.Instance.ChangeTimeScale(-1f);
        time_scale_text.text = "X " + value + ".0";
    }

    public void BackToLobbyButton()
    {
        //InGameManager.Instance.ResetInGameMemory();
        GameManager.Instance.UpdateHighestWave();
        SceneChanger.Instance.LoadSceneByName("Out-Game");
    }

    public void NextStageButton()
    {
        end_panel.SetActive(false);
        BattleManager.Instance.GameReady();
    }

    // 임시 스킬
    public void OnClickedSkill(int index)
    {
        skillManager.UseSkill(index);
    }
}

