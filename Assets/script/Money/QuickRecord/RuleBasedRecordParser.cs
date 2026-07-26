// 引入 System，讓程式可以使用 DateTime。
using System;

// 引入泛型集合，讓程式可以使用 Dictionary 和 List。
using System.Collections.Generic;

// 引入正規表示式功能，用來尋找句子中的金額。
using System.Text.RegularExpressions;

// 建立規則式帳目解析器。
public class RuleBasedRecordParser
{
    // 建立付款方式關鍵字清單。
    private readonly List<string> paymentMethods =
        new List<string>
        {
            "LINE Pay",
            "Line Pay",
            "line pay",
            "街口支付",
            "街口",
            "悠遊卡",
            "一卡通",
            "Apple Pay",
            "Google Pay",
            "信用卡",
            "金融卡",
            "轉帳",
            "現金"
        };

    // 建立分類和品項關鍵字的對應資料。
    private readonly Dictionary<string, string[]> categoryKeywords =
        new Dictionary<string, string[]>
        {
            // 餐飲分類常見關鍵字。
            {
                "餐飲",
                new[]
                {
                    "早餐",
                    "午餐",
                    "晚餐",
                    "宵夜",
                    "便當",
                    "飲料",
                    "咖啡",
                    "茶",
                    "餐廳",
                    "麥當勞",
                    "火鍋",
                    "食物"
                }
            },

            // 交通分類常見關鍵字。
            {
                "交通",
                new[]
                {
                    "公車",
                    "捷運",
                    "火車",
                    "高鐵",
                    "計程車",
                    "uber",
                    "Uber",
                    "加油",
                    "停車",
                    "車票"
                }
            },

            // 購物分類常見關鍵字。
            {
                "購物",
                new[]
                {
                    "衣服",
                    "鞋子",
                    "包包",
                    "網購",
                    "蝦皮",
                    "生活用品",
                    "日用品"
                }
            },

            // 娛樂分類常見關鍵字。
            {
                "娛樂",
                new[]
                {
                    "電影",
                    "遊戲",
                    "唱歌",
                    "KTV",
                    "展覽",
                    "門票"
                }
            },

            // 收入分類常見關鍵字。
            {
                "收入",
                new[]
                {
                    "薪水",
                    "薪資",
                    "獎金",
                    "退款",
                    "利息",
                    "股息",
                    "收入"
                }
            }
        };

    // 解析使用者輸入的自然語句。
    public RecordParseResult Parse(string originalText) 
    {
        // 建立一個新的解析結果。
        RecordParseResult result =
            new RecordParseResult();

        // 預設先將解析狀態設定為失敗。
        result.isSuccess = false;

        // 預設將日期設定為今天。
        result.date = DateTime.Today;

        // 預設付款方式為未指定。
        result.paymentMethod = "未指定";

        // 預設分類為其他。
        result.category = "其他";

        // 預設帳目類型為支出。
        result.recordType = RecordType.Expense;

        // 檢查使用者是否沒有輸入內容。
        if (string.IsNullOrWhiteSpace(originalText))
        {
            // 保存錯誤提示。
            result.message = "請先輸入記帳內容。";

            // 回傳失敗結果。
            return result;
        }

        // 移除句子前後的空白。
        string cleanText = originalText.Trim();

        // 解析日期。
        result.date = ParseDate(cleanText);

        // 解析金額。
        result.amount = ParseAmount(cleanText);

        // 解析付款方式。
        result.paymentMethod = ParsePaymentMethod(cleanText);

        // 判斷帳目分類。
        result.category = ParseCategory(cleanText);

        // 判斷收入或支出。
        result.recordType = ParseRecordType(cleanText, result.category);

        // 同時解析主要品項與備註。
        ParsedItemNote itemNote =
            ParseItemAndNote(
                cleanText,
                result.amount,
                result.paymentMethod);

        // 保存解析出的品項。
        result.itemName =
            itemNote.itemName;

        // 保存解析出的備註。
        result.note =
            itemNote.note;

        // 如果品項是依照進食語句與目前時間推測出的餐別。
        if (itemNote.isMealInferred)
        {
            // 將分類設定為餐飲。
            result.category = "餐飲";

            // 這種情況屬於支出。
            result.recordType = RecordType.Expense;
        }

        // 判斷是否沒有取得有效金額。
        if (result.amount <= 0)
        {
            // 保存缺少金額的提示。
            result.message = "找不到有效金額，請確認句子中有輸入金額。";

            // 回傳失敗結果。
            return result;
        }

        // 判斷品項是否為空。
        if (string.IsNullOrWhiteSpace(result.itemName))
        {
            // 找不到品項時先使用分類名稱。
            result.itemName = result.category;
        }

        // 將解析狀態設定為成功。
        result.isSuccess = true;

        // 保存成功訊息。
        result.message = "解析成功。";

        // 回傳完整解析結果。
        return result;
    }

