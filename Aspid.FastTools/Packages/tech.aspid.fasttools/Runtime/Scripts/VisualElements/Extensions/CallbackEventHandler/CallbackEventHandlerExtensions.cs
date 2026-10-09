using UnityEngine.UIElements;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.UIElements
{
    /// <summary>
    /// Provides extension methods for <see cref="CallbackEventHandler"/>.
    /// </summary>
    public static class CallbackEventHandlerExtensions
    {
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
            element.UnregisterCallback(callback, useTrickleDown);
            return element;
        }
    }
}
