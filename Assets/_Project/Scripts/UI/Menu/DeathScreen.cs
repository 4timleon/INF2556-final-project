using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DeathScreen : MonoBehaviour
{
    [SerializeField] private Image background;
    [SerializeField] private CanvasGroup titleCanvasGroup;
    [SerializeField] private CanvasGroup scoreBoardCanvasGroup;
    [SerializeField] private CanvasGroup buttonsCanvasGroup;
    [SerializeField] private float alphaDurationBackground = 5f;
    [SerializeField] private float alphaDurationDeathScreen = 1f;
    [SerializeField] private GameObject endingScreenContainer;

    [Header("References")]
    [SerializeField] private PointsUI pointsUI;

    [Header("Texts")]
    [SerializeField] private TextMeshProUGUI pointsTextMesh;
    [SerializeField] private TextMeshProUGUI killsTextMesh;
    [SerializeField] private TextMeshProUGUI deathsTextMesh;

    private float kills = 0f;

    private void OnEnable()
    {
        endingScreenContainer.SetActive(false);
        ResetAlpha();
        GameEventSystem.OnPlayerDeath += ShowDeathScreen;
        GameEventSystem.OnEnemyKilled += GameEventSystem_OnEnemyKilled;
    }

    private void OnDisable()
    {
        LeanTween.cancel(gameObject);
        GameEventSystem.OnPlayerDeath -= ShowDeathScreen;
        GameEventSystem.OnEnemyKilled -= GameEventSystem_OnEnemyKilled;
    }

    private void ResetAlpha()
    {
        //Set every alpha to 0
        Color bgColor = background.color;
        bgColor.a = 0f;
        background.color = bgColor;

        titleCanvasGroup.alpha = 0f;
        scoreBoardCanvasGroup.alpha = 0f;
        buttonsCanvasGroup.alpha = 0f;
    }

    private void ShowDeathScreen(float deathCounter)
    {
        endingScreenContainer.SetActive(true);
        LeanTween.cancel(gameObject);

        //Update text
        pointsTextMesh.text = GameData.Money.ToString();
        killsTextMesh.text = kills.ToString();
        deathsTextMesh.text = deathCounter.ToString();

        //Change background Alpha
        LeanTween.alpha(background.rectTransform, 1f, alphaDurationBackground)
            .setIgnoreTimeScale(true);

        //Change Title alpha then scoreboard and at last the buttons
        LeanTween.alphaCanvas(titleCanvasGroup, 1f, alphaDurationDeathScreen)
            .setIgnoreTimeScale(true);

        LeanTween.alphaCanvas(scoreBoardCanvasGroup, 1f, alphaDurationDeathScreen)
            .setDelay(alphaDurationDeathScreen)
            .setIgnoreTimeScale(true);

        LeanTween.alphaCanvas(buttonsCanvasGroup, 1f, alphaDurationDeathScreen)
            .setDelay(alphaDurationDeathScreen * 2f)
            .setIgnoreTimeScale(true);
    }

    private void GameEventSystem_OnEnemyKilled(Zombie zombie)
    {
        kills++;
    }
}
