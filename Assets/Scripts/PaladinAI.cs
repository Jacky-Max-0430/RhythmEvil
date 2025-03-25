using UnityEngine;

public class PaladinAI : EnemyAI
{
    //--------------- ������Ϊ���� ---------------
    [Header("������Ϊ����")]
    //[SerializeField] private GameObject _chargeEffect;   // ������Ч
    //[SerializeField] private GameObject _dashEffect;     // �����Ч
    //[SerializeField] private GameObject _aoeEffect;      // ��Χ������Ч
    [SerializeField] private int _restBeats = 2;         // ����������

    //--------------- ����ʱ״̬ ---------------
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
    private int _actionStep;      // ��ǰ�ж�����
    private Vector2Int _dashDirection; // ��淽��
    private int _restCounter;     // ����������
    private bool _isInitialized;

    protected override void Start()
    {
        base.Start();

        // ȷ��������ȷ��ʼ��
        _beatManager = BeatManager.Instance;

        // �������¼�����
        if (!_isInitialized)
        {
            _beatManager.OnBeat -= OnBeatAction; // ��ֹ�ظ�����
            _beatManager.OnBeat += OnBeatAction;
            _isInitialized = true;
        }
        // ǿ��״̬����
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
        // ״̬���ȼ�����
        if (_currentPhase == BossPhase.Resting)
        {
            _restCounter++;
            if (_restCounter >= _restBeats)
            {
                _currentPhase = BossPhase.Idle;
                Debug.Log("<color=green>[״̬] ��������</color>");
            }
            return;
        }

        _beatCounter++;
        Debug.Log($"<color=blue>[����] ��ǰ����: {_beatCounter} | ״̬: {_currentPhase}</color>");

        switch (_currentPhase)
        {
            case BossPhase.Idle:
                if (ShouldStartCharging())
                {
                    StartCharging();
                    _beatCounter = 0; // ���ý��ļ�����
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

    //--------------- ��Ҫ��Ϊ�߼� ---------------
    private bool ShouldStartCharging()
    {
        // ���ӽ�������Ч����֤
        return _beatCounter > 0 && _beatCounter % 5 == 0;
    }

    private void StartCharging()
    {
        _currentPhase = BossPhase.Charging;
        //_chargeEffect.SetActive(true);
        animator.Play("Charge");
        Debug.Log("<color=yellow>[��Ϊ] ��ʼ����</color>");
    }

    private void ChooseAttackPattern()
    {
        //_chargeEffect.SetActive(false);
        _currentPhase = BossPhase.Idle; // ǿ��״̬����

        if (Random.value > 0.5f)
        {
            InitDashAttack();
        }
        else
        {
            InitMultiAttack();
        }
    }

    //--------------- ��湥���߼� ---------------
    private void InitDashAttack()
    {
        _currentPhase = BossPhase.Dashing;
        _actionStep = 0;
        _dashDirection = GetPlayerDirection();
        animator.SetTrigger("Attack");
        //_dashEffect.SetActive(true);
        Debug.Log("<color=cyan>[��Ϊ] ������湥��</color>");
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
            Debug.Log($"<color=cyan>[���] ���� {_actionStep}/3</color>");
        }
        else
        {
            EndAction();
        }
    }
    //--------------- ���ط�Χ�����߼� ---------------
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

    //--------------- ͨ���߼� ---------------
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
            EndAction(); // ��ײ����������
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

    //--------------- ��д���෽�� ---------------
    public override void TakeDamage(int damage)
    {
        if (_currentPhase != BossPhase.Resting) return; // ������Ϣʱ�ܻ�
        base.TakeDamage(damage);
    }
}
