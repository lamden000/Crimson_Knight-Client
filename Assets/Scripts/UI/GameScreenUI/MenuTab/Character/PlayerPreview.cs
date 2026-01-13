using UnityEngine;

public class PlayerPreview : MonoBehaviour
{
    public Camera previewCamera;
    public Transform previewRoot;

    private GameObject previewPlayer;

    public void Show(Player sourcePlayer)
    {
        Clear();

        // Clone player
        previewPlayer = Instantiate(sourcePlayer.gameObject, previewRoot);
        previewPlayer.transform.localPosition = Vector3.zero;
        previewPlayer.transform.localScale = Vector3.one;

        // Disable toàn bộ logic
        foreach (var mb in previewPlayer.GetComponentsInChildren<MonoBehaviour>())
            mb.enabled = false;

        // Set layer cho camera nhìn thấy
        int layer = LayerMask.NameToLayer("PlayerPreview");
        SetLayerRecursively(previewPlayer, layer);

        // Đặt vị trí camera
        previewCamera.transform.localPosition = new Vector3(0, 0, -10);
    }

    public void Clear()
    {
        if (previewPlayer != null)
            Destroy(previewPlayer);
    }

    void SetLayerRecursively(GameObject obj, int layer)
    {
        obj.layer = layer;
        foreach (Transform child in obj.transform)
            SetLayerRecursively(child.gameObject, layer);
    }
}
