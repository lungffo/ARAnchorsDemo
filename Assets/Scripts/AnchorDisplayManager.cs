using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.XR.ARFoundation;
using Microsoft.Azure.SpatialAnchors.Unity;
using Microsoft.Azure.SpatialAnchors;
using System.Threading.Tasks;
using System;
using UnityEngine.SceneManagement;

/// <summary>
/// The AnchorDisplayManager controls displaying content to the user.
/// </summary>
public class AnchorDisplayManager : MonoBehaviour
{
    // Cached References.
    public CinchDBDatabase anchorDatabase;
    public TextMeshProUGUI log;

    // Azure Spatial Anchors Setup
    private static SpatialAnchorManager manager;
    private static AnchorLocateCriteria criteria;
    private static CloudSpatialAnchorWatcher watcher;
    private static ARRaycastManager arRaycastManager;

    // Stored list of all tracked anchors and all located anchors.
    private static Dictionary<string, GameObject> anchors = new Dictionary<string, GameObject>();
    private static Dictionary<string, AnchorContent> locatedAnchors = new Dictionary<string, AnchorContent>();

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
        manager.AnchorLocated += OnAnchorLocated;
        SetupASASession();
    }

    /// <summary>
    /// Unity Callback: Called on every frame. Handle content visibility based on distance here.
    /// </summary>
    private void Update()
    {
        foreach (KeyValuePair<string, AnchorContent> entry in locatedAnchors)
        {
            float distanceToAnchor = Vector3.Distance(entry.Value.gameObject.transform.position, Camera.main.transform.position);
            entry.Value.gameObject.SetActive(distanceToAnchor < entry.Value.visabilityDistance);
        }
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
    /// Actions to perform when a spatial anchor is located.
    /// </summary>
    protected virtual void OnAnchorLocated(object sender, AnchorLocatedEventArgs args)
    {
        UnityDispatcher.InvokeOnAppThread(() =>
        {
            string anchorID = args.Anchor.Identifier;
            Pose anchorPose = args.Anchor.GetPose();

            Log("Found " + args.Anchor.Identifier);

            Handheld.Vibrate();
            anchors[anchorID].gameObject.SetActive(true);
            anchors[anchorID].transform.position = anchorPose.position;
            anchors[anchorID].transform.rotation = anchorPose.rotation;

            //locatedAnchors.Add(anchorID, anchors[anchorID].GetComponent<AnchorContent>());
            if (anchors[anchorID].GetComponent<CloudNativeAnchor>() == null)
                anchors[anchorID].AddComponent<CloudNativeAnchor>();
        });
    }

    /// <summary>
    /// Perform all initial setup actions, starting the appropriate watcher.
    /// </summary>
    public async void SetupASASession()
    {
        // Do initial loading of the spatial anchors system. For whatever reason, Azure needs time to do this and if the
        // hardcoded waits aren't added, it will fail. Likely something on Azure's end. Very weird.
        Log("Loading ASA.");
        await Task.Delay(5000);
        await manager.CreateSessionAsync();
        Log("Created Session.");
        await Task.Delay(2000);
        await manager.StartSessionAsync();
        Log("Started Session.");
        await Task.Delay(2000);

        // Compile a distionary of all anchors which need to be tracked in the form <string: anchorID, GameObject: content>.
        // Ignore any anchors which have not been given an ID, as this will cause the tracking to fail on the Azure end.
        // If any duplicate IDs are found, only the first will be tracked (inform the user if this occurs).
        AnchorContent[] anchorContentObjects = GameObject.FindObjectsOfType<AnchorContent>(true);
        List<string> anchorsToFind = new List<string>();
        foreach (AnchorContent anchorContentObject in anchorContentObjects)
        {
            if (anchorContentObject.anchorID.Length > 0 && !anchorsToFind.Contains(anchorContentObject.anchorID))
            {
                anchorsToFind.Add(anchorContentObject.anchorID);

                if (anchors.ContainsKey(anchorContentObject.anchorID))
                    anchors[anchorContentObject.anchorID] = anchorContentObject.gameObject;
                else
                    anchors.Add(anchorContentObject.anchorID, anchorContentObject.gameObject);
            }
            else if (anchorsToFind.Contains(anchorContentObject.anchorID))
            {
                Log("Warning: Multiple objects have the same anchor ID!");
            }
            else
            {
                Log("Warning: No Anchor ID attached to content!");
            }
        }

        // If at least one anchor needs to be tracked, create the criteria and start the watcher.
        if (anchorsToFind.Count > 0)
        {
            AnchorLocateCriteria criteria = new AnchorLocateCriteria();
            criteria.BypassCache = true;
            criteria.Strategy = LocateStrategy.VisualInformation;
            criteria.Identifiers = anchorsToFind.ToArray();
            watcher = manager.Session.CreateWatcher(criteria);
            Log("Created Watcher");
            Log(anchorsToFind[0]);
        }
    }

    /// <summary>
    /// Handles some basic logging. Nothing too fancy. If you need this, I would write your
    /// own expanded logging system instead.
    /// </summary>
    public void Log (string text)
    {
        log.text = text;
    }

    public void LoadMenuScene()
    {
        SceneManager.LoadScene("SceneSelector");
    }
}
