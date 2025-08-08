using System;
using Unity.Netcode;
using UnityEngine;

public class GameVisualManager : NetworkBehaviour
{
    [SerializeField]
    private Transform crossPrefab;

    [SerializeField]
    private Transform circlePrefab;

    [SerializeField]
    private Transform lineCompletePrefab;

    private const float GRID_SIZE = 0.5f;

    private void Awake() { }

    private void Start()
    {
        GameManager.Instance.OnClickedOnGridPosition += GameManager_OnClickedOnGridPosition;

        GameManager.Instance.OnGameWin += GameManager_OnGameWin;
    }

    private void GameManager_OnGameWin(object sender, GameManager.OnGameWinArgs e)
    {
        float eulerZ = 0f;

        switch (e.line.orientation)
        {
            default:
            case GameManager.Orientation.Horizontal:
                eulerZ = 0f;
                break;

            case GameManager.Orientation.Vertical:
                eulerZ = 90f;
                break;

            case GameManager.Orientation.DiagonalA:
                eulerZ = -45f;
                break;
            case GameManager.Orientation.DiagonalB:
                eulerZ = 45f;
                break;
        }

        Transform lineCompleteTransform = Instantiate(
            lineCompletePrefab,
            GetWorldPositionForLine(e.line.centerGridPosition),
            Quaternion.Euler(0, 0, eulerZ)
        );

        lineCompleteTransform.GetComponent<NetworkObject>().Spawn(true);
    }

    private void GameManager_OnClickedOnGridPosition(
        object sender,
        GameManager.OnClickedOnGridPositionArgs e
    )
    {
        SpawnObjectRpc(e.x, e.y, e.playerType);
    }

    private Vector3 GetWorldPositionForLine(Vector2Int gridPos)
    {
        return new Vector3(
            (gridPos.y * GRID_SIZE) - GRID_SIZE,
            GRID_SIZE - (gridPos.x * GRID_SIZE),
            0f
        );
    }

    [Rpc(SendTo.Server)]
    private void SpawnObjectRpc(int x, int y, GameManager.PlayerType playerType)
    {
        Transform prefab;
        switch (playerType)
        {
            default:
            case GameManager.PlayerType.Cross:
                prefab = crossPrefab;
                break;
            case GameManager.PlayerType.Circle:
                prefab = circlePrefab;
                break;
        }

        Transform spawnedTransform = Instantiate(
            prefab,
            GetGridWorldPosition(x, y),
            Quaternion.identity
        );

        spawnedTransform.GetComponent<NetworkObject>().Spawn(true);
    }

    private Vector2 GetGridWorldPosition(int row, int col)
    {
        float worldX = (col * GRID_SIZE) - GRID_SIZE;
        float worldY = GRID_SIZE - row * GRID_SIZE;
        return new Vector2(worldX, worldY);
    }
}
