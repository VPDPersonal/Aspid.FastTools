using System;
using UnityEditor;
using UnityEngine.UIElements;
using Aspid.FastTools.UIElements;
using Aspid.FastTools.UIElements.Editors.Internal;
using Aspid.FastTools.SerializeReferences.Editors;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.Types.Editors
{
    /// <summary>
    /// Builds the type selector's settings controls bound to <see cref="TypeSelectorSettings"/> — the "Type Selector"
    /// section of the SerializeReference window's Settings tab (<see cref="SettingsView"/>). Every row is a per-user
    /// preference (<see cref="SettingsView.UserScopeClass"/>): the Favorites section toggle, the recents capacity
    /// (whose 0 doubles as the Recent section's off switch, so no separate toggle exists), and a maintenance row that
    /// clears the stored per-project Favorites / Recent lists.
    /// </summary>
    internal static class TypeSelectorSettingsUI
    {
        /// <summary>
        /// Appends the Favorites toggle, the recents-capacity slider and the clear-saved-lists maintenance row to
        /// <paramref name="container"/>, each wired straight to <see cref="TypeSelectorSettings"/> /
        /// <see cref="TypeSelectorPreferences"/>.
        /// </summary>
        public static void BuildControls(VisualElement container)
        {
            var showFavorites = new AspidSwitch("Favorites section")
            {
                value = TypeSelectorSettings.ShowFavorites,
                tooltip = "Show the ★ Favorites section on the picker's root page.\n"
                    + "Hiding it keeps your saved favorites (and the per-row ★ toggle) — turning it back on restores the same list.\n"
                    + "Per-user setting — stored locally, never committed.",
            };
            showFavorites.AddClass(SettingsView.UserScopeClass);
            showFavorites.RegisterValueChangedCallback(evt => TypeSelectorSettings.ShowFavorites = evt.newValue);
            SyncFromSettings(showFavorites, () => TypeSelectorSettings.ShowFavorites);
            container.Add(showFavorites);

            var capacity = new SliderInt("Recent items", 0, TypeSelectorSettings.MaxRecentsCapacity)
            {
                value = TypeSelectorSettings.RecentsCapacity,
                showInputField = true,
                tooltip = "How many picks the picker's Recent section keeps (most recent first).\n"
                    + "0 hides the section and pauses recording without wiping the already-collected history.\n"
                    + "Per-user setting — stored locally, never committed.",
            };
            capacity.AddClass(SettingsView.UserScopeClass);
            capacity.RegisterValueChangedCallback(evt => TypeSelectorSettings.RecentsCapacity = evt.newValue);
            SyncFromSettings(capacity, () => TypeSelectorSettings.RecentsCapacity);
            container.Add(capacity);

            container.Add(BuildClearRow());
        }

        // The maintenance row: the same card as the field rows, a caption on the left and the two destructive
        // clear actions pinned right, each behind a count-naming confirmation since a wiped list is not undoable.
        private static VisualElement BuildClearRow()
        {
            var row = new VisualElement().AddClass(SettingsView.RowClass).AddClass(SettingsView.UserScopeClass);
            row.tooltip = "The Favorites / Recent lists are saved per user and per project.\n"
                + "Clearing removes every stored entry, including ones kept for types that don't currently resolve.";

            var caption = new Label("Saved lists").AddClass(SettingsView.RowCaptionClass);

            var clearFavorites = new Button(ClearFavorites) { text = "Clear favorites" }
                .AddClass(SettingsView.ActionClass)
                .AddClass(SettingsView.ActionDangerClass);

            var clearRecents = new Button(ClearRecents) { text = "Clear recents" }
                .AddClass(SettingsView.ActionClass)
                .AddClass(SettingsView.ActionDangerClass);

            return row
                .AddChild(caption)
                .AddChild(clearFavorites)
                .AddChild(clearRecents);
        }

        private static void ClearFavorites()
        {
            if (!ConfirmClear("favorites", TypeSelectorPreferences.FavoritesCount)) return;
            TypeSelectorPreferences.ClearFavorites();
        }

        private static void ClearRecents()
        {
            if (!ConfirmClear("recents", TypeSelectorPreferences.RecentsCount)) return;
            TypeSelectorPreferences.ClearRecents();
        }

        // Names the exact entry count in the confirmation (raw stored count — cleared entries include the ones kept
        // for currently-unresolvable types), and short-circuits with a plain notice when there is nothing to clear.
        private static bool ConfirmClear(string list, int count)
        {
            var title = $"Clear {list}";

            if (count == 0)
            {
                EditorUtility.DisplayDialog(title, $"There are no saved {list} to clear.", "OK");
                return false;
            }

            return EditorUtility.DisplayDialog(
                title,
                $"Remove all saved {list} ({count} {(count == 1 ? "entry" : "entries")}) for this project? This cannot be undone.",
                "Clear",
                "Cancel");
        }

        /// <summary>
        /// Keeps <paramref name="control"/> in lock-step with <see cref="TypeSelectorSettings"/>, mirroring the
        /// References section's live-sync contract: on every <see cref="TypeSelectorSettings.Changed"/> the control
        /// re-reads its backing value <i>without notifying</i> (never writing back or looping), the control being
        /// actively edited is skipped so an in-progress edit isn't clobbered, and the subscription is released on
        /// <see cref="DetachFromPanelEvent"/> so a closed surface leaks nothing.
        /// </summary>
        private static void SyncFromSettings<TControl, TValue>(TControl control, Func<TValue> read)
            where TControl : VisualElement, INotifyValueChanged<TValue>
        {
            void Handler()
            {
                if (control.focusController?.focusedElement == control) return;
                control.SetValueWithoutNotify(read());
            }

            TypeSelectorSettings.Changed += Handler;
            control.RegisterCallback<DetachFromPanelEvent>(_ => TypeSelectorSettings.Changed -= Handler);
        }
    }
}