    // 從文字中解析日期。
    private DateTime ParseDate(string text)
    {
        // 判斷句子是否包含前天。
        if (text.Contains("前天"))
        {
            // 回傳前天日期。
            return DateTime.Today.AddDays(-2);
        }

        // 判斷句子是否包含昨天。
        if (text.Contains("昨天"))
        {
            // 回傳昨天日期。
            return DateTime.Today.AddDays(-1);
        }

        // 判斷句子是否包含明天。
        if (text.Contains("明天"))
        {
            // 回傳明天日期。
            return DateTime.Today.AddDays(1);
        }

        // 判斷句子是否包含後天。
        if (text.Contains("後天"))
        {
            // 回傳後天日期。
            return DateTime.Today.AddDays(2);
        }

        // 沒有辨識到其他日期時，預設回傳今天。
        return DateTime.Today;
    }

    // 從使用者輸入的文字中解析金額。
    private int ParseAmount(string text)
    {
        // ==============================
        // 第一優先：尋找「花了 + 阿拉伯數字」。
        // 例如「花了290」、「花了2,900元」。
        // ==============================

        Match spendArabicMatch =
            Regex.Match(
                text,
                @"花了\s*(?<amount>\d[\d,]*)\s*(元|塊)?");

        // 判斷是否成功找到。
        if (spendArabicMatch.Success)
        {
            // 取得數字文字並移除千分位逗號。
            string amountText =
                spendArabicMatch.Groups["amount"]
                    .Value
                    .Replace(",", "");

            // 嘗試轉成整數。
            if (int.TryParse(amountText, out int amount))
            {
                // 成功時直接回傳。
                return amount;
            }
        }

        // ==============================
        // 第二優先：尋找「花了 + 中文數字」。
        // 例如「花了兩百九」、「花了四千」。
        // ==============================

        Match spendChineseMatch =
            Regex.Match(
                text,
                @"花了\s*(?<amount>[零〇一二兩三四五六七八九十百千萬億]+)\s*(元|塊)?");

        // 判斷是否成功找到。
        if (spendChineseMatch.Success)
        {
            // 將中文數字轉成整數。
            int amount =
                ChineseNumberToInt(
                    spendChineseMatch.Groups["amount"].Value);

            // 金額有效時直接回傳。
            if (amount > 0)
            {
                // 回傳金額。
                return amount;
            }
        }

        // ==============================
        // 第三優先：尋找明確「數字 + 元／塊」。
        // 例如「290元」、「2,900塊」。
        // ==============================

        Match moneyArabicMatch =
            Regex.Match(
                text,
                @"(?<amount>\d[\d,]*)\s*(元|塊)");

        // 判斷是否找到。
        if (moneyArabicMatch.Success)
        {
            // 移除千分位逗號。
            string amountText =
                moneyArabicMatch.Groups["amount"]
                    .Value
                    .Replace(",", "");

            // 嘗試轉成整數。
            if (int.TryParse(amountText, out int amount))
            {
                // 回傳金額。
                return amount;
            }
        }

        // ==============================
        // 第四優先：尋找「中文數字 + 元／塊」。
        // 例如「兩百九元」。
        // ==============================

        Match moneyChineseMatch =
            Regex.Match(
                text,
                @"(?<amount>[零〇一二兩三四五六七八九十百千萬億]+)\s*(元|塊)");

        // 判斷是否找到。
        if (moneyChineseMatch.Success)
        {
            // 將中文數字轉換為整數。
            int amount =
                ChineseNumberToInt(
                    moneyChineseMatch.Groups["amount"].Value);

            // 判斷結果是否有效。
            if (amount > 0)
            {
                // 回傳金額。
                return amount;
            }
        }

        // ==============================
        // 前面都沒有明確金額提示時，
        // 開始進行「一般數字 = 金額」的推測。
        // ==============================

        // 先建立一份只用於尋找金額的文字。
        string amountSearchText =
            text;

        // 移除相對日期文字。
        // 避免這些資訊干擾後續數字判斷。
        amountSearchText =
            amountSearchText
                .Replace("今天", "")
                .Replace("昨天", "")
                .Replace("前天", "")
                .Replace("明天", "")
                .Replace("後天", "");

        // ==============================
        // 第五優先：一般阿拉伯數字。
        // 例如「今天晚餐290」。
        // ==============================

        Match generalArabicMatch =
            Regex.Match(
                amountSearchText,
                @"(?<amount>\d[\d,]*)");

        // 判斷是否找到數字。
        if (generalArabicMatch.Success)
        {
            // 取得數字並移除千分位逗號。
            string amountText =
                generalArabicMatch.Groups["amount"]
                    .Value
                    .Replace(",", "");

            // 嘗試轉成整數。
            if (int.TryParse(amountText, out int amount))
            {
                // 回傳金額。
                return amount;
            }
        }

        // ==============================
        // 第六優先：一般中文數字。
        // 這就是讓「兩百九」即使沒有「花了」也能被找到。
        // ==============================

        Match generalChineseMatch =
            Regex.Match(
                amountSearchText,
                @"(?<amount>[零〇一二兩三四五六七八九十百千萬億]+)");

        // 判斷是否找到中文數字。
        if (generalChineseMatch.Success)
        {
            // 將中文數字轉成整數。
            int amount =
                ChineseNumberToInt(
                    generalChineseMatch.Groups["amount"].Value);

            // 判斷金額是否有效。
            if (amount > 0)
            {
                // 回傳金額。
                return amount;
            }
        }

        // 完全找不到數值時才回傳0。
        return 0;
    }

