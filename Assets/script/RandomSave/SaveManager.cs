using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class SaveManager : MonoBehaviour
{
    private static string filePath = Application.persistentDataPath + "/save.json";

    public static void Save(List<SavingEntry> data)
    {
        SaveData saveData = new SaveData();
        saveData.savings = data;

        string json = JsonUtility.ToJson(saveData, true);
        File.WriteAllText(filePath, json);
    }

    public static List<SavingEntry> Load()
    {
        if (!File.Exists(filePath))
        {
            Debug.Log("沒有儲存檔，回傳空清單");
            return new List<SavingEntry>();
        }

        string json = File.ReadAllText(filePath);
        SaveData saveData = JsonUtility.FromJson<SaveData>(json);
        return saveData.savings ?? new List<SavingEntry>();
    }

    public static void DeleteSave()
    {
        if (File.Exists(filePath))
        {
            File.Delete(filePath);
        }
    }
}
