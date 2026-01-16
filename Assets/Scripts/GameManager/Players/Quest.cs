public class Quest
{
    public int Id { get; set; }
    public int QuantityCur { get; set; }
    public QuestState QuestState { get; set; }
    public QuestTemplate GetTemplate()
    {
        return TemplateManager.QuestTemplates[Id];
    }
}
public enum QuestState
{
    NotAccepted = 0,
    InProgress = 1,
    Completed = 2
}