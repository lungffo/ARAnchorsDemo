using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public class ScoreboardExample : MonoBehaviour
{
    [Header("Scoreboard Properties")]
    public CinchDBDatabase database;
    public int maxScoresToShow = 10;

    [Header("UI Components")]
    public Text playerNameContent;
    public Text playerScoreContent;
    public InputField scoreNameInput;
    public InputField scoreValueInput;

    // Start is called before the first frame update
    void Start()
    {
        LoadScoreboard();
    }

    public async void LoadScoreboard ()
    {
        CinchDBRetrieveDataRequest request = new CinchDBRetrieveDataRequest(database);
        request.SetLimit(maxScoresToShow);
        request.SetOrder(false, CinchDBColumn.Column2);
        List<CinchDBRecord> records = await request.ExecuteRequest();

        playerNameContent.text = "";
        playerScoreContent.text = "";
        foreach (CinchDBRecord record in records)
        {
            playerNameContent.text += record.Columns[0] + Environment.NewLine;
            playerScoreContent.text += record.Columns[1] + Environment.NewLine;
        }
    }

    public async void AddScore ()
    {
        string scoreName = scoreNameInput.text;
        string scoreValue = scoreValueInput.text;
        scoreNameInput.text = "";
        scoreValueInput.text = "";
        if (scoreName.Length > 0 && scoreValue.Length > 0)
        {
            await database.AddRecord(scoreName, scoreValue);
            LoadScoreboard();
        }
    }

    public async void Example()
    {
        List<CinchDBRecord> records = await database.GetAllRecords();
        // Do things with the database contents.
    }
}
