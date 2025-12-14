using Cysharp.Threading.Tasks;
using UnityEngine;

public enum PlayerState
{
    NONE,
    IDLE,
    MOVING,
    AIMING,
    SHOOTING,
    DEAD,
}

public class BearController : MonoBehaviour
{
    public static string PLAYER_IDLE        = "isIdle";
    public static string PLAYER_MOVING      = "isMoving";
    public static string PLAYER_AIMING      = "isAiming";
    public static string PLAYER_SHOOTING    = "isShooting";
    public static string PLAYER_DEAD        = "isDead";
                    
    [Header("레이어 마스크")]
    [SerializeField] private LayerMask enemyLayer;

    [Header("Reference")]
    [SerializeField] private Animator animator;
    [SerializeField] private BearAttacker attacker;

    private LevelData levelData;
    private PlayerState currentState = PlayerState.NONE;

    private bool isShootandOver = false;

    private float lastAnimatorSpeed = 1f;

    void Start()
    {
        levelData = InGameManager.Instance.currentLevelData;

        SetPlayerScale(levelData.player.scale);

        InGameManager.Instance.OnGameEnd.AddListener(OnGameEnd);
        InGameManager.Instance.OnGameStart.AddListener(OnGameStart);
        InGameManager.Instance.OnGamePause.AddListener(OnGamePause);
    }

    #region setter

    public void SetPlayerScale(float scale)
    {
        transform.localScale = Vector3.one * scale;
        levelData.player.scale = scale;
    }

    #endregion

    public void OnGameStart()
    {
        ResetState();
    }
    
    void Update()
    {
        if (!InGameManager.Instance.isGameActive) { return; }

        if (Input.GetMouseButtonDown(0))
        {
            if (currentState != PlayerState.IDLE) return;
            if (Input.mousePosition.x < Screen.width / 2f)
            {
                ChangeState(PlayerState.AIMING);
            }
            else
            {
                ChangeState(PlayerState.MOVING);  
            }
        }

        if (Input.GetMouseButton(0))
        {
            if (currentState == PlayerState.AIMING)
            {
                if (Input.mousePosition.x >= Screen.width / 2f)
                {
                    attacker.CancelAiming();
                    ChangeState(PlayerState.IDLE);
                    return;
                }
            }
        }

        if (Input.GetMouseButtonUp(0))
        {
            if (currentState == PlayerState.AIMING)
            {
                ChangeState(PlayerState.SHOOTING);
            }
            else if (currentState == PlayerState.MOVING)
            {
                ChangeState(PlayerState.IDLE);
            }
        }

        DoState(currentState);
    }

    #region state machine
    public void ChangeState(PlayerState nextState)
    {
        Debug.Log($" [Player] === {currentState} 에서 {nextState} 으로 상태 변경!! ===");
        if (currentState == nextState) {Debug.Log("currentState == nextState"); return; }
        if (nextState == PlayerState.SHOOTING && currentState == PlayerState.IDLE) { Debug.Log("current state가 aiming이 아니었다면, shoot 불가!"); return; }

        ExitState(currentState);
        currentState = nextState;
        EnterState(currentState);


    }

    private void EnterState(PlayerState state)
    {
        switch (state)
        {
            case PlayerState.IDLE:
                {
                    EnterIdle();
                    break;
                }
            case PlayerState.MOVING:
                {
                    EnterMoving();
                    break;
                }
            case PlayerState.AIMING:
                {
                    EnterAiming();
                    break;
                }
            case PlayerState.SHOOTING:
                {
                    EnterShooting();
                    break;
                }
            case PlayerState.DEAD:
                {
                    EnterDead();
                    break;
                }
        }
    }

    private void DoState(PlayerState state)
    {
        switch (state)
        {
            case PlayerState.IDLE:
                {
                    DoIdle();
                    break;
                }
            case PlayerState.MOVING:
                {
                    DoMoving();
                    break;
                }
            case PlayerState.AIMING:
                {
                    DoAiming();
                    break;
                }
            case PlayerState.SHOOTING:
                {
                    DoShooting();
                    break;
                }
            case PlayerState.DEAD:
                {
                    DoDead();
                    break;
                }
        }
    }

