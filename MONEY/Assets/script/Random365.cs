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
    public TextMeshProUGUI ResultText;//��ܭn�s�h�ֿ�
    public TextMeshProUGUI DaysText;//��ܤw�g�s�h�֤�
    public TextMeshProUGUI TotalText;//��ܤw�g�s�h�ֿ�
    public TextMeshProUGUI ListText;//��ܤw�g�X�{�L����Ʀr
    public UnityEngine.UI.Button choosenumber;
    public int num,maxDays;
    public List<SavingEntry> Day = new List<SavingEntry>();
    private string savePath;

    [Tooltip("�}��")]
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
        Debug.Log("�s�ɦ�m�G" + Application.persistentDataPath);
        EditManagerScript = eventSystem.GetComponent<EditManager>();
        //if () == 365)
        //{

        //}//�w�g�s�F�@�~�F
    }

    //public void SaveMoney()
    //{
    //    //Day.Add(num);//�[�J�Ʀr
    //    //Day.Sort();//�ƦC
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

        ResultText.text = $"���Ѧs:{num}��";
        UpdateUI();
        SaveToFile();
    }

    public void UpdateUI()
    {
        ResultText.text = $"���Ѧs:{num}��";
        DaysText.text = $"�w�s�ѼơG{Day.Count} ��";
        TotalText.text = $"�`���B�G{Day.Sum(e => e.amount)} ��";
        // �o�̥u�O���²�椺�e
        ListText.text = string.Join(", ", Day.Select(e => $"{e.year}/{e.month}/{e.day}:{e.amount}��"));


    }

    public void SaveToFile()
    {
        //SaveManager.Save(Day);
        string json = JsonConvert.SerializeObject(Day, Newtonsoft.Json.Formatting.Indented); // ���ƿ�X
        File.WriteAllText(savePath, json);
        Debug.Log("�s�ɧ���: " + savePath);
    }
    public void DeleteData()
    {
        Day.Clear();
        // 2. �R���x�s�ɮ�
        string path = savePath;
        if (File.Exists(path))
        {
            File.Delete(path);
        }
        // 3. �M�� ScrollView �̪�����
        foreach (Transform child in EditManagerScript.contentArea)  // ���]�A�� contentArea ���V ScrollView �̪� Content
        {
            Destroy(child.gameObject);
        }
        // 4. ���]�έp���
        num = 0;
        UpdateUI();
        
    }

    public void LoadFromFile()
    {
        if (File.Exists(savePath))
        {
            string json = File.ReadAllText(savePath);
            Day = JsonConvert.DeserializeObject<List<SavingEntry>>(json);
            Debug.Log("Ū���s�ɧ���: " + Day.Count + " �����");
        }
        else
        {
            Day = new List<SavingEntry>();
            Debug.Log("�S�����s�ɡA�إ߷s���");
        }
    }
}
