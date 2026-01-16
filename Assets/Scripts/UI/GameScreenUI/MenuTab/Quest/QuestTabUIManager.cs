using UnityEngine;

public class QuestTabUIManager : MonoBehaviour
{
    [Header("Quest List")]
    public Transform content;
    public QuestListItem questItemPrefab;

    [Header("Quest Detail")]
    public QuestDetailPanel detailPanel;

    private void OnEnable()
    {
        LoadQuestList();
        detailPanel.Clear();
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
        detailPanel.Show(quest);
    }

    private void ClearContent()
    {
        for (int i = content.childCount - 1; i >= 0; i--)
            Destroy(content.GetChild(i).gameObject);
    }
}
