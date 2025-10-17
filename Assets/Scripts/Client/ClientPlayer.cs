using System;
using System.Globalization;
using System.Reflection;
using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class ClientPlayer : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 5f;
    public static string LocalClientId { get; private set; }

    private Type keyboardType;
    private PropertyInfo keyboardCurrentProp;
    private object keyboardCurrentInstance;

    private Rigidbody rb;
    private Vector3 pendingDelta = Vector3.zero;

    private TCPClient tcpClient;
    private float sendInterval = 0.1f;

    private void Awake()
    {
        var tcp = FindFirstObjectByType<TCPClient>();
        if (tcp != null && string.IsNullOrEmpty(LocalClientId))
        {
            LocalClientId = Guid.NewGuid().ToString();
        }
    }

    private void Start()
    {
        tcpClient = FindFirstObjectByType<TCPClient>();

        keyboardType = FindType("UnityEngine.InputSystem.Keyboard");
        if (keyboardType != null)
        {
            keyboardCurrentProp = keyboardType.GetProperty("current", BindingFlags.Static | BindingFlags.Public);
            if (keyboardCurrentProp != null)
                keyboardCurrentInstance = keyboardCurrentProp.GetValue(null);
        }

        rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.useGravity = false;
            rb.constraints = RigidbodyConstraints.FreezeRotation;
            rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
            rb.interpolation = RigidbodyInterpolation.Interpolate;
        }

        StartCoroutine(SendPositionLoop());
    }

    private void Update()
    {
        Vector2 delta2 = Vector2.zero;

        if (IsPressed(KeyCode.UpArrow, "upArrowKey")) delta2 += Vector2.up;
        if (IsPressed(KeyCode.DownArrow, "downArrowKey")) delta2 += Vector2.down;
        if (IsPressed(KeyCode.LeftArrow, "leftArrowKey")) delta2 += Vector2.left;
        if (IsPressed(KeyCode.RightArrow, "rightArrowKey")) delta2 += Vector2.right;

        var delta = new Vector3(delta2.x, delta2.y, 0f).normalized * moveSpeed * Time.deltaTime;
        pendingDelta = delta;
    }

    private void FixedUpdate()
    {
        if (rb == null) return;
        rb.MovePosition(rb.position + pendingDelta);
    }

    private IEnumerator SendPositionLoop()
    {
        while (true)
        {
            yield return new WaitForSeconds(sendInterval);
            if (tcpClient != null && !string.IsNullOrEmpty(LocalClientId))
            {
                Vector3 p = transform.position;
                string msg = $"POS|{LocalClientId}|{p.x.ToString(CultureInfo.InvariantCulture)}|{p.y.ToString(CultureInfo.InvariantCulture)}|{p.z.ToString(CultureInfo.InvariantCulture)}";
                tcpClient.Send(msg);
            }
        }
    }

    private bool IsPressed(KeyCode kc, string inputSystemPropName)
    {
        try
        {
            return UnityEngine.Input.GetKey(kc);
        }
        catch
        {
            try
            {
                if (keyboardCurrentInstance == null || keyboardType == null) return false;
                var keyProp = keyboardType.GetProperty(inputSystemPropName, BindingFlags.Instance | BindingFlags.Public);
                if (keyProp == null) return false;
                var keyControl = keyProp.GetValue(keyboardCurrentInstance);
                if (keyControl == null) return false;
                var isPressedProp = keyControl.GetType().GetProperty("isPressed", BindingFlags.Instance | BindingFlags.Public);
                if (isPressedProp == null) return false;
                var val = isPressedProp.GetValue(keyControl);
                return val is bool b && b;
            }
            catch
            {
                return false;
            }
        }
    }

    private Type FindType(string fullname)
    {
        foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
        {
            try
            {
                var t = asm.GetType(fullname);
                if (t != null) return t;
            }
            catch { }
        }
        return null;
    }
}