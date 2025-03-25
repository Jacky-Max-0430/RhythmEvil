
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// PlayerController.cs
public class PlayerController : MonoBehaviour
{
    public int health = 4;
    public int attackPower = 1;
    public float moveSpeed = 5f; // �ƶ������ٶ�
    public Animator animator;    // ����������
    public float attackDuration = 0.3f; // ��������ʱ��
    public GameObject deathEffectPrefab;

    private Vector2Int _moveDirection;
    private GridSystem _grid;
    private BeatManager _beatManager;
    private float _moveCooldown;
    private float _attackCooldown;
    private bool _isMoving;
    private bool _isAttacking;
    private Vector3 _targetPosition;
    public static PlayerController Instance; // ������̬����

    void Awake()
    {
        // ������ʼ��
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject); // �����ظ�ʵ��
        }
    }
    void Start()
    {
        _grid = GridSystem.Instance;
        _beatManager = BeatManager.Instance;
        animator = GetComponent<Animator>();

        // ��ʼ��CDΪ���ļ��
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
        // ��������ƶ�������������ȴ�У����������
        if (_moveCooldown > 0 || _isMoving || _isAttacking || _attackCooldown > 0) return;

        Vector2Int attackDirection = GetAttackDirection();
        if (attackDirection != Vector2Int.zero)
        {
            StartAttack(attackDirection);
            return; // ���ȴ�����
        }

        if (_moveCooldown > 0 || _isMoving) return;

        _moveDirection = Vector2Int.zero;

        // ���ȴ���ֱ�������루W/S��
        if (Input.GetKey(KeyCode.W))
        {
            _moveDirection.y = 1;
        }
        else if (Input.GetKey(KeyCode.S))
        {
            _moveDirection.y = -1;
        }

        // ���û�д�ֱ���룬�ٴ���ˮƽ����A/D��
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
        _attackCooldown = _beatManager.GetBeatInterval()-0.2f; // ���ù�����ȴ�����ƶ�һ�£�
        //animator.SetTrigger("Attack");
        //animator.SetFloat("DirectionX", direction.x);
        //animator.SetFloat("DirectionY", direction.y);
        StartCoroutine(AttackAction(direction));
    }

    IEnumerator AttackAction(Vector2Int direction)
    {
        yield return new WaitForSeconds(attackDuration);

        Vector3 attackPosition = transform.position + new Vector3(direction.x, direction.y, 0) * _grid.gridSize;
        Collider2D hit = Physics2D.OverlapPoint(attackPosition);
        if (hit != null && hit.CompareTag("Enemy"))
        {
            hit.GetComponent<EnemyAI>().TakeDamage(attackPower);
        }

        _isAttacking = false;
    }

    void StartMovement()
    {
        _isMoving = true;
        _moveCooldown = _beatManager.GetBeatInterval()-0.2f; // ����CD
        animator.SetBool("IsMoving", true);
        animator.SetFloat("DirectionX", _moveDirection.x);
        animator.SetFloat("DirectionY", _moveDirection.y);
    }

    void MoveWithAnimation()
    {
        if (!_isMoving) return;

        // ƽ���ƶ�
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