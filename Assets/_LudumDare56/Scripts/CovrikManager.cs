using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CovrikManager : MonoBehaviour
{
    private HashSet<Collider> colliders = new HashSet<Collider>();

    private void OnTriggerEnter(Collider collider)
    {
        colliders.Add(collider);
    }

    private void OnTriggerExit(Collider collider)
    {
        colliders.Remove(collider);
    }    

    private void Update()
    {
        if (colliders.Count == 2)
        {
            GamePlayManager.Instance.GameFinihsed();
        }

        GamePlayManager.Instance.SetCovrikColliderSet(colliders);
    }
}
