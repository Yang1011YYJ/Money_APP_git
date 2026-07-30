using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class MoneyRecordEditPanel : MonoBehaviour
{
    [Header("編輯介面")]
    [Tooltip("整個編輯面板")] public GameObject AllEditPanel;
    [Tooltip("編輯金額")] public TMP_InputField editAmountInput;
    [Tooltip("分類")] public TMP_Dropdown editCategoryDropdown;

    [Header("其他UI")]
    public GameObject QuickRecordPanel;

    [Header("紀錄資料")]
    public RecordType editingRecordType = RecordType.Expense;// 儲存目前在編輯面板選擇的收入或支出類型。
    // 記錄目前編輯的資料是不是一筆尚未正式儲存的新帳目。
    private bool isNewRecord = false;

    // 如果新帳目成功儲存後，需要通知來源物件。
    private System.Action onNewRecordSaved;

    [Header("腳本")]
    public MoneyRecordManager MoneyRecordManagerScript;
    public MoneyRecord editingRecord;// 儲存目前正在編輯的帳目。

    

    //開啟編輯面板並帶入指定資料
    public void OpenEditPanel(MoneyRecord record)
    {
        // 這是既有帳目的修改，
        // 不是建立新資料。
        isNewRecord = false;

        // 既有帳目不需要新帳目儲存完成事件。
        onNewRecordSaved = null;


        if (record == null)
        {
            Debug.LogError("開啟編輯面板沒有收到資料。");

            return;
        }

        //儲存目前正在編輯的項目
        editingRecord = record;

        //將資料金額帶入欄位
        editAmountInput.text = record.amount.ToString();

        //儲存原本的收入或支出類型
        editingRecordType = record.recordType;

        //根據原本分類找到對應選項
        int categoryIndex = FindDropdownOptionIndex(editCategoryDropdown/*要找的父物件*/, record.category/*參考的對象*/);

        //將編輯介面分類切換為資料的分類
        editCategoryDropdown.value = categoryIndex;

        //更新下拉式選單畫面
        editCategoryDropdown.RefreshShownValue();

        //開啟編輯面板
        AllEditPanel.SetActive(true);
    }

    // 開啟編輯面板，編輯一筆尚未正式存入帳本的新資料。
    public void OpenNewRecordPanel(
        MoneyRecord record,
        System.Action onSaved)
    {
        // 檢查傳進來的新資料是否存在。
        if (record == null)
        {
            // 顯示錯誤。
            Debug.LogError(
                "開啟新帳目編輯面板時沒有收到資料。");

            // 中止。
            return;
        }

        // 保存目前正在編輯的新資料。
        editingRecord =
            record;

        // 標記這次是在建立新帳目。
        isNewRecord =
            true;

        // 保存新帳目正式儲存完成後要執行的事件。
        onNewRecordSaved =
            onSaved;


        // 將金額帶入輸入欄位。
        editAmountInput.text =
            record.amount > 0
                ? record.amount.ToString()
                : "";

        // 帶入原本分析出的收入或支出。
        editingRecordType =
            record.recordType;


        // 根據分析出的分類尋找 Dropdown 選項。
        int categoryIndex =
            FindDropdownOptionIndex(
                editCategoryDropdown,
                record.category);

        // 將分類切換到對應選項。
        editCategoryDropdown.value =
            categoryIndex;

        // 更新 Dropdown 顯示。
        editCategoryDropdown.RefreshShownValue();


        // 開啟編輯面板。
        AllEditPanel.SetActive(
            true);
    }

    //將編輯類型切換為支出
    public void SelectExpense()
    {
        editingRecordType = RecordType.Expense;
    }

    //將編輯類型切換為收入
    public void SelectIncome()
    {
        editingRecordType = RecordType.Income;
    }

    //確認並儲存修改
    // 確認並儲存修改。
    public void ConfirmEdit()
    {
        // 檢查目前是否真的有資料正在編輯。
        if (editingRecord == null)
        {
            // 顯示錯誤。
            Debug.LogError(
                "目前沒有可編輯項目。");

            // 中止。
            return;
        }


        // 將金額欄位轉換為整數。
        bool amountSuccess =
            int.TryParse(
                editAmountInput.text,
                out int editedAmount);

        // 檢查金額是否合法。
        if (!amountSuccess || editedAmount <= 0)
        {
            // 顯示提醒。
            Debug.LogWarning(
                "請輸入大於 0 的金額。");

            // 不儲存。
            return;
        }


        // 取得目前選擇的大分類。
        string editedCategory =
            editCategoryDropdown
                .options[editCategoryDropdown.value]
                .text;


        // 更新這筆資料的金額。
        editingRecord.amount =
            editedAmount;

        // 更新這筆資料的大分類。
        editingRecord.category =
            editedCategory;

        // 更新收入或支出類型。
        editingRecord.recordType =
            editingRecordType;


        // 檢查帳目管理器是否存在。
        if (MoneyRecordManagerScript == null)
        {
            // 顯示錯誤。
            Debug.LogError(
                "MoneyRecordEditPanel 沒有連接 MoneyRecordManager。");

            // 不繼續儲存。
            return;
        }


        // =============================
        // 新帳目
        // =============================

        // 判斷目前是不是從無法辨識區進來的新帳目。
        if (isNewRecord)
        {
            // 使用原本已經寫好的 AddRecord，
            // 將這筆資料正式加入帳目清單。
            MoneyRecordManagerScript.AddRecord(
                editingRecord);

            // 如果來源有提供「成功後事件」。
            if (onNewRecordSaved != null)
            {
                // 通知來源：
                // 這筆資料現在真的已經成功存進帳本。
                onNewRecordSaved.Invoke();
            }
        }

        // =============================
        // 原本已存在的帳目
        // =============================

        else
        {
            // 因為 editingRecord 本身就是原 List 裡的物件，
            // 欄位前面已經直接修改完成，
            // 所以只需要重新儲存檔案。
            MoneyRecordManagerScript.SaveToFile();

            // 重新整理目前日期的帳目顯示。
            MoneyRecordManagerScript.RefreshDailyRecords();
        }


        // 清除目前正在編輯的資料。
        editingRecord =
            null;

        // 恢復成普通編輯狀態。
        isNewRecord =
            false;

        // 清除成功事件。
        onNewRecordSaved =
            null;

        // 儲存完成後關閉編輯面板。
        AllEditPanel.SetActive(
            false);
    }

    //關閉編輯面板
    public void ExitEditPanel()
    {
        AllEditPanel.SetActive(false);
    }

    //取消編輯
    public void CancelEdit()
    {
        // 清除目前正在編輯的資料。
        editingRecord =
            null;

        // 清除是否為新帳目的狀態。
        isNewRecord =
            false;

        // 清除新帳目儲存完成事件。
        onNewRecordSaved =
            null;

        // 關閉編輯面板。
        AllEditPanel.SetActive(
            false);
    }

    // 根據文字尋找 TMP_Dropdown 的選項索引。
    private int FindDropdownOptionIndex(
        TMP_Dropdown dropdown,
        string targetText)
    {
        // 檢查下拉選單是否存在。
        if (dropdown == null)
        {
            // 找不到下拉選單時回傳第一個選項。
            return 0;
        }

        // 逐一檢查下拉選單中的所有選項。
        for (int i = 0; i < dropdown.options.Count; i++)
        {
            // 判斷目前選項文字是否和目標文字相同。
            if (dropdown.options[i].text == targetText)
            {
                // 找到後回傳這個選項的索引。
                return i;
            }
        }

        // 找不到相同分類時，預設使用第一個選項。
        return 0;
    }
}
