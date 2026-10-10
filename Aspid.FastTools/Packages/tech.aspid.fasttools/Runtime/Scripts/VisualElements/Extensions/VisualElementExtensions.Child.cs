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
        #region AddChildren
        /// <summary>
        /// Adds an element to the <see cref="VisualElement.contentContainer"/> of this element.
        /// </summary>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="child">The child element to add; <see langword="null"/> leaves the element unchanged.</param>
        /// <returns>The element, for chaining.</returns>
        /// <exception cref="InvalidOperationException">
        /// A non-null child is added to an element without a <see cref="VisualElement.contentContainer"/>, such as a <see cref="ListView"/> or a <see cref="TreeView"/>.
        /// </exception>
        public static T AddChild<T>(this T element, VisualElement? child)
            where T : VisualElement
        {
            element.Add(child);
            return element;
        }

        /// <summary>
        /// Conditionally adds an element to the <see cref="VisualElement.contentContainer"/> of this element.
        /// </summary>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="condition">When <see langword="true"/>, the operation is performed; otherwise, the element is returned unchanged.</param>
        /// <param name="child">The child element to add; <see langword="null"/> leaves the element unchanged.</param>
        /// <returns>The element, for chaining.</returns>
        /// <exception cref="InvalidOperationException">
        /// A non-null child is added to an element without a <see cref="VisualElement.contentContainer"/>, such as a <see cref="ListView"/> or a <see cref="TreeView"/>.
        /// </exception>
        public static T AddChildIf<T>(this T element, bool condition, VisualElement? child)
            where T : VisualElement => condition ? element.AddChild(child) : element;

        /// <summary>
        /// Adds an array of child elements to the <see cref="VisualElement.contentContainer"/> of this element.
        /// </summary>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="children">The children to add; <see langword="null"/> entries are skipped.</param>
        /// <returns>The element, for chaining.</returns>
        /// <exception cref="InvalidOperationException">
        /// A non-null child is added to an element without a <see cref="VisualElement.contentContainer"/>, such as a <see cref="ListView"/> or a <see cref="TreeView"/>.
        /// </exception>
        public static T AddChildren<T>(this T element, params VisualElement?[]? children)
            where T : VisualElement
        {
            if (children is null) return element;

            foreach (var child in children)
                element.Add(child);

            return element;
        }

        /// <summary>
        /// Conditionally adds an array of child elements to the <see cref="VisualElement.contentContainer"/> of this element.
        /// </summary>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="condition">When <see langword="true"/>, the operation is performed; otherwise, the element is returned unchanged.</param>
        /// <param name="children">The children to add; <see langword="null"/> entries are skipped.</param>
        /// <returns>The element, for chaining.</returns>
        /// <exception cref="InvalidOperationException">
        /// A non-null child is added to an element without a <see cref="VisualElement.contentContainer"/>, such as a <see cref="ListView"/> or a <see cref="TreeView"/>.
        /// </exception>
        public static T AddChildrenIf<T>(this T element, bool condition, params VisualElement?[]? children)
            where T : VisualElement => condition ? element.AddChildren(children) : element;

        /// <summary>
        /// Adds a list of child elements to the <see cref="VisualElement.contentContainer"/> of this element.
        /// </summary>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="children">The children to add; <see langword="null"/> entries are skipped.</param>
        /// <returns>The element, for chaining.</returns>
        /// <exception cref="InvalidOperationException">
        /// A non-null child is added to an element without a <see cref="VisualElement.contentContainer"/>, such as a <see cref="ListView"/> or a <see cref="TreeView"/>.
        /// </exception>
        public static T AddChildren<T>(this T element, List<VisualElement>? children)
            where T : VisualElement
        {
            if (children is null) return element;

            foreach (var child in children)
                element.Add(child);

            return element;
        }

        /// <summary>
        /// Conditionally adds a list of child elements to the <see cref="VisualElement.contentContainer"/> of this element.
        /// </summary>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="condition">When <see langword="true"/>, the operation is performed; otherwise, the element is returned unchanged.</param>
        /// <param name="children">The children to add; <see langword="null"/> entries are skipped.</param>
        /// <returns>The element, for chaining.</returns>
        /// <exception cref="InvalidOperationException">
        /// A non-null child is added to an element without a <see cref="VisualElement.contentContainer"/>, such as a <see cref="ListView"/> or a <see cref="TreeView"/>.
        /// </exception>
        public static T AddChildrenIf<T>(this T element, bool condition, List<VisualElement>? children)
            where T : VisualElement => condition ? element.AddChildren(children) : element;

        /// <summary>
        /// Adds an enumerable of child elements to the <see cref="VisualElement.contentContainer"/> of this element.
        /// </summary>
        /// <remarks>
        /// <paramref name="children"/> is copied before the first change, so it may be the live
        /// <see cref="VisualElement.Children"/> of this or another element.
        /// </remarks>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="children">The children to add; <see langword="null"/> entries are skipped.</param>
        /// <returns>The element, for chaining.</returns>
        /// <exception cref="InvalidOperationException">
        /// A non-null child is added to an element without a <see cref="VisualElement.contentContainer"/>, such as a <see cref="ListView"/> or a <see cref="TreeView"/>.
        /// </exception>
        public static T AddChildren<T>(this T element, IEnumerable<VisualElement?>? children)
            where T : VisualElement
        {
            switch (children)
            {
                case null: return element;
                case VisualElement?[] array: return element.AddChildren(array);
            }

            using var pooled = ListPool<VisualElement?>.Get(out var snapshot);
            snapshot.AddRange(children);

            foreach (var child in snapshot)
                element.Add(child);

            return element;
        }

        /// <summary>
        /// Conditionally adds an enumerable of child elements to the <see cref="VisualElement.contentContainer"/> of this element.
        /// </summary>
        /// <remarks>
        /// <paramref name="children"/> is copied before the first change, so it may be the live
        /// <see cref="VisualElement.Children"/> of this or another element.
        /// </remarks>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="condition">When <see langword="true"/>, the operation is performed; otherwise, the element is returned unchanged.</param>
        /// <param name="children">The children to add; <see langword="null"/> entries are skipped.</param>
        /// <returns>The element, for chaining.</returns>
        /// <exception cref="InvalidOperationException">
        /// A non-null child is added to an element without a <see cref="VisualElement.contentContainer"/>, such as a <see cref="ListView"/> or a <see cref="TreeView"/>.
        /// </exception>
        public static T AddChildrenIf<T>(this T element, bool condition, IEnumerable<VisualElement?>? children)
            where T : VisualElement => condition ? element.AddChildren(children) : element;

        /// <summary>
        /// Adds a read-only span of child elements to the <see cref="VisualElement.contentContainer"/> of this element.
        /// </summary>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="children">The children to add; <see langword="null"/> entries are skipped.</param>
        /// <returns>The element, for chaining.</returns>
        /// <exception cref="InvalidOperationException">
        /// A non-null child is added to an element without a <see cref="VisualElement.contentContainer"/>, such as a <see cref="ListView"/> or a <see cref="TreeView"/>.
        /// </exception>
        public static T AddChildren<T>(this T element, ReadOnlySpan<VisualElement> children)
            where T : VisualElement
        {
            foreach (var child in children)
                element.Add(child);

            return element;
        }

        /// <summary>
        /// Conditionally adds a read-only span of child elements to the <see cref="VisualElement.contentContainer"/> of this element.
        /// </summary>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="condition">When <see langword="true"/>, the operation is performed; otherwise, the element is returned unchanged.</param>
        /// <param name="children">The children to add; <see langword="null"/> entries are skipped.</param>
        /// <returns>The element, for chaining.</returns>
        /// <exception cref="InvalidOperationException">
        /// A non-null child is added to an element without a <see cref="VisualElement.contentContainer"/>, such as a <see cref="ListView"/> or a <see cref="TreeView"/>.
        /// </exception>
        public static T AddChildrenIf<T>(this T element, bool condition, ReadOnlySpan<VisualElement> children)
            where T : VisualElement => condition ? element.AddChildren(children) : element;
        #endregion

        #region InsertChildren
        /// <summary>
        /// Inserts a child element at the specified index in the <see cref="VisualElement.contentContainer"/> of this element.
        /// </summary>
        /// <remarks>
        /// Does nothing on an element without a <see cref="VisualElement.contentContainer"/>, such as a <see cref="ListView"/> or a <see cref="TreeView"/>.
        /// </remarks>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="index">The index at which to insert the child.</param>
        /// <param name="child">The child element to insert; <see langword="null"/> leaves the element unchanged.</param>
        /// <returns>The element, for chaining.</returns>
        public static T InsertChild<T>(this T element, int index, VisualElement? child)
            where T : VisualElement
        {
            element.Insert(index, child);
            return element;
        }

        /// <summary>
        /// Conditionally inserts a child element at the specified index in the <see cref="VisualElement.contentContainer"/> of this element.
        /// </summary>
        /// <remarks>
        /// Does nothing on an element without a <see cref="VisualElement.contentContainer"/>, such as a <see cref="ListView"/> or a <see cref="TreeView"/>.
        /// </remarks>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="condition">When <see langword="true"/>, the operation is performed; otherwise, the element is returned unchanged.</param>
        /// <param name="index">The index at which to insert the child.</param>
        /// <param name="child">The child element to insert; <see langword="null"/> leaves the element unchanged.</param>
        /// <returns>The element, for chaining.</returns>
        public static T InsertChildIf<T>(this T element, bool condition, int index, VisualElement? child)
            where T : VisualElement => condition ? element.InsertChild(index, child) : element;

        /// <summary>
        /// Inserts an array of child elements starting at the specified index in the <see cref="VisualElement.contentContainer"/> of this element.
        /// </summary>
        /// <remarks>
        /// Does nothing on an element without a <see cref="VisualElement.contentContainer"/>, such as a <see cref="ListView"/> or a <see cref="TreeView"/>.
        /// </remarks>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="index">The index at which to start inserting children.</param>
        /// <param name="children">The children to insert; <see langword="null"/> entries are skipped.</param>
        /// <returns>The element, for chaining.</returns>
        public static T InsertChildren<T>(this T element, int index, params VisualElement?[]? children)
            where T : VisualElement
        {
            if (children is null) return element;

            foreach (var child in children)
            {
                if (child is null) continue;
                element.Insert(index++, child);
            }

            return element;
        }

        /// <summary>
        /// Conditionally inserts an array of child elements starting at the specified index in the <see cref="VisualElement.contentContainer"/> of this element.
        /// </summary>
        /// <remarks>
        /// Does nothing on an element without a <see cref="VisualElement.contentContainer"/>, such as a <see cref="ListView"/> or a <see cref="TreeView"/>.
        /// </remarks>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="condition">When <see langword="true"/>, the operation is performed; otherwise, the element is returned unchanged.</param>
        /// <param name="index">The index at which to start inserting children.</param>
        /// <param name="children">The children to insert; <see langword="null"/> entries are skipped.</param>
        /// <returns>The element, for chaining.</returns>
        public static T InsertChildrenIf<T>(this T element, bool condition, int index, params VisualElement?[]? children)
            where T : VisualElement => condition ? element.InsertChildren(index, children) : element;

        /// <summary>
        /// Inserts a list of child elements starting at the specified index in the <see cref="VisualElement.contentContainer"/> of this element.
        /// </summary>
        /// <remarks>
        /// Does nothing on an element without a <see cref="VisualElement.contentContainer"/>, such as a <see cref="ListView"/> or a <see cref="TreeView"/>.
        /// </remarks>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="index">The index at which to start inserting children.</param>
        /// <param name="children">The children to insert; <see langword="null"/> entries are skipped.</param>
        /// <returns>The element, for chaining.</returns>
        public static T InsertChildren<T>(this T element, int index, List<VisualElement>? children)
            where T : VisualElement
        {
            if (children is null) return element;

            foreach (var child in children)
            {
                if (child is null) continue;
                element.Insert(index++, child);
            }

            return element;
        }

        /// <summary>
        /// Conditionally inserts a list of child elements starting at the specified index in the <see cref="VisualElement.contentContainer"/> of this element.
        /// </summary>
        /// <remarks>
        /// Does nothing on an element without a <see cref="VisualElement.contentContainer"/>, such as a <see cref="ListView"/> or a <see cref="TreeView"/>.
        /// </remarks>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="condition">When <see langword="true"/>, the operation is performed; otherwise, the element is returned unchanged.</param>
        /// <param name="index">The index at which to start inserting children.</param>
        /// <param name="children">The children to insert; <see langword="null"/> entries are skipped.</param>
        /// <returns>The element, for chaining.</returns>
        public static T InsertChildrenIf<T>(this T element, bool condition, int index, List<VisualElement>? children)
            where T : VisualElement => condition ? element.InsertChildren(index, children) : element;

        /// <summary>
        /// Inserts an enumerable of child elements starting at the specified index in the <see cref="VisualElement.contentContainer"/> of this element.
        /// </summary>
        /// <remarks>
        /// <para>
        /// <paramref name="children"/> is copied before the first change, so it may be the live
        /// <see cref="VisualElement.Children"/> of another element.
        /// </para>
        /// <para>
        /// Does nothing on an element without a <see cref="VisualElement.contentContainer"/>, such as a <see cref="ListView"/> or a <see cref="TreeView"/>.
        /// </para>
        /// </remarks>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="index">The index at which to start inserting children.</param>
        /// <param name="children">The children to insert; <see langword="null"/> entries are skipped.</param>
        /// <returns>The element, for chaining.</returns>
        public static T InsertChildren<T>(this T element, int index, IEnumerable<VisualElement?>? children)
            where T : VisualElement
        {
            switch (children)
            {
                case null: return element;
                case VisualElement?[] array: return element.InsertChildren(index, array);
            }

            using var pooled = ListPool<VisualElement?>.Get(out var snapshot);
            snapshot.AddRange(children);

            foreach (var child in snapshot)
            {
                if (child is null) continue;
                element.Insert(index++, child);
            }

            return element;
        }

        /// <summary>
        /// Conditionally inserts an enumerable of child elements starting at the specified index in the <see cref="VisualElement.contentContainer"/> of this element.
        /// </summary>
        /// <remarks>
        /// <para>
        /// <paramref name="children"/> is copied before the first change, so it may be the live
        /// <see cref="VisualElement.Children"/> of another element.
        /// </para>
        /// <para>
        /// Does nothing on an element without a <see cref="VisualElement.contentContainer"/>, such as a <see cref="ListView"/> or a <see cref="TreeView"/>.
        /// </para>
        /// </remarks>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="condition">When <see langword="true"/>, the operation is performed; otherwise, the element is returned unchanged.</param>
        /// <param name="index">The index at which to start inserting children.</param>
        /// <param name="children">The children to insert; <see langword="null"/> entries are skipped.</param>
        /// <returns>The element, for chaining.</returns>
        public static T InsertChildrenIf<T>(this T element, bool condition, int index, IEnumerable<VisualElement?>? children)
            where T : VisualElement => condition ? element.InsertChildren(index, children) : element;

        /// <summary>
        /// Inserts a read-only span of child elements starting at the specified index in the <see cref="VisualElement.contentContainer"/> of this element.
        /// </summary>
        /// <remarks>
        /// Does nothing on an element without a <see cref="VisualElement.contentContainer"/>, such as a <see cref="ListView"/> or a <see cref="TreeView"/>.
        /// </remarks>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="index">The index at which to start inserting children.</param>
        /// <param name="children">The children to insert; <see langword="null"/> entries are skipped.</param>
        /// <returns>The element, for chaining.</returns>
        public static T InsertChildren<T>(this T element, int index, ReadOnlySpan<VisualElement> children)
            where T : VisualElement
        {
            foreach (var child in children)
            {
                if (child is null) continue;
                element.Insert(index++, child);
            }

            return element;
        }

        /// <summary>
        /// Conditionally inserts a read-only span of child elements starting at the specified index in the <see cref="VisualElement.contentContainer"/> of this element.
        /// </summary>
        /// <remarks>
        /// Does nothing on an element without a <see cref="VisualElement.contentContainer"/>, such as a <see cref="ListView"/> or a <see cref="TreeView"/>.
        /// </remarks>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="condition">When <see langword="true"/>, the operation is performed; otherwise, the element is returned unchanged.</param>
        /// <param name="index">The index at which to start inserting children.</param>
        /// <param name="children">The children to insert; <see langword="null"/> entries are skipped.</param>
        /// <returns>The element, for chaining.</returns>
        public static T InsertChildrenIf<T>(this T element, bool condition, int index, ReadOnlySpan<VisualElement> children)
            where T : VisualElement => condition ? element.InsertChildren(index, children) : element;
        #endregion

        #region RemoveChildren
        /// <summary>
        /// Removes the specified child from the element.
        /// </summary>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="child">The child element to remove; <see langword="null"/> or an element that is not a child leaves the element unchanged.</param>
        /// <returns>The element, for chaining.</returns>
        public static T RemoveChild<T>(this T element, VisualElement? child)
            where T : VisualElement
        {
            if (IsContentChild(element, child)) element.Remove(child);
            return element;
        }

        /// <summary>
        /// Conditionally removes the specified child from the element.
        /// </summary>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="condition">When <see langword="true"/>, the operation is performed; otherwise, the element is returned unchanged.</param>
        /// <param name="child">The child element to remove; <see langword="null"/> or an element that is not a child leaves the element unchanged.</param>
        /// <returns>The element, for chaining.</returns>
        public static T RemoveChildIf<T>(this T element, bool condition, VisualElement? child)
            where T : VisualElement => condition ? element.RemoveChild(child) : element;

        /// <summary>
        /// Removes the child at the specified index from the element.
        /// </summary>
        /// <remarks>
        /// Does nothing on an element without a <see cref="VisualElement.contentContainer"/>, such as a <see cref="ListView"/> or a <see cref="TreeView"/>.
        /// </remarks>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="index">The index of the child to remove.</param>
        /// <returns>The element, for chaining.</returns>
        public static T RemoveChildAt<T>(this T element, int index)
            where T : VisualElement
        {
            element.RemoveAt(index);
            return element;
        }

        /// <summary>
        /// Conditionally removes the child at the specified index from the element.
        /// </summary>
        /// <remarks>
        /// Does nothing on an element without a <see cref="VisualElement.contentContainer"/>, such as a <see cref="ListView"/> or a <see cref="TreeView"/>.
        /// </remarks>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="condition">When <see langword="true"/>, the operation is performed; otherwise, the element is returned unchanged.</param>
        /// <param name="index">The index of the child to remove.</param>
        /// <returns>The element, for chaining.</returns>
        public static T RemoveChildAtIf<T>(this T element, bool condition, int index)
            where T : VisualElement => condition ? element.RemoveChildAt(index) : element;

        /// <summary>
        /// Removes an array of child elements from the element.
        /// </summary>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="children">The children to remove; <see langword="null"/> entries and elements that are not children are skipped.</param>
        /// <returns>The element, for chaining.</returns>
        public static T RemoveChildren<T>(this T element, params VisualElement?[]? children)
            where T : VisualElement
        {
            if (children is null) return element;

            foreach (var child in children)
                element.RemoveChild(child);

            return element;
        }

        /// <summary>
        /// Conditionally removes an array of child elements from the element.
        /// </summary>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="condition">When <see langword="true"/>, the operation is performed; otherwise, the element is returned unchanged.</param>
        /// <param name="children">The children to remove; <see langword="null"/> entries and elements that are not children are skipped.</param>
        /// <returns>The element, for chaining.</returns>
        public static T RemoveChildrenIf<T>(this T element, bool condition, params VisualElement?[]? children)
            where T : VisualElement => condition ? element.RemoveChildren(children) : element;

        /// <summary>
        /// Removes a list of child elements from the element.
        /// </summary>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="children">The children to remove; <see langword="null"/> entries and elements that are not children are skipped.</param>
        /// <returns>The element, for chaining.</returns>
        public static T RemoveChildren<T>(this T element, List<VisualElement>? children)
            where T : VisualElement
        {
            if (children is null) return element;

            foreach (var child in children)
                element.RemoveChild(child);

            return element;
        }

        /// <summary>
        /// Conditionally removes a list of child elements from the element.
        /// </summary>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="condition">When <see langword="true"/>, the operation is performed; otherwise, the element is returned unchanged.</param>
        /// <param name="children">The children to remove; <see langword="null"/> entries and elements that are not children are skipped.</param>
        /// <returns>The element, for chaining.</returns>
        public static T RemoveChildrenIf<T>(this T element, bool condition, List<VisualElement>? children)
            where T : VisualElement => condition ? element.RemoveChildren(children) : element;

        /// <summary>
        /// Removes an enumerable of child elements from the element.
        /// </summary>
        /// <remarks>
        /// <paramref name="children"/> is copied before the first change, so it may be this element's own
        /// <see cref="VisualElement.Children"/>.
        /// </remarks>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="children">The children to remove; <see langword="null"/> entries and elements that are not children are skipped.</param>
        /// <returns>The element, for chaining.</returns>
        public static T RemoveChildren<T>(this T element, IEnumerable<VisualElement?>? children)
            where T : VisualElement
        {
            switch (children)
            {
                case null: return element;
                case VisualElement?[] array: return element.RemoveChildren(array);
            }

            using var pooled = ListPool<VisualElement?>.Get(out var snapshot);
            snapshot.AddRange(children);

            foreach (var child in snapshot)
                element.RemoveChild(child);

            return element;
        }

        /// <summary>
        /// Conditionally removes an enumerable of child elements from the element.
        /// </summary>
        /// <remarks>
        /// <paramref name="children"/> is copied before the first change, so it may be this element's own
        /// <see cref="VisualElement.Children"/>.
        /// </remarks>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="condition">When <see langword="true"/>, the operation is performed; otherwise, the element is returned unchanged.</param>
        /// <param name="children">The children to remove; <see langword="null"/> entries and elements that are not children are skipped.</param>
        /// <returns>The element, for chaining.</returns>
        public static T RemoveChildrenIf<T>(this T element, bool condition, IEnumerable<VisualElement?>? children)
            where T : VisualElement => condition ? element.RemoveChildren(children) : element;

        /// <summary>
        /// Removes a read-only span of child elements from the element.
        /// </summary>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="children">The children to remove; <see langword="null"/> entries and elements that are not children are skipped.</param>
        /// <returns>The element, for chaining.</returns>
        public static T RemoveChildren<T>(this T element, ReadOnlySpan<VisualElement> children)
            where T : VisualElement
        {
            foreach (var child in children)
                element.RemoveChild(child);

            return element;
        }

        /// <summary>
        /// Conditionally removes a read-only span of child elements from the element.
        /// </summary>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="condition">When <see langword="true"/>, the operation is performed; otherwise, the element is returned unchanged.</param>
        /// <param name="children">The children to remove; <see langword="null"/> entries and elements that are not children are skipped.</param>
        /// <returns>The element, for chaining.</returns>
        public static T RemoveChildrenIf<T>(this T element, bool condition, ReadOnlySpan<VisualElement> children)
            where T : VisualElement => condition ? element.RemoveChildren(children) : element;
        #endregion

        #region ClearChildren
        /// <summary>
        /// Removes all children from the element.
        /// </summary>
        /// <remarks>
        /// Does nothing on an element without a <see cref="VisualElement.contentContainer"/>, such as a <see cref="ListView"/> or a <see cref="TreeView"/>.
        /// </remarks>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <returns>The element, for chaining.</returns>
        public static T ClearChildren<T>(this T element)
            where T : VisualElement
        {
            element.Clear();
            return element;
        }

        /// <summary>
        /// Conditionally removes all children from the element.
        /// </summary>
        /// <remarks>
        /// Does nothing on an element without a <see cref="VisualElement.contentContainer"/>, such as a <see cref="ListView"/> or a <see cref="TreeView"/>.
        /// </remarks>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="condition">When <see langword="true"/>, the operation is performed; otherwise, the element is returned unchanged.</param>
        /// <returns>The element, for chaining.</returns>
        public static T ClearChildrenIf<T>(this T element, bool condition)
            where T : VisualElement => condition ? element.ClearChildren() : element;
        #endregion

        private static bool IsContentChild(VisualElement element, VisualElement? child) =>
            child is not null && element.IndexOf(child) >= 0;
    }
}
