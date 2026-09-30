using UnityEngine;

public class PruebaAbsoluta : MonoBehaviour
{
    void Update()
    {
        if (Input.GetKey(KeyCode.W))
        {
            transform.Translate(Vector3.forward * 5f * Time.deltaTime);
            Debug.Log("¡Me estoy moviendo con Translate!");
        }
    }
}