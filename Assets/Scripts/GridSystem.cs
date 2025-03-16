using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class GridSystem : MonoBehaviour
{

    public static GridSystem Instance; // 单例静态引用

    public int gridSize = 1;//格子单位长度
    public Vector2Int mapSize = new Vector2Int(10, 10);//地图尺寸

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

    public Vector3 SnapToGrid(Vector3 position)
    {
        int x = Mathf.RoundToInt(position.x / gridSize) * gridSize;
        int y = Mathf.RoundToInt(position.y / gridSize) * gridSize;
        return new Vector3(x, y, 0);
    }

    public bool IsPositionValid(Vector3 targetPos)
    {
        // 检测是否超出地图范围或已有障碍物
        return targetPos.x >= -mapSize.x / 2 && targetPos.x <= mapSize.x / 2
            && targetPos.y >= -mapSize.y / 2 && targetPos.y <= mapSize.y / 2;
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.gray;
        for (int x = -mapSize.x / 2; x <= mapSize.x / 2; x++)
            for (int y = -mapSize.y / 2; y <= mapSize.y / 2; y++)
                Gizmos.DrawWireCube(new Vector3(x, y, 0), Vector3.one * gridSize);
    }
}