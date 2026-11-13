using System;
using UnityEngine;

public class WeaponSwitcher : MonoBehaviour
{
    [Tooltip("Armas en orden: la tecla 1 elige la primera, la 2 la segunda, etc.")]
    [SerializeField] private Weapon[] weapons;
    [Tooltip("Índice del arma con la que se empieza (0 = la primera).")]
    [SerializeField] private int startingIndex = 0;

    public event Action<Weapon> OnWeaponChanged;

    public Weapon CurrentWeapon { get; private set; }

    private int currentIndex = -1;

    private void Start()
    {
        if (weapons == null || weapons.Length == 0)
        {
            Debug.LogError("WeaponSwitcher: no hay armas asignadas.", this);
            enabled = false;
            return;
        }

        for (int i = 0; i < weapons.Length; i++)
        {
            weapons[i].gameObject.SetActive(false);
        }

        Select(Mathf.Clamp(startingIndex, 0, weapons.Length - 1));
    }

    private void Update()
    {
        for (int i = 0; i < weapons.Length; i++)
        {
            if (Input.GetKeyDown(KeyCode.Alpha1 + i))
            {
                Select(i);
                break;
            }
        }
    }

    public void Select(int index)
    {
        if (index == currentIndex || index < 0 || index >= weapons.Length) return;

        if (currentIndex >= 0)
        {
            weapons[currentIndex].gameObject.SetActive(false);
        }

        currentIndex = index;
        CurrentWeapon = weapons[index];
        CurrentWeapon.gameObject.SetActive(true);

        OnWeaponChanged?.Invoke(CurrentWeapon);
    }
}