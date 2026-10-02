using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlopeTrackGenerator : MonoBehaviour
{
    [Header("Prefabs & Tracking")]
    [Tooltip("Add your 3 road segment prefabs here (Element 0 will be the clean starting piece)")]
    [SerializeField] private RoadSegment[] roadPrefabs = new RoadSegment[3];
    [SerializeField] private Transform bikeTransform;

    [Header("Spawn Settings")]
    [SerializeField] private int activeSegmentsCount = 15;
    [SerializeField] private float segmentLength = 20f;

    [Header("Slope Progression")]
    [Tooltip("Initial downward pitch in degrees")]
    [SerializeField] private float minPitchAngle = 5f;
    [Tooltip("Maximum downward pitch in degrees")]
    [SerializeField] private float maxPitchAngle = 55f;
    [Tooltip("Meters traveled to reach max steepness")]
    [SerializeField] private float distanceToMaxSlope = 2000f;

    private readonly Queue _activeSegments = new Queue();
    private Transform _lastEndPoint;
    private float _totalDistanceTravelled = 0f;

    private void Start()
    {
        if (roadPrefabs == null || roadPrefabs.Length == 0)
        {
            Debug.LogError("[SlopeTrackGenerator] Assign your road prefabs in the Inspector!", this);
            return;
        }

        // Spawn initial runway
        for (int i = 0; i < activeSegmentsCount; i++)
        {
            SpawnNextSegment(isFirstRunway: i < 3);
        }
    }

    private void Update()
    {
        if (bikeTransform == null || _lastEndPoint == null) return;

        float distanceToEnd = Vector3.Distance(bikeTransform.position, _lastEndPoint.position);
        float safeBufferDistance = segmentLength * 6f;

        if (distanceToEnd < safeBufferDistance)
        {
            SpawnNextSegment(isFirstRunway: false);
        }

        CleanUpOldSegmentsBehindBike();
    }

    private void SpawnNextSegment(bool isFirstRunway)
    {
        // Pick prefab: force element 0 for initial pieces, otherwise pick randomly from the 3
        RoadSegment selectedPrefab;
        if (isFirstRunway || roadPrefabs.Length == 1)
        {
            selectedPrefab = roadPrefabs[0];
        }
        else
        {
            int randomIndex = Random.Range(0, roadPrefabs.Length);
            selectedPrefab = roadPrefabs[randomIndex];
        }

        if (selectedPrefab == null)
        {
            Debug.LogWarning("[SlopeTrackGenerator] A slot in roadPrefabs array is empty!");
            return;
        }

        // Calculate slope pitch
        float progress = Mathf.Clamp01(_totalDistanceTravelled / distanceToMaxSlope);
        float currentAngle = Mathf.Lerp(minPitchAngle, maxPitchAngle, progress);
        Quaternion targetRotation = Quaternion.Euler(currentAngle, 0f, 0f);

        Vector3 spawnPosition = (_lastEndPoint == null) ? Vector3.zero : _lastEndPoint.position;

        RoadSegment newSegment = Instantiate(selectedPrefab, spawnPosition, targetRotation);

        if (_lastEndPoint == null)
        {
            newSegment.transform.position = Vector3.zero;
        }

        newSegment.transform.SetParent(transform, true);
        _activeSegments.Enqueue(newSegment);

        _lastEndPoint = newSegment.EndPoint;
        _totalDistanceTravelled += segmentLength;
    }

    private void CleanUpOldSegmentsBehindBike()
    {
        if (_activeSegments.Count <= activeSegmentsCount) return;

        RoadSegment oldest = _activeSegments.Peek() as RoadSegment;
        if (oldest == null)
        {
            _activeSegments.Dequeue();
            return;
        }

        if (bikeTransform.position.z > oldest.EndPoint.position.z + 25f)
        {
            RoadSegment removed = _activeSegments.Dequeue() as RoadSegment;
            Destroy(removed.gameObject);
        }
    }
}