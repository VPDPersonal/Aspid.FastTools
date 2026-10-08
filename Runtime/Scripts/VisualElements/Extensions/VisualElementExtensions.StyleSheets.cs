#nullable enable
using System;
using UnityEngine;
using UnityEngine.UIElements;
using System.Collections.Generic;

// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.UIElements
{
    public static partial class VisualElementExtensions
    {
        #region AddStyleSheets
        /// <summary>
        /// Adds a style sheet to <see cref="VisualElement.styleSheets"/>.
        /// </summary>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="styleSheet">The style sheet to add; <see langword="null"/> leaves the element unchanged.</param>
        /// <returns>The element, for chaining.</returns>
        public static T AddStyleSheet<T>(this T element, StyleSheet? styleSheet)
            where T : VisualElement
        {
            if (styleSheet != null) element.styleSheets.Add(styleSheet);
            return element;
        }

        /// <summary>
        /// Conditionally adds a style sheet to <see cref="VisualElement.styleSheets"/>.
        /// </summary>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="condition">When <see langword="true"/>, the operation is performed; otherwise, the element is returned unchanged.</param>
        /// <param name="styleSheet">The style sheet to add; <see langword="null"/> leaves the element unchanged.</param>
        /// <returns>The element, for chaining.</returns>
        public static T AddStyleSheetIf<T>(this T element, bool condition, StyleSheet? styleSheet)
            where T : VisualElement => condition ? element.AddStyleSheet(styleSheet) : element;

        /// <summary>
        /// Adds an array of style sheets to <see cref="VisualElement.styleSheets"/>.
        /// </summary>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="styleSheets">The style sheets to add; <see langword="null"/> entries are skipped.</param>
        /// <returns>The element, for chaining.</returns>
        public static T AddStyleSheets<T>(this T element, params StyleSheet?[]? styleSheets)
            where T : VisualElement
        {
            if (styleSheets is null) return element;

            foreach (var styleSheet in styleSheets)
                element.AddStyleSheet(styleSheet);

            return element;
        }

        /// <summary>
        /// Conditionally adds an array of style sheets to <see cref="VisualElement.styleSheets"/>.
        /// </summary>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="condition">When <see langword="true"/>, the operation is performed; otherwise, the element is returned unchanged.</param>
        /// <param name="styleSheets">The style sheets to add; <see langword="null"/> entries are skipped.</param>
        /// <returns>The element, for chaining.</returns>
        public static T AddStyleSheetsIf<T>(this T element, bool condition, params StyleSheet?[]? styleSheets)
            where T : VisualElement => condition ? element.AddStyleSheets(styleSheets) : element;

        /// <summary>
        /// Adds a list of style sheets to <see cref="VisualElement.styleSheets"/>.
        /// </summary>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="styleSheets">The style sheets to add; <see langword="null"/> entries are skipped.</param>
        /// <returns>The element, for chaining.</returns>
        public static T AddStyleSheets<T>(this T element, List<StyleSheet>? styleSheets)
            where T : VisualElement
        {
            if (styleSheets is null) return element;

            foreach (var styleSheet in styleSheets)
                element.AddStyleSheet(styleSheet);

            return element;
        }

        /// <summary>
        /// Conditionally adds a list of style sheets to <see cref="VisualElement.styleSheets"/>.
        /// </summary>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="condition">When <see langword="true"/>, the operation is performed; otherwise, the element is returned unchanged.</param>
        /// <param name="styleSheets">The style sheets to add; <see langword="null"/> entries are skipped.</param>
        /// <returns>The element, for chaining.</returns>
        public static T AddStyleSheetsIf<T>(this T element, bool condition, List<StyleSheet>? styleSheets)
            where T : VisualElement => condition ? element.AddStyleSheets(styleSheets) : element;

        /// <summary>
        /// Adds an enumerable of style sheets to <see cref="VisualElement.styleSheets"/>.
        /// </summary>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="styleSheets">The style sheets to add; <see langword="null"/> entries are skipped.</param>
        /// <returns>The element, for chaining.</returns>
        public static T AddStyleSheets<T>(this T element, IEnumerable<StyleSheet?>? styleSheets)
            where T : VisualElement
        {
            if (styleSheets is null) return element;

            foreach (var styleSheet in styleSheets)
                element.AddStyleSheet(styleSheet);

            return element;
        }

        /// <summary>
        /// Conditionally adds an enumerable of style sheets to <see cref="VisualElement.styleSheets"/>.
        /// </summary>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="condition">When <see langword="true"/>, the operation is performed; otherwise, the element is returned unchanged.</param>
        /// <param name="styleSheets">The style sheets to add; <see langword="null"/> entries are skipped.</param>
        /// <returns>The element, for chaining.</returns>
        public static T AddStyleSheetsIf<T>(this T element, bool condition, IEnumerable<StyleSheet?>? styleSheets)
            where T : VisualElement => condition ? element.AddStyleSheets(styleSheets) : element;

        /// <summary>
        /// Adds a read-only span of style sheets to <see cref="VisualElement.styleSheets"/>.
        /// </summary>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="styleSheets">The style sheets to add; <see langword="null"/> entries are skipped.</param>
        /// <returns>The element, for chaining.</returns>
        public static T AddStyleSheets<T>(this T element, ReadOnlySpan<StyleSheet> styleSheets)
            where T : VisualElement
        {
            foreach (var styleSheet in styleSheets)
                element.AddStyleSheet(styleSheet);

            return element;
        }

        /// <summary>
        /// Conditionally adds a read-only span of style sheets to <see cref="VisualElement.styleSheets"/>.
        /// </summary>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="condition">When <see langword="true"/>, the operation is performed; otherwise, the element is returned unchanged.</param>
        /// <param name="styleSheets">The style sheets to add; <see langword="null"/> entries are skipped.</param>
        /// <returns>The element, for chaining.</returns>
        public static T AddStyleSheetsIf<T>(this T element, bool condition, ReadOnlySpan<StyleSheet> styleSheets)
            where T : VisualElement => condition ? element.AddStyleSheets(styleSheets) : element;

        /// <summary>
        /// Loads a <see cref="StyleSheet"/> from Resources and adds it to <see cref="VisualElement.styleSheets"/>.
        /// </summary>
        /// <remarks>
        /// Logs a warning and leaves the element unchanged when no asset is found at <paramref name="path"/>.
        /// </remarks>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="path">The Resources-relative path to the style sheet asset.</param>
        /// <returns>The element, for chaining.</returns>
        public static T AddStyleSheetFromResources<T>(this T element, string path)
            where T : VisualElement => element.AddStyleSheet(LoadStyleSheetFromResources(path));

        /// <summary>
        /// Conditionally loads a <see cref="StyleSheet"/> from Resources and adds it to <see cref="VisualElement.styleSheets"/>.
        /// </summary>
        /// <remarks>
        /// Logs a warning and leaves the element unchanged when no asset is found at <paramref name="path"/>.
        /// </remarks>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="condition">When <see langword="true"/>, the operation is performed; otherwise, the element is returned unchanged.</param>
        /// <param name="path">The Resources-relative path to the style sheet asset.</param>
        /// <returns>The element, for chaining.</returns>
        public static T AddStyleSheetFromResourcesIf<T>(this T element, bool condition, string path)
            where T : VisualElement => condition ? element.AddStyleSheetFromResources(path) : element;

        /// <summary>
        /// Loads style sheets from an array of Resources paths and adds them to <see cref="VisualElement.styleSheets"/>.
        /// </summary>
        /// <remarks>
        /// Logs a warning for each path where no asset is found and skips that path.
        /// </remarks>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="paths">The Resources-relative paths to the style sheet assets.</param>
        /// <returns>The element, for chaining.</returns>
        public static T AddStyleSheetsFromResources<T>(this T element, params string[]? paths)
            where T : VisualElement
        {
            if (paths is null) return element;

            foreach (var path in paths)
                element.AddStyleSheet(LoadStyleSheetFromResources(path));

            return element;
        }

        /// <summary>
        /// Conditionally loads style sheets from an array of Resources paths and adds them to <see cref="VisualElement.styleSheets"/>.
        /// </summary>
        /// <remarks>
        /// Logs a warning for each path where no asset is found and skips that path.
        /// </remarks>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="condition">When <see langword="true"/>, the operation is performed; otherwise, the element is returned unchanged.</param>
        /// <param name="paths">The Resources-relative paths to the style sheet assets.</param>
        /// <returns>The element, for chaining.</returns>
        public static T AddStyleSheetsFromResourcesIf<T>(this T element, bool condition, params string[]? paths)
            where T : VisualElement => condition ? element.AddStyleSheetsFromResources(paths) : element;

        /// <summary>
        /// Loads style sheets from a list of Resources paths and adds them to <see cref="VisualElement.styleSheets"/>.
        /// </summary>
        /// <remarks>
        /// Logs a warning for each path where no asset is found and skips that path.
        /// </remarks>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="paths">The Resources-relative paths to the style sheet assets.</param>
        /// <returns>The element, for chaining.</returns>
        public static T AddStyleSheetsFromResources<T>(this T element, List<string>? paths)
            where T : VisualElement
        {
            if (paths is null) return element;

            foreach (var path in paths)
                element.AddStyleSheet(LoadStyleSheetFromResources(path));

            return element;
        }

        /// <summary>
        /// Conditionally loads style sheets from a list of Resources paths and adds them to <see cref="VisualElement.styleSheets"/>.
        /// </summary>
        /// <remarks>
        /// Logs a warning for each path where no asset is found and skips that path.
        /// </remarks>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="condition">When <see langword="true"/>, the operation is performed; otherwise, the element is returned unchanged.</param>
        /// <param name="paths">The Resources-relative paths to the style sheet assets.</param>
        /// <returns>The element, for chaining.</returns>
        public static T AddStyleSheetsFromResourcesIf<T>(this T element, bool condition, List<string>? paths)
            where T : VisualElement => condition ? element.AddStyleSheetsFromResources(paths) : element;

        /// <summary>
        /// Loads style sheets from an enumerable of Resources paths and adds them to <see cref="VisualElement.styleSheets"/>.
        /// </summary>
        /// <remarks>
        /// Logs a warning for each path where no asset is found and skips that path.
        /// </remarks>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="paths">The Resources-relative paths to the style sheet assets.</param>
        /// <returns>The element, for chaining.</returns>
        public static T AddStyleSheetsFromResources<T>(this T element, IEnumerable<string>? paths)
            where T : VisualElement
        {
            if (paths is null) return element;

            foreach (var path in paths)
                element.AddStyleSheet(LoadStyleSheetFromResources(path));

            return element;
        }

        /// <summary>
        /// Conditionally loads style sheets from an enumerable of Resources paths and adds them to <see cref="VisualElement.styleSheets"/>.
        /// </summary>
        /// <remarks>
        /// Logs a warning for each path where no asset is found and skips that path.
        /// </remarks>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="condition">When <see langword="true"/>, the operation is performed; otherwise, the element is returned unchanged.</param>
        /// <param name="paths">The Resources-relative paths to the style sheet assets.</param>
        /// <returns>The element, for chaining.</returns>
        public static T AddStyleSheetsFromResourcesIf<T>(this T element, bool condition, IEnumerable<string>? paths)
            where T : VisualElement => condition ? element.AddStyleSheetsFromResources(paths) : element;

        /// <summary>
        /// Loads style sheets from a read-only span of Resources paths and adds them to <see cref="VisualElement.styleSheets"/>.
        /// </summary>
        /// <remarks>
        /// Logs a warning for each path where no asset is found and skips that path.
        /// </remarks>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="paths">The Resources-relative paths to the style sheet assets.</param>
        /// <returns>The element, for chaining.</returns>
        public static T AddStyleSheetsFromResources<T>(this T element, ReadOnlySpan<string> paths)
            where T : VisualElement
        {
            foreach (var path in paths)
                element.AddStyleSheet(LoadStyleSheetFromResources(path));

            return element;
        }

        /// <summary>
        /// Conditionally loads style sheets from a read-only span of Resources paths and adds them to <see cref="VisualElement.styleSheets"/>.
        /// </summary>
        /// <remarks>
        /// Logs a warning for each path where no asset is found and skips that path.
        /// </remarks>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="condition">When <see langword="true"/>, the operation is performed; otherwise, the element is returned unchanged.</param>
        /// <param name="paths">The Resources-relative paths to the style sheet assets.</param>
        /// <returns>The element, for chaining.</returns>
        public static T AddStyleSheetsFromResourcesIf<T>(this T element, bool condition, ReadOnlySpan<string> paths)
            where T : VisualElement => condition ? element.AddStyleSheetsFromResources(paths) : element;
        #endregion

        #region InsertStyleSheets
        /// <summary>
        /// Inserts a style sheet into <see cref="VisualElement.styleSheets"/> at the specified index.
        /// </summary>
        /// <remarks>
        /// A style sheet that <see cref="VisualElement.styleSheets"/> already holds keeps its position.
        /// </remarks>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="index">The index at which to insert the style sheet.</param>
        /// <param name="styleSheet">The style sheet to insert; <see langword="null"/> leaves the element unchanged.</param>
        /// <returns>The element, for chaining.</returns>
        public static T InsertStyleSheet<T>(this T element, int index, StyleSheet? styleSheet)
            where T : VisualElement
        {
            InsertStyleSheetAt(element, index, styleSheet);
            return element;
        }

        /// <summary>
        /// Conditionally inserts a style sheet into <see cref="VisualElement.styleSheets"/> at the specified index.
        /// </summary>
        /// <remarks>
        /// A style sheet that <see cref="VisualElement.styleSheets"/> already holds keeps its position.
        /// </remarks>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="condition">When <see langword="true"/>, the operation is performed; otherwise, the element is returned unchanged.</param>
        /// <param name="index">The index at which to insert the style sheet.</param>
        /// <param name="styleSheet">The style sheet to insert; <see langword="null"/> leaves the element unchanged.</param>
        /// <returns>The element, for chaining.</returns>
        public static T InsertStyleSheetIf<T>(this T element, bool condition, int index, StyleSheet? styleSheet)
            where T : VisualElement => condition ? element.InsertStyleSheet(index, styleSheet) : element;

        /// <summary>
        /// Inserts an array of style sheets into <see cref="VisualElement.styleSheets"/> starting at the specified index.
        /// </summary>
        /// <remarks>
        /// Style sheets are inserted in order; a style sheet that <see cref="VisualElement.styleSheets"/> already holds keeps its position.
        /// </remarks>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="index">The index at which to start inserting style sheets.</param>
        /// <param name="styleSheets">The style sheets to insert; <see langword="null"/> entries are skipped.</param>
        /// <returns>The element, for chaining.</returns>
        public static T InsertStyleSheets<T>(this T element, int index, params StyleSheet?[]? styleSheets)
            where T : VisualElement
        {
            if (styleSheets is null) return element;

            foreach (var styleSheet in styleSheets)
                index = InsertStyleSheetAt(element, index, styleSheet);

            return element;
        }

        /// <summary>
        /// Conditionally inserts an array of style sheets into <see cref="VisualElement.styleSheets"/> starting at the specified index.
        /// </summary>
        /// <remarks>
        /// Style sheets are inserted in order; a style sheet that <see cref="VisualElement.styleSheets"/> already holds keeps its position.
        /// </remarks>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="condition">When <see langword="true"/>, the operation is performed; otherwise, the element is returned unchanged.</param>
        /// <param name="index">The index at which to start inserting style sheets.</param>
        /// <param name="styleSheets">The style sheets to insert; <see langword="null"/> entries are skipped.</param>
        /// <returns>The element, for chaining.</returns>
        public static T InsertStyleSheetsIf<T>(this T element, bool condition, int index, params StyleSheet?[]? styleSheets)
            where T : VisualElement => condition ? element.InsertStyleSheets(index, styleSheets) : element;

        /// <summary>
        /// Inserts a list of style sheets into <see cref="VisualElement.styleSheets"/> starting at the specified index.
        /// </summary>
        /// <remarks>
        /// Style sheets are inserted in order; a style sheet that <see cref="VisualElement.styleSheets"/> already holds keeps its position.
        /// </remarks>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="index">The index at which to start inserting style sheets.</param>
        /// <param name="styleSheets">The style sheets to insert; <see langword="null"/> entries are skipped.</param>
        /// <returns>The element, for chaining.</returns>
        public static T InsertStyleSheets<T>(this T element, int index, List<StyleSheet>? styleSheets)
            where T : VisualElement
        {
            if (styleSheets is null) return element;

            foreach (var styleSheet in styleSheets)
                index = InsertStyleSheetAt(element, index, styleSheet);

            return element;
        }

        /// <summary>
        /// Conditionally inserts a list of style sheets into <see cref="VisualElement.styleSheets"/> starting at the specified index.
        /// </summary>
        /// <remarks>
        /// Style sheets are inserted in order; a style sheet that <see cref="VisualElement.styleSheets"/> already holds keeps its position.
        /// </remarks>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="condition">When <see langword="true"/>, the operation is performed; otherwise, the element is returned unchanged.</param>
        /// <param name="index">The index at which to start inserting style sheets.</param>
        /// <param name="styleSheets">The style sheets to insert; <see langword="null"/> entries are skipped.</param>
        /// <returns>The element, for chaining.</returns>
        public static T InsertStyleSheetsIf<T>(this T element, bool condition, int index, List<StyleSheet>? styleSheets)
            where T : VisualElement => condition ? element.InsertStyleSheets(index, styleSheets) : element;

        /// <summary>
        /// Inserts an enumerable of style sheets into <see cref="VisualElement.styleSheets"/> starting at the specified index.
        /// </summary>
        /// <remarks>
        /// Style sheets are inserted in order; a style sheet that <see cref="VisualElement.styleSheets"/> already holds keeps its position.
        /// </remarks>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="index">The index at which to start inserting style sheets.</param>
        /// <param name="styleSheets">The style sheets to insert; <see langword="null"/> entries are skipped.</param>
        /// <returns>The element, for chaining.</returns>
        public static T InsertStyleSheets<T>(this T element, int index, IEnumerable<StyleSheet?>? styleSheets)
            where T : VisualElement
        {
            if (styleSheets is null) return element;

            foreach (var styleSheet in styleSheets)
                index = InsertStyleSheetAt(element, index, styleSheet);

            return element;
        }

        /// <summary>
        /// Conditionally inserts an enumerable of style sheets into <see cref="VisualElement.styleSheets"/> starting at the specified index.
        /// </summary>
        /// <remarks>
        /// Style sheets are inserted in order; a style sheet that <see cref="VisualElement.styleSheets"/> already holds keeps its position.
        /// </remarks>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="condition">When <see langword="true"/>, the operation is performed; otherwise, the element is returned unchanged.</param>
        /// <param name="index">The index at which to start inserting style sheets.</param>
        /// <param name="styleSheets">The style sheets to insert; <see langword="null"/> entries are skipped.</param>
        /// <returns>The element, for chaining.</returns>
        public static T InsertStyleSheetsIf<T>(this T element, bool condition, int index, IEnumerable<StyleSheet?>? styleSheets)
            where T : VisualElement => condition ? element.InsertStyleSheets(index, styleSheets) : element;

        /// <summary>
        /// Inserts a read-only span of style sheets into <see cref="VisualElement.styleSheets"/> starting at the specified index.
        /// </summary>
        /// <remarks>
        /// Style sheets are inserted in order; a style sheet that <see cref="VisualElement.styleSheets"/> already holds keeps its position.
        /// </remarks>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="index">The index at which to start inserting style sheets.</param>
        /// <param name="styleSheets">The style sheets to insert; <see langword="null"/> entries are skipped.</param>
        /// <returns>The element, for chaining.</returns>
        public static T InsertStyleSheets<T>(this T element, int index, ReadOnlySpan<StyleSheet> styleSheets)
            where T : VisualElement
        {
            foreach (var styleSheet in styleSheets)
                index = InsertStyleSheetAt(element, index, styleSheet);

            return element;
        }

        /// <summary>
        /// Conditionally inserts a read-only span of style sheets into <see cref="VisualElement.styleSheets"/> starting at the specified index.
        /// </summary>
        /// <remarks>
        /// Style sheets are inserted in order; a style sheet that <see cref="VisualElement.styleSheets"/> already holds keeps its position.
        /// </remarks>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="condition">When <see langword="true"/>, the operation is performed; otherwise, the element is returned unchanged.</param>
        /// <param name="index">The index at which to start inserting style sheets.</param>
        /// <param name="styleSheets">The style sheets to insert; <see langword="null"/> entries are skipped.</param>
        /// <returns>The element, for chaining.</returns>
        public static T InsertStyleSheetsIf<T>(this T element, bool condition, int index, ReadOnlySpan<StyleSheet> styleSheets)
            where T : VisualElement => condition ? element.InsertStyleSheets(index, styleSheets) : element;

        /// <summary>
        /// Loads a <see cref="StyleSheet"/> from Resources and inserts it into <see cref="VisualElement.styleSheets"/> at the specified index.
        /// </summary>
        /// <remarks>
        /// <para>Logs a warning and leaves the element unchanged when no asset is found at <paramref name="path"/>.</para>
        /// <para>A style sheet that <see cref="VisualElement.styleSheets"/> already holds keeps its position.</para>
        /// </remarks>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="index">The index at which to insert the style sheet.</param>
        /// <param name="path">The Resources-relative path to the style sheet asset.</param>
        /// <returns>The element, for chaining.</returns>
        public static T InsertStyleSheetFromResources<T>(this T element, int index, string path)
            where T : VisualElement => element.InsertStyleSheet(index, LoadStyleSheetFromResources(path));

        /// <summary>
        /// Conditionally loads a <see cref="StyleSheet"/> from Resources and inserts it into <see cref="VisualElement.styleSheets"/> at the specified index.
        /// </summary>
        /// <remarks>
        /// <para>Logs a warning and leaves the element unchanged when no asset is found at <paramref name="path"/>.</para>
        /// <para>A style sheet that <see cref="VisualElement.styleSheets"/> already holds keeps its position.</para>
        /// </remarks>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="condition">When <see langword="true"/>, the operation is performed; otherwise, the element is returned unchanged.</param>
        /// <param name="index">The index at which to insert the style sheet.</param>
        /// <param name="path">The Resources-relative path to the style sheet asset.</param>
        /// <returns>The element, for chaining.</returns>
        public static T InsertStyleSheetFromResourcesIf<T>(this T element, bool condition, int index, string path)
            where T : VisualElement => condition ? element.InsertStyleSheetFromResources(index, path) : element;

        /// <summary>
        /// Loads style sheets from an array of Resources paths and inserts them into <see cref="VisualElement.styleSheets"/> starting at the specified index.
        /// </summary>
        /// <remarks>
        /// <para>Logs a warning for each path where no asset is found and skips that path.</para>
        /// <para>Style sheets are inserted in order; a style sheet that <see cref="VisualElement.styleSheets"/> already holds keeps its position.</para>
        /// </remarks>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="index">The index at which to start inserting style sheets.</param>
        /// <param name="paths">The Resources-relative paths to the style sheet assets.</param>
        /// <returns>The element, for chaining.</returns>
        public static T InsertStyleSheetsFromResources<T>(this T element, int index, params string[]? paths)
            where T : VisualElement
        {
            if (paths is null) return element;

            foreach (var path in paths)
                index = InsertStyleSheetAt(element, index, LoadStyleSheetFromResources(path));

            return element;
        }

        /// <summary>
        /// Conditionally loads style sheets from an array of Resources paths and inserts them into <see cref="VisualElement.styleSheets"/> starting at the specified index.
        /// </summary>
        /// <remarks>
        /// <para>Logs a warning for each path where no asset is found and skips that path.</para>
        /// <para>Style sheets are inserted in order; a style sheet that <see cref="VisualElement.styleSheets"/> already holds keeps its position.</para>
        /// </remarks>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="condition">When <see langword="true"/>, the operation is performed; otherwise, the element is returned unchanged.</param>
        /// <param name="index">The index at which to start inserting style sheets.</param>
        /// <param name="paths">The Resources-relative paths to the style sheet assets.</param>
        /// <returns>The element, for chaining.</returns>
        public static T InsertStyleSheetsFromResourcesIf<T>(this T element, bool condition, int index, params string[]? paths)
            where T : VisualElement => condition ? element.InsertStyleSheetsFromResources(index, paths) : element;

        /// <summary>
        /// Loads style sheets from a list of Resources paths and inserts them into <see cref="VisualElement.styleSheets"/> starting at the specified index.
        /// </summary>
        /// <remarks>
        /// <para>Logs a warning for each path where no asset is found and skips that path.</para>
        /// <para>Style sheets are inserted in order; a style sheet that <see cref="VisualElement.styleSheets"/> already holds keeps its position.</para>
        /// </remarks>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="index">The index at which to start inserting style sheets.</param>
        /// <param name="paths">The Resources-relative paths to the style sheet assets.</param>
        /// <returns>The element, for chaining.</returns>
        public static T InsertStyleSheetsFromResources<T>(this T element, int index, List<string>? paths)
            where T : VisualElement
        {
            if (paths is null) return element;

            foreach (var path in paths)
                index = InsertStyleSheetAt(element, index, LoadStyleSheetFromResources(path));

            return element;
        }

        /// <summary>
        /// Conditionally loads style sheets from a list of Resources paths and inserts them into <see cref="VisualElement.styleSheets"/> starting at the specified index.
        /// </summary>
        /// <remarks>
        /// <para>Logs a warning for each path where no asset is found and skips that path.</para>
        /// <para>Style sheets are inserted in order; a style sheet that <see cref="VisualElement.styleSheets"/> already holds keeps its position.</para>
        /// </remarks>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="condition">When <see langword="true"/>, the operation is performed; otherwise, the element is returned unchanged.</param>
        /// <param name="index">The index at which to start inserting style sheets.</param>
        /// <param name="paths">The Resources-relative paths to the style sheet assets.</param>
        /// <returns>The element, for chaining.</returns>
        public static T InsertStyleSheetsFromResourcesIf<T>(this T element, bool condition, int index, List<string>? paths)
            where T : VisualElement => condition ? element.InsertStyleSheetsFromResources(index, paths) : element;

        /// <summary>
        /// Loads style sheets from an enumerable of Resources paths and inserts them into <see cref="VisualElement.styleSheets"/> starting at the specified index.
        /// </summary>
        /// <remarks>
        /// <para>Logs a warning for each path where no asset is found and skips that path.</para>
        /// <para>Style sheets are inserted in order; a style sheet that <see cref="VisualElement.styleSheets"/> already holds keeps its position.</para>
        /// </remarks>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="index">The index at which to start inserting style sheets.</param>
        /// <param name="paths">The Resources-relative paths to the style sheet assets.</param>
        /// <returns>The element, for chaining.</returns>
        public static T InsertStyleSheetsFromResources<T>(this T element, int index, IEnumerable<string>? paths)
            where T : VisualElement
        {
            if (paths is null) return element;

            foreach (var path in paths)
                index = InsertStyleSheetAt(element, index, LoadStyleSheetFromResources(path));

            return element;
        }

        /// <summary>
        /// Conditionally loads style sheets from an enumerable of Resources paths and inserts them into <see cref="VisualElement.styleSheets"/> starting at the specified index.
        /// </summary>
        /// <remarks>
        /// <para>Logs a warning for each path where no asset is found and skips that path.</para>
        /// <para>Style sheets are inserted in order; a style sheet that <see cref="VisualElement.styleSheets"/> already holds keeps its position.</para>
        /// </remarks>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="condition">When <see langword="true"/>, the operation is performed; otherwise, the element is returned unchanged.</param>
        /// <param name="index">The index at which to start inserting style sheets.</param>
        /// <param name="paths">The Resources-relative paths to the style sheet assets.</param>
        /// <returns>The element, for chaining.</returns>
        public static T InsertStyleSheetsFromResourcesIf<T>(this T element, bool condition, int index, IEnumerable<string>? paths)
            where T : VisualElement => condition ? element.InsertStyleSheetsFromResources(index, paths) : element;

        /// <summary>
        /// Loads style sheets from a read-only span of Resources paths and inserts them into <see cref="VisualElement.styleSheets"/> starting at the specified index.
        /// </summary>
        /// <remarks>
        /// <para>Logs a warning for each path where no asset is found and skips that path.</para>
        /// <para>Style sheets are inserted in order; a style sheet that <see cref="VisualElement.styleSheets"/> already holds keeps its position.</para>
        /// </remarks>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="index">The index at which to start inserting style sheets.</param>
        /// <param name="paths">The Resources-relative paths to the style sheet assets.</param>
        /// <returns>The element, for chaining.</returns>
        public static T InsertStyleSheetsFromResources<T>(this T element, int index, ReadOnlySpan<string> paths)
            where T : VisualElement
        {
            foreach (var path in paths)
                index = InsertStyleSheetAt(element, index, LoadStyleSheetFromResources(path));

            return element;
        }

        /// <summary>
        /// Conditionally loads style sheets from a read-only span of Resources paths and inserts them into <see cref="VisualElement.styleSheets"/> starting at the specified index.
        /// </summary>
        /// <remarks>
        /// <para>Logs a warning for each path where no asset is found and skips that path.</para>
        /// <para>Style sheets are inserted in order; a style sheet that <see cref="VisualElement.styleSheets"/> already holds keeps its position.</para>
        /// </remarks>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="condition">When <see langword="true"/>, the operation is performed; otherwise, the element is returned unchanged.</param>
        /// <param name="index">The index at which to start inserting style sheets.</param>
        /// <param name="paths">The Resources-relative paths to the style sheet assets.</param>
        /// <returns>The element, for chaining.</returns>
        public static T InsertStyleSheetsFromResourcesIf<T>(this T element, bool condition, int index, ReadOnlySpan<string> paths)
            where T : VisualElement => condition ? element.InsertStyleSheetsFromResources(index, paths) : element;
        #endregion

        #region RemoveStyleSheets
        /// <summary>
        /// Removes a style sheet from <see cref="VisualElement.styleSheets"/>.
        /// </summary>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="styleSheet">The style sheet to remove; <see langword="null"/> leaves the element unchanged.</param>
        /// <returns>The element, for chaining.</returns>
        public static T RemoveStyleSheet<T>(this T element, StyleSheet? styleSheet)
            where T : VisualElement
        {
            if (styleSheet != null) element.styleSheets.Remove(styleSheet);
            return element;
        }

        /// <summary>
        /// Conditionally removes a style sheet from <see cref="VisualElement.styleSheets"/>.
        /// </summary>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="condition">When <see langword="true"/>, the operation is performed; otherwise, the element is returned unchanged.</param>
        /// <param name="styleSheet">The style sheet to remove; <see langword="null"/> leaves the element unchanged.</param>
        /// <returns>The element, for chaining.</returns>
        public static T RemoveStyleSheetIf<T>(this T element, bool condition, StyleSheet? styleSheet)
            where T : VisualElement => condition ? element.RemoveStyleSheet(styleSheet) : element;

        /// <summary>
        /// Removes an array of style sheets from <see cref="VisualElement.styleSheets"/>.
        /// </summary>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="styleSheets">The style sheets to remove; <see langword="null"/> entries are skipped.</param>
        /// <returns>The element, for chaining.</returns>
        public static T RemoveStyleSheets<T>(this T element, params StyleSheet?[]? styleSheets)
            where T : VisualElement
        {
            if (styleSheets is null) return element;

            foreach (var styleSheet in styleSheets)
                element.RemoveStyleSheet(styleSheet);

            return element;
        }

        /// <summary>
        /// Conditionally removes an array of style sheets from <see cref="VisualElement.styleSheets"/>.
        /// </summary>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="condition">When <see langword="true"/>, the operation is performed; otherwise, the element is returned unchanged.</param>
        /// <param name="styleSheets">The style sheets to remove; <see langword="null"/> entries are skipped.</param>
        /// <returns>The element, for chaining.</returns>
        public static T RemoveStyleSheetsIf<T>(this T element, bool condition, params StyleSheet?[]? styleSheets)
            where T : VisualElement => condition ? element.RemoveStyleSheets(styleSheets) : element;

        /// <summary>
        /// Removes a list of style sheets from <see cref="VisualElement.styleSheets"/>.
        /// </summary>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="styleSheets">The style sheets to remove; <see langword="null"/> entries are skipped.</param>
        /// <returns>The element, for chaining.</returns>
        public static T RemoveStyleSheets<T>(this T element, List<StyleSheet>? styleSheets)
            where T : VisualElement
        {
            if (styleSheets is null) return element;

            foreach (var styleSheet in styleSheets)
                element.RemoveStyleSheet(styleSheet);

            return element;
        }

        /// <summary>
        /// Conditionally removes a list of style sheets from <see cref="VisualElement.styleSheets"/>.
        /// </summary>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="condition">When <see langword="true"/>, the operation is performed; otherwise, the element is returned unchanged.</param>
        /// <param name="styleSheets">The style sheets to remove; <see langword="null"/> entries are skipped.</param>
        /// <returns>The element, for chaining.</returns>
        public static T RemoveStyleSheetsIf<T>(this T element, bool condition, List<StyleSheet>? styleSheets)
            where T : VisualElement => condition ? element.RemoveStyleSheets(styleSheets) : element;

        /// <summary>
        /// Removes an enumerable of style sheets from <see cref="VisualElement.styleSheets"/>.
        /// </summary>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="styleSheets">The style sheets to remove; <see langword="null"/> entries are skipped.</param>
        /// <returns>The element, for chaining.</returns>
        public static T RemoveStyleSheets<T>(this T element, IEnumerable<StyleSheet?>? styleSheets)
            where T : VisualElement
        {
            if (styleSheets is null) return element;

            foreach (var styleSheet in styleSheets)
                element.RemoveStyleSheet(styleSheet);

            return element;
        }

        /// <summary>
        /// Conditionally removes an enumerable of style sheets from <see cref="VisualElement.styleSheets"/>.
        /// </summary>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="condition">When <see langword="true"/>, the operation is performed; otherwise, the element is returned unchanged.</param>
        /// <param name="styleSheets">The style sheets to remove; <see langword="null"/> entries are skipped.</param>
        /// <returns>The element, for chaining.</returns>
        public static T RemoveStyleSheetsIf<T>(this T element, bool condition, IEnumerable<StyleSheet?>? styleSheets)
            where T : VisualElement => condition ? element.RemoveStyleSheets(styleSheets) : element;

        /// <summary>
        /// Removes a read-only span of style sheets from <see cref="VisualElement.styleSheets"/>.
        /// </summary>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="styleSheets">The style sheets to remove; <see langword="null"/> entries are skipped.</param>
        /// <returns>The element, for chaining.</returns>
        public static T RemoveStyleSheets<T>(this T element, ReadOnlySpan<StyleSheet> styleSheets)
            where T : VisualElement
        {
            foreach (var styleSheet in styleSheets)
                element.RemoveStyleSheet(styleSheet);

            return element;
        }

        /// <summary>
        /// Conditionally removes a read-only span of style sheets from <see cref="VisualElement.styleSheets"/>.
        /// </summary>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="condition">When <see langword="true"/>, the operation is performed; otherwise, the element is returned unchanged.</param>
        /// <param name="styleSheets">The style sheets to remove; <see langword="null"/> entries are skipped.</param>
        /// <returns>The element, for chaining.</returns>
        public static T RemoveStyleSheetsIf<T>(this T element, bool condition, ReadOnlySpan<StyleSheet> styleSheets)
            where T : VisualElement => condition ? element.RemoveStyleSheets(styleSheets) : element;

        /// <summary>
        /// Loads a <see cref="StyleSheet"/> from Resources and removes it from <see cref="VisualElement.styleSheets"/>.
        /// </summary>
        /// <remarks>
        /// Logs a warning and leaves the element unchanged when no asset is found at <paramref name="path"/>.
        /// </remarks>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="path">The Resources-relative path to the style sheet asset.</param>
        /// <returns>The element, for chaining.</returns>
        public static T RemoveStyleSheetFromResources<T>(this T element, string path)
            where T : VisualElement => element.RemoveStyleSheet(LoadStyleSheetFromResources(path));

        /// <summary>
        /// Conditionally loads a <see cref="StyleSheet"/> from Resources and removes it from <see cref="VisualElement.styleSheets"/>.
        /// </summary>
        /// <remarks>
        /// Logs a warning and leaves the element unchanged when no asset is found at <paramref name="path"/>.
        /// </remarks>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="condition">When <see langword="true"/>, the operation is performed; otherwise, the element is returned unchanged.</param>
        /// <param name="path">The Resources-relative path to the style sheet asset.</param>
        /// <returns>The element, for chaining.</returns>
        public static T RemoveStyleSheetFromResourcesIf<T>(this T element, bool condition, string path)
            where T : VisualElement => condition ? element.RemoveStyleSheetFromResources(path) : element;

        /// <summary>
        /// Loads style sheets from an array of Resources paths and removes them from <see cref="VisualElement.styleSheets"/>.
        /// </summary>
        /// <remarks>
        /// Logs a warning for each path where no asset is found and skips that path.
        /// </remarks>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="paths">The Resources-relative paths to the style sheet assets.</param>
        /// <returns>The element, for chaining.</returns>
        public static T RemoveStyleSheetsFromResources<T>(this T element, params string[]? paths)
            where T : VisualElement
        {
            if (paths is null) return element;

            foreach (var path in paths)
                element.RemoveStyleSheet(LoadStyleSheetFromResources(path));

            return element;
        }

        /// <summary>
        /// Conditionally loads style sheets from an array of Resources paths and removes them from <see cref="VisualElement.styleSheets"/>.
        /// </summary>
        /// <remarks>
        /// Logs a warning for each path where no asset is found and skips that path.
        /// </remarks>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="condition">When <see langword="true"/>, the operation is performed; otherwise, the element is returned unchanged.</param>
        /// <param name="paths">The Resources-relative paths to the style sheet assets.</param>
        /// <returns>The element, for chaining.</returns>
        public static T RemoveStyleSheetsFromResourcesIf<T>(this T element, bool condition, params string[]? paths)
            where T : VisualElement => condition ? element.RemoveStyleSheetsFromResources(paths) : element;

        /// <summary>
        /// Loads style sheets from a list of Resources paths and removes them from <see cref="VisualElement.styleSheets"/>.
        /// </summary>
        /// <remarks>
        /// Logs a warning for each path where no asset is found and skips that path.
        /// </remarks>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="paths">The Resources-relative paths to the style sheet assets.</param>
        /// <returns>The element, for chaining.</returns>
        public static T RemoveStyleSheetsFromResources<T>(this T element, List<string>? paths)
            where T : VisualElement
        {
            if (paths is null) return element;

            foreach (var path in paths)
                element.RemoveStyleSheet(LoadStyleSheetFromResources(path));

            return element;
        }

        /// <summary>
        /// Conditionally loads style sheets from a list of Resources paths and removes them from <see cref="VisualElement.styleSheets"/>.
        /// </summary>
        /// <remarks>
        /// Logs a warning for each path where no asset is found and skips that path.
        /// </remarks>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="condition">When <see langword="true"/>, the operation is performed; otherwise, the element is returned unchanged.</param>
        /// <param name="paths">The Resources-relative paths to the style sheet assets.</param>
        /// <returns>The element, for chaining.</returns>
        public static T RemoveStyleSheetsFromResourcesIf<T>(this T element, bool condition, List<string>? paths)
            where T : VisualElement => condition ? element.RemoveStyleSheetsFromResources(paths) : element;

        /// <summary>
        /// Loads style sheets from an enumerable of Resources paths and removes them from <see cref="VisualElement.styleSheets"/>.
        /// </summary>
        /// <remarks>
        /// Logs a warning for each path where no asset is found and skips that path.
        /// </remarks>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="paths">The Resources-relative paths to the style sheet assets.</param>
        /// <returns>The element, for chaining.</returns>
        public static T RemoveStyleSheetsFromResources<T>(this T element, IEnumerable<string>? paths)
            where T : VisualElement
        {
            if (paths is null) return element;

            foreach (var path in paths)
                element.RemoveStyleSheet(LoadStyleSheetFromResources(path));

            return element;
        }

        /// <summary>
        /// Conditionally loads style sheets from an enumerable of Resources paths and removes them from <see cref="VisualElement.styleSheets"/>.
        /// </summary>
        /// <remarks>
        /// Logs a warning for each path where no asset is found and skips that path.
        /// </remarks>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="condition">When <see langword="true"/>, the operation is performed; otherwise, the element is returned unchanged.</param>
        /// <param name="paths">The Resources-relative paths to the style sheet assets.</param>
        /// <returns>The element, for chaining.</returns>
        public static T RemoveStyleSheetsFromResourcesIf<T>(this T element, bool condition, IEnumerable<string>? paths)
            where T : VisualElement => condition ? element.RemoveStyleSheetsFromResources(paths) : element;

        /// <summary>
        /// Loads style sheets from a read-only span of Resources paths and removes them from <see cref="VisualElement.styleSheets"/>.
        /// </summary>
        /// <remarks>
        /// Logs a warning for each path where no asset is found and skips that path.
        /// </remarks>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="paths">The Resources-relative paths to the style sheet assets.</param>
        /// <returns>The element, for chaining.</returns>
        public static T RemoveStyleSheetsFromResources<T>(this T element, ReadOnlySpan<string> paths)
            where T : VisualElement
        {
            foreach (var path in paths)
                element.RemoveStyleSheet(LoadStyleSheetFromResources(path));

            return element;
        }

        /// <summary>
        /// Conditionally loads style sheets from a read-only span of Resources paths and removes them from <see cref="VisualElement.styleSheets"/>.
        /// </summary>
        /// <remarks>
        /// Logs a warning for each path where no asset is found and skips that path.
        /// </remarks>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="condition">When <see langword="true"/>, the operation is performed; otherwise, the element is returned unchanged.</param>
        /// <param name="paths">The Resources-relative paths to the style sheet assets.</param>
        /// <returns>The element, for chaining.</returns>
        public static T RemoveStyleSheetsFromResourcesIf<T>(this T element, bool condition, ReadOnlySpan<string> paths)
            where T : VisualElement => condition ? element.RemoveStyleSheetsFromResources(paths) : element;
        #endregion

        #region EnableStyleSheets
        /// <summary>
        /// Adds a style sheet to <see cref="VisualElement.styleSheets"/> or removes it, depending on <paramref name="enable"/>.
        /// </summary>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="styleSheet">The style sheet to add or remove; <see langword="null"/> leaves the element unchanged.</param>
        /// <param name="enable">When <see langword="true"/>, the style sheet is added; otherwise, it is removed.</param>
        /// <returns>The element, for chaining.</returns>
        public static T EnableStyleSheet<T>(this T element, StyleSheet? styleSheet, bool enable)
            where T : VisualElement => enable ? element.AddStyleSheet(styleSheet) : element.RemoveStyleSheet(styleSheet);

        /// <summary>
        /// Conditionally adds a style sheet to <see cref="VisualElement.styleSheets"/> or removes it, depending on <paramref name="enable"/>.
        /// </summary>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="condition">When <see langword="true"/>, the operation is performed; otherwise, the element is returned unchanged.</param>
        /// <param name="styleSheet">The style sheet to add or remove; <see langword="null"/> leaves the element unchanged.</param>
        /// <param name="enable">When <see langword="true"/>, the style sheet is added; otherwise, it is removed.</param>
        /// <returns>The element, for chaining.</returns>
        public static T EnableStyleSheetIf<T>(this T element, bool condition, StyleSheet? styleSheet, bool enable)
            where T : VisualElement => condition ? element.EnableStyleSheet(styleSheet, enable) : element;

        /// <summary>
        /// Adds an array of style sheets to <see cref="VisualElement.styleSheets"/> or removes them, depending on <paramref name="enable"/>.
        /// </summary>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="enable">When <see langword="true"/>, the style sheets are added; otherwise, they are removed.</param>
        /// <param name="styleSheets">The style sheets to add or remove; <see langword="null"/> entries are skipped.</param>
        /// <returns>The element, for chaining.</returns>
        public static T EnableStyleSheets<T>(this T element, bool enable, params StyleSheet?[]? styleSheets)
            where T : VisualElement
        {
            if (styleSheets is null) return element;

            foreach (var styleSheet in styleSheets)
                element.EnableStyleSheet(styleSheet, enable);

            return element;
        }

        /// <summary>
        /// Conditionally adds an array of style sheets to <see cref="VisualElement.styleSheets"/> or removes them, depending on <paramref name="enable"/>.
        /// </summary>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="condition">When <see langword="true"/>, the operation is performed; otherwise, the element is returned unchanged.</param>
        /// <param name="enable">When <see langword="true"/>, the style sheets are added; otherwise, they are removed.</param>
        /// <param name="styleSheets">The style sheets to add or remove; <see langword="null"/> entries are skipped.</param>
        /// <returns>The element, for chaining.</returns>
        public static T EnableStyleSheetsIf<T>(this T element, bool condition, bool enable, params StyleSheet?[]? styleSheets)
            where T : VisualElement => condition ? element.EnableStyleSheets(enable, styleSheets) : element;

        /// <summary>
        /// Adds a list of style sheets to <see cref="VisualElement.styleSheets"/> or removes them, depending on <paramref name="enable"/>.
        /// </summary>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="enable">When <see langword="true"/>, the style sheets are added; otherwise, they are removed.</param>
        /// <param name="styleSheets">The style sheets to add or remove; <see langword="null"/> entries are skipped.</param>
        /// <returns>The element, for chaining.</returns>
        public static T EnableStyleSheets<T>(this T element, bool enable, List<StyleSheet>? styleSheets)
            where T : VisualElement
        {
            if (styleSheets is null) return element;

            foreach (var styleSheet in styleSheets)
                element.EnableStyleSheet(styleSheet, enable);

            return element;
        }

        /// <summary>
        /// Conditionally adds a list of style sheets to <see cref="VisualElement.styleSheets"/> or removes them, depending on <paramref name="enable"/>.
        /// </summary>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="condition">When <see langword="true"/>, the operation is performed; otherwise, the element is returned unchanged.</param>
        /// <param name="enable">When <see langword="true"/>, the style sheets are added; otherwise, they are removed.</param>
        /// <param name="styleSheets">The style sheets to add or remove; <see langword="null"/> entries are skipped.</param>
        /// <returns>The element, for chaining.</returns>
        public static T EnableStyleSheetsIf<T>(this T element, bool condition, bool enable, List<StyleSheet>? styleSheets)
            where T : VisualElement => condition ? element.EnableStyleSheets(enable, styleSheets) : element;

        /// <summary>
        /// Adds an enumerable of style sheets to <see cref="VisualElement.styleSheets"/> or removes them, depending on <paramref name="enable"/>.
        /// </summary>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="enable">When <see langword="true"/>, the style sheets are added; otherwise, they are removed.</param>
        /// <param name="styleSheets">The style sheets to add or remove; <see langword="null"/> entries are skipped.</param>
        /// <returns>The element, for chaining.</returns>
        public static T EnableStyleSheets<T>(this T element, bool enable, IEnumerable<StyleSheet?>? styleSheets)
            where T : VisualElement
        {
            if (styleSheets is null) return element;

            foreach (var styleSheet in styleSheets)
                element.EnableStyleSheet(styleSheet, enable);

            return element;
        }

        /// <summary>
        /// Conditionally adds an enumerable of style sheets to <see cref="VisualElement.styleSheets"/> or removes them, depending on <paramref name="enable"/>.
        /// </summary>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="condition">When <see langword="true"/>, the operation is performed; otherwise, the element is returned unchanged.</param>
        /// <param name="enable">When <see langword="true"/>, the style sheets are added; otherwise, they are removed.</param>
        /// <param name="styleSheets">The style sheets to add or remove; <see langword="null"/> entries are skipped.</param>
        /// <returns>The element, for chaining.</returns>
        public static T EnableStyleSheetsIf<T>(this T element, bool condition, bool enable, IEnumerable<StyleSheet?>? styleSheets)
            where T : VisualElement => condition ? element.EnableStyleSheets(enable, styleSheets) : element;

        /// <summary>
        /// Adds a read-only span of style sheets to <see cref="VisualElement.styleSheets"/> or removes them, depending on <paramref name="enable"/>.
        /// </summary>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="enable">When <see langword="true"/>, the style sheets are added; otherwise, they are removed.</param>
        /// <param name="styleSheets">The style sheets to add or remove; <see langword="null"/> entries are skipped.</param>
        /// <returns>The element, for chaining.</returns>
        public static T EnableStyleSheets<T>(this T element, bool enable, ReadOnlySpan<StyleSheet> styleSheets)
            where T : VisualElement
        {
            foreach (var styleSheet in styleSheets)
                element.EnableStyleSheet(styleSheet, enable);

            return element;
        }

        /// <summary>
        /// Conditionally adds a read-only span of style sheets to <see cref="VisualElement.styleSheets"/> or removes them, depending on <paramref name="enable"/>.
        /// </summary>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="condition">When <see langword="true"/>, the operation is performed; otherwise, the element is returned unchanged.</param>
        /// <param name="enable">When <see langword="true"/>, the style sheets are added; otherwise, they are removed.</param>
        /// <param name="styleSheets">The style sheets to add or remove; <see langword="null"/> entries are skipped.</param>
        /// <returns>The element, for chaining.</returns>
        public static T EnableStyleSheetsIf<T>(this T element, bool condition, bool enable, ReadOnlySpan<StyleSheet> styleSheets)
            where T : VisualElement => condition ? element.EnableStyleSheets(enable, styleSheets) : element;

        /// <summary>
        /// Loads a <see cref="StyleSheet"/> from Resources and adds it to <see cref="VisualElement.styleSheets"/> or removes it, depending on <paramref name="enable"/>.
        /// </summary>
        /// <remarks>
        /// Logs a warning and leaves the element unchanged when no asset is found at <paramref name="path"/>.
        /// </remarks>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="path">The Resources-relative path to the style sheet asset.</param>
        /// <param name="enable">When <see langword="true"/>, the style sheet is added; otherwise, it is removed.</param>
        /// <returns>The element, for chaining.</returns>
        public static T EnableStyleSheetFromResources<T>(this T element, string path, bool enable)
            where T : VisualElement => element.EnableStyleSheet(LoadStyleSheetFromResources(path), enable);

        /// <summary>
        /// Conditionally loads a <see cref="StyleSheet"/> from Resources and adds it to <see cref="VisualElement.styleSheets"/> or removes it, depending on <paramref name="enable"/>.
        /// </summary>
        /// <remarks>
        /// Logs a warning and leaves the element unchanged when no asset is found at <paramref name="path"/>.
        /// </remarks>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="condition">When <see langword="true"/>, the operation is performed; otherwise, the element is returned unchanged.</param>
        /// <param name="path">The Resources-relative path to the style sheet asset.</param>
        /// <param name="enable">When <see langword="true"/>, the style sheet is added; otherwise, it is removed.</param>
        /// <returns>The element, for chaining.</returns>
        public static T EnableStyleSheetFromResourcesIf<T>(this T element, bool condition, string path, bool enable)
            where T : VisualElement => condition ? element.EnableStyleSheetFromResources(path, enable) : element;

        /// <summary>
        /// Loads style sheets from an array of Resources paths and adds them to <see cref="VisualElement.styleSheets"/> or removes them, depending on <paramref name="enable"/>.
        /// </summary>
        /// <remarks>
        /// Logs a warning for each path where no asset is found and skips that path.
        /// </remarks>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="enable">When <see langword="true"/>, the style sheets are added; otherwise, they are removed.</param>
        /// <param name="paths">The Resources-relative paths to the style sheet assets.</param>
        /// <returns>The element, for chaining.</returns>
        public static T EnableStyleSheetsFromResources<T>(this T element, bool enable, params string[]? paths)
            where T : VisualElement
        {
            if (paths is null) return element;

            foreach (var path in paths)
                element.EnableStyleSheet(LoadStyleSheetFromResources(path), enable);

            return element;
        }

        /// <summary>
        /// Conditionally loads style sheets from an array of Resources paths and adds them to <see cref="VisualElement.styleSheets"/> or removes them, depending on <paramref name="enable"/>.
        /// </summary>
        /// <remarks>
        /// Logs a warning for each path where no asset is found and skips that path.
        /// </remarks>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="condition">When <see langword="true"/>, the operation is performed; otherwise, the element is returned unchanged.</param>
        /// <param name="enable">When <see langword="true"/>, the style sheets are added; otherwise, they are removed.</param>
        /// <param name="paths">The Resources-relative paths to the style sheet assets.</param>
        /// <returns>The element, for chaining.</returns>
        public static T EnableStyleSheetsFromResourcesIf<T>(this T element, bool condition, bool enable, params string[]? paths)
            where T : VisualElement => condition ? element.EnableStyleSheetsFromResources(enable, paths) : element;

        /// <summary>
        /// Loads style sheets from a list of Resources paths and adds them to <see cref="VisualElement.styleSheets"/> or removes them, depending on <paramref name="enable"/>.
        /// </summary>
        /// <remarks>
        /// Logs a warning for each path where no asset is found and skips that path.
        /// </remarks>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="enable">When <see langword="true"/>, the style sheets are added; otherwise, they are removed.</param>
        /// <param name="paths">The Resources-relative paths to the style sheet assets.</param>
        /// <returns>The element, for chaining.</returns>
        public static T EnableStyleSheetsFromResources<T>(this T element, bool enable, List<string>? paths)
            where T : VisualElement
        {
            if (paths is null) return element;

            foreach (var path in paths)
                element.EnableStyleSheet(LoadStyleSheetFromResources(path), enable);

            return element;
        }

        /// <summary>
        /// Conditionally loads style sheets from a list of Resources paths and adds them to <see cref="VisualElement.styleSheets"/> or removes them, depending on <paramref name="enable"/>.
        /// </summary>
        /// <remarks>
        /// Logs a warning for each path where no asset is found and skips that path.
        /// </remarks>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="condition">When <see langword="true"/>, the operation is performed; otherwise, the element is returned unchanged.</param>
        /// <param name="enable">When <see langword="true"/>, the style sheets are added; otherwise, they are removed.</param>
        /// <param name="paths">The Resources-relative paths to the style sheet assets.</param>
        /// <returns>The element, for chaining.</returns>
        public static T EnableStyleSheetsFromResourcesIf<T>(this T element, bool condition, bool enable, List<string>? paths)
            where T : VisualElement => condition ? element.EnableStyleSheetsFromResources(enable, paths) : element;

        /// <summary>
        /// Loads style sheets from an enumerable of Resources paths and adds them to <see cref="VisualElement.styleSheets"/> or removes them, depending on <paramref name="enable"/>.
        /// </summary>
        /// <remarks>
        /// Logs a warning for each path where no asset is found and skips that path.
        /// </remarks>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="enable">When <see langword="true"/>, the style sheets are added; otherwise, they are removed.</param>
        /// <param name="paths">The Resources-relative paths to the style sheet assets.</param>
        /// <returns>The element, for chaining.</returns>
        public static T EnableStyleSheetsFromResources<T>(this T element, bool enable, IEnumerable<string>? paths)
            where T : VisualElement
        {
            if (paths is null) return element;

            foreach (var path in paths)
                element.EnableStyleSheet(LoadStyleSheetFromResources(path), enable);

            return element;
        }

        /// <summary>
        /// Conditionally loads style sheets from an enumerable of Resources paths and adds them to <see cref="VisualElement.styleSheets"/> or removes them, depending on <paramref name="enable"/>.
        /// </summary>
        /// <remarks>
        /// Logs a warning for each path where no asset is found and skips that path.
        /// </remarks>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="condition">When <see langword="true"/>, the operation is performed; otherwise, the element is returned unchanged.</param>
        /// <param name="enable">When <see langword="true"/>, the style sheets are added; otherwise, they are removed.</param>
        /// <param name="paths">The Resources-relative paths to the style sheet assets.</param>
        /// <returns>The element, for chaining.</returns>
        public static T EnableStyleSheetsFromResourcesIf<T>(this T element, bool condition, bool enable, IEnumerable<string>? paths)
            where T : VisualElement => condition ? element.EnableStyleSheetsFromResources(enable, paths) : element;

        /// <summary>
        /// Loads style sheets from a read-only span of Resources paths and adds them to <see cref="VisualElement.styleSheets"/> or removes them, depending on <paramref name="enable"/>.
        /// </summary>
        /// <remarks>
        /// Logs a warning for each path where no asset is found and skips that path.
        /// </remarks>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="enable">When <see langword="true"/>, the style sheets are added; otherwise, they are removed.</param>
        /// <param name="paths">The Resources-relative paths to the style sheet assets.</param>
        /// <returns>The element, for chaining.</returns>
        public static T EnableStyleSheetsFromResources<T>(this T element, bool enable, ReadOnlySpan<string> paths)
            where T : VisualElement
        {
            foreach (var path in paths)
                element.EnableStyleSheet(LoadStyleSheetFromResources(path), enable);

            return element;
        }

        /// <summary>
        /// Conditionally loads style sheets from a read-only span of Resources paths and adds them to <see cref="VisualElement.styleSheets"/> or removes them, depending on <paramref name="enable"/>.
        /// </summary>
        /// <remarks>
        /// Logs a warning for each path where no asset is found and skips that path.
        /// </remarks>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="condition">When <see langword="true"/>, the operation is performed; otherwise, the element is returned unchanged.</param>
        /// <param name="enable">When <see langword="true"/>, the style sheets are added; otherwise, they are removed.</param>
        /// <param name="paths">The Resources-relative paths to the style sheet assets.</param>
        /// <returns>The element, for chaining.</returns>
        public static T EnableStyleSheetsFromResourcesIf<T>(this T element, bool condition, bool enable, ReadOnlySpan<string> paths)
            where T : VisualElement => condition ? element.EnableStyleSheetsFromResources(enable, paths) : element;
        #endregion

        #region ClearStyleSheets
        /// <summary>
        /// Removes all style sheets from <see cref="VisualElement.styleSheets"/>.
        /// </summary>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <returns>The element, for chaining.</returns>
        public static T ClearStyleSheets<T>(this T element)
            where T : VisualElement
        {
            element.styleSheets.Clear();
            return element;
        }

        /// <summary>
        /// Conditionally removes all style sheets from <see cref="VisualElement.styleSheets"/>.
        /// </summary>
        /// <typeparam name="T">The element type.</typeparam>
        /// <param name="element">The element to modify.</param>
        /// <param name="condition">When <see langword="true"/>, the operation is performed; otherwise, the element is returned unchanged.</param>
        /// <returns>The element, for chaining.</returns>
        public static T ClearStyleSheetsIf<T>(this T element, bool condition)
            where T : VisualElement => condition ? element.ClearStyleSheets() : element;
        #endregion

        private static int InsertStyleSheetAt(VisualElement element, int index, StyleSheet? styleSheet)
        {
            if (styleSheet == null) return index;

            var count = element.styleSheets.count;
            element.styleSheets.Insert(index, styleSheet);
            return element.styleSheets.count > count ? index + 1 : index;
        }

        private static StyleSheet? LoadStyleSheetFromResources(string path)
        {
            var styleSheet = Resources.Load<StyleSheet>(path);
            if (styleSheet == null)
                Debug.LogWarning($"Failed to load StyleSheet from Resources path: '{path}'");

            return styleSheet;
        }
    }
}
