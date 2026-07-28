// 引入 TextMeshPro 命名空間。
using TMPro;

// 引入 Unity 基本功能。
using UnityEngine;

// 建立快速記帳輸入介面控制器。
public class QuickRecordInputController : MonoBehaviour
{
    // ==============================
    // 快速記帳 UI
    // ==============================


    // 在 Inspector 中建立介面分類。
    [Header("快速記帳介面")]
    [Tooltip("整個面板")] public GameObject QuickRecordPanel;

    // 接收使用者輸入自然語句的欄位。
    [Tooltip("自然語句輸入欄位")]
    public TMP_InputField sentenceInput;

    // 顯示解析結果的文字。
    [Tooltip("顯示規則解析結果")]
    public TextMeshProUGUI parseResultText;

    // ==============================
    // 解析資料
    // ==============================

    // 儲存規則式文字解析器。
    private RuleBasedRecordParser parser;

    // 儲存最近一次解析成功的結果。
    private RecordParseResult latestResult;

    // ==============================
    // 其他腳本
    // ==============================

    [Header("腳本")]
    [Tooltip("帳目資料管理器")] public MoneyRecordManager moneyRecordManager;

    // 物件初始化時執行。
    private void Awake()
    {
        // 建立規則式解析器。
        parser = new RuleBasedRecordParser();
    }
    // ==============================
    // 分析文字
    // ==============================
    // 提供解析按鈕呼叫。
    public void ParseInput()
    {
        // 檢查輸入欄位是否存在。
        if (sentenceInput == null)
        {
            // 顯示欄位未連接錯誤。
            Debug.LogError(
                "QuickRecordInputController 的 Sentence Input 尚未連接。");

            // 中止解析。
            return;
        }

        // 將輸入文字傳給規則解析器。
        latestResult =
            parser.Parse(sentenceInput.text);

        // 判斷解析是否失敗。
        if (!latestResult.isSuccess)
        {
            // 顯示解析失敗原因。
            parseResultText.text =
                latestResult.message;

            // 中止結果顯示。
            return;
        }

        // 將解析結果顯示在畫面上。
        parseResultText.text =
            $"日期：{latestResult.date:yyyy-MM-dd}\n" +
            $"品項：{latestResult.itemName}\n" +
            $"金額：{latestResult.amount} 元\n" +
            $"付款方式：{latestResult.paymentMethod}\n" +
            $"分類：{latestResult.category}\n" +
            $"備註：{latestResult.note}\n" +
            $"類型：{GetRecordTypeText(latestResult.recordType)}";

        // 在 Console 顯示成功結果。
        Debug.Log(
            $"規則解析成功：" +
            $"日期={latestResult.date:yyyy-MM-dd}，" +
            $"品項={latestResult.itemName}，" +
            $"金額={latestResult.amount}，" +
            $"付款方式={latestResult.paymentMethod}，" +
            $"分類={latestResult.category}");
    }

    // ==============================
    // 儲存分析結果
    // ==============================

    // 將最近一次成功的分析結果儲存成正式帳目。
    public void SaveParsedRecord()
    {
        // 檢查目前是否有分析結果。
        if (latestResult == null)
        {
            // 沒有分析結果時顯示提醒。
            Debug.LogWarning("目前沒有分析結果，請先進行分析。");

            // 中止儲存。
            return;
        }

        // 檢查最近一次分析是否成功。
        if (!latestResult.isSuccess)
        {
            // 分析失敗的資料不能儲存。
            Debug.LogWarning("目前的分析結果無效，無法儲存。");

            // 中止儲存。
            return;
        }

        // 檢查 MoneyRecordManager 是否有正確連接。
        if (moneyRecordManager == null)
        {
            // 顯示錯誤訊息。
            Debug.LogError(
                "QuickRecordInputController 沒有連接 MoneyRecordManager。");

            // 中止儲存。
            return;
        }

        // 將分析結果轉成 MoneyRecord。
        MoneyRecord newRecord =
            new MoneyRecord();

        // 儲存分析出的日期。
        newRecord.date =
            latestResult.date.ToString("yyyy-MM-dd");

        // 儲存分析出的金額。
        newRecord.amount =
            latestResult.amount;

        // 儲存分析出的大分類。
        newRecord.category =
            latestResult.category;

        // 目前解析器的 itemName 實際上就是早餐、午餐、捷運等內容，
        // 先同步當成小分類使用。
        newRecord.subCategory =
            latestResult.itemName;

        // 儲存分析出的品項。
        newRecord.itemName =
            latestResult.itemName;

        // 儲存分析出的付款方式。
        newRecord.paymentMethod =
            latestResult.paymentMethod;

        // 儲存分析出的備註。
        newRecord.note =
            latestResult.note;

        // 儲存分析出的收入或支出類型。
        newRecord.recordType =
            latestResult.recordType;

        // 直接呼叫原本已經寫好的新增帳目函式。
        // ID、加入清單、JSON 儲存、畫面刷新都由 MoneyRecordManager 處理。
        moneyRecordManager.AddRecord(newRecord);

        // 儲存完成後顯示訊息。
        Debug.Log("快速記帳分析結果已儲存。");
    }
    // 將 RecordType 轉成中文顯示文字。
    private string GetRecordTypeText(RecordType recordType)
    {
        // 判斷類型是否為收入。
        if (recordType == RecordType.Income)
        {
            // 回傳收入文字。
            return "收入";
        }

        // 其他情況回傳支出。
        return "支出";
    }

    //開關
    public void OpenOrClosePanel()
    {
        QuickRecordPanel.SetActive(!QuickRecordPanel.activeSelf);
    }
}