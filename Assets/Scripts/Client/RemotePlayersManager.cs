using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

public class RemotePlayersManager : MonoBehaviour
{
    [SerializeField] private TCPClient tcpClient;
    [SerializeField] private GameObject remotePrefab;
    [SerializeField] private string remotePrefix = "remote_";
    private readonly Dictionary<string, GameObject> remotes = new Dictionary<string, GameObject>();

    private void Start()
    {
        if (tcpClient == null) tcpClient = GetComponent<TCPClient>();
        if (tcpClient != null) tcpClient.OnDataReceived += OnMessage;
    }

    private void OnDestroy()
    {
        if (tcpClient != null) tcpClient.OnDataReceived -= OnMessage;
    }

    private void OnMessage(string msg)
    {
        if (string.IsNullOrEmpty(msg)) return;

        if (msg == "SERVER_SHUTDOWN")
        {
            try { tcpClient.Disconnect(); } catch { }
            ClearAllRemotes();
            return;
        }

        var parts = msg.Split(new[] { '|' }, 2);
        if (parts.Length != 2) return;
        var id = parts[0];
        var data = parts[1];
        if (string.IsNullOrEmpty(id)) return;
        if (id == ClientPlayer.LocalClientId)
        {
            if (data == "DISCONNECT") ClearAllRemotes();
            return;
        }

        if (data == "DISCONNECT")
        {
            RemoveRemote(id);
            return;
        }

        var coords = data.Split(',');
        if (coords.Length < 3) return;
        if (!float.TryParse(coords[0], System.Globalization.NumberStyles.Float, CultureInfo.InvariantCulture, out var x)) return;
        if (!float.TryParse(coords[1], System.Globalization.NumberStyles.Float, CultureInfo.InvariantCulture, out var y)) return;
        if (!float.TryParse(coords[2], System.Globalization.NumberStyles.Float, CultureInfo.InvariantCulture, out var z)) return;
        var pos = new Vector3(x, y, z);

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

    private void ClearAllRemotes()
    {
        foreach (var kv in remotes)
        {
            try { if (kv.Value != null) Destroy(kv.Value); } catch { }
        }
        remotes.Clear();
    }
}