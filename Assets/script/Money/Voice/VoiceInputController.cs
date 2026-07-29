// 引入 IEnumerator，讓程式可以使用 Coroutine。
using System.Collections;

// 引入 TextMeshPro，讓程式可以控制原本的 TMP_InputField。
using TMPro;

// 引入 Unity 基本功能。
using UnityEngine;

// 引入 Unity UI，讓程式可以控制 Button。
using UnityEngine.UI;

// Windows 平台使用 Unity 內建的語音聽寫功能。
#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
using UnityEngine.Windows.Speech;
#endif


// 負責控制快速記帳的語音輸入。
public class VoiceInputController : MonoBehaviour
{
    // -----------------------------
    // Inspector：UI
    // -----------------------------

    [Header("語音輸入 UI")]

    // 原本快速記帳使用的文字輸入欄位。
    // 語音辨識結果會直接寫進這裡。
    public TMP_InputField sentenceInput;

    // 開啟 / 關閉麥克風的按鈕。
    public Button micButton;


    // -----------------------------
    // Inspector：其他腳本
    // -----------------------------

    [Header("腳本")]

    // 原本負責分析文字的 QuickRecordInputController。
    // 停止語音輸入後會直接呼叫它的 ParseInput()。
    public QuickRecordInputController quickRecordInputController;


    // -----------------------------
    // Inspector：設定
    // -----------------------------

    [Header("語音輸入設定")]

    // 開啟麥克風後，幾秒內不允許再次按按鈕。
    [SerializeField]
    private float buttonLockSeconds = 10f;


    // -----------------------------
    // 執行狀態
    // -----------------------------

    // 記錄目前是否正在進行語音輸入。
    private bool isListening = false;
    // 暫存這一次語音輸入已經確認的辨識文字。
    [Tooltip("暫存這一次語音輸入已經確認的辨識文字")][SerializeField] string recognizedText = "";
    // 暫存語音辨識過程中最後一次的推測文字。
    // 平常不會顯示在 Console，只在辨識失敗時拿來除錯。
    [Tooltip("暫存語音辨識過程中最後一次的推測文字")][SerializeField]private string lastHypothesisText = "";

#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN

    // Windows 使用的語音聽寫辨識器。
    private DictationRecognizer dictationRecognizer;

#endif


    // 場景開始時執行。
    private void Start()
    {
        // 檢查麥克風按鈕是否有正確連接。
        if (micButton == null)
        {
            // 顯示錯誤。
            Debug.LogError(
                "VoiceInputController 沒有連接 Mic Button。");

            // 不繼續設定按鈕。
            return;
        }

        // 清除 Inspector 可能留下的舊事件。
        micButton.onClick.RemoveAllListeners();

        // 按下麥克風按鈕時，
        // 執行切換語音輸入狀態。
        micButton.onClick.AddListener(
            ToggleVoiceInput);
    }


    // 按下麥克風按鈕時執行。
    public void ToggleVoiceInput()
    {
        // 如果目前沒有在聽，
        // 就開始語音輸入。
        if (!isListening)
        {
            // 開始語音輸入。
            StartVoiceInput();
        }
        else
        {
            // 目前正在聽時，
            // 第二次按下按鈕就停止。
            StopVoiceInput();
        }
    }


    // 開始語音輸入。
    private void StartVoiceInput()
    {
        // 檢查輸入欄位是否存在。
        if (sentenceInput == null)
        {
            // 顯示錯誤。
            Debug.LogError(
                "VoiceInputController 沒有連接 Sentence Input。");

            // 中止。
            return;
        }
        // 清空上一輪語音辨識留下的暫存文字。
        // 只清除語音暫存，不會刪掉使用者原本手動輸入的內容。
        recognizedText = "";
        // 清空上一輪留下的推測文字。
        lastHypothesisText = "";

#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN

        // 如果之前已經建立過辨識器，
        // 先釋放舊資源。
        if (dictationRecognizer != null)
        {
            // 釋放辨識器。
            dictationRecognizer.Dispose();

            // 清除舊參考。
            dictationRecognizer = null;
        }


        // 建立新的 Windows 語音聽寫辨識器。
        dictationRecognizer =
            new DictationRecognizer();

        // 當系統產生暫時推測文字時執行。
        // 只會記錄，不會每次都顯示 Debug。
        dictationRecognizer.DictationHypothesis +=
            OnDictationHypothesis;

        // 當系統辨識出一段確定文字時執行。
        dictationRecognizer.DictationResult +=
            OnDictationResult;

        // 當語音辨識結束時執行。
        dictationRecognizer.DictationComplete +=
            OnDictationComplete;


        // 當辨識器發生錯誤時執行。
        dictationRecognizer.DictationError +=
            OnDictationError;


        // 將目前狀態改成正在聽。
        isListening = true;


        // 開始 Windows 語音辨識。
        dictationRecognizer.Start();


        // 啟動 10 秒按鈕鎖定。
        StartCoroutine(
            LockMicButton());


        // 顯示狀態。
        Debug.Log(
            "開始語音輸入。");

#else

        // 非 Windows 平台目前還沒有接手機版語音辨識。
        Debug.LogWarning(
            "目前的語音辨識版本只先支援 Windows 測試。");

#endif
    }


    // 停止語音輸入。
    // 停止語音輸入。
    private void StopVoiceInput()
    {
#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN

        // 辨識器不存在時直接結束。
        if (dictationRecognizer == null)
        {
            // 復原狀態。
            isListening = false;

            // 結束。
            return;
        }

        // 如果語音辨識仍然正在運作。
        if (
            dictationRecognizer.Status ==
            SpeechSystemStatus.Running)
        {
            // 要求語音辨識停止。
            dictationRecognizer.Stop();
        }

#endif
    }


#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN

