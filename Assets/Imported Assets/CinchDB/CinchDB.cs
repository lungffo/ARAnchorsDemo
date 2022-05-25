using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;

/// <summary>
/// Main CinchDB class. Contains a number of helper methods for communicating with the remote
/// database. 
/// </summary>
public class CinchDB
{
    /// <summary>
    /// Adds any number of records to the database.
    /// </summary>
    public static async Task AddRecords(CinchDBDatabase database, params CinchDBRecord[] records)
    {
        foreach (CinchDBRecord record in records)
        {
            string result = "";
            foreach (string col in record.Columns)
            {
                result += "/" + col;
            }
            string url = $"https://cinchdb.com/{database.key}/insert{result}";
            using (WebClient client = new WebClient())
            {
                string responseText = await client.DownloadStringTaskAsync(url);
            }
        }
    }

    /// <summary>
    /// Deletes any number of records from the database. Only use this method if you have
    /// a reference to specific record objects. 
    /// </summary>
    public static async Task DeleteRecords(CinchDBDatabase database, params CinchDBRecord[] records)
    {
        foreach (CinchDBRecord record in records)
        {
            if (record.RecordID.Length > 0)
            {
                string url = $"https://cinchdb.com/{database.key}/deletebykey/{record.RecordID}";
                using (WebClient client = new WebClient())
                {
                    string responseText = await client.DownloadStringTaskAsync(url);
                }
            }
        }
    }

    /// <summary>
    /// Clears all data from the database. !!WARNING!! Are you sure you want to use this method?!
    /// </summary>
    public static async Task ClearAllRecords(CinchDBDatabase database)
    {
        string url = $"https://cinchdb.com/{database.key}/clear";
        using (WebClient client = new WebClient())
        {
            string responseText = await client.DownloadStringTaskAsync(url);
        }
    }

    /// <summary>
    /// Update all of the specified records to use the newly stored values.
    /// </summary>
    public static async Task UpdateRecords(CinchDBDatabase database, params CinchDBRecord[] records)
    {
        foreach (CinchDBRecord record in records)
        {
            if (record.RecordID.Length > 0)
            {
                string url = $"https://cinchdb.com/{database.key}/updatebyid/{record.RecordID}";

                for (int i = 0; i < record.Columns.Length; i++)
                {
                    url += "/" + record.Columns[i];
                }
                using (WebClient client = new WebClient())
                {
                    string responseText = await client.DownloadStringTaskAsync(url);
                }
            }
        }
    }

    /// <summary>
    /// Retrieves all records stored in the remote database.
    /// </summary>
    public static async Task<List<CinchDBRecord>> GetAllRecords(CinchDBDatabase database)
    {
        List<CinchDBRecord> records = new List<CinchDBRecord>();
        string url = $"https://cinchdb.com/{database.key}/retrieve/csv";
        using (WebClient client = new WebClient())
        {
            string responseText = await client.DownloadStringTaskAsync(url);
            return GetRecordsFromDataString(responseText);
        }
    }

    /// <summary>
    /// Generates a new remote database and returns the database key. This is used by the editor code
    /// to create new databases, and I would not recommend using it at runtime.
    /// </summary>
    public static async Task<string> GetDatabaseKey()
    {
        using (WebClient client = new WebClient())
        {
            string key = await client.DownloadStringTaskAsync("https://cinchdb.com/generatekey.php");
            return key.Trim();
        }
    }

    /// <summary>
    /// Helper method which converts the raw server response into database records.
    /// </summary>
    public static List<CinchDBRecord> GetRecordsFromDataString(string responseText)
    {
        List<CinchDBRecord> records = new List<CinchDBRecord>();
        string[] lines = responseText.Split('\n');
        for (int i = 0; i < lines.Length; i++)
        {
            string[] values = lines[i].Split(',');
            if (values.Length == 11)
            {
                records.Add(new CinchDBRecord(values));
            }
        }
        return records;
    }
}

/// <summary>
/// Determines how the returned data should be sorted.
/// </summary>
public enum CinchDBDataOrderType
{
    None,
    Ascending,
    Descending
}

/// <summary>
/// Enum for describing a column in a database.
/// </summary>
public enum CinchDBColumn
{
    None,
    Column1,
    Column2,
    Column3,
    Column4,
    Column5,
    Column6,
    Column7,
    Column8,
    Column9,
    Column10
}

/// <summary>
/// Defines the column evaluator for conditions. E.g. Column1 [GreaterThan] 4.
/// </summary>
public enum CinchDBEvaluator
{
    EqualTo,
    NotEqualTo,
    GreaterThan,
    GreaterThanOrEqualTo,
    LessThan,
    LessThanOrEqualTo,
    None
}

