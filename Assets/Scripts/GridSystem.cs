
using UnityEngine;


public class GridSystem : MonoBehaviour
{

    public static GridSystem Instance; // ������̬����

    public int gridSize = 1;//���ӵ�λ����
    public Vector2Int mapSize = new Vector2Int(10, 10);//��ͼ�ߴ�

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

    public Vector3 SnapToGrid(Vector3 position)
    {
        int x = Mathf.RoundToInt(position.x / gridSize) * gridSize;
        int y = Mathf.RoundToInt(position.y / gridSize) * gridSize;
        return new Vector3(x, y, 0);
    }

    public bool IsPositionValid(Vector3 targetPos)
    {
        // ����Ƿ񳬳���ͼ��Χ�������ϰ���
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