    // 從文字中解析付款方式。
    private string ParsePaymentMethod(string text)
    {
        // 逐一檢查所有已知付款方式。
        for (int i = 0; i < paymentMethods.Count; i++)
        {
            // 取得目前要檢查的付款方式。
            string paymentMethod = paymentMethods[i];

            // 判斷句子是否包含這個付款方式。
            if (text.Contains(paymentMethod))
            {
                // 統一 LINE Pay 的不同大小寫。
                if (paymentMethod.ToLower().Contains("line pay"))
                {
                    // 回傳統一格式。
                    return "LINE Pay";
                }

                // 回傳找到的付款方式。
                return paymentMethod;
            }
        }

        // 找不到付款方式時回傳現金。
        return "現金";
    }

    // 根據句子中的關鍵字推測分類。
    private string ParseCategory(string text)
    {
        // 逐一檢查所有分類。
        foreach (
            KeyValuePair<string, string[]> categoryPair
            in categoryKeywords)
        {
            // 取得這個分類的所有關鍵字。
            string[] keywords = categoryPair.Value;

            // 逐一檢查分類關鍵字。
            for (int i = 0; i < keywords.Length; i++)
            {
                // 判斷句子是否包含目前關鍵字。
                if (text.Contains(keywords[i]))
                {
                    // 找到後回傳對應分類。
                    return categoryPair.Key;
                }
            }
        }

        // 找不到符合分類時回傳其他。
        return "其他";
    }

    // 判斷這筆帳目是收入或支出。
    private RecordType ParseRecordType(
        string text,
        string category)
    {
        // 判斷分類是否是收入。
        if (category == "收入")
        {
            // 回傳收入。
            return RecordType.Income;
        }

        // 判斷句子是否有明確收入相關詞語。
        if (
            text.Contains("收到") ||
            text.Contains("入帳") ||
            text.Contains("賺了"))
        {
            // 回傳收入。
            return RecordType.Income;
        }

        // 其他情況預設為支出。
        return RecordType.Expense;
    }

