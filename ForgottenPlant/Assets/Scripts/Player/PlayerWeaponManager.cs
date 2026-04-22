using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerWeaponManager : MonoBehaviour
{
    [Header("References")]
    // List of all player weapon GameObjects that can be activated or deactivated.
    [SerializeField] private GameObject[] weapons;

    // HUD manager used to update the active weapon slot in the UI.
    [SerializeField] private PlayerHUDManager playerHUDManager;

    [Header("Start Weapon")]
    // Weapon index that should be active when the scene starts.
    [SerializeField] private int startWeaponIndex = 0;

    [Header("Debug")]
    // Enables debug logs when switching weapons.
    [SerializeField] private bool debugLogWeaponSwitch = false;

    // Input actions instance used for weapon switching input.
    private InputSystem_Actions controls;

    // Stores the currently active weapon index.
    private int currentWeaponIndex = -1;

    private void Awake()
    {
        // Create a new input action instance.
        controls = new InputSystem_Actions();

        // Bind next weapon input.
        controls.Player.Next.performed += _ => SelectNextWeapon();

        // Bind previous weapon input.
        controls.Player.Previous.performed += _ => SelectPreviousWeapon();

        // Bind mouse wheel / scroll input for weapon switching.
        controls.Player.Weaponscroll.performed += ctx => HandleWeaponScroll(ctx.ReadValue<Vector2>());
    }

    private void Start()
    {
        // Ensure all weapons are disabled before selecting the start weapon.
        DisableAllWeapons();

        if (weapons == null || weapons.Length == 0)
            return;

        // Clamp the start weapon index to a valid range.
        startWeaponIndex = Mathf.Clamp(startWeaponIndex, 0, weapons.Length - 1);

        // Activate the configured start weapon.
        SetActiveWeapon(startWeaponIndex);
    }

    private void OnEnable()
    {
        // Enable input handling when this component becomes active.
        controls.Enable();
    }

    private void OnDisable()
    {
        // Disable input handling when this component becomes inactive.
        controls.Disable();
    }

    private void HandleWeaponScroll(Vector2 scrollValue)
    {
        // Scroll up selects the next weapon.
        if (scrollValue.y > 0.01f)
        {
            SelectNextWeapon();
        }
        // Scroll down selects the previous weapon.
        else if (scrollValue.y < -0.01f)
        {
            SelectPreviousWeapon();
        }
    }

    private void SelectNextWeapon()
    {
        // Stop if no weapons are configured.
        if (weapons == null || weapons.Length == 0)
            return;

        // Move to the next weapon and wrap around at the end of the list.
        int nextIndex = currentWeaponIndex + 1;

        if (nextIndex >= weapons.Length)
            nextIndex = 0;

        SetActiveWeapon(nextIndex);
    }

    private void SelectPreviousWeapon()
    {
        // Stop if no weapons are configured.
        if (weapons == null || weapons.Length == 0)
            return;

        // Move to the previous weapon and wrap around at the start of the list.
        int previousIndex = currentWeaponIndex - 1;

        if (previousIndex < 0)
            previousIndex = weapons.Length - 1;

        SetActiveWeapon(previousIndex);
    }

    private void SetActiveWeapon(int index)
    {
        // Stop if the weapon array is invalid.
        if (weapons == null || weapons.Length == 0)
            return;

        // Stop if the requested index is outside the valid range.
        if (index < 0 || index >= weapons.Length)
            return;

        // Activate only the selected weapon and disable all others.
        for (int i = 0; i < weapons.Length; i++)
        {
            if (weapons[i] != null)
                weapons[i].SetActive(i == index);
        }

        // Store the currently active weapon index.
        currentWeaponIndex = index;

        // Update the HUD weapon slot highlight if available.
        if (playerHUDManager != null)
        {
            playerHUDManager.SetActiveWeaponSlot(index);
        }

        if (debugLogWeaponSwitch && weapons[index] != null)
        {
            Debug.Log($"{name}: Active weapon = {weapons[index].name}");
        }
    }

    private void DisableAllWeapons()
    {
        // Stop if no weapons are assigned.
        if (weapons == null)
            return;

        // Disable every configured weapon.
        for (int i = 0; i < weapons.Length; i++)
        {
            if (weapons[i] != null)
                weapons[i].SetActive(false);
        }
    }
}