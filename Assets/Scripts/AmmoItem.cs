using UnityEngine;

public class AmmoItem : MonoBehaviour
{
    [Header("Configuración de Munición")]
    [Tooltip("Cantidad de balas que otorga este cubo (Valor: 5).")]
    [SerializeField] private int ammoAmount = 5;

    // Detectar cuando el jugador entra en contacto con el cubo (usando Trigger)
    private void OnTriggerEnter(Collider other)
    {
        // Verificamos si el objeto que colisiona es el jugador
        if (other.CompareTag("Player"))
        {
            // Intentamos acceder al controlador del jugador
            PlayerController player = other.GetComponent<PlayerController>();
            if (player != null)
            {
                player.AddAmmo(ammoAmount);
                Debug.Log("¡Munición recogida! + " + ammoAmount + " balas.");

                // Desaparece el cubo de la escena
                Destroy(gameObject);
            }
        }
    }
}