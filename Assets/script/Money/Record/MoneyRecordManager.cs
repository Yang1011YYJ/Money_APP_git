using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using System.IO;
using Newtonsoft.Json;

public class MoneyRecordManager : MonoBehaviour
{
    [Header("輸入介面")]
    [Tooltip("輸入金額的欄位")] public TMP_InputField amountInput;
    [Tooltip("分類的下拉清單")] public TMP_Dropdown categoryDropdown;

    [Header("腳本")]
    public CalenderControll calenderControllScript;

    [Header("目前帳目類型")]
    [Tooltip("目前選擇的是支出還是收入")][SerializeField]RecordType currentRecoedType = RecordType.Expense;
    [Tooltip("儲存目前建立的所有帳目")][SerializeField]List<MoneyRecord> moneyRecords = new List<MoneyRecord>();

    [Header("帳目顯示")]
    [Tooltip("單筆資料的prefeb")] public GameObject recordItemPrefab;
    [Tooltip("顯示每日帳目的content")] public Transform dailyRecordContent;

    [Tooltip("帳目編輯面板")] public MoneyRecordEditPanel editRecordPanel;

    [Header("資料儲存")]
    [Tooltip("儲存檔案名稱")] public string saveFileName = "moneyRecords.json";

    //取得完整的帳目檔案儲存路徑
    string SaveFilePath
    {
        get
        {
            return Path.Combine(Application.persistentDataPath, saveFileName);
        }
    }

    private void Awake()
    {
        LoadFromFile();
    }

    //當app暫停或進入背景時執行
    private void OnApplicationPause(bool pauseStatus)
    {
        if (pauseStatus)//正在暫停狀態
        {
            SaveToFile();
        }
    }

    //當APP關閉時執行
    private void OnApplicationQuit()
    {
        SaveToFile();
    }
    public void SaveToFile()
    {
        try
        {
            //將帳目清單轉換為格式化後的JSON文字
            string json = JsonConvert.SerializeObject(moneyRecords, Formatting.Indented);

            // 將 JSON 文字寫入指定檔案。
            // 如果檔案不存在，會自動建立。
            // 如果檔案已存在，會覆蓋成最新內容。
            File.WriteAllText(
                SaveFilePath,
                json);

            Debug.Log(
                $"帳目資料儲存成功，" +
                $"共儲存 {moneyRecords.Count} 筆資料。\n" +
                $"儲存位置:{SaveFilePath}");
        }
        catch (Exception exception){
                Debug.LogError($"帳目資料儲存失敗:{exception.Message}");
            }
        }

    //從本機json檔案讀取帳目資料
    public void LoadFromFile()
    {
        try
        {
            //判斷儲存檔案是否存在
            if (!File.Exists(SaveFilePath))
            {
                //第一次啟動沒有檔案，建立空清單
                moneyRecords = new List<MoneyRecord>();

                //顯示沒有舊資料
                Debug.Log(
                    $"尚未找到帳目儲存檔案，" +
                    $"將從空白資料開始。\n" +
                    $"預計儲存位置: {SaveFilePath}");

                return;
            }
            //讀取存檔中的所有JSON文字
            string json = File.ReadAllText(SaveFilePath);

            //判斷是否為空
            if (string.IsNullOrWhiteSpace(json))
            {
                //空檔案視為沒有資料
                moneyRecords = new List<MoneyRecord>();

                Debug.LogWarning("帳目儲存檔案存在，但內容是空的。");

                return;
            }

            //將json轉回moneyrecord清單
            List<MoneyRecord> loadedRecords = JsonConvert.DeserializeObject<List<MoneyRecord>>(json);

            //判斷解析後的清單是否存在
            if (loadedRecords == null)
            {
                //解析為空的時候使用新的空清單
                moneyRecords = new List<MoneyRecord>();
            }
            else
            {
                //將讀取到的帳目保存到目前使用中的清單
                moneyRecords = loadedRecords;
            }

            //顯示
            Debug.Log(
                $"帳目資料讀取成功，" +
                $"共載入 {moneyRecords.Count} 筆資料。\n" +
                $"讀取位置: {SaveFilePath}");
        }
        catch (Exception exception)
        {
            //當檔案讀取失敗或損壞先建立新資料
            moneyRecords = new List<MoneyRecord>();

            //顯示讀取失敗原因
            Debug.LogError($"帳目資料讀取失敗:{exception.Message}");
        }
    } 
    //將目前帳目類型設定為支出
    public void SelectExpense()
    {
        currentRecoedType = RecordType.Expense;

        Debug.Log("目前選擇:支出");
    }

    //將目前帳目類型設定為收入
    public void SelectIncome()
    {
        currentRecoedType = RecordType.Income;

        Debug.Log("目前選擇:收入");
    }

