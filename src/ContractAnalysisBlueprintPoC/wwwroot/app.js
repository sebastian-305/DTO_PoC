const form = document.getElementById('analysis-form');
const countryInput = document.getElementById('country-input');
const resultOutput = document.getElementById('result-output');

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
    const formatted = JSON.stringify(result, null, 2);
    resultOutput.textContent = formatted;
}

function showError(message) {
    resultOutput.textContent = `Fehler: ${message}`;
}

function setLoadingState(isLoading) {
    form.querySelector('button').disabled = isLoading;
    resultOutput.textContent = isLoading ? 'Analyse läuft…' : resultOutput.textContent;
}
