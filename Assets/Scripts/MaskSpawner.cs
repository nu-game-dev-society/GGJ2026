using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class MaskSpawner : MonoBehaviour
{
    [SerializeField]
    private Vector2 start;

    [SerializeField]
    private Vector2 end;

    [SerializeField]
    private LayerMask spawningLayerMask;

    [SerializeField]
    [Range(0f, 2f)]
    private float spawnSpaceRadius = 0.75f;

    [SerializeField]
    [Range(1, 30)]
    private int spawnTime = 5;

    [SerializeField]
    [Range(1, 30)]
    private int maxSpawnCount = 4;

    [SerializeField]
    private GameObject[] maskPrefabs;

    private Vector3 lastSpawn;
    private Vector3 lastHit;

    private List<GameObject> spawnedMasks = new();

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        StartCoroutine(DoSpawning());
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private IEnumerator DoSpawning()
    {
        while (true)
        {
            yield return new WaitForSeconds(spawnTime);
            if (CanSpawnMask()) SpawnMask();
        }
    }

    private bool CanSpawnMask()
    {
        // Remove any old destroyed masks from the list
        spawnedMasks.RemoveAll(item => item == null);

        // Make sure we are under the max spawn count
        return spawnedMasks.Count <= maxSpawnCount;
    }

    private void SpawnMask()
    {
        Rect rect = new Rect(start, end - start);

        // Pick random point inside rect
        float x = Random.Range(0, rect.width);
        float z = Random.Range(0, rect.height);

        Vector3 spawnPos = transform.position + new Vector3(start.x, 0, start.y) + new Vector3(x, 0, z);

        RaycastHit hit;
        if (Physics.Raycast(spawnPos, transform.TransformDirection(Vector3.down), out hit, Mathf.Infinity))
        {
            Vector3 hitPos = spawnPos + transform.TransformDirection(Vector3.down) * (hit.distance);

            // Make sure we are not going to collide with any non spawning objects
            if (Physics.CheckBox(hitPos, Vector3.one * spawnSpaceRadius * 0.5f, Quaternion.identity, ~spawningLayerMask))
            {
                SpawnMask();
                return;
            }

            // Check if the hit object is allowed as a spawn location based on layer mask
            if ((spawningLayerMask & (1 << hit.transform.gameObject.layer)) == 0)
            {
                SpawnMask();
                return;
            }

            // Store last spawn and hit positions for gizmos
            lastSpawn = spawnPos;
            lastHit = hitPos;

            // Instantiate mask at hit position
            GameObject mask = GameObject.Instantiate(maskPrefabs[Random.Range(0, maskPrefabs.Length)]);
            mask.transform.position = lastHit;

            // Add to spawned masks list
            spawnedMasks.Add(mask);
        }
        else
        {
            SpawnMask();
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Rect rect = new Rect(start, end - start);

        // Draw spawning area
        Gizmos.DrawWireCube(transform.position + new Vector3(rect.center.x, 0, rect.center.y), new Vector3(rect.size.x, 0.01f, rect.size.y));

        // Draw last spawn and hit positions
        if (lastSpawn != Vector3.zero && lastHit != Vector3.zero)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(lastSpawn, lastHit);
            Gizmos.DrawWireCube(lastHit, Vector3.one * spawnSpaceRadius);
        }
    }
}
