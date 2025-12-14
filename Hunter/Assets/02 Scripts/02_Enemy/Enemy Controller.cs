using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

public enum EnemyState
{
    IDLE,
    MOVE,
    REST,
    DEFENSE_LOW,
    DEFENSE_HIGH,
    DROP_ARROW,
    ATTACK,
    DEAD,
}

public class EnemyController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] public BearController player;
    [SerializeField] private Animator animator;
    private Camera mainCamera;

    [Header("Weak Points")]
    [SerializeField] private GameObject[] weakPointObjects = new GameObject[8];
    private int remainingWeakPoints = 0;

    [Header("Effects")]
    [SerializeField] public GameObject ChasingMark;
    [SerializeField] public GameObject sweatEffect;

    // 화살
    private List<SpearProjectile> stuckArrows = new List<SpearProjectile>();

    // 상태
    private StateMachine stateMachine;
    private Dictionary<EnemyState, IState> stateList = new Dictionary<EnemyState, IState>();
    private bool isDead = false;

    private float attackTimer = 0f;
    private float defenseLowTimer = 0f;
    private float defenseHighTimer = 0f;

    // 데이터
    public LevelData levelData { get; private set; }

    void Start()
    {
        levelData = InGameManager.Instance.currentLevelData;

        mainCamera = Camera.main;

        SetEnemyScale(levelData.enemyBase.scale);
        // SetEnemyPosition(levelData.enemyBase.startViewportPos);

        InGameManager.Instance.OnGameEnd.AddListener(OnGameEnd);
        InGameManager.Instance.OnGameStart.AddListener(OnGameStart);

        SetStateMachine();
    }

    #region  setter

    public void SetStateMachine()
    {
        IState idle = new StateIdle(this);
        IState move = new StateMove(this);
        IState rest = new StateRest(this);
        IState defenseLow = new StateDefenseLow();
        IState defenseHigh = new StateDefenseHigh();
        IState dropArrow = new StateDropArrow(this);
        IState attack = new StateAttack(this);
        IState dead = new StateDead(this);

        stateList.Add(EnemyState.IDLE, idle);
        stateList.Add(EnemyState.MOVE, move);
        stateList.Add(EnemyState.REST, rest);
        stateList.Add(EnemyState.DEFENSE_LOW, defenseLow);
        stateList.Add(EnemyState.DEFENSE_HIGH, defenseHigh);
        stateList.Add(EnemyState.DROP_ARROW, dropArrow);
        stateList.Add(EnemyState.ATTACK, attack);
        stateList.Add(EnemyState.DEAD, dead);

        StartCooldown(EnemyState.ATTACK);
        StartCooldown(EnemyState.DEFENSE_LOW);
        StartCooldown(EnemyState.DEFENSE_HIGH);

        stateMachine = new StateMachine(idle, animator);
    }

    public void SetEnemyPosition(Vector2 pos)
    {
        Vector3 enemyViewportPos = new Vector3(pos.x, pos.y, 0);
        Vector3 enemyWorldPos = mainCamera.ViewportToWorldPoint(enemyViewportPos);
        enemyWorldPos.z = transform.position.z;
        transform.position = enemyWorldPos;

        levelData.enemyBase.startViewportPos = pos;
    }

    public void SetEnemyScale(float scale)
    {
        transform.localScale = Vector3.one * scale;

        levelData.enemyBase.scale = scale;
    }

    #endregion

    public void OnGameStart()
    {
        animator.Play("Idle");
    }
    
    void Update()
    {
        if (!InGameManager.Instance.isGameActive) return;
        if (isDead) return;

        UpdateTimers();
        CheckArrowCount(); // 화살 개수 체크
        
        stateMachine.DoState();
    }

    // 화살 개수 체크해서 Drop Arrow 상태로 전환
    private void CheckArrowCount()
    {
        if (stuckArrows.Count >= levelData.enemyPattern.dusting_arrow_num)
        {
            ChangeState(EnemyState.DROP_ARROW);
        }
    }

    // 외부에서 Defense 상태로 전환 (이벤트 발생 시 호출)
    public void TriggerDefense(bool isHighDefense)
    {
        if (isHighDefense && CanUseDefenseHigh())
        {
            ChangeState(EnemyState.DEFENSE_HIGH);
        }
        else if (!isHighDefense && CanUseDefenseLow())
        {
            ChangeState(EnemyState.DEFENSE_LOW);
        }
    }

    public void ChangeState(EnemyState newState)
    {
        if (stateList.ContainsKey(newState))
        {
            Debug.Log($"{newState}");
            stateMachine.SetState(stateList[newState]);
        }
    }

    public bool TryToMove()
    {
        float distance = Mathf.Abs(player.transform.position.x - transform.position.x);

        return distance > levelData.enemyBase.stopWorldDistance;
    }

    public bool TryToAttack()
    {
        float distance = Mathf.Abs(player.transform.position.x - transform.position.x);

        return distance > levelData.enemyBase.attackWorldDistance;
    }
    
    private void UpdateTimers()
    {
        if (attackTimer > 0f)
        {
            attackTimer -= Time.deltaTime;
            if (attackTimer < 0f) attackTimer = 0f;
        }

        if (defenseLowTimer > 0f)
        {
            defenseLowTimer -= Time.deltaTime;
            if (defenseLowTimer < 0f) defenseLowTimer = 0f;
        }

        if (defenseHighTimer > 0f)
        {
            defenseHighTimer -= Time.deltaTime;
            if (defenseHighTimer < 0f) defenseHighTimer = 0f;
        }
    }

    public bool CanUseAttack()
    {
        bool isUse = false;
        foreach (var stateWeight in levelData.enemyPattern.stateWeights)
        {
            if (stateWeight.state == EnemyState.ATTACK)
            {
                isUse = stateWeight.isUse;
                break;
            }
        }

        return isUse && attackTimer <= 0f;
    }

    public bool CanUseDefenseLow()
    {
        bool isUse = false;
        foreach (var stateWeight in levelData.enemyPattern.stateWeights)
        {
            if (stateWeight.state == EnemyState.DEFENSE_LOW)
            {
                isUse = stateWeight.isUse;
                break;
            }
        }

        return isUse && defenseLowTimer <= 0f;
    }

    public bool CanUseDefenseHigh()
    {
        bool isUse = false;
        foreach (var stateWeight in levelData.enemyPattern.stateWeights)
        {
            if (stateWeight.state == EnemyState.DEFENSE_HIGH)
            {
                isUse = stateWeight.isUse;
                break;
            }
        }

        return isUse && defenseHighTimer <= 0f;
    }

    public void StartCooldown(EnemyState state)
    {
        switch (state)
        {
            case EnemyState.ATTACK:
                attackTimer = levelData.enemyPattern.attack_cool_time;
                break;
            case EnemyState.DEFENSE_LOW:
                defenseLowTimer = levelData.enemyPattern.defense_low_cool_time;
                break;
            case EnemyState.DEFENSE_HIGH:
                defenseHighTimer = levelData.enemyPattern.defense_high_cool_time;
                break;
        }
    }

    public void OnGameEnd(bool isWin)
    {
        animator.speed = 1f;

        if (!isWin)
        {
            animator.SetTrigger("Idle");
        }
        else
        {
            OnDead().Forget();
        }
    }

    private async UniTaskVoid OnDead()
    {
        if (stuckArrows.Count > 0)
        {
            ScatterAllArrows();
        }
        await UniTask.Delay(1000);

        CameraController.Instance.ShowEnemyCamera();

        animator.speed = 0.5f;

        animator.SetTrigger("Dead");

    }
    
    
    
    /// <summary>
    /// 게임 종료 시 모든 화살을 새처럼 날려보내는 효과
    /// </summary>
    private void ScatterAllArrows()
    {
        foreach (var arrow in stuckArrows)
        {
            if (arrow != null)
            {
                arrow.ScatterLikeBird();
            }
        }
        
        // 리스트 비우기
        stuckArrows.Clear();
    }
    


    #region arrow management
    public void RegisterStuckArrow(SpearProjectile arrow)
    {
        if (!stuckArrows.Contains(arrow))
        {
            stuckArrows.Add(arrow);
        }
    }

    public void DropAllArrows()
    {
        if (stuckArrows.Count == 0) return;

        int keepCount = Random.Range(0, Mathf.Min(4, stuckArrows.Count + 1));

        int dropCount = stuckArrows.Count - keepCount;

        if (dropCount <= 0) return;

        for (int i = 0; i < stuckArrows.Count; i++)
        {
            int randomIndex = Random.Range(i, stuckArrows.Count);
            var temp = stuckArrows[i];
            stuckArrows[i] = stuckArrows[randomIndex];
            stuckArrows[randomIndex] = temp;
        }

        for (int i = 0; i < dropCount; i++)
        {
            if (stuckArrows[i] != null)
            {
                stuckArrows[i].transform.SetParent(null);
                stuckArrows[i].EnablePhysicsBounce(null);
            }
        }

        stuckArrows.RemoveRange(0, dropCount);
    }

    public int GetStuckArrowCount()
    {
        return stuckArrows.Count;
    }
    #endregion

    #region weak point
    public void SetActiveWeakPoints(int[] activeIndices)
    {
        for (int i = 0; i < weakPointObjects.Length; i++)
        {
            if (weakPointObjects[i] != null)
            {
                weakPointObjects[i].SetActive(false);
            }
        }

        remainingWeakPoints = 0;
        foreach (int index in activeIndices)
        {
            if (index >= 0 && index < weakPointObjects.Length && weakPointObjects[index] != null)
            {
                weakPointObjects[index].SetActive(true);
                remainingWeakPoints++;
            }
        }
    }

    public void OnWeakPointHit()
    {
        remainingWeakPoints--;
        if (remainingWeakPoints < 0)
        {
            remainingWeakPoints = 0;
        }
    }

    public int GetRemainingWeakPoints()
    {
        return remainingWeakPoints;
    }
    #endregion

}
