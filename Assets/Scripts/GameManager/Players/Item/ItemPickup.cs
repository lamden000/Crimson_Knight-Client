using UnityEngine;

public class ItemPickup : MonoBehaviour
{
    public BaseItem item;

    public void SetItem(BaseItem item)
    {
        this.item = item;
    }
}
