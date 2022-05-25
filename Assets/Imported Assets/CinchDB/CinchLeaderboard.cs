using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class CinchScore
{
    public string name;
    public float score;

}
public class CinchLeaderboard
{
    private static CinchDBDatabase database;

    public static async Task AddScore (string name, string value)
    {
        ValidateDatabase();
        await CinchDB.AddRecords(database, new CinchDBRecord(name, value));
    }

    public static async Task<List<CinchScore>> GetScores (int count)
    {
        ValidateDatabase();
        CinchDBRetrieveDataRequest request = new CinchDBRetrieveDataRequest(database);
        request.SetLimit(count);
        request.SetOrder(false, CinchDBColumn.Column2);
        List<CinchDBRecord> records = await request.ExecuteRequest();

        List<CinchScore> scores = new List<CinchScore>();
        foreach (CinchDBRecord record in records)
        {
            CinchScore score = new CinchScore();
            score.name = record.Columns[0];
            score.score = float.Parse(record.Columns[1]);
            scores.Add(score);
        }
        return scores;
    }

    public static async Task<float> GetHighestScoreByPlayer (string playerName)
    {
        ValidateDatabase();
        CinchDBRetrieveDataRequest request = new CinchDBRetrieveDataRequest(database);
        request.SetLimit(1);
        request.SetOrder(false, CinchDBColumn.Column2);
        request.AddCondition(CinchDBColumn.Column1, CinchDBEvaluator.EqualTo, playerName);
        List<CinchDBRecord> records = await request.ExecuteRequest();

        if (records.Count == 0)
            return 0;
        else
        {
            return float.Parse(records[0].Columns[1]);
        }
    }

    private static void ValidateDatabase ()
    {
        if (database == null)
        {
            database = Resources.LoadAll<CinchDBDatabase>("")[0];
            if (Resources.LoadAll<CinchDBDatabase>("").Length > 0)
            {
                Debug.Log("You have more than one database in this project. For the simple scoreboard to work, there can only be one database in the project.");
            }
        }
    }
}
