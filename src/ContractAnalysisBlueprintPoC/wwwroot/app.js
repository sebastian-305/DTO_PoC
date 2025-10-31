const blueprintSelect = document.getElementById('blueprint-select');
const summaryContainer = document.querySelector('#summary .summary-content');
const summaryHeading = document.querySelector('#summary h2');
const sectionsContainer = document.querySelector('#sections .section-content');
const conclusionContainer = document.querySelector('#conclusion .conclusion-content');
const conclusionHeading = document.querySelector('#conclusion h2');
const messageElement = document.getElementById('message');
const schemaPanel = document.getElementById('schema-panel');
const schemaPre = document.getElementById('schema-json');
const showSchemaButton = document.getElementById('show-schema');
const reloadButton = document.getElementById('reload-data');

let blueprints = [];
let currentBlueprint = null;
let currentSample = null;

async function fetchJson(url) {
    const response = await fetch(url);
    if (!response.ok) {
        const text = await response.text();
        throw new Error(`Fehler beim Laden von ${url}: ${response.status} ${text}`);
    }
    return response.json();
}

function showMessage(text, type = 'info') {
    if (!text) {
        messageElement.hidden = true;
        messageElement.textContent = '';
        messageElement.className = 'message';
        return;
    }

    messageElement.hidden = false;
    messageElement.textContent = text;
    messageElement.className = `message ${type}`;
}

async function loadBlueprints() {
    try {
        blueprints = await fetchJson('/api/blueprints');
        blueprintSelect.innerHTML = '';

        blueprints.forEach((bp, index) => {
            const option = document.createElement('option');
            option.value = bp.id;
            option.textContent = `${bp.displayName} (${bp.sectionCount} Abschnitte)`;
            if (index === 0) {
                option.selected = true;
            }
            blueprintSelect.append(option);
        });

        if (blueprints.length > 0) {
            await loadBlueprintDetails(blueprints[0].id);
        }
    } catch (error) {
        console.error(error);
        showMessage('Blueprints konnten nicht geladen werden.', 'error');
    }
}

async function loadBlueprintDetails(id) {
    try {
        showMessage('');
        schemaPanel.hidden = true;
        schemaPre.textContent = '';

        currentBlueprint = await fetchJson(`/api/blueprints/${id}`);
        summaryHeading.textContent = currentBlueprint.summaryTitle;
        conclusionHeading.textContent = currentBlueprint.conclusionLabel;

        await loadSample(id);
    } catch (error) {
        console.error(error);
        showMessage('Blueprint konnte nicht geladen werden.', 'error');
    }
}

async function loadSample(id) {
    try {
        currentSample = await fetchJson(`/api/samples/${id}`);
        showMessage('');
        renderCurrent();
    } catch (error) {
        console.error(error);
        showMessage('Beispieldaten konnten nicht geladen werden.', 'error');
    }
}

function renderCurrent() {
    if (!currentBlueprint || !currentSample) {
        return;
    }

    renderSummary(currentSample.summary ?? {}, currentBlueprint.summaryFields ?? []);
    renderSections(currentSample.sections ?? {}, currentBlueprint.sections ?? []);
    renderConclusion(
        currentSample.conclusion ?? '',
        currentBlueprint.conclusionLabel ?? 'Fazit',
        currentSample.collectiveAgreement,
        currentBlueprint.collectiveAgreementLabel
    );
}

function renderSummary(summary, fields) {
    summaryContainer.innerHTML = '';

    if (!fields.length) {
        summaryContainer.textContent = 'Keine Zusammenfassung verfügbar.';
        return;
    }

    const dl = document.createElement('dl');
    fields.forEach(field => {
        const dt = document.createElement('dt');
        dt.textContent = field.label;
        const dd = document.createElement('dd');
        dd.textContent = summary[field.id] ?? '–';
        dl.append(dt, dd);
    });

    summaryContainer.append(dl);
}

function renderSections(sections, sectionBlueprints) {
    sectionsContainer.innerHTML = '';

    if (!sectionBlueprints.length) {
        sectionsContainer.textContent = 'Keine Abschnitte vorhanden.';
        return;
    }

    sectionBlueprints.forEach(section => {
        const article = document.createElement('article');
        article.className = 'section-card';

        const heading = document.createElement('h3');
        heading.innerHTML = `${section.title}`;
        article.append(heading);

        const entries = sections[section.id] ?? [];
        if (!entries.length) {
            const empty = document.createElement('p');
            empty.textContent = 'Keine Einträge.';
            empty.className = 'empty-hint';
            article.append(empty);
        } else {
            entries.forEach(entry => {
                const item = document.createElement('div');
                item.className = 'section-item';

                section.fields.forEach(field => {
                    const row = document.createElement('div');
                    row.className = 'field-row';

                    const label = document.createElement('span');
                    label.className = 'field-label';
                    label.textContent = field.label;

                    const value = createFieldValue(field, entry[field.id]);
                    row.append(label, value);
                    item.append(row);
                });

                article.append(item);
            });
        }

        sectionsContainer.append(article);
    });
}

function createFieldValue(field, value) {
    if (field.kind === 'List') {
        const list = Array.isArray(value) ? value : [];
        const ul = document.createElement('ul');
        ul.className = 'value-list';
        if (!list.length) {
            const li = document.createElement('li');
            li.textContent = '–';
            ul.append(li);
        } else {
            list.forEach(item => {
                const li = document.createElement('li');
                li.textContent = item;
                ul.append(li);
            });
        }
        return ul;
    }

    const text = value ?? '–';
    if (field.kind === 'Emphasis') {
        const em = document.createElement('em');
        em.textContent = text;
        return em;
    }

    const span = document.createElement('span');
    span.textContent = text;
    return span;
}

function renderConclusion(conclusion, conclusionLabel, collectiveAgreement, collectiveAgreementLabel) {
    conclusionContainer.innerHTML = '';

    const conclusionParagraph = document.createElement('p');
    conclusionParagraph.textContent = conclusion || 'Keine Bewertung vorhanden.';
    conclusionContainer.append(conclusionParagraph);

    if (collectiveAgreementLabel && collectiveAgreement) {
        const collective = document.createElement('p');
        collective.innerHTML = `<strong>${collectiveAgreementLabel}:</strong> ${collectiveAgreement}`;
        conclusionContainer.append(collective);
    }
}

showSchemaButton.addEventListener('click', async () => {
    if (!currentBlueprint) {
        return;
    }

    try {
        const schema = await fetchJson(`/api/blueprints/${currentBlueprint.id}/schema`);
        schemaPanel.hidden = false;
        schemaPre.textContent = JSON.stringify(schema, null, 2);
    } catch (error) {
        console.error(error);
        showMessage('Schema konnte nicht geladen werden.', 'error');
    }
});

reloadButton.addEventListener('click', async () => {
    if (!currentBlueprint) {
        return;
    }

    await loadSample(currentBlueprint.id);
});

blueprintSelect.addEventListener('change', async event => {
    const id = event.target.value;
    if (id) {
        await loadBlueprintDetails(id);
    }
});

loadBlueprints();
