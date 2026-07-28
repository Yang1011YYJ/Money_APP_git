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
    //[Tooltip("分類的下拉清單")] public TMP_Dropdown categoryDropdown;
    //[Tooltip("顯示帳目即將要儲存的日期")]public 
    [Tooltip("整個新增帳目面板")] public  GameObject AddRecordPanel;

    [Header("腳本")]
    public CalenderControll calenderControllScript;
    public CategoryDropdownManager categoryDropdownManager;

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

                // 修補舊版本資料。
                bool repaired =
                    RepairOldRecordData();

                // 如果真的有修補到舊資料。
                if (repaired)
                {
                    // 將補完的新格式重新存回檔案。
                    SaveToFile();

                    // 顯示修補完成訊息。
                    Debug.Log(
                        "偵測到舊版本帳目資料，已自動補上缺少欄位。");
                }
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
    public void AddRecord(MoneyRecord newRecord)
    {
        // 檢查傳入的帳目是否存在。
        if (newRecord == null)
        {
            // 顯示錯誤訊息。
            Debug.LogError("新增帳目時沒有收到 MoneyRecord。");

            // 中止新增。
            return;
        }

        // 檢查帳目金額是否有效。
        if (newRecord.amount <= 0)
        {
            // 顯示錯誤訊息。
            Debug.LogWarning("帳目金額必須大於0。");

            // 中止新增。
            return;
        }

        // 帳目沒有唯一編號時，自動建立。
        if (string.IsNullOrWhiteSpace(newRecord.id))
        {
            // 建立唯一識別碼。
            newRecord.id = Guid.NewGuid().ToString();
        }

        // 將帳目加入資料清單。
        moneyRecords.Add(newRecord);

        // 儲存到本機。
        SaveToFile();

        // 更新目前日期的帳目清單。
        RefreshDailyRecords();
    }

    // 提供 Unity UI 的儲存按鈕直接呼叫。
    // 這個方法負責從目前的手動輸入介面建立一筆帳目。
    public void AddRecord()
    {
        // 檢查金額輸入欄位是否存在。
        if (amountInput == null)
        {
            // 顯示錯誤訊息。
            Debug.LogError(
                "MoneyRecordManager 的 Amount Input 尚未連接。");

            // 中止新增。
            return;
        }

        // 嘗試將使用者輸入的文字轉換成整數金額。
        bool amountSuccess =
            int.TryParse(
                amountInput.text,
                out int amount);

        // 判斷金額是否有效。
        if (!amountSuccess || amount <= 0)
        {
            // 顯示提示。
            Debug.LogWarning(
                "請輸入大於 0 的有效金額。");

            // 中止新增。
            return;
        }

        // 檢查日曆控制器是否存在。
        if (calenderControllScript == null)
        {
            // 顯示錯誤訊息。
            Debug.LogError(
                "MoneyRecordManager 的 Calender Controll 尚未連接。");

            // 中止新增。
            return;
        }

        // 檢查分類管理器是否存在。
        if (categoryDropdownManager == null)
        {
            // 顯示錯誤訊息。
            Debug.LogError(
                "MoneyRecordManager 的 Category Dropdown Manager 尚未連接。");

            // 中止新增。
            return;
        }

        // 取得目前日曆選取的日期。
        DateTime selectedDate =
            calenderControllScript.GetSelectedDate();

        // 建立新的帳目資料。
        MoneyRecord newRecord =
            new MoneyRecord();

        // 建立這筆資料的唯一編號。
        newRecord.id =
            Guid.NewGuid().ToString();

        // 儲存目前選取日期。
        newRecord.date =
            selectedDate.ToString("yyyy-MM-dd");

        // 儲存輸入金額。
        newRecord.amount =
            amount;

        // 取得並儲存目前選取的大分類。
        newRecord.category =
            categoryDropdownManager.GetCategory();

        // 取得並儲存目前選取的小分類。
        newRecord.subCategory =
            categoryDropdownManager.GetSubCategory();

        // 儲存目前選擇的收入或支出。
        newRecord.recordType =
            currentRecoedType;

        // 目前手動輸入沒有另外輸入品項，
        // 所以暫時把小分類當作品項。
        newRecord.itemName =
            newRecord.subCategory;

        // 目前手動介面還沒有付款方式，
        // 暫時預設為現金。
        newRecord.paymentMethod =
            "現金";

        // 目前手動介面沒有備註輸入，
        // 所以先使用空字串。
        newRecord.note =
            "";

        // 呼叫原本已有的 AddRecord(MoneyRecord)，
        // 讓它統一負責加入清單、存檔和刷新畫面。
        AddRecord(newRecord);

        // 新增完成後清空金額輸入框。
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

    // 修補舊版本資料，並回傳是否有發生修改。
    // 舊 JSON 沒有新欄位時，Newtonsoft 會將 string 設為 null。
    // 這裡統一補上預設值，避免 UI 顯示空白。
    private bool RepairOldRecordData()
    {
        // 預設沒有修改任何資料。
        bool hasChanged =
            false;

        // 逐一檢查所有帳目資料。
        for (int i = 0; i < moneyRecords.Count; i++)
        {
            // 取得目前正在檢查的帳目。
            MoneyRecord record =
                moneyRecords[i];

            // 如果這筆帳目本身是空的，就跳過。
            if (record == null)
            {
                // 繼續下一筆。
                continue;
            }

            // 如果大分類沒有資料，就設定為其他。
            if (string.IsNullOrWhiteSpace(record.category))
            {
                // 補上大分類預設值。
                record.category =
                    "其他";

                // 紀錄資料有被修改。
                hasChanged =
                    true;
            }

            // 如果小分類沒有資料，就設定為其他。
            if (string.IsNullOrWhiteSpace(record.subCategory))
            {
                // 補上小分類預設值。
                record.subCategory =
                    "其他";

                // 紀錄資料有被修改。
                hasChanged =
                    true;
            }

            // 如果品項沒有資料，就設定為其他。
            if (string.IsNullOrWhiteSpace(record.itemName))
            {
                // 補上品項預設值。
                record.itemName =
                    "其他";

                // 紀錄資料有被修改。
                hasChanged =
                    true;
            }

            // 如果付款方式沒有資料，就設定為現金*。
            if (string.IsNullOrWhiteSpace(record.paymentMethod))
            {
                // 補上付款方式預設值。
                record.paymentMethod =
                    "現金*";

                // 紀錄資料有被修改。
                hasChanged =
                    true;
            }

            // 如果備註沒有資料，就設定為空白。
            if (string.IsNullOrWhiteSpace(record.note))
            {
                // 補上備註預設值。
                record.note =
                    "";

                // 紀錄資料有被修改。
                hasChanged =
                    true;
            }
        }

        // 回傳這次是否修補過資料。
        return hasChanged;
    }

    //開啟新增帳目面板
    public  void OpenAddRecordPanel()
    {
        AddRecordPanel.SetActive(true);
    }

    //關閉新增帳目面板
    public  void CloseAddRecordPanel()
    {
        AddRecordPanel.SetActive(false);
    }
}
