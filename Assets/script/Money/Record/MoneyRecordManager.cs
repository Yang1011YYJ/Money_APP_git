using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class MoneyRecordManager : MonoBehaviour
{
    [Header("輸入介面")]
    [Tooltip("輸入金額的欄位")] public TMP_InputField amountInput;
    [Tooltip("分類的下拉清單")] public TMP_Dropdown categoryDropdown;

    [Header("腳本")]
    public CalenderControll calenderControllScript;

    [Header("目前帳目類型")]
    [Tooltip("目前選擇的是支出還是收入")][SerializeField]RecordType currentRecoedType = RecordType.Expense;
    [Tooltip("儲存目前建立的所有帳目")][SerializeField]List<MoneyRecord> moneyRecords = new List<MoneyRecord>();

    //將目前帳目類型設定為支出
    public void SelectExpense()
    {
        currentRecoedType = RecordType.Expense;

        Debug.Log("目前選擇:支出");
    }

    //將目前帳目類型設定為收入
    public void SelectIncome()
    {
        currentRecoedType = RecordType.Income;

        Debug.Log("目前選擇:收入");
    }

    //儲存帳目
    public void AddRecord()
    {
        // 檢查金額輸入欄位是否已經在 Inspector 中連接。
        if (amountInput == null)
        {
            // 如果沒有連接，在 Console 顯示錯誤訊息。
            Debug.LogError("MoneyRecordManager 的 Amount Input 尚未連接。");

            // 中止新增帳目。
            return;
        }

        // 檢查分類選單是否已經在 Inspector 中連接。
        if (categoryDropdown == null)
        {
            // 如果沒有連接，在 Console 顯示錯誤訊息。
            Debug.LogError("MoneyRecordManager 的 Category Dropdown 尚未連接。");

            // 中止新增帳目。
            return;
        }

        // 檢查日曆控制腳本是否已經在 Inspector 中連接。
        if (calenderControllScript == null)
        {
            // 如果沒有連接，在 Console 顯示錯誤訊息。
            Debug.LogError("MoneyRecordManager 的 Calender Controll 尚未連接。");

            // 中止新增帳目。
            return;
        }

        //把金額欄位的文字換成分數
        bool amountSuccess = int.TryParse(amountInput.text, out int amount);

        //金額轉換失敗或小於等於0
        if(!amountSuccess || amount <= 0)
        {
            Debug.LogWarning("請輸入大於0的正確金額。");
            //終止新增項目
            return;
        }

        //取得分類的文字
        string selectedCategory = categoryDropdown.options[categoryDropdown.value].text;

        //取得目前日期
        DateTime selectedDate = calenderControllScript.GetSelectedDate();

        //建立新的帳目資料
        MoneyRecord newRecord = new MoneyRecord();

        //建立唯一識別碼
        newRecord.id = Guid.NewGuid().ToString();

        //將日期轉換為yyyy-MM-dd格式保存
        newRecord.date = selectedDate.ToString("yyyy-MM-dd");

        //保存輸入的金額
        newRecord.amount = amount;
        //保存分類
        newRecord.category = selectedCategory;
        //保存支出還是收入
        newRecord.recordType = currentRecoedType;

        //將新帳目加入帳目清單
        moneyRecords.Add(newRecord);

        // 在 Console 顯示剛建立的帳目，方便確認資料是否正確。
        Debug.Log(
            $"新增帳目成功：" +
            $"日期={newRecord.date}，" +
            $"類型={newRecord.recordType}，" +
            $"分類={newRecord.category}，" +
            $"金額={newRecord.amount}");

        // 清空金額輸入欄位，方便輸入下一筆。
        amountInput.text = "";
    }

    // 提供其他腳本取得全部帳目。
    public List<MoneyRecord> GetAllRecords()
    {
        // 回傳目前保存的帳目清單。
        return moneyRecords;
    }
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
