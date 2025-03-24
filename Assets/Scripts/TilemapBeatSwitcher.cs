using UnityEngine;
using UnityEngine.Tilemaps;

public class TilemapBeatSwitcher : MonoBehaviour
{
    [Header("Tilemap References")]
    [SerializeField] private Tilemap _tilemapA;
    [SerializeField] private Tilemap _tilemapB;

    [Header("Beat Settings")]
    [SerializeField] private int _switchInterval = 1; // �л������ÿ���������л�һ�Σ�

    private int _currentBeatCount;
    private bool _isTilemapAActive = true;

    void Start()
    {
        // ��ʼ״̬����
        SetTilemapState(_tilemapA, true);
        SetTilemapState(_tilemapB, false);

        // ���Ľ����¼�
        BeatManager.Instance.OnBeat += HandleBeat;
    }

    void OnDestroy()
    {
        // ȡ������
        if (BeatManager.Instance != null)
            BeatManager.Instance.OnBeat -= HandleBeat;
    }

    private void HandleBeat()
    {
        _currentBeatCount++;

        // ������л�
        if (_currentBeatCount % _switchInterval == 0)
        {
            _isTilemapAActive = !_isTilemapAActive;
            SetTilemapState(_tilemapA, _isTilemapAActive);
            SetTilemapState(_tilemapB, !_isTilemapAActive);
        }
    }

    private void SetTilemapState(Tilemap tilemap, bool isActive)
    {
        if (tilemap != null)
        {
            tilemap.gameObject.SetActive(isActive);
        }
        else
        {
            Debug.LogWarning("Tilemap reference is missing!");
        }
    }
}