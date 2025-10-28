using UnityEngine;

public class CharacterYSort : MonoBehaviour
{
    [Tooltip("Hệ số nhân để làm mịn thứ tự (mặc định 100)")]
    public int sortPrecision = 100;

    [Tooltip("Offset chung cho toàn bộ nhóm object")]
    public int baseOffset = 0;

    private SpriteRenderer[] renderers;
    private Vector3 lastPosition;

    void Awake() => CacheRenderers();

#if UNITY_EDITOR
    void OnValidate() => CacheRenderers();
#endif

    void CacheRenderers()
    {
        renderers = GetComponentsInChildren<SpriteRenderer>(includeInactive: true);
        lastPosition = transform.position;
    }

    void LateUpdate()
    {
        // 🔸 Chỉ cập nhật nếu vị trí Y thay đổi
        if (Mathf.Abs(transform.position.y - lastPosition.y) < 0.001f)
            return;

        lastPosition = transform.position;

        foreach (var sr in renderers)
        {
            if (sr == null) continue;

            string layerName = sr.sortingLayerName;
            int originalOrder = sr.sortingOrder;

            int ySort = Mathf.RoundToInt(-sr.transform.position.y * sortPrecision);
            int newOrder = ySort + baseOffset + originalOrder;

            if (sr.sortingOrder != newOrder)
            {
                sr.sortingOrder = newOrder;
                sr.sortingLayerName = layerName;
            }
        }
    }
}
