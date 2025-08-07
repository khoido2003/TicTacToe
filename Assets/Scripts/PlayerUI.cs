using System;
using UnityEngine;

public class PlayerUI : MonoBehaviour
{
    [SerializeField]
    private GameObject crossArrowObject;

    [SerializeField]
    private GameObject circleArrowObject;

    [SerializeField]
    private GameObject crossTextObject;

    [SerializeField]
    private GameObject circleTextObject;

    private void Awake()
    {
        crossArrowObject.SetActive(false);

        circleArrowObject.SetActive(false);

        crossTextObject.SetActive(false);

        circleTextObject.SetActive(false);
    }

    private void Start()
    {
        GameManager.Instance.OnGameStarted += GameManager_OnGameStarted;
        GameManager.Instance.OnCurrentPlayablePlayerTypeChanged +=
            GameManager_OnCurrentPlayablePlayerTypeChanged;
    }

    private void GameManager_OnCurrentPlayablePlayerTypeChanged(object sender, EventArgs e)
    {
        UpdateArrow();
    }

    private void GameManager_OnGameStarted(object sender, EventArgs e)
    {
        if (GameManager.Instance.GetLocalPlayerType() == GameManager.PlayerType.Cross)
        {
            crossTextObject.SetActive(true);
        }
        else
        {
            circleTextObject.SetActive(true);
        }

        UpdateArrow();
    }

    private void UpdateArrow()
    {
        if (GameManager.Instance.GetCurrentPlayablePlayerType() == GameManager.PlayerType.Cross)
        {
            crossArrowObject.SetActive(true);
            circleArrowObject.SetActive(false);
        }
        else
        {
            crossArrowObject.SetActive(false);
            circleArrowObject.SetActive(true);
        }
    }
}
