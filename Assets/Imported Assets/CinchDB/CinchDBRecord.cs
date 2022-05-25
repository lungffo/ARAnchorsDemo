using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A Record 
/// </summary>
public class CinchDBRecord
{
    public string[] Columns = new string[10];

    // True if the record is currently being edited by the user, otherwise false.
    public bool IsEditing;

    // The unique record code.
    public string RecordID = "";

    /// <summary>
    /// 
    /// </summary>
    public CinchDBRecord (params string[] columnValues)
    {
        Columns = new string[10];
        for (int i = 0; i < columnValues.Length && i < 10; i++)
        {
            Columns[i] = columnValues[i];
        }
        if (columnValues.Length == 11)
        {
            RecordID = columnValues[10];
        }
    }
}
