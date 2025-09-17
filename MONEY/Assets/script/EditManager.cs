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
            // ����ó]�w Text
            row.transform.Find("YearText").GetComponent<TextMeshProUGUI>().text = number.year.ToString();
            row.transform.Find("MonthText").GetComponent<TextMeshProUGUI>().text = number.month.ToString();
            row.transform.Find("DayText").GetComponent<TextMeshProUGUI>().text = number.day.ToString();
            row.transform.Find("AmountText").GetComponent<TextMeshProUGUI>().text = number.amount.ToString() + " ��";

            // �]�w�R�����s
            Button deleteBtn = row.transform.Find("DeleteButton").GetComponent<Button>();
            deleteBtn.onClick.AddListener(() => {
                tempList.Remove(number);
                RefreshUI();
            });
        }
    }

    void AddNumber()
    {
        // ���ոѪR��J���~�����B
        bool valid =
            int.TryParse(yearInput.text, out year) &&
            int.TryParse(monthInput.text, out month) &&
            int.TryParse(dayInput.text, out day) &&
            int.TryParse(amountInput.text, out amount);

        if (!valid)
        {
            hintPanel.SetActive(true);
            hintText.text = "��J�榡���~�I";
            return;
        }

        // �򥻤������
        if (month < 1 || month > 12 || day < 1 || day > 31 || amount <= 0 || year>System.DateTime.Now.Year)
        {
            hintPanel.SetActive(true);
            hintText.text = "��J������Ϊ��B���X�z�I";
            return;
        }
        // �إ߷s���x�W���
        SavingEntry newEntry = new SavingEntry
        {
            year = year,
            month = month,
            day = day,
            amount = amount
        };

        tempList.Add(newEntry);

        // �M�����
        yearInput.text = "";
        monthInput.text = "";
        dayInput.text = "";
        amountInput.text = "";

        RefreshUI();
    }

    void ConfirmChanges()
    {
        random365.Day = new List<SavingEntry>(tempList);
        random365.SaveToFile(); // �T�O���o�Ӥ�k
        random365.UpdateUI();   // �T�O���o�Ӥ�k
        editPanel.SetActive(false);
    }

    void CancelChanges()
    {
        editPanel.SetActive(false);
    }
}
