using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MoneyRecordItem : MonoBehaviour
{
    [Header("UI")]
    [Tooltip("顯示收入或支出")] public TextMeshProUGUI typeText;
    [Tooltip("顯示帳目分類")] public TextMeshProUGUI categoryText;
    // 顯示小分類。
    [Tooltip("顯示帳目小分類")] public TextMeshProUGUI subCategoryText;
    [Tooltip("顯示付款方式")] public TextMeshProUGUI paymentMethodText;
    [Tooltip("顯示帳目金額")] public TextMeshProUGUI amountText;
    [Tooltip("編輯按鈕")] public Button editButton;

    [Header("文字顏色")]
    [Tooltip("支出金額顏色")] public Color expenseColor = Color.red;
    [Tooltip("收入金額顏色")] public Color incomeColor = Color.green;

    [Tooltip("儲存目前UI項目的帳目資料")] MoneyRecord currentRecord;
    [Tooltip("儲存帳目編輯面板")]MoneyRecordEditPanel editPanel;

    //將一筆資料顯示到UI項目上
    public void SetUP(MoneyRecord record,MoneyRecordEditPanel targetEditPanel)
    {
        if(record == null)
        {
            Debug.LogError("MoneyRecordItem沒有收到MoneyRecord。");

            return;
        }

        // 保存目前這個項目對應的帳目。
        currentRecord = record;

        // 保存編輯面板的參考。
        editPanel = targetEditPanel;

        //將帳目的分類顯示在分類文字中
        categoryText.text = record.category;

        // 顯示這筆帳目的小分類。
        subCategoryText.text = record.subCategory;

        //判斷資料是否為支出
        if (record.recordType == RecordType.Expense)
        {
            //顯示帳目類型為支出
            typeText.text = "支出";

            //金額前面加上負號
            amountText.text = $"-{record.amount} 元";

            //改變文字顏色
            amountText.color = expenseColor;
        }
        else//如果是收入
        {
            typeText.text = "收入";

            amountText.text = $"+{record.amount} 元";

            amountText.color = incomeColor;
        }

        //顯示這筆帳目的付款/收款方式
        paymentMethodText.text = record.paymentMethod;

        // 檢查目前這個帳目項目的編輯按鈕是否存在。
        if (editButton == null)
        {
            // 如果是空的，代表 Prefab 的 Edit Button 欄位沒有正確連接。
            Debug.LogError(
                $"帳目「{record.category}」的 EditButton 沒有連接。");
        }
        else
        {
            // 顯示目前取得的按鈕物件名稱。
            Debug.Log(
                $"帳目「{record.category}」已取得編輯按鈕：" +
                $"{editButton.gameObject.name}");

            // 清除舊的點擊事件。
            editButton.onClick.RemoveAllListeners();

            // 加入開啟編輯面板的點擊事件。
            editButton.onClick.AddListener(OpenEditPanel);

            // 顯示事件已加入。
            Debug.Log(
                $"帳目「{record.category}」的編輯事件已加入。");
        }
    }
    
    //開啟編輯面板
    void OpenEditPanel()
    {
        // 確認編輯按鈕確實有呼叫到這個方法。
        Debug.Log("已點擊帳目編輯按鈕。");

        // 檢查目前帳目資料是否存在。
        if (currentRecord == null)
        {
            // 沒有帳目資料時顯示錯誤。
            Debug.LogError("MoneyRecordItem 的 currentRecord 是空的。");

            // 中止開啟流程。
            return;
        }

        if (editPanel == null)
        {
            Debug.LogError("MoneyRecordItem 沒有取得 MoneyRecordEditPanel。");

            return;
        }

        // 顯示即將編輯的帳目資料。
        Debug.Log(
            $"準備編輯帳目：" +
            $"日期={currentRecord.date}，" +
            $"分類={currentRecord.category}，" +
            $"金額={currentRecord.amount}");

        //將目前的帳目傳入編輯面板
        editPanel.OpenEditPanel(currentRecord);
    }
}
