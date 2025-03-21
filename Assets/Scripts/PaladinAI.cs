using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PaladinAI : EnemyAI
{
    // 特有字段
    public float moveSpeed = 5f;
    private enum BossState { Idle, Moving, Charging, Attacking }
    private BossState _currentState = BossState.Idle;
    private BeatManager _beatManager;
    private Vector2Int _nextMoveDirection;
    private int _beatCounter;

    // 覆盖基类Start方法
    protected override void Start()
    {
        base.Start(); // 必须调用基类初始化
        _beatManager = BeatManager.Instance;
        _beatManager.OnBeat += OnBeat;
        health = 10; // 初始化Boss血量
        attackPower = 2;
    }

    void OnDestroy()
    {
        if (_beatManager != null)
            _beatManager.OnBeat -= OnBeat;
    }

    // 特有逻辑：节拍驱动
    private void OnBeat()
    {
        _beatCounter++;
        if (_currentState == BossState.Idle)
        {
            DecideNextAction();
        }
    }

    void DecideNextAction()
    {
        if (_beatCounter % 3 == 0)
        {
            StartCoroutine(ChargeAndAttack());
        }
        else if (_beatCounter % 2 == 0)
        {
            StartCoroutine(Move());
        }
    }

    // 移动逻辑
    IEnumerator Move()
    {
        _currentState = BossState.Moving;
        _nextMoveDirection = GetRandomDirection();

        Vector3 targetPos = transform.position + new Vector3(_nextMoveDirection.x, _nextMoveDirection.y, 0) * _grid.gridSize;

        // 检测碰撞
        if (_grid.IsPositionValid(targetPos))
        {
            // 平滑移动
            float elapsed = 0;
            Vector3 startPos = transform.position;
            while (elapsed < moveSpeed)
            {
                transform.position = Vector3.Lerp(startPos, targetPos, elapsed / moveSpeed);
                elapsed += Time.deltaTime;
                yield return null;
            }
            transform.position = targetPos;

            // 推动玩家或造成伤害
            CheckPlayerPush(_nextMoveDirection);
        }

        _currentState = BossState.Idle;
    }

    // 蓄力攻击逻辑
    IEnumerator ChargeAndAttack()
    {
        _currentState = BossState.Charging;
        // 显示蓄力特效（需美术支持）
        yield return new WaitForSeconds(_beatManager.GetBeatInterval());

        _currentState = BossState.Attacking;
        AreaAttack();
        _currentState = BossState.Idle;
    }

    // 推动玩家检测
    void CheckPlayerPush(Vector2Int direction)
    {
        Vector3 pushTarget = transform.position + new Vector3(direction.x, direction.y, 0) * _grid.gridSize;
        Collider2D hit = Physics2D.OverlapPoint(pushTarget);
        if (hit != null && hit.CompareTag("Player"))
        {
            Vector3 playerNewPos = hit.transform.position + new Vector3(direction.x, direction.y, 0) * _grid.gridSize;
            if (_grid.IsPositionValid(playerNewPos))
            {
                hit.transform.position = playerNewPos;
            }
            else
            {
                hit.GetComponent<PlayerController>().TakeDamage(2);
            }
        }
    }

    // 范围攻击
    void AreaAttack()
    {
        Collider2D[] hits = Physics2D.OverlapBoxAll(transform.position, new Vector2(_grid.gridSize * 3, _grid.gridSize * 3), 0);
        foreach (var hit in hits)
        {
            if (hit.CompareTag("Player"))
            {
                hit.GetComponent<PlayerController>().TakeDamage(attackPower);
            }
        }
    }

    // 随机选择方向
    Vector2Int GetRandomDirection()
    {
        Vector2Int[] directions = { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };
        return directions[Random.Range(0, directions.Length)];
    }

    // 覆盖受伤逻辑：添加Boss受击特效
    public override void TakeDamage(int damage)
    {
        base.TakeDamage(damage); // 调用基类逻辑
       
    }
}
