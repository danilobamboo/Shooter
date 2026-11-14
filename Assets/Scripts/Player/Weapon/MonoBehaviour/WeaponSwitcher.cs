using System;
using UnityEngine;

public class WeaponSwitcher : MonoBehaviour
{
    [SerializeField] private Weapon[] weapons;
    [SerializeField] private int startingIndex = 0;
    [SerializeField] private Weapon[] extraWeaponsAtStart;

    public event Action<Weapon> OnWeaponChanged;

    public event Action<Weapon> OnWeaponUnlocked;

    public Weapon CurrentWeapon { get; private set; }

    private bool[] unlocked = new bool[0];
    private int currentIndex = -1;

    private void Awake()
    {
        if (weapons == null || weapons.Length == 0) return;

        unlocked = new bool[weapons.Length];

        unlocked[Mathf.Clamp(startingIndex, 0, weapons.Length - 1)] = true;

        if (extraWeaponsAtStart == null) return;

        foreach (Weapon weapon in extraWeaponsAtStart)
        {
            if (weapon == null) continue;

            int index = Array.IndexOf(weapons, weapon);
            if (index >= 0)
            {
                unlocked[index] = true;
            }
            else
            {
                Debug.LogWarning($"WeaponSwitcher: '{weapon.name}' está en Extra Weapons At Start pero no en la lista Weapons.", this);
            }
        }
    }

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
        if (Time.timeScale == 0f) return;

        for (int i = 0; i < weapons.Length; i++)
        {
            if (Input.GetKeyDown(KeyCode.Alpha1 + i))
            {
                Select(i);
                break;
            }
        }
    }

    public bool IsUnlocked(Weapon weapon)
    {
        int index = Array.IndexOf(weapons, weapon);
        return index >= 0 && unlocked[index];
    }

    public bool Unlock(Weapon weapon, bool equip)
    {
        if (weapon == null) return false;

        int index = Array.IndexOf(weapons, weapon);
        if (index < 0)
        {
            Debug.LogError($"WeaponSwitcher: '{weapon.name}' no está en la lista Weapons.", this);
            return false;
        }

        bool newlyUnlocked = !unlocked[index];
        unlocked[index] = true;

        if (newlyUnlocked)
        {
            OnWeaponUnlocked?.Invoke(weapon);
        }

        if (equip)
        {
            Select(index);
        }

        return newlyUnlocked;
    }

    public void Select(int index)
    {
        if (index == currentIndex || index < 0 || index >= weapons.Length) return;
        if (!unlocked[index]) return;

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