using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NPCDialogue", menuName = "Scriptable Objects/NPCDialogue")]
public class NPCDialogue : ScriptableObject
{
    public string npcName;
    public Sprite npcPotrait;
    public float typingSpeed = 0.05f;
    public float voicePitch=1f;
    public bool[] autoProgressLines;
    public float autoProgressDelay = 1.5f;
    public NPCMenu npcMenu;
    [SerializeField]
    private List<DialogueEntry> dialogueList = new List<DialogueEntry>();

    private Dictionary<string, DialogueEntry> dialogueDict;

    private void OnEnable()
    {
        BuildDictionary();
    }

    private void BuildDictionary()
    {
        dialogueDict = new Dictionary<string, DialogueEntry>();

        foreach (var entry in dialogueList)
        {
            if (!dialogueDict.ContainsKey(entry.key))
                dialogueDict.Add(entry.key, entry);
            else
                Debug.LogWarning($"Duplicate dialogue key found: {entry.key}");
        }
    }

    public DialogueEntry GetEntry(string key)
    {
        if (dialogueDict == null) BuildDictionary();
        if (dialogueDict.TryGetValue(key, out var entry))
            return entry;

        Debug.LogWarning($"Dialogue key not found: {key}");
        return null;
    }
}
