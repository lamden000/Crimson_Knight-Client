using UnityEngine;
using System.Collections;

public class QuestTabUIManager : MonoBehaviour
{
    [Header("Quest List")]
    public Transform content;
    public QuestListItem questItemPrefab;

    [Header("Quest Detail")]
    public GameObject detailPanelRoot;
    public QuestDetailPanel detailPanel;

    private Coroutine loadRoutine;

    private void OnEnable()
    {
        if (detailPanelRoot != null)
            detailPanelRoot.SetActive(false);

        if (loadRoutine != null)
            StopCoroutine(loadRoutine);

        loadRoutine = StartCoroutine(WaitAndLoad());
    }

    private void OnDisable()
    {
        if (loadRoutine != null)
        {
            StopCoroutine(loadRoutine);
            loadRoutine = null;
        }

        if (detailPanelRoot != null)
            detailPanelRoot.SetActive(false);

        detailPanel.Clear();
        ClearContent();
    }

    private IEnumerator WaitAndLoad()
    {
        yield return new WaitUntil(() =>
            ClientReceiveMessageHandler.Player != null
        );

        LoadQuestList();
    }

    private void LoadQuestList()
    {
        ClearContent();

        Player player = ClientReceiveMessageHandler.Player;
        if (player == null || player.Quest == null)
            return;

        QuestListItem item = Instantiate(questItemPrefab, content);
        item.Bind(player.Quest, this);
    }

    public void ShowQuestDetail(Quest quest)
    {
        if (quest == null)
            return;

        if (detailPanelRoot != null)
            detailPanelRoot.SetActive(true);

        detailPanel.Show(quest);
    }

    public void HideQuestDetail()
    {
        if (detailPanelRoot != null)
            detailPanelRoot.SetActive(false);

        detailPanel.Clear();
    }

    private void ClearContent()
    {
        for (int i = content.childCount - 1; i >= 0; i--)
            Destroy(content.GetChild(i).gameObject);
    }
}
