using System;
using UnityEngine;
using DG.Tweening;
using UnityEngine.Animations.Rigging;

public class TargetHover : MonoBehaviour
{
    private Outline _outline;
    [SerializeField] private float _targetWidth = 6.2f;
    [SerializeField] private float _duration = 0.25f;

    [SerializeField] private CustomHeadLookAtTarget customHeadLookAtTarget;
    [SerializeField] private Boolean isEnableDotween = false;
    void Awake()
    {
        _outline = GetComponent<Outline>();
        
        if (_outline != null)
        {
            _outline.OutlineWidth = 0;
        }

        if (customHeadLookAtTarget == null)
        {
            customHeadLookAtTarget = FindAnyObjectByType<CustomHeadLookAtTarget>();
        }
    }

    private void OnMouseEnter()
    {
        if (_outline == null) return;

        if (!isEnableDotween)
        {
            _outline.OutlineWidth = _targetWidth;
            return;
        }
        
        DOTween.Kill(_outline);
        DOTween.To(() => _outline.OutlineWidth, x => _outline.OutlineWidth = x, _targetWidth, _duration)
            .SetEase(Ease.OutCubic);
        
        customHeadLookAtTarget.Target = this.transform;
        DOTween.To(() => customHeadLookAtTarget.weight, x => customHeadLookAtTarget.weight = x, 1f, 0.5f)
        .SetEase(Ease.OutCubic);
    }

    private void OnMouseExit()
    {
        if (_outline == null) return;
        
        if (!isEnableDotween)
        {
            _outline.OutlineWidth = 0;
            return;
        }

        DOTween.Kill(_outline);
        DOTween.To(() => _outline.OutlineWidth, x => _outline.OutlineWidth = x, 0f, _duration)
            .SetEase(Ease.InCubic);
        
        DOTween.To(() => customHeadLookAtTarget.weight, x => customHeadLookAtTarget.weight = x, 0f, 0.5f)
            .SetEase(Ease.OutCubic);
    }
}