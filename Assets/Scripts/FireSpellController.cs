using UnityEngine;
using System.Threading.Tasks;
using UnityEngine;
using DG.Tweening;
using UnityEngine.Serialization;
using UnityEngine.Animations.Rigging;
using UnityEngine.InputSystem;


public class FireSpellController : MonoBehaviour
{

    [Header("Prefabs")] [SerializeField] private GameObject prefabToSpawn;
    [SerializeField] private GameObject previewPrefab;

    [Header("Settings")]
    [SerializeField] private LayerMask objectLayer;
    [SerializeField] private GameObject target;
    [SerializeField] private KeyCode fireSpellKey = KeyCode.Alpha2;
    [SerializeField] private GameObject handTarget;
    [SerializeField] private Vector3 previewOffset = new Vector3(0, 0.31f, 0);

    [Header("Special Effects")] [SerializeField]
    private GameObject effect1Prefab;

    [Header("Rigging")] 
    [SerializeField] private RigBuilder rigBuilder;
    [SerializeField] private TwoBoneIKConstraint twoBoneIKConstraint;
    [SerializeField] private CustomHeadLookAtTarget customHeadLookAtTarget;
    private Vector3 rightArmTargetOrginalPosition;
    [SerializeField] private GameObject rightArmTarget;
    
    [Header("Audio")]
    [SerializeField] private AudioClip fireExplosionSound;

    private Camera _mainCamera;
    private bool _isPlacementModeActive = false;
    private GameObject _currentPreview;
    private Vector3? currentHitPoint = null;

    void Awake()
    {
        _mainCamera = Camera.main;

        rightArmTargetOrginalPosition = rightArmTarget.transform.localPosition;
    }

    void Update()
    {

        if (Input.GetKeyDown(fireSpellKey))
        {
            rigBuilder.enabled = true;
            ToggleSpellMode();
        }

        if (_isPlacementModeActive)
        {
            UpdatePreviewPosition();

            if (Input.GetKeyDown(KeyCode.Escape))
            {
                ToggleSpellMode();
            }
        }
    }


    public void OnLeftClick(InputValue value)
    {
        if (_isPlacementModeActive)
        {
            Ray ray = _mainCamera.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit, 100f, objectLayer))
            {
                currentHitPoint = hit.point;
                PlaceObject(hit.point);
                
                // rightArmTarget.transform.position = currentHitPoint.Value;
            }
        }
    }

    
    private void ToggleSpellMode()
    {
        
        _isPlacementModeActive = !_isPlacementModeActive;

        if (_isPlacementModeActive)
        {
            DOTween.To(() => twoBoneIKConstraint.weight, x => twoBoneIKConstraint.weight = x,
                1f, 1.0f).SetEase(Ease.OutCubic).OnComplete(() =>
            {
                if (_isPlacementModeActive)
                {
                    customHeadLookAtTarget.Target = handTarget.transform;
                    DOTween.To(() => customHeadLookAtTarget.weight, x => customHeadLookAtTarget.weight = x, 1f,
                            0.5f)
                        .SetEase(Ease.OutCubic);


                    _currentPreview = Instantiate(previewPrefab);
                    _currentPreview.transform.position = handTarget.transform.position + previewOffset;
                    _currentPreview.transform.localScale = Vector3.zero;
                    _currentPreview.transform.DOScale(0.4f, 0.4f).SetEase(Ease.OutBack);
                }
            });

            customHeadLookAtTarget.Target = handTarget.transform;
            DOTween.To(() => customHeadLookAtTarget.weight, x => customHeadLookAtTarget.weight = x, 1f, 1.0f)
                .SetEase(Ease.OutCubic);


        }
        else
        {
            DOTween.To(() => twoBoneIKConstraint.weight, x => twoBoneIKConstraint.weight = x,
                0f, 1.0f).SetEase(Ease.OutCubic);

            GameObject objectToDestroy = _currentPreview;
            // _currentPreview = null;

            _currentPreview.transform.DOScale(0f, 0.4f).SetEase(Ease.OutBack);

            if (objectToDestroy != null)
            {
                objectToDestroy.transform.DOScale(0f, 0.4f).SetEase(Ease.OutBack).OnComplete(() =>
                {
                    Destroy(objectToDestroy);
                });
            }
            
            DOTween.To(() => customHeadLookAtTarget.weight, x => customHeadLookAtTarget.weight = x, 0f, 0.5f)
                .SetEase(Ease.OutCubic);
        }
    }

    private void UpdatePreviewPosition()
    {
        if (_currentPreview == null) return;

        _currentPreview.transform.position = handTarget.transform.position + previewOffset;
    }


    private void PlaceObject(Vector3 hitPoint)
    {
        if(_currentPreview == null) return;
        
        Vector3 spawnPos = _currentPreview.transform.position;
        spawnPos.y += 0.75f;

        // ToggleSpellMode();

        Sequence spawnSequence = DOTween.Sequence();
        spawnSequence.AppendCallback(() =>
        {
            _currentPreview.transform.DOMove(hitPoint, 2.0f);
            rightArmTarget.transform.DOMove(hitPoint, 2.0f);
        });
        spawnSequence.AppendInterval(2.0f);
        spawnSequence.AppendCallback(() =>
        {
            Destroy(_currentPreview);
            currentHitPoint = null;
            
            rightArmTarget.transform.DOLocalMove(rightArmTargetOrginalPosition, 0.4f);
            ToggleSpellMode();
            
            GameObject newObj = Instantiate(prefabToSpawn, hitPoint, Quaternion.identity);
            newObj.transform.localScale = Vector3.zero;
            newObj.transform.DOScale(3f, 0.4f).SetEase(Ease.OutBack);

            CreateExplosion(hitPoint);
        });
    }
    
    private void CreateExplosion(Vector3 explosionPos)
    {
        float radius = 5.0f;        // How far the blast reaches
        float power = 10.0f;        // How strong the push is
        float upwardModifier = 3.0f; // Lifts objects up, making them "pop" into the air

        // 1. Find all colliders in the blast area
        Collider[] colliders = Physics.OverlapSphere(explosionPos, radius);

        foreach (Collider hit in colliders)
        {
            Rigidbody rb = hit.GetComponent<Rigidbody>();

            // 2. If the object has a Rigidbody, blast it!
            if (rb != null)
            {
                rb.AddExplosionForce(power, explosionPos, radius, upwardModifier, ForceMode.Impulse);
            }
        }
        
        if (fireExplosionSound != null)
        {
            AudioSource.PlayClipAtPoint(fireExplosionSound, explosionPos, 1);
        }
    }
}