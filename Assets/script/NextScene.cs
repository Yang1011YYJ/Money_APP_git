using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class NextScene : MonoBehaviour
{
    [Header("UI")]
    public GameObject BlackPanel;

    [Header("淡出數據")]
    public float fadeduration = 1.5f;

    public void Start()
    {
        BlackPanel.SetActive(false);
    }
    public void Next()
    {
        StartCoroutine(ChangeScene(Wait(1f),BlackPanel, 0, 1, () => SceneManager.LoadScene(1)));
    }

    public IEnumerator Wait(float time)
    {
        yield return new WaitForSeconds(time);
    }

    public IEnumerator ChangeScene(IEnumerator a,GameObject Panel,int Start,int End, Action b)
    {

        // 如果有傳入前置協程，就等待它執行完成。
        if (a != null)
        {
            // 等待前置協程完成。
            yield return StartCoroutine(a);
        }

        CanvasGroup canvasGroup = Panel.GetComponent<CanvasGroup>();

        // 防止淡出期間使用者繼續點擊畫面。
        canvasGroup.blocksRaycasts = true;

        canvasGroup.alpha = Start;

        Panel.SetActive(true);

        // 紀錄目前已經經過的時間。
        float elapsedTime = 0f;

        while(elapsedTime < fadeduration)
        {
            elapsedTime += Time.deltaTime;

            float progress = elapsedTime / fadeduration;

            canvasGroup.alpha = Mathf.Lerp(Start, End, progress);

            yield return null;
        }

        canvasGroup.alpha = End;

        Panel.SetActive(false);

        b();
    }
}
