
using UnityEngine;

// EnemyAI.cs




public class EnemyAI : MonoBehaviour
{
    // �����ֶΣ���Ϊprotected����������ʣ�
    public int health = 1;
    public int attackPower = 1;
    public GameObject deathEffectPrefab;
    [SerializeField] protected Animator animator; // ��Ϊprotected

    // �������
    protected Transform _player;
    protected GridSystem _grid;

    protected virtual void Start()
    {
        _player = PlayerController.Instance.transform;
        _grid = GridSystem.Instance;
        animator = GetComponent<Animator>(); // �Զ���ȡAnimator
    }

    // �鷽��������������д�����߼�
    public virtual void TakeDamage(int damage)
    {
        health -= damage;
        animator.SetTrigger("Hurt");

        if (health <= 0)
        {
            Die();
        }
    }

    // �鷽��������������չ�����߼�
    protected virtual void Die()
    {
        if (deathEffectPrefab != null)
        {
            Instantiate(deathEffectPrefab, transform.position, Quaternion.identity);
        }
        Destroy(gameObject);
    }
}