    // 從原始句子中推測品項名稱。
    /*private string ParseItemName(
        string originalText,
        int amount,
        string paymentMethod)
    {
        // 複製原始文字，避免直接修改傳入內容。
        string itemText = originalText;

        // 移除常見日期詞語。
        itemText =
            itemText
                .Replace("今天", "")
                .Replace("昨天", "")
                .Replace("前天", "")
                .Replace("明天", "")
                .Replace("後天", "");

        // 移除「花了 + 阿拉伯數字」形式的金額文字。
        itemText =
            Regex.Replace(
                itemText,
                @"花了\s*\d[\d,]*\s*(元|塊)?",
                "");

        // 移除「花了 + 中文數字」形式的金額文字。
        itemText =
            Regex.Replace(
                itemText,
                @"花了\s*[零〇一二兩三四五六七八九十百千萬億]+\s*(元|塊)?",
                "");

        // 移除一般「阿拉伯數字 + 元／塊」形式。
        itemText =
            Regex.Replace(
                itemText,
                @"\d[\d,]*\s*(元|塊)",
                "");

        // 移除一般「中文數字 + 元／塊」形式。
        itemText =
            Regex.Replace(
                itemText,
                @"[零〇一二兩三四五六七八九十百千萬億]+\s*(元|塊)",
                "");

        // 判斷是否有辨識到付款方式。
        if (
            !string.IsNullOrWhiteSpace(paymentMethod) &&
            paymentMethod != "未指定")
        {
            // 移除付款方式文字。
            itemText =
                itemText.Replace(
                    paymentMethod,
                    "");
        }

        // 移除常見連接文字和標點符號。
        itemText =
            itemText
                .Replace("用", "")
                .Replace("使用", "")
                .Replace("付款", "")
                .Replace("支付", "")
                .Replace("花了", "")
                .Replace("買了", "")
                .Replace("，", "")
                .Replace(",", "")
                .Replace("。", "")
                .Trim();

        // 回傳整理後的品項名稱。
        return itemText;
    }*/

    // 從已移除日期、金額與付款方式的文字中，
    // 分析主要品項以及額外備註。
    private ParsedItemNote ParseItemAndNote(
        string originalText,
        int amount,
        string paymentMethod)
    {
        // 建立品項與備註解析結果。
        ParsedItemNote result =
            new ParsedItemNote();

        // 預設品項為空字串。
        result.itemName = "";

        // 預設備註為空字串。
        result.note = "";

        // 複製原始文字，避免直接修改傳入內容。
        string cleanText =
            originalText.Trim();

        // 移除日期相關詞語。
        cleanText =
            cleanText
                .Replace("今天", "")
                .Replace("昨天", "")
                .Replace("前天", "")
                .Replace("明天", "")
                .Replace("後天", "");

        // 移除「花了 + 阿拉伯數字」形式的金額。
        cleanText =
            Regex.Replace(
                cleanText,
                @"花了\s*\d[\d,]*\s*(元|塊)?",
                "");

        // 移除「花了 + 中文數字」形式的金額。
        cleanText =
            Regex.Replace(
                cleanText,
                @"花了\s*[零〇一二兩三四五六七八九十百千萬億]+\s*(元|塊)?",
                "");

        // 移除一般「阿拉伯數字 + 元／塊」形式。
        cleanText =
            Regex.Replace(
                cleanText,
                @"\d[\d,]*\s*(元|塊)",
                "");

        // 移除一般「中文數字 + 元／塊」形式。
        cleanText =
            Regex.Replace(
                cleanText,
                @"[零〇一二兩三四五六七八九十百千萬億]+\s*(元|塊)",
                "");

        // 移除剩餘的阿拉伯數字。
        // 因為前面已經完成金額解析，這裡的目的只是不要讓金額跑進品項或備註。
        cleanText =
            Regex.Replace(
                cleanText,
                @"\d[\d,]*",
                "");

        // 移除剩餘的中文數字。
        // 例如「兩百九」即使沒有「花了」或「元」，也不要留在備註中。
        cleanText =
            Regex.Replace(
                cleanText,
                @"[零〇一二兩三四五六七八九十百千萬億]+",
                "");

        // 如果有成功辨識付款方式，就將付款方式從文字中移除。
        if (!string.IsNullOrWhiteSpace(paymentMethod) && paymentMethod != "未指定")
        {
            // 移除付款方式。
            cleanText =
                cleanText.Replace(
                    paymentMethod,
                    "");
        }

        // 移除常見付款連接詞。
        cleanText =
            cleanText
                .Replace("用", "")
                .Replace("使用", "")
                .Replace("付款", "")
                .Replace("支付", "");

        // 清除標點符號與多餘空白。
        cleanText =
            cleanText
                .Replace("，", " ")
                .Replace(",", " ")
                .Replace("。", " ")
                .Trim();

        // 尋找句子中是否存在明確品項關鍵字。
        string detectedItem =
            FindItemKeyword(cleanText);

        // 如果找到明確品項。
        if (!string.IsNullOrWhiteSpace(detectedItem))
        {
            // 將找到的關鍵字作為主要品項。
            result.itemName = detectedItem;

            // 從剩餘文字中移除主要品項。
            string remainingText =
                cleanText.Replace(
                    detectedItem,
                    "");

            // 將剩餘內容整理成備註。
            result.note =
                CleanNoteText(remainingText);

            // 回傳解析結果。
            return result;
        }

        // 如果沒有找到早餐、午餐、晚餐等明確品項，
        // 再判斷句子中是否存在餐飲相關動作。
        if (IsFoodActionText(cleanText))
        {
            // 根據使用者輸入這句話的當下時間，
            // 推測早餐、午餐、晚餐或宵夜。
            result.itemName = GetMealByCurrentTime();

            // 將「吃、喝、點、訂」等餐飲動作移除，
            // 剩下的店名、食物名稱或補充資訊放入備註。
            result.note = CleanFoodDetail(cleanText);

            // 紀錄這個餐別不是使用者明確輸入，
            // 而是系統依照時間推測得到的。
            result.isMealInferred = true;

            // 完成解析並回傳。
            return result;
        }

        // 如果沒有找到已知品項，
        // 嘗試將句子拆成「主要內容 + 備註」。
        SplitUnknownItemAndNote(
            cleanText,
            result);

        // 回傳最後結果。
        return result;
    }

