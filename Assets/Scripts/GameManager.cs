using System;
using System.Collections.Generic;
using NUnit.Framework.Internal;
using Unity.Netcode;
using UnityEngine;

public class GameManager : NetworkBehaviour
{
    public static GameManager Instance { get; private set; }

    private PlayerType[,] playerTypeArray;

    public event EventHandler OnGameStarted;
    public event EventHandler OnCurrentPlayablePlayerTypeChanged;

    public event EventHandler<OnClickedOnGridPositionArgs> OnClickedOnGridPosition;

    public event EventHandler<OnGameWinArgs> OnGameWin;

    public class OnGameWinArgs : EventArgs
    {
        public Line line;
    }

    public class OnClickedOnGridPositionArgs : EventArgs
    {
        public int x;
        public int y;
        public PlayerType playerType;
    }

    public enum PlayerType
    {
        None,
        Cross,
        Circle,
    }

    public enum Orientation
    {
        Horizontal,
        Vertical,
        DiagonalA,
        DiagonalB,
    }

    public struct Line
    {
        public List<Vector2Int> gridVector2IntList;
        public Vector2Int centerGridPosition;
        public Orientation orientation;
    }

    private List<Line> lineList;

    private NetworkVariable<PlayerType> currentPlayablePlayerType = new(
        PlayerType.None,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Owner
    );

    private PlayerType localPlayerType;

    ///////////////////////////////////////////////////


    private void Awake()
    {
        if (Instance != null)
        {
            Debug.LogError("More than one GameManager instance");
        }
        Instance = this;

        playerTypeArray = new PlayerType[3, 3];

        lineList = new List<Line>
        {
            // Horizontal
            new Line
            {
                gridVector2IntList = new List<Vector2Int>
                {
                    new Vector2Int(0, 0),
                    new Vector2Int(0, 1),
                    new Vector2Int(0, 2),
                },
                centerGridPosition = new Vector2Int(0, 1),

                orientation = Orientation.Horizontal,
            },
            new Line
            {
                gridVector2IntList = new List<Vector2Int>
                {
                    new Vector2Int(1, 0),
                    new Vector2Int(1, 1),
                    new Vector2Int(1, 2),
                },
                centerGridPosition = new Vector2Int(1, 1),

                orientation = Orientation.Horizontal,
            },
            new Line
            {
                gridVector2IntList = new List<Vector2Int>
                {
                    new Vector2Int(2, 0),
                    new Vector2Int(2, 1),
                    new Vector2Int(2, 2),
                },
                centerGridPosition = new Vector2Int(2, 1),

                orientation = Orientation.Horizontal,
            },
            // Vertical
            new Line
            {
                gridVector2IntList = new List<Vector2Int>
                {
                    new Vector2Int(0, 0),
                    new Vector2Int(1, 0),
                    new Vector2Int(2, 0),
                },
                centerGridPosition = new Vector2Int(1, 0),

                orientation = Orientation.Vertical,
            },
            new Line
            {
                gridVector2IntList = new List<Vector2Int>
                {
                    new Vector2Int(0, 1),
                    new Vector2Int(1, 1),
                    new Vector2Int(2, 1),
                },
                centerGridPosition = new Vector2Int(1, 1),

                orientation = Orientation.Vertical,
            },
            new Line
            {
                gridVector2IntList = new List<Vector2Int>
                {
                    new Vector2Int(0, 2),
                    new Vector2Int(1, 2),
                    new Vector2Int(2, 2),
                },
                centerGridPosition = new Vector2Int(1, 2),

                orientation = Orientation.Vertical,
            },
            // Diagonal ↘
            new Line
            {
                gridVector2IntList = new List<Vector2Int>
                {
                    new Vector2Int(0, 0),
                    new Vector2Int(1, 1),
                    new Vector2Int(2, 2),
                },
                centerGridPosition = new Vector2Int(1, 1),

                orientation = Orientation.DiagonalA,
            },
            // Diagonal ↙
            new Line
            {
                gridVector2IntList = new List<Vector2Int>
                {
                    new Vector2Int(0, 2),
                    new Vector2Int(1, 1),
                    new Vector2Int(2, 0),
                },
                centerGridPosition = new Vector2Int(1, 1),
                orientation = Orientation.DiagonalB,
            },
        };
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
            NetworkManager.Singleton.OnClientConnectedCallback +=
                NetworkManager_OnClientConnectedCallback;
        }

        currentPlayablePlayerType.OnValueChanged += (
            PlayerType oldPlayerType,
            PlayerType newPlayerType
        ) =>
        {
            OnCurrentPlayablePlayerTypeChanged?.Invoke(this, EventArgs.Empty);
        };
    }

    /////////////////////////////////////////////

    [Rpc(SendTo.ClientsAndHost)]
    private void TriggerOnGameStartedRpc()
    {
        OnGameStarted?.Invoke(this, EventArgs.Empty);
    }

    // [Rpc(SendTo.ClientsAndHost)]
    // private void TriggerOnCurrentPlayablePlayerTypeChangedRpc()
    // {
    //     OnCurrentPlayablePlayerTypeChanged?.Invoke(this, EventArgs.Empty);
    // }

    [Rpc(SendTo.Server)]
    public void ClickedOnGridPositionRpc(int x, int y, PlayerType playerType)
    {
        if (playerType != currentPlayablePlayerType.Value)
        {
            return;
        }

        if (playerTypeArray[x, y] != PlayerType.None)
        {
            // Already occupied
            return;
        }

        playerTypeArray[x, y] = playerType;

        OnClickedOnGridPosition?.Invoke(
            this,
            new OnClickedOnGridPositionArgs
            {
                x = x,
                y = y,
                playerType = playerType,
            }
        );

        switch (currentPlayablePlayerType.Value)
        {
            default:
            case PlayerType.Cross:
                currentPlayablePlayerType.Value = PlayerType.Circle;
                break;
            case PlayerType.Circle:
                currentPlayablePlayerType.Value = PlayerType.Cross;
                break;
        }

        // // Try to update UI both server and client together when turn changed
        // TriggerOnCurrentPlayablePlayerTypeChangedRpc();

        TestWinner();
    }

    private void NetworkManager_OnClientConnectedCallback(ulong obj)
    {
        if (NetworkManager.Singleton.ConnectedClientsList.Count == 2)
        {
            // Start game
            currentPlayablePlayerType.Value = PlayerType.Cross;

            // Try to update UI both server and client together
            TriggerOnGameStartedRpc();
        }
    }

    private bool TestWinnerLine(
        PlayerType playerTypeA,
        PlayerType playerTypeB,
        PlayerType playerTypeC
    )
    {
        return playerTypeA != PlayerType.None
            && playerTypeA == playerTypeB
            && playerTypeB == playerTypeC;
    }

    private bool TestWinnerLine(Line line)
    {
        return TestWinnerLine(
            playerTypeArray[line.gridVector2IntList[0].x, line.gridVector2IntList[0].y],
            playerTypeArray[line.gridVector2IntList[1].x, line.gridVector2IntList[1].y],
            playerTypeArray[line.gridVector2IntList[2].x, line.gridVector2IntList[2].y]
        );
    }

    private void TestWinner()
    {
        foreach (Line line in lineList)
        {
            if (TestWinnerLine(line))
            {
                // Win
                currentPlayablePlayerType.Value = PlayerType.None;

                OnGameWin?.Invoke(this, new OnGameWinArgs { line = line });

                break;
            }
        }
    }

    public PlayerType GetLocalPlayerType()
    {
        return localPlayerType;
    }

    public PlayerType GetCurrentPlayablePlayerType()
    {
        return currentPlayablePlayerType.Value;
    }
}
