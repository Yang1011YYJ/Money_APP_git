using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 將這個類別標記為可序列化，之後才能轉成 JSON 儲存在裝置中。
[Serializable]
public class MoneyRecord
{
    // 儲存這筆帳目的唯一編號，避免不同紀錄混在一起。
    public string id;

    // 儲存這筆帳目的日期，格式例如「2026-07-16」。
    public string date;

    // 儲存這筆帳目的金額。
    public int amount;

    // 儲存這筆帳目的分類，例如餐飲、交通或娛樂。
    public string category;

    // 儲存這筆帳目是收入還是支出。
    public RecordType recordType;
}

// 定義帳目類型，讓程式只能選擇收入或支出。
public enum RecordType
{
    // 代表支出。
    Expense,

    // 代表收入。
    Income
}