    // 從文字中尋找已知的品項關鍵字。
    private string FindItemKeyword(string text)
    {
        // 逐一檢查所有已知品項。
        for (int i = 0; i < itemKeywords.Count; i++)
        {
            // 取得目前品項。
            string keyword =
                itemKeywords[i];

            // 判斷輸入文字是否包含目前品項。
            if (text.Contains(keyword))
            {
                // 找到後直接回傳。
                return keyword;
            }
        }

        // 找不到任何已知品項時回傳空字串。
        return "";
    }

    // 清理剩餘文字，作為帳目的備註。
    private string CleanNoteText(string text)
    {
        // 判斷輸入文字是否為空。
        if (string.IsNullOrWhiteSpace(text))
        {
            // 沒有備註時回傳空字串。
            return "";
        }

        // 移除前後空白。
        string noteText = text.Trim();

        // 移除剩餘文字中的常見餐飲動作詞。
        // 例如：
        // 「吃美美」→「美美」
        // 「美美吃」→「美美」
        // 「吃吃美美」→「美美」
        noteText =
            Regex.Replace(
                noteText,
                "(吃|喝|點|訂)",
                "");

        // 移除可能留下的多餘空白。
        noteText = noteText.Trim();

        // 回傳整理後的備註。
        return noteText;
    }

    // 在沒有找到已知品項時，
    // 嘗試將主要內容和明顯備註分開。
    private void SplitUnknownItemAndNote(
        string text,
        ParsedItemNote result)
    {
        // 整理輸入文字。
        string cleanText =
            text.Trim();

        // 建立常見的備註提示詞。
        string[] noteIndicators =
        {
        "男朋友",
        "女朋友",
        "朋友",
        "家人",
        "公司",
        "報帳",
        "忘記",
        "請客",
        "代墊",
        "幫我",
        "幫忙"
    };

        // 預設沒有找到備註起始位置。
        int noteStartIndex = -1;

        // 逐一尋找備註提示詞。
        for (int i = 0; i < noteIndicators.Length; i++)
        {
            // 尋找目前提示詞的位置。
            int currentIndex =
                cleanText.IndexOf(
                    noteIndicators[i]);

            // 如果完全沒有找到，就繼續下一個。
            if (currentIndex < 0)
            {
                // 繼續尋找。
                continue;
            }

            // 如果這是目前找到的第一個提示詞，
            // 或比之前找到的位置更前面。
            if (
                noteStartIndex < 0 ||
                currentIndex < noteStartIndex)
            {
                // 保存最前面的備註起始位置。
                noteStartIndex = currentIndex;
            }
        }

        // 如果找到明顯備註。
        if (noteStartIndex > 0)
        {
            // 備註之前的內容先作為品項。
            result.itemName =
                cleanText
                    .Substring(
                        0,
                        noteStartIndex)
                    .Trim();

            // 從提示詞開始到最後作為備註。
            result.note =
                cleanText
                    .Substring(
                        noteStartIndex)
                    .Trim();

            // 完成處理。
            return;
        }

        // 如果整句都是明顯備註。
        if (noteStartIndex == 0)
        {
            // 品項暫時留空。
            result.itemName = "";

            // 整句保存成備註。
            result.note = cleanText;

            // 完成處理。
            return;
        }

        // 完全找不到備註訊號時，
        // 暫時將剩餘文字當成品項。
        result.itemName = cleanText;

        // 沒有備註。
        result.note = "";
    }

