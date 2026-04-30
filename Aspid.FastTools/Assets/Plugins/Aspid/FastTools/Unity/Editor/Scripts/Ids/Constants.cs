// ReSharper disable once CheckNamespace
namespace Aspid.FastTools.Ids.Editors
{
    internal static class Constants
    {
        public const string NoneOption = "<None>";

        public const string StringIdFieldName = "__stringId";
        public const string IntIdFieldName = "_id";

        public static class Drawer
        {
            public const string StyleSheetPath = "Styles/Aspid-FastTools-Id-Drawer";

            public const string Root = "aspid-fasttools-id-drawer";
            public const string MainRow = "aspid-fasttools-id-drawer__main-row";
            public const string Label = "aspid-fasttools-id-drawer__label";
            public const string Dropdown = "aspid-fasttools-id-drawer__dropdown";
            public const string DropdownMissing = "aspid-fasttools-id-drawer__dropdown--missing";
            public const string CreateButton = "aspid-fasttools-id-drawer__create-button";
            public const string CreateRow = "aspid-fasttools-id-drawer__create-row";
            public const string Input = "aspid-fasttools-id-drawer__input";
            public const string AddButton = "aspid-fasttools-id-drawer__add-button";
            public const string CancelButton = "aspid-fasttools-id-drawer__cancel-button";
            public const string Error = "aspid-fasttools-id-drawer__error";
            public const string OpenButton = "aspid-fasttools-id-drawer__open-button";
            public const string IntOnlyHint = "aspid-fasttools-id-drawer__int-only-hint";
        }

        public static class Registry
        {
            public const string StyleSheetPath = "Styles/Aspid-FastTools-Id-Registry";

            public const string Delete = "aspid-fasttools-id-registry__delete";
            public const string Confirm = "aspid-fasttools-id-registry__confirm";
            public const string Add = "aspid-fasttools-id-registry__add";
            public const string Warning = "aspid-fasttools-id-registry__warning";
            public const string WarningVisible = "aspid-fasttools-id-registry__warning--visible";
            public const string WarningLabel = "aspid-fasttools-id-registry__warning-label";
            public const string WarningButton = "aspid-fasttools-id-registry__warning-button";
            public const string NextId = "aspid-fasttools-id-registry__next-id";
            public const string Toolbar = "aspid-fasttools-id-registry__toolbar";
            public const string GroupFoldout = "aspid-fasttools-id-registry__group-foldout";
            public const string List = "aspid-fasttools-id-registry__list";

            public const int ScrollThreshold = 10;
            public const int MaxVisibleRows = 10;
            public const float RowHeight = 32.5f;
        }

        public static class Selector
        {
            public const string StyleSheetPath = "Styles/Aspid-FastTools-Id-Selector";

            public const string Container = "aspid-fasttools-id-selector__container";
            public const string Item = "aspid-fasttools-id-selector__item";
            public const string ItemName = "aspid-fasttools-id-selector__item-name";
            public const string ItemId = "aspid-fasttools-id-selector__item-id";
        }
    }
}
