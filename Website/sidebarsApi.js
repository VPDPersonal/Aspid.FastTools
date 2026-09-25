import generated from './api/sidebar.js';

/**
 * Reshapes the generated API sidebar for navigation without touching `api/sidebar.js`:
 * - a namespace is a section whose title links to the namespace page, with a separate caret that folds it;
 * - the Classes/Structs/… layer is dropped; types keep DocFX's order (classes, structs, interfaces, enums, delegates);
 * - the generated `BaseFieldExtensionsSetLabel*` family folds into one collapsed group;
 * - types outside any namespace gather under "Global".
 */
const PREFIX = /^Aspid\.FastTools\./;
const SET_LABEL = /^BaseFieldExtensionsSetLabel/;

function foldSetLabel(items) {
  const overloads = items.filter((item) => SET_LABEL.test(item.label ?? ''));
  if (overloads.length < 2) return items;
  const rest = items.filter((item) => !SET_LABEL.test(item.label ?? ''));
  const anchor = Math.max(0, rest.findIndex((item) => item.label === 'BaseFieldExtensions') + 1);
  rest.splice(anchor, 0, {
    type: 'category',
    label: `SetLabel overloads (${overloads.length})`,
    collapsed: true,
    className: 'api-overloads',
    items: overloads,
  });
  return rest;
}

function namespaceSection(category) {
  const label = category.label.replace(PREFIX, '');
  const kinds = category.items.filter((item) => item.type === 'category');
  const items = foldSetLabel(kinds.flatMap((kind) => kind.items));
  return { type: 'category', label, collapsed: true, className: 'doc-menu-group api-namespace', link: category.link, items };
}

// DocFX emits a type outside any namespace as a childless category linked to its page.
const isNamespace = (item) => item.type === 'category' && item.items?.length > 0;
const asDoc = (item) => item.type === 'category' ? { type: 'doc', id: item.link.id, label: item.label } : item;

const namespaces = generated.api.filter(isNamespace).map(namespaceSection);
const globals = generated.api.filter((item) => !isNamespace(item)).map(asDoc);

const globalSection = { type: 'category', label: 'Global', collapsed: true, className: 'doc-menu-group api-namespace', items: globals };

export default {
  api: globals.length ? [...namespaces, globalSection] : namespaces,
};
