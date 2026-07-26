// 引入 System 命名空間，讓這個資料類別可以序列化。
using System;

// 將解析結果標記為可序列化。
[Serializable]
public class RecordParseResult
{
    // 記錄這次解析是否成功。
    public bool isSuccess;

    // 儲存解析失敗或缺少資料時的提示。
    public string message;

    // 儲存解析出的日期。
    public DateTime date;

    // 儲存解析出的品項。
    public string itemName;

    // 儲存解析出的金額。
    public int amount;

    // 儲存解析出的付款方式。
    public string paymentMethod;

    // 儲存系統判斷的分類。
    public string category;

    // 儲存系統判斷的收入或支出。
    public RecordType recordType;

    // 儲存從自然語句中解析出的備註。
    public string note;
}