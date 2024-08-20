using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class UnityMainThreadDispatcher : MonoBehaviour
{
    private static UnityMainThreadDispatcher _instance = null;

    private readonly Queue<Action> _executionQueue = new Queue<Action>();

    public static UnityMainThreadDispatcher Instance()
    {
        if (!_instance)
        {
            _instance = FindObjectOfType(typeof(UnityMainThreadDispatcher)) as UnityMainThreadDispatcher;
            if (!_instance)
            {
                GameObject obj = new GameObject("UnityMainThreadDispatcher");
                _instance = obj.AddComponent<UnityMainThreadDispatcher>();
                DontDestroyOnLoad(obj);
            }
        }
        return _instance;
    }

    void Update()
    {
        lock (_executionQueue)
        {
            while (_executionQueue.Count > 0)
            {
                _executionQueue.Dequeue().Invoke();
            }
        }
    }

    public void Enqueue(Action action)
    {
        lock (_executionQueue)
        {
            _executionQueue.Enqueue(action);
        }
    }

    public void EnqueueAsync(Func<IEnumerator> action)
    {
        Enqueue(() => StartCoroutine(action()));
    }
}