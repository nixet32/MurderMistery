using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class InspectButton : MonoBehaviour
{
    [SerializeField] private InspectViewer viewer;
    [SerializeField] private GameObject modelPrefabOrTemplate;

    private void Awake()
    {
        var btn = GetComponent<Button>();
        btn.onClick.AddListener(OnClicked);
    }

    private void OnClicked()
    {
        Debug.Log("[InspectButton] Clicked: " + gameObject.name);
        if (viewer == null)
        {
            Debug.LogError("[InspectButton] Viewer not assigned.", this);
            return;
        }

        if (modelPrefabOrTemplate == null)
        {
            Debug.LogError("[InspectButton] Model prefab/template not assigned.", this);
            return;
        }

        viewer.Show(modelPrefabOrTemplate);
    }
}
