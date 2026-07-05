using UnityEngine;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.Samples.VisualElements
{
    // Step-by-step tour of the fluent VisualElement extension API. This component holds no UI — it is a bag of
    // knobs; the whole inspector is built in code by VisualElementsTutorialEditor, one STEP section per lesson.
    // Open Scenes/VisualElementsTutorial.unity, select the GameObject and follow TUTORIAL.md / TUTORIAL_RU.md.
    public sealed class VisualElementsTutorial : MonoBehaviour
    {
        [Tooltip("STEP 3 — feeds the composed header row the editor builds with AddChild + a flex layout.")]
        [SerializeField]
        private string _abilityName = "Fireball";

        [Tooltip("STEP 4 — reactive knob. PropertyField + AddValueChanged re-runs the badge on every edit; 0 = FREE.")]
        [SerializeField] [Min(0)]
        private int _manaCost = 30;

        [Tooltip("STEP 5 — bound to a ProgressBar via SetValue; edit it to see the bar and its title update live.")]
        [SerializeField] [Range(0f, 1f)]
        private float _charge = 0.6f;

        public string AbilityName => _abilityName;

        public int ManaCost => _manaCost;

        public float Charge => _charge;
    }
}
