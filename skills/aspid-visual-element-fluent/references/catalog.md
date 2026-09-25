# Aspid.FastTools UI Toolkit extension catalog

Namespace `Aspid.FastTools.UIElements` unless marked **Editor** (`Aspid.FastTools.UIElements.Editors`,
assembly `Aspid.FastTools.Editor`). Every method returns the receiver (`T`) for chaining unless noted.
The receiver constraint is given per section. Version-guarded members are marked *(6000.x+)*.

## VisualElement — identity and state (`where T : VisualElement`)

| Method | Parameter |
|---|---|
| `SetName` | `string` |
| `SetTooltip` | `string` |
| `SetVisible` | `bool` |
| `SetEnabledSelf` | `bool` (calls `SetEnabled`) |
| `SetUserData` | `object` |
| `SetViewDataKey` | `string` |
| `SetPickingMode` | `PickingMode` |
| `SetUsageHints` | `UsageHints` |
| `SetDisablePlayModeTint` | `bool` |
| `SetLanguageDirection` | `LanguageDirection` |
| `SetDataSource` | `object` |
| `SetDataSourceType` | `Type` |
| `SetDataSourcePath` | `PropertyPath` (`using Unity.Properties;`) |

## Children (`where T : VisualElement`, return the parent)

| Method | Parameters |
|---|---|
| `AddChild` | `VisualElement child` |
| `AddChildren` | `params VisualElement[]`, `IEnumerable<VisualElement>`, `List<VisualElement>`, `Span<VisualElement>`, `ReadOnlySpan<VisualElement>` |
| `InsertChild` | `int index, VisualElement child` |
| `InsertChildren` | `int index` + the same collection overloads as `AddChildren` |
| `RemoveChild` | `VisualElement child` |
| `RemoveChildAt` | `int index` |
| `ClearChildren` | — |

Every method has an `…If(bool condition, …)` variant: `AddChildIf`, `AddChildrenIf`, `InsertChildIf`,
`InsertChildrenIf`, `RemoveChildIf`, `RemoveChildAtIf`, `ClearChildrenIf`. Collection overloads keep
order and ignore a `null` collection.

## USS classes and style sheets (`where T : VisualElement`)

`AddClass(string)`, `RemoveClass(string)`, `ToggleClass(string)`, `EnableClass(string className, bool enable)`,
`ClearClasses()`, `AddStyleSheet(StyleSheet)`, `RemoveStyleSheet(StyleSheet)`,
`AddStyleSheetFromResources(string path)`, `RemoveStyleSheetFromResources(string path)`.
The `Resources` variants log a warning and do nothing when the asset is missing.

## Styles

Each method exists on the element (`where T : VisualElement`, returns the element) and on the style
(`where T : IStyle`, e.g. `element.style.SetMargin(4)`, returns the style). Enum-valued properties
accept both the enum and `StyleEnum<TEnum>`. Colour setters marked "+ string" also take an HTML
colour string (`"#FFC24D"`, `"red"`); an unparsable string logs a warning and changes nothing.

### Layout

| Method | Value |
|---|---|
| `SetFlexDirection`, `SetFlexWrap` | `FlexDirection`, `Wrap` |
| `SetFlexGrow`, `SetFlexShrink` | `StyleFloat` |
| `SetFlexBasis` | `StyleLength` |
| `SetAlignItems`, `SetAlignSelf`, `SetAlignContent` | `Align` |
| `SetJustifyContent` | `Justify` |
| `SetPosition` | `Position` |
| `SetDistance` | `StyleLength` for all four; or named `top`, `right`, `bottom`, `left` (`StyleLength?`, omitted = unchanged) |
| `SetDistanceX`, `SetDistanceY` | `StyleLength` (left+right, top+bottom) |
| `SetTop`, `SetRight`, `SetBottom`, `SetLeft` | `StyleLength` |
| `SetDisplay`, `SetVisibility`, `SetOverflow` | `DisplayStyle`, `Visibility`, `Overflow` |
| `SetUnityOverflowClipBox` | `OverflowClipBox` |
| `SetAspectRatio` *(6000.3+)* | `StyleRatio` |

