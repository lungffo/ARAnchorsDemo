using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.XR.ARFoundation;
using Microsoft.Azure.SpatialAnchors.Unity;
using Microsoft.Azure.SpatialAnchors;
using System.Threading.Tasks;
using System;
using UnityEngine.UI;

/// <summary>
/// The AnchorPlacementManager script handles the placement of new spatial anchors.
/// </summary>
public class AnchorPlacementManager : MonoBehaviour
{
    [Header("Anchor Database. Controls where anchors are saved.")]
    public CinchDBDatabase anchorDatabase;

    [Header("Anchor Placement Stages - UI Components")]
    public GameObject stage0_NotPlacingAnchorComponents;
    public GameObject stage1_EnteringAnchorDataComponents;
    public GameObject stage2_PlacingAnchorComponents;
    public GameObject stage3_ScanningEnvironmentComponents;
    public GameObject stage4_UploadingAnchorComponents;

    [Header("Stage 1 - Entering Anchor Data")]
    public TMP_InputField anchorTitleText;
    public TMP_InputField anchorDescriptionText;
    public Button anchorDataConfirmButton;

    [Header("Stage 2 - Placing Anchor")]
    public GameObject anchorPlaceholder;
    private CloudNativeAnchor cloudNativeAnchor;

    [Header("Stage 3 - Scanning Environment")]
    public TextMeshProUGUI scanningText;
    public GameObject scanningPanel;
    public GameObject uploadAnchorButton;

    [Header("Debug Log UI")]
    public TextMeshProUGUI log;

    // Azure Spatial Anchors Setup
    public static SpatialAnchorManager manager;
    public static AnchorLocateCriteria criteria;
    public static CloudSpatialAnchorWatcher watcher;
    public static ARRaycastManager arRaycastManager;

    // The current stage the user is at in the creation process.
    // 0 = NOT CREATING ANCHOR.
    // 1 = ENTERING ANCHOR DATA.
    // 2 = SELECTING ANCHOR LOCATION.
    // 3 = SCANNING ENVIRONMENT.
    // 4 = UPLOADING ANCHOR.
    [HideInInspector] public int currentStage = 0;

    // Is the anchor currently being rotated?
    public static bool isRotatingPlacementMarker;

    // What is the current rotation offset of the anchor?
    public static float rotationOffset;

    /// <summary>
    /// Unity Callback: Called when the object is first loaded/created.
    /// </summary>
    private void Awake()
    {
        Application.targetFrameRate = 60;
    }

    /// <summary>
    /// Unity Callback: Called on the first frame after the attached object is loaded/created.
    /// </summary>
    void Start()
    {
        arRaycastManager = FindObjectOfType<ARRaycastManager>();
        manager = FindObjectOfType<SpatialAnchorManager>();
        SetupASASession();
    }

    /// <summary>
    /// Unity Callback: Called on every frame.
    /// </summary>
    void Update()
    {
        if (isRotatingPlacementMarker) rotationOffset += 100.0f * Time.deltaTime;
        if (rotationOffset > 360) rotationOffset -= 360;
        ProcessStageUpdate();
        
    }

    /// <summary>
    /// Unity Callback: Called when the application finishes, stop the session.
    /// </summary>
    private void OnDestroy()
    {
        if (watcher != null) watcher.Stop();
        if (manager != null) manager.StopSession();
    }

    /// <summary>
    /// Handle all per-frame updates required to process the creation of an anchor.
    /// </summary>
    public void ProcessStageUpdate()
    {
        // The anchor placement object only appears after stage 2 of the creation process.
        anchorPlaceholder.SetActive(currentStage >= 2);

        // Only allow the user to submit the data if they have given the anchor a name.
        if (currentStage == 1)
        {
            anchorDataConfirmButton.interactable = anchorTitleText.text.Length > 0;
        }

        // If in the second stage (selecting the anchor location), raycast from the center
        // of the screen to find the location of the anchor.
        if (currentStage == 2)
        {
            Vector2 position = new Vector2(Screen.width / 2, Screen.height / 2);
            List<ARRaycastHit> aRRaycastHits = new List<ARRaycastHit>();
            if (arRaycastManager.Raycast(position, aRRaycastHits) && aRRaycastHits.Count > 0)
            {
                anchorPlaceholder.SetActive(true);
                anchorPlaceholder.transform.position = aRRaycastHits[0].pose.position;
                anchorPlaceholder.transform.rotation = aRRaycastHits[0].pose.rotation;
                Vector3 euler = anchorPlaceholder.transform.rotation.eulerAngles;
                euler.y += rotationOffset;
                anchorPlaceholder.transform.rotation = Quaternion.Euler(euler);
            }
            else
            {
                anchorPlaceholder.SetActive(false);
            }
        }

        // If insufficient environment data has been captured, show the capture percentage.
        // Otherwise, show the upload button to allow the user to upload when they're ready.
        else if (currentStage == 3)
        {
            scanningText.text = "Scan the Area (" + (int)(manager.SessionStatus.RecommendedForCreateProgress * 100.0f) + "%)";
            if (manager.SessionStatus.RecommendedForCreateProgress >= 1)
            {
                scanningPanel.SetActive(false);
                uploadAnchorButton.SetActive(true);
            }
            else
            {
                scanningPanel.SetActive(true);
                uploadAnchorButton.SetActive(false);
            }
        }
    }


