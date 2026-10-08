#nullable enable
using System;
using UnityEngine.Pool;
using UnityEngine.UIElements;
using System.Collections.Generic;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.UIElements
{
    public static partial class VisualElementExtensions
    {
        #region AddClasses
        /// <summary>
        /// Adds a USS class to the element.
        /// </summary>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="className">The USS class name to add; <see langword="null"/> leaves the element unchanged.</param>
        /// <returns>The element, for chaining.</returns>
        public static T AddClass<T>(this T element, string? className)
            where T : VisualElement
        {
            element.AddToClassList(className);
            return element;
        }

        /// <summary>
        /// Conditionally adds a USS class to the element.
        /// </summary>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="condition">When <see langword="true"/>, the operation is performed; otherwise, the element is returned unchanged.</param>
        /// <param name="className">The USS class name to add; <see langword="null"/> leaves the element unchanged.</param>
        /// <returns>The element, for chaining.</returns>
        public static T AddClassIf<T>(this T element, bool condition, string? className)
            where T : VisualElement => condition ? element.AddClass(className) : element;

        /// <summary>
        /// Adds an array of USS classes to the element.
        /// </summary>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="classNames">The USS class names to add; <see langword="null"/> entries are skipped.</param>
        /// <returns>The element, for chaining.</returns>
        public static T AddClasses<T>(this T element, params string?[]? classNames)
            where T : VisualElement
        {
            if (classNames is null) return element;

            foreach (var className in classNames)
                element.AddToClassList(className);

            return element;
        }

        /// <summary>
        /// Conditionally adds an array of USS classes to the element.
        /// </summary>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="condition">When <see langword="true"/>, the operation is performed; otherwise, the element is returned unchanged.</param>
        /// <param name="classNames">The USS class names to add; <see langword="null"/> entries are skipped.</param>
        /// <returns>The element, for chaining.</returns>
        public static T AddClassesIf<T>(this T element, bool condition, params string?[]? classNames)
            where T : VisualElement => condition ? element.AddClasses(classNames) : element;

        /// <summary>
        /// Adds a list of USS classes to the element.
        /// </summary>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="classNames">The USS class names to add; <see langword="null"/> entries are skipped.</param>
        /// <returns>The element, for chaining.</returns>
        public static T AddClasses<T>(this T element, List<string>? classNames)
            where T : VisualElement
        {
            if (classNames is null) return element;

            foreach (var className in classNames)
                element.AddToClassList(className);

            return element;
        }

        /// <summary>
        /// Conditionally adds a list of USS classes to the element.
        /// </summary>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="condition">When <see langword="true"/>, the operation is performed; otherwise, the element is returned unchanged.</param>
        /// <param name="classNames">The USS class names to add; <see langword="null"/> entries are skipped.</param>
        /// <returns>The element, for chaining.</returns>
        public static T AddClassesIf<T>(this T element, bool condition, List<string>? classNames)
            where T : VisualElement => condition ? element.AddClasses(classNames) : element;

        /// <summary>
        /// Adds an enumerable of USS classes to the element.
        /// </summary>
        /// <remarks>
        /// <paramref name="classNames"/> is copied before the first change, so it may read
        /// <see cref="VisualElement.GetClasses"/> of this element, directly or through a LINQ query.
        /// </remarks>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="classNames">The USS class names to add; <see langword="null"/> entries are skipped.</param>
        /// <returns>The element, for chaining.</returns>
        public static T AddClasses<T>(this T element, IEnumerable<string?>? classNames)
            where T : VisualElement
        {
            switch (classNames)
            {
                case null: return element;
                case string?[] array: return element.AddClasses(array);
            }

            using var pooled = ListPool<string?>.Get(out var snapshot);
            snapshot.AddRange(classNames);

            foreach (var className in snapshot)
                element.AddToClassList(className);

            return element;
        }

        /// <summary>
        /// Conditionally adds an enumerable of USS classes to the element.
        /// </summary>
        /// <remarks>
        /// <paramref name="classNames"/> is copied before the first change, so it may read
        /// <see cref="VisualElement.GetClasses"/> of this element, directly or through a LINQ query.
        /// </remarks>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="condition">When <see langword="true"/>, the operation is performed; otherwise, the element is returned unchanged.</param>
        /// <param name="classNames">The USS class names to add; <see langword="null"/> entries are skipped.</param>
        /// <returns>The element, for chaining.</returns>
        public static T AddClassesIf<T>(this T element, bool condition, IEnumerable<string?>? classNames)
            where T : VisualElement => condition ? element.AddClasses(classNames) : element;

        /// <summary>
        /// Adds a read-only span of USS classes to the element.
        /// </summary>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="classNames">The USS class names to add; <see langword="null"/> entries are skipped.</param>
        /// <returns>The element, for chaining.</returns>
        public static T AddClasses<T>(this T element, ReadOnlySpan<string> classNames)
            where T : VisualElement
        {
            foreach (var className in classNames)
                element.AddToClassList(className);

            return element;
        }

        /// <summary>
        /// Conditionally adds a read-only span of USS classes to the element.
        /// </summary>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="condition">When <see langword="true"/>, the operation is performed; otherwise, the element is returned unchanged.</param>
        /// <param name="classNames">The USS class names to add; <see langword="null"/> entries are skipped.</param>
        /// <returns>The element, for chaining.</returns>
        public static T AddClassesIf<T>(this T element, bool condition, ReadOnlySpan<string> classNames)
            where T : VisualElement => condition ? element.AddClasses(classNames) : element;
        #endregion

        #region RemoveClasses
        /// <summary>
        /// Removes a USS class from the element.
        /// </summary>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="className">The USS class name to remove; <see langword="null"/> leaves the element unchanged.</param>
        /// <returns>The element, for chaining.</returns>
        public static T RemoveClass<T>(this T element, string? className)
            where T : VisualElement
        {
            element.RemoveFromClassList(className);
            return element;
        }

        /// <summary>
        /// Conditionally removes a USS class from the element.
        /// </summary>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="condition">When <see langword="true"/>, the operation is performed; otherwise, the element is returned unchanged.</param>
        /// <param name="className">The USS class name to remove; <see langword="null"/> leaves the element unchanged.</param>
        /// <returns>The element, for chaining.</returns>
        public static T RemoveClassIf<T>(this T element, bool condition, string? className)
            where T : VisualElement => condition ? element.RemoveClass(className) : element;

        /// <summary>
        /// Removes an array of USS classes from the element.
        /// </summary>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="classNames">The USS class names to remove; <see langword="null"/> entries are skipped.</param>
        /// <returns>The element, for chaining.</returns>
        public static T RemoveClasses<T>(this T element, params string?[]? classNames)
            where T : VisualElement
        {
            if (classNames is null) return element;

            foreach (var className in classNames)
                element.RemoveFromClassList(className);

            return element;
        }

        /// <summary>
        /// Conditionally removes an array of USS classes from the element.
        /// </summary>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="condition">When <see langword="true"/>, the operation is performed; otherwise, the element is returned unchanged.</param>
        /// <param name="classNames">The USS class names to remove; <see langword="null"/> entries are skipped.</param>
        /// <returns>The element, for chaining.</returns>
        public static T RemoveClassesIf<T>(this T element, bool condition, params string?[]? classNames)
            where T : VisualElement => condition ? element.RemoveClasses(classNames) : element;

        /// <summary>
        /// Removes a list of USS classes from the element.
        /// </summary>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="classNames">The USS class names to remove; <see langword="null"/> entries are skipped.</param>
        /// <returns>The element, for chaining.</returns>
        public static T RemoveClasses<T>(this T element, List<string>? classNames)
            where T : VisualElement
        {
            if (classNames is null) return element;

            foreach (var className in classNames)
                element.RemoveFromClassList(className);

            return element;
        }

        /// <summary>
        /// Conditionally removes a list of USS classes from the element.
        /// </summary>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="condition">When <see langword="true"/>, the operation is performed; otherwise, the element is returned unchanged.</param>
        /// <param name="classNames">The USS class names to remove; <see langword="null"/> entries are skipped.</param>
        /// <returns>The element, for chaining.</returns>
        public static T RemoveClassesIf<T>(this T element, bool condition, List<string>? classNames)
            where T : VisualElement => condition ? element.RemoveClasses(classNames) : element;

        /// <summary>
        /// Removes an enumerable of USS classes from the element.
        /// </summary>
        /// <remarks>
        /// <paramref name="classNames"/> is copied before the first change, so it may read
        /// <see cref="VisualElement.GetClasses"/> of this element, directly or through a LINQ query.
        /// </remarks>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="classNames">The USS class names to remove; <see langword="null"/> entries are skipped.</param>
        /// <returns>The element, for chaining.</returns>
        public static T RemoveClasses<T>(this T element, IEnumerable<string?>? classNames)
            where T : VisualElement
        {
            switch (classNames)
            {
                case null: return element;
                case string?[] array: return element.RemoveClasses(array);
            }

            using var pooled = ListPool<string?>.Get(out var snapshot);
            snapshot.AddRange(classNames);

            foreach (var className in snapshot)
                element.RemoveFromClassList(className);

            return element;
        }

        /// <summary>
        /// Conditionally removes an enumerable of USS classes from the element.
        /// </summary>
        /// <remarks>
        /// <paramref name="classNames"/> is copied before the first change, so it may read
        /// <see cref="VisualElement.GetClasses"/> of this element, directly or through a LINQ query.
        /// </remarks>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="condition">When <see langword="true"/>, the operation is performed; otherwise, the element is returned unchanged.</param>
        /// <param name="classNames">The USS class names to remove; <see langword="null"/> entries are skipped.</param>
        /// <returns>The element, for chaining.</returns>
        public static T RemoveClassesIf<T>(this T element, bool condition, IEnumerable<string?>? classNames)
            where T : VisualElement => condition ? element.RemoveClasses(classNames) : element;

        /// <summary>
        /// Removes a read-only span of USS classes from the element.
        /// </summary>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="classNames">The USS class names to remove; <see langword="null"/> entries are skipped.</param>
        /// <returns>The element, for chaining.</returns>
        public static T RemoveClasses<T>(this T element, ReadOnlySpan<string> classNames)
            where T : VisualElement
        {
            foreach (var className in classNames)
                element.RemoveFromClassList(className);

            return element;
        }

        /// <summary>
        /// Conditionally removes a read-only span of USS classes from the element.
        /// </summary>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="condition">When <see langword="true"/>, the operation is performed; otherwise, the element is returned unchanged.</param>
        /// <param name="classNames">The USS class names to remove; <see langword="null"/> entries are skipped.</param>
        /// <returns>The element, for chaining.</returns>
        public static T RemoveClassesIf<T>(this T element, bool condition, ReadOnlySpan<string> classNames)
            where T : VisualElement => condition ? element.RemoveClasses(classNames) : element;
        #endregion

        #region ToggleClasses
        /// <summary>
        /// Adds the USS class when it is absent and removes it when it is present.
        /// </summary>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="className">The USS class name to toggle; <see langword="null"/> leaves the element unchanged.</param>
        /// <returns>The element, for chaining.</returns>
        public static T ToggleClass<T>(this T element, string? className)
            where T : VisualElement
        {
            element.ToggleInClassList(className);
            return element;
        }

        /// <summary>
        /// Conditionally adds the USS class when it is absent and removes it when it is present.
        /// </summary>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="condition">When <see langword="true"/>, the operation is performed; otherwise, the element is returned unchanged.</param>
        /// <param name="className">The USS class name to toggle; <see langword="null"/> leaves the element unchanged.</param>
        /// <returns>The element, for chaining.</returns>
        public static T ToggleClassIf<T>(this T element, bool condition, string? className)
            where T : VisualElement => condition ? element.ToggleClass(className) : element;

        /// <summary>
        /// Adds each USS class of an array when it is absent and removes it when it is present.
        /// </summary>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="classNames">The USS class names to toggle; <see langword="null"/> entries are skipped.</param>
        /// <returns>The element, for chaining.</returns>
        public static T ToggleClasses<T>(this T element, params string?[]? classNames)
            where T : VisualElement
        {
            if (classNames is null) return element;

            foreach (var className in classNames)
                element.ToggleInClassList(className);

            return element;
        }

        /// <summary>
        /// Conditionally adds each USS class of an array when it is absent and removes it when it is present.
        /// </summary>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="condition">When <see langword="true"/>, the operation is performed; otherwise, the element is returned unchanged.</param>
        /// <param name="classNames">The USS class names to toggle; <see langword="null"/> entries are skipped.</param>
        /// <returns>The element, for chaining.</returns>
        public static T ToggleClassesIf<T>(this T element, bool condition, params string?[]? classNames)
            where T : VisualElement => condition ? element.ToggleClasses(classNames) : element;

        /// <summary>
        /// Adds each USS class of a list when it is absent and removes it when it is present.
        /// </summary>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="classNames">The USS class names to toggle; <see langword="null"/> entries are skipped.</param>
        /// <returns>The element, for chaining.</returns>
        public static T ToggleClasses<T>(this T element, List<string>? classNames)
            where T : VisualElement
        {
            if (classNames is null) return element;

            foreach (var className in classNames)
                element.ToggleInClassList(className);

            return element;
        }

        /// <summary>
        /// Conditionally adds each USS class of a list when it is absent and removes it when it is present.
        /// </summary>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="condition">When <see langword="true"/>, the operation is performed; otherwise, the element is returned unchanged.</param>
        /// <param name="classNames">The USS class names to toggle; <see langword="null"/> entries are skipped.</param>
        /// <returns>The element, for chaining.</returns>
        public static T ToggleClassesIf<T>(this T element, bool condition, List<string>? classNames)
            where T : VisualElement => condition ? element.ToggleClasses(classNames) : element;

        /// <summary>
        /// Adds each USS class of an enumerable when it is absent and removes it when it is present.
        /// </summary>
        /// <remarks>
        /// <paramref name="classNames"/> is copied before the first change, so it may read
        /// <see cref="VisualElement.GetClasses"/> of this element, directly or through a LINQ query.
        /// </remarks>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="classNames">The USS class names to toggle; <see langword="null"/> entries are skipped.</param>
        /// <returns>The element, for chaining.</returns>
        public static T ToggleClasses<T>(this T element, IEnumerable<string?>? classNames)
            where T : VisualElement
        {
            switch (classNames)
            {
                case null: return element;
                case string?[] array: return element.ToggleClasses(array);
            }

            using var pooled = ListPool<string?>.Get(out var snapshot);
            snapshot.AddRange(classNames);

            foreach (var className in snapshot)
                element.ToggleInClassList(className);

            return element;
        }

        /// <summary>
        /// Conditionally adds each USS class of an enumerable when it is absent and removes it when it is present.
        /// </summary>
        /// <remarks>
        /// <paramref name="classNames"/> is copied before the first change, so it may read
        /// <see cref="VisualElement.GetClasses"/> of this element, directly or through a LINQ query.
        /// </remarks>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="condition">When <see langword="true"/>, the operation is performed; otherwise, the element is returned unchanged.</param>
        /// <param name="classNames">The USS class names to toggle; <see langword="null"/> entries are skipped.</param>
        /// <returns>The element, for chaining.</returns>
        public static T ToggleClassesIf<T>(this T element, bool condition, IEnumerable<string?>? classNames)
            where T : VisualElement => condition ? element.ToggleClasses(classNames) : element;

        /// <summary>
        /// Adds each USS class of a read-only span when it is absent and removes it when it is present.
        /// </summary>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="classNames">The USS class names to toggle; <see langword="null"/> entries are skipped.</param>
        /// <returns>The element, for chaining.</returns>
        public static T ToggleClasses<T>(this T element, ReadOnlySpan<string> classNames)
            where T : VisualElement
        {
            foreach (var className in classNames)
                element.ToggleInClassList(className);

            return element;
        }

        /// <summary>
        /// Conditionally adds each USS class of a read-only span when it is absent and removes it when it is present.
        /// </summary>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="condition">When <see langword="true"/>, the operation is performed; otherwise, the element is returned unchanged.</param>
        /// <param name="classNames">The USS class names to toggle; <see langword="null"/> entries are skipped.</param>
        /// <returns>The element, for chaining.</returns>
        public static T ToggleClassesIf<T>(this T element, bool condition, ReadOnlySpan<string> classNames)
            where T : VisualElement => condition ? element.ToggleClasses(classNames) : element;
        #endregion

        #region EnableClasses
        /// <summary>
        /// Adds or removes the USS class depending on <paramref name="enable"/>.
        /// </summary>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="className">The USS class name to enable or disable; <see langword="null"/> leaves the element unchanged.</param>
        /// <param name="enable">When <see langword="true"/>, the class is added; otherwise, it is removed.</param>
        /// <returns>The element, for chaining.</returns>
        public static T EnableClass<T>(this T element, string? className, bool enable)
            where T : VisualElement
        {
            element.EnableInClassList(className, enable);
            return element;
        }

        /// <summary>
        /// Conditionally adds or removes the USS class depending on <paramref name="enable"/>.
        /// </summary>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="condition">When <see langword="true"/>, the operation is performed; otherwise, the element is returned unchanged.</param>
        /// <param name="className">The USS class name to enable or disable; <see langword="null"/> leaves the element unchanged.</param>
        /// <param name="enable">When <see langword="true"/>, the class is added; otherwise, it is removed.</param>
        /// <returns>The element, for chaining.</returns>
        public static T EnableClassIf<T>(this T element, bool condition, string? className, bool enable)
            where T : VisualElement => condition ? element.EnableClass(className, enable) : element;

        /// <summary>
        /// Adds or removes an array of USS classes depending on <paramref name="enable"/>.
        /// </summary>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="enable">When <see langword="true"/>, the classes are added; otherwise, they are removed.</param>
        /// <param name="classNames">The USS class names to enable or disable; <see langword="null"/> entries are skipped.</param>
        /// <returns>The element, for chaining.</returns>
        public static T EnableClasses<T>(this T element, bool enable, params string?[]? classNames)
            where T : VisualElement
        {
            if (classNames is null) return element;

            foreach (var className in classNames)
                element.EnableInClassList(className, enable);

            return element;
        }

        /// <summary>
        /// Conditionally adds or removes an array of USS classes depending on <paramref name="enable"/>.
        /// </summary>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="condition">When <see langword="true"/>, the operation is performed; otherwise, the element is returned unchanged.</param>
        /// <param name="enable">When <see langword="true"/>, the classes are added; otherwise, they are removed.</param>
        /// <param name="classNames">The USS class names to enable or disable; <see langword="null"/> entries are skipped.</param>
        /// <returns>The element, for chaining.</returns>
        public static T EnableClassesIf<T>(this T element, bool condition, bool enable, params string?[]? classNames)
            where T : VisualElement => condition ? element.EnableClasses(enable, classNames) : element;

        /// <summary>
        /// Adds or removes a list of USS classes depending on <paramref name="enable"/>.
        /// </summary>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="enable">When <see langword="true"/>, the classes are added; otherwise, they are removed.</param>
        /// <param name="classNames">The USS class names to enable or disable; <see langword="null"/> entries are skipped.</param>
        /// <returns>The element, for chaining.</returns>
        public static T EnableClasses<T>(this T element, bool enable, List<string>? classNames)
            where T : VisualElement
        {
            if (classNames is null) return element;

            foreach (var className in classNames)
                element.EnableInClassList(className, enable);

            return element;
        }

        /// <summary>
        /// Conditionally adds or removes a list of USS classes depending on <paramref name="enable"/>.
        /// </summary>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="condition">When <see langword="true"/>, the operation is performed; otherwise, the element is returned unchanged.</param>
        /// <param name="enable">When <see langword="true"/>, the classes are added; otherwise, they are removed.</param>
        /// <param name="classNames">The USS class names to enable or disable; <see langword="null"/> entries are skipped.</param>
        /// <returns>The element, for chaining.</returns>
        public static T EnableClassesIf<T>(this T element, bool condition, bool enable, List<string>? classNames)
            where T : VisualElement => condition ? element.EnableClasses(enable, classNames) : element;

        /// <summary>
        /// Adds or removes an enumerable of USS classes depending on <paramref name="enable"/>.
        /// </summary>
        /// <remarks>
        /// <paramref name="classNames"/> is copied before the first change, so it may read
        /// <see cref="VisualElement.GetClasses"/> of this element, directly or through a LINQ query.
        /// </remarks>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="enable">When <see langword="true"/>, the classes are added; otherwise, they are removed.</param>
        /// <param name="classNames">The USS class names to enable or disable; <see langword="null"/> entries are skipped.</param>
        /// <returns>The element, for chaining.</returns>
        public static T EnableClasses<T>(this T element, bool enable, IEnumerable<string?>? classNames)
            where T : VisualElement
        {
            switch (classNames)
            {
                case null: return element;
                case string?[] array: return element.EnableClasses(enable, array);
            }

            using var pooled = ListPool<string?>.Get(out var snapshot);
            snapshot.AddRange(classNames);

            foreach (var className in snapshot)
                element.EnableInClassList(className, enable);

            return element;
        }

        /// <summary>
        /// Conditionally adds or removes an enumerable of USS classes depending on <paramref name="enable"/>.
        /// </summary>
        /// <remarks>
        /// <paramref name="classNames"/> is copied before the first change, so it may read
        /// <see cref="VisualElement.GetClasses"/> of this element, directly or through a LINQ query.
        /// </remarks>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="condition">When <see langword="true"/>, the operation is performed; otherwise, the element is returned unchanged.</param>
        /// <param name="enable">When <see langword="true"/>, the classes are added; otherwise, they are removed.</param>
        /// <param name="classNames">The USS class names to enable or disable; <see langword="null"/> entries are skipped.</param>
        /// <returns>The element, for chaining.</returns>
        public static T EnableClassesIf<T>(this T element, bool condition, bool enable, IEnumerable<string?>? classNames)
            where T : VisualElement => condition ? element.EnableClasses(enable, classNames) : element;

        /// <summary>
        /// Adds or removes a read-only span of USS classes depending on <paramref name="enable"/>.
        /// </summary>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="enable">When <see langword="true"/>, the classes are added; otherwise, they are removed.</param>
        /// <param name="classNames">The USS class names to enable or disable; <see langword="null"/> entries are skipped.</param>
        /// <returns>The element, for chaining.</returns>
        public static T EnableClasses<T>(this T element, bool enable, ReadOnlySpan<string> classNames)
            where T : VisualElement
        {
            foreach (var className in classNames)
                element.EnableInClassList(className, enable);

            return element;
        }

        /// <summary>
        /// Conditionally adds or removes a read-only span of USS classes depending on <paramref name="enable"/>.
        /// </summary>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="condition">When <see langword="true"/>, the operation is performed; otherwise, the element is returned unchanged.</param>
        /// <param name="enable">When <see langword="true"/>, the classes are added; otherwise, they are removed.</param>
        /// <param name="classNames">The USS class names to enable or disable; <see langword="null"/> entries are skipped.</param>
        /// <returns>The element, for chaining.</returns>
        public static T EnableClassesIf<T>(this T element, bool condition, bool enable, ReadOnlySpan<string> classNames)
            where T : VisualElement => condition ? element.EnableClasses(enable, classNames) : element;
        #endregion

        #region ClearClasses
        /// <summary>
        /// Removes all USS classes from the element.
        /// </summary>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <returns>The element, for chaining.</returns>
        public static T ClearClasses<T>(this T element)
            where T : VisualElement
        {
            element.ClearClassList();
            return element;
        }

        /// <summary>
        /// Conditionally removes all USS classes from the element.
        /// </summary>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="condition">When <see langword="true"/>, the operation is performed; otherwise, the element is returned unchanged.</param>
        /// <returns>The element, for chaining.</returns>
        public static T ClearClassesIf<T>(this T element, bool condition)
            where T : VisualElement => condition ? element.ClearClasses() : element;
        #endregion
    }
}
