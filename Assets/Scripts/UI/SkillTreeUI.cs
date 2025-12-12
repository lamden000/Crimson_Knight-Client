using UnityEngine;
using UnityEngine.UI;
public class SkillTreeUI : MonoBehaviour
{
    public SkillNode[] skillNodes;   
    public SkillInfoPanel infoPanel;
    public Button closeButton;
    SkillData[] skillList;
    public GameObject menuRoot;
    void Start()
    {
        LoadSkillList();     // tạo dữ liệu
        LoadSkillNodes();
        // đưa dữ liệu vào node
        closeButton.onClick.AddListener(CloseAllMenu);
    }
    void CloseAllMenu()
    {
        menuRoot.SetActive(false);      // ← TẮT TOÀN BỘ MENU
    }
    void LoadSkillList()
    {
        skillList = new SkillData[]
        {
            new SkillData(0,"Gốc","Skill gốc",10,2,30,0),
            new SkillData(1,"Nhanh 1","Mô tả",10,1,10,1),
            new SkillData(2,"Nhanh 2","Mô tả",10,1,10,2),
            new SkillData(3,"Nhánh 1.1","Mô tả",10,1,10,3),
            new SkillData(4,"Nhánh 1.2","Mô tả",10,1,10,4),
            new SkillData(5,"Nhánh 2.1","Mô tả",10,1,10,5),
            new SkillData(6,"Nhánh 2.2","Mô tả",10,1,10,6),
        };
    }

    void LoadSkillNodes()
    {
        for (int i = 0; i < skillNodes.Length; i++)
        {
            skillNodes[i].Load(skillList[i]);

            int id = i;  

            skillNodes[i].GetComponent<UnityEngine.UI.Button>().onClick.AddListener(() =>
            {
                Debug.Log("Bạn đã click vào skill ID = " + skillList[id].id);
                infoPanel.ShowSkill(skillList[id]);
            });
        }
    }

}
