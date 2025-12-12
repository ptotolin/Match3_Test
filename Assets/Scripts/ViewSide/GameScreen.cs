using System.Collections;
using TMPro;
using UnityEngine;
using Zenject;

public class GameScreen : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI scoreText;

    // dependencies
    private IEventBus eventBus;

    // locas
    private float destScore;
    private Coroutine displayScoreCoroutine;

    private void OnEnable()
    {
        if (eventBus != null) {
            this.eventBus.Subscribe<ScoreEventData>(OnScoreChanged);
        }
    }

    private void OnDisable()
    {
        if (eventBus != null) {
            this.eventBus.Unsubscribe<ScoreEventData>(OnScoreChanged);
        }
    }

    [Inject]
    public void Initialize(IEventBus eventBus)
    {
        this.eventBus = eventBus;
        this.eventBus.Subscribe<ScoreEventData>(OnScoreChanged);
    }

    private void OnScoreChanged(ScoreEventData eventData)
    {
        destScore = eventData.ScoreNew;
        if (displayScoreCoroutine == null) {
            displayScoreCoroutine = StartCoroutine(DisplayScore(eventData.ScoreOld));
        }
    }

    private IEnumerator DisplayScore(int scoreFrom)
    {
        float fromValue = scoreFrom;
        scoreText.text = ((int)fromValue).ToString();
        while (fromValue <= destScore) {
            fromValue += SC_GameVariables.Instance.scoreSpeed * Time.deltaTime;
            scoreText.text = ((int)fromValue).ToString("0");

            yield return null;
        }

        displayScoreCoroutine = null;
    }
}
