using UnityEngine;
using System.Collections;

public class SlideUpSubMenu : MonoBehaviour
{
    public RectTransform panel;
    public float moveDistance = 30f;
    public float moveTime = 0.2f;
    public float delayBetweenItems = 0.05f;

    private CanvasGroup[] items;
    private Vector2[] startPos;
    private bool isOpen;

    void Awake()
    {
        items = panel.GetComponentsInChildren<CanvasGroup>(false);
        startPos = new Vector2[items.Length];

        for (int i = 0; i < items.Length; i++)
        {
            RectTransform rt = items[i].GetComponent<RectTransform>();
            startPos[i] = rt.anchoredPosition;

            items[i].alpha = 0;
            items[i].interactable = false;
            items[i].blocksRaycasts = false;
        }

        panel.gameObject.SetActive(false);
    }

    public void Toggle()
    {
        if (isOpen)
            StartCoroutine(Hide());
        else
            StartCoroutine(Show());
    }

    IEnumerator Show()
    {
        panel.gameObject.SetActive(true);
        isOpen = true;

        for (int i = 0; i < items.Length; i++)
        {
            RectTransform rt = items[i].GetComponent<RectTransform>();
            rt.anchoredPosition = startPos[i] - Vector2.up * moveDistance;

            items[i].alpha = 0;
            items[i].interactable = false;
            items[i].blocksRaycasts = false;

            float t = 0;
            while (t < moveTime)
            {
                t += Time.deltaTime;
                float p = t / moveTime;

                rt.anchoredPosition = Vector2.Lerp(
                    startPos[i] - Vector2.up * moveDistance,
                    startPos[i],
                    p
                );
                items[i].alpha = p;

                yield return null;
            }

            rt.anchoredPosition = startPos[i];
            items[i].alpha = 1;
            items[i].interactable = true;
            items[i].blocksRaycasts = true;

            yield return new WaitForSeconds(delayBetweenItems);
        }
    }

    IEnumerator Hide()
    {
        isOpen = false;

        for (int i = items.Length - 1; i >= 0; i--)
        {
            RectTransform rt = items[i].GetComponent<RectTransform>();

            items[i].alpha = 0;
            items[i].interactable = false;
            items[i].blocksRaycasts = false;

            rt.anchoredPosition = startPos[i] - Vector2.up * moveDistance;
        }

        yield return new WaitForSeconds(0.05f);
        panel.gameObject.SetActive(false);
    }
}
