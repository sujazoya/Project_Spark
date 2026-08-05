// ============================================================================
// CustomerDialogue.cs
// ============================================================================

using TMPro;
using UnityEngine;

namespace ProjectSpark.Gameplay.Flashlight
{
    public sealed class CustomerDialogue : MonoBehaviour
    {
        [SerializeField]
        TMP_Text customerName;

        [SerializeField]
        TMP_Text dialogue;

        void Start()
        {
            customerName.text = "Customer";

            dialogue.text =
                "My flashlight stopped working even after changing the batteries.";
        }
    }
}