    // 當 Windows 產生尚未完全確認的語音辨識結果時執行。
    private void OnDictationHypothesis(
        string text)
    {
        // 如果沒有任何文字，
        // 就不更新暫存內容。
        if (string.IsNullOrWhiteSpace(text))
        {
            // 結束這次事件。
            return;
        }

        // 保存目前最後一次的推測結果。
        // 不在這裡 Debug，
        // 避免每次辨識一兩個字就一直洗 Console。
        lastHypothesisText =
            text;
    }

    // 收到一段完整辨識結果時執行。
    private void OnDictationResult(
        string text,
        ConfidenceLevel confidence)
    {
        // 防止回傳空白文字。
        if (string.IsNullOrWhiteSpace(text))
        {
            // 不處理空白結果。
            return;
        }


        // 如果輸入框目前已經有文字。
        if (!string.IsNullOrWhiteSpace(recognizedText))
        {
            // 在原文字後面補一個空格，
            // 再加入新的辨識結果。
            // 將新的確定辨識結果接到暫存文字後面。
            recognizedText +=
                " " + text;
        }
        else
        {
            // 原本沒有文字時，
            // 直接放入辨識結果。
            recognizedText =
                text;
        }


        //// 將游標移到文字最後面。
        //sentenceInput.caretPosition =
        //    sentenceInput.text.Length;


        // 顯示辨識結果。
        Debug.Log(
            $"語音辨識結果：{text}");
    }

    // 語音辨識器自行結束時執行。
    private void OnDictationComplete(
        DictationCompletionCause cause)
    {
        // 將狀態改成沒有在聽。
        isListening = false;


        // 如果不是正常完成。
        if (cause != DictationCompletionCause.Complete)
        {
            // 顯示原因。
            Debug.LogWarning(
                $"語音辨識結束：{cause}");
        }

        // 將這一次真正完成的語音文字送進輸入框。
        ApplyRecognizedText();
    }


    // 語音辨識發生錯誤時執行。
    private void OnDictationError(
        string error,
        int hresult)
    {
        // 顯示錯誤資訊。
        Debug.LogError(
            $"語音辨識錯誤：" +
            $"{error}，" +
            $"HResult = {hresult}");


        // 將狀態復原。
        isListening = false;
    }

    // 將本次語音辨識結果正式加入原本的輸入框。
    private void ApplyRecognizedText()
    {
        // 如果這一次完全沒有取得辨識文字。
        if (string.IsNullOrWhiteSpace(recognizedText))
        {
            // 顯示提醒。
            Debug.LogWarning("這次語音輸入沒有取得有效文字。");

            // 顯示正式結果目前的內容。
            Debug.Log($"正式辨識結果 recognizedText：[{recognizedText}]");

            // 顯示最後一次推測的內容。
            Debug.Log($"最後推測結果 lastHypothesisText：[{lastHypothesisText}]");


            // 不進行分析。
            return;
        }

        // 如果輸入框原本已經有文字。
        if (!string.IsNullOrWhiteSpace(sentenceInput.text))
        {
            // 保留原本手動輸入文字，
            // 再加入這次語音輸入。
            sentenceInput.text =
                sentenceInput.text +
                " " +
                recognizedText;
        }
        else
        {
            // 原本沒有文字時，
            // 直接使用語音辨識結果。
            sentenceInput.text =
                recognizedText;
        }

        // 將游標移到輸入文字最後方。
        sentenceInput.caretPosition =
            sentenceInput.text.Length;

        // 強制 TMP_InputField 更新目前顯示。
        sentenceInput.ForceLabelUpdate();

        // 顯示最後取得的完整文字。
        Debug.Log(
            $"語音輸入完成：{recognizedText}");


        // 檢查分析腳本是否存在。
        if (quickRecordInputController != null)
        {
            // 直接呼叫原本的分析功能。
            quickRecordInputController.ParseInput();
        }

        // 完成後清掉這次語音暫存，
        // 避免下一次重複使用。
        recognizedText =
            "";
    }

#endif


    // 麥克風開始後，
    // 暫時禁止使用者再次點擊按鈕。
    private IEnumerator LockMicButton()
    {
        // 檢查按鈕是否存在。
        if (micButton == null)
        {
            // 沒有按鈕就直接結束。
            yield break;
        }


        // 暫時禁止按鈕互動。
        micButton.interactable =
            false;


        // 等待設定的秒數。
        yield return new WaitForSeconds(
            buttonLockSeconds);


        // 10 秒後恢復按鈕互動。
        micButton.interactable =
            true;
    }


    // 物件被銷毀時執行。
    private void OnDestroy()
    {
#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN

        // 如果辨識器不存在，
        // 不需要處理。
        if (dictationRecognizer == null)
        {
            // 結束。
            return;
        }


        // 如果辨識器仍然正在執行。
        if (
            dictationRecognizer.Status ==
            SpeechSystemStatus.Running)
        {
            // 先停止辨識。
            dictationRecognizer.Stop();
        }


        // 解除事件。
        dictationRecognizer.DictationResult -=
            OnDictationResult;

        // 解除推測辨識結果事件。
        dictationRecognizer.DictationHypothesis -=
            OnDictationHypothesis;

        dictationRecognizer.DictationComplete -=
            OnDictationComplete;

        dictationRecognizer.DictationError -=
            OnDictationError;


        // 釋放語音辨識資源。
        dictationRecognizer.Dispose();


        // 清除參考。
        dictationRecognizer = null;

#endif
    }
}