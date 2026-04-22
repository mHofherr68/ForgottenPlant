using UnityEngine;

public class PlayerHUDManager : MonoBehaviour
{
    [Header("Weapon HUD Slots")]
    // Array of HUD slot GameObjects used to display the currently selected weapon.
    [SerializeField] private GameObject[] weaponSlots;

    [Header("Debug")]
    // Enables debug logs when the active HUD slot changes.
    [SerializeField] private bool debugLogHudSwitch = false;

    private void Start()
    {
        // Stop if no HUD slots are assigned.
        if (weaponSlots == null || weaponSlots.Length == 0)
            return;

        // Activate only the first HUD slot at startup.
        for (int i = 0; i < weaponSlots.Length; i++)
        {
            if (weaponSlots[i] != null)
                weaponSlots[i].SetActive(i == 0);
        }

        if (debugLogHudSwitch && weaponSlots[0] != null)
        {
            Debug.Log($"{name}: Active HUD slot = {weaponSlots[0].name}");
        }
    }

    public void SetActiveWeaponSlot(int index)
    {
        // Stop if no HUD slots are configured.
        if (weaponSlots == null || weaponSlots.Length == 0)
            return;

        // Stop if the requested index is outside the valid range.
        if (index < 0 || index >= weaponSlots.Length)
            return;

        // Activate only the HUD slot that matches the selected weapon index.
        for (int i = 0; i < weaponSlots.Length; i++)
        {
            if (weaponSlots[i] != null)
                weaponSlots[i].SetActive(i == index);
        }

        if (debugLogHudSwitch && weaponSlots[index] != null)
        {
            Debug.Log($"{name}: Active HUD slot = {weaponSlots[index].name}");
        }
    }
}