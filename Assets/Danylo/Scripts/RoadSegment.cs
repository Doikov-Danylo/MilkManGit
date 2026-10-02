using UnityEngine;

public class RoadSegment : MonoBehaviour
{
    [SerializeField] private Transform endPoint;

    // Public accessor to get the socket transform
    public Transform EndPoint => endPoint != null ? endPoint : transform;
}