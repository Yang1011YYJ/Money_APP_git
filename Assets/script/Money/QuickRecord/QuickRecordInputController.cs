using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// 建立快速記帳輸入介面控制器。

// 定義解析結果目前應該進入哪一個區域。
public enum ParseRecordStatus
{
    // 所有必要資料完整，可以等待直接儲存。
    Ready,

    // 帳目可以成立，但部分欄位需要人工確認。
    NeedReview,

    // 解析結果不足以建立帳目。
    Failed
}
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

    [Header("分析結果顯示區")]
    // 分析完整的結果要放的位置。
    [Tooltip("分析完整，可以直接儲存的區域")]public Transform readyContent;
    // 有不確定欄位的結果要放的位置。
    [Tooltip("需要人工確認的區域")]public Transform reviewContent;
    // 分析失敗的結果要放的位置。
    [Tooltip("分析失敗的區域")]public Transform failedContent;
    // 成功解析結果使用的 Prefab。
    [Tooltip("分析結果成功 Prefab")]public GameObject parsedRecordItemPrefab;
    // 失敗解析結果使用的 Prefab。
    [Tooltip("分析結果失敗 Prefab")] public GameObject failedRecordItemPrefab;

    // ==============================
    // 其他腳本
    // ==============================

    [Header("腳本")]
    [Tooltip("帳目資料管理器")] public MoneyRecordManager moneyRecordManager;
    // 原本正式帳目使用的編輯面板。
    public MoneyRecordEditPanel moneyRecordEditPanel;
    // 全系統共用的刪除確認框。
    public DeleteConfirmPanel deleteConfirmPanel;

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
    // 提供分析按鈕呼叫。
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

        // 保存這次輸入的原始文字。
        string originalSentence =
            sentenceInput.text;

        // 將輸入文字傳給規則解析器。
        latestResult =
            parser.Parse(
                originalSentence);


        // 判斷解析是否失敗。
        if (!latestResult.isSuccess)
        {
            // 原本的文字區仍然顯示失敗原因。
            parseResultText.text =
                latestResult.message;

            // 將失敗結果放進第三區。
            CreateFailedResult(
                latestResult,
                originalSentence);

            // 分析失敗，不繼續建立正常帳目卡片。
            return;
        }


        // 保留你原本的分析結果文字顯示。
        parseResultText.text =
            $"日期：{latestResult.date:yyyy-MM-dd}\n" +
            $"品項：{latestResult.itemName}\n" +
            $"金額：{latestResult.amount} 元\n" +
            $"付款方式：{latestResult.paymentMethod}\n" +
            $"分類：{latestResult.category}\n" +
            $"備註：{latestResult.note}\n" +
            $"類型：{GetRecordTypeText(latestResult.recordType)}";


        // 判斷這筆資料是否需要人工確認。
        bool needReview =
            NeedsReview(
                latestResult);


        // 如果需要人工確認。
        if (needReview)
        {
            // 將分析結果產生在第二區。
            CreateParsedRecordItem(
                latestResult,
                originalSentence,
                reviewContent);
        }
        else
        {
            // 資料完整時產生在第一區。
            CreateParsedRecordItem(
                latestResult,
                originalSentence,
                readyContent);
        }


        // 在 Console 顯示成功結果。
        Debug.Log(
            $"規則解析成功：" +
            $"日期={latestResult.date:yyyy-MM-dd}，" +
            $"品項={latestResult.itemName}，" +
            $"金額={latestResult.amount}，" +
            $"付款方式={latestResult.paymentMethod}，" +
            $"分類={latestResult.category}");
    }

    // 判斷一筆成功分析的資料是否需要人工確認。
    private bool NeedsReview(
        RecordParseResult result)
    {
        // 沒有大分類時需要確認。
        if (string.IsNullOrWhiteSpace(result.category))
        {
            // 回傳需要確認。
            return true;
        }

        // 大分類為其他時需要確認。
        if (result.category == "其他")
        {
            // 回傳需要確認。
            return true;
        }

        // 目前 itemName 暫時就是小分類，
        // 沒有小分類時需要確認。
        if (string.IsNullOrWhiteSpace(result.itemName))
        {
            // 回傳需要確認。
            return true;
        }

        // 小分類為其他時需要確認。
        if (result.itemName == "其他")
        {
            // 回傳需要確認。
            return true;
        }

        // 付款方式完全沒有資料時需要確認。
        if (string.IsNullOrWhiteSpace(result.paymentMethod))
        {
            // 回傳需要確認。
            return true;
        }

        // 付款方式為未指定或其他時需要確認。
        if (
            result.paymentMethod == "未指定" ||
            result.paymentMethod == "其他")
        {
            // 回傳需要確認。
            return true;
        }

        // 備註沒有資料沒關係，
        // 所以前面都正常就不需要人工確認。
        return false;
    }

    // 在指定顯示區建立一張分析結果卡片。
    private void CreateParsedRecordItem(
        RecordParseResult result,
        string originalSentence,
        Transform targetContent)
    {
        // 檢查 Prefab 是否存在。
        if (parsedRecordItemPrefab == null)
        {
            // 顯示錯誤。
            Debug.LogError(
                "QuickRecordInputController 沒有連接 Parsed Record Item Prefab。");

            // 中止建立。
            return;
        }

        // 檢查 Prefab 是否存在。
        if (failedRecordItemPrefab == null)
        {
            // 顯示錯誤。
            Debug.LogError(
                "QuickRecordInputController 沒有連接 Parsed Record Item Prefab。");

            // 中止建立。
            return;
        }

        // 檢查目標區域是否存在。
        if (targetContent == null)
        {
            // 顯示錯誤。
            Debug.LogError(
                "QuickRecordInputController 的分析結果 Content 沒有連接。");

            // 中止建立。
            return;
        }

        // 在指定 Content 底下建立 Prefab。
        GameObject itemObject =
            Instantiate(
                parsedRecordItemPrefab,
                targetContent);

        // 取得 Prefab 上的 ParsedRecordItem 腳本。
        ParsedRecordItem item =
            itemObject.GetComponent<ParsedRecordItem>();

        // 檢查腳本是否存在。
        if (item == null)
        {
            // 顯示錯誤。
            Debug.LogError(
                "ParsedRecordItem Prefab 沒有掛 ParsedRecordItem 腳本。");

            // 刪掉錯誤建立的物件。
            Destroy(itemObject);

            // 中止設定。
            return;
        }

        // 將分析結果、原始文字與帳目管理器傳給卡片。
        item.Setup(
            result,
            originalSentence);
    }

    // 將分析失敗的內容顯示在第三區。
    // 在無法辨識區建立一張正式的分析失敗卡片。
    private void CreateFailedResult(
        RecordParseResult result,
        string originalSentence)
    {
        // 檢查 FailedContent 是否有正確連接。
        if (failedContent == null)
        {
            // 顯示錯誤。
            Debug.LogError(
                "QuickRecordInputController 沒有連接 Failed Content。");

            // 中止建立。
            return;
        }


        // 檢查失敗資料 Prefab 是否有正確連接。
        if (failedRecordItemPrefab == null)
        {
            // 顯示錯誤。
            Debug.LogError(
                "QuickRecordInputController 沒有連接 Failed Record Item Prefab。");

            // 中止建立。
            return;
        }


        // 在 FailedContent 底下建立失敗資料 Prefab。
        GameObject itemObject =
            Instantiate(
                failedRecordItemPrefab,
                failedContent);


        // 取得 Prefab 上的 FailedRecordItem 腳本。
        FailedRecordItem item =
            itemObject.GetComponent<FailedRecordItem>();


        // 檢查 Prefab 是否真的有掛腳本。
        if (item == null)
        {
            // 顯示錯誤。
            Debug.LogError(
                "FailedRecordItem Prefab 沒有掛 FailedRecordItem 腳本。");

            // 刪除錯誤產生的物件。
            Destroy(itemObject);

            // 中止設定。
            return;
        }

        // 檢查 Scene 裡的控制器是否有成功連接。
        Debug.Log(
            $"CreateFailedResult：" +
            $"MoneyRecordEditPanel = {(moneyRecordEditPanel != null ? "有" : "NULL")}，" +
            $"DeleteConfirmPanel = {(deleteConfirmPanel != null ? "有" : "NULL")}");
        // 將失敗資料需要的所有 Scene 物件交給新產生的 Prefab。
        item.Setup(
            result,
            originalSentence,
            DateTime.Now,
            sentenceInput);
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

    // 根據解析結果判斷這筆資料要放在哪一區。
    private ParseRecordStatus GetParseStatus(
        RecordParseResult result)
    {
        // 沒有解析結果時視為失敗。
        if (result == null)
        {
            // 放入分析失敗區。
            return ParseRecordStatus.Failed;
        }

        // 解析器已經判斷失敗時，
        // 直接放入分析失敗區。
        if (!result.isSuccess)
        {
            // 回傳失敗。
            return ParseRecordStatus.Failed;
        }

        // 金額不存在時，
        // 已經不足以建立有效帳目。
        if (result.amount <= 0)
        {
            // 回傳失敗。
            return ParseRecordStatus.Failed;
        }

        // 大分類為空白或其他，
        // 代表系統無法確定分類。
        if (
            string.IsNullOrWhiteSpace(result.category) ||
            result.category == "其他")
        {
            // 放入需要人工確認區。
            return ParseRecordStatus.NeedReview;
        }

        // 品項目前暫時代表小分類。
        // 如果沒有辨識到，或只有其他，
        // 代表需要人工確認。
        if (
            string.IsNullOrWhiteSpace(result.itemName) ||
            result.itemName == "其他")
        {
            // 放入需要人工確認區。
            return ParseRecordStatus.NeedReview;
        }

        // 付款方式沒有判斷出來時，
        // 也先交由人工確認。
        if (
            string.IsNullOrWhiteSpace(result.paymentMethod) ||
            result.paymentMethod == "未指定" ||
            result.paymentMethod == "其他")
        {
            // 放入需要人工確認區。
            return ParseRecordStatus.NeedReview;
        }

        // 前面的條件都沒有發生，
        // 代表必要資訊完整。
        return ParseRecordStatus.Ready;
    }

    //開關
    public void OpenOrClosePanel()
    {
        QuickRecordPanel.SetActive(!QuickRecordPanel.activeSelf);
    }
}