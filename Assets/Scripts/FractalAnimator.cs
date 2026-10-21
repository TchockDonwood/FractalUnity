using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class FractalAnimator : MonoBehaviour
{
    public FractalController controller;
    public Button animationButton;
    public TMP_Text animationButtonLabel;
    public float interval = 0.7f;

    private Coroutine _routine;
    private bool _running;

    void Start()
    {
        animationButton.onClick.AddListener(Toggle);
        UpdateLabel();
    }

    private void Toggle()
    {
        if (_running) StopAnim();
        else StartAnim();
    }

    private void StartAnim()
    {
        _running = true;
        _routine = StartCoroutine(Loop());
        UpdateLabel();
    }

    private void StopAnim()
    {
        _running = false;
        if (_routine != null) StopCoroutine(_routine);
        _routine = null;
        UpdateLabel();
    }

    private IEnumerator Loop()
    {
        int it = controller.CurrentIterations;
        int max = controller.MaxIterations;
        int min = controller.MinIterations;
        int dir = it >= max ? -1 : 1;

        while (true)
        {
            yield return new WaitForSeconds(interval);
            it += dir;
            if (it >= max) { it = max; dir = -1; }
            else if (it <= min) { it = min; dir = 1; }
            controller.SetIterations(it);
        }
    }

    private void UpdateLabel()
    {
        if (animationButtonLabel)
            animationButtonLabel.text = _running ? "Стоп" : "Анимация";
    }

    void OnDisable() => StopAnim();
}