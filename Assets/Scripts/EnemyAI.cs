
using UnityEngine;

// EnemyAI.cs
public enum EnemyType { Melee, Ranged, Boss }

public class EnemyAI : MonoBehaviour
{
    public EnemyType type;
    public int health;
    public int attackPower;

    private Transform _player;
    private GridSystem _grid;

    void Start()
    {
        _player = GameObject.FindGameObjectWithTag("Player").transform;
        _grid = GridSystem.Instance;
    }

    //void OnEnable() => BeatManager.Instance.OnBeat += OnBeatAction;

    //void OnBeatAction()
    //{
    //    switch (type)
    //    {
    //        case EnemyType.Melee:
    //            ChasePlayer();
    //            break;
    //        //case EnemyType.Ranged:
    //        //    ShootProjectile();
    //        //    break;
    //        //case EnemyType.Boss:
    //        //    BossBehavior();
    //            //break;
    //    }
    //}

    //void ChasePlayer()
    //{
    //    Vector3 direction = (_player.position - transform.position).normalized;
    //    Vector3 targetPos = transform.position + direction * _grid.gridSize;
    //    if (_grid.IsPositionValid(targetPos))
    //        transform.position = _grid.SnapToGrid(targetPos);
    //}

    // 其他行为方法...
}