    // 將中文數字轉換成整數。
    // 同時支援完整寫法與日常口語省略單位的寫法。
    // 例如：
    // 「兩千八」→ 2800
    // 「三百五」→ 350
    // 「一萬二」→ 12000
    // 「三千零四」→ 3004
    // 「三千零四十」→ 3040
    private int ChineseNumberToInt(string chineseNumber)
    {
        // 判斷輸入內容是否為空。
        if (string.IsNullOrWhiteSpace(chineseNumber))
        {
            // 沒有內容時回傳0。
            return 0;
        }

        // 先取得標準中文字解析結果。
        int normalResult =
            ParseStandardChineseNumber(chineseNumber);

        // 判斷最後一個字是否為單純數字。
        int lastDigit =
            GetChineseDigit(
                chineseNumber[chineseNumber.Length - 1]);

        // 如果最後一個字不是數字，
        // 例如「兩千八十」最後是「十」，
        // 代表單位已經寫完整，直接使用標準解析結果。
        if (lastDigit < 0)
        {
            // 回傳標準解析結果。
            return normalResult;
        }

        // 尋找最後一個數字以前，最近出現的數字單位。
        int previousUnitIndex = -1;

        // 儲存最近一個單位所代表的數值。
        long previousUnitValue = 0;

        // 從倒數第二個字開始往前尋找。
        for (int i = chineseNumber.Length - 2; i >= 0; i--)
        {
            // 取得目前中文字。
            char currentChar = chineseNumber[i];

            // 取得這個字代表的單位。
            long unitValue =
                GetChineseUnitValue(currentChar);

            // 如果找到數字單位。
            if (unitValue > 0)
            {
                // 保存這個單位的位置。
                previousUnitIndex = i;

                // 保存這個單位的數值。
                previousUnitValue = unitValue;

                // 找到最近的單位後停止搜尋。
                break;
            }
        }

        // 如果前面完全沒有單位，
        // 例如只有「八」，
        // 就直接使用原本解析結果。
        if (previousUnitIndex < 0)
        {
            // 回傳標準解析結果。
            return normalResult;
        }

        // 檢查最近單位與最後一個數字之間是否出現「零」或「〇」。
        bool hasZeroAfterUnit = false;

        // 從最近單位後面開始檢查到最後一個數字之前。
        for (
            int i = previousUnitIndex + 1;
            i < chineseNumber.Length - 1;
            i++)
        {
            // 判斷目前是否為零。
            if (
                chineseNumber[i] == '零' ||
                chineseNumber[i] == '〇')
            {
                // 紀錄中間出現零。
                hasZeroAfterUnit = true;

                // 已經確定，不需要繼續檢查。
                break;
            }
        }

        // 如果中間有零，
        // 代表使用者有刻意指出位值斷層，
        // 不套用口語省略規則。
        if (hasZeroAfterUnit)
        {
            // 使用標準解析結果。
            return normalResult;
        }

        // 取得最近單位的下一級單位。
        long nextUnitValue =
            GetNextLowerUnitValue(previousUnitValue);

        // 如果這個單位沒有可推導的下一級，
        // 就使用標準結果。
        if (nextUnitValue <= 0)
        {
            // 回傳標準解析結果。
            return normalResult;
        }

        // 標準解析中，最後一個數字已經被當成個位數加進去了。
        // 所以先把這個個位數移除。
        long result =
            normalResult - lastDigit;

        // 再將最後一個數字乘上推導出的下一級單位。
        result +=
            lastDigit * nextUnitValue;

        // 確認結果沒有超過 int 可以儲存的範圍。
        if (result > int.MaxValue)
        {
            // 超過範圍時視為解析失敗。
            return 0;
        }

        // 回傳最後解析完成的數字。
        return (int)result;
    }

