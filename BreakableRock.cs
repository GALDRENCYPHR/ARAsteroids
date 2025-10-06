using System.Collections;
using UnityEngine;

public class BreakableRock : MonoBehaviour
{
    [Header("Split Settings")]
    [Tooltip("2 = large, 1 = medium, 0 = small (no further split)")]
    public int sizeLevel = 2;

    [Header("Auto Size Detection (optional)")]
    [Tooltip("If true, sizeLevel will be inferred from this object's current localScale. Useful when using one prefab scaled in code.")]
    public bool autoSizeByScale = true;

    [Tooltip("Average localScale threshold for LARGE (>= this) vs MEDIUM/SMALL. Assume 1.0 is your normal base size.")]
    public float mediumScaleThreshold = 0.9f;

    [Tooltip("Average localScale threshold for MEDIUM (>= this) vs SMALL. Values below this are SMALL.")]
    public float smallScaleThreshold = 0.6f;

    [Tooltip("Prefab to use for split children (can be the same prefab)")]
    public GameObject childRockPrefab;

    [Tooltip("Scale applied to each child vs parent")]

    public float childScaleFactor = 0.6f;

    [Tooltip("Impulse applied to push the two children apart")]
    public float splitImpulse = 2.5f;

    [Tooltip("Extra sideways spread")]
    public float lateralSpread = 0.75f;

    [Header("Overlap Safety")]
    public float spawnClearRadius = 0.3f;      // tweak to your collider size
    public LayerMask asteroidLayer;            // set to your Asteroid layer
    public int maxRepositionAttempts = 6;      // try a few tiny nudges to avoid overlap

    void Awake()
    {
        if (!autoSizeByScale) return;
        Vector3 s = transform.localScale;
        float avg = (s.x + s.y + s.z) / 3f;
        if (avg >= mediumScaleThreshold) sizeLevel = 2;           // Large
        else if (avg >= smallScaleThreshold) sizeLevel = 1;       // Medium
        else sizeLevel = 0;                                       // Small
    }

    public void Split(Vector3 hitPoint, Vector3 hitNormal)
    {
        if (sizeLevel <= 0 || childRockPrefab == null) return;

        // Compute base transform for children
        Vector3 parentPos = transform.position;
        Quaternion parentRot = transform.rotation;
        Vector3 parentScale = transform.localScale;

        // Build a local frame using the hit normal so children fly apart nicely
        Vector3 n = (hitNormal.sqrMagnitude > 0.0001f ? hitNormal : Random.onUnitSphere).normalized;
        Vector3 t1 = Vector3.Cross(n, Vector3.up);
        if (t1.sqrMagnitude < 0.0001f) t1 = Vector3.Cross(n, Vector3.right);
        t1.Normalize();
        Vector3 t2 = Vector3.Cross(n, t1).normalized;

        // Two opposite directions in the tangent plane + a tiny normal component
        Vector3 dirA = (t1 + n * 0.2f).normalized;
        Vector3 dirB = (-t1 + n * 0.2f).normalized;

        // Spawn two children
        SpawnChild(parentPos, parentRot, parentScale, dirA);
        SpawnChild(parentPos, parentRot, parentScale, dirB);
    }

    void SpawnChild(Vector3 parentPos, Quaternion parentRot, Vector3 parentScale, Vector3 direction)
    {
        // Start slightly offset so they don't overlap the destroyed parent
        Vector3 spawnPos = parentPos + direction * 0.15f;

        // Try tiny nudges if spawning into another asteroid
        bool clear = IsAreaClear(spawnPos, spawnClearRadius);
        int tries = 0;
        while (!clear && tries < maxRepositionAttempts)
        {
            spawnPos += Random.insideUnitSphere * 0.08f;
            clear = IsAreaClear(spawnPos, spawnClearRadius);
            tries++;
        }

        var child = Instantiate(childRockPrefab, spawnPos, parentRot);
        child.transform.localScale = parentScale * childScaleFactor;

        // Copy size level down
        var br = child.GetComponent<BreakableRock>();
        if (br != null)
        {
            br.sizeLevel = Mathf.Max(0, sizeLevel - 1);
            br.childRockPrefab = childRockPrefab;          // keep splitting with same prefab
            br.childScaleFactor = childScaleFactor;
            br.splitImpulse = splitImpulse;
            br.lateralSpread = lateralSpread;
            br.spawnClearRadius = spawnClearRadius;
            br.asteroidLayer = asteroidLayer;
            br.maxRepositionAttempts = maxRepositionAttempts;
        }

        // Give it some motion/impulse
        var rb = child.GetComponent<Rigidbody>();
        if (rb == null) rb = child.AddComponent<Rigidbody>();
        rb.mass = Mathf.Max(0.1f, rb.mass * childScaleFactor);   // lighter child
        rb.useGravity = false;
        rb.isKinematic = false;
        Vector3 impulse = (direction.normalized + Random.insideUnitSphere * lateralSpread) * splitImpulse;
        rb.AddForce(impulse, ForceMode.Impulse);

        // Make sure your movement script is present (RockScript)
        var mover = child.GetComponent<RockScript>();
        if (mover == null) child.AddComponent<RockScript>();
    }

    bool IsAreaClear(Vector3 pos, float radius)
    {
        return !Physics.CheckSphere(pos, radius, asteroidLayer);
    }
}