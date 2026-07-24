using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace HollerHorror.UI
{
    /// <summary>
    /// The front door (M9). Wires the menu buttons to scene loads at runtime so
    /// the editor builder doesn't have to serialize UnityEvent listeners. The
    /// case scenes it launches are the greybox test scenes for now; a real
    /// contract-select flow replaces "New Investigation" later.
    /// </summary>
    public sealed class MainMenuController : MonoBehaviour
    {
        [SerializeField] private Button newInvestigationButton;
        [SerializeField] private Button wendigoButton;
        [SerializeField] private Button fetchButton;
        [SerializeField] private Button hollowButton;
        [SerializeField] private Button quitButton;

        [SerializeField, Tooltip("The real map — entity is rolled at run start by CaseGenerator.")]
        private string hollerScene = "BrickersHoller";
        [SerializeField] private string wendigoScene = "CaseHunt_Test";
        [SerializeField] private string fetchScene = "FetchCase_Test";
        [SerializeField] private string hollowScene = "HollowCase_Test";

        private void Awake()
        {
            // New Investigation drops you into Bricker's Holler, where the entity
            // is rolled per run — you go in not knowing (GDD §3). The per-entity
            // buttons remain as direct entry into the old single-entity test scenes.
            if (newInvestigationButton != null)
                newInvestigationButton.onClick.AddListener(() => LoadCase(hollerScene));

            Bind(wendigoButton, wendigoScene);
            Bind(fetchButton, fetchScene);
            Bind(hollowButton, hollowScene);

            if (quitButton != null)
                quitButton.onClick.AddListener(Quit);

            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }


        private void Bind(Button button, string sceneName)
        {
            if (button != null)
                button.onClick.AddListener(() => LoadCase(sceneName));
        }

        private void LoadCase(string sceneName)
        {
            if (Application.CanStreamedLevelBeLoaded(sceneName))
                SceneManager.LoadScene(sceneName);
            else
                Debug.LogError($"[MainMenu] Scene '{sceneName}' isn't in Build Settings — rebuild it from the Holler Horror menu.");
        }

        private void Quit()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }
}
