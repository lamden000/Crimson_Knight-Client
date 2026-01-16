using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class QuestListItem : MonoBehaviour
{
    public TextMeshProUGUI txtName;
    public TextMeshProUGUI txtState;
    public Button btnSelect;

    private Quest quest;
    private QuestTabUIManager manager;

    public void Bind(Quest quest, QuestTabUIManager manager)
    {
        this.quest = quest;
        this.manager = manager;

        QuestTemplate tpl = quest.GetTemplate();

        txtName.text = tpl.Name;
        txtState.text = GetStateText(quest.QuestState);

        btnSelect.onClick.RemoveAllListeners();
        btnSelect.onClick.AddListener(() =>
        {
            manager.ShowQuestDetail(quest);
        });
    }

    private string GetStateText(QuestState state)
    {
        switch (state)
        {
            case QuestState.NotAccepted: return "Chưa nhận";
            case QuestState.InProgress: return "Đang làm";
            case QuestState.Completed: return "Hoàn thành";
            default: return "";
        }
    }
}
