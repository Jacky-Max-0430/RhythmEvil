
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// PlayerController.cs
public class PlayerController : MonoBehaviour
{
    public int health = 4;
    public int attackPower = 1;
    public float moveSpeed = 5f; // 移动动画速度
    public Animator animator;    // 动画控制器
    public float attackDuration = 0.3f; // 攻击动画时长
    public GameObject deathEffectPrefab;

    private Vector2Int _moveDirection;
    private GridSystem _grid;
    private BeatManager _beatManager;
    private float _moveCooldown;
    private float _attackCooldown;
    private bool _isMoving;
    private bool _isAttacking;
    private Vector3 _targetPosition;
    public static PlayerController Instance; // 单例静态引用

    // 在PlayerController类中添加
    public GameObject attackEffectPrefab; // 攻击特效预制体
    public float effectDuration = 0.5f;   // 特效持续时间
    void Awake()
    {
        // 单例初始化
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject); // 避免重复实例
        }
    }
    void Start()
    {
        _grid = GridSystem.Instance;
        _beatManager = BeatManager.Instance;
        animator = GetComponent<Animator>();

        // 初始化CD为节拍间隔
        _moveCooldown = _beatManager.GetBeatInterval();
        _attackCooldown = _beatManager.GetBeatInterval();
    }

    void Update()
    {
        HandleInput();
        UpdateCooldown();
        MoveWithAnimation();
        //if (Input.GetKeyDown(KeyCode.T))
        //    GetComponent<Animator>().SetTrigger("Hurt");

    }

    void HandleInput()
    {
        // 如果正在移动、攻击，或冷却中，则忽略输入
        if (_moveCooldown > 0 || _isMoving || _isAttacking || _attackCooldown > 0) return;

        Vector2Int attackDirection = GetAttackDirection();
        if (attackDirection != Vector2Int.zero)
        {
            StartAttack(attackDirection);
            return; // 优先处理攻击
        }

        if (_moveCooldown > 0 || _isMoving) return;

        _moveDirection = Vector2Int.zero;

        // 优先处理垂直方向输入（W/S）
        if (Input.GetKey(KeyCode.W))
        {
            _moveDirection.y = 1;
        }
        else if (Input.GetKey(KeyCode.S))
        {
            _moveDirection.y = -1;
        }

        // 如果没有垂直输入，再处理水平方向（A/D）
        if (_moveDirection.y == 0)
        {
            if (Input.GetKey(KeyCode.A))
            {
                _moveDirection.x = -1;
            }
            else if (Input.GetKey(KeyCode.D))
            {
                _moveDirection.x = 1;
            }
        }

        if (_moveDirection != Vector2Int.zero)
        {
            _targetPosition = transform.position + new Vector3(_moveDirection.x, _moveDirection.y, 0) * _grid.gridSize;
            if (_grid.IsPositionValid(_targetPosition) && !CheckCollision(_targetPosition))
            {
                StartMovement();
            }
        }
    }

    Vector2Int GetAttackDirection()
    {
        Vector2Int direction = Vector2Int.zero;
        if (Input.GetKey(KeyCode.UpArrow)) direction.y = 1;
        else if (Input.GetKey(KeyCode.DownArrow)) direction.y = -1;
        else if (Input.GetKey(KeyCode.LeftArrow)) direction.x = -1;
        else if (Input.GetKey(KeyCode.RightArrow)) direction.x = 1;
        return direction;
    }

    void StartAttack(Vector2Int direction)
    {
        _isAttacking = true;
        _attackCooldown = _beatManager.GetBeatInterval()-0.2f; // 设置攻击冷却（和移动一致）
        //animator.SetTrigger("Attack");
        //animator.SetFloat("DirectionX", direction.x);
        //animator.SetFloat("DirectionY", direction.y);
        StartCoroutine(AttackAction(direction));
    }

    IEnumerator AttackAction(Vector2Int direction)
    {
        // 生成特效
        if (attackEffectPrefab != null)
        {
            Vector3 spawnPos = transform.position +
                new Vector3(direction.x, direction.y, 0) * _grid.gridSize;

            // 计算特效旋转角度
            Quaternion rotation = Quaternion.Euler(0, 0, GetEffectRotation(direction));

            GameObject effect = Instantiate(
                attackEffectPrefab,
                spawnPos,
                rotation
            );
            Destroy(effect, effectDuration); // 自动销毁
        }

        yield return new WaitForSeconds(attackDuration);
        // ...原有伤害判定逻辑...
        Vector3 attackPosition = transform.position + new Vector3(direction.x, direction.y, 0) * _grid.gridSize;
        Collider2D hit = Physics2D.OverlapPoint(attackPosition);
        if (hit != null && hit.CompareTag("Enemy"))
        {
            hit.GetComponent<EnemyAI>().TakeDamage(attackPower);
        }
    }

    // 根据攻击方向计算特效旋转角度
    float GetEffectRotation(Vector2Int direction)
    {
        if (direction == Vector2Int.up) return 90f;     // 上
        if (direction == Vector2Int.down) return -90f;  // 下
        if (direction == Vector2Int.left) return 180f;  // 左
        return 0f;                                      // 右
    }

    void StartMovement()
    {
        _isMoving = true;
        _moveCooldown = _beatManager.GetBeatInterval()-0.2f; // 重置CD
        animator.SetBool("IsMoving", true);
        animator.SetFloat("DirectionX", _moveDirection.x);
        animator.SetFloat("DirectionY", _moveDirection.y);
    }

    void MoveWithAnimation()
    {
        if (!_isMoving) return;

        // 平滑移动
        transform.position = Vector3.MoveTowards(transform.position, _targetPosition, moveSpeed * Time.deltaTime);

        if (Vector3.Distance(transform.position, _targetPosition) < 0.01f)
        {
            transform.position = _targetPosition;
            _isMoving = false;
            animator.SetBool("IsMoving", false);
        }
    }

    void UpdateCooldown()
    {
        if (_moveCooldown > 0)  _moveCooldown -= Time.deltaTime;
        if (_attackCooldown > 0) _attackCooldown -= Time.deltaTime;
    }

    bool CheckCollision(Vector3 targetPos)
    {
        Collider2D hit = Physics2D.OverlapPoint(targetPos);
        return hit != null && hit.CompareTag("Enemy");
    }

    public void TakeDamage(int damage)
    {
        health -= damage;
        animator.SetTrigger("Hurt");

        if (health <= 0)
        {
            Die();
        }
    }

    
   private void Die()
    {
        if (deathEffectPrefab != null)
        {
            Instantiate(deathEffectPrefab, transform.position, Quaternion.identity);
        }
        Destroy(gameObject);
    }
}