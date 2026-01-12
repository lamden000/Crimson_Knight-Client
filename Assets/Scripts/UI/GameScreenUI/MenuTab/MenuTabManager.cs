using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class MenuTabManager : BaseUIManager
{
    [System.Serializable]
    public class Tab
    {
        public Button button;     
        public GameObject panel;  
    }

    public List<Tab> tabs = new List<Tab>();
    [SerializeField]
    private Button btnClose;
    private int currentIndex = -1;

    void Start()
    {
        for (int i = 0; i < tabs.Count; i++)
        {
            int id = i;
            tabs[i].button.onClick.AddListener(() => OpenTab(id));
        }
        btnClose?.onClick.AddListener(() => {
            UIManager.Instance.gameScreenUIManager.ShowHUD();
        });
        CloseAllTabs();
        OpenTab(0);
    }

    public void OpenTab(int index)
    {
        CloseAllTabs();

        tabs[index].panel.SetActive(true);
        currentIndex = index;
    }

    public void CloseAllTabs()
    {
        foreach (var t in tabs)
            t.panel.SetActive(false);
        currentIndex = -1;
    }
   
    public override void ShowUI()
    {
        base.ShowUI();
        OpenTab(0);
    }
}
