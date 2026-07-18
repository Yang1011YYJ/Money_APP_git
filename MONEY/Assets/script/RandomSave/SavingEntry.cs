using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SavingEntry
{
    public int year;
    public int month;
    public int day;
    public int amount;//¦sªºª÷ÃB

    public System.DateTime GetDate()
    {
        return new System.DateTime(year, month, day);
    }
}