### Size

| Method | Value |
|---|---|
| `SetWidth`, `SetHeight`, `SetMinWidth`, `SetMinHeight`, `SetMaxWidth`, `SetMaxHeight` | `StyleLength` |
| `SetSize` | one `StyleLength` for both, or `(StyleLength? width = null, StyleLength? height = null)` |
| `SetMinSize` | one value, or `(minWidth, minHeight)` |
| `SetMaxSize` | one value, or `(maxWidth, maxHeight)` |

### Spacing and borders

| Method | Value | Per-side variants |
|---|---|---|
| `SetMargin`, `SetPadding` | `StyleLength`, or named `top/right/bottom/left` | `X`, `Y`, `Top`, `Right`, `Bottom`, `Left` |
| `SetBorderWidth` | `StyleFloat`, or named `top/right/bottom/left` | `X`, `Y`, `Top`, `Right`, `Bottom`, `Left` |
| `SetBorderColor` | `StyleColor` + string, or named `top/right/bottom/left` (`StyleColor?`) | `X`, `Y`, `Top`, `Right`, `Bottom`, `Left` (each + string) |
| `SetBorderRadius` | `StyleLength`, or named `topLeft/topRight/bottomRight/bottomLeft` | `Top`, `Bottom`, `Left`, `Right`, `TopLeft`, `TopRight`, `BottomRight`, `BottomLeft` |
| `SetUnitySlice` | `StyleInt`, or named `top/right/bottom/left` | `X`, `Y`, `Top`, `Right`, `Bottom`, `Left` |
| `SetUnitySliceScale` | `StyleFloat` | — |
| `SetUnitySliceType` | `SliceType` | — |

`X` = left and right, `Y` = top and bottom.

### Colour, background, effects

| Method | Value |
|---|---|
| `SetColor` | `StyleColor` + string |
| `SetBackgroundColor` | `StyleColor` + string |
| `SetBackgroundImage` | `StyleBackground` only (`new StyleBackground(texture / sprite / vectorImage)`) |
| `SetBackgroundImageFromResources` | `string path` (loads a `Texture2D`) |
| `SetUnityBackgroundImageTintColor` | `StyleColor` + string |
| `SetBackgroundSize` | `StyleBackgroundSize` |
| `SetBackgroundRepeat` | `StyleBackgroundRepeat` |
| `SetBackgroundPosition` | `StyleBackgroundPosition`, or named `x`, `y` |
| `SetBackgroundPositionX`, `SetBackgroundPositionY` | `StyleBackgroundPosition` |
| `SetOpacity` | `StyleFloat` |
| `SetCursor` | `StyleCursor` |
| `SetFilter` *(6000.3+)* | `StyleList<FilterFunction>` |
| `SetUnityMaterial` *(6000.3+)* | `StyleMaterialDefinition` |

### Transform and transitions

`SetRotate(StyleRotate)`, `SetScale(StyleScale)`, `SetTranslate(StyleTranslate)`,
`SetTransformOrigin(StyleTransformOrigin)`.
`SetTransitionProperty(StyleList<StylePropertyName>)`, `SetTransitionDuration(StyleList<TimeValue>)`,
`SetTransitionDelay(StyleList<TimeValue>)`, `SetTransitionTimingFunction(StyleList<EasingFunction>)` —
`StyleList<T>` converts implicitly from `List<T>`, not from arrays.

### Text

