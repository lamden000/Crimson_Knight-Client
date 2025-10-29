using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Unity.VisualScripting;
public class NPCDialogueController : MonoBehaviour
{

    [Header("Dialogue Data")]
    public NPCDialogue dialogueData;
    public string currentQuest;

    [Header("UI References")]
    public GameObject dialoguePanel;
    public TextMeshProUGUI dialogueText;
    public Image portraitImage;
    public Button questButton;
    public Button closeButton;

    private AudioSource audioSource;

    private DialogueEntry currentEntry;
    private int currentLineIndex;
    private bool isTyping;
    private bool skipTyping;
    private bool isDialogueActive;

    private void Start()
    {
        audioSource=GetComponent<AudioSource>();    
    }

    // Call this when player interacts with the NPC
    public void StartDialogue()
    {
        if (isDialogueActive) return;

        if(currentQuest.Length==0)
            questButton.gameObject.SetActive(false);
        dialoguePanel.SetActive(true);
        questButton.onClick.AddListener(OnQuestButton);
        closeButton.onClick.AddListener(CloseDialogue);
        isDialogueActive = true;
        portraitImage.sprite = dialogueData.npcPotrait;
        PlayDialogue("Idle"); 
    }

    private void PlayDialogue(string key)
    {
        var entry = GetDialogueEntry(key);
        if (entry == null)
        {
            Debug.LogWarning($"Dialogue key not found: {key}");
            return;
        }

        currentEntry = entry;
        currentLineIndex = 0;

        StopAllCoroutines();
        StartCoroutine(PlayDialogueCoroutine());
    }

    private IEnumerator PlayDialogueCoroutine()
    {
        while (currentLineIndex < currentEntry.lines.Count)
        {
            yield return StartCoroutine(TypeLine(currentEntry.lines[currentLineIndex],
                GetAudioClip(currentLineIndex)));

            bool auto = (dialogueData.autoProgressLines.Length > currentLineIndex)
                && dialogueData.autoProgressLines[currentLineIndex];

            if (auto)
                yield return new WaitForSeconds(dialogueData.autoProgressDelay);
            else
                yield return new WaitUntil(() => Input.GetKeyDown(KeyCode.Space));

            currentLineIndex++;
        }

        // After Idle or Quest dialogue finishes, show buttons
        questButton.gameObject.SetActive(true);
        closeButton.gameObject.SetActive(true);
    }

    private IEnumerator TypeLine(string line, AudioClip clip)
    {
        isTyping = true;
        skipTyping = false;

        dialogueText.text = "";

        if (clip != null)
        {
            audioSource.pitch = dialogueData.voicePitch;
            audioSource.clip = clip;
            audioSource.Play();
        }

        foreach (char c in line)
        {
            if (skipTyping) break;

            dialogueText.text += c;
            yield return new WaitForSeconds(dialogueData.typingSpeed);
        }

        dialogueText.text = line;
        isTyping = false;
    }

    private AudioClip GetAudioClip(int lineIndex)
    {
        if (currentEntry.audioClips != null && lineIndex < currentEntry.audioClips.Length)
            return currentEntry.audioClips[lineIndex];
        return null;
    }

    private DialogueEntry GetDialogueEntry(string key)
    {
        var field = typeof(NPCDialogue).GetField("dialogueDict",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var dict = (Dictionary<string, DialogueEntry>)field.GetValue(dialogueData);

        if (dict == null)
        {
            // Build dictionary manually if needed
            var listField = typeof(NPCDialogue).GetField("dialogueList",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var list = (List<DialogueEntry>)listField.GetValue(dialogueData);
            dict = new Dictionary<string, DialogueEntry>();
            foreach (var entry in list)
                dict[entry.key] = entry;

            field.SetValue(dialogueData, dict);
        }

        dict.TryGetValue(key, out var entryFound);
        return entryFound;
    }

    private void OnQuestButton()
    {
        questButton.gameObject.SetActive(false);
        closeButton.gameObject.SetActive(false);

        PlayDialogue(currentQuest);
    }

    private void CloseDialogue()
    {
        StopAllCoroutines();
        dialogueText.text = "";
        dialoguePanel.SetActive(false);
        closeButton.onClick.RemoveAllListeners();
        questButton.onClick.RemoveAllListeners();
        isDialogueActive = false;
    }

    public void SkipTyping()
    {
        if (isTyping)
            skipTyping = true;
    }
}
