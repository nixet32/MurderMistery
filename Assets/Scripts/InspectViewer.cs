using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class InspectViewer : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private GameObject inspectPanel;
    [SerializeField] private Button rotateLeftButton;
    [SerializeField] private Button rotateRightButton;
    [SerializeField] private Button closeButton;

    [Tooltip("Text (TMP) v InspectPaneli, kde sa zobrazí popis itemu")]
    [SerializeField] private TextMeshProUGUI descriptionText;

    [Header("Hide during inspect")]
    [SerializeField] private GameObject bookPanelToHide;
    [SerializeField] private GameObject overlayToHide;

    [Header("Spawn")]
    [SerializeField] private Transform spawnPoint;

    [Header("Rotation")]
    [SerializeField] private float rotationStepDegrees = 15f;

    [Header("Transform")]
    [SerializeField] private bool resetLocalTransformOnSpawn = true;
    [SerializeField] private Vector3 forcedLocalScale = new Vector3(0.6f, 0.6f, 0.6f);

    [Header("Special scale overrides")]
    [SerializeField] private Vector3 glovesForcedScale = new Vector3(100f, 100f, 100f);

    [Header("Rendering")]
    [SerializeField] private bool forceUnlitShaderForInspect = true;

    [Header("Debug")]
    [SerializeField] private bool debugLogs = false;

    private GameObject currentInstance;

    // 🔒 Y-only rotácia
    private Quaternion baseRotation;
    private float currentYawDegrees = 0f;
    private string currentItemKey;

    // 📝 Popisy itemov
    private readonly Dictionary<string, string> descriptions = new Dictionary<string, string>
    {
        { "Knife",   "Kuchynský nôž nájdený pri tele. Zodpovedá rane obete." },
        { "Key",     "Kľúč patriaci Marekovi. Nemal sa nachádzať v kancelárii." },
        { "Mobile",  "Posledná správa bola od kontaktu „Marek“ krátko pred smrťou." },
        { "Notepad", "Obchodná zmluva s agresívnymi poznámkami na okrajoch." },
        { "Paper",   "Papierik s hrozbou: „Ak to niekomu povieš, zničím ťa.“" },
        { "Ring",    "Dámsky prsteň nájdený pri stole. Zodpovedá manželke obete." },
        { "Gloves",  "Jednorazové rukavice, zjavne použité nedávno." }
    };

    // 📄 Itemy otočené na čítanie
    private readonly HashSet<string> autoRotateX90Items = new HashSet<string>
    {
        "Notepad",
        "Paper"
    };

    private void Awake()
    {
        if (inspectPanel != null)
            inspectPanel.SetActive(false);

        rotateLeftButton?.onClick.AddListener(() => RotateBy(-rotationStepDegrees));
        rotateRightButton?.onClick.AddListener(() => RotateBy(rotationStepDegrees));
        closeButton?.onClick.AddListener(Close);
    }

    public void Show(GameObject prefabOrTemplate)
    {
        if (prefabOrTemplate == null || spawnPoint == null)
            return;

        // Skry UI
        if (bookPanelToHide != null) bookPanelToHide.SetActive(false);
        if (overlayToHide != null) overlayToHide.SetActive(false);
        if (inspectPanel != null) inspectPanel.SetActive(true);

        // Znič starý item
        if (currentInstance != null)
            Destroy(currentInstance);

        // Spawn nového
        currentInstance = Instantiate(prefabOrTemplate);
        currentInstance.name = prefabOrTemplate.name + "_InspectInstance";
        currentInstance.transform.SetParent(spawnPoint, false);
        currentInstance.SetActive(true);

        if (resetLocalTransformOnSpawn)
        {
            currentInstance.transform.localPosition = Vector3.zero;
            currentInstance.transform.localRotation = Quaternion.identity;
        }

        // Normalizovaný názov
        currentItemKey = NormalizeItemName(prefabOrTemplate.name);

        // 📏 SCALE
        if (currentItemKey == "Gloves")
            currentInstance.transform.localScale = glovesForcedScale;
        else
            currentInstance.transform.localScale = forcedLocalScale;

        // 📄 Auto-rotate pre papier / notepad
        if (autoRotateX90Items.Contains(currentItemKey))
            currentInstance.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);

        // 🔒 Ulož základnú rotáciu
        baseRotation = currentInstance.transform.localRotation;
        currentYawDegrees = 0f;

        // 📝 Popis
        if (descriptionText != null)
        {
            if (descriptions.TryGetValue(currentItemKey, out var desc))
                descriptionText.text = desc;
            else
                descriptionText.text = "(Bez popisu)";
        }

        // 🎨 Shader fix (URP safe)
        if (forceUnlitShaderForInspect)
        {
            Shader unlit = Shader.Find("Universal Render Pipeline/Unlit");
            if (unlit != null)
            {
                foreach (var r in currentInstance.GetComponentsInChildren<Renderer>(true))
                {
                    foreach (var m in r.materials)
                        if (m != null) m.shader = unlit;
                }
            }
        }
    }

    public void Close()
    {
        if (currentInstance != null)
            Destroy(currentInstance);

        currentInstance = null;

        if (inspectPanel != null) inspectPanel.SetActive(false);
        if (bookPanelToHide != null) bookPanelToHide.SetActive(true);
        if (overlayToHide != null) overlayToHide.SetActive(true);
    }

    private void RotateBy(float degrees)
    {
        if (currentInstance == null) return;

        // 🔄 IBA okolo Y
        currentYawDegrees += degrees;
        currentInstance.transform.localRotation =
            baseRotation * Quaternion.Euler(currentYawDegrees, currentYawDegrees, 0f);
    }

    private string NormalizeItemName(string rawName)
    {
        if (string.IsNullOrEmpty(rawName)) return rawName;

        return rawName
            .Replace("_InspectInstance", "")
            .Replace("Inspect", "")
            .Replace("Pref", "")
            .Trim();
    }
}
