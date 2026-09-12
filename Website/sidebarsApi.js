import generated from './api/sidebar.js';

// Keep generated docs untouched; the shared namespace prefix is redundant in navigation.
export default {
  api: generated.api.map((item) => item.type === 'category' ? {
    ...item,
    label: item.label.replace(/^Aspid\.FastTools\./, ''),
    className: 'api-namespace',
  } : item),
};