| Method | Value |
|---|---|
| `SetFontSize`, `SetLetterSpacing`, `SetWordSpacing`, `SetUnityParagraphSpacing` | `StyleLength` |
| `SetUnityFont` | `StyleFont` |
| `SetUnityFontDefinition` | `StyleFontDefinition` |
| `SetUnityFontStyleAndWeight` | `FontStyle` |
| `SetUnityTextAlign` | `TextAnchor` |
| `SetTextOverflow`, `SetUnityTextOverflowPosition` | `TextOverflow`, `TextOverflowPosition` |
| `SetWhiteSpace` | `WhiteSpace` |
| `SetTextShadow` | `StyleTextShadow` |
| `SetUnityTextOutlineColor` | `StyleColor` + string |
| `SetUnityTextOutlineWidth` | `StyleFloat` |
| `SetUnityTextGenerator` | `TextGeneratorType` |
| `SetUnityEditorTextRenderingMode` | `EditorTextRenderingMode` |
| `SetUnityTextAutoSize` *(6000.2+)* | `StyleTextAutoSize` |

### Font style presets (element and `IStyle`)

`SetNormalUnityFontStyleAndWeight()`, `AddBoldUnityFontStyleAndWeight()`,
`RemoveBoldUnityFontStyleAndWeight()`, `AddItalicUnityFontStyleAndWeight()`,
`RemoveItalicUnityFontStyleAndWeight()`. Add/Remove flip one flag and keep the other; they read the
inline `style` value, not the resolved one. These are the only presets.

## Values and change events

`where T : INotifyValueChanged<TValue>`:

- `SetValue(TValue value, bool notify = true)` — `notify: false` calls `SetValueWithoutNotify`.
- `AddValueChanged(EventCallback<ChangeEvent<TValue>>)`, `RemoveValueChanged(…)`.

Typed overloads (no type arguments needed): `int`, `uint`, `nint`, `nuint`, `long`, `ulong`, `short`,
`ushort`, `byte`, `sbyte`, `float`, `double`, `decimal`, `bool`, `char`, `string`, `object`,
`UnityEngine.Object`, `Enum`, `Delegate`, `Color`, `Rect`, `RectInt`, `Bounds`, `BoundsInt`,
`Hash128`, `Vector2`, `Vector2Int`, `Vector3`, `Vector3Int`, `Vector4`, `Quaternion`, `Matrix4x4`,
`Gradient`, `AnimationCurve`, `GUID` *(6000.4+)*.
Any other type: `SetValue<TField, TValue>` (infers from the argument),
`AddValueChanged<TField, TValue>` / `RemoveValueChanged<TField, TValue>` (explicit type arguments).

`Unity.Mathematics` overloads (class `INotifyValueChangedMathExtensions`, assembly
`Aspid.FastTools.VisualElements.Math`, only with `com.unity.mathematics`): `SetValue`,
`AddValueChanged`, `RemoveValueChanged` for `int2..int4x4`, `uint2..uint4x4`, `float2..float4x4`,
`double2..double4x4`, `bool2..bool4x4`, `half`, `half2..half4`, `quaternion`.

`IMixedValueSupport`: `SetShowMixedValue(bool)`.

## Fields

| Receiver | Methods |
|---|---|
| `BaseField<X>` for `int`, `uint`, `long`, `ulong`, `short`, `ushort`, `byte`, `sbyte`, `float`, `double`, `decimal`, `char`, `string`, `Enum`, `Object`, `Color`, `Color32`, `Rect`, `Bounds`, `BoundsInt`, `Hash128`, `Vector2`, `Vector2Int`, `Vector3`, `Vector3Int`, `Vector4`, `Quaternion`, `Gradient`, `AnimationCurve` | `SetLabel(string)` |
| any other `BaseField<TValue>` | `SetLabel<TField, TValue>(string)` |
| `BaseBoolField` (`Toggle`) | `SetLabel`, `SetText`, `SetToggleOnLabelClick(bool)` |
| `EnumField` | `Initialize(Enum defaultValue, bool includeObsoleteValues = false)` |
| `TextInputBaseField<X>` for `string`, `int`, `uint`, `long`, `ulong`, `float`, `double`, `Hash128` (`TextField`, `IntegerField`, `FloatField`…) | `SetPlaceholder(string)`, `SetHidePlaceholderOnFocus(bool)`, `SetMaxLength(int)`, `SetMaskChar(char)`, `SetPassword(bool)`, `SetReadOnly(bool)`, `SetDelayed(bool)`, `SetAutoCorrection(bool)`, `SetHideMobileInput(bool)`, `SetKeyboardType(TouchScreenKeyboardType)`, `SetHideSoftKeyboard(bool)` *(6000.4+)*; selection: `SetSelectable`, `SetSelectAllOnFocus`, `SetSelectAllOnMouseUp`, `SetDoubleClickSelectsWord`, `SetTripleClickSelectsLine` (`bool`), `SetCursorIndex`, `SetSelectIndex` (`int`), `Add/RemoveOnCursorIndexChange`, `Add/RemoveOnSelectIndexChange` (`Action`, *6000.3+*) |
| any other `TextInputBaseField<TValue>` | the same methods as `Method<TField, TValue>(…)` |

