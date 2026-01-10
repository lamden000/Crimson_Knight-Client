using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class NPCPrefab : MonoBehaviour
{
    private NPCAnimatonController animatonController;

    private NpcTemplate template;
    private void Start()
    {
        LoadData();
    }

    public void Init(NpcTemplate template)
    {
        this.template = template;
        LoadData();
    }

    private void LoadData()
    {
        animatonController = GetComponent<NPCAnimatonController>();
        animatonController.LoadIdleSprites(template);
    }
}
