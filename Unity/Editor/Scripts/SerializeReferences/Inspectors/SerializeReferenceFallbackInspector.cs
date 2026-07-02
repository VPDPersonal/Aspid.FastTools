using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.SerializeReferences.Editors
{
    /// <summary>
    /// The fallback inspector behind the "Dropdown without [TypeSelector]" opt-in
    /// (<see cref="SerializeReferenceSettings.DropdownWithoutAttributeEnabled"/>): when the setting is on and the
    /// inspected component declares a top-level <c>[SerializeReference]</c> field (or list) that carries no
    /// <c>[TypeSelector]</c>, it rebuilds the default inspector in UIToolkit with those fields drawn as the package's
    /// dropdown field — everything else stays a plain <see cref="PropertyField"/>, so attributed fields keep their
    /// drawer (base-type narrowing included). In every other case <see cref="CreateInspectorGUI"/> returns
    /// <see langword="null"/>, handing the component back to Unity's default inspector untouched.
    /// </summary>
    /// <remarks>
    /// Registered with <c>isFallback = true</c>, so ANY custom editor — a user's own or another package's — always
    /// wins over it; the opt-in only ever affects components that would otherwise show the default inspector. A user
    /// custom editor that wants the same rendering calls <see cref="SerializeReferenceEditorGUI"/> instead.
    /// Limitations of the substituted fields: decorator attributes (<c>[Header]</c>, <c>[Space]</c>) on an
    /// attribute-free reference field are not drawn, and managed references nested inside plain <c>[Serializable]</c>
    /// containers keep Unity's default rendering (their drawing belongs to Unity's generic handler).
    /// </remarks>
    internal abstract class SerializeReferenceFallbackInspector : Editor
    {
        public override VisualElement CreateInspectorGUI()
        {
            // Build lazily but decide strictly: without at least one attribute-free reference field this editor must
            // change NOTHING — null hands the component to Unity's default inspector path.
            VisualElement root = null;

            var iterator = serializedObject.GetIterator();
            if (!iterator.NextVisible(enterChildren: true)) return null;

            var substitutedAny = false;
            do
            {
                var property = iterator.Copy();
                root ??= new VisualElement();

                if (property.propertyPath == "m_Script")
                {
                    var script = new PropertyField(property);
                    script.SetEnabled(false);
                    root.Add(script);
                }
                else if (SerializeReferenceAutoDropdown.ShouldDraw(property))
                {
                    substitutedAny = true;
                    root.Add(SerializeReferenceAutoDropdown.CreateField(property));
                }
                else
                {
                    root.Add(new PropertyField(property));
                }
            } while (iterator.NextVisible(enterChildren: false));

            return substitutedAny ? root : null;
        }

        // Flipping the opt-in must rebuild open inspectors, not just repaint them: this editor is ALWAYS the selected
        // fallback editor for the affected components, and what the toggle changes is the UI CreateInspectorGUI
        // built — which the tracker only re-queries on a rebuild. Watches the general Changed signal for a flip of
        // this one value, so unrelated settings never pay the rebuild. (The shared tracker covers the main inspector;
        // a locked inspector rebuilds on its next reselect.)
        [InitializeOnLoadMethod]
        private static void RebuildInspectorsOnToggle()
        {
            var last = SerializeReferenceAutoDropdown.Enabled;
            SerializeReferenceSettings.Changed += () =>
            {
                var current = SerializeReferenceAutoDropdown.Enabled;
                if (current == last) return;

                last = current;
                ActiveEditorTracker.sharedTracker.ForceRebuild();
            };
        }
    }

    /// <summary>The attribute-free dropdown fallback for components; see <see cref="SerializeReferenceFallbackInspector"/>.</summary>
    [CanEditMultipleObjects]
    [CustomEditor(typeof(MonoBehaviour), editorForChildClasses: true, isFallback = true)]
    internal sealed class SerializeReferenceMonoBehaviourFallbackInspector : SerializeReferenceFallbackInspector { }

    /// <summary>The attribute-free dropdown fallback for assets; see <see cref="SerializeReferenceFallbackInspector"/>.</summary>
    [CanEditMultipleObjects]
    [CustomEditor(typeof(ScriptableObject), editorForChildClasses: true, isFallback = true)]
    internal sealed class SerializeReferenceScriptableObjectFallbackInspector : SerializeReferenceFallbackInspector { }
}
