using System;
using System.Collections;
using TMPro;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class LevelIntro : MonoBehaviour
{
    [Serializable]
    public class IntroLine
    {
        public TMP_Text text;
        [TextArea(1, 3)] public string message = "Texto";
        public TMP_FontAsset font;
        public Color color = Color.white;

        [NonSerialized] public float restX;
        [NonSerialized] public float restY;
        [NonSerialized] public float offscreenX;
    }

    private const float MaxFrameTime = 0.1f;

    [Header("Pantalla negra")]
    [SerializeField] private CanvasGroup blackScreen;
    [SerializeField] private float fadeOutTime = 2f;

    [Header("Sonido")]
    [SerializeField] private AudioClip introSound;
    [Range(0f, 1f)]
    [SerializeField] private float soundVolume = 1f;

    [Header("Textos")]
    [SerializeField] private IntroLine leftText = new IntroLine();
    [SerializeField] private IntroLine rightText = new IntroLine();

    [Header("Tiempos (segundos)")]
    [SerializeField] private float startDelay = 0.5f;
    [SerializeField] private float textDelayAfterSound = 0.25f;
    [SerializeField] private float enterTime = 0.4f;
    [SerializeField] private float holdTime = 3.5f;
    [SerializeField] private float exitTime = 0.35f;
    [SerializeField] private float staggerDelay = 0.15f;

    [Header("Movimiento durante la lectura")]
    [SerializeField] private float driftDistance = 80f;

    [Header("Control del juego durante la intro")]
    [SerializeField] private bool freezeWorld = true;
    [SerializeField] private MonoBehaviour[] disableDuringIntro;

    [Header("Saltar")]
    [SerializeField] private bool allowSkip = true;
    [SerializeField] private KeyCode skipKey = KeyCode.Return;

    private AudioSource audioSource;
    private bool isLocked;
    private bool finished;

    private float Enter => Mathf.Max(enterTime, 0.01f);
    private float Hold => Mathf.Max(holdTime, 0.01f);
    private float Exit => Mathf.Max(exitTime, 0.01f);

    private void Awake()
    {
        if (blackScreen == null || leftText.text == null || rightText.text == null)
        {
            Debug.LogError("macana", this);
            enabled = false;
            return;
        }

        audioSource = GetComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 0f;

        blackScreen.gameObject.SetActive(true);
        blackScreen.alpha = 1f;
        leftText.text.enabled = false;
        rightText.text.enabled = false;

        Lock();
    }

    private void Start()
    {
        StartCoroutine(Run());
    }

    private void Update()
    {
        if (allowSkip && !finished && Input.GetKeyDown(skipKey))
        {
            Skip();
        }
    }

    private void OnDestroy()
    {
        Unlock();
    }

    private IEnumerator Run()
    {
        Canvas.ForceUpdateCanvases();
        PrepareLine(leftText, -1f);
        PrepareLine(rightText, 1f);

        yield return new WaitForSecondsRealtime(startDelay);

        if (introSound != null)
        {
            audioSource.clip = introSound;
            audioSource.volume = soundVolume;
            audioSource.Play();
        }

        yield return new WaitForSecondsRealtime(textDelayAfterSound);

        yield return AnimateTexts();

        Unlock();
        yield return FadeOut();

        Finish();
    }

    private IEnumerator AnimateTexts()
    {
        float duration = Enter + Hold + Exit + staggerDelay;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Mathf.Min(Time.unscaledDeltaTime, MaxFrameTime);

            AnimateLine(leftText, -1f, elapsed);
            AnimateLine(rightText, 1f, elapsed - staggerDelay);

            yield return null;
        }

        leftText.text.enabled = false;
        rightText.text.enabled = false;
    }

    private IEnumerator FadeOut()
    {
        float duration = Mathf.Max(fadeOutTime, 0.01f);
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Mathf.Min(Time.unscaledDeltaTime, MaxFrameTime);
            blackScreen.alpha = 1f - Mathf.SmoothStep(0f, 1f, elapsed / duration);
            yield return null;
        }

        blackScreen.alpha = 0f;
    }

    private void Finish()
    {
        finished = true;
        blackScreen.gameObject.SetActive(false);
    }

    private void Skip()
    {
        StopAllCoroutines();
        audioSource.Stop();

        leftText.text.enabled = false;
        rightText.text.enabled = false;

        Unlock();
        blackScreen.alpha = 0f;
        Finish();
    }

    private void PrepareLine(IntroLine line, float side)
    {
        TMP_Text t = line.text;
        t.text = line.message;
        t.color = line.color;
        if (line.font != null) t.font = line.font;

        RectTransform rt = t.rectTransform;
        RectTransform canvasRect = t.canvas.rootCanvas.GetComponent<RectTransform>();

        line.restX = rt.anchoredPosition.x;
        line.restY = rt.anchoredPosition.y;

        line.offscreenX = side * (canvasRect.rect.width * 0.5f + rt.rect.width * 0.5f + 50f);

        rt.anchoredPosition = new Vector2(line.offscreenX, line.restY);
        t.enabled = true;
    }

    private void AnimateLine(IntroLine line, float side, float t)
    {
        float inward = -side;
        float driftEndX = line.restX + inward * driftDistance;
        float x;

        if (t <= 0f)
        {
            x = line.offscreenX;
        }
        else if (t < Enter)
        {
            x = Mathf.Lerp(line.offscreenX, line.restX, EaseOut(t / Enter));
        }
        else if (t < Enter + Hold)
        {
            x = Mathf.Lerp(line.restX, driftEndX, (t - Enter) / Hold);
        }
        else if (t < Enter + Hold + Exit)
        {
            x = Mathf.Lerp(driftEndX, line.offscreenX, EaseIn((t - Enter - Hold) / Exit));
        }
        else
        {
            x = line.offscreenX;
        }

        line.text.rectTransform.anchoredPosition = new Vector2(x, line.restY);
    }

    private static float EaseOut(float t)
    {
        float inverse = 1f - Mathf.Clamp01(t);
        return 1f - inverse * inverse * inverse;
    }

    private static float EaseIn(float t)
    {
        t = Mathf.Clamp01(t);
        return t * t * t;
    }

    private void Lock()
    {
        isLocked = true;

        if (freezeWorld) Time.timeScale = 0f;
        SetBehavioursEnabled(false);
    }

    private void Unlock()
    {
        if (!isLocked) return;
        isLocked = false;

        if (freezeWorld) Time.timeScale = 1f;
        SetBehavioursEnabled(true);
    }

    private void SetBehavioursEnabled(bool value)
    {
        if (disableDuringIntro == null) return;

        foreach (MonoBehaviour behaviour in disableDuringIntro)
        {
            if (behaviour != null) behaviour.enabled = value;
        }
    }
}