using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
// 引入 WhisperManager。
using Whisper;

// 引入 MicrophoneRecord 與 AudioChunk。
using Whisper.Utils;


// 負責控制快速記帳的語音輸入。
public class VoiceInputController : MonoBehaviour
{
    [Header("語音輸入 UI")]
    // 原本快速記帳使用的文字輸入欄位。
    // 語音辨識結果會直接寫進這裡。
    public TMP_InputField sentenceInput;
    // 開啟 / 關閉麥克風的按鈕。
    public Button micButton;

    [Header("Whisper")]
    public WhisperManager whisperManager;// Whisper 模型管理器腳本。
    public MicrophoneRecord microphoneRecord;// Whisper 套件提供的麥克風錄音器腳本。


    [Header("腳本")]
    // 原本負責分析文字的 QuickRecordInputController。
    // 停止語音輸入後會直接呼叫它的 ParseInput()。
    public QuickRecordInputController quickRecordInputController;

    [Header("語音輸入設定")]
    [SerializeField]private float buttonLockSeconds = 10f;// 開啟麥克風後，幾秒內不允許再次按按鈕。

    
    private bool isRecording = false;// 記錄目前是否正在進行語音輸入。
    [Tooltip("暫存這一次語音輸入已經確認的辨識文字")][SerializeField] string recognizedText = "";
    [Tooltip("暫存語音辨識過程中最後一次的推測文字")][SerializeField]private string lastHypothesisText = "";

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
        // 檢查 MicrophoneRecord 是否存在。
        if (microphoneRecord == null)
        {
            // 顯示錯誤。
            Debug.LogError(
                "VoiceInputController 沒有連接 MicrophoneRecord。");

            // 中止初始化。
            return;
        }

        // 清除 Inspector 可能留下的舊事件。
        micButton.onClick.RemoveAllListeners();

        // 按下麥克風按鈕時，
        // 執行切換語音輸入狀態。
        micButton.onClick.AddListener(ToggleVoiceInput);

