// 引入 TextMeshPro，讓程式可以控制 TMP 文字。
using TMPro;

// 引入 Unity 基本功能。
using UnityEngine;

// 引入 Unity UI，讓程式可以使用 Button。
using UnityEngine.UI;

// 負責顯示一筆「尚未正式儲存」的分析結果。
public class ParsedRecordItem : MonoBehaviour
{
    // 在 Inspector 建立 UI 分類。
    [Header("顯示欄位")]

    // 顯示日期。
    public TextMeshProUGUI dateText;

    // 顯示收入或支出。
    public TextMeshProUGUI typeText;

    // 顯示大分類。
    public TextMeshProUGUI categoryText;

    // 顯示小分類。
    public TextMeshProUGUI subCategoryText;

    // 顯示金額。
    public TextMeshProUGUI amountText;

    // 顯示付款方式。
    public TextMeshProUGUI paymentMethodText;

    // 顯示備註。
    public TextMeshProUGUI noteText;

    // 顯示原始輸入文字。
    public TextMeshProUGUI originalText;

    // 儲存按鈕。
    public Button saveButton;


    // 儲存這張卡片代表的分析結果。
    private RecordParseResult parseResult;

    // 儲存原始輸入文字。
    private string originalSentence;

    // 儲存帳目管理器。
    private MoneyRecordManager moneyRecordManager;


    // 初始化這張分析結果卡片。
    public void Setup(
        RecordParseResult result,
        string sentence,
        MoneyRecordManager manager)
    {
        // 保存分析結果。
        parseResult = result;

        // 保存原始文字。
        originalSentence = sentence;

        // 保存帳目管理器。
        moneyRecordManager = manager;


        // 顯示日期。
        dateText.text =
            parseResult.date.ToString("yyyy-MM-dd");

        // 判斷收入或支出。
        if (parseResult.recordType == RecordType.Expense)
        {
            // 顯示支出。
            typeText.text = "支出";
        }
        else
        {
            // 顯示收入。
            typeText.text = "收入";
        }

        // 顯示大分類。
        categoryText.text =
            parseResult.category;

        // 目前解析器的 itemName 暫時作為小分類使用。
        subCategoryText.text =
            parseResult.itemName;

        // 判斷目前分析出的帳目是支出還是收入。
        if (parseResult.recordType == RecordType.Expense)
        {
            // 顯示帳目類型為支出。
            typeText.text =
                "支出";

            // 支出金額前面顯示負號。
            amountText.text =
                $"-{parseResult.amount} 元";
        }
        else
        {
            // 顯示帳目類型為收入。
            typeText.text =
                "收入";

            // 收入金額前面顯示正號。
            amountText.text =
                $"+{parseResult.amount} 元";
        }

        // 顯示付款方式。
        paymentMethodText.text =
            parseResult.paymentMethod;

        // 顯示備註。
        noteText.text =
            parseResult.note;

        // 顯示原始輸入內容。
        originalText.text =
            $"原文：{originalSentence}";


        // 清除按鈕原本可能存在的事件。
        saveButton.onClick.RemoveAllListeners();

        // 將儲存功能加入按鈕。
        saveButton.onClick.AddListener(
            SaveRecord);
    }


    // 將這張卡片的分析結果正式儲存。
    private void SaveRecord()
    {
        // 檢查分析結果是否存在。
        if (parseResult == null)
        {
            // 顯示錯誤。
            Debug.LogError(
                "ParsedRecordItem 沒有分析結果。");

            // 中止儲存。
            return;
        }

        // 檢查帳目管理器是否存在。
        if (moneyRecordManager == null)
        {
            // 顯示錯誤。
            Debug.LogError(
                "ParsedRecordItem 沒有 MoneyRecordManager。");

            // 中止儲存。
            return;
        }

        // 建立正式帳目資料。
        MoneyRecord newRecord =
            new MoneyRecord();

        // 儲存日期。
        newRecord.date =
            parseResult.date.ToString("yyyy-MM-dd");

        // 儲存金額。
        newRecord.amount =
            parseResult.amount;

        // 儲存大分類。
        newRecord.category =
            parseResult.category;

        // 目前先將 itemName 同時作為小分類。
        newRecord.subCategory =
            parseResult.itemName;

        // 儲存品項。
        newRecord.itemName =
            parseResult.itemName;

        // 儲存付款方式。
        newRecord.paymentMethod =
            parseResult.paymentMethod;

        // 儲存備註。
        newRecord.note =
            parseResult.note;

        // 儲存收入或支出。
        newRecord.recordType =
            parseResult.recordType;

        // 呼叫原本已經存在的帳目儲存功能。
        moneyRecordManager.AddRecord(
            newRecord);

        // 儲存成功後刪掉這張等待處理的卡片。
        // 原始語音 / 輸入文字也會一起消失。
        Destroy(gameObject);
    }
}