using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// AnchorContent will only be displayed when the "anchorID" is detected by the spatial anchors system.
/// It will then be tracked via the AR SLAM and positioned correctly automatically. If the user gets
/// too far away from the content (controlled by the visabilityDistance (m) variable), the content will
/// disappear until the user gets closer again. Set this to a very high value if you do not want the
/// content to disappear.
/// </summary>
public class AnchorContent : MonoBehaviour
{
    public string anchorID;
    public float visabilityDistance = 10;
}
