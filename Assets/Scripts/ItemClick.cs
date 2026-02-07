using UnityEngine;
using UnityEngine.InputSystem;
 
public class EvidencePickupDisableLock : MonoBehaviour
{
    [Header("Raycast Settings")]
    public LayerMask hittableLayers = ~0; // Everything
    public float maxDistance = 100f;
 
    [Header("Name Mapping")]
    [Tooltip("Suffix, ktorý sa odstráni z názvu itemu. Napr. KeyPref -> Key")]
    public string itemSuffixToRemove = "Pref";
 
    [Tooltip("Suffix, ktorý sa pridá k názvu itemu po odstránení 'Pref'. Napr. Key -> KeyLock")]
    public string lockSuffixToAdd = "Lock";
 
    [Header("Optional override")]
    [Tooltip("Ak chceš ručne priradiť lock objekt, sem ho nastav. Inak sa nájde podľa názvu.")]
    public GameObject lockObject;
 
    private Camera cam;
 
    void Awake()
    {
        cam = Camera.main;
        if (cam == null)
        {
            cam = FindFirstObjectByType<Camera>();
            Debug.Log("[EvidencePickupDisableLock] MainCamera not found, searching any Camera...");
        }
 
        if (cam != null) Debug.Log("[EvidencePickupDisableLock] Using camera: " + cam.name);
        else Debug.LogError("[EvidencePickupDisableLock] ❌ No camera found!");
    }
 
    void Update()
    {
        if (cam == null) return;
 
        if (TryGetPointerDownPosition(out Vector2 screenPos))
        {
            Debug.Log($"[EvidencePickupDisableLock] Pointer DOWN at: {screenPos} (this item: {gameObject.name})");
 
            Ray ray = cam.ScreenPointToRay(screenPos);
            Debug.DrawRay(ray.origin, ray.direction * maxDistance, Color.red, 1f);
 
            if (Physics.Raycast(ray, out RaycastHit hit, maxDistance, hittableLayers, QueryTriggerInteraction.Ignore))
            {
                Debug.Log($"[EvidencePickupDisableLock] Raycast HIT: {hit.transform.name}");
 
                if (hit.transform == transform || hit.transform.IsChildOf(transform))
                {
                    Debug.Log($"[EvidencePickupDisableLock] ✅ Picked item: {gameObject.name}");
                    Pick();
                }
                else
                {
                    Debug.Log("[EvidencePickupDisableLock] ℹ️ Hit different object, ignoring.");
                }
            }
            else
            {
                Debug.Log("[EvidencePickupDisableLock] ❌ Raycast hit nothing.");
            }
        }
    }
 
    private void Pick()
    {
        // 1) Resolve lock object
        GameObject lockToDisable = lockObject;
 
        if (lockToDisable == null)
        {
            string baseName = GetBaseItemName(gameObject.name); // KeyPref -> Key
            string expectedLockName = baseName + lockSuffixToAdd; // Key -> KeyLock
 
            Debug.Log($"[EvidencePickupDisableLock] Auto-find lock. Item='{gameObject.name}', Base='{baseName}', ExpectedLock='{expectedLockName}'");
 
            // Prefer: sibling under same parent
            if (transform.parent != null)
            {
                Transform t = transform.parent.Find(expectedLockName);
                if (t != null)
                {
                    lockToDisable = t.gameObject;
                    Debug.Log($"[EvidencePickupDisableLock] Found lock as sibling: {lockToDisable.name}");
                }
            }
 
            // Fallback: search whole scene
            if (lockToDisable == null)
            {
                GameObject found = GameObject.Find(expectedLockName);
                if (found != null)
                {
                    lockToDisable = found;
                    Debug.Log($"[EvidencePickupDisableLock] Found lock in scene: {lockToDisable.name}");
                }
            }
 
            if (lockToDisable == null)
            {
                Debug.LogWarning($"[EvidencePickupDisableLock] ⚠️ Lock not found for '{gameObject.name}'. Tried: '{expectedLockName}'");
            }
        }
        else
        {
            Debug.Log($"[EvidencePickupDisableLock] Using manually assigned lock: {lockToDisable.name}");
        }
 
        if (lockToDisable != null)
        {
            Debug.Log($"[EvidencePickupDisableLock] 🔒 Disabling lock: {lockToDisable.name}");
            lockToDisable.SetActive(false);
        }
 
        // 2) Disable picked item itself
        Debug.Log($"[EvidencePickupDisableLock] 🧾 Disabling item: {gameObject.name}");
        gameObject.SetActive(false);
    }
 
    private string GetBaseItemName(string itemName)
    {
        if (!string.IsNullOrEmpty(itemSuffixToRemove) && itemName.EndsWith(itemSuffixToRemove))
        {
            return itemName.Substring(0, itemName.Length - itemSuffixToRemove.Length);
        }
        return itemName;
    }
 
    private bool TryGetPointerDownPosition(out Vector2 screenPos)
    {
        // Touch
        if (Touchscreen.current != null)
        {
            var primaryTouch = Touchscreen.current.primaryTouch;
            if (primaryTouch.press.wasPressedThisFrame)
            {
                screenPos = primaryTouch.position.ReadValue();
                Debug.Log("[EvidencePickupDisableLock] Touch detected");
                return true;
            }
        }
 
        // Mouse
        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            screenPos = Mouse.current.position.ReadValue();
            Debug.Log("[EvidencePickupDisableLock] Mouse click detected");
            return true;
        }
 
        // Pointer fallback
        if (Pointer.current != null && Pointer.current.press.wasPressedThisFrame)
        {
            screenPos = Pointer.current.position.ReadValue();
            Debug.Log("[EvidencePickupDisableLock] Pointer device click detected");
            return true;
        }
 
        screenPos = default;
        return false;
    }
}