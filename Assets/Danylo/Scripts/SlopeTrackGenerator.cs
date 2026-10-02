using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlopeTrackGenerator : MonoBehaviour
{
    [Header("Prefabs & Tracking")]
    [SerializeField] private RoadSegment roadSegmentPrefab;
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

    private readonly Queue<RoadSegment> _activeSegments = new Queue<RoadSegment>();
    private Transform _lastEndPoint;
    private float _totalDistanceTravelled = 0f;

    private void Start()
    {
        if (roadSegmentPrefab == null)
        {
            Debug.LogError("[SlopeTrackGenerator] Assign roadSegmentPrefab in the Inspector!", this);
            return;
        }

        // Generate the initial runway
        for (int i = 0; i < activeSegmentsCount; i++)
        {
            SpawnNextSegment();
        }
    }

    private void Update()
    {
        if (bikeTransform == null || _lastEndPoint == null) return;

        // Keep a minimum buffer of segments ahead of the bike
        float distanceToEnd = Vector3.Distance(bikeTransform.position, _lastEndPoint.position);
        float safeBufferDistance = segmentLength * 6f; // Always keep ~6 segments ahead

        if (distanceToEnd < safeBufferDistance)
        {
            SpawnNextSegment();
        }

        // Only cleanup segments that are WELL behind the bike
        CleanUpOldSegmentsBehindBike();
    }

    private void SpawnNextSegment()
    {
        float progress = Mathf.Clamp01(_totalDistanceTravelled / distanceToMaxSlope);
        float currentAngle = Mathf.Lerp(minPitchAngle, maxPitchAngle, progress);
        Quaternion targetRotation = Quaternion.Euler(currentAngle, 0f, 0f);

        Vector3 spawnPosition = (_lastEndPoint == null) ? Vector3.zero : _lastEndPoint.position;

        RoadSegment newSegment = Instantiate(roadSegmentPrefab, spawnPosition, targetRotation);

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
        // Don't cull if we have too few segments
        if (_activeSegments.Count <= activeSegmentsCount) return;

        RoadSegment oldest = _activeSegments.Peek();
        if (oldest == null)
        {
            _activeSegments.Dequeue();
            return;
        }

        // Safe cleanup: only destroy when the bike has traveled at least 25m PAST the segment's end
        if (bikeTransform.position.z > oldest.EndPoint.position.z + 25f)
        {
            _activeSegments.Dequeue();
            Destroy(oldest.gameObject);
        }
    }
}