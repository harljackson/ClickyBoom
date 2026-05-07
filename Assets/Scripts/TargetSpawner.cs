using System.Collections.Generic;
using UnityEngine;

public class TargetSpawner : MonoBehaviour
{
    [Header("Targets")]
    [SerializeField] private ClickTarget[] targetPrefabs;
    [SerializeField] private int maxTargetsOnScreen = 8;
    [SerializeField] private float spawnInterval = 0.7f;
    [SerializeField] private int pointsPerTarget = 1;
    [SerializeField] private float targetLifeTime = 4f;

    [Header("Spawn Area")]
    [SerializeField] private Vector2 xRange = new Vector2(-6f, 6f);
    [SerializeField] private Vector2 zRange = new Vector2(-3f, 5f);
    [SerializeField] private float yPosition = 0.75f;
    [SerializeField] private float minSpacing = 2.15f;

    private readonly List<ClickTarget> activeTargets = new List<ClickTarget>();
    private float spawnTimer;

    private void Update()
    {
        activeTargets.RemoveAll(target => target == null);

        if (GameManager.Instance != null && !GameManager.Instance.GameActive)
        {
            return;
        }

        spawnTimer -= Time.deltaTime;

        if (spawnTimer <= 0f && activeTargets.Count < maxTargetsOnScreen)
        {
            SpawnTarget();
            spawnTimer = spawnInterval;
        }
    }

    public void TargetRemoved(ClickTarget target)
    {
        activeTargets.Remove(target);
    }

    public void ResetSpawner()
    {
        activeTargets.Clear();
        spawnTimer = 0f;
    }

    private void SpawnTarget()
    {
        if (!TryFindSpawnPosition(out Vector3 spawnPosition))
        {
            return;
        }

        ClickTarget target = CreateTarget(spawnPosition);

        target.Initialise(this, pointsPerTarget, targetLifeTime);
        activeTargets.Add(target);
    }

    private bool TryFindSpawnPosition(out Vector3 spawnPosition)
    {
        for (int attempt = 0; attempt < 40; attempt++)
        {
            Vector3 candidate = new Vector3(
                Random.Range(xRange.x, xRange.y),
                yPosition,
                Random.Range(zRange.x, zRange.y)
            );

            if (HasEnoughSpace(candidate))
            {
                spawnPosition = candidate;
                return true;
            }
        }

        spawnPosition = Vector3.zero;
        return false;
    }

    private ClickTarget CreateTarget(Vector3 position)
    {
        if (targetPrefabs != null && targetPrefabs.Length > 0)
        {
            ClickTarget prefab = targetPrefabs[Random.Range(0, targetPrefabs.Length)];
            return Instantiate(prefab, position, Quaternion.identity);
        }

        GameObject fallbackTarget = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        fallbackTarget.name = "Click Target";
        fallbackTarget.transform.position = position;
        fallbackTarget.transform.localScale = Vector3.one * Random.Range(0.75f, 1.25f);

        Renderer renderer = fallbackTarget.GetComponent<Renderer>();
        renderer.material.color = Random.ColorHSV(0f, 1f, 0.65f, 1f, 0.8f, 1f);

        return fallbackTarget.AddComponent<ClickTarget>();
    }

    private bool HasEnoughSpace(Vector3 candidate)
    {
        foreach (ClickTarget target in activeTargets)
        {
            if (target == null)
            {
                continue;
            }

            Vector3 targetPosition = target.transform.position;
            targetPosition.y = candidate.y;

            if (Vector3.Distance(candidate, targetPosition) < minSpacing)
            {
                return false;
            }
        }

        return true;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;

        Vector3 centre = new Vector3(
            (xRange.x + xRange.y) * 0.5f,
            yPosition,
            (zRange.x + zRange.y) * 0.5f
        );

        Vector3 size = new Vector3(
            Mathf.Abs(xRange.y - xRange.x),
            0.1f,
            Mathf.Abs(zRange.y - zRange.x)
        );

        Gizmos.DrawWireCube(centre, size);
    }
}
