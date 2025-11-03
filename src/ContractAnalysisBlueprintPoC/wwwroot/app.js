const form = document.getElementById('analysis-form');
const countryInput = document.getElementById('country-input');
const resultOutput = document.getElementById('result-output');
const DEFAULT_MESSAGE = 'Noch keine Analyse vorhanden.';

renderMessage(DEFAULT_MESSAGE);

form.addEventListener('submit', async (event) => {
    event.preventDefault();

    const country = countryInput.value.trim();
    if (!country) {
        showError('Bitte gib den Namen eines Landes an.');
        return;
    }

    setLoadingState(true);

    try {
        const response = await fetch('/api/analyze', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify({ country })
        });

        if (!response.ok) {
            const error = await response.json().catch(() => ({ detail: 'Unbekannter Fehler.' }));
            showError(error.detail || error.message || 'Analyse konnte nicht durchgeführt werden.');
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

function renderResult(result) {
    resultOutput.classList.remove('result-output--message', 'result-output--error');
    resultOutput.innerHTML = '';

    const content = createNodeFromData(result);
    resultOutput.appendChild(content);
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
