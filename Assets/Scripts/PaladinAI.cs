using UnityEngine;

public class PaladinAI : EnemyAI
{
    //--------------- 节拍行为配置 ---------------
    [Header("节拍行为配置")]
    [SerializeField] private GameObject _chargeEffect;   // 蓄力特效
    [SerializeField] private GameObject _dashEffect;     // 冲锋特效
    [SerializeField] private GameObject _aoeEffect;      // 范围攻击特效
    [SerializeField] private int _restBeats = 2;         // 待机节拍数

    //--------------- 运行时状态 ---------------
    private enum BossPhase
    {
        Idle,
        Charging,
        Dashing,
        MultiAttack,
        Resting
    }

    private BossPhase _currentPhase = BossPhase.Idle;
    private BeatManager _beatManager;
    private int _beatCounter;
    private int _actionStep;      // 当前行动步骤
    private Vector2Int _dashDirection; // 冲锋方向
    private int _restCounter;     // 待机计数器

    protected override void Start()
    {
        base.Start();
        health = 10;
        attackPower = 2;
        _beatManager = BeatManager.Instance;
        _beatManager.OnBeat += OnBeatAction;

        _chargeEffect.SetActive(false);
        _dashEffect.SetActive(false);
        _aoeEffect.SetActive(false);
    }

    void OnDestroy()
    {
        if (_beatManager != null)
            _beatManager.OnBeat -= OnBeatAction;
    }

    private void OnBeatAction()
    {
        _beatCounter++;

        switch (_currentPhase)
        {
            case BossPhase.Idle:
                if (ShouldStartCharging())
                    StartCharging();
                break;

            case BossPhase.Charging:
                ChooseAttackPattern();
                break;

            case BossPhase.Dashing:
                ExecuteDashStep();
                break;

            case BossPhase.MultiAttack:
                ExecuteAoEStep();
                break;

            case BossPhase.Resting:
                HandleResting();
                break;
        }
    }

    //--------------- 主要行为逻辑 ---------------
    private bool ShouldStartCharging()
    {
        // 每5拍触发一次蓄力
        return _beatCounter % 5 == 0;
    }

    private void StartCharging()
    {
        _currentPhase = BossPhase.Charging;
        _chargeEffect.SetActive(true);
        animator.Play("Charge");
    }

    private void ChooseAttackPattern()
    {
        _chargeEffect.SetActive(false);

        // 50%概率选择攻击模式
        if (Random.value > 0.5f)
        {
            InitDashAttack();
        }
        else
        {
            InitMultiAttack();
        }
    }

    //--------------- 冲锋攻击逻辑 ---------------
    private void InitDashAttack()
    {
        _currentPhase = BossPhase.Dashing;
        _actionStep = 0;
        _dashDirection = GetPlayerDirection();
        _dashEffect.SetActive(true);
    }

    private void ExecuteDashStep()
    {
        if (_actionStep >= 3)
        {
            EndAction();
            return;
        }

        // 每次移动两格
        for (int i = 0; i < 2; i++)
        {
            Vector3 targetPos = transform.position +
                new Vector3(_dashDirection.x, _dashDirection.y, 0) * _grid.gridSize;

            if (_grid.IsPositionValid(targetPos))
            {
                transform.position = targetPos;
                CheckCollision(_dashDirection);
            }
            else
            {
                // 撞墙提前结束
                EndAction();
                return;
            }
        }

        _actionStep++;
        animator.Play("DashMove");
    }

    //--------------- 多重范围攻击逻辑 ---------------
    private void InitMultiAttack()
    {
        _currentPhase = BossPhase.MultiAttack;
        _actionStep = 0;
        _aoeEffect.SetActive(true);
    }

    private void ExecuteAoEStep()
    {
        if (_actionStep >= 3)
        {
            EndAction();
            return;
        }

        Collider2D[] hits = Physics2D.OverlapCircleAll(
            transform.position,
            _grid.gridSize * 1.5f
        );

        foreach (var hit in hits)
        {
            if (hit.CompareTag("Player"))
            {
                hit.GetComponent<PlayerController>().TakeDamage(1);
            }
        }

        _actionStep++;
        animator.Play("AoEAttack");
    }

    //--------------- 通用逻辑 ---------------
    private Vector2Int GetPlayerDirection()
    {
        Vector3 dir = (PlayerController.Instance.transform.position - transform.position).normalized;
        return new Vector2Int(
            Mathf.RoundToInt(dir.x),
            Mathf.RoundToInt(dir.y)
        );
    }

    private void CheckCollision(Vector2Int direction)
    {
        Collider2D hit = Physics2D.OverlapPoint(transform.position);
        if (hit != null && hit.CompareTag("Player"))
        {
            hit.GetComponent<PlayerController>().TakeDamage(2);
            EndAction(); // 碰撞后立即结束
        }
    }

    private void EndAction()
    {
        _currentPhase = BossPhase.Resting;
        _restCounter = 0;
        _dashEffect.SetActive(false);
        _aoeEffect.SetActive(false);
    }

    private void HandleResting()
    {
        _restCounter++;
        if (_restCounter >= _restBeats)
        {
            _currentPhase = BossPhase.Idle;
        }
    }

    //--------------- 重写基类方法 ---------------
    public override void TakeDamage(int damage)
    {
        if (_currentPhase != BossPhase.Resting) return; // 仅限休息时受击
        base.TakeDamage(damage);
    }
}