## Text elements

| Receiver | Methods |
|---|---|
| `TextElement` (`Label`, `Button`…) | `SetText(string)`, `SetEnableRichText`, `SetEmojiFallbackSupport`, `SetParseEscapeSequences`, `SetDisplayTooltipWhenElided` (`bool`) |
| `ITextEdition` (e.g. `Label`, or `field.textEdition` — returns the interface) | the `TextInputBaseField` edition setters above |
| `ITextSelection` (e.g. `Label`, or `field.textSelection` — returns the interface) | the selection setters above |

## Controls

| Receiver | Methods |
|---|---|
| `Button` | `AddClicked(Action)`, `RemoveClicked(Action)`, `SetClickable(Clickable)`, `SetClickable(Action)` (replaces `clickable` with `new Clickable(action)`), `SetIconImage(Background)` |
| `Foldout` | `SetText(string)`, `SetToggleOnLabelClick(bool)` |
| `HelpBox` | `SetText(string)`, `SetMessageType(HelpBoxMessageType)` |
| `Image` | `SetImage(Texture)`, `SetSprite(Sprite)`, `SetVectorImage(VectorImage)`, `SetImageFromResources` / `SetSpriteFromResources` / `SetVectorImageFromResources` (`string path`), `SetTintColor(Color)`, `SetScaleMode(ScaleMode)`, `SetUv(Rect)`, `SetSourceRect(Rect)` |
| `AbstractProgressBar` | `SetTitle(string)`, `SetLowValue`, `SetHighValue`, `SetValue` (`float`) |
| `BaseSlider<float>` / `BaseSlider<int>` | `SetLowValue`, `SetHighValue`, `SetPageSize(float)`, `SetDirection(SliderDirection)`, `SetInverted(bool)`, `SetShowInputField(bool)`, `SetFill(bool)` |
| `BaseSlider<X>` for `double`, `long`, `ulong`, `uint`, `short`, `ushort`, `byte`, `sbyte` | `SetLowValue`, `SetHighValue` |
| any other `BaseSlider<TValue>` | `SetLowValue<T, TValue>`, `SetHighValue<T, TValue>` |
| `IMGUIContainer` | `SetOnGUIHandler(Action)`, `AddOnGUIHandler(Action)`, `RemoveOnGUIHandler(Action)`, `SetCullingEnabled(bool)`, `SetContextType(ContextType)`, `MarkDirtyLayoutSelf()` |

## Focus (`where T : Focusable`)

`FocusSelf()`, `BlurSelf()`, `SetFocusable(bool)`, `SetTabIndex(int)`, `SetDelegatesFocus(bool)`;
`IsFocused()` returns `bool`.

## Manipulators (`where T : VisualElement`)

| Method | Overloads |
|---|---|
| `AddManipulatorSelf`, `RemoveManipulatorSelf` | `IManipulator` |
| `AddClickable` | `Action`; `Action<EventBase>`; `Action, long delay, long interval`; each also with `out Clickable` |
| `AddContextualMenuManipulator` | `Action<ContextualMenuPopulateEvent>`, optional `out ContextualMenuManipulator` |
| `AddKeyboardNavigationManipulator` | `Action<KeyboardNavigationOperation, EventBase>`, optional `out KeyboardNavigationManipulator` |

