using UnityEngine;
using DG.Tweening;

public class MoveWithDOTween : MonoBehaviour
{
    [Header("Target Positions")]
    public Vector3 pointA;
    public Vector3 pointB;

    [Header("Animation Settings")]
    public float duration = 2f;
    public Ease easeType = Ease.InOutSine;
    void Start()
    {
        StartMovement();
    }

    public void StartMovement()
    {
        transform.position = pointA;
        transform.DOMove(pointB, duration).SetEase(easeType);
    }
}