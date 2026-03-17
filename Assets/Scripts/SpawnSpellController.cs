using System.Threading.Tasks;
using UnityEngine;
using DG.Tweening;
using UnityEngine.Serialization;
using UnityEngine.Animations.Rigging;
using UnityEngine.InputSystem;
public class SpawnSpellController : MonoBehaviour
{
    [Header("Prefabs")]
    [SerializeField] private GameObject prefabToSpawn;
    [SerializeField] private GameObject previewPrefab;
    

    [Header("Settings")]
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private KeyCode spawnSpellKey = KeyCode.Alpha1; // "1" Key
    
    [Header("Special Effects")]
    [SerializeField] private GameObject effect1Prefab;
    [SerializeField] private GameObject effect2Prefab;
    [SerializeField] private float effect1Delay = 2f;
    [SerializeField] private float effect2Delay = 0.5f;
    
    [Header("Rigging")] 
    [SerializeField] private CustomHeadLookAtTarget customHeadLookAtTarget;
    
    [Header("Audio")]
    [SerializeField] private AudioClip spawnSound;

    private GameObject _currentPreview;
    private bool _isPlacementModeActive = false;
    private Camera _mainCamera;

    void Awake()
    {
        _mainCamera = Camera.main;
    }

    void Update()
    {
        if (Input.GetKeyDown(spawnSpellKey))
        {
            TogglePlacementMode();
        }

        if (_isPlacementModeActive)
        {
            UpdatePreviewPosition();
            
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                TogglePlacementMode();
            }
        }
    }
    
    public void OnLeftClick(InputValue value)
    {
        if (_isPlacementModeActive)
        {
            PlaceObject();    
        }
    }

    private void TogglePlacementMode()
    {
        _isPlacementModeActive = !_isPlacementModeActive;

        if (_isPlacementModeActive)
        {
            _currentPreview = Instantiate(previewPrefab);
            _currentPreview.transform.localScale = Vector3.zero;
            _currentPreview.transform.DOScale(0.15f, 0.4f).SetEase(Ease.OutBack);
            if (_currentPreview.TryGetComponent<Collider>(out var col)) col.enabled = false;
            
            customHeadLookAtTarget.Target = _currentPreview.transform;
            DOTween.To(() => customHeadLookAtTarget.weight, x => customHeadLookAtTarget.weight = x, 1f,
                    0.5f)
                .SetEase(Ease.OutCubic);
        }
        else
        {
            GameObject objectToDestroy = _currentPreview;
            _currentPreview = null;
            
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
        if(_currentPreview == null) return;
        
        customHeadLookAtTarget.Target = _currentPreview.transform;
        
        Ray ray = _mainCamera.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit, 100f, groundLayer))
        {
            _currentPreview.transform.position = hit.point;
            _currentPreview.SetActive(true);
        }
        else
        {
            // Hide preview if mouse is pointing at the sky/void
            _currentPreview.SetActive(false);
        }
    }
    
    
    private void PlaceObject()
    {
        Vector3 spawnPos = _currentPreview.transform.position;

        spawnPos.y += 0.75f;

        TogglePlacementMode();
        
        Sequence spawnSequence = DOTween.Sequence();
        spawnSequence.AppendCallback(() => {
            GameObject fx1 = Instantiate(effect1Prefab, spawnPos, Quaternion.identity);
            
            fx1.transform.localScale = Vector3.zero;
            Sequence fxLifetime = DOTween.Sequence();
            fxLifetime.Append(fx1.transform.DOScale(0.15f, 0.4f));
            fxLifetime.AppendInterval(1.5f);                      
            fxLifetime.Append(fx1.transform.DOScale(0f, 0.4f));
            fxLifetime.OnComplete(() => Destroy(fx1));
        });
        spawnSequence.AppendInterval(effect1Delay);
        spawnSequence.AppendCallback(() => {
            GameObject fx2 = Instantiate(effect2Prefab, spawnPos, Quaternion.identity);
            fx2.transform.localScale = Vector3.zero;
            Sequence fxLifetime = DOTween.Sequence();
            fxLifetime.Append(fx2.transform.DOScale(0.5f, 0.4f));
            fxLifetime.AppendInterval(effect2Delay - 0.5f);                      
            fxLifetime.Append(fx2.transform.DOScale(0f, 0.4f));
            fxLifetime.OnComplete(() => Destroy(fx2));
        });
        spawnSequence.AppendInterval(effect2Delay);
        spawnSequence.AppendCallback(() => {
            GameObject newObj = Instantiate(prefabToSpawn, spawnPos, Quaternion.identity);
            newObj.transform.localScale = Vector3.zero;
            newObj.transform.DOScale(1f, 0.4f).SetEase(Ease.OutBack);
            
            if (spawnSound != null)
            {
                AudioSource.PlayClipAtPoint(spawnSound, spawnPos, 0.5f);
            }
        });
    }
}