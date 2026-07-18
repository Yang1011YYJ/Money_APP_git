using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CalenderDayCell : MonoBehaviour
{
    [Header("UI")]
    public TextMeshProUGUI dateText;//顯示日期數字的文字
    public UnityEngine.UI.Button DateButton;//日期按鈕
    [Tooltip("當日總收入")]public TextMeshProUGUI incomeText;
    [Tooltip("當日總支出")]public TextMeshProUGUI expenseText;
    [Tooltip("顯示選擇的區塊")] public GameObject SelectPanel;
    //// 讓 Unity Inspector 可以指定日期格背景或框框的 Image 元件。
    //[Tooltip("日期格顯示顏色的 Image")]public Image DateImage;

    [Header("特殊日期")]
    //[Tooltip("一般日期顏色")]public Color normalColor = new Color(1,1,1,0);// 一般日期使用的顏色。
    //[Tooltip("國定假日顏色")]public Color holidayColor = new Color(1, 0.645283f, 0.645283f, 0.4313726f);// 國定假日使用的顏色。
    //[Tooltip("選取日期顏色")]public Color selectedColor = new Color(0.6470588f, 0.6470588f,1, 0.4313726f);// 被選取時使用的顏色。
    [Tooltip("國定假日顯示")] public GameObject HolidayMark; 
    [SerializeField]public bool isHoliday;// 記錄這個日期格是否為國定假日。

    [SerializeField] DateTime date;//時間
    [SerializeField] CalenderControll calenderControll;//控制日曆的腳本

    public void Setup(DateTime targetDate/*這一格代表的日期*/, CalenderControll controll/*抓CalenderControll腳本進來*/, bool isCurrentMonth/*這一格的顯示亮框*/)//設定日期格的內容
    {
        date = targetDate;//這一格的日期
        calenderControll = controll;//日曆控制腳本

        dateText.text = targetDate.Day.ToString();//取得日

        // 暫時清空收入文字，之後再接入真實記帳資料。
        incomeText.text = "";

        // 暫時清空支出文字，之後再接入真實記帳資料。
        expenseText.text = "";

        // 每次重新設定日期格時，先清除假日狀態。
        isHoliday = false;

        // 檢查假日提示物件是否存在。
        if (HolidayMark != null)
        {
            // 預設先隱藏假日提示，稍後由 SetHoliday 重新判斷。
            HolidayMark.SetActive(false);
        }

        // 檢查選取外框是否存在。
        if (SelectPanel != null)
        {
            // 關閉上一個月份留下的選取外框。
            SelectPanel.SetActive(false);
        }

        // 啟用這個日期格的按鈕，讓使用者可以點擊。
        DateButton.interactable = true;

        DateButton.onClick.RemoveAllListeners();
        DateButton.onClick.AddListener(Onclick);

        gameObject.SetActive(isCurrentMonth);//顯示日期
        
    }

    // 將這個日期格設定成空白格。
    public void ShowEmpty()
    {
        // 清除日期數字。
        dateText.text = "";

        // 清除收入文字。
        incomeText.text = "";

        // 清除支出文字。
        expenseText.text = "";

        //清除顯示框
        SelectPanel.SetActive(false);

        // 將空白日期格設定為非假日。
        isHoliday = false;

        // 檢查國定假日提示物件是否存在。
        if (HolidayMark != null)
        {
            // 空白格不顯示國定假日提示。
            HolidayMark.SetActive(false);
        }

        // 檢查選取外框是否存在。
        if (SelectPanel != null)
        {
            // 空白格不顯示選取外框。
            SelectPanel.SetActive(false);
        }

        // 停用按鈕互動，避免使用者點到空白日期。
        DateButton.interactable = false;

        // 移除這個日期格之前加入的所有點擊事件。
        DateButton.onClick.RemoveAllListeners();

        // 清除日曆控制器參考，避免保留上一個月份的資料。
        calenderControll = null;
    }

    // 控制這個日期格是否被選擇，亮框要開啟還是關閉。
    public void SetSelected(bool isSelected)
    {
        // 檢查選取外框是否已在 Inspector 中正確連接。
        if (SelectPanel == null)
        {
            // 如果沒有連接，就在 Console 顯示錯誤訊息。
            Debug.LogError("CalenderDayCell 的 SelectPanel 尚未連接。");

            // 中止這次選取狀態更新。
            return;
        }

        // 被選取時開啟藍色外框，取消選取時關閉外框。
        SelectPanel.SetActive(isSelected);
    }
    
    //國定假日判斷
    public void SetHoliday(bool holiday)
    {
        // 保存目前日期格是否為國定假日。
        isHoliday = holiday;

        // 檢查國定假日提示物件是否已在 Inspector 中正確連接。
        if (HolidayMark == null)
        {
            // 如果沒有連接，就在 Console 顯示錯誤訊息。
            Debug.LogError("CalenderDayCell 的 HolidayMark 尚未連接。");

            // 中止這次顯示更新。
            return;
        }

        // 國定假日時顯示紅色提示，一般日期則隱藏。
        HolidayMark.SetActive(isHoliday);
    } 
    public void Onclick()
    {
        // 檢查日曆控制腳本是否存在，避免發生空參考錯誤。
        if (calenderControll == null)
        {
            // 如果沒有取得日曆控制腳本，就在 Console 顯示錯誤訊息。
            Debug.LogError("CalenderDayCell 找不到 CalenderControll。");

            // 中止這次點擊處理。
            return;
        }

        // 呼叫 CalendarController 的 SelectDate 方法，傳入這格所代表的日期。
        calenderControll.SelectDate(this);
    }

    // 將這個日期格所代表的完整日期回傳出去。
    public DateTime GetDate()
    {
        // 回傳目前日期格保存的日期。
        return date;
    }
}
