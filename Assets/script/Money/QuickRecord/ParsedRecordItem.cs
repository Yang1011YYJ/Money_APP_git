// 引入 TextMeshPro，讓程式可以控制 TMP 文字。
using TMPro;

// 引入 Unity 基本功能。
using UnityEngine;
using UnityEngine.EventSystems;


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

    [Header("腳本")]
    [SerializeField] EventSystem eventSystem;
    // 儲存帳目管理器。
    [SerializeField]private MoneyRecordManager moneyRecordManager;
    [SerializeField]private MoneyRecordEditPanel moneyRecordEditPanel;

    private void Awake()
    {
        eventSystem = EventSystem.current;
        moneyRecordManager = eventSystem.GetComponent<MoneyRecordManager>();
        moneyRecordEditPanel = eventSystem.GetComponent<MoneyRecordEditPanel>();
    }

    // 初始化這張分析結果卡片。
    public void Setup(
        RecordParseResult result,
        string sentence)
    {
        // 保存分析結果。
        parseResult = result;

        // 保存原始文字。
        originalSentence = sentence;

        // 顯示日期。
        dateText.text =
            parseResult.date.ToString("yyyy-MM-dd");

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
    }


    // 將這張卡片的分析結果正式儲存。
    public void SaveRecord()
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

    // 開啟正式帳目的編輯面板，
    // 讓使用者補完這筆無法辨識的資料。
    public void EditRecord()
    {
        // 檢查編輯面板是否存在。
        if (moneyRecordEditPanel == null)
        {
            // 顯示錯誤。
            Debug.LogError(
                "ParsedRecordItem 沒有連接 MoneyRecordEditPanel。");

            // 中止。
            return;
        }


        // 建立一筆尚未正式儲存的帳目資料。
        MoneyRecord draftRecord =
            new MoneyRecord();


        // 如果有分析結果。
        if (parseResult != null)
        {
            // 使用分析器目前取得的日期。
            draftRecord.date =
                parseResult.date.ToString(
                    "yyyy-MM-dd");

            // 使用目前分析出的金額。
            draftRecord.amount =
                parseResult.amount;

            // 使用目前分析出的大分類。
            draftRecord.category =
                parseResult.category;

            // 目前 itemName 暫時作為小分類。
            draftRecord.subCategory =
                parseResult.itemName;

            // 保存目前分析出的品項。
            draftRecord.itemName =
                parseResult.itemName;

            // 保存付款方式。
            draftRecord.paymentMethod =
                parseResult.paymentMethod;

            // 保存備註。
            draftRecord.note =
                parseResult.note;

            // 保存收入或支出。
            draftRecord.recordType =
                parseResult.recordType;

            // 開啟原本已經做好的編輯面板。
            // 這時候不刪除 FailedRecordItem。
            moneyRecordEditPanel.OpenNewRecordPanel(
                draftRecord,
                OnEditSaved);
        }
    }

    // 當使用者在編輯面板完成修改，
    // 並且成功加入正式帳目後執行。
    private void OnEditSaved()
    {
        // 正式帳目已經由 MoneyRecordEditPanel
        // 呼叫 MoneyRecordManager.AddRecord() 儲存完成。

        // 此時才刪除等待確認 / 需要確認區中的這張卡片。
        Destroy(gameObject);
    }
}