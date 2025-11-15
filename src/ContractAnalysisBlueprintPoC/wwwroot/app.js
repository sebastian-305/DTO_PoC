const form = document.getElementById('analysis-form');
const typeInputs = Array.from(form.querySelectorAll('input[name="analysis-type"]'));
const queryInput = document.getElementById('query-input');
const queryLabel = form.querySelector('label[for="query-input"]');
const resultOutput = document.getElementById('result-output');
const schemaHint = document.getElementById('schema-hint');

const PLACEHOLDER = {
    person: 'z.\u00a0B. Marie Curie',
    country: 'z.\u00a0B. Portugal'
};

const HINTS = {
    person: 'Das Schema liefert biografische Daten inklusive Kurzbiografie, Auszeichnungen, Werken und Bild-Prompt.',
    country: 'Das Schema liefert Fakten zu Hauptstadt, Einwohnerzahl, Amtssprachen, Staatsform, Kurzbeschreibung und Bild-Prompt.'
};

const LABELS = {
    person: 'Berühmte Person',
    country: 'Land'
};

const EMPTY_MESSAGES = {
    person: 'Bitte gib den Namen einer berühmten Person an.',
    country: 'Bitte gib den Namen eines Landes an.'
};

const schemaMetadataCache = new Map();

let currentType = getSelectedType();

renderMessage('Noch keine Analyse vorhanden.');
updateFormForType(currentType);

typeInputs.forEach((input) => {
    input.addEventListener('change', () => {
        if (input.checked) {
            currentType = input.value;
            updateFormForType(currentType);
            renderMessage('Noch keine Analyse vorhanden.');
        }
    });
});

form.addEventListener('submit', async (event) => {
    event.preventDefault();

    const query = queryInput.value.trim();
    if (!query) {
        showError(EMPTY_MESSAGES[currentType]);
        return;
    }

    setLoadingState(true);

    const payload = { type: currentType };
    if (currentType === 'person') {
        payload.person = query;
    } else {
        payload.country = query;
    }

    try {
        const response = await fetch('/api/analyze', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify(payload)
        });

        if (!response.ok) {
            const error = await response.json().catch(() => ({ detail: 'Unbekannter Fehler.' }));
            const detail = error.detail || error.message;
            showError(detail || EMPTY_MESSAGES[currentType]);
            return;
        }

        const result = await response.json();
        await renderResult(result);
    } catch (error) {
        console.error(error);
        showError('Die Anfrage konnte nicht gesendet werden.');
    } finally {
        setLoadingState(false);
    }
});

function getSelectedType() {
    const selected = typeInputs.find((input) => input.checked);
    return selected ? selected.value : 'person';
}

function updateFormForType(type) {
    queryInput.placeholder = PLACEHOLDER[type];
    schemaHint.textContent = HINTS[type];
    if (queryLabel) {
        queryLabel.textContent = LABELS[type];
    }
}

async function renderResult(result) {
    resultOutput.classList.remove('result-output--message', 'result-output--error');
    resultOutput.innerHTML = '';

    const meta = document.createElement('p');
    meta.className = 'result-meta';
    meta.textContent = `Typ: ${formatType(result?.type)} – Anfrage: ${result?.query ?? 'unbekannt'}`;
    resultOutput.appendChild(meta);

    const normalizedType = normalizeType(result?.type ?? currentType);
    const metadata = await getSchemaMetadata(normalizedType);
    const content = createNodeFromData(result?.data ?? result, { metadata });
    resultOutput.appendChild(content);

    const prompt = extractPrompt(result);
    if (prompt) {
        const nodes = createImageContainer(prompt);
        resultOutput.appendChild(nodes.container);
        renderImageSection(nodes, result?.image, result?.imageError);
    }
}

function formatType(type) {
    if (typeof type === 'string' && type.toLowerCase() === 'country') {
        return 'Land';
    }
    return 'Person';
}

function extractPrompt(result) {
    const prompt = result?.image?.prompt
        ?? result?.data?.bildPrompt
        ?? result?.data?.bildprompt;

    if (typeof prompt === 'string') {
        const trimmed = prompt.trim();
        return trimmed.length > 0 ? trimmed : '';
    }

    return '';
}

function renderImageSection(nodes, imageResult, imageError) {
    const rendered = renderImage(nodes, imageResult);
    if (rendered) {
        return;
    }

    if (imageError) {
        renderImageError(nodes, imageError);
        return;
    }

    if (nodes.status) {
        nodes.status.remove();
    }

    const fallback = document.createElement('p');
    fallback.className = 'result-message result-message--error';
    fallback.textContent = 'Bild konnte nicht dargestellt werden.';
    nodes.container.appendChild(fallback);
}

