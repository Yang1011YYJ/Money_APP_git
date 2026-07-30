using System.Collections;
using System.Collections.Generic;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DeleteConfirmPanel : MonoBehaviour
{
    [Header("確認框 UI")]
    [Tooltip("整個面板")] public GameObject AllConfirmPanel;
    // 顯示確認訊息。
    [Tooltip("確認訊息")] public TextMeshProUGUI messageText;
    // 確認刪除按鈕。
    [Tooltip("刪除按鈕")] public Button confirmButton;
    // 取消刪除按鈕。
    [Tooltip("取消刪除")] public Button cancelButton;

    [Header("事件")]
    // 保存使用者按下「確認」後真正要執行的刪除功能。
    private Action confirmAction;
    // Start is called before the first frame update
    void Start()
    {
        // 確認按鈕存在時才設定事件。
        if (confirmButton != null)
        {
            // 清除原本可能存在的事件。
            confirmButton.onClick.RemoveAllListeners();

            // 加入確認事件。
            confirmButton.onClick.AddListener(
                ConfirmDelete);
        }

        // 取消按鈕存在時才設定事件。
        if (cancelButton != null)
        {
            // 清除原本可能存在的事件。
            cancelButton.onClick.RemoveAllListeners();

            // 加入取消事件。
            cancelButton.onClick.AddListener(
                CancelDelete);
        }

    }

    // 開啟確認框。
    public void Show(Action action, string message = "確定刪除這筆資料嗎？")
    {
        // 保存真正要執行的刪除功能。
        confirmAction = action;

        // 如果有提示文字元件。
        if (messageText != null)
        {
            // 顯示這次指定的訊息。
            messageText.text = message;
        }

        // 開啟確認框。
        AllConfirmPanel.SetActive(true);
    }

    // 使用者按下確認。
    private void ConfirmDelete()
    {
        // 先保存目前的刪除事件。
        Action action = confirmAction;

        // 清除暫存，
        // 避免下一次誤用上一個刪除事件。
        confirmAction = null;

        // 關閉確認框。
        AllConfirmPanel.SetActive(false);

        // 如果真的有刪除事件。
        if (action != null)
        {
            // 執行真正的刪除。
            action.Invoke();
        }
    }

    // 使用者按下取消。
    private void CancelDelete()
    {
        // 清除這次的刪除事件。
        confirmAction =null;

        // 關閉確認框。
        AllConfirmPanel.SetActive(false);
    }
    // Update is called once per frame
    void Update()
    {
        
    }
}
