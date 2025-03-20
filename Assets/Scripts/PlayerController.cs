
using UnityEngine;

// PlayerController.cs
public class PlayerController : MonoBehaviour
{
    public int health = 4;
    public int attackPower = 1;
    public float moveSpeed = 5f; // 移动动画速度
    public Animator animator;    // 动画控制器

    private Vector2Int _moveDirection;
    private GridSystem _grid;
    private BeatManager _beatManager;
    private float _moveCooldown;
    private bool _isMoving;
    private Vector3 _targetPosition;

    void Start()
    {
        _grid = GridSystem.Instance;
        _beatManager = BeatManager.Instance;
        animator = GetComponent<Animator>();

        // 初始化CD为节拍间隔
        _moveCooldown = _beatManager.GetBeatInterval();
    }

    void Update()
    {
        HandleInput();
        UpdateCooldown();
        MoveWithAnimation();
    }

    void HandleInput()
    {
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

    void StartMovement()
    {
        _isMoving = true;
        _moveCooldown = _beatManager.GetBeatInterval(); // 重置CD
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
        if (_moveCooldown > 0)
            _moveCooldown -= Time.deltaTime;
    }

    bool CheckCollision(Vector3 targetPos)
    {
        Collider2D hit = Physics2D.OverlapPoint(targetPos);
        return hit != null && hit.CompareTag("Enemy");
    }

    //public void TakeDamage(int damage) => GameManager.Instance.PlayerTakeDamage(damage);
}