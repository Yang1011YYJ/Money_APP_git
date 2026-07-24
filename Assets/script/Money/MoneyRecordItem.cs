using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class MoneyRecordItem : MonoBehaviour
{
    [Header("UI")]
    [Tooltip("顯示收入或支出")] public TextMeshProUGUI typeText;
    [Tooltip("顯示帳目分類")] public TextMeshProUGUI categoryText;
    [Tooltip("顯示帳目金額")] public TextMeshProUGUI amountText;

    [Header("文字顏色")]
    [Tooltip("支出金額顏色")] public Color expenseColor = Color.red;
    [Tooltip("收入金額顏色")] public Color incomeColor = Color.green;

    //將一筆資料顯示到UI項目上
    public void SetUP(MoneyRecord record)
    {
        if(record == null)
        {
            Debug.LogError("MoneyRecordItem沒有收到MoneyRecord。");

            return;
        }

        //將帳目的分類顯示在分類文字中
        categoryText.text = record.category;

        //判斷資料是否為支出
        if(record.recordType == RecordType.Expense)
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