    private void ExitState(PlayerState state)
    {
        switch (state)
        {
            case PlayerState.IDLE:
                {
                    ExitIdle();
                    break;
                }
            case PlayerState.MOVING:
                {
                    ExitMoving();
                    break;
                }
            case PlayerState.AIMING:
                {
                    ExitAiming();
                    break;
                }
            case PlayerState.SHOOTING:
                {
                    ExitShooting();
                    break;
                }
            case PlayerState.DEAD:
                {
                    ExitDead();
                    break;
                }
        }
    }
    
    public void ResetState()
    {
        animator.SetBool(PLAYER_IDLE, false);
        animator.SetBool(PLAYER_MOVING, false);
        animator.SetBool(PLAYER_AIMING, false);
        animator.SetBool(PLAYER_SHOOTING, false);
        animator.speed = 1f;

        currentState = PlayerState.NONE;

        ChangeState(PlayerState.IDLE);
    }
    #endregion

    #region state - idle
    private void EnterIdle()
    {
        animator.SetBool(PLAYER_IDLE, true);
    }
    
    private void DoIdle()
    {

    }

    private void ExitIdle()
    {
        animator.SetBool(PLAYER_IDLE, false);
    }
    #endregion

    #region state - moving
    private void EnterMoving()
    {
        animator.speed = levelData.player.speed;
        animator.SetBool(PLAYER_MOVING, true);
    }
    private void DoMoving()
    {
        transform.Translate(Vector3.left * levelData.player.speed * Time.deltaTime);
    }

    private void ExitMoving()
    {
        animator.speed = 1f;
        animator.SetBool(PLAYER_MOVING, false);
    }
    #endregion

    #region state - aiming
    private void EnterAiming()
    {
        animator.SetBool(PLAYER_AIMING, true);
        attacker.DoAiming();
    }
    
    private void DoAiming()
    {
        attacker.UpdateAiming();
    }

    private void ExitAiming()
    {
        animator.SetBool(PLAYER_AIMING, false);
    }
    #endregion

    #region state - shooting
    private void EnterShooting()
    {
        bool didFire = attacker.DoFire();
        if (didFire)
        {
            bool hasArrowsLeft = InGameManager.Instance.TryThrow();

            if (hasArrowsLeft)
            {
                animator.SetBool(PLAYER_SHOOTING, true);
            }
            else
            {
                isShootandOver = true;
                InGameManager.Instance.GameEnd(false).Forget();
            }
        }
        else
        {
            ChangeState(PlayerState.IDLE);
        }


    }
    
    private void DoShooting()
    {

    }

    private void ExitShooting()
    {
        animator.SetBool(PLAYER_SHOOTING, false);
    }
    #endregion

    #region state - dead
    private void EnterDead()
    {
        ResetState();

        if (!isShootandOver)
        {
            animator.SetTrigger("Dead");
        }
        else
        {
            animator.SetTrigger("ShootAndOver");
        }
    }
    private void DoDead()
    {

    }

    private void ExitDead()
    {

    }
    #endregion

    public void OnGameEnd(bool isWin)
    {
        animator.speed = 1f;
        attacker.HideDots();
        if (isWin)
        {
            ChangeState(PlayerState.IDLE);
        }
        else
        {
            ChangeState(PlayerState.DEAD);
        }
    }

    public void OnGamePause(bool isPause)
    {
        if (isPause)
        {
            lastAnimatorSpeed = animator.speed;
            animator.speed = 0;
        }
        else
        {
            animator.speed = lastAnimatorSpeed;
        }
    }

    public bool IsMoving()
    {
        return currentState == PlayerState.MOVING;
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (!InGameManager.Instance.isGameActive) return;

        int hitLayer = 1 << collision.gameObject.layer;
        if ((hitLayer & enemyLayer) != 0)
        {
            InGameManager.Instance.OnPlyerHit();
        }
    }    
}
