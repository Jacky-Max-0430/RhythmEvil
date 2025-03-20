
using UnityEngine;

// PlayerController.cs
public class PlayerController : MonoBehaviour
{
    public int health = 4;
    public int attackPower = 1;
    public float moveSpeed = 5f; // �ƶ������ٶ�
    public Animator animator;    // ����������

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

        // ��ʼ��CDΪ���ļ��
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

    void StartMovement()
    {
        _isMoving = true;
        _moveCooldown = _beatManager.GetBeatInterval(); // ����CD
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