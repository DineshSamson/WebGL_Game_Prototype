using UnityEngine;

public class PickupItem : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) 
            return;

        //Do next success message
        gameObject.SetActive(false);
        
    }
}
