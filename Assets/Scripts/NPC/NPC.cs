using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class NPC : BaseObject
{
    public NPCName npcName;
    private NPCDialogueController dialogueController;
    private NPCAnimatonController animatonController;
    private bool loadedData=false;

    private void Start()
    {
        if(!loadedData) 
            LoadData();
    }

    public void Init(NPCName name)
    {
        npcName = name;
        LoadData();
    }

    private void LoadData()
    {
        if (loadedData)
            return;

        dialogueController = GetComponent<NPCDialogueController>();
        animatonController = GetComponent<NPCAnimatonController>();

        dialogueController.LoadDialogueData(npcName);
        animatonController.LoadIdleSprites(npcName);


        loadedData = true;
    }

    public override void AutoMoveToXY(int x, int y)
    {

    }

    public override void DestroyObject()
    {

    }

    public override ObjectType GetObjectType()
    {
        return ObjectType.NPC;
    }
}
