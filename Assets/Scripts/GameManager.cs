using System;
using Unity.Netcode;
using UnityEngine;

public class GameManager : NetworkBehaviour
{
    public static GameManager Instance { get; private set; }

    public event EventHandler<OnClickedOnGridPositionArgs> OnClickedOnGridPosition;

    public class OnClickedOnGridPositionArgs : EventArgs
    {
        public int x;
        public int y;
        public PlayerType playerType;
    }

    public enum PlayerType
    {
        Cross,
        Circle,
        None,
    }

    private PlayerType currentPlayablePlayerType;

    private PlayerType localPlayerType;

    private void Awake()
    {
        if (Instance != null)
        {
            Debug.LogError("More than one GameManager instance");
        }
        Instance = this;
    }

    [Rpc(SendTo.Server)]
    public void ClickedOnGridPositionRpc(int x, int y, PlayerType playerType)
    {
        if (playerType != currentPlayablePlayerType)
        {
            return;
        }

        OnClickedOnGridPosition?.Invoke(
            this,
            new OnClickedOnGridPositionArgs
            {
                x = x,
                y = y,
                playerType = playerType,
            }
        );

        switch (currentPlayablePlayerType)
        {
            default:
            case PlayerType.Cross:
                currentPlayablePlayerType = PlayerType.Circle;
                break;
            case PlayerType.Circle:
                currentPlayablePlayerType = PlayerType.Cross;
                break;
        }
    }

    public override void OnNetworkSpawn()
    {
        if (NetworkManager.Singleton.LocalClientId == 0)
        {
            localPlayerType = PlayerType.Cross;
        }
        else
        {
            localPlayerType = PlayerType.Circle;
        }

        if (IsServer)
        {
            currentPlayablePlayerType = PlayerType.Cross;
        }
    }

    public PlayerType GetLocalPlayerType()
    {
        return localPlayerType;
    }
}
