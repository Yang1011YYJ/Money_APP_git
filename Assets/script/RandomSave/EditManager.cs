using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using static UnityEngine.EventSystems.EventTrigger;

public class EditManager : MonoBehaviour
{
    public GameObject editPanel;
    public Transform contentArea;
    public GameObject rowPrefab;

    public Button addButton;
    public Button confirmButton;
    public Button cancelButton;

    private List<SavingEntry> tempList = new List<SavingEntry>();
    public TMP_InputField yearInput;
    public TMP_InputField monthInput;
    public TMP_InputField dayInput;
    public TMP_InputField amountInput;
    int year, month, day, amount;

    private Random365 random365;
    public GameObject hintPanel;
    public TextMeshProUGUI hintText;

    void Start()
    {
        random365 = FindObjectOfType<Random365>();

        addButton.onClick.AddListener(AddNumber);
        confirmButton.onClick.AddListener(ConfirmChanges);
        cancelButton.onClick.AddListener(CancelChanges);
    }

    public void OpenEditor()
    {
        tempList = new List<SavingEntry>(random365.Day); // copy list
        RefreshUI();
        editPanel.SetActive(true);
    }

    void RefreshUI()
    {
        foreach (Transform child in contentArea)
            Destroy(child.gameObject);

        foreach (SavingEntry number in tempList
            .OrderBy(n => n.year)
            .ThenBy(n => n.month)
            .ThenBy(n => n.day))
        {
            GameObject row = Instantiate(rowPrefab, contentArea);
            // 抓取並設定 Text
            row.transform.Find("YearText").GetComponent<TextMeshProUGUI>().text = number.year.ToString();
            row.transform.Find("MonthText").GetComponent<TextMeshProUGUI>().text = number.month.ToString();
            row.transform.Find("DayText").GetComponent<TextMeshProUGUI>().text = number.day.ToString();
            row.transform.Find("AmountText").GetComponent<TextMeshProUGUI>().text = number.amount.ToString() + " 元";

            // 設定刪除按鈕
            Button deleteBtn = row.transform.Find("DeleteButton").GetComponent<Button>();
            deleteBtn.onClick.AddListener(() => {
                tempList.Remove(number);
                RefreshUI();
            });
        }
    }

    void AddNumber()
    {
        // 嘗試解析輸入的年月日金額
        bool valid =
            int.TryParse(yearInput.text, out year) &&
            int.TryParse(monthInput.text, out month) &&
            int.TryParse(dayInput.text, out day) &&
            int.TryParse(amountInput.text, out amount);

        if (!valid)
        {
            hintPanel.SetActive(true);
            hintText.text = "輸入格式錯誤！";
            return;
        }

        // 基本日期驗證
        if (month < 1 || month > 12 || day < 1 || day > 31 || amount <= 0 || year>System.DateTime.Now.Year)
        {
            hintPanel.SetActive(true);
            hintText.text = "輸入的日期或金額不合理！";
            return;
        }
        // 建立新的儲蓄資料
        SavingEntry newEntry = new SavingEntry
        {
            year = year,
            month = month,
            day = day,
            amount = amount
        };

        tempList.Add(newEntry);

        // 清空欄位
        yearInput.text = "";
        monthInput.text = "";
        dayInput.text = "";
        amountInput.text = "";

        RefreshUI();
    }

    void ConfirmChanges()
    {
        random365.Day = new List<SavingEntry>(tempList);
        random365.SaveToFile(); // 確保有這個方法
        random365.UpdateUI();   // 確保有這個方法
        editPanel.SetActive(false);
    }

    void CancelChanges()
    {
        editPanel.SetActive(false);
    }
}