        // 訂閱 MicrophoneRecord 的停止錄音事件。
        // StopRecord() 完成後會把錄好的 AudioChunk 傳進來。
        microphoneRecord.OnRecordStop += OnRecordStop;
    }


    // 按下麥克風按鈕時執行。
    public void ToggleVoiceInput()
    {
        // 如果目前沒有在聽，
        // 就開始語音輸入。
        if (!isRecording)
        {
            // 開始語音輸入。
            StartVoiceInput();
        }
        else
        {
            // 目前正在聽時，第二次按下按鈕就停止。
            StopVoiceInput();
        }
    }


    // 開始語音輸入。
    private void StartVoiceInput()
    {
        // 檢查 MicrophoneRecord 是否存在。
        if (microphoneRecord == null)
        {
            // 顯示錯誤。
            Debug.LogError("MicrophoneRecord 尚未設定。");

            // 中止。
            return;
        }

        

        // 清空上一輪語音辨識留下的暫存文字。
        // 只清除語音暫存，不會刪掉使用者原本手動輸入的內容。
        recognizedText = "";
        // 清空上一輪留下的推測文字。
        lastHypothesisText = "";

        // 開始錄音。
        microphoneRecord.StartRecord();

        // 將目前狀態改成正在聽。
        isRecording = true;

        // 暫時禁止麥克風按鈕。
        StartCoroutine(LockMicButton());

        // 顯示除錯訊息。
        Debug.Log("Whisper 麥克風開始錄音。");

        // 啟動 10 秒按鈕鎖定。
        StartCoroutine(LockMicButton());
    }


    // 停止語音輸入。
    private void StopVoiceInput()
    {
        // 檢查 MicrophoneRecord 是否存在。
        if (microphoneRecord == null)
        {
            // 顯示錯誤。
            Debug.LogError("MicrophoneRecord 尚未設定。");

            // 中止。
            return;
        }

        // 先將狀態改成沒有錄音。
        isRecording = false;

        // 停止錄音。
        // 完成後 MicrophoneRecord 會觸發 OnRecordStop。
        microphoneRecord.StopRecord();

        // 顯示除錯訊息。
        Debug.Log("Whisper 麥克風停止錄音，準備辨識。");
    }

    // MicrophoneRecord 停止後會呼叫這個函式。
    // AudioChunk 內含真正錄到的聲音資料。
    private async void OnRecordStop(
        AudioChunk recordedAudio)
    {
        // 檢查 WhisperManager 是否存在。
        if (whisperManager == null)
        {
            // 顯示錯誤。
            Debug.LogError(
                "WhisperManager 尚未設定。");

            // 中止辨識。
            return;
        }

        // 檢查是否真的錄到聲音資料。
        if (
            recordedAudio.Data == null ||
            recordedAudio.Data.Length == 0)
        {
            // 顯示警告。
            Debug.LogWarning(
                "這次麥克風沒有錄到任何音訊資料。");

            // 中止辨識。
            return;
        }

        // 顯示這次錄音的基本資訊。
        Debug.Log(
            $"開始 Whisper 辨識。" +
            $"長度={recordedAudio.Length:F2} 秒，" +
            $"取樣率={recordedAudio.Frequency}，" +
            $"聲道={recordedAudio.Channels}");


        // 將錄到的原始音訊資料交給 Whisper。
        // WhisperManager 會自行處理必要的音訊預處理與重新取樣。
        WhisperResult result =
            await whisperManager.GetTextAsync(
                recordedAudio.Data,
                recordedAudio.Frequency,
                recordedAudio.Channels);


        // 檢查 Whisper 是否成功回傳結果。
        if (result == null)
        {
            // 顯示錯誤。
            Debug.LogError(
                "Whisper 辨識失敗，沒有取得 WhisperResult。");

            // 中止。
            return;
        }


        // 取得 Whisper 最終辨識文字。
        string recognizedText =
            result.Result;


        // 顯示辨識結果供目前除錯。
        Debug.Log(
            $"Whisper 辨識結果：[{recognizedText}]");


        // 檢查結果是否只有空白。
        if (string.IsNullOrWhiteSpace(recognizedText))
        {
            // 顯示警告。
            Debug.LogWarning(
                "Whisper 沒有辨識出有效文字。");

            // 中止。
            return;
        }


        // 去掉前後多餘空白。
        recognizedText =
            recognizedText.Trim();


        // 如果原本的輸入框已有手動輸入內容。
        if (
            sentenceInput != null &&
            !string.IsNullOrWhiteSpace(sentenceInput.text))
        {
            // 保留原本文字，
            // 再把這次語音辨識文字接在後面。
            sentenceInput.text =
                sentenceInput.text +
                " " +
                recognizedText;
        }
        else if (sentenceInput != null)
        {
            // 原本輸入框沒有文字時，
            // 直接放入 Whisper 的結果。
            sentenceInput.text =
                recognizedText;
        }


        // 如果輸入框存在。
        if (sentenceInput != null)
        {
            // 將游標放到最後面。
            sentenceInput.caretPosition =
                sentenceInput.text.Length;

            // 強制更新 TMP_InputField 畫面。
            sentenceInput.ForceLabelUpdate();
        }


        // 確認原本的分析器存在。
        if (quickRecordInputController != null)
        {
            // 直接呼叫目前已經做好的文字分析。
            quickRecordInputController.ParseInput();
        }
    }

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
        micButton.interactable = false;


        // 等待設定的秒數。
        yield return new WaitForSeconds(buttonLockSeconds);


        // 10 秒後恢復按鈕互動。
        micButton.interactable = true;
    }


    // 物件被銷毀時執行。
    private void OnDestroy()
    {// 如果 MicrophoneRecord 不存在，就沒有事件需要解除。
        if (microphoneRecord == null)
        {
            // 結束。
            return;
        }

        // 解除錄音停止事件。
        microphoneRecord.OnRecordStop -= 
            OnRecordStop;
    }
}