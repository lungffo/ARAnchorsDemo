using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using UnityEngine;

/// <summary>
/// Stores data related to a CinchDB database.
/// </summary>
public class CinchDBDatabase : ScriptableObject
{
    [Header("Unique database key. Do not edit.")]
    // The key this database connects to.
    public string key;

    [Header("How many columns does this database have?")]
    // The number of columns used by this database. This doesn't affect functionality,
    // but does limit the number of columns shown in the editor.
    [Range(1, 10)]
    public int visibleColumns = 5;

    [Header("Database column headers. Change these.")]
    /// <summary>
    /// The database column headers.
    /// </summary>
    public string[] ColumnHeaders = new string[]
    {
        "Column 1",
        "Column 2",
        "Column 3",
        "Column 4",
        "Column 5",
        "Column 6",
        "Column 7",
        "Column 8",
        "Column 9",
        "Column 10"
    };

    //public CinchDBColumn sortColumn = CinchDBColumn.None;
    //public bool sortAscending = true;

    /// <summary>
    /// Adds any number of records to the database.
    /// </summary>
    public async Task AddRecords(params CinchDBRecord[] records)
    {
        await CinchDB.AddRecords(this, records);
    }

    /// <summary>
    /// Adds one record to the database.
    /// </summary>
    public async Task AddRecord(CinchDBRecord record)
    {
        await CinchDB.AddRecords(this, record);
    }

    /// <summary>
    /// Adds a single record to the database.
    /// </summary>
    public async Task AddRecord(params string[] columns)
    {
        await CinchDB.AddRecords(this, new CinchDBRecord(columns));
    }

    /// <summary>
    /// Deletes any number of records from the database. Only use this method if you have
    /// a reference to specific record objects. 
    /// </summary>
    public async Task DeleteRecords(params CinchDBRecord[] records)
    {
        await CinchDB.DeleteRecords(this, records);
    }

    /// <summary>
    /// Clears all data from the database. !!WARNING!! Are you sure you want to use this method?!
    /// </summary>
    public async Task ClearAllRecords()
    {
        await CinchDB.ClearAllRecords(this);
    }

    /// <summary>
    /// Update all of the specified records to use the newly stored values.
    /// </summary>
    public async Task UpdateRecords(params CinchDBRecord[] records)
    {
        await CinchDB.UpdateRecords(this, records);
    }

    /// <summary>
    /// Retrieves all records stored in the remote database.
    /// </summary>
    public async Task<List<CinchDBRecord>> GetAllRecords()
    {
        return await CinchDB.GetAllRecords(this);
    }
}
