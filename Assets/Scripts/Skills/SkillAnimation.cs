using UnityEngine;
using System.Collections;
public class SkillAnimation : MonoBehaviour
{
    public SpriteRenderer sr;

    private void Start()
    {
        sr = GetComponent<SpriteRenderer>();
    }

    public Coroutine Play(Sprite[] frames, float fps, bool autoDisable = true)
    {
        if (frames == null || frames.Length == 0)
        {
            sr.enabled = false;
            return null;
        }

        sr.enabled = true;
        return StartCoroutine(PlayAnim(frames, fps, autoDisable));
    }

    IEnumerator PlayAnim(Sprite[] frames, float fps, bool autoDisable)
    {
        float delay = 1f / fps;

        // Nếu autoDisable = false → LOOP
        if (!autoDisable)
        {
            while (true)
            {
                for (int i = 0; i < frames.Length; i++)
                {
                    sr.sprite = frames[i];
                    yield return new WaitForSeconds(delay);
                }
            }
        }

        // Nếu autoDisable = true → chạy 1 vòng rồi tắt
        for (int i = 0; i < frames.Length; i++)
        {
            sr.sprite = frames[i];
            yield return new WaitForSeconds(delay);
        }

        sr.enabled = false;
    }
}
