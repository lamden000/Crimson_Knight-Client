using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class TabManager : MonoBehaviour
{
    [System.Serializable]
    public class Tab
    {
        public Button button;     
        public GameObject panel;  
    }

    public List<Tab> tabs = new List<Tab>();
    public Button closeButton;

    private int currentIndex = -1;

    void Start()
    {
        for (int i = 0; i < tabs.Count; i++)
        {
            int id = i;
            tabs[i].button.onClick.AddListener(() => OpenTab(id));
        }

        if (closeButton != null)
            closeButton.onClick.AddListener(CloseMenu);

        CloseAllTabs();
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

    public void CloseMenu()
    {
        this.gameObject.SetActive(false);
    }
}
