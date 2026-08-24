using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR.Interaction.Toolkit;

/// <summary>
/// Maps a 0-1 console control onto the magnet's lift speed, and trips the
/// magnet if the trainee runs over the load rating while carrying a box.
/// </summary>
public class MagnetSpeedControl : MonoBehaviour
{
    [Header("Speed")]
    [Tooltip("The magnet's mover - speed is written straight into this")]
    public MoveWithVelocity magnet = null;
    public float minSpeed = 1.0f;
    public float maxSpeed = 10.0f;

    [Tooltip("Throttle position the Reset button returns to")]
    [Range(0, 1)] public float defaultValue = 0.44f;

    [Header("Load rating")]
    [Tooltip("The socket that holds the box")]
    public XRSocketInteractor socket = null;

    [Tooltip("Fastest safe lift while carrying a box")]
    public float ratedSpeed = 6.0f;

    [Tooltip("Seconds of overspeed allowed before the magnet trips")]
    public float graceSeconds = 2.0f;

    [Header("Readout")]
    public TMPro.TextMeshProUGUI readout = null;
    public UnityEngine.UI.Image bar = null;
    public Color safeColor = new Color(0.20f, 0.72f, 0.45f);
    public Color warnColor = new Color(0.90f, 0.63f, 0.13f);

    // Overspeed with a load - alarm, amber readout
    public UnityEvent OnWarning = new UnityEvent();

    // Held it too long - drop the box, sparks
    public UnityEvent OnTrip = new UnityEvent();

    // Back inside the rating
    public UnityEvent OnSafe = new UnityEvent();

    public float Speed { get; private set; }

    private float overspeedTime = 0.0f;
    private bool warning = false;

    public static float SpeedFor(float value, float min, float max)
    {
        return Mathf.Lerp(min, max, Mathf.Clamp01(value));
    }

    private void Start()
    {
        SetSpeed(defaultValue);
    }

    // Hook this to XRSlider.OnValueChange
    public void SetSpeed(float value)
    {
        Speed = SpeedFor(value, minSpeed, maxSpeed);
        magnet.speed = Speed;

        if (readout)
            readout.text = string.Format("LIFT  {0:0.0} m/s", Speed);

        if (bar)
        {
            bar.fillAmount = Mathf.Clamp01(value);
            bar.color = Speed > ratedSpeed ? warnColor : safeColor;
        }
    }

    public void ResetSpeed()
    {
        overspeedTime = 0.0f;
        warning = false;
        SetSpeed(defaultValue);
    }

    private void Update()
    {
        bool carrying = socket && socket.hasSelection;

        if (carrying && Speed > ratedSpeed)
        {
            if (!warning)
            {
                warning = true;
                OnWarning.Invoke();
            }

            overspeedTime += Time.deltaTime;

            if (overspeedTime >= graceSeconds)
            {
                overspeedTime = 0.0f;
                warning = false;
                OnTrip.Invoke();
            }
        }
        else if (warning)
        {
            warning = false;
            overspeedTime = 0.0f;
            OnSafe.Invoke();
        }
    }

    // Right-click the component -> Self check. Fails loudly in the Console
    // if the speed mapping ever stops doing what the readout claims.
    [ContextMenu("Self check")]
    private void SelfCheck()
    {
        Debug.Assert(Mathf.Approximately(SpeedFor(0.0f, 1f, 10f), 1f), "min");
        Debug.Assert(Mathf.Approximately(SpeedFor(1.0f, 1f, 10f), 10f), "max");
        Debug.Assert(Mathf.Approximately(SpeedFor(0.5f, 1f, 11f), 6f), "midpoint");
        Debug.Assert(Mathf.Approximately(SpeedFor(2.0f, 1f, 10f), 10f), "clamp");
        Debug.Log("MagnetSpeedControl: mapping OK");
    }
}