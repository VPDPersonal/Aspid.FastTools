using System;
using UnityEngine.UIElements;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.UIElements
{
    /// <summary>
    /// Provides extension methods for <see cref="CallbackEventHandler"/>.
    /// </summary>
    public static class CallbackEventHandlerExtensions
    {
#if !UNITY_6000_0_OR_NEWER
        private static readonly ConditionalWeakTable<CallbackEventHandler,
            Dictionary<(Delegate Callback, TrickleDown Phase), Delegate>> OnceCallbackWrappers = new();
#endif

        /// <summary>
        /// Registers a callback for an event via <see cref="CallbackEventHandler.RegisterCallback{TEventType}(EventCallback{TEventType}, TrickleDown)"/>.
        /// </summary>
        /// <remarks>
        /// Name both type arguments unless the callback has a typed parameter: <c>RegisterCallbackSelf&lt;Button, ClickEvent&gt;(OnClick)</c>.
        /// </remarks>
        /// <typeparam name="T">The element type.</typeparam>
        /// <typeparam name="TEventType">The event type to listen for.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="callback">The callback to invoke when the event reaches the element.</param>
        /// <param name="useTrickleDown">Selects the phase of the callback: <see cref="TrickleDown.TrickleDown"/> for trickle-down, <see cref="TrickleDown.NoTrickleDown"/> for bubble-up.</param>
        /// <returns>The element, for chaining.</returns>
        public static T RegisterCallbackSelf<T, TEventType>(
            this T element,
            EventCallback<TEventType> callback,
            TrickleDown useTrickleDown = TrickleDown.NoTrickleDown)
            where T : CallbackEventHandler
            where TEventType : EventBase<TEventType>, new()
        {
            element.RegisterCallback(callback, useTrickleDown);
            return element;
        }

        /// <summary>
        /// Registers a callback with user arguments for an event via <see cref="CallbackEventHandler.RegisterCallback{TEventType, TUserArgsType}(EventCallback{TEventType, TUserArgsType}, TUserArgsType, TrickleDown)"/>.
        /// </summary>
        /// <remarks>
        /// Name all type arguments unless the callback has typed parameters: <c>RegisterCallbackSelf&lt;Button, ClickEvent, int&gt;(OnClick, 5)</c>.
        /// </remarks>
        /// <typeparam name="T">The element type.</typeparam>
        /// <typeparam name="TEventType">The event type to listen for.</typeparam>
        /// <typeparam name="TUserArgsType">The type of the user arguments.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="callback">The callback to invoke when the event reaches the element.</param>
        /// <param name="userArgs">The value passed to the callback with every event.</param>
        /// <param name="useTrickleDown">Selects the phase of the callback: <see cref="TrickleDown.TrickleDown"/> for trickle-down, <see cref="TrickleDown.NoTrickleDown"/> for bubble-up.</param>
        /// <returns>The element, for chaining.</returns>
        public static T RegisterCallbackSelf<T, TEventType, TUserArgsType>(
            this T element,
            EventCallback<TEventType, TUserArgsType> callback,
            TUserArgsType userArgs,
            TrickleDown useTrickleDown = TrickleDown.NoTrickleDown)
            where T : CallbackEventHandler
            where TEventType : EventBase<TEventType>, new()
        {
            element.RegisterCallback(callback, userArgs, useTrickleDown);
            return element;
        }

        /// <summary>
        /// Registers a callback that runs for one event and is then removed.
        /// </summary>
        /// <remarks>
        /// Name both type arguments unless the callback has a typed parameter: <c>RegisterCallbackOnceSelf&lt;Button, ClickEvent&gt;(OnClick)</c>.
        /// </remarks>
        /// <typeparam name="T">The element type.</typeparam>
        /// <typeparam name="TEventType">The event type to listen for.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="callback">The callback to invoke when the first event reaches the element.</param>
        /// <param name="useTrickleDown">Selects the phase of the callback: <see cref="TrickleDown.TrickleDown"/> for trickle-down, <see cref="TrickleDown.NoTrickleDown"/> for bubble-up.</param>
        /// <returns>The element, for chaining.</returns>
        public static T RegisterCallbackOnceSelf<T, TEventType>(
            this T element,
            EventCallback<TEventType> callback,
            TrickleDown useTrickleDown = TrickleDown.NoTrickleDown)
            where T : CallbackEventHandler
            where TEventType : EventBase<TEventType>, new()
        {
#if UNITY_6000_0_OR_NEWER
            element.RegisterCallbackOnce(callback, useTrickleDown);
#else
            if (callback is null) return element;
            RegisterOnce<TEventType>(element, callback: callback, invoke: callback.Invoke, useTrickleDown: useTrickleDown);
#endif
            return element;
        }

        /// <summary>
        /// Registers a callback with user arguments that runs for one event and is then removed.
        /// </summary>
        /// <remarks>
        /// Name all type arguments unless the callback has typed parameters: <c>RegisterCallbackOnceSelf&lt;Button, ClickEvent, int&gt;(OnClick, 5)</c>.
        /// </remarks>
        /// <typeparam name="T">The element type.</typeparam>
        /// <typeparam name="TEventType">The event type to listen for.</typeparam>
        /// <typeparam name="TUserArgsType">The type of the user arguments.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="callback">The callback to invoke when the first event reaches the element.</param>
        /// <param name="userArgs">The value passed to the callback.</param>
        /// <param name="useTrickleDown">Selects the phase of the callback: <see cref="TrickleDown.TrickleDown"/> for trickle-down, <see cref="TrickleDown.NoTrickleDown"/> for bubble-up.</param>
        /// <returns>The element, for chaining.</returns>
        public static T RegisterCallbackOnceSelf<T, TEventType, TUserArgsType>(
            this T element,
            EventCallback<TEventType, TUserArgsType> callback,
            TUserArgsType userArgs,
            TrickleDown useTrickleDown = TrickleDown.NoTrickleDown)
            where T : CallbackEventHandler
            where TEventType : EventBase<TEventType>, new()
        {
#if UNITY_6000_0_OR_NEWER
            element.RegisterCallbackOnce(callback, userArgs, useTrickleDown);
#else
            if (callback is null) return element;
            RegisterOnce<TEventType>(element, callback: callback, invoke: evt => callback(evt, userArgs), useTrickleDown: useTrickleDown);
#endif
            return element;
        }

        /// <summary>
        /// Removes a callback for an event via <see cref="CallbackEventHandler.UnregisterCallback{TEventType}(EventCallback{TEventType}, TrickleDown)"/>.
        /// </summary>
        /// <remarks>
        /// Name both type arguments unless the callback has a typed parameter: <c>UnregisterCallbackSelf&lt;Button, ClickEvent&gt;(OnClick)</c>.
        /// </remarks>
        /// <typeparam name="T">The element type.</typeparam>
        /// <typeparam name="TEventType">The event type the callback listens for.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="callback">The callback to remove.</param>
        /// <param name="useTrickleDown">The phase the callback was registered for: <see cref="TrickleDown.TrickleDown"/> for trickle-down, <see cref="TrickleDown.NoTrickleDown"/> for bubble-up.</param>
        /// <returns>The element, for chaining.</returns>
        public static T UnregisterCallbackSelf<T, TEventType>(
            this T element,
            EventCallback<TEventType> callback,
            TrickleDown useTrickleDown = TrickleDown.NoTrickleDown)
            where T : CallbackEventHandler
            where TEventType : EventBase<TEventType>, new()
        {
#if !UNITY_6000_0_OR_NEWER
            TryUnregisterOnce<TEventType>(element, callback: callback, useTrickleDown: useTrickleDown);
#endif
            element.UnregisterCallback(callback, useTrickleDown);
            return element;
        }

        /// <summary>
        /// Removes a callback with user arguments for an event via <see cref="CallbackEventHandler.UnregisterCallback{TEventType, TUserArgsType}(EventCallback{TEventType, TUserArgsType}, TrickleDown)"/>.
        /// </summary>
        /// <remarks>
        /// Name all type arguments unless the callback has typed parameters: <c>UnregisterCallbackSelf&lt;Button, ClickEvent, int&gt;(OnClick)</c>.
        /// </remarks>
        /// <typeparam name="T">The element type.</typeparam>
        /// <typeparam name="TEventType">The event type the callback listens for.</typeparam>
        /// <typeparam name="TUserArgsType">The type of the user arguments.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="callback">The callback to remove.</param>
        /// <param name="useTrickleDown">The phase the callback was registered for: <see cref="TrickleDown.TrickleDown"/> for trickle-down, <see cref="TrickleDown.NoTrickleDown"/> for bubble-up.</param>
        /// <returns>The element, for chaining.</returns>
        public static T UnregisterCallbackSelf<T, TEventType, TUserArgsType>(
            this T element,
            EventCallback<TEventType, TUserArgsType> callback,
            TrickleDown useTrickleDown = TrickleDown.NoTrickleDown)
            where T : CallbackEventHandler
            where TEventType : EventBase<TEventType>, new()
        {
#if !UNITY_6000_0_OR_NEWER
            TryUnregisterOnce<TEventType>(element, callback: callback, useTrickleDown: useTrickleDown);
#endif
            element.UnregisterCallback(callback, useTrickleDown);
            return element;
        }

#if !UNITY_6000_0_OR_NEWER
        private static void RegisterOnce<TEventType>(
            CallbackEventHandler element, Delegate callback, Action<TEventType> invoke, TrickleDown useTrickleDown)
            where TEventType : EventBase<TEventType>, new()
        {
            var key = (callback, useTrickleDown);
            var wrappers = OnceCallbackWrappers.GetOrCreateValue(element);

            if (!wrappers.TryGetValue(key, out var wrapper))
            {
                wrapper = (EventCallback<TEventType, Action<TEventType>>)InvokeOnce;
                wrappers.Add(key, value: wrapper);
            }

            element.RegisterCallback(
                (EventCallback<TEventType, Action<TEventType>>)wrapper, userArgs: invoke, useTrickleDown: useTrickleDown);

            void InvokeOnce(TEventType evt, Action<TEventType> currentCallback)
            {
                if (TryUnregisterOnce<TEventType>(element, callback: callback, useTrickleDown: useTrickleDown, expectedWrapper: wrapper))
                    currentCallback(evt);
            }
        }

        private static bool TryUnregisterOnce<TEventType>(
            CallbackEventHandler element, Delegate callback, TrickleDown useTrickleDown, Delegate expectedWrapper = null)
            where TEventType : EventBase<TEventType>, new()
        {
            if (callback is null || !OnceCallbackWrappers.TryGetValue(element, out var wrappers)) return false;

            var key = (callback, useTrickleDown);

            if (!wrappers.TryGetValue(key, out var wrapper)) return false;

            // Unity 2022 may still invoke a replaced wrapper from its current dispatch list.
            if (expectedWrapper is not null && !ReferenceEquals(wrapper, expectedWrapper)) return false;

            wrappers.Remove(key);
            element.UnregisterCallback((EventCallback<TEventType, Action<TEventType>>)wrapper, useTrickleDown: useTrickleDown);
            return true;
        }
#endif
    }
}
