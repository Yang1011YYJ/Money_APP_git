using System.Collections;
using System.Collections.Generic;
using TMPro;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;
using UnityEngine.EventSystems;
using System.IO;
using System.Xml;
using Newtonsoft.Json;

public class Random365 : MonoBehaviour
{
    public TextMeshProUGUI ResultText;//顯示要存多少錢
    public TextMeshProUGUI DaysText;//顯示已經存多少天
    public TextMeshProUGUI TotalText;//顯示已經存多少錢
    public TextMeshProUGUI ListText;//顯示已經出現過什麼數字
    public UnityEngine.UI.Button choosenumber;
    public int num,maxDays;
    public List<SavingEntry> Day = new List<SavingEntry>();
    private string savePath;

    [Tooltip("腳本")]
    public EventSystem eventSystem;
    public EditManager EditManagerScript;
    // Start is called before the first frame update
    void Awake()
    {
        savePath = Path.Combine(Application.persistentDataPath, "savings.json");
        LoadFromFile();
    }
    void Start()
    {
        num = 0;
        UpdateUI();
        Debug.Log("存檔位置：" + Application.persistentDataPath);
        EditManagerScript = eventSystem.GetComponent<EditManager>();
        //if () == 365)
        //{

        //}//已經存了一年了
    }

    //public void SaveMoney()
    //{
    //    //Day.Add(num);//加入數字
    //    //Day.Sort();//排列
    //    UpdateUI();
    //    SaveToFile();
    //}

    public void ChooseNumeber()
    {
        num = Random.Range(1, maxDays + 1);

        System.DateTime now = System.DateTime.Now;
        SavingEntry entry = new SavingEntry
        {
            year = now.Year,
            month = now.Month,
            day = now.Day,
            amount = num
        };

        Day.Add(entry);

        ResultText.text = $"今天存:{num}元";
        UpdateUI();
        SaveToFile();
    }

    public void UpdateUI()
    {
        ResultText.text = $"今天存:{num}元";
        DaysText.text = $"已存天數：{Day.Count} 天";
        TotalText.text = $"總金額：{Day.Sum(e => e.amount)} 元";
        // 這裡只是顯示簡單內容
        ListText.text = string.Join(", ", Day.Select(e => $"{e.year}/{e.month}/{e.day}:{e.amount}元"));


    }

    public void SaveToFile()
    {
        //SaveManager.Save(Day);
        string json = JsonConvert.SerializeObject(Day, Newtonsoft.Json.Formatting.Indented); // 美化輸出
        File.WriteAllText(savePath, json);
        Debug.Log("存檔完成: " + savePath);
    }
    public void DeleteData()
    {
        Day.Clear();
        // 2. 刪除儲存檔案
        string path = savePath;
        if (File.Exists(path))
        {
            File.Delete(path);
        }
        // 3. 清除 ScrollView 裡的項目
        foreach (Transform child in EditManagerScript.contentArea)  // 假設你有 contentArea 指向 ScrollView 裡的 Content
        {
            Destroy(child.gameObject);
        }
        // 4. 重設統計顯示
        num = 0;
        UpdateUI();
        
    }

    public void LoadFromFile()
    {
        if (File.Exists(savePath))
        {
            string json = File.ReadAllText(savePath);
            Day = JsonConvert.DeserializeObject<List<SavingEntry>>(json);
            Debug.Log("讀取存檔完成: " + Day.Count + " 筆資料");
        }
        else
        {
            Day = new List<SavingEntry>();
            Debug.Log("沒有找到存檔，建立新資料");
        }
    }
}
