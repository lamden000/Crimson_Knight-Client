using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class QuestDetailPanel : MonoBehaviour
{
    public TextMeshProUGUI txtName;
    public TextMeshProUGUI txtDesc;
    public TextMeshProUGUI txtProgress;
    public TextMeshProUGUI txtReward;

    public Button btnAction;
    public TextMeshProUGUI txtAction;

    private Quest quest;

    public void Show(Quest quest)
    {
        this.quest = quest;
        QuestTemplate tpl = quest.GetTemplate();

        txtName.text = tpl.Name;
        txtDesc.text = tpl.Description;

        string monsterName =
            TemplateManager.MonsterTemplates[tpl.MonsterTemplateId].Name;

        // Tiến độ
        txtProgress.text =
            $"Tiêu diệt {monsterName}: {quest.QuantityCur}/{tpl.Quantity}";

        // Phần thưởng
        txtReward.text =
            $"Phần thưởng:\n" +
            $"• Vàng: {tpl.GoldReceive}\n" +
            $"• Kinh nghiệm: {tpl.EXPReceive}";

        UpdateActionButton();
    }

    private void UpdateActionButton()
    {
        btnAction.onClick.RemoveAllListeners();

        switch (quest.QuestState)
        {
            case QuestState.NotAccepted:
                txtAction.text = "Nhận nhiệm vụ";
                btnAction.interactable = true;
                btnAction.onClick.AddListener(AcceptQuest);
                break;

            case QuestState.InProgress:
                txtAction.text = "Đang thực hiện";
                btnAction.interactable = false;
                break;

            case QuestState.Completed:
                txtAction.text = "Nhận thưởng";
                btnAction.interactable = true;
                btnAction.onClick.AddListener(ReceiveReward);
                break;
        }
    }

    private void AcceptQuest()
    {
        // RequestManager.AcceptQuest(quest.Id);
        quest.QuestState = QuestState.InProgress;
        UpdateActionButton();
    }

    private void ReceiveReward()
    {
        // RequestManager.ReceiveQuestReward(quest.Id);
        quest.QuestState = QuestState.NotAccepted;
        quest.QuantityCur = 0;
        Clear();
    }

    public void Clear()
    {
        txtName.text = "";
        txtDesc.text = "";
        txtProgress.text = "";
        txtReward.text = "";
        txtAction.text = "";
        btnAction.interactable = false;
    }
}
