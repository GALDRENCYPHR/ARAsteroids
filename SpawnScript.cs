using UnityEngine;
using System.Collections;

public class SpawnScript : MonoBehaviour
{
    [Header("References")]
    public Transform[] spawnPoints;          // candidate spawn points
    public GameObject[] rocks;               // asteroid prefabs
    public Transform cameraTransform;        // Main/AR Camera

    [Header("Spawn Safety")]
    public float minDistanceFromCamera = 3f; // no-spawn bubble around player
    public float preSpawnCheckRadius = 0.6f; // ≈ your rock collider radius
    public LayerMask asteroidLayer;          // set to Asteroid
    public int maxSpawnAttemptsPerPoint = 8;
    public float spawnStagger = 0.12f;       // seconds between spawns in a wave

    [Header("Repel on Spawn")]
    public float repelRadius = 1.0f;         // nearby check radius
    public float repelImpulse = 1.5f;        // strength baseline (we convert to a small nudge)

    [Header("Drifters (toward camera)")]
    [Range(0f, 1f)] public float driftChance = 0.2f; // 20% will drift
    public float driftSpeedMin = 0.08f;              // m/s
    public float driftSpeedMax = 0.18f;              // m/s
    public float driftDrag = 0.2f;                   // small drag so they coast

    [Header("Wave Settings")]
    public int rocksPerWave = 7;
    public float initialDelay = 3f;

    void Start()
    {
        StartCoroutine(StartSpawning());
    }

    IEnumerator StartSpawning()
    {
        yield return new WaitForSeconds(initialDelay);

        for (int i = 0; i < rocksPerWave; i++)
        {
            Transform basePoint = spawnPoints[Random.Range(0, spawnPoints.Length)];
            Vector3 candidate = basePoint.position;
            bool spawned = false;

            for (int attempt = 0; attempt < maxSpawnAttemptsPerPoint && !spawned; attempt++)
            {
                if (IsFarFromPlayer(candidate) && IsAreaClear(candidate, preSpawnCheckRadius))
                {
                    GameObject prefab = rocks[Random.Range(0, rocks.Length)];
                    GameObject rock = Instantiate(prefab, candidate, Random.rotation);

                    // Force layer = "Asteroid" on root + children
                    ApplyAsteroidLayer(rock, "Asteroid");

                    // Ensure Rigidbody exists and is configured
                    var rb = rock.GetComponent<Rigidbody>();
                    if (rb == null) rb = rock.AddComponent<Rigidbody>();
                    rb.isKinematic = false;   // allow drifters to use velocity
                    rb.useGravity = false;    // space rocks :)
                    rb.collisionDetectionMode = CollisionDetectionMode.Continuous;

                    // ~20% become slow drifters toward the camera
                    if (cameraTransform != null && Random.value < driftChance)
                    {
                        Vector3 toCam = (cameraTransform.position - rock.transform.position).normalized;
                        float spd = Random.Range(driftSpeedMin, driftSpeedMax);
                        rb.linearVelocity = toCam * spd;
                        rb.linearDamping = driftDrag;
                    }

                    // Gentle separation nudge right after spawn
                    RepelNearby(candidate, rock);

                    spawned = true;
                    yield return new WaitForSeconds(spawnStagger);
                    break;
                }

                // try a nearby offset and re-check
                candidate = basePoint.position + Random.insideUnitSphere * 0.5f;
            }
        }

        // keep spawning waves
        StartCoroutine(StartSpawning());
    }

    // ---------- helpers ----------

    bool IsFarFromPlayer(Vector3 pos)
    {
        if (cameraTransform == null) return true;
        return Vector3.Distance(pos, cameraTransform.position) >= minDistanceFromCamera;
    }

    bool IsAreaClear(Vector3 pos, float radius)
    {
        // true if no asteroids in the area
        return !Physics.CheckSphere(pos, radius, asteroidLayer);
    }

    void RepelNearby(Vector3 pos, GameObject justSpawned)
    {
        // tiny positional nudge (not AddForce) so we don't depend on physics for all rocks
        var hits = Physics.OverlapSphere(pos, repelRadius, asteroidLayer);
        foreach (var h in hits)
        {
            if (h == null) continue;
            if (justSpawned != null && h.gameObject == justSpawned) continue;

            Vector3 dir = (h.transform.position - pos);
            if (dir.sqrMagnitude < 0.002f) dir = Random.onUnitSphere;
            dir.Normalize();

            // tone down push to be subtle
            float push = repelImpulse * 0.005f; // adjust in Inspector if needed
            h.transform.position += dir * push;
        }
    }

    void ApplyAsteroidLayer(GameObject root, string layerName)
    {
        int layer = LayerMask.NameToLayer(layerName);
        if (layer < 0)
        {
            Debug.LogWarning($"[SpawnScript] Layer '{layerName}' does not exist. Create it in Add Layer… first.");
            return;
        }

        root.layer = layer;
        foreach (Transform child in root.transform)
            child.gameObject.layer = layer;
    }
}