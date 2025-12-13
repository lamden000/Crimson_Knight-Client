using UnityEngine;
using UnityEngine.UI;

public class SkillTreeUI : MonoBehaviour
{
    public SkillNode[] skillNodes;
    public SkillInfoPanel infoPanel;
    public Button closeButton;
    public GameObject menuRoot;

    SkillData[] skillList;

    void Start()
    {
        Debug.Log("SkillTreeUI START");

        LoadSkillList();
        LoadSkillNodes();

        closeButton.onClick.RemoveAllListeners();
        closeButton.onClick.AddListener(CloseAllMenu);
    }

    void CloseAllMenu()
    {
        menuRoot.SetActive(false);
    }

    void LoadSkillList()
    {
        skillList = new SkillData[]
        {
            new SkillData(0,"Gốc","Skill gốc",10,2,30,0),
            new SkillData(1,"Nhánh 1","Mô tả",10,1,10,1),
            new SkillData(2,"Nhánh 2","Mô tả",10,1,10,2),
            new SkillData(3,"Nhánh 1.1","Mô tả",10,1,10,3),
            new SkillData(4,"Nhánh 1.2","Mô tả",10,1,10,4),
            new SkillData(5,"Nhánh 2.1","Mô tả",10,1,10,5),
            new SkillData(6,"Nhánh 2.2","Mô tả",10,1,10,6),
        };
    }

    void LoadSkillNodes()
    {
        int count = Mathf.Min(skillNodes.Length, skillList.Length);

        Debug.Log($"LoadSkillNodes count = {count}");

        for (int i = 0; i < count; i++)
        {
            skillNodes[i].Load(skillList[i], infoPanel);
        }
    }
}
