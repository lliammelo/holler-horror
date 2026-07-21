using HollerHorror.Player;
using UnityEngine;
using Yarn.Unity;

namespace HollerHorror.Dialogue
{
    /// <summary>
    /// Greybox dialogue presenter: OnGUI panel with speaker, line, and option
    /// buttons. Takes over the player's controls for the conversation's duration.
    /// Replaced by a real UI in the art pass — the async flow stays the same.
    /// </summary>
    public sealed class DialogueHud : DialoguePresenterBase
    {
        private string speaker = "";
        private string lineText = "";
        private DialogueOption[] options;
        private bool continueClicked;
        private int chosenOption = -1;
        private bool inDialogue;

        private FirstPersonController capturedController;

        public override YarnTask OnDialogueStartedAsync()
        {
            inDialogue = true;
            if (PlayerRegistry.All.Count > 0)
            {
                capturedController = PlayerRegistry.All[0];
                capturedController.enabled = false;
            }
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            return YarnTask.CompletedTask;
        }

        public override YarnTask OnDialogueCompleteAsync()
        {
            inDialogue = false;
            speaker = lineText = "";
            options = null;
            if (capturedController != null)
            {
                capturedController.enabled = true;
                capturedController = null;
            }
            return YarnTask.CompletedTask;
        }

        public override async YarnTask RunLineAsync(LocalizedLine line, LineCancellationToken token)
        {
            speaker = line.CharacterName ?? "";
            lineText = line.TextWithoutCharacterName.Text;
            continueClicked = false;

            while (!continueClicked && !token.NextContentToken.IsCancellationRequested)
                await YarnTask.Delay(16);

            lineText = "";
        }

        public override async YarnTask<DialogueOption> RunOptionsAsync(DialogueOption[] dialogueOptions, LineCancellationToken cancellationToken)
        {
            options = dialogueOptions;
            chosenOption = -1;

            while (chosenOption < 0 && !cancellationToken.NextContentToken.IsCancellationRequested)
                await YarnTask.Delay(16);

            var selected = chosenOption >= 0 ? dialogueOptions[chosenOption] : null;
            options = null;
            return selected;
        }

        private void OnGUI()
        {
            if (!inDialogue)
                return;

            float width = Mathf.Min(720f, Screen.width - 40f);
            float x = (Screen.width - width) / 2f;

            GUILayout.BeginArea(new Rect(x, Screen.height - 230, width, 220), GUI.skin.box);

            if (!string.IsNullOrEmpty(lineText))
            {
                if (!string.IsNullOrEmpty(speaker))
                    GUILayout.Label($"— {speaker} —");
                GUILayout.Label(lineText);
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("Continue"))
                    continueClicked = true;
            }
            else if (options != null)
            {
                for (int i = 0; i < options.Length; i++)
                {
                    if (!options[i].IsAvailable)
                        continue;
                    if (GUILayout.Button(options[i].Line.TextWithoutCharacterName.Text))
                        chosenOption = i;
                }
            }

            GUILayout.EndArea();
        }
    }
}
