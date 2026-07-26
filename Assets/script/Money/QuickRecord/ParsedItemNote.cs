// 儲存品項與備註解析結果。
public class ParsedItemNote
{
    // 儲存解析出的品項名稱。
    public string itemName;

    // 儲存解析出的備註。
    public string note;

    // 紀錄這次品項是否是依照目前時間推測出的餐別。
    public bool isMealInferred;
}