using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class PkIconManager : MonoBehaviour
{
    private SpriteRenderer _spriteRenderer;

    private static Dictionary<PkType, Sprite[]> _iconCache = new Dictionary<PkType, Sprite[]>();

    private Sprite[] _currentFrames; 
    private int _frameIndex = 0;
    private float _timer = 0f;
    private float _animationSpeed = 0.15f; 

    void Awake()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _spriteRenderer.enabled = false;

        if (_iconCache.Count == 0)
        {
            LoadIcons();
        }
    }

    private void LoadIcons()
    {
        string folderPath = "iconpk/";

        _iconCache[PkType.Red] = Resources.LoadAll<Sprite>(folderPath + "red");
        _iconCache[PkType.Green] = Resources.LoadAll<Sprite>(folderPath + "green");
        _iconCache[PkType.Blue] = Resources.LoadAll<Sprite>(folderPath + "blue");
        _iconCache[PkType.Yellow] = Resources.LoadAll<Sprite>(folderPath + "yellow");

        foreach (var kvp in _iconCache)
        {
            if (kvp.Value == null || kvp.Value.Length == 0)
            {
                Debug.Log("Khong thay icon pk");
            }
        }
    }

    void Update()
    {
        if (_currentFrames == null || _currentFrames.Length == 0 || !_spriteRenderer.enabled) return;

        _timer += Time.deltaTime;
        if (_timer >= _animationSpeed)
        {
            _timer = 0f;
            _frameIndex++;

            if (_frameIndex >= _currentFrames.Length)
            {
                _frameIndex = 0;
            }

            _spriteRenderer.sprite = _currentFrames[_frameIndex];
        }
    }

    public void SetPkState(PkType type)
    {
        if (type == PkType.None)
        {
            _spriteRenderer.enabled = false;
            _currentFrames = null;
            return;
        }

        if (_iconCache.TryGetValue(type, out Sprite[] icons) && icons != null && icons.Length > 0)
        {
            _currentFrames = icons;
            _frameIndex = 0;
            _timer = 0f;

            _spriteRenderer.sprite = _currentFrames[0];
            _spriteRenderer.enabled = true;
        }
        else
        {
            _spriteRenderer.enabled = false;
            _currentFrames = null;
            Debug.LogWarning($"Chua load duoc sprite cho loai: {type}");
        }
    }
}