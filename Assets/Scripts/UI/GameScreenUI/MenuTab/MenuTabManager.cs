using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;

public class MenuTabManager : BaseUIManager
{
    [System.Serializable]
    public class Tab
    {
        public Button button;
        public GameObject panel;
        public Button closeButton;
    }

    public List<Tab> tabs = new List<Tab>();

    [SerializeField] private TextMeshProUGUI txtGold;

    private int currentIndex = -1;

    void Start()
    {
        for (int i = 0; i < tabs.Count; i++)
        {
            int id = i;

            tabs[i].button.onClick.AddListener(() => OpenTab(id));

            if (tabs[i].closeButton != null)
            {
                tabs[i].closeButton.onClick.AddListener(() => CloseTab(id));
            }
        }

        OpenTab(0);
    }

    private void Update()
    {
        if (txtGold != null && ClientReceiveMessageHandler.Player != null)
        {
            txtGold.text = Helpers.MoneyToString(ClientReceiveMessageHandler.Player.Gold);
        }
    }

    public void OpenTab(int index)
    {
        if (currentIndex == index)
            return;

        if (currentIndex >= 0 && currentIndex < tabs.Count)
        {
            tabs[currentIndex].panel.SetActive(false);
        }

        tabs[index].panel.SetActive(true);
        currentIndex = index;

        SkillUIManager skillUI = tabs[index].panel.GetComponent<SkillUIManager>();
        if (skillUI != null)
        {
            skillUI.EnsureLoaded();
        }
    }

    public void CloseTab(int index)
    {
        if (index < 0 || index >= tabs.Count)
            return;

        tabs[index].panel.SetActive(false);

        if (currentIndex == index)
            currentIndex = -1;

        UIManager.Instance.gameScreenUIManager.ShowHUD();
    }

    public void CloseAllTabs()
    {
        foreach (var t in tabs)
        {
            t.panel.SetActive(false);
        }

        currentIndex = -1;
    }

    public override void ShowUI()
    {
        base.ShowUI();
        OpenTab(0);
    }
}
