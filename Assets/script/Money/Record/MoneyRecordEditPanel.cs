using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class MoneyRecordEditPanel : MonoBehaviour
{
    [Header("編輯介面")]
    [Tooltip("編輯金額")] public TMP_InputField editAmountInput;
    [Tooltip("分類")] public TMP_Dropdown editCategoryDropdown;

    [Header("腳本")]
    public MoneyRecordManager MoneyRecordManagerScript;
    public MoneyRecord editingRecord;// 儲存目前正在編輯的帳目。
    public RecordType editingRecordType = RecordType.Expense;// 儲存目前在編輯面板選擇的收入或支出類型。

    //開啟編輯面板並帶入指定資料
    public void OpenEditPanel(MoneyRecord record)
    {
        if(record == null)
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
        gameObject.SetActive(true);
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
    public void ConfirmEdit()
    {
        if(editingRecord == null)
        {
            Debug.LogError("目前沒有可編輯項目。");

            return;
        }

        //將輸入文字轉換為整體金額
        bool amountSuccess = int.TryParse(editAmountInput.text, out int editedAmount);

        //檢查金額是否有效
        if(!amountSuccess || editedAmount <= 0)
        {
            Debug.LogWarning("請輸入大於0的金額。");

            return;
        }

        //取得分類的文字
        string editedcategory = editCategoryDropdown.options[editCategoryDropdown.value].text;

        //修改帳目金額
        editingRecord.amount = editedAmount;

        //修改帳目分類
        editingRecord.category = editedcategory;

        //修改支出或收入
        editingRecord.recordType = editingRecordType;

        // 檢查帳目管理器是否存在。
        if (MoneyRecordManagerScript != null)
        {
            // 重新顯示目前日期的帳目清單。
            MoneyRecordManagerScript.RefreshDailyRecords();
        }

        //清除目前編輯的參考
        editingRecord = null;
    }

    //關閉編輯面板
    public void ExitEditPanel()
    {
        gameObject.SetActive(false);
    }

    //取消編輯
    public void CancelEdit()
    {
        //清除目前編輯內容
        editingRecord = null;

        gameObject.SetActive(false);
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
