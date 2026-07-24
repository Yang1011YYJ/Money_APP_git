using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class CalenderControll : MonoBehaviour
{
    [Header("UI")]
    [Tooltip("月份標題")] public TextMeshProUGUI yearMonthText;
    [Tooltip("日期格")] public List<CalenderDayCell> daycells;
    [Tooltip("選取日期")] public TextMeshProUGUI selectedDateText;

    [Header("腳本")]
    // 儲存目前被選取的日期格。
    [SerializeField] private CalenderDayCell selectedDayCell;
    [Tooltip("假日資料管理器")] public HolidayManager holidayManager;
    [Tooltip("記帳資料管理器")] public MoneyRecordManager moneyRecordManager;

    [Header("選取日期")]
    // 儲存目前被使用者選取的完整日期。
    [SerializeField]private DateTime selectedDate;

    [Header("目前時間")]
    [SerializeField] int currentYear;
    [SerializeField] int currentMonth;

    // Start is called before the first frame update
    void Start()
    {
        DateTime today = DateTime.Today;//抓取今天日期

        currentYear = today.Year;//今天年分
        currentMonth = today.Month;//今天月份

        // 判斷 HolidayManager 是否已經在 Inspector 中連接。
        if (holidayManager != null)
        {
            // 啟動下載假日資料的 Coroutine。
            StartCoroutine(
                holidayManager.LoadHolidayData(
                    // API 下載完成後執行 RefreshCalender。
                    RefreshCalender));
        }
        else
        {
            // 如果沒有 HolidayManager，仍然先顯示一般日曆。
            RefreshCalender();

            // 在 Console 顯示提醒。
            Debug.LogWarning(
                "CalenderControll 尚未連接 HolidayManager，因此不會顯示假日。");
        }
    }

    public void PreviewsMonth()//往上一個月
    {
        currentMonth--;//月份減一

        if (currentMonth < 1)//如果月份已經到0
        {
            currentMonth = 12;//設定為12
            currentYear--;//年份減一
        }

        RefreshCalender();
    }

    public void NextMonth()//往下一個月
    {
        currentMonth++;//同上

        if (currentMonth > 12)
        {
            currentMonth = 1;
            currentYear++;
        }
        RefreshCalender();
    }

    // 根據 currentYear 和 currentMonth 更新整個日曆畫面。
    public void RefreshCalender()
    {
        // 判斷目前是否有被選取的日期格。
        if (selectedDayCell != null)
        {
            // 關閉目前日期格的亮框。
            selectedDayCell.SetSelected(false);

            // 清除目前選取日期格的紀錄。
            selectedDayCell = null;
        }

        // 檢查年月標題文字是否有在 Inspector 中正確連接。
        if (yearMonthText == null)
        {
            // 如果沒有連接，就在 Console 顯示錯誤訊息。
            Debug.LogError("CalendarController 的 Year Month Text 尚未連接。");

            // 中止日曆更新，避免產生空參考錯誤。
            return;
        }
        // 檢查日期格清單是否存在。
        if (daycells == null)
        {
            // 如果清單不存在，就在 Console 顯示錯誤訊息。
            Debug.LogError("CalendarController 的 Day Cells 清單不存在。");

            // 中止日曆更新。
            return;
        }

        // 檢查日期格是否至少有42個。
        if (daycells.Count < 42)
        {
            // 如果少於42個，就在 Console 顯示警告。
            Debug.LogWarning(
                $"CalendarController 目前只有 {daycells.Count} 個日期格，建議設定為42個。");
        }


        yearMonthText.text = $"{currentYear} 年 {currentMonth} 月";//更新顯示的年月份

        // 建立目前顯示月份第一天的日期資料。
        DateTime firstDayOfMonth = new DateTime(currentYear, currentMonth, 1/*從一開始*/);

        // 取得當月第一天是星期幾。
        // 星期日為0、星期一為1，一直到星期六為6。
        int firstDayIndex = (int)firstDayOfMonth.DayOfWeek;
        // 取得目前月份總共有幾天，例如7月有31天。
        int daysInMonth = DateTime.DaysInMonth(currentYear, currentMonth);

        for (int i = 0; i < daycells.Count; i++)
        {
            // 判斷目前日期格是否沒有正確連接。
            if (daycells[i] == null)
            {
                // 如果這一格是空參考，跳過這一格。
                continue;
            }

            // 根據日期格位置和當月第一天的星期，計算這格應該顯示幾號。
            int dayNumber = i - firstDayIndex + 1;

            // 判斷計算出的日期是否在當月有效範圍內。
            if (dayNumber >= 1 && dayNumber <= daysInMonth)
            {
                // 用目前年份、月份和計算出的日期建立完整 DateTime。
                DateTime date = new DateTime(currentYear, currentMonth, dayNumber);


                daycells[i].gameObject.SetActive(true);// 顯示這個日期格。
                daycells[i].Setup(date, this, true); // 把正確日期和 CalendarController 傳給日期格。

                // 預設目前日期不是假日。
                bool isHoliday = false;

                // 判斷 HolidayManager 是否存在，而且資料已經載入完成。
                if (holidayManager != null && holidayManager.IsLoaded)
                {
                    // 向 HolidayManager 查詢目前日期是否為放假日。
                    isHoliday =
                        holidayManager.IsHoliday(date);
                }

                // 將假日判斷結果傳給日期格。
                daycells[i].SetHoliday(isHoliday);
            }
            else
            { // 如果這格本月不會用到，就先將整個日期格隱藏。
                // 確保空白日期格物件仍保持啟用，占住原本的日曆位置。
                daycells[i].gameObject.SetActive(true);

                // 清空這一格的內容並停用點擊功能。
                daycells[i].ShowEmpty();
            }
        }

        // 日期格全部更新完成後，自動選取今天或該月1號。
        SelectDefaultDate();
    }

    public void SelectDate(CalenderDayCell clickedDayCell)// 當使用者點擊某個日期格時，由 CalendarDayCell 呼叫這個方法。
    {
        // 檢查顯示選取日期的文字元件是否已正確連接。
        if (selectedDateText == null)
        {
            // 如果沒有連接，就在 Console 顯示錯誤訊息。
            Debug.LogError("CalendarController 的 Selected Date Text 尚未連接。");

            // 中止方法，避免空參考錯誤。
            return;
        }
        // 判斷目前是否已經有一個被選取的日期格。
        if (selectedDayCell != null)
        {
            // 判斷這次按下的日期格是否和上一次不同。
            if (selectedDayCell != clickedDayCell)
            {
                // 關閉上一個日期格的亮框。
                selectedDayCell.SetSelected(false);
            }
        }

        // 將這次按下的日期格記錄為目前選取的日期格。
        selectedDayCell = clickedDayCell;

        // 開啟目前選取日期格的亮框。
        selectedDayCell.SetSelected(true);

        // 從目前選取的日期格取得完整日期，並保存到控制器欄位中。
        selectedDate = selectedDayCell.GetDate();

        // 將選取日期顯示。
        selectedDateText.text = $"{selectedDate.Year} 年 {selectedDate.Month} 月 {selectedDate.Day} 日";

        // 判斷記帳資料管理器是否已經正確連接。
        if (moneyRecordManager != null)
        {
            // 根據剛選取的日期重新顯示當天帳目。
            moneyRecordManager.RefreshDailyRecords();
        }
    }

    //根據顯示的月份決定要亮特定日期還是1號
    public void SelectDefaultDate()
    {
        //取得裝置的日期
        DateTime today = DateTime.Today;

        //自動選許的日期的參數
        int autoSelectDay;

        //判斷目前的年月是不是等於今天的年月
        if (currentYear == today.Year && currentMonth == today.Month)
        {
            autoSelectDay = today.Day;
        }
        else//不是今天的同月份
        {
            autoSelectDay = 1;
        }

        for (int i = 0; i < daycells.Count; i++)
        {
            // 判斷目前這一格是否沒有連接日期格物件。
            if (daycells[i] == null)
            {
                // 如果是空參考，就跳過這一格。
                continue;
            }

            //取得目前日期格代表的完整日期
            DateTime cellDate = daycells[i].GetDate();

            // 判斷日期格的年月日是否等於準備選取的日期。
            if (cellDate.Year == currentYear &&
                cellDate.Month == currentMonth &&
                cellDate.Day == autoSelectDay)
            {
                // 將找到的日期格設定為目前選取日期。
                SelectDate(daycells[i]);

                // 已經找到目標日期，不需要繼續尋找。
                return;
            }
        }
    }

    // 提供其他腳本取得目前選取的日期。
    public DateTime GetSelectedDate()
    {
        // 回傳目前保存的選取日期。
        return selectedDate;
    }
}
