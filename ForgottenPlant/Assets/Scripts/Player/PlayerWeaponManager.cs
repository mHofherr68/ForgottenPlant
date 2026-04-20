//using UnityEngine;
//using UnityEngine.InputSystem;

//public class PlayerWeaponManager : MonoBehaviour
//{
//    [Header("References")]
//    [SerializeField] private GameObject[] weapons;

//    [Header("Start Weapon")]
//    [SerializeField] private int startWeaponIndex = 0;

//    [Header("Debug")]
//    [SerializeField] private bool debugLogWeaponSwitch = false;

//    private InputSystem_Actions controls;
//    private int currentWeaponIndex = -1;

//    private void Awake()
//    {
//        controls = new InputSystem_Actions();

//        controls.Player.Next.performed += _ => SelectNextWeapon();
//        controls.Player.Previous.performed += _ => SelectPreviousWeapon();
//        controls.Player.Weaponscroll.performed += ctx => HandleWeaponScroll(ctx.ReadValue<Vector2>());
//    }

//    private void Start()
//    {
//        DisableAllWeapons();

//        if (weapons == null || weapons.Length == 0)
//            return;

//        startWeaponIndex = Mathf.Clamp(startWeaponIndex, 0, weapons.Length - 1);
//        SetActiveWeapon(startWeaponIndex);
//    }

//    private void OnEnable()
//    {
//        controls.Enable();
//    }

//    private void OnDisable()
//    {
//        controls.Disable();
//    }

//    private void HandleWeaponScroll(Vector2 scrollValue)
//    {
//        if (scrollValue.y > 0.01f)
//        {
//            SelectNextWeapon();
//        }
//        else if (scrollValue.y < -0.01f)
//        {
//            SelectPreviousWeapon();
//        }
//    }

//    private void SelectNextWeapon()
//    {
//        if (weapons == null || weapons.Length == 0)
//            return;

//        int nextIndex = currentWeaponIndex + 1;

//        if (nextIndex >= weapons.Length)
//            nextIndex = 0;

//        SetActiveWeapon(nextIndex);
//    }

//    private void SelectPreviousWeapon()
//    {
//        if (weapons == null || weapons.Length == 0)
//            return;

//        int previousIndex = currentWeaponIndex - 1;

//        if (previousIndex < 0)
//            previousIndex = weapons.Length - 1;

//        SetActiveWeapon(previousIndex);
//    }

//    private void SetActiveWeapon(int index)
//    {
//        if (weapons == null || weapons.Length == 0)
//            return;

//        if (index < 0 || index >= weapons.Length)
//            return;

//        for (int i = 0; i < weapons.Length; i++)
//        {
//            if (weapons[i] != null)
//                weapons[i].SetActive(i == index);
//        }

//        currentWeaponIndex = index;

//        if (debugLogWeaponSwitch && weapons[index] != null)
//        {
//            Debug.Log($"{name}: Active weapon = {weapons[index].name}");
//        }
//    }

//    private void DisableAllWeapons()
//    {
//        if (weapons == null)
//            return;

//        for (int i = 0; i < weapons.Length; i++)
//        {
//            if (weapons[i] != null)
//                weapons[i].SetActive(false);
//        }
//    }
//}
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerWeaponManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject[] weapons;
    [SerializeField] private PlayerHUDManager playerHUDManager;

    [Header("Start Weapon")]
    [SerializeField] private int startWeaponIndex = 0;

    [Header("Debug")]
    [SerializeField] private bool debugLogWeaponSwitch = false;

    private InputSystem_Actions controls;
    private int currentWeaponIndex = -1;

    private void Awake()
    {
        controls = new InputSystem_Actions();

        controls.Player.Next.performed += _ => SelectNextWeapon();
        controls.Player.Previous.performed += _ => SelectPreviousWeapon();
        controls.Player.Weaponscroll.performed += ctx => HandleWeaponScroll(ctx.ReadValue<Vector2>());
    }

    private void Start()
    {
        DisableAllWeapons();

        if (weapons == null || weapons.Length == 0)
            return;

        startWeaponIndex = Mathf.Clamp(startWeaponIndex, 0, weapons.Length - 1);
        SetActiveWeapon(startWeaponIndex);
    }

    private void OnEnable()
    {
        controls.Enable();
    }

    private void OnDisable()
    {
        controls.Disable();
    }

    private void HandleWeaponScroll(Vector2 scrollValue)
    {
        if (scrollValue.y > 0.01f)
        {
            SelectNextWeapon();
        }
        else if (scrollValue.y < -0.01f)
        {
            SelectPreviousWeapon();
        }
    }

    private void SelectNextWeapon()
    {
        if (weapons == null || weapons.Length == 0)
            return;

        int nextIndex = currentWeaponIndex + 1;

        if (nextIndex >= weapons.Length)
            nextIndex = 0;

        SetActiveWeapon(nextIndex);
    }

    private void SelectPreviousWeapon()
    {
        if (weapons == null || weapons.Length == 0)
            return;

        int previousIndex = currentWeaponIndex - 1;

        if (previousIndex < 0)
            previousIndex = weapons.Length - 1;

        SetActiveWeapon(previousIndex);
    }

    private void SetActiveWeapon(int index)
    {
        if (weapons == null || weapons.Length == 0)
            return;

        if (index < 0 || index >= weapons.Length)
            return;

        for (int i = 0; i < weapons.Length; i++)
        {
            if (weapons[i] != null)
                weapons[i].SetActive(i == index);
        }

        currentWeaponIndex = index;

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
        if (weapons == null)
            return;

        for (int i = 0; i < weapons.Length; i++)
        {
            if (weapons[i] != null)
                weapons[i].SetActive(false);
        }
    }
}