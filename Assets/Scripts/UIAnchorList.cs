using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// The UIAnchorList allows the user to see all created anchors and edit the attached data if needed.
/// </summary>
public class UIAnchorList : MonoBehaviour
{
    public CinchDBDatabase anchorDatabase;

    [Header("Cached References")]
    public Transform anchorButtonContainer;
    public GameObject anchorButtonTemplate;
    public GameObject anchorDataWindow;
    public TMP_InputField anchorNameInput, anchorDescriptionInput, anchorIDInput, anchorCreationTimeInput;

    // A pool of all available anchor buttons.
    private static List<Button> anchorButtons = new List<Button>();

    // A list of all anchor records being displayed.
    private static List<CinchDBRecord> records = new List<CinchDBRecord>();

    // The current anchor record ID being edited.
    private static int currentID = -1;

    /// <summary>
    /// When the UI anchor list is opened, populate the list of anchors.
    /// </summary>
    private void OnEnable()
    {
        RefreshAnchorButtons();
    }

    /// <summary>
    /// Refresh the list of visible anchor buttons.
    /// </summary>
    public async void RefreshAnchorButtons()
    {
        // Hide all anchor buttons.
        foreach (Button anchorButton in anchorButtons)
        {
            anchorButton.gameObject.SetActive(false);
            anchorButton.onClick.RemoveAllListeners();
        }

        // Retrieve a list of all anchors.
        records = await CinchDB.GetAllRecords(anchorDatabase);

        // Create more anchor buttons if needed.
        while (records.Count > anchorButtons.Count)
        {
            anchorButtons.Add(Instantiate(anchorButtonTemplate, anchorButtonContainer).GetComponent<Button>());
        }

        // Enable the necessary anchor buttons, name them appropriately and link
        // them to their appropriate data, so it will be displayed when clicked.
        for (int i = 0; i < records.Count; i++)
        {
            anchorButtons[i].gameObject.SetActive(true);
            anchorButtons[i].GetComponentInChildren<TextMeshProUGUI>().text = records[i].Columns[1];
            int temp = i;
            anchorButtons[i].onClick.AddListener(() => LoadButton(temp));
        }
    }

    /// <summary>
    /// Load and display the data associated with a particular anchor record.
    /// </summary>
    public void LoadButton (int id)
    {
        currentID = id;
        anchorDataWindow.SetActive(true);
        anchorNameInput.text = records[id].Columns[1];
        anchorDescriptionInput.text = records[id].Columns[2];
        anchorCreationTimeInput.text = records[id].Columns[3];
        anchorIDInput.text = records[id].Columns[0];
    }

    /// <summary>
    /// Confirm all of the changes made to an anchor record and upload it to the database.
    /// </summary>
    public async void OnConfirmChangesPressed ()
    {
        anchorDataWindow.SetActive(false);
        CinchDBRecord record = records[currentID];
        record.Columns[1] = anchorNameInput.text;
        record.Columns[2] = anchorDescriptionInput.text;
        anchorButtons[currentID].GetComponentInChildren<TextMeshProUGUI>().text = record.Columns[1];
        await CinchDB.UpdateRecords(anchorDatabase, record);
    }
}
