using UnityEngine;
using UnityEngine.Tilemaps;

public class TilemapBeatSwitcher : MonoBehaviour
{
    [Header("Tilemap References")]
    [SerializeField] private Tilemap _tilemapA;
    [SerializeField] private Tilemap _tilemapB;

    [Header("Beat Settings")]
    [SerializeField] private int _switchInterval = 1; // 切换间隔（每几个节拍切换一次）

    private int _currentBeatCount;
    private bool _isTilemapAActive = true;

    void Start()
    {
        // 初始状态设置
        SetTilemapState(_tilemapA, true);
        SetTilemapState(_tilemapB, false);

        // 订阅节拍事件
        BeatManager.Instance.OnBeat += HandleBeat;
    }

    void OnDestroy()
    {
        // 取消订阅
        if (BeatManager.Instance != null)
            BeatManager.Instance.OnBeat -= HandleBeat;
    }

    private void HandleBeat()
    {
        _currentBeatCount++;

        // 按间隔切换
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