using UnityEngine;

public class MousePlacementController : MonoBehaviour
{
    [Header("Raycast Settings")]
    public Camera mainCamera;
    public LayerMask groundLayer;
    public float maxRayDistance = 100f;

    [Header("Prefab Settings")]
    public GameObject placeholderPrefab;
    private GameObject currentPlaceholder;

    [Header("Nature of Code: Steering")]
    public float maxSpeed = 10f;
    [Tooltip("How strongly it can steer or brake. Lower = slides more.")]
    public float maxForce = 0.5f; 
    [Tooltip("The distance (like the '100' in the book) where it starts slowing down.")]
    public float slowingDistance = 5f;

    // Physics variables from the book
    private Vector3 velocity = Vector3.zero;
    private Vector3 acceleration = Vector3.zero;

    void Start()
    {
        if (mainCamera == null) mainCamera = Camera.main;
        
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None; 
    }

    void Update()
    {
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out RaycastHit hit, maxRayDistance, groundLayer))
        {
            if (currentPlaceholder == null)
            {
                currentPlaceholder = Instantiate(placeholderPrefab, hit.point, Quaternion.identity);
            }
            else
            {
                if (!currentPlaceholder.activeSelf) 
                {
                    currentPlaceholder.SetActive(true);
                    currentPlaceholder.transform.position = hit.point;
                    velocity = Vector3.zero;
                    acceleration = Vector3.zero;
                }

                // 1. Calculate steering
                Arrive(hit.point);

                // 2. Update physics (equivalent to Shiffman's update() function)
                UpdatePhysics();
            }
        }
        else
        {
            if (currentPlaceholder != null && currentPlaceholder.activeSelf)
            {
                currentPlaceholder.SetActive(false);
            }
        }
    }

    // Exact translation of the p5.js arrive() function
    private void Arrive(Vector3 target)
    {
        // let desired = p5.Vector.sub(target, this.position);
        Vector3 desired = target - currentPlaceholder.transform.position;

        // let d = desired.mag();
        float d = desired.magnitude;

        // if (d < 100) { ... } else { ... }
        if (d < slowingDistance)
        {
            // let m = map(d, 0, 100, 0, this.maxspeed);
            float m = Map(d, 0, slowingDistance, 0, maxSpeed);
            
            // desired.setMag(m);
            desired = desired.normalized * m;
        }
        else
        {
            // desired.setMag(this.maxspeed);
            desired = desired.normalized * maxSpeed;
        }

        // let steer = p5.Vector.sub(desired, this.velocity);
        Vector3 steer = desired - velocity;

        // steer.limit(this.maxforce);
        steer = Vector3.ClampMagnitude(steer, maxForce);

        // this.applyForce(steer);
        ApplyForce(steer);
    }

    private void ApplyForce(Vector3 force)
    {
        // this.acceleration.add(force);
        // (If you wanted to add mass, it would be force / mass)
        acceleration += force;
    }

    private void UpdatePhysics()
    {
        // 1. Velocity changes according to acceleration
        velocity += acceleration * Time.deltaTime;

        // 2. Limit the velocity to max speed
        velocity = Vector3.ClampMagnitude(velocity, maxSpeed);

        // 3. Position changes according to velocity
        currentPlaceholder.transform.position += velocity * Time.deltaTime;

        // Make the placeholder look in the direction it is moving (like the arrow in the book!)
        if (velocity.sqrMagnitude > 0.01f)
        {
            currentPlaceholder.transform.rotation = Quaternion.LookRotation(velocity.normalized);
        }

        // 4. Clear acceleration each frame
        acceleration = Vector3.zero;
    }

    // C# doesn't have p5.js's map() function built-in, so we recreate it here:
    private float Map(float value, float from1, float to1, float from2, float to2)
    {
        return (value - from1) / (to1 - from1) * (to2 - from2) + from2;
    }
}