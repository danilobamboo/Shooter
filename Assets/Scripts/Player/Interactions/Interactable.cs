using UnityEngine;
using UnityEngine.Events;

public class Interactable : MonoBehaviour
{
    public enum ActivationMode
    {
        OnTouch,
        PressKey
    }

    public enum RemoveMode
    {
        Destroy,
        Deactivate
    }

    [Header("Cómo se activa")]
    [SerializeField] private ActivationMode activation = ActivationMode.PressKey;
    [SerializeField] private KeyCode interactKey = KeyCode.E;
    [SerializeField] private GameObject prompt;

    [Header("Efecto 1: hacer desaparecer objetos")]
    [SerializeField] private GameObject[] objectsToRemove;
    [SerializeField] private RemoveMode removeMode = RemoveMode.Destroy;

    [Header("Efecto 2: desbloquear un arma")]
    [SerializeField] private Weapon weaponToUnlock;
    [SerializeField] private WeaponSwitcher weaponSwitcher;
    [SerializeField] private bool equipWeaponOnUnlock = true;

    [Header("Después de usarlo")]
    [SerializeField] private bool removeSelfAfterUse = true;
    [SerializeField] private UnityEvent onInteracted;

    private bool playerInside;
    private bool used;

    private void Reset()
    {
        Collider zone = GetComponent<Collider>();
        if (zone != null) zone.isTrigger = true;
    }

    private void Awake()
    {
        Collider zone = GetComponent<Collider>();
        if (zone == null)
        {
            Debug.LogError("Interactable: necesita un Collider que marque la zona de interacción.", this);
            enabled = false;
            return;
        }
        zone.isTrigger = true;

        if (prompt != null) prompt.SetActive(false);

        if (weaponToUnlock != null)
        {
            if (weaponSwitcher == null)
            {
                weaponSwitcher = weaponToUnlock.GetComponentInParent<WeaponSwitcher>(true);
            }

            if (weaponSwitcher == null)
            {
                Debug.LogError("Interactable: no se encontró el WeaponSwitcher. Asignalo en el campo Weapon Switcher.", this);
            }
        }

        bool removesSomething = objectsToRemove != null && objectsToRemove.Length > 0;
        if (!removesSomething && weaponToUnlock == null)
        {
            Debug.LogWarning("Interactable: no tiene ningún efecto configurado (ni objetos para quitar ni arma).", this);
        }
    }

    private void Update()
    {
        if (activation == ActivationMode.PressKey && playerInside && !used
            && Time.timeScale > 0f && Input.GetKeyDown(interactKey))
        {
            Interact();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (used || !other.CompareTag("Player")) return;

        if (activation == ActivationMode.OnTouch)
        {
            Interact();
            return;
        }

        playerInside = true;
        if (prompt != null) prompt.SetActive(true);
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        playerInside = false;
        if (prompt != null) prompt.SetActive(false);
    }

    private void Interact()
    {
        if (used) return;
        used = true;

        if (prompt != null) prompt.SetActive(false);

        if (objectsToRemove != null)
        {
            foreach (GameObject target in objectsToRemove)
            {
                Remove(target);
            }
        }

        if (weaponToUnlock != null && weaponSwitcher != null)
        {
            weaponSwitcher.Unlock(weaponToUnlock, equipWeaponOnUnlock);
        }

        onInteracted?.Invoke();

        if (removeSelfAfterUse)
        {
            Remove(gameObject);
        }
    }

    private void Remove(GameObject target)
    {
        if (target == null) return;

        if (removeMode == RemoveMode.Destroy)
        {
            Destroy(target);
        }
        else
        {
            target.SetActive(false);
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (objectsToRemove == null) return;

        Gizmos.color = Color.red;
        foreach (GameObject target in objectsToRemove)
        {
            if (target == null) continue;

            Gizmos.DrawLine(transform.position, target.transform.position);
            Gizmos.DrawWireSphere(target.transform.position, 0.3f);
        }
    }
}