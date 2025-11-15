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
        renderResult(result);
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

function renderResult(result) {
    resultOutput.classList.remove('result-output--message', 'result-output--error');
    resultOutput.innerHTML = '';

    const meta = document.createElement('p');
    meta.className = 'result-meta';
    meta.textContent = `Typ: ${formatType(result?.type)} – Anfrage: ${result?.query ?? 'unbekannt'}`;
    resultOutput.appendChild(meta);

    const content = createNodeFromData(result?.data ?? result);
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

function createNodeFromData(data) {
    if (Array.isArray(data)) {
        return createListNode(data);
    }

    if (data !== null && typeof data === 'object') {
        return createDictionaryNode(data);
    }

    return createValueNode(data);
}

function createDictionaryNode(object) {
    const entries = Object.entries(object);

    if (entries.length === 0) {
        return createMessageNode('Keine Daten vorhanden.');
    }

    const container = document.createElement('dl');
    container.className = 'result-dictionary';

    entries.forEach(([key, value]) => {
        const term = document.createElement('dt');
        term.textContent = key;

        const description = document.createElement('dd');
        description.appendChild(createNodeFromData(value));

        container.appendChild(term);
        container.appendChild(description);
    });

    return container;
}

function createListNode(items) {
    if (items.length === 0) {
        return createMessageNode('Keine Werte.');
    }

    const list = document.createElement('ol');
    list.className = 'result-list';

    items.forEach((item) => {
        const listItem = document.createElement('li');
        listItem.appendChild(createNodeFromData(item));
        list.appendChild(listItem);
    });

    return list;
}

function createValueNode(value) {
    const span = document.createElement('span');
    span.className = 'result-value';

    if (value === null || value === undefined || value === '') {
        span.textContent = 'Keine Daten';
    } else if (typeof value === 'number') {
        span.textContent = value.toLocaleString('de-DE');
    } else if (typeof value === 'boolean') {
        span.textContent = value ? 'Ja' : 'Nein';
    } else {
        span.textContent = String(value);
    }

    return span;
}

function createMessageNode(text) {
    const wrapper = document.createElement('p');
    wrapper.className = 'result-message';
    wrapper.textContent = text;
    return wrapper;
}