function renderImage(nodes, imageResult) {
    if (!nodes || !imageResult) {
        return false;
    }

    if (imageResult.prompt) {
        nodes.prompt.textContent = imageResult.prompt;
    }

    const source = resolveImageSource(imageResult);
    if (!source) {
        return false;
    }

    if (nodes.status) {
        nodes.status.remove();
    }

    const img = document.createElement('img');
    img.className = 'result-image__preview';
    img.alt = imageResult.prompt || 'Automatisch generiertes Bild';
    img.src = source;
    nodes.container.appendChild(img);

    return true;
}

function renderImageError(nodes, message) {
    if (!nodes?.status) {
        return;
    }

    nodes.status.textContent = message || 'Bildgenerierung fehlgeschlagen.';
    nodes.status.classList.add('result-message--error');
}

function createImageContainer(prompt) {
    const container = document.createElement('section');
    container.className = 'result-image';

    const heading = document.createElement('h3');
    heading.className = 'result-image__heading';
    heading.textContent = 'Bildidee';

    const promptNode = document.createElement('p');
    promptNode.className = 'result-message result-image__prompt';
    promptNode.textContent = prompt;

    const status = document.createElement('p');
    status.className = 'result-message';
    status.textContent = 'Bild wird vorbereitet…';

    container.appendChild(heading);
    container.appendChild(promptNode);
    container.appendChild(status);

    return {
        container,
        status,
        prompt: promptNode
    };
}

function resolveImageSource(imageResult) {
    if (!imageResult) {
        return null;
    }

    if (imageResult.imageUrl) {
        return imageResult.imageUrl;
    }

    if (imageResult.imageBase64) {
        const mediaType = imageResult.mediaType || 'image/png';
        return `data:${mediaType};base64,${imageResult.imageBase64}`;
    }

    return null;
}

function showError(message) {
    renderMessage(`Fehler: ${message}`, { isError: true });
}

function setLoadingState(isLoading) {
    form.querySelector('button').disabled = isLoading;
    if (isLoading) {
        renderMessage('Analyse läuft…');
    }
}

function renderMessage(message, { isError = false } = {}) {
    resultOutput.classList.add('result-output--message');
    resultOutput.classList.toggle('result-output--error', isError);
    resultOutput.innerHTML = '';

    const paragraph = document.createElement('p');
    paragraph.className = 'result-message';
    paragraph.textContent = message;

    resultOutput.appendChild(paragraph);
}

function createNodeFromData(data, context = {}) {
    if (Array.isArray(data)) {
        return createListNode(data, context);
    }

    if (data !== null && typeof data === 'object') {
        return createDictionaryNode(data, context);
    }

    return createValueNode(data, context.fieldMeta);
}

function createDictionaryNode(object, context = {}) {
    const entries = Object.entries(object);

    if (entries.length === 0) {
        return createMessageNode('Keine Daten vorhanden.');
    }

    const container = document.createElement('dl');
    container.className = 'result-dictionary';

    const metadata = context.metadata || null;
    const orderedEntries = metadata
        ? [...entries].sort((a, b) => {
            const metaA = metadata[a[0]];
            const metaB = metadata[b[0]];
            const orderA = typeof metaA?.order === 'number' ? metaA.order : Number.MAX_SAFE_INTEGER;
            const orderB = typeof metaB?.order === 'number' ? metaB.order : Number.MAX_SAFE_INTEGER;
            return orderA - orderB;
        })
        : entries;

    orderedEntries.forEach(([key, value]) => {
        const fieldMeta = metadata?.[key];
        const term = document.createElement('dt');
        const displayLabel = fieldMeta?.label ?? formatKeyForDisplay(key);
        term.textContent = displayLabel;

        const tooltipText = typeof fieldMeta?.tooltip === 'string' ? fieldMeta.tooltip.trim() : '';
        const hint = typeof fieldMeta?.hint === 'string' ? fieldMeta.hint.trim() : '';
        const hoverText = tooltipText || hint;
        term.title = hoverText ? `${displayLabel} – ${hoverText}` : displayLabel;

        const description = document.createElement('dd');
        const childContext = {
            metadata: fieldMeta?.children,
            fieldMeta
        };
        description.appendChild(createNodeFromData(value, childContext));

        container.appendChild(term);
        container.appendChild(description);
    });

    return container;
}

function createListNode(items, context = {}) {
    if (items.length === 0) {
        return createMessageNode('Keine Werte.');
    }

    const fieldMeta = context.fieldMeta;
    if (fieldMeta?.variant === 'pill-list') {
        return createPillList(items);
    }

    const list = document.createElement('ol');
    list.className = 'result-list';

    const childMetadata = fieldMeta?.children || context.metadata;

    items.forEach((item) => {
        const listItem = document.createElement('li');
        const childContext = {
            metadata: childMetadata,
            fieldMeta: null
        };
        listItem.appendChild(createNodeFromData(item, childContext));
        list.appendChild(listItem);
    });

    return list;
}

