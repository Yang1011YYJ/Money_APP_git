// 引入 TextMeshPro 命名空間。
using TMPro;

// 引入 Unity 基本功能。
using UnityEngine;

// 建立快速記帳輸入介面控制器。
public class QuickRecordInputController : MonoBehaviour
{
    // 在 Inspector 中建立介面分類。
    [Header("快速記帳介面")]

    // 接收使用者輸入自然語句的欄位。
    [Tooltip("自然語句輸入欄位")]
    public TMP_InputField sentenceInput;

    // 顯示解析結果的文字。
    [Tooltip("顯示規則解析結果")]
    public TextMeshProUGUI parseResultText;

    // 儲存規則式文字解析器。
    private RuleBasedRecordParser parser;

    // 儲存最近一次解析成功的結果。
    private RecordParseResult latestResult;

    // 物件初始化時執行。
    private void Awake()
    {
        // 建立規則式解析器。
        parser = new RuleBasedRecordParser();
    }

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
}