using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
// 引入 Unity 的網路功能，讓程式可以透過 UnityWebRequest 呼叫 API。
using UnityEngine.Networking;
using Newtonsoft.Json;

[Serializable]public class HolidayRecord
{
    //儲存日期
    [Tooltip("儲存日期")]public string date;
    [Tooltip("儲存日期的年分")] public string year;
    [Tooltip("儲存日期名稱")] public string name;
    [Tooltip("儲存是否放假")] public string isHoliday;
    [Tooltip("假期種類")] public string holidaycategory;
    [Tooltip("官方資料的備註")] public string description;
}

public class HolidayManager : MonoBehaviour
{
    // 在 Inspector 中建立「API設定」標題。
    [Header("API設定")]
    [Tooltip("儲存官方假日 API 網址")] public string apiurl = "https://data.ntpc.gov.tw/api/datasets/308dcd75-6434-45bc-a95f-584da4fed251/json?page=0&size=2000";
    [Tooltip("儲存所有日期資料")]public Dictionary<string,HolidayRecord> holidayRecords = new Dictionary<string, HolidayRecord> ();
    [Tooltip("紀錄是否下載完成")]public bool IsLoaded {  get; private set; }

    //從官方下載資料
    public System.Collections.IEnumerator LoadHolidayData(Action onCompleted = null)
    {
        IsLoaded = false;//未完成下載

        //建立網路請求
        using (UnityWebRequest request = UnityWebRequest.Get(apiurl))
        {
            //傳送網路請求，等待回應
            yield return request.SendWebRequest();

            //判斷請求是否失敗
            if(request.result != UnityWebRequest.Result.Success)
            {
                //除錯區顯示原因
                Debug.LogError($"假日資料下載失敗：{request.error}");

                //中止coroutine
                yield break;
            }

            //如果成功
            //取得API回傳的JSON
            string originalJSON = request.downloadHandler.text;

            // 移除文字開頭與結尾的一般空白字元。
            originalJSON = originalJSON.Trim();

            // 移除 JSON 開頭可能存在的 UTF-8 BOM 隱藏字元。
            originalJSON = originalJSON.TrimStart('\uFEFF');

            // 將 API 回傳內容的前200個字印到 Console，方便確認實際格式。
            Debug.Log(
                "假日 API 回傳內容：" +
                originalJSON.Substring(
                    0,
                    Mathf.Min(200, originalJSON.Length)));

            // 判斷 API 回傳內容是否為空字串。
            if (string.IsNullOrWhiteSpace(originalJSON))
            {
                // 如果沒有取得任何內容，就顯示錯誤訊息。
                Debug.LogError("假日 API 沒有回傳任何內容。");

                // 中止目前的 Coroutine。
                yield break;
            }

            // 判斷 API 回傳內容是否以左中括號開頭，代表最外層是 JSON 陣列。
            if (!originalJSON.StartsWith("["))
            {
                // 如果不是 JSON 陣列，就將實際內容顯示在 Console。
                Debug.LogError(
                    "假日 API 回傳格式不是預期的 JSON 陣列：" +
                    originalJSON);

                // 中止目前的 Coroutine。
                yield break;
            }

            // 宣告一個清單，用來保存 API 解析後的所有日期資料。
            List<HolidayRecord> recordList;

            // 使用 try 包住 JSON 解析，避免格式異常時整個 Coroutine 中斷。
            try
            {
                // 直接將 API 最外層 JSON 陣列轉換成 HolidayRecord 清單。
                recordList =
                    JsonConvert.DeserializeObject<List<HolidayRecord>>(originalJSON);
            }
            catch (Exception exception)
            {
                // 如果 JSON 解析失敗，就在 Console 顯示錯誤原因。
                Debug.LogError(
                    $"假日 JSON 解析失敗：{exception.Message}");

                // 中止目前的 Coroutine。
                yield break;
            }

            // 判斷解析後的假日資料清單是否存在。
            if (recordList == null)
            {
                // 如果清單不存在，就顯示錯誤訊息。
                Debug.LogError("假日 API 解析結果為空。");

                // 中止目前的 Coroutine。
                yield break;
            }

            // 清除先前已經保存的日期資料。
            holidayRecords.Clear();

            // 逐一處理 API 回傳的每一筆日期資料。
            for (int i = 0; i < recordList.Count; i++)
            {
                // 取得目前這一筆日期資料。
                HolidayRecord record = recordList[i];

                // 判斷目前資料是否為空。
                if (record == null)
                {
                    // 如果是空資料，就跳過這一筆。
                    continue;
                }

                // 嘗試將 API 回傳的日期文字轉換成 DateTime。
                if (!TryParseAPIDate(record.date, out DateTime parsedDate))
                {
                    // 如果日期無法解析，就跳過這一筆。
                    continue;
                }

                // 將日期統一轉換成 yyyy-MM-dd 格式，作為 Dictionary 的索引。
                string dateKey =
                    parsedDate.ToString("yyyy-MM-dd");

                // 將這筆日期資料保存到 Dictionary 中。
                holidayRecords[dateKey] = record;
            }

            // 將資料載入狀態設定為完成。
            IsLoaded = true;

            // 在 Console 顯示實際成功載入的資料筆數。
            Debug.Log(
                $"假日資料下載完成，共載入 {holidayRecords.Count} 筆日期資料。");

            // 執行資料載入完成後要呼叫的方法，例如更新日曆。
            onCompleted?.Invoke();
        }
    }

    //將 API 的日期文字轉換成 DateTime
    public bool TryParseAPIDate(/*接收回傳的日期*/string dateText,/*傳回成功的日期*/out DateTime parseDate)
    {
        //將輸出的日期設為預設值
        parseDate = default;

        //判斷日期是否為空
        if (string.IsNullOrWhiteSpace(dateText))
        {
            // 日期為空時回傳 false。
            return false;
        }

        //移除日期前後的空白
        string cleanDate = dateText.Trim();

        // 嘗試直接解析例如 2026-07-16 或 2026/07/16 的格式。
        if (DateTime.TryParse(cleanDate, out parseDate))
        {
            // 解析成功時回傳 true。
            return true;
        }

        // 判斷日期是否為八位數，例如 20260716。
        if (cleanDate.Length == 8)
        {
            // 嘗試取得前四碼年份。
            bool yearSuccess =
                int.TryParse(cleanDate.Substring(0, 4), out int year);

            // 嘗試取得中間兩碼月份。
            bool monthSuccess =
                int.TryParse(cleanDate.Substring(4, 2), out int month);

            // 嘗試取得最後兩碼日期。
            bool daySuccess =
                int.TryParse(cleanDate.Substring(6, 2), out int day);

            // 判斷年月日是否都成功轉換。
            if (yearSuccess && monthSuccess && daySuccess)
            {
                // 嘗試建立完整日期，避免錯誤年月日造成例外。
                try
                {
                    // 使用解析出的年月日建立 DateTime。
                    parseDate =
                        new DateTime(year, month, day);

                    // 建立成功時回傳 true。
                    return true;
                }
                catch
                {
                    // 日期內容不合法時不做處理。
                }
            }
        }

        // 所有格式都無法解析時回傳 false。
        return false;
    }
    // 判斷指定日期是否為官方標示的放假日。
    public bool IsHoliday(DateTime date)
    {
        // 將要查詢的日期轉換成 Dictionary 使用的格式。
        string dateKey =
            date.ToString("yyyy-MM-dd");

        // 嘗試取得這一天的官方資料。
        if (!holidayRecords.TryGetValue(
            dateKey,
            out HolidayRecord record))
        {
            // 找不到資料時，先視為非假日。
            return false;
        }

        // 取得 API 回傳的放假文字，並移除前後空白。
        string holidayValue = record.isHoliday?.Trim();

        // 取得 API 回傳的假日類別，並移除前後空白。
        string categoryValue = record.holidaycategory?.Trim();

        // 判斷官方資料是否標示為放假。
        bool isDayOff = holidayValue == "是";

        // 判斷假日類別是否為放假之紀念日及節日。
        bool isNationalHoliday =
            categoryValue == "放假之紀念日及節日";

        // 必須同時是放假日與國定紀念日／節日，才回傳 true。
        return isDayOff && isNationalHoliday;
    }

    // 取得指定日期的節日名稱。
    public string GetHolidayName(DateTime date)
    {
        // 將日期轉換成 Dictionary 使用的格式。
        string dateKey =
            date.ToString("yyyy-MM-dd");

        // 嘗試取得這一天的官方資料。
        if (holidayRecords.TryGetValue(
            dateKey,
            out HolidayRecord record))
        {
            // 回傳官方資料中的節日名稱。
            return record.name;
        }

        // 找不到資料時回傳空字串。
        return "";
    }
}
