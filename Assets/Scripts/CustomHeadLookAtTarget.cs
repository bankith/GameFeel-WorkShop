using UnityEngine;
using DG.Tweening; // Required for DOTween

[ExecuteAlways]
public class CustomHeadLookAtTarget : MonoBehaviour
{
    [Header("Targeting")]
    public Transform Target;
    [Range(0, 1)] public float weight = 0f; // 0 = Animator, 1 = Target

    [Header("Lock Settings")]
    public bool LockRotationX;
    public bool LockRotationY;
    public bool LockRotationZ;

    private Vector3 m_InitialRotation;
    private Tween m_WeightTween;

    void Awake()
    {
        // Store the "Home" rotation if the Animator isn't playing
        m_InitialRotation = transform.localRotation.eulerAngles;
    }

    // Call this from your Hover script!
    public void SetTarget(Transform newTarget)
    {
        Target = newTarget;
        
        m_WeightTween?.Kill();
        // If Target is null, fade out. If we have a target, fade in.
        float targetWeight = (newTarget != null) ? 1f : 0f;
        
        m_WeightTween = DOTween.To(() => weight, x => weight = x, targetWeight, 0.5f)
            .SetEase(Ease.OutCubic);
    }

    void LateUpdate() // CRITICAL: Must be LateUpdate to override the Animator
    {
        if (weight <= 0) return;

        // 1. Capture the rotation the Animator just gave us
        Quaternion animatorRotation = transform.rotation;

        // 2. Calculate the LookAt rotation
        if (Target != null)
        {
            Vector3 direction = Target.position - transform.position;
            if (direction != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(direction, transform.root.up);

                // Handle Axis Locking
                if (LockRotationX || LockRotationY || LockRotationZ)
                {
                    var euler = targetRotation.eulerAngles;
                    var initial = animatorRotation.eulerAngles;
                    euler.x = LockRotationX ? initial.x : euler.x;
                    euler.y = LockRotationY ? initial.y : euler.y;
                    euler.z = LockRotationZ ? initial.z : euler.z;
                    targetRotation = Quaternion.Euler(euler);
                }

                // 3. Blend between Animator and Target based on weight
                transform.rotation = Quaternion.Slerp(animatorRotation, targetRotation, weight);
            }
        }
    }
}