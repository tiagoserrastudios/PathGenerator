using UnityEngine;


public class PathGenerator : MonoBehaviour
{
    #region Variables
    [Header("Required References")]
    [SerializeField] private Transform pointsParent; // Assign in the inspector
    [SerializeField] private GameObject segmentPrefab; // Assign in the inspector
    [SerializeField] private Transform cloneHolder;  // Assign in the inspector

    private Transform pointsHolder; 
    private Transform[] points;
    private Vector3[] previousPointPositions;
    
    [Header("Path Customization")]
    public int clonesBetweenPoints = 1;
    public float customSpacing = 5f;
    #endregion

    private void Start()
    {
        // Validate references
        if (!ValidateReferences())
        {
            Debug.LogError("References not properly set on the PathGenerator component.");
            return; // Exit early if references are missing
        }

        pointsHolder = pointsParent.GetChild(0);

        Points pointsComponent = pointsParent.GetComponentInChildren<Points>();
        if (pointsComponent != null)
        {
            pointsComponent.onPointsChanged += GeneratePath;
        }
        else
        {
            Debug.LogWarning("No 'Points' component found on the children of pointsParent!");
        }

        GeneratePath(); 
    }

    private void OnDestroy()
    {
        Points pointsComponent = pointsParent.GetComponentInChildren<Points>();
        if (pointsComponent != null)
        {
            pointsComponent.onPointsChanged -= GeneratePath;
        }
    }

    void GeneratePath()
    {
        if (!ValidateReferences()) return;
        ClearExistingPathSegments();
        CachePoints();

        for (int i = 0; i < points.Length; i++)
        {
            Transform start = points[i];
            Transform end = GetNextPoint(i);
            Vector3 direction = (end.position - start.position).normalized;
            float segmentDistance = Vector3.Distance(start.position, end.position);
            int segments = Mathf.Max(1, Mathf.FloorToInt(segmentDistance / customSpacing)); // Dynamic segment count

            Vector3 prevPoint = start.position;
            for (int j = 1; j <= segments; j++)
            {
                Vector3 currentPoint = start.position + direction * (customSpacing * j);
                CreatePathSegment(prevPoint, currentPoint);
                prevPoint = currentPoint;
            }
        }
    }
    
    void CachePoints()
    {
        points = pointsParent.GetComponentsInChildren<Transform>();
        previousPointPositions = new Vector3[points.Length]; 
    }

    bool ValidateReferences()
    {
        return pointsParent != null && segmentPrefab != null && cloneHolder != null;
    }

    Transform GetNextPoint(int currentIndex)
    {
        return points[(currentIndex + 1) % points.Length];
    }

    void CreatePathSegment(Vector3 start, Vector3 end)
    {
        GameObject segment = Instantiate(segmentPrefab, cloneHolder);
        segment.transform.position = start;
        segment.transform.LookAt(end);
        segment.transform.localScale = new Vector3(Vector3.Distance(start, end), 1f, 1f);
    }

    void ClearExistingPathSegments()
    {
        while (cloneHolder.childCount > 0) 
        {
            DestroyImmediate(cloneHolder.GetChild(0).gameObject);
        }
    }

    bool HavePointPositionsChanged()
    {
        if (points == null || previousPointPositions == null || points.Length != previousPointPositions.Length)
            return true; 
        
        for (int i = 0; i < points.Length; i++)
        {
            if (points[i].position != previousPointPositions[i])
                return true; 
        }
        return false; 
    }

    void StorePointPositions()
    {
        for (int i = 0; i < points.Length; i++)
        {
            previousPointPositions[i] = points[i].position;
        }
    }

    int GetNextPointIndex(int currentIndex)
    {
        return (currentIndex + 1) % points.Length;
    }
    
    void OnDrawGizmos()
    {
        if (!ValidateReferences()) return; 
        CachePoints();
        if (points.Length < 2) return;
        Gizmos.color = Color.yellow;
        for (int i = 0; i < points.Length; i++)
        {
            Transform startPoint = points[i];
            Transform endPoint = GetNextPoint(i);
            int segments = 20; 
            Vector3 prevPoint = startPoint.position;
            for (int j = 1; j <= segments; j++)
            {
                float t = (float)j / segments;
                Vector3 currentPoint = Vector3.Lerp(startPoint.position, endPoint.position, t);
                Gizmos.DrawLine(prevPoint, currentPoint);
                prevPoint = currentPoint;
            }
        }
    }
}
