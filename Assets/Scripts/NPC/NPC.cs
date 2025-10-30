using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class NPC : MonoBehaviour
{
    public NPCName npcName;
    private NPCDialogueController dialogueController;
    private NPCAnimatonController animatonController;
    private bool loadedData=false;

    private void Start()
    {
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
}
