using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class FailedRecordItem : MonoBehaviour
{
    [Header("UI")]
    [Tooltip("錯誤訊息")] public TextMeshProUGUI errorText;
    [Tooltip("原本的辨識文字")] public TextMeshProUGUI OriginalText;
    [Tooltip("日期")]public TextMeshProUGUI dateText;
    [Tooltip("重新辨識按紐")] public Button RetryButton;
    [Tooltip("編輯按紐")] public Button EditButton;
    [Tooltip("刪除按紐")] public Button DeleteButton;

    [Header("其他UI")]
    public GameObject QuickReccordPanel;

    [Header("暫存資料")]
    // 保存這筆資料的分析結果。
    private RecordParseResult parseResult;
    [Tooltip("原始輸入文字")]private string originalSentence;
    // 保存這筆資料收到時的日期與時間。
    private DateTime receivedDate;
    [Tooltip("快速記帳的輸入欄位")] private TMP_InputField sentenceInput;

    [Header("腳本")]
    [SerializeField]EventSystem eventSystem;
    // 保存快速記帳分析腳本。
    [SerializeField] private QuickRecordInputController quickRecordInputController;
    // 正式帳目使用的編輯面板。
    [SerializeField] MoneyRecordEditPanel MoneyRecordEditPanel;
    // 共用刪除確認框。
    [SerializeField] DeleteConfirmPanel deleteConfirmPanel;

    private void Awake()
    {
        eventSystem = EventSystem.current;
        MoneyRecordEditPanel = eventSystem.GetComponent<MoneyRecordEditPanel>();
        deleteConfirmPanel = eventSystem.GetComponent<DeleteConfirmPanel>();
        quickRecordInputController = eventSystem.GetComponent<MoneyRecordEditPanel>().QuickRecordPanel.GetComponent<QuickRecordInputController>();
    }

    // 初始化這張失敗卡片。
    public void Setup(
        RecordParseResult result,
        string sentence,
        DateTime receivedAt,
        TMP_InputField targetSentenceInput)
    {
        // 保存分析結果。
        parseResult = result;

        // 保存原始輸入文字。
        originalSentence = sentence;

        // 保存這筆資料真正被系統接收到的時間。
        receivedDate = receivedAt;

        // 保存原本的文字輸入框。
        sentenceInput = targetSentenceInput;

        // 顯示系統沒有辨識成功的欄位。
        errorText.text = BuildMissingFieldMessage();


        // 判斷原文有沒有明確提到日期。
        if (HasDateInformation(originalSentence))
        {
            // 如果原文包含日期，
            // 而且解析結果存在，
            // 就顯示解析出的帳目日期。
            if (parseResult != null)
            {
                // 顯示真正的帳目日期。
                dateText.text = $"{parseResult.date:yyyy-MM-dd}";
            }
            else
            {
                // 沒有解析結果時無法取得帳目日期，
                // 因此退回顯示接收日期。
                dateText.text = $"接收日期{receivedDate:yyyy-MM-dd}";
            }
        }
        else
        {
            // 原文完全沒有日期資訊時，
            // 顯示這筆文字或語音實際進入系統的日期。
            dateText.text = $"接收日期：{receivedDate:yyyy-MM-dd}";
        }


        // 顯示原始輸入文字。
        OriginalText.text = $"原文：{originalSentence}";


        // -----------------------------
        // 設定重新辨識按鈕
        // -----------------------------

        // 確認按鈕存在。
        if (RetryButton != null)
        {
            
        }


        // -----------------------------
        // 設定編輯按鈕
        // -----------------------------

        // 確認按鈕存在。
        if (EditButton != null)
        {
            
        }


        // -----------------------------
        // 設定刪除按鈕
        // -----------------------------

        // 確認按鈕存在。
        if (DeleteButton != null)
        {
            
        }

        // 除錯：確認 Scene 物件到底有沒有成功傳進來。
        Debug.Log(
            $"FailedRecordItem Setup：" +
            $"EditPanel = {(MoneyRecordEditPanel != null ? "有" : "NULL")}，" +
            $"DeleteConfirmPanel = {(deleteConfirmPanel != null ? "有" : "NULL")}");

    }


    // 根據解析結果整理「到底缺了哪些資料」。
    private string BuildMissingFieldMessage()
    {
        // 如果連分析結果都不存在，
        // 就無法判斷任何帳目內容。
        if (parseResult == null)
        {
            // 回傳通用錯誤。
            return "無法辨識：無法取得分析結果";
        }


        // 建立缺失欄位文字。
        string missingFields ="";


        // -----------------------------
        // 金額
        // -----------------------------

        // 金額小於等於 0，
        // 代表沒有成功辨識出有效金額。
        if (parseResult.amount <= 0)
        {
            // 加入金額。
            missingFields = AddMissingField(missingFields, "金額");
        }


        // -----------------------------
        // 大分類
        // -----------------------------

        // 大分類完全沒有資料時視為沒有辨識成功。
        if (string.IsNullOrWhiteSpace(parseResult.category))
        {
            // 加入大分類。
            missingFields = AddMissingField(missingFields, "大分類");
        }


        // -----------------------------
        // 小分類
        // -----------------------------

        // 目前 itemName 暫時代表解析器辨識出的小分類。
        if (string.IsNullOrWhiteSpace(parseResult.itemName))
        {
            // 加入小分類。
            missingFields = AddMissingField( missingFields, "子分類");
        }


        // -----------------------------
        // 付款方式
        // -----------------------------

        // 付款方式完全沒有值時，
        // 代表沒有成功辨識。
        if (string.IsNullOrWhiteSpace(parseResult.paymentMethod))
        {
            // 加入付款方式。
            missingFields = AddMissingField(missingFields,"付款方式");
        }


        // 如果前面沒有找到明確缺失欄位，
        // 就使用 Parser 自己回傳的失敗原因。
        if (string.IsNullOrWhiteSpace(missingFields))
        {
            // 如果 Parser 本身有錯誤訊息。
            if (!string.IsNullOrWhiteSpace(parseResult.message))
            {
                // 顯示 Parser 的錯誤原因。
                return $"無法辨識：{parseResult.message}";
            }

            // 都沒有時使用通用文字。
            return "無法辨識：資料不足，無法建立帳目";
        }


        // 顯示實際缺少的欄位。
        return $"無法辨識：缺少 {missingFields} 欄位。";
    }


    // 將新的缺失欄位加到目前的文字後面。
    private string AddMissingField(
        string currentText,
        string newField)
    {
        // 如果目前還沒有任何欄位。
        if (string.IsNullOrWhiteSpace(currentText))
        {
            // 直接回傳第一個欄位。
            return newField;
        }

        // 已經有其他欄位時，
        // 使用頓號分隔。
        return currentText + "、" + newField;
    }


    // 判斷原始文字中是否有日期資訊。
    private bool HasDateInformation(string text)
    {
        // 空白文字不可能包含日期。
        if (string.IsNullOrWhiteSpace(text))
        {
            // 回傳沒有日期。
            return false;
        }


        // -----------------------------
        // 相對日期
        // -----------------------------

        // 判斷常用的日期詞。
        if (text.Contains("今天") || text.Contains("昨天") || text.Contains("前天") || text.Contains("明天") || text.Contains("後天"))
        {
            // 有日期資訊。
            return true;
        }


        // -----------------------------
        // 月／日形式
        // -----------------------------

        // 例如：
        // 7月30日
        // 7月30號
        if (Regex.IsMatch(text, @"\d{1,2}月\d{1,2}[日號]"))
        {
            // 有日期資訊。
            return true;
        }


        // -----------------------------
        // 數字日期形式
        // -----------------------------

        // 例如：
        // 2026/7/30
        // 2026-07-30
        if (Regex.IsMatch(text, @"\d{4}[-/]\d{1,2}[-/]\d{1,2}"))
        {
            // 有日期資訊。
            return true;
        }


        // 沒有找到任何日期資訊。
        return false;
    }


    // 使用原始文字直接重新進行一次分析。
    public void RetryRecognition()
    {
        // 檢查輸入框是否存在。
        if (sentenceInput == null)
        {
            // 顯示錯誤。
            Debug.LogError("FailedRecordItem 沒有 Sentence Input。");

            // 中止。
            return;
        }


        // 檢查分析腳本是否存在。
        if (quickRecordInputController == null)
        {
            // 顯示錯誤。
            Debug.LogError("FailedRecordItem 沒有 QuickRecordInputController。");

            // 中止。
            return;
        }


        // 把原始文字放回輸入框。
        sentenceInput.text = originalSentence;

        // 強制更新畫面。
        sentenceInput.ForceLabelUpdate();


        // 先刪掉舊的失敗卡片，
        // 避免重新分析後留下兩張相同資料。
        Destroy(gameObject);


        // 直接使用原本的分析功能重新解析。
        quickRecordInputController.ParseInput();
    }


    // 開啟正式帳目的編輯面板，
    // 讓使用者補完這筆無法辨識的資料。
    public void EditRecord()
    {
        // 檢查編輯面板是否存在。
        if (MoneyRecordEditPanel == null)
        {
            // 顯示錯誤。
            Debug.LogError(
                "FailedRecordItem 沒有連接 MoneyRecordEditPanel。");

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
        }
        else
        {
            // 完全沒有分析結果時，
            // 日期至少使用這筆資料的接收日期。
            draftRecord.date =
                receivedDate.ToString(
                    "yyyy-MM-dd");

            // 金額先留 0，
            // 讓使用者進編輯面板補上。
            draftRecord.amount =
                0;

            // 無法判斷分類時使用其他。
            draftRecord.category =
                "其他";

            // 小分類也先使用其他。
            draftRecord.subCategory =
                "其他";

            // 品項先使用其他。
            draftRecord.itemName =
                "其他";

            // 付款方式先標示未指定。
            draftRecord.paymentMethod =
                "未指定";

            // 備註先留空。
            draftRecord.note =
                "";

            // 預設先當成支出。
            draftRecord.recordType =
                RecordType.Expense;
        }


        // 開啟原本已經做好的編輯面板。
        // 這時候不刪除 FailedRecordItem。
        MoneyRecordEditPanel.OpenNewRecordPanel(
            draftRecord,
            OnEditSaved);
    }

    // 當編輯面板真的成功將帳目加入正式資料後執行。
    public void OnEditSaved()
    {
        // 這時資料已經正式加入 MoneyRecordManager，
        // Failed 暫存才可以安全刪除。
        Destroy(gameObject);
    }

    // 刪除這筆無法辨識資料。
    public void DeleteRecord()
    {
        // 檢查共用確認框是否存在。
        if (deleteConfirmPanel == null)
        {
            // 顯示錯誤。
            Debug.LogError(
                "FailedRecordItem 沒有連接 DeleteConfirmPanel。");

            // 不直接刪除，
            // 避免確認框沒接好時誤刪資料。
            return;
        }

        // 開啟刪除確認框。
        deleteConfirmPanel.Show(ConfirmDelete,
            "確定要刪除這筆無法辨識的紀錄嗎？");
    }

    // 使用者在確認框按下「確認」後才真正執行。
    public void ConfirmDelete()
    {
        // 刪除這筆 Failed 暫存資料。
        Destroy(gameObject);
    }
}