function createValueNode(value, fieldMeta) {
    const span = document.createElement('span');
    span.className = 'result-value';

    if (fieldMeta?.variant === 'highlight') {
        span.classList.add('result-value--highlight');
    }

    if (fieldMeta?.variant === 'stat') {
        span.classList.add('result-value--stat');
    }

    if (fieldMeta?.variant === 'muted') {
        span.classList.add('result-value--muted');
    }

    span.textContent = formatPrimitiveValue(value);

    return span;
}

function createMessageNode(text) {
    const wrapper = document.createElement('p');
    wrapper.className = 'result-message';
    wrapper.textContent = text;
    return wrapper;
}

function createPillList(items) {
    if (!Array.isArray(items) || items.length === 0) {
        return createMessageNode('Keine Werte.');
    }

    const wrapper = document.createElement('div');
    wrapper.className = 'result-pill-list';

    items.forEach((item) => {
        const pill = document.createElement('span');
        pill.className = 'result-value result-value--pill';
        pill.textContent = formatPrimitiveValue(item);
        wrapper.appendChild(pill);
    });

    return wrapper;
}

function formatPrimitiveValue(value) {
    if (value === null || value === undefined || value === '') {
        return 'Keine Daten';
    }

    if (typeof value === 'number') {
        return value.toLocaleString('de-DE');
    }

    if (typeof value === 'boolean') {
        return value ? 'Ja' : 'Nein';
    }

    return String(value);
}

function formatKeyForDisplay(key) {
    if (typeof key !== 'string' || key.length === 0) {
        return '';
    }

    const withSpaces = key
        .replace(/([A-Z])/g, ' $1')
        .replace(/_/g, ' ')
        .trim();

    return withSpaces.charAt(0).toUpperCase() + withSpaces.slice(1);
}

function normalizeType(type) {
    if (typeof type === 'string') {
        const lowered = type.toLowerCase();
        return lowered === 'country' ? 'country' : 'person';
    }

    if (typeof type === 'number') {
        return type === 1 ? 'country' : 'person';
    }

    return 'person';
}

async function getSchemaMetadata(type) {
    const normalized = normalizeType(type);
    if (schemaMetadataCache.has(normalized)) {
        return schemaMetadataCache.get(normalized);
    }

    try {
        const schema = await fetchSchema(normalized);
        const metadata = buildMetadataFromSchema(schema);
        schemaMetadataCache.set(normalized, metadata);
        return metadata;
    } catch (error) {
        console.error('Schema konnte nicht geladen werden:', error);
        schemaMetadataCache.set(normalized, null);
        return null;
    }
}

async function fetchSchema(type) {
    const search = type ? `?type=${encodeURIComponent(type)}` : '';
    const response = await fetch(`/api/schema${search}`);
    if (!response.ok) {
        throw new Error(`Schema-Request fehlgeschlagen (${response.status})`);
    }

    return response.json();
}

function buildMetadataFromSchema(schema) {
    return buildChildrenMetadata(schema);
}

function buildChildrenMetadata(definition) {
    const properties = definition?.properties;
    if (!properties || typeof properties !== 'object') {
        return null;
    }

    const metadata = {};
    Object.entries(properties).forEach(([key, propertyDefinition]) => {
        const fieldMeta = buildFieldMetadata(key, propertyDefinition);
        if (fieldMeta) {
            metadata[key] = fieldMeta;
        }
    });

    return Object.keys(metadata).length > 0 ? metadata : null;
}

function buildFieldMetadata(key, definition) {
    if (!definition || typeof definition !== 'object') {
        return null;
    }

    const ui = definition['x-ui'] || {};
    const label = typeof ui.label === 'string' ? ui.label : formatKeyForDisplay(key);
    const hint = typeof ui.hint === 'string' ? ui.hint : definition.description;
    const tooltip = typeof ui.tooltip === 'string'
        ? ui.tooltip
        : (typeof hint === 'string' ? hint : definition.description);
    const order = typeof ui.order === 'number' ? ui.order : Number.MAX_SAFE_INTEGER;
    const variant = typeof ui.variant === 'string' ? ui.variant : null;

    const fieldMeta = {
        label,
        hint,
        tooltip,
        order,
        variant
    };

    const children = resolveChildrenMetadata(definition);
    if (children) {
        fieldMeta.children = children;
    }

    return fieldMeta;
}

function resolveChildrenMetadata(definition) {
    if (!definition || typeof definition !== 'object') {
        return null;
    }

    if (definition.type === 'object' || typeof definition.properties === 'object') {
        return buildChildrenMetadata(definition);
    }

    if (definition.type === 'array' || definition.items) {
        const itemsDefinition = Array.isArray(definition.items)
            ? definition.items[0]
            : definition.items;
        return resolveChildrenMetadata(itemsDefinition);
    }

    return null;
}
