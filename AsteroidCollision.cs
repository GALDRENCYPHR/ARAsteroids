using UnityEngine;
using System.Collections;
public class AsteroidCollision : MonoBehaviour
{
   private void OnCollisionEnter(Collision collision)
{
    Debug.Log($"Asteroid collided with: {collision.gameObject.name}, tag={collision.gameObject.tag}");
    if (collision.gameObject.CompareTag("MainCamera"))
    {
        CameraHealth camHealth = collision.gameObject.GetComponent<CameraHealth>();
        if (camHealth != null) camHealth.TakeDamage(1);
    }
}

// add this too so it works if something is using triggers
private void OnTriggerEnter(Collider other)
{
    Debug.Log($"Asteroid trigger with: {other.gameObject.name}, tag={other.gameObject.tag}");
    if (other.CompareTag("MainCamera"))
    {
        CameraHealth camHealth = other.GetComponent<CameraHealth>();
        if (camHealth != null) camHealth.TakeDamage(1);
    }
}
}
