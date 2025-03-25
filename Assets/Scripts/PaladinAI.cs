using UnityEngine;

public class PaladinAI : EnemyAI
{
    //--------------- 节拍行为配置 ---------------
    [Header("节拍行为配置")]
    //[SerializeField] private GameObject _chargeEffect;   // 蓄力特效
    //[SerializeField] private GameObject _dashEffect;     // 冲锋特效
    //[SerializeField] private GameObject _aoeEffect;      // 范围攻击特效
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
    private bool _isInitialized;

    protected override void Start()
    {
        base.Start();

        // 确保单例正确初始化
        _beatManager = BeatManager.Instance;

        // 防御性事件订阅
        if (!_isInitialized)
        {
            _beatManager.OnBeat -= OnBeatAction; // 防止重复订阅
            _beatManager.OnBeat += OnBeatAction;
            _isInitialized = true;
        }
        // 强制状态重置
        _currentPhase = BossPhase.Idle;
        _beatCounter = 0;
        _restCounter = 0;

        //_chargeEffect.SetActive(false);
        //_dashEffect.SetActive(false);
        //_aoeEffect.SetActive(false);
    }



    void OnDestroy()
    {
        if (_beatManager != null)
            _beatManager.OnBeat -= OnBeatAction;
    }

    private void OnBeatAction()
    {
        // 状态优先级处理
        if (_currentPhase == BossPhase.Resting)
        {
            _restCounter++;
            if (_restCounter >= _restBeats)
            {
                _currentPhase = BossPhase.Idle;
                Debug.Log("<color=green>[状态] 结束待机</color>");
            }
            return;
        }

        _beatCounter++;
        Debug.Log($"<color=blue>[节拍] 当前节拍: {_beatCounter} | 状态: {_currentPhase}</color>");

        switch (_currentPhase)
        {
            case BossPhase.Idle:
                if (ShouldStartCharging())
                {
                    StartCharging();
                    _beatCounter = 0; // 重置节拍计数器
                }
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
        }
    }

    //--------------- 主要行为逻辑 ---------------
    private bool ShouldStartCharging()
    {
        // 增加节拍数有效性验证
        return _beatCounter > 0 && _beatCounter % 5 == 0;
    }

    private void StartCharging()
    {
        _currentPhase = BossPhase.Charging;
        //_chargeEffect.SetActive(true);
        animator.Play("Charge");
        Debug.Log("<color=yellow>[行为] 开始蓄力</color>");
    }

    private void ChooseAttackPattern()
    {
        //_chargeEffect.SetActive(false);
        _currentPhase = BossPhase.Idle; // 强制状态重置

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
        animator.SetTrigger("Attack");
        //_dashEffect.SetActive(true);
        Debug.Log("<color=cyan>[行为] 启动冲锋攻击</color>");
    }

    private void ExecuteDashStep()
    {
        if (_actionStep >= 3)
        {
            EndAction();
            return;
        }

        Vector3 targetPos = transform.position +
            new Vector3(_dashDirection.x, _dashDirection.y, 0) * _grid.gridSize * 2;

        if (_grid.IsPositionValid(targetPos))
        {
            transform.position = targetPos;
            CheckCollision(_dashDirection);
            _actionStep++;
            Debug.Log($"<color=cyan>[冲锋] 步骤 {_actionStep}/3</color>");
        }
        else
        {
            EndAction();
        }
    }
    //--------------- 多重范围攻击逻辑 ---------------
    private void InitMultiAttack()
    {
        _currentPhase = BossPhase.MultiAttack;
        _actionStep = 0;
        //_aoeEffect.SetActive(true);
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
        animator.ResetTrigger("Attack");
        //_dashEffect.SetActive(false);
        //_aoeEffect.SetActive(false);
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
