using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class SuspectButtons : MonoBehaviour
{
    [Header("Correct answer")]
    [SerializeField] private string correctKillerId = "Marek Cerny";

    [Header("Result Text (TextMeshPro)")]
    [SerializeField] private TMP_Text resultTextTMP;

    [Header("Buttons to lock")]
    [SerializeField] private Button[] suspectButtons;

    [Header("Messages")]
    [SerializeField] private string winMessage = "Našli ste vraha!";
    [SerializeField] private string loseMessage = "Usvedčili ste nevinného!";

    [Header("Restart settings")]
    [SerializeField] private float restartDelay = 5f;

    private bool hasAnswered = false;

    private void Start()
    {
        // Text je na začiatku skrytý
        if (resultTextTMP != null)
            resultTextTMP.gameObject.SetActive(false);
    }

    // Volané z Button OnClick()
    public void ChooseSuspect(string suspectId)
    {
        if (hasAnswered)
        {
            Debug.Log("[SuspectButtons] Click ignored (already answered)");
            return;
        }

        hasAnswered = true;

        Debug.Log($"[SuspectButtons] Clicked suspect: {suspectId}");

        LockButtons();

        bool isCorrect = string.Equals(
            suspectId,
            correctKillerId,
            System.StringComparison.OrdinalIgnoreCase
        );

        // Zobraz text až teraz
        if (resultTextTMP != null)
            resultTextTMP.gameObject.SetActive(true);

        if (isCorrect)
        {
            ShowResult(winMessage);
            Debug.Log("[SuspectButtons] CORRECT");
        }
        else
        {
            ShowResult(loseMessage);
            Debug.Log("[SuspectButtons] WRONG - restarting level in 5 seconds");
            Invoke(nameof(RestartLevel), restartDelay);
        }
    }

    private void LockButtons()
    {
        foreach (Button btn in suspectButtons)
        {
            if (btn != null)
                btn.interactable = false;
        }
    }

    private void ShowResult(string message)
    {
        if (resultTextTMP != null)
            resultTextTMP.text = message;
    }

    private void RestartLevel()
    {
        Scene current = SceneManager.GetActiveScene();
        SceneManager.LoadScene(current.buildIndex);
    }
}