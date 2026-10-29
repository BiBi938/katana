using System;
using System.Collections.Generic;
using UnityEngine;

public class EffectPlayer : MonoBehaviour
{
    [Serializable]
    public class Socket
    {
        public string name;
        public Transform point;
    }

    public List<Socket> sockets = new List<Socket>();

    Dictionary<string, Transform> socketLookup;

    void Awake()
    {
        socketLookup = new Dictionary<string, Transform>();
        foreach (Socket socket in sockets)
        {
            if (socket.point != null) socketLookup[socket.name] = socket.point;
        }
    }

    public GameObject Play(EffectDefinition def, bool flipX = false)
    {
        if (def == null || def.prefab == null) return null;

        Transform socket = ResolveSocket(def.socketName);
        Vector3 offset = new Vector3(flipX ? -def.localOffset.x : def.localOffset.x, def.localOffset.y, 0f);
        Vector3 position = (socket != null ? socket.position : transform.position) + offset;

        GameObject instance = EffectPoolManager.Instance.Spawn(def.prefab, position, Quaternion.identity);
        if (instance == null) return null;

        if (def.followSocket)
        {
            instance.transform.SetParent(socket != null ? socket : transform, true);
        }
        else
        {
            instance.transform.SetParent(null, true);
        }

        if (instance.TryGetComponent(out SpriteRenderer sr))
        {
            sr.flipX = flipX;
            if (!string.IsNullOrEmpty(def.sortingLayerName)) sr.sortingLayerName = def.sortingLayerName;
            sr.sortingOrder = def.sortingOrder;
        }

        if (instance.TryGetComponent(out EffectInstance effectInstance))
        {
            effectInstance.Begin(def.lifetime);
        }

        return instance;
    }

    Transform ResolveSocket(string socketName)
    {
        if (string.IsNullOrEmpty(socketName)) return transform;
        return socketLookup.TryGetValue(socketName, out Transform point) ? point : transform;
    }
}