    /// <summary>
    /// Go to the next stage of the anchor placement, showing appropriate new controls.
    /// </summary>
    public void GoToNextStage ()
    {
        currentStage++;
        Redraw();
    }

    /// <summary>
    /// Go back to the previous stage of anchor placement, allowing the user to update the
    /// data and/or anchor location.
    /// </summary>
    public void GoToPreviousStage ()
    {
        currentStage--;
        Redraw();
    }

    /// <summary>
    /// Redraw will show the appropriate UI elements for the current anchor placement stage.
    /// </summary>
    public void Redraw ()
    {
        stage0_NotPlacingAnchorComponents.SetActive(currentStage == 0);
        stage1_EnteringAnchorDataComponents.SetActive(currentStage == 1);
        stage2_PlacingAnchorComponents.SetActive(currentStage == 2);
        stage3_ScanningEnvironmentComponents.SetActive(currentStage == 3);
        stage4_UploadingAnchorComponents.SetActive(currentStage == 4);
    }

    /// <summary>
    /// Perform all initial setup actions, starting the appropriate watcher.
    /// </summary>
    public async void SetupASASession()
    {
        await Task.Delay(5000);
        await manager.CreateSessionAsync();
        await Task.Delay(2000);
        await manager.StartSessionAsync();
        cloudNativeAnchor = anchorPlaceholder.AddComponent<CloudNativeAnchor>();
    }

    /// <summary>
    /// Uploads the anchor currently being created into the cloud.
    /// </summary>
    public async void UploadAnchorToCloud()
    {
        // If the cloud portion of the anchor hasn't been created yet, create it
        if (cloudNativeAnchor.CloudAnchor == null)
        {
            await cloudNativeAnchor.NativeToCloud();
        }

        // Get the cloud portion of the anchor
        CloudSpatialAnchor cloudAnchor = cloudNativeAnchor.CloudAnchor;
        cloudAnchor.Expiration = DateTimeOffset.Now.AddDays(100);

        // Loop while the anchor isn't ready to create.
        while (!manager.IsReadyForCreate)
        {
            await Task.Delay(300);
            float createProgress = manager.SessionStatus.RecommendedForCreateProgress;
        }

        // Try to upload the anchor and save it to the database.
        try
        {
            await manager.CreateAnchorAsync(cloudAnchor);
            await SaveAnchorToDatabase();
            GameObject.Destroy(cloudNativeAnchor);
            cloudNativeAnchor = anchorPlaceholder.AddComponent<CloudNativeAnchor>();
        } catch { }
        currentStage = 0;
        Redraw();
    }

    /// <summary>
    /// Save the anchor to the database.
    /// </summary>
    public async Task SaveAnchorToDatabase()
    {
        CinchDBRecord record = new CinchDBRecord();
        record.Columns[0] = cloudNativeAnchor.CloudAnchor.Identifier;
        record.Columns[1] = anchorTitleText.text;
        record.Columns[2] = anchorDescriptionText.text;
        record.Columns[3] = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        await CinchDB.AddRecords(anchorDatabase, record);        
    }

    /// <summary>
    /// Handles some basic logging. Nothing too fancy. If you need this, I would write your
    /// own expanded logging system instead.
    /// </summary>
    public void Log(string text)
    {
        log.text = text;
    }

    /// <summary>
    /// Begin rotating the anchor placement marker.
    /// </summary>
    public void BeginAnchorPlacementRotation()
    {
        isRotatingPlacementMarker = true;
    }

    /// <summary>
    /// Stop rotating the anchor placement marker.
    /// </summary>
    public void EndAnchorPlacementRotation()
    {
        isRotatingPlacementMarker = false;
    }

    public void LoadMenuScene()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene("SceneSelector");
    }
}
