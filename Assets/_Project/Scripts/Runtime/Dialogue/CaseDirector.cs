using UnityEngine;
using Yarn.Unity;

namespace HollerHorror.Dialogue
{
    /// <summary>
    /// Per-run knowledge randomization (GDD §6): which resident holds which
    /// fragment is rolled at run start and written into Yarn variables, so
    /// "talk to the widow first" never becomes rote. M3 slice randomizes one
    /// load-bearing fact: who can name Elias Cole.
    /// </summary>
    [RequireComponent(typeof(DialogueRunner))]
    public sealed class CaseDirector : MonoBehaviour
    {
        private void Start()
        {
            var storage = GetComponent<DialogueRunner>().VariableStorage;

            string nameHolder = Random.value < 0.5f ? "ada" : "tetch";
            storage.SetValue("$name_holder", nameHolder);
            Debug.Log($"[CaseDirector] This run, the name-holder is: {nameHolder}");
        }
    }
}
