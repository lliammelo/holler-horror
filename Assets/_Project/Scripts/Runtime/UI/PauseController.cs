using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

namespace HollerHorror.UI
{
    /// <summary>
    /// Minimal in-case pause overlay so the loop closes back to the menu.
    /// Bound to F10 rather than Escape, which the clue board and rituals already
    /// use for "step back". Auto-added to case scenes by the scene builders.
    /// </summary>
    public sealed class PauseController : MonoBehaviour
    {
        [SerializeField] private string mainMenuScene = "MainMenu";

        private bool paused;

        private void Update()
        {
            if (Keyboard.current != null && Keyboard.current.f10Key.wasPressedThisFrame)
                SetPaused(!paused);
        }

        private void SetPaused(bool value)
        {
            paused = value;
            Time.timeScale = paused ? 0f : 1f;
            Cursor.lockState = paused ? CursorLockMode.None : CursorLockMode.Locked;
            Cursor.visible = paused;
        }

        private void OnGUI()
        {
            if (!paused)
                return;

            const float w = 260f, h = 150f;
            GUILayout.BeginArea(new Rect((Screen.width - w) / 2f, (Screen.height - h) / 2f, w, h), GUI.skin.box);
            GUILayout.Label("PAUSED");
            GUILayout.Space(8);
            if (GUILayout.Button("Resume"))
                SetPaused(false);
            if (GUILayout.Button("Return to Main Menu"))
            {
                Time.timeScale = 1f;
                SceneManager.LoadScene(mainMenuScene);
            }
            if (GUILayout.Button("Quit"))
            {
#if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
#else
                Application.Quit();
#endif
            }
            GUILayout.Label("(F10 to pause)");
            GUILayout.EndArea();
        }
    }
}
