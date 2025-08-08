using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameOverUI : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI resultTextMesh;

    [SerializeField]
    private Color winColor;

    [SerializeField]
    private Color loseColor;

    [SerializeField]
    private Button rematchBtn;

    private void Awake()
    {
        rematchBtn.onClick.AddListener(() =>
        {
            GameManager.Instance.RematchRpc();
        });
    }

    private void Start()
    {
        GameManager.Instance.OnGameWin += GameManager_OnGameWin;
        Hide();
        GameManager.Instance.OnRematch += GameManager_OnGameRematch;
    }

    private void GameManager_OnGameRematch(object sender, EventArgs e)
    {
        Hide();
    }

    private void GameManager_OnGameWin(object sender, GameManager.OnGameWinArgs e)
    {
        if (e.winPlayerType == GameManager.Instance.GetLocalPlayerType())
        {
            resultTextMesh.text = "YOU WIN!";
            // resultTextMesh.color = winColor;
        }
        else
        {
            resultTextMesh.text = "YOU LOSE!";
            // resultTextMesh.color = loseColor;
        }
        Show();
    }

    private void Show()
    {
        gameObject.SetActive(true);
    }

    private void Hide()
    {
        gameObject.SetActive(false);
    }
}
