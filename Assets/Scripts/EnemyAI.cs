
using UnityEngine;

// EnemyAI.cs




public class EnemyAI : MonoBehaviour
{
    // 公共字段（改为protected便于子类访问）
    public int health = 1;
    public int attackPower = 1;
    public GameObject deathEffectPrefab;
    [SerializeField] protected Animator animator; // 改为protected

    // 组件引用
    protected Transform _player;
    protected GridSystem _grid;

    protected virtual void Start()
    {
        _player = PlayerController.Instance.transform;
        _grid = GridSystem.Instance;
        animator = GetComponent<Animator>(); // 自动获取Animator
    }

    // 虚方法：允许子类重写受伤逻辑
    public virtual void TakeDamage(int damage)
    {
        health -= damage;
        animator.SetTrigger("Hurt");

        if (health <= 0)
        {
            Die();
        }
    }

    // 虚方法：允许子类扩展死亡逻辑
    protected virtual void Die()
    {
        if (deathEffectPrefab != null)
        {
            Instantiate(deathEffectPrefab, transform.position, Quaternion.identity);
        }
        Destroy(gameObject);
    }
}