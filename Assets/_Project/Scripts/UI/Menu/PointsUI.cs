using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Pool;

public class PointsUI : MonoBehaviour {

    [Header("Points Text")]
    [SerializeField] private TextMeshProUGUI pointsText;
    [SerializeField] private TextMeshProUGUI fadingNumberText;
    [SerializeField] private Transform spawnPivot;

    [Header("Settings")]
    [SerializeField] private float moveDistance = 50f;
    [SerializeField] private float duration = 0.8f;
    [SerializeField] private float minAngle = -35f;
    [SerializeField] private float maxAngle = 35f;
    [SerializeField] private LeanTweenType easeType = LeanTweenType.easeOutQuad;

    [Header("Pool Config")]
    [SerializeField] private int defaultCapacity = 20;
    [SerializeField] private int maxSize = 100;

    private readonly StringBuilder stringBuilder = new StringBuilder(16);

    private int points = 0;
    public int Points => points;

    //Pool
    private IObjectPool<GameObject> textPool;

    private void Awake() {
        textPool = new ObjectPool<GameObject>(
            createFunc: () => Instantiate(fadingNumberText.gameObject, transform),
            actionOnGet: go => go.SetActive(true),
            actionOnRelease: go => go.SetActive(false),
            actionOnDestroy: Destroy,
            collectionCheck: false,
            defaultCapacity: defaultCapacity,
            maxSize: maxSize
        );
    }

    private void Update() {
        if (Keyboard.current.digit4Key.wasPressedThisFrame) {
            ApplyPoints(25);
        }
    }

    public void ApplyPoints(int value) {
        if (value <= 0f) return;

        stringBuilder.Clear();
        points += value;
        stringBuilder.Append(points);
        pointsText.SetText(stringBuilder);

        SpawnVisual(value);
    }

    private void SpawnVisual(int value) {
        //Get Object out of the Pool and set its position
        GameObject fadingNumberGameObject = textPool.Get();
        fadingNumberGameObject.transform.position = spawnPivot.position;

        //Get the text Component
        TextMeshProUGUI fadingNumberText = fadingNumberGameObject.GetComponent<TextMeshProUGUI>();

        //Set the amount of points as text
        stringBuilder.Clear();
        stringBuilder.Append('+');
        stringBuilder.Append(value);
        fadingNumberText.SetText(stringBuilder);

        //Reset the Alpha because it is reused
        Color color = fadingNumberText.color;
        color.a = 1f;
        fadingNumberText.color = color;

        //Calculate a random Direction
        float randomAngle = Random.Range(minAngle, maxAngle);
        Vector3 randomDirection = Quaternion.Euler(0, 0, randomAngle) * Vector3.up;
        Vector3 targetPosition = spawnPivot.position + (randomDirection * moveDistance);

        //Tween to the calculated random position
        LeanTween.move(fadingNumberGameObject, targetPosition, duration)
            .setEase(easeType);

        //Make it fade out with tween
        LeanTween.alphaText(fadingNumberText.rectTransform, 0f, duration)
            .setEase(easeType)
            .setOnComplete(() => {
                LeanTween.cancel(fadingNumberGameObject);
                textPool.Release(fadingNumberGameObject);
            });
    }

    private void OnDestroy() {
        LeanTween.cancelAll();
    }
}