    // 將單一中文數字轉換成0到9。
    private int GetChineseDigit(char chineseDigit)
    {
        // 根據不同中文字回傳對應數字。
        switch (chineseDigit)
        {
            // 中文零。
            case '零':

            // 另一種零的寫法。
            case '〇':

                // 回傳0。
                return 0;

            // 中文一。
            case '一':

                // 回傳1。
                return 1;

            // 中文二。
            case '二':

            // 日常口語常使用「兩百」、「兩千」。
            case '兩':

                // 回傳2。
                return 2;

            // 中文三。
            case '三':

                // 回傳3。
                return 3;

            // 中文四。
            case '四':

                // 回傳4。
                return 4;

            // 中文五。
            case '五':

                // 回傳5。
                return 5;

            // 中文六。
            case '六':

                // 回傳6。
                return 6;

            // 中文七。
            case '七':

                // 回傳7。
                return 7;

            // 中文八。
            case '八':

                // 回傳8。
                return 8;

            // 中文九。
            case '九':

                // 回傳9。
                return 9;

            // 不屬於0到9時回傳-1。
            default:

                // 代表這個字不是單一數字。
                return -1;
        }
    }

    // 依照中文字中明確寫出的數字與單位進行標準解析。
    // 這個方法不處理口語省略單位。
    private int ParseStandardChineseNumber(string chineseNumber)
    {
        // 儲存整個數字目前的總和。
        long total = 0;

        // 儲存目前「萬」或「億」區段中的數值。
        long section = 0;

        // 儲存目前讀到的單一數字。
        long number = 0;

        // 逐一處理每一個中文字。
        for (int i = 0; i < chineseNumber.Length; i++)
        {
            // 取得目前正在處理的字。
            char currentChar =
                chineseNumber[i];

            // 嘗試取得這個字所代表的0到9。
            int digit =
                GetChineseDigit(currentChar);

            // 如果目前是一般數字。
            if (digit >= 0)
            {
                // 暫存目前數字。
                number = digit;

                // 繼續處理下一個字。
                continue;
            }

            // 判斷目前是否為十。
            if (currentChar == '十')
            {
                // 「十五」這類沒有寫「一」的情況，
                // 預設視為一十。
                if (number == 0)
                {
                    // 補上一。
                    number = 1;
                }

                // 將目前數字乘以十加入區段。
                section += number * 10;

                // 清除暫存數字。
                number = 0;

                // 繼續處理。
                continue;
            }

            // 判斷目前是否為百。
            if (currentChar == '百')
            {
                // 沒寫前置數字時預設為一。
                if (number == 0)
                {
                    // 補上一。
                    number = 1;
                }

                // 加入百位。
                section += number * 100;

                // 清除暫存數字。
                number = 0;

                // 繼續處理。
                continue;
            }

            // 判斷目前是否為千。
            if (currentChar == '千')
            {
                // 沒有前置數字時預設為一。
                if (number == 0)
                {
                    // 補上一。
                    number = 1;
                }

                // 加入千位。
                section += number * 1000;

                // 清除暫存數字。
                number = 0;

                // 繼續處理。
                continue;
            }

            // 判斷目前是否為萬。
            if (currentChar == '萬')
            {
                // 將尚未加入的數字加入目前區段。
                section += number;

                // 如果萬前完全沒有內容，就當成一萬。
                if (section == 0)
                {
                    // 設定為一。
                    section = 1;
                }

                // 將整個區段乘上一萬後加入總數。
                total += section * 10000;

                // 清空目前區段。
                section = 0;

                // 清空目前數字。
                number = 0;

                // 繼續處理。
                continue;
            }

            // 判斷目前是否為億。
            if (currentChar == '億')
            {
                // 將尚未加入的數字加入區段。
                section += number;

                // 如果億前沒有內容，就視為一億。
                if (section == 0)
                {
                    // 設定為一。
                    section = 1;
                }

                // 將前面全部內容乘上一億。
                total =
                    (total + section) * 100000000;

                // 清空區段。
                section = 0;

                // 清空目前數字。
                number = 0;
            }
        }

        // 將最後剩餘內容加入總數。
        total += section + number;

        // 檢查是否超過 int 最大值。
        if (total > int.MaxValue)
        {
            // 超過時視為失敗。
            return 0;
        }

        // 回傳標準解析結果。
        return (int)total;
    }

