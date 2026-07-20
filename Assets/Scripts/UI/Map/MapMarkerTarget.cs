using UnityEngine;

public class MapMarkerTarget : MonoBehaviour
{
    [SerializeField] private string markerID;
    [SerializeField] private MapMarkerType markerType;
    [SerializeField] private bool showOnMinimap = true;
    [SerializeField] private bool showOnFullMap = true;
    [SerializeField] private bool markerActive = true;

    public string MarkerID => markerID;
    public MapMarkerType MarkerType => markerType;
    public bool ShowOnMinimap => showOnMinimap;
    public bool ShowOnFullMap => showOnFullMap;
    public bool IsActive => markerActive && gameObject.activeInHierarchy;
    public Vector3 WorldPosition => transform.position;

    public void SetMarkerActive(bool value)
    {
        markerActive = value;
    }

    public void Configure(
        string configuredMarkerID,
        MapMarkerType configuredMarkerType,
        bool minimap = true,
        bool fullMap = true)
    {
        markerID = configuredMarkerID;
        markerType = configuredMarkerType;
        showOnMinimap = minimap;
        showOnFullMap = fullMap;
        markerActive = true;
    }

    public static MapMarkerTarget FindByID(string id)
    {
        if (string.IsNullOrWhiteSpace(id))
            return null;

        MapMarkerTarget[] targets =
            FindObjectsByType<MapMarkerTarget>(
                FindObjectsSortMode.None
            );

        foreach (MapMarkerTarget target in targets)
        {
            if (target.MarkerID == id)
                return target;
        }

        return null;
    }
}
