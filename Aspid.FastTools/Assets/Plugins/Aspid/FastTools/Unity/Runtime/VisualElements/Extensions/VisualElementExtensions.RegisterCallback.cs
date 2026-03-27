#if !UNITY_2023_1_OR_NEWER
using UnityEngine.UIElements;
#endif

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools
{
    public static partial class VisualElementExtensions
    {
#if !UNITY_2023_1_OR_NEWER
        /// <summary>
        /// Registers a callback that is automatically unregistered after its first invocation.
        /// Backport of <c>RegisterCallbackOnce</c> for Unity versions prior to 2023.1.
        /// </summary>
        /// <param name="element">The element to register the callback on.</param>
        /// <param name="callback">The callback to invoke once.</param>
        /// <param name="useTrickleDown">Whether to use trickle-down phase.</param>
        public static void RegisterCallbackOnce<TEventType>(this VisualElement element,
            EventCallback<TEventType> callback,
            TrickleDown useTrickleDown = TrickleDown.NoTrickleDown)
            where TEventType : EventBase<TEventType>, new()
        {
            element.RegisterCallback<TEventType>(Once, useTrickleDown);
            return;

            void Once(TEventType evt)
            {
                callback?.Invoke(evt);
                element.UnregisterCallback<TEventType>(Once, useTrickleDown);
            }
        }

        /// <summary>
        /// Registers a callback with user arguments that is automatically unregistered after its first invocation.
        /// Backport of <c>RegisterCallbackOnce</c> for Unity versions prior to 2023.1.
        /// </summary>
        /// <param name="element">The element to register the callback on.</param>
        /// <param name="userArgs">User-defined arguments passed to the callback.</param>
        /// <param name="callback">The callback to invoke once.</param>
        /// <param name="useTrickleDown">Whether to use trickle-down phase.</param>
        public static void RegisterCallbackOnce<TEventType, TUserArgsType>(this VisualElement element,
            TUserArgsType userArgs,
            EventCallback<TEventType, TUserArgsType> callback,
            TrickleDown useTrickleDown = TrickleDown.NoTrickleDown)
            where TEventType : EventBase<TEventType>, new()
        {
            element.RegisterCallback<TEventType, TUserArgsType>(Once, userArgs, useTrickleDown);
            return;

            void Once(TEventType evt, TUserArgsType args)
            {
                callback?.Invoke(evt, args);
                element.UnregisterCallback<TEventType, TUserArgsType>(Once, useTrickleDown);
            }
        }
#endif
    }
}
