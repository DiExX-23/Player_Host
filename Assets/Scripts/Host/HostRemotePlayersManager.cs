using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using UnityEngine;

public class HostRemotePlayersManager : MonoBehaviour
{
    [SerializeField] private TCPServer tcpServer;
    [SerializeField] private GameObject remotePrefab;
    [SerializeField] private string remotePrefix = "client_";
    private readonly Dictionary<string, GameObject> remotes = new Dictionary<string, GameObject>();
    private readonly Queue<Action> mainThreadActions = new Queue<Action>();
    private readonly object queueLock = new object();

    private void Start()
    {
        if (tcpServer == null) tcpServer = GetComponent<TCPServer>();
        if (tcpServer != null) tcpServer.OnDataReceived += OnMessage;
    }

    private void OnDestroy()
    {
        if (tcpServer != null) tcpServer.OnDataReceived -= OnMessage;
    }

    private void Update()
    {
        while (true)
        {
            Action act = null;
            lock (queueLock)
            {
                if (mainThreadActions.Count > 0) act = mainThreadActions.Dequeue();
            }
            if (act == null) break;
            try { act(); } catch { }
        }
    }

    private void OnMessage(string msg)
    {
        try
        {
            if (string.IsNullOrEmpty(msg)) return;

            // Global server shutdown message handled elsewhere (clients). Host ignores it here.
            if (msg == "SERVER_SHUTDOWN") return;

            var parts = msg.Split(new[] { '|' }, 2);
            if (parts.Length != 2) return;
            var id = parts[0];
            var data = parts[1];
            if (string.IsNullOrEmpty(id)) return;
            if (id == "HOST") return;

            if (data == "DISCONNECT")
            {
                lock (queueLock) mainThreadActions.Enqueue(() => RemoveRemote(id));
                return;
            }

            var coords = data.Split(',');
            if (coords.Length < 3) return;
            if (!float.TryParse(coords[0], System.Globalization.NumberStyles.Float, CultureInfo.InvariantCulture, out var x)) return;
            if (!float.TryParse(coords[1], System.Globalization.NumberStyles.Float, CultureInfo.InvariantCulture, out var y)) return;
            if (!float.TryParse(coords[2], System.Globalization.NumberStyles.Float, CultureInfo.InvariantCulture, out var z)) return;
            var pos = new Vector3(x, y, z);
            lock (queueLock)
            {
                mainThreadActions.Enqueue(() => ApplyRemotePosition(id, pos));
            }
        }
        catch { }
    }

    private void ApplyRemotePosition(string id, Vector3 pos)
    {
        if (!remotes.TryGetValue(id, out var go) || go == null)
        {
            if (remotePrefab != null) go = Instantiate(remotePrefab);
            else go = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            go.name = remotePrefix + id;
            go.transform.localScale = Vector3.one * 0.9f;
            remotes[id] = go;
        }
        go.transform.position = pos;
    }

    private void RemoveRemote(string id)
    {
        if (remotes.TryGetValue(id, out var go) && go != null)
        {
            remotes.Remove(id);
            try { Destroy(go); } catch { }
        }
    }
}