    //儲存帳目
    public void AddRecord()
    {
        // 檢查金額輸入欄位是否已經在 Inspector 中連接。
        if (amountInput == null)
        {
            // 如果沒有連接，在 Console 顯示錯誤訊息。
            Debug.LogError("MoneyRecordManager 的 Amount Input 尚未連接。");

            // 中止新增帳目。
            return;
        }

        // 檢查分類選單是否已經在 Inspector 中連接。
        if (categoryDropdown == null)
        {
            // 如果沒有連接，在 Console 顯示錯誤訊息。
            Debug.LogError("MoneyRecordManager 的 Category Dropdown 尚未連接。");

            // 中止新增帳目。
            return;
        }

        // 檢查日曆控制腳本是否已經在 Inspector 中連接。
        if (calenderControllScript == null)
        {
            // 如果沒有連接，在 Console 顯示錯誤訊息。
            Debug.LogError("MoneyRecordManager 的 Calender Controll 尚未連接。");

            // 中止新增帳目。
            return;
        }

        //把金額欄位的文字換成分數
        bool amountSuccess = int.TryParse(amountInput.text, out int amount);

        //金額轉換失敗或小於等於0
        if(!amountSuccess || amount <= 0)
        {
            Debug.LogWarning("請輸入大於0的正確金額。");
            //終止新增項目
            return;
        }

        //取得分類的文字
        string selectedCategory = categoryDropdown.options[categoryDropdown.value].text;

        //取得目前日期
        DateTime selectedDate = calenderControllScript.GetSelectedDate();

        //建立新的帳目資料
        MoneyRecord newRecord = new MoneyRecord();

        //建立唯一識別碼
        newRecord.id = Guid.NewGuid().ToString();

        //將日期轉換為yyyy-MM-dd格式保存
        newRecord.date = selectedDate.ToString("yyyy-MM-dd");

        //保存輸入的金額
        newRecord.amount = amount;
        //保存分類
        newRecord.category = selectedCategory;
        //保存支出還是收入
        newRecord.recordType = currentRecoedType;

        //將新帳目加入帳目清單
        moneyRecords.Add(newRecord);

        //將清單儲存到本機
        SaveToFile();

        // 新帳目建立後，立即更新目前日期的帳目清單。
        RefreshDailyRecords();

        // 在 Console 顯示剛建立的帳目，方便確認資料是否正確。
        Debug.Log(
            $"新增帳目成功：" +
            $"日期={newRecord.date}，" +
            $"類型={newRecord.recordType}，" +
            $"分類={newRecord.category}，" +
            $"金額={newRecord.amount}");

        // 清空金額輸入欄位，方便輸入下一筆。
        amountInput.text = "";
    }

    // 提供其他腳本取得全部帳目。
    public List<MoneyRecord> GetAllRecords()
    {
        // 回傳目前保存的帳目清單。
        return moneyRecords;
    }
    // Start is called before the first frame update
    
    //更新目前選取日期的帳目顯示清單
    public void RefreshDailyRecords()
    {
        // 檢查日曆控制腳本是否已正確連接。
        if (calenderControllScript == null)
        {
            // 如果沒有連接，就顯示錯誤訊息。
            Debug.LogError("MoneyRecordManager 的 Calender Controll 尚未連接。");

            // 中止清單更新。
            return;
        }

        // 檢查單筆帳目 Prefab 是否已正確連接。
        if (recordItemPrefab == null)
        {
            // 如果沒有連接，就顯示錯誤訊息。
            Debug.LogError("MoneyRecordManager 的 Record Item Prefab 尚未連接。");

            // 中止清單更新。
            return;
        }

        // 檢查帳目顯示區域是否已正確連接。
        if (dailyRecordContent == null)
        {
            // 如果沒有連接，就顯示錯誤訊息。
            Debug.LogError("MoneyRecordManager 的 Daily Record Content 尚未連接。");

            // 中止清單更新。
            return;
        }

        //逐一取得content底下已顯示的帳目物件
        foreach(Transform child in dailyRecordContent)
        {
            //刪除舊物件，避免切換日期後累積
            Destroy(child.gameObject);
        }

        //從日曆控制器取得目前選擇日期
        DateTime selectedDate = calenderControllScript.GetSelectedDate();

        //轉換日期格式
        string selectedDateText = selectedDate.ToString("yyyy-MM-dd");

        //逐一檢查所有帳目項目
        for(int i = 0; i < moneyRecords.Count; i++)
        {
            //取得目前正在檢查的項目
            MoneyRecord record = moneyRecords[i];

            //判斷資料是否存在
            if(record == null) { continue; }

            //判斷目前這筆資料是否不是當前選取的日期
            if(record.date != selectedDateText) { continue; }

            //在DailyRecordContent下建立一個Prefab
            GameObject recordItemObject = Instantiate(recordItemPrefab/*要建立的物件*/, dailyRecordContent/*要建立的位置*/);

            //從新產生的物件取得moneyRecordItem腳本
            MoneyRecordItem recordItem = recordItemObject.GetComponent<MoneyRecordItem>();

            //判斷prefab上有沒有掛載這個腳本
            if(recordItem == null)
            {
                Debug.LogError("RecordItem Prefab上沒有MoneyRecordITem這個腳本。");

                Destroy(recordItemObject);

                continue;
            }

            //將帳目資料傳給recordItem顯示
            recordItem.SetUP(record,editRecordPanel);
        }
    }
}
