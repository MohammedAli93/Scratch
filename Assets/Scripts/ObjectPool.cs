using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class ObjectPool<T> : MonoBehaviour where T : MonoBehaviour {

    [SerializeField] GameObject opPrefab;
    [SerializeField] bool singleton = true;
    Queue<T> pool = new Queue<T> ();
    public static ObjectPool<T> Instance;

    bool isQuitting = false;

    protected internal UnityEvent<T> OnGetFromPool = new UnityEvent<T> ();

    protected virtual void Awake () {
        if (singleton) Instance = this;
        Application.quitting += Quitting;
    }

    public virtual T GetFromObjectPool () {
        T opObject = default (T);

        if (pool.Count > 0) {
            opObject = pool.Dequeue ();
        }
        if (opObject == null) {
            opObject = Instantiate (opPrefab, transform).GetComponent<T> ();
        }
        opObject.gameObject.SetActive (true);

        OnGetFromPool.Invoke (opObject);

        return opObject;
    }

    public virtual void ReturnToObjectPool (T opObject) {
        if (isQuitting) return;
        pool.Enqueue (opObject);
        opObject?.transform.SetParent (transform, false);
        opObject?.gameObject.SetActive (false);
    }

    void Quitting () {
        isQuitting = true;
    }

}