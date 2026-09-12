/** Prism themes in the Ayu palette (Light and Dark), Dark uses the Ayu editor background as in Rider. */

const light = {
  plain: { color: '#4b463d', backgroundColor: '#e1d8c8' },
  styles: [
    { types: ['comment', 'prolog', 'doctype', 'cdata'], style: { color: '#62594e' } },
    { types: ['punctuation'], style: { color: '#4b463d' } },
    { types: ['keyword', 'operator', 'important'], style: { color: '#9b4209' } },
    { types: ['builtin', 'class-name', 'namespace', 'maybe-class-name', 'return-type'], style: { color: '#1e6091' } },
    { types: ['function'], style: { color: '#805200' } },
    { types: ['string', 'char', 'attr-value', 'inserted'], style: { color: '#47640f' } },
    { types: ['number', 'boolean', 'constant', 'symbol'], style: { color: '#714a94' } },
    { types: ['regex'], style: { color: '#1e674d' } },
    { types: ['tag', 'selector', 'deleted'], style: { color: '#226276' } },
    { types: ['attr-name', 'property', 'variable'], style: { color: '#a33844' } },
    { types: ['annotation', 'decorator', 'attribute'], style: { color: '#1e6091' } },
  ],
};

const dark = {
  plain: { color: '#bfbdb6', backgroundColor: '#0e1015' },
  styles: [
    { types: ['comment', 'prolog', 'doctype', 'cdata'], style: { color: '#89919d' } },
    { types: ['punctuation'], style: { color: '#bfbdb6' } },
    { types: ['keyword', 'operator', 'important'], style: { color: '#f29750' } },
    { types: ['builtin', 'class-name', 'namespace', 'maybe-class-name', 'return-type'], style: { color: '#73c0f8' } },
    { types: ['function'], style: { color: '#ffb454' } },
    { types: ['string', 'char', 'attr-value', 'inserted'], style: { color: '#b0d860' } },
    { types: ['number', 'boolean', 'constant', 'symbol'], style: { color: '#d2a6ff' } },
    { types: ['regex'], style: { color: '#95e6cb' } },
    { types: ['tag', 'selector', 'deleted'], style: { color: '#39bae6' } },
    { types: ['attr-name', 'property', 'variable'], style: { color: '#f07178' } },
    { types: ['annotation', 'decorator', 'attribute'], style: { color: '#73c0f8' } },
  ],
};

export default { light, dark };
