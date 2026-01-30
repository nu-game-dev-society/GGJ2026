using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

public class MaskSpawner : MonoBehaviour
{
    [SerializeField]
    private Vector2 start;

    [SerializeField]
    private Vector2 end;

    [SerializeField]
    private GameObject[] maskPrefabs;

    [SerializeField]
    private LayerMask spawningLayerMask;

    [SerializeField]
    [Range(0f, 2f)]
    private float spawnSpaceRadius = 0.75f;

    private Vector3 lastSpawn;
    private Vector3 lastHit;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        StartCoroutine(TestSpawning());
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private IEnumerator TestSpawning()
    {
        while (true)
        {
            yield return new WaitForSeconds(1);
            SpawnMask();
        }
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

            lastSpawn = spawnPos;
            lastHit = hitPos;

            GameObject mask = GameObject.Instantiate(maskPrefabs[Random.Range(0, maskPrefabs.Length)]);
            mask.transform.position = lastHit;
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

        Gizmos.DrawWireCube(transform.position + new Vector3(rect.center.x, 0, rect.center.y), new Vector3(rect.size.x, 0.01f, rect.size.y));

        if (lastSpawn != null && lastHit != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(lastSpawn, lastHit);
            Gizmos.DrawWireCube(lastHit, Vector3.one * spawnSpaceRadius);
        }
    }
}
