using UnityEngine;

public class PickupItem : MonoBehaviour
{
    [SerializeField] GuardAIController[] guards;
    [SerializeField] Gun gun;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) 
            return;

        GameManager.instance.CollectedItem();
        GameManager.instance.CallAlerRoutin();
        gameObject.SetActive(false);
    }  
}
