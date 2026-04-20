//using UnityEngine;

//public class PlayerHUDManager : MonoBehaviour
//{
//    [Header("Weapon HUD Slots")]
//    [SerializeField] private GameObject[] weaponSlots;

//    [Header("Debug")]
//    [SerializeField] private bool debugLogHudSwitch = false;

//    private void Start()
//    {
//        DisableAllWeaponSlots();
//    }

//    public void SetActiveWeaponSlot(int index)
//    {
//        if (weaponSlots == null || weaponSlots.Length == 0)
//            return;

//        if (index < 0 || index >= weaponSlots.Length)
//            return;

//        for (int i = 0; i < weaponSlots.Length; i++)
//        {
//            if (weaponSlots[i] != null)
//                weaponSlots[i].SetActive(i == index);
//        }

//        if (debugLogHudSwitch && weaponSlots[index] != null)
//        {
//            Debug.Log($"{name}: Active HUD slot = {weaponSlots[index].name}");
//        }
//    }

//    private void DisableAllWeaponSlots()
//    {
//        if (weaponSlots == null)
//            return;

//        for (int i = 0; i < weaponSlots.Length; i++)
//        {
//            if (weaponSlots[i] != null)
//                weaponSlots[i].SetActive(false);
//        }
//    }
//}
using UnityEngine;

public class PlayerHUDManager : MonoBehaviour
{
    [Header("Weapon HUD Slots")]
    [SerializeField] private GameObject[] weaponSlots;

    [Header("Debug")]
    [SerializeField] private bool debugLogHudSwitch = false;

    private void Start()
    {
        if (weaponSlots == null || weaponSlots.Length == 0)
            return;

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
        if (weaponSlots == null || weaponSlots.Length == 0)
            return;

        if (index < 0 || index >= weaponSlots.Length)
            return;

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