using System.Collections;
using UnityEngine;

public class RockScript : MonoBehaviour
{
    private float moveSpeed;
    public float collisionCheckRadius = 0.5f; // Adjust based on asteroid size
    public LayerMask asteroidLayer;           // Assign in Inspector to detect only asteroids

    void Start()
    {
        moveSpeed = Random.Range(0.2f, 0.5f);
    }

    void Update()
    {
        Vector3 directionToCamera = (Camera.main.transform.position - transform.position).normalized;

        // Check for another asteroid directly in the path
        if (!CheckSphereAhead(directionToCamera))
        {
            transform.Translate(directionToCamera * Time.deltaTime * moveSpeed, Space.World);
        }
        else
        {
            // Optional: small sidestep or slow down if blocked
            transform.Translate(Vector3.up * Time.deltaTime * moveSpeed, Space.World);
        }
    }

    bool CheckSphereAhead(Vector3 direction)
    {
        Vector3 checkPosition = transform.position + direction * collisionCheckRadius;
        return Physics.CheckSphere(checkPosition, collisionCheckRadius, asteroidLayer);
    }
}