// 引入泛型集合，讓程式可以使用 Dictionary 和 List。
using System.Collections.Generic;

// 引入 TextMeshPro UI 功能。
using TMPro;

// 引入 Unity 基本功能。
using UnityEngine;

// 管理大分類與小分類 Dropdown 的連動。
public class CategoryDropdownManager : MonoBehaviour
{
    // 在 Inspector 中顯示 UI 分類標題。
    [Header("分類 UI")]

    // 大分類下拉式選單，例如餐飲、交通、購物。
    public TMP_Dropdown categoryDropdown;

    // 小分類下拉式選單，例如早餐、午餐、晚餐。
    public TMP_Dropdown subCategoryDropdown;

    // 儲存每個大分類對應的小分類。
    private Dictionary<string, List<string>> categoryData;

    // 物件初始化時執行。
    private void Awake()
    {
        // 建立分類資料。
        InitializeCategoryData();
    }

    // 場景開始時執行。
    private void Start()
    {
        // 檢查兩個 Dropdown 是否都有正確連接。
        if (
            categoryDropdown == null ||
            subCategoryDropdown == null)
        {
            // 顯示錯誤訊息。
            Debug.LogError(
                "CategoryDropdownManager 的分類 Dropdown 尚未完整連接。");

            // 中止初始化。
            return;
        }

        // 當大分類被改變時，呼叫 OnCategoryChanged。
        categoryDropdown.onValueChanged.AddListener(
            OnCategoryChanged);

        // 根據目前的大分類，初始化小分類內容。
        OnCategoryChanged(
            categoryDropdown.value);
    }

    // 建立大分類與小分類的對應資料。
    private void InitializeCategoryData()
    {
        // 建立分類 Dictionary。
        categoryData =
            new Dictionary<string, List<string>>();

        // 建立餐飲的小分類。
        categoryData.Add(
            "餐飲",
            new List<string>
            {
                "早餐",
                "午餐",
                "晚餐",
                "宵夜",
                "甜點",
                "飲料",
                "其他"
            });

        // 建立交通的小分類。
        categoryData.Add(
            "交通",
            new List<string>
            {
                "捷運",
                "公車",
                "火車",
                "高鐵",
                "計程車",
                "加油",
                "停車",
                "其他"
            });

        // 建立購物的小分類。
        categoryData.Add(
            "購物",
            new List<string>
            {
                "服飾",
                "生活用品",
                "美妝",
                "3C",
                "網購",
                "其他"
            });

        // 建立娛樂的小分類。
        categoryData.Add(
            "娛樂",
            new List<string>
            {
                "電影",
                "遊戲",
                "展覽",
                "唱歌",
                "旅遊",
                "其他"
            });
    }

    // 大分類 Dropdown 改變時執行。
    private void OnCategoryChanged(int categoryIndex)
    {
        // 檢查大分類目前是否有選項。
        if (categoryDropdown.options.Count == 0)
        {
            // 沒有選項時不繼續處理。
            return;
        }

        // 取得目前選取的大分類名稱。
        string selectedCategory =
            categoryDropdown
                .options[categoryIndex]
                .text;

        // 清除小分類原本的所有選項。
        subCategoryDropdown.ClearOptions();

        // 嘗試取得這個大分類對應的小分類。
        if (
            categoryData.TryGetValue(
                selectedCategory,
                out List<string> subCategories))
        {
            // 將對應的小分類加入 Dropdown。
            subCategoryDropdown.AddOptions(
                subCategories);
        }
        else
        {
            // 如果這個大分類沒有設定小分類，
            // 就至少提供一個「其他」。
            subCategoryDropdown.AddOptions(
                new List<string>
                {
                    "其他"
                });
        }

        // 預設選擇第一個小分類。
        subCategoryDropdown.value = 0;

        // 立即更新 Dropdown 顯示的文字。
        subCategoryDropdown.RefreshShownValue();
    }

    // 提供其他腳本取得目前選取的大分類。
    public string GetCategory()
    {
        // 如果大分類沒有任何選項，就回傳空字串。
        if (categoryDropdown.options.Count == 0)
        {
            // 回傳空字串。
            return "";
        }

        // 回傳目前選取的大分類名稱。
        return categoryDropdown
            .options[categoryDropdown.value]
            .text;
    }

    // 提供其他腳本取得目前選取的小分類。
    public string GetSubCategory()
    {
        // 如果小分類沒有任何選項，就回傳空字串。
        if (subCategoryDropdown.options.Count == 0)
        {
            // 回傳空字串。
            return "";
        }

        // 回傳目前選取的小分類名稱。
        return subCategoryDropdown
            .options[subCategoryDropdown.value]
            .text;
    }
}