using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AmmoHUD : MonoBehaviour
{
    [Header("Gestor de armas")]
    [SerializeField] private WeaponSwitcher switcher;

    [Header("Munición")]
    [SerializeField] private TMP_Text ammoText;

    [Header("Recarga")]
    [SerializeField] private GameObject reloadRoot;
    [SerializeField] private Image reloadFill;

    private HitscanWeapon currentWeapon;

    private void Awake()
    {
        reloadFill.type = Image.Type.Filled;
        reloadFill.fillMethod = Image.FillMethod.Radial360;
        reloadFill.fillOrigin = (int)Image.Origin360.Top;
        reloadFill.fillClockwise = true;

        reloadRoot.SetActive(false);
    }

    private void OnEnable()
    {
        if (switcher == null)
        {
            Debug.LogError("AmmoHUD: falta asignar el WeaponSwitcher.", this);
            return;
        }

        switcher.OnWeaponChanged += HandleWeaponChanged;

        if (currentWeapon != null) currentWeapon.OnAmmoChanged += UpdateAmmoText;
    }

    private void OnDisable()
    {
        if (switcher != null) switcher.OnWeaponChanged -= HandleWeaponChanged;
        if (currentWeapon != null) currentWeapon.OnAmmoChanged -= UpdateAmmoText;
    }

    private void HandleWeaponChanged(HitscanWeapon newWeapon)
    {
        if (currentWeapon != null) currentWeapon.OnAmmoChanged -= UpdateAmmoText;

        currentWeapon = newWeapon;
        currentWeapon.OnAmmoChanged += UpdateAmmoText;

        UpdateAmmoText(currentWeapon.CurrentAmmo, currentWeapon.MagazineSize);
    }

    private void Update()
    {
        if (currentWeapon == null) return;

        bool isReloading = currentWeapon.IsReloading;

        if (reloadRoot.activeSelf != isReloading)
        {
            reloadRoot.SetActive(isReloading);
        }

        if (isReloading)
        {
            reloadFill.fillAmount = currentWeapon.ReloadProgress;
        }
    }

    private void UpdateAmmoText(int current, int max)
    {
        ammoText.text = $"{current} / {max}";
    }
}