    // 取得中文字單位所代表的數值。
    private long GetChineseUnitValue(char chineseUnit)
    {
        // 根據不同單位回傳數值。
        switch (chineseUnit)
        {
            // 十。
            case '十':
                // 代表10。
                return 10;

            // 百。
            case '百':
                // 代表100。
                return 100;

            // 千。
            case '千':
                // 代表1000。
                return 1000;

            // 萬。
            case '萬':
                // 代表10000。
                return 10000;

            // 億。
            case '億':
                // 代表100000000。
                return 100000000;

            // 不是單位。
            default:
                // 回傳0。
                return 0;
        }
    }

    // 根據目前單位取得口語省略時應使用的下一級單位。
    private long GetNextLowerUnitValue(long currentUnit)
    {
        // 根據目前單位判斷下一級。
        switch (currentUnit)
        {
            // 億的下一級視為千萬。
            case 100000000:
                // 回傳一千萬。
                return 10000000;

            // 萬的下一級是千。
            case 10000:
                // 回傳一千。
                return 1000;

            // 千的下一級是百。
            case 1000:
                // 回傳一百。
                return 100;

            // 百的下一級是十。
            case 100:
                // 回傳十。
                return 10;

            // 十的下一級是個位。
            case 10:
                // 回傳一。
                return 1;

            // 不支援的單位。
            default:
                // 回傳0。
                return 0;
        }
    }

    // 儲存常見可直接辨識為品項的關鍵字。
    private readonly List<string> itemKeywords =
        new List<string>
        {
        // 餐飲相關品項。
        "早餐",
        "午餐",
        "晚餐",
        "宵夜",
        "咖啡",
        "飲料",
        "點心",

        // 交通相關品項。
        "捷運",
        "公車",
        "火車",
        "高鐵",
        "計程車",
        "Uber",
        "加油",
        "停車"
        };

    // 根據收到句子的當下時間，推測目前最可能是哪一餐。
    private string GetMealByCurrentTime()
    {
        // 取得現在時間的小時數，範圍為0到23。
        int currentHour = DateTime.Now.Hour;

        // 凌晨4點到上午11點以前，判斷為早餐。
        if (currentHour >= 4 && currentHour < 11)
        {
            // 回傳早餐。
            return "早餐";
        }

        // 上午11點到下午4點以前，判斷為午餐。
        if (currentHour >= 11 && currentHour < 16)
        {
            // 回傳午餐。
            return "午餐";
        }

        // 下午4點到晚上11點以前，判斷為晚餐。
        if (currentHour >= 16 && currentHour < 23)
        {
            // 回傳晚餐。
            return "晚餐";
        }

        // 晚上11點到隔天凌晨4點以前，判斷為宵夜。
        return "宵夜";
    }

    // 判斷剩餘文字是否是在描述吃東西或喝東西。
    private bool IsFoodActionText(string text)
    {
        // 檢查文字是否為空。
        if (string.IsNullOrWhiteSpace(text))
        {
            // 沒有內容時不視為餐飲語句。
            return false;
        }

        // 移除前後多餘空白。
        string cleanText = text.Trim();

        // 判斷文字中是否出現常見餐飲動作。
        // 不限制一定要在句子開頭，
        // 因為使用者可能輸入「吃美美」或「美美吃」。
        if (
            cleanText.Contains("吃") ||
            cleanText.Contains("喝") ||
            cleanText.Contains("點") ||
            cleanText.Contains("訂"))
        {
            // 只要存在明確餐飲動作，就視為餐飲相關語句。
            return true;
        }

        // 沒有發現餐飲動作。
        return false;
    }

    // 清理餐飲語句，只留下店名、食物名稱或其他補充資訊。
    private string CleanFoodDetail(string text)
    {
        // 判斷文字是否為空。
        if (string.IsNullOrWhiteSpace(text))
        {
            // 沒有內容時回傳空字串。
            return "";
        }

        // 移除前後多餘空白。
        string result = text.Trim();

        // 移除常見的餐飲動作詞。
        // 不限制動作詞的位置，
        // 因此「吃美美」與「美美吃」最後都會留下「美美」。
        result =
            Regex.Replace(
                result,
                "(吃|喝|點|訂)",
                "");

        // 移除常見、不需要保留在備註中的連接詞。
        result =
            result
                .Replace("去", "")
                .Replace("在", "");

        // 移除處理後可能留下的多餘空白。
        result = result.Trim();

        // 回傳整理後的店名、食物名稱或補充內容。
        return result;
    }
}