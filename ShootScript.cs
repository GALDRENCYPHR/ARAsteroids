using System.Collections;
using UnityEngine;

public class ShootScript : MonoBehaviour
{
    public GameObject arCamera; // Reference to the AR camera
    public GameObject RockFracture; // Reference to the rock fragment prefab
    public GameObject muzzleFlashPrefab; // Reference to the muzzle flash prefab
    [Header("Muzzle Points")]
    public Transform[] muzzlePoints;
    public float explosionBaseScale = 1f;
    [Header("Muzzle Flash Offsets")]
    public float muzzleFlashForwardOffset = 0.05f;
    public float muzzleFlashUpOffset = 0f;
    public float muzzleFlashRightOffset = 0f;
    
    [Header("Muzzle Flash Size")]
    public float muzzleFlashScale = 1f;

    [Header("Fracture Physics (no VFX)")]
    public float fractureExplosionForce = 2.5f;   // outward push on fracture pieces
    public float fractureExplosionRadius = 1.2f;  // radius used by AddExplosionForce
    public float fractureLifetime = 2.5f;         // seconds before cleanup

    public void Shoot()
    {
        if (muzzleFlashPrefab != null)
        {
            bool usedMuzzlePoints = (muzzlePoints != null && muzzlePoints.Length > 0);
            if (usedMuzzlePoints)
            {
                foreach (var muzzle in muzzlePoints)
                {
                    if (muzzle == null) continue;
                    GameObject flash = Instantiate(muzzleFlashPrefab, muzzle.position, muzzle.rotation, muzzle);
                    flash.transform.localPosition +=
                        (Vector3.forward * muzzleFlashForwardOffset) +
                        (Vector3.up * muzzleFlashUpOffset) +
                        (Vector3.right * muzzleFlashRightOffset);
                    // Scale particle system correctly without distortion
                    var particleSystems = flash.GetComponentsInChildren<ParticleSystem>(true);
                    foreach (var ps in particleSystems)
                    {
                        var main = ps.main;
                        main.scalingMode = ParticleSystemScalingMode.Local;
                        if (main.startSize3D)
                        {
                            main.startSizeXMultiplier *= muzzleFlashScale;
                            main.startSizeYMultiplier *= muzzleFlashScale;
                            main.startSizeZMultiplier *= muzzleFlashScale;
                        }
                        else
                        {
                            main.startSizeMultiplier *= muzzleFlashScale;
                        }
                    }
                    Destroy(flash, 0.2f); // Destroy after short time
                }
            }
            else
            {
                GameObject flash = Instantiate(muzzleFlashPrefab, arCamera.transform.position, arCamera.transform.rotation, arCamera.transform);
                flash.transform.localPosition +=
                    (Vector3.forward * muzzleFlashForwardOffset) +
                    (Vector3.up * muzzleFlashUpOffset) +
                    (Vector3.right * muzzleFlashRightOffset);
                // Scale particle system correctly without distortion
                var particleSystems = flash.GetComponentsInChildren<ParticleSystem>(true);
                foreach (var ps in particleSystems)
                {
                    var main = ps.main;
                    main.scalingMode = ParticleSystemScalingMode.Local;
                    if (main.startSize3D)
                    {
                        main.startSizeXMultiplier *= muzzleFlashScale;
                        main.startSizeYMultiplier *= muzzleFlashScale;
                        main.startSizeZMultiplier *= muzzleFlashScale;
                    }
                    else
                    {
                        main.startSizeMultiplier *= muzzleFlashScale;
                    }
                }
                Destroy(flash, 0.2f); // Destroy after short time
            }
        }
    

        RaycastHit hit;
        if (Physics.Raycast(arCamera.transform.position, arCamera.transform.forward, out hit)) // Cast a ray from the camera forward (from the crosshair)
        {
            if (hit.transform.name == "Rock1(Clone)" ||
                hit.transform.name == "Rock2(Clone)" ||
                hit.transform.name == "Rock3(Clone)" ||
                hit.transform.name == "Rock4(Clone)" ||
                hit.transform.name == "Rock5(Clone)" ||
                hit.transform.name == "Rock6(Clone)" ||
                hit.transform.name == "Rock7(Clone)")
            {
                float size = 1f;
                Collider col = hit.transform.GetComponent<Collider>();
                if (col != null)
                {
                    size = col.bounds.extents.magnitude * 1.2f;
                }
                else
                {
                    Vector3 scale = hit.transform.localScale;
                    size = (scale.x + scale.y + scale.z) / 3f;
                }

                // Spawn fracture MESH at the rock's center, match scale & rotation,
                // then push its pieces outward (no particle VFX required)
                if (RockFracture != null)
                {
                    Transform rockT = hit.transform;
                    Vector3 rockPos = rockT.position;
                    Quaternion rockRot = rockT.rotation;
                    Vector3 rockScale = rockT.localScale;

                    GameObject fx = Instantiate(RockFracture, rockPos, rockRot);
                    fx.transform.localScale = rockScale * explosionBaseScale; // keep explosionBaseScale=1 for exact size

                    // Ensure each chunk has a Rigidbody, then push outward
                    var chunkRBs = fx.GetComponentsInChildren<Rigidbody>(true);
                    if (chunkRBs == null || chunkRBs.Length == 0)
                    {
                        // Try to add lightweight RBs to child pieces
                        var chunks = fx.GetComponentsInChildren<Transform>(true);
                        foreach (var c in chunks)
                        {
                            if (c == fx.transform) continue; // skip root
                            // Add a small collider if none is present (MeshCollider preferred if available)
                            if (c.GetComponent<Collider>() == null)
                            {
                                var mc = c.gameObject.AddComponent<MeshCollider>();
                                mc.convex = true; // required for RBs
                            }
                            var rb = c.GetComponent<Rigidbody>();
                            if (rb == null) rb = c.gameObject.AddComponent<Rigidbody>();
                            rb.useGravity = false;
                            rb.isKinematic = false;
                        }
                        chunkRBs = fx.GetComponentsInChildren<Rigidbody>(true);
                    }

                    // Apply an explosion-like push from the rock center
                    foreach (var rb in chunkRBs)
                    {
                        if (rb == null) continue;
                        rb.useGravity = false;         // space
                        rb.isKinematic = false;
                        rb.AddExplosionForce(
                            fractureExplosionForce,
                            fx.transform.position,
                            fractureExplosionRadius,
                            0.0f,                        // upwards modifier
                            ForceMode.Impulse
                        );
                    }

                    Destroy(fx, fractureLifetime);
                }

                // Award points
                if (ScoreManager.Instance != null)
                {
                    ScoreManager.Instance.AddScore(10);
                }

                // Finally destroy the rock
                Destroy(hit.transform.gameObject);
            }
        }
        else
        {

        }
    }
    
}
