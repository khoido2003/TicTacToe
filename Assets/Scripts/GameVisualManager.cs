using System;
using Unity.Netcode;
using UnityEngine;

public class GameVisualManager : NetworkBehaviour
{
    [SerializeField]
    private Transform crossPrefab;

    [SerializeField]
    private Transform circlePrefab;

    private const float GRID_SIZE = 0.5f;

    private void Awake() { }

    private void Start()
    {
        GameManager.Instance.OnClickedOnGridPosition += GameManager_OnClickedOnGridPosition;
    }

    private void GameManager_OnClickedOnGridPosition(
        object sender,
        GameManager.OnClickedOnGridPositionArgs e
    )
    {
        SpawnObjectRpc(e.x, e.y);
    }

    [Rpc(SendTo.Server)]
    private void SpawnObjectRpc(int x, int y)
    {
        Debug.Log("Spawn object");
        Transform spawnedCrossTransform = Instantiate(
            crossPrefab,
            GetGridWorldPosition(x, y),
            Quaternion.identity
        );

        spawnedCrossTransform.GetComponent<NetworkObject>().Spawn(true);
    }

    private Vector2 GetGridWorldPosition(int row, int col)
    {
        float worldX = (col * GRID_SIZE) - GRID_SIZE;
        float worldY = GRID_SIZE - row * GRID_SIZE;
        return new Vector2(worldX, worldY);
    }
}