Keep the `out` manipulator to remove it later with `RemoveManipulatorSelf`.

## Collection views

| Receiver | Methods |
|---|---|
| `BaseVerticalCollectionView` | `SetItemsSource(IList)`, `SetFixedItemHeight(float)`, `SetSelectionType(SelectionType)`, `SetSelectedIndex(int)`, `SetReorderable(bool)`, `SetHorizontalScrollingEnabled(bool)`, `SetVirtualizationMethod(CollectionVirtualizationMethod)`, `SetShowAlternatingRowBackgrounds(AlternatingRowBackground)`; events `Add/Remove` + `SelectionChanged`, `SelectedIndicesChanged`, `ItemsChosen`, `ItemIndexChanged`, `ItemsSourceChanged`, `CanStartDrag`, `SetupDragAndDrop`, `DragAndDropUpdate`, `HandleDrop` |
| `BaseListView` | `SetHeaderTitle(string)`, `SetShowFoldoutHeader`, `SetShowAddRemoveFooter`, `SetShowBoundCollectionSize`, `SetAllowAdd`, `SetAllowRemove` (`bool`), `SetReorderMode(ListViewReorderMode)`, `SetBindingSourceSelectionMode`, `SetMakeHeader` / `SetMakeFooter` / `SetMakeNoneElement` (`Func<VisualElement>`), `SetOnAdd` / `AddOnAdd` / `RemoveOnAdd`, `SetOnRemove` / `AddOnRemove` / `RemoveOnRemove` (`Action<BaseListView>`), `SetOverridingAddButtonBehavior` / `Add…` / `Remove…` (`Action<BaseListView, Button>`), `Add/RemoveItemsAdded`, `Add/RemoveItemsRemoved` (`Action<IEnumerable<int>>`) |
| `ListView`, `TreeView` | `SetMakeItem(Func<VisualElement>)`, `SetBindItem` / `AddBindItem` / `RemoveBindItem`, `SetUnbindItem` / `AddUnbindItem` / `RemoveUnbindItem` (`Action<VisualElement, int>`), `SetDestroyItem` / `AddDestroyItem` / `RemoveDestroyItem` (`Action<VisualElement>`), `SetItemTemplate(VisualTreeAsset)` |
| `BaseTreeView` | `SetAutoExpand(bool)`, `Add/RemoveItemExpandedChanged(Action<TreeViewExpansionChangedArgs>)` |
| `MultiColumnListView`, `MultiColumnTreeView` | `SetSortingMode(ColumnSortingMode)`, `Add/RemoveColumnSortingChanged(Action)` |

## Custom USS properties

`ICustomStyle.TryGetByEnum<TEnum>(CustomStyleProperty<string> property, out TEnum value)` — reads a
string property and parses it as an enum ignoring case; returns `bool`.

## Editor (`using Aspid.FastTools.UIElements.Editors;`)

| Method | Receiver | Notes |
|---|---|---|
| `BindTo(SerializedObject)` | `VisualElement` | `Bind` on the subtree |
| `UnbindFrom()` | `VisualElement` | `Unbind` |
| `BindTo(SerializedObject, string propertyPath)` | `VisualElement, IBindable` | sets `bindingPath`, then `Bind` |
| `BindPropertyTo(SerializedProperty)` | `VisualElement, IBindable` | `BindProperty` |
| `SetBindingPath(string)` | `VisualElement, IBindable` | path only |
| `SetLabel(string)` | `PropertyField` | |
| `AddValueChanged` / `RemoveValueChanged` | `PropertyField` | `EventCallback<SerializedPropertyChangeEvent>` |
| `Initialize(Enum, bool includeObsoleteValues = false)` | `EnumFlagsField` | |
| `AddOpenScriptCommand(Object)` | `VisualElement` | left double-click opens the script of a `MonoBehaviour` / `ScriptableObject`; other objects add nothing |
| `GetOwnerWindow()` | `VisualElement` | returns `EditorWindow`: the window owning the element's panel, else the focused window, else the one under the mouse, or `null` |
