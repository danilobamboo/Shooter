using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AmmoHUD : MonoBehaviour
{
    [Header("Arma a mostrar")]
    [SerializeField] private HitscanWeapon weapon;

    [Header("Munición")]
    [SerializeField] private TMP_Text ammoText;

    [Header("Recarga")]
    [SerializeField] private GameObject reloadRoot;
    [SerializeField] private Image reloadFill;

    private void Awake()
    {
        reloadFill.type = Image.Type.Filled;
        reloadFill.fillMethod = Image.FillMethod.Radial360;
        reloadFill.fillOrigin = (int)Image.Origin360.Top;
        reloadFill.fillClockwise = true;
    }

    private void OnEnable()
    {
        if (weapon != null) weapon.OnAmmoChanged += UpdateAmmoText;
    }

    private void OnDisable()
    {
        if (weapon != null) weapon.OnAmmoChanged -= UpdateAmmoText;
    }

    private void Start()
    {
        if (weapon == null)
        {
            Debug.LogError("AmmoHUD: falta asignar el arma.", this);
            enabled = false;
            return;
        }

        UpdateAmmoText(weapon.CurrentAmmo, weapon.MagazineSize);
        reloadRoot.SetActive(false);
    }

    private void Update()
    {
        bool isReloading = weapon.IsReloading;

        if (reloadRoot.activeSelf != isReloading)
        {
            reloadRoot.SetActive(isReloading);
        }

        if (isReloading)
        {
            reloadFill.fillAmount = weapon.ReloadProgress;
        }
    }

    private void UpdateAmmoText(int current, int max)
    {
        ammoText.text = $"{current} / {max}";
    }
}