/// <summary>
/// Defines a filter condition. A condition compares a database column with a value, using a column evaluator.
/// </summary>
public class CinchDBCondition
{
    public CinchDBColumn conditionColumn;
    public CinchDBEvaluator evaluator;
    public string value;
    public CinchDBCondition(CinchDBColumn conditionColumn, CinchDBEvaluator evaluator, string value)
    {
        this.conditionColumn = conditionColumn;
        this.evaluator = evaluator;
        this.value = value;
    }
}

/// <summary>
/// Simple class for storing the value associated with a column.
/// </summary>
public class CinchDBColumnValue
{
    public CinchDBColumn column;
    public string value;
    public CinchDBColumnValue (CinchDBColumn column, string value)
    {
        this.column = column;
        this.value = value;
    }
}

/// <summary>
/// Defines a data order, on both a primary and second column, using the specified order type (ascending/descending).
/// </summary>
public class CinchDBDataOrder
{
    public CinchDBColumn primaryOrderColumn;
    public CinchDBColumn secondaryOrderColumn;
    public CinchDBDataOrderType orderType;
    public CinchDBDataOrder(CinchDBDataOrderType orderType, CinchDBColumn primaryOrderColumn = CinchDBColumn.None, CinchDBColumn secondaryOrderColumn = CinchDBColumn.None)
    {
        this.orderType = orderType;
        this.primaryOrderColumn = primaryOrderColumn;
        this.secondaryOrderColumn = secondaryOrderColumn;
    }
}


/// <summary>
/// Contains all logic for a request to delete the contents of the database.
/// </summary>
public class CinchDBDeleteDataRequest : CinchDBDataRequest
{
    public CinchDBDeleteDataRequest(CinchDBDatabase database) : base(database) { }

    /// <summary>
    /// Executes the request, deleting all matching records.
    /// </summary>
    public async Task ExecuteRequest ()
    {
        if (conditions.Count > 0)
        {
            string url = $"https://cinchdb.com/{database.key}/delete/";
            for (int i = 0; i < conditions.Count; i++)
            {
                if (i != 0) url += " AND ";
                url += ToText(conditions[i].conditionColumn) + ToText(conditions[i].evaluator) + conditions[i].value;
            }
            using (WebClient client = new WebClient())
            {
                string data = await client.DownloadStringTaskAsync(url);
            }
        }
        else
        {
            Debug.Log("CinchDB Warning: You tried to execute a delete request without specifying a condition. " +
                "This has been prevented as it would delete the entire database. If you intended to do this, " +
                "instead use the CinchDB.ClearAllRecords() function instead.");
        }
    }
}

/// <summary>
/// Contains all of the logic for a request to update the contents of the database.
/// </summary>
public class CinchDBUpdateDataRequest : CinchDBDataRequest
{
    // The new values which should be applied to the specified columns in the matching records.
    protected List<CinchDBColumnValue> updatedColumns = new List<CinchDBColumnValue>();

    // Constructor.
    public CinchDBUpdateDataRequest(CinchDBDatabase database) : base(database) { }

    /// <summary>
    /// Define an updated value. All matching records will have the values for those columns
    /// updated to the given values.
    /// </summary>
    public void AddUpdatedValue (CinchDBColumn column, string value)
    {
        updatedColumns.Add(new CinchDBColumnValue(column, value));
    }

    /// <summary>
    /// Executes the request, updating all matching records.
    /// </summary>
    public async Task ExecuteRequest()
    {
        string url = $"https://cinchdb.com/{database.key}/update/";

        for (int i = 0; i < conditions.Count; i++)
        {
            if (i != 0) url += " AND ";
            url += ToText(conditions[i].conditionColumn) + ToText(conditions[i].evaluator) + conditions[i].value;
        }
        for (int i = 0; i < updatedColumns.Count; i++)
        {
            url += "/" + ToText(updatedColumns[i].column) + "='" + updatedColumns[i].value + "'";
        }
        using (WebClient client = new WebClient())
        {
            string data = await client.DownloadStringTaskAsync(url);
        }
    }
}

/// <summary>
/// A retieve data request collects data from a database. 
/// </summary>
public class CinchDBRetrieveDataRequest : CinchDBDataRequest
{
    // The maximum number of rows which can be returned from the query.
    protected int limit = 1000000;

    // The order in which the data should be shown.
    CinchDBDataOrder dataOrder;


    public CinchDBRetrieveDataRequest(CinchDBDatabase database) : base(database) {
        dataOrder = new CinchDBDataOrder(CinchDBDataOrderType.None);
    }

    /// <summary>
    /// Sets the number of records which can be returned from the request.
    /// </summary>
    public void SetLimit (int limit)
    {
        this.limit = limit;
    }

    /// <summary>
    /// Sets the order in which the returned rows should be shown.
    /// </summary>
    public void SetOrder(bool ascending, CinchDBColumn primarySortColumn, CinchDBColumn secondarySortColumn = CinchDBColumn.None)
    {
        dataOrder = new CinchDBDataOrder(ascending ? CinchDBDataOrderType.Ascending : CinchDBDataOrderType.Descending, primarySortColumn, secondarySortColumn);
    }

    /// <summary>
    /// Executes the data request, returning a list of all matching rows.
    /// </summary>
    public async Task<List<CinchDBRecord>> ExecuteRequest()
    {
        string url = $"https://cinchdb.com/{database.key}/retrieve/csv/";
        for (int i = 0; i < conditions.Count; i++)
        {
            if (i != 0) url += " AND ";
            url += ToText(conditions[i].conditionColumn) + ToText(conditions[i].evaluator) + conditions[i].value;
        }
        url += "/";
        if (dataOrder.orderType != CinchDBDataOrderType.None)
        {
            url += "cast(" + ToText(dataOrder.primaryOrderColumn) + " as unsigned) " + (dataOrder.secondaryOrderColumn == CinchDBColumn.None ? "" : "," + 
                " cast(" + ToText(dataOrder.secondaryOrderColumn) + " as unsigned)") + " " + (dataOrder.orderType == CinchDBDataOrderType.Ascending ? "asc" : "desc");
        }
        url += "/" + limit;
        using (WebClient client = new WebClient())
        {
            string response = await client.DownloadStringTaskAsync(url);
            return CinchDB.GetRecordsFromDataString(response);
        }
    }
}

/// <summary>
/// Base Request Class. Controls all logic behind a simple request.
/// </summary>
public abstract class CinchDBDataRequest
{
    // The database the query will run on.
    protected CinchDBDatabase database;

    // A list of conditions which should be used for the query.
    protected List<CinchDBCondition> conditions = new List<CinchDBCondition>();

    /// <summary>
    /// Base Constructor. Builds the request and assigns a database to query.
    /// </summary>
    public CinchDBDataRequest(CinchDBDatabase database)
    {
        this.database = database;
    }

    /// <summary>
    /// Adds the specified condition to the request.
    /// </summary>
    /// <param name="column">The column associated with the condition.</param>
    /// <param name="evaluator">The evaluator to run on the column.</param>
    /// <param name="value">The value the column is being checked against.</param>
    public void AddCondition(CinchDBColumn column, CinchDBEvaluator evaluator, string value)
    {
        conditions.Add(new CinchDBCondition(column, evaluator, value));
    }

    /// <summary>
    /// Helper method. Converts an enum value into the correct string value for processing.
    /// Quite messy - should improve later.
    /// </summary>
    protected static string ToText(CinchDBDataOrderType orderType)
    {
        if (orderType == CinchDBDataOrderType.Ascending)
            return "ORDER BY ";
        else if (orderType == CinchDBDataOrderType.Ascending)
            return "ORDER BY ";
        return "";
    }

    /// <summary>
    /// Helper method. Converts an enum value into the correct string value for processing.
    /// Quite messy - should improve later.
    /// </summary>
    protected static string ToText(CinchDBColumn column)
    {
        if (column == CinchDBColumn.Column1)
            return "col1";
        else if (column == CinchDBColumn.Column2)
            return "col2";
        else if (column == CinchDBColumn.Column3)
            return "col3";
        else if (column == CinchDBColumn.Column4)
            return "col4";
        else if (column == CinchDBColumn.Column5)
            return "col5";
        else if (column == CinchDBColumn.Column6)
            return "col6";
        else if (column == CinchDBColumn.Column7)
            return "col7";
        else if (column == CinchDBColumn.Column8)
            return "col8";
        else if (column == CinchDBColumn.Column9)
            return "col9";
        else
            return "col10";
    }

    /// <summary>
    /// Helper method. Converts an enum value into the correct string value for processing.
    /// Quite messy - should improve later.
    /// </summary>
    protected static string ToText(CinchDBEvaluator evaluator)
    {
        if (evaluator == CinchDBEvaluator.EqualTo)
            return "=";
        else if (evaluator == CinchDBEvaluator.NotEqualTo)
            return "!=";
        else if (evaluator == CinchDBEvaluator.GreaterThan)
            return ">";
        else if (evaluator == CinchDBEvaluator.GreaterThanOrEqualTo)
            return ">=";
        else if (evaluator == CinchDBEvaluator.LessThan)
            return "<";
        else if (evaluator == CinchDBEvaluator.LessThanOrEqualTo)
            return "<=";
        return "ERROR";
    }
}