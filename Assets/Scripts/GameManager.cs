using System;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public event EventHandler<OnClickedOnGridPositionArgs> OnClickedOnGridPosition;

    public class OnClickedOnGridPositionArgs : EventArgs
    {
        public int x;
        public int y;
    }

    private void Awake()
    {
        if (Instance != null)
        {
            Debug.LogError("More than one GameManager instance");
        }
        Instance = this;
    }

    public void ClickedOnGridPosition(int x, int y)
    {
        Debug.Log("ClickedOnGridPosition " + x + " " + y);

        OnClickedOnGridPosition?.Invoke(this, new OnClickedOnGridPositionArgs { x = x, y = y });
    }
}
