// HostPlayer.cs
using System;
using System.Globalization;
using System.Reflection;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class HostPlayer : MonoBehaviour
{
    [SerializeField] private TCPServer tcpServer;
    [SerializeField] private TCPServerUI tcpServerUI;
    [SerializeField] private float moveSpeed = 5f;
    private const string HostId = "HOST";

    private Type keyboardType;
    private PropertyInfo keyboardCurrentProp;
    private object keyboardCurrentInstance;

    private Rigidbody rb;
    private Vector3 pendingDelta = Vector3.zero;

    private void Start()
    {
        if (tcpServer == null) tcpServer = GetComponent<TCPServer>();
        if (tcpServerUI == null) tcpServerUI = GetComponent<TCPServerUI>();
        if (tcpServer != null && tcpServerUI != null) tcpServer.StartServer(tcpServerUI.serverPort);

        keyboardType = FindType("UnityEngine.InputSystem.Keyboard");
        if (keyboardType != null)
        {
            keyboardCurrentProp = keyboardType.GetProperty("current", BindingFlags.Static | BindingFlags.Public);
            if (keyboardCurrentProp != null) keyboardCurrentInstance = keyboardCurrentProp.GetValue(null);
        }

        rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.useGravity = false;
            rb.constraints = RigidbodyConstraints.FreezeRotation;
            rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
            rb.interpolation = RigidbodyInterpolation.Interpolate;
        }
    }

    private void Update()
    {
        Vector2 delta2 = Vector2.zero;
        if (IsPressed(KeyCode.W, "wKey")) delta2 += Vector2.up;
        if (IsPressed(KeyCode.S, "sKey")) delta2 += Vector2.down;
        if (IsPressed(KeyCode.A, "aKey")) delta2 += Vector2.left;
        if (IsPressed(KeyCode.D, "dKey")) delta2 += Vector2.right;

        var delta = new Vector3(delta2.x, delta2.y, 0f).normalized * moveSpeed * Time.deltaTime;
        pendingDelta = delta;
    }

    private void FixedUpdate()
    {
        if (rb == null) return;

        Vector3 newPos = rb.position + pendingDelta;
        rb.MovePosition(newPos);

        string pos = newPos.x.ToString(CultureInfo.InvariantCulture) + "," +
                     newPos.y.ToString(CultureInfo.InvariantCulture) + "," +
                     newPos.z.ToString(CultureInfo.InvariantCulture);
        if (tcpServer != null) tcpServer.Broadcast(HostId + "|" + pos);
    }

    private void OnDestroy()
    {
        if (tcpServer != null) tcpServer.StopServer();
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
                if (val is bool b) return b;
                return false;
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