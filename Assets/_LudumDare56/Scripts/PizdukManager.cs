using UnityEngine;

public class PizdukManager : MonoBehaviour
{
    private void Awake()
    {
        gameObject.transform.localRotation = Quaternion.Euler(0,  Random.Range(0, 180), 0);
    }

    void OnCollisionEnter(Collision collision)
    {
        GamePlayManager.Instance.GameOver();
    }
}
