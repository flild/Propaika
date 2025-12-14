//< слайдер кейсов>
document.querySelectorAll('.cases-teaser .ba-slider').forEach(slider => {
    const range = slider.querySelector('.ba-range');
    const before = slider.querySelector('.ba-img-before');
    const divider = slider.querySelector('.ba-divider');
    const handle = slider.querySelector('.ba-handle');

    let dragging = false;

    const clamp = v => Math.max(0, Math.min(100, v));
    const setPos = v => {
        const x = clamp(v);
        before.style.clipPath = `inset(0 ${100 - x}% 0 0)`;
        divider.style.left = x + '%';
        handle.style.left = x + '%';
        range.value = x;
    };

    // стартовая позиция
    setPos(parseFloat(range.value || '50'));

    const getXPercent = clientX => {
        const rect = slider.getBoundingClientRect();
        const rel = (clientX - rect.left) / rect.width;
        return clamp(rel * 100);
    };

    const startDrag = clientX => {
        dragging = true;
        setPos(getXPercent(clientX));
    };

    const moveDrag = clientX => {
        if (!dragging) return;
        setPos(getXPercent(clientX));
    };

    const endDrag = () => { dragging = false; };

    // мышь
    slider.addEventListener('mousedown', e => {
        e.preventDefault();
        startDrag(e.clientX);
    });

    window.addEventListener('mousemove', e => moveDrag(e.clientX));
    window.addEventListener('mouseup', endDrag);

    // тач
    slider.addEventListener('touchstart', e => {
        const t = e.touches[0];
        startDrag(t.clientX);
    }, { passive: true });

    window.addEventListener('touchmove', e => {
        if (!dragging) return;
        const t = e.touches[0];
        moveDrag(t.clientX);
    }, { passive: true });

    window.addEventListener('touchend', endDrag);
    window.addEventListener('touchcancel', endDrag);

    // клик по слайдеру — прыжок ручки
    slider.addEventListener('click', e => setPos(getXPercent(e.clientX)));

    // синк с range для клавы/доступности
    range.addEventListener('input', e => setPos(parseFloat(e.target.value)));
});

document.querySelectorAll('.cases-teaser .ba-slider').forEach(slider => {
    // Блокируем клики по слайдеру, чтобы не было навигации/фокусов/выделений
    ['click', 'mousedown', 'touchstart', 'touchend'].forEach(evt =>
        slider.addEventListener(evt, (e) => {
            e.preventDefault();
            e.stopPropagation();
        }, { passive: false })
    );
});


//< ОТПРАВКА ФОРМЫ >
// Находим элементы
document.addEventListener("DOMContentLoaded", function () {

    // 1. Находим форму
    const form = document.getElementById('ajax-repair-form');

    // Если формы нет (например, другая страница), выходим, чтобы не было ошибок в консоли
    if (!form) return;

    const formContainer = document.getElementById('form-container');
    const successMsg = document.getElementById('success-message');
    const submitBtn = document.getElementById('submit-btn');
    const btnText = submitBtn.querySelector('.btn-text');
    const btnLoader = submitBtn.querySelector('.btn-loader');
    const serverErrorDiv = document.getElementById('server-error');

    // 2. Вешаем обработчик события
    form.addEventListener('submit', async function (e) {

        // ГЛАВНОЕ: Отменяем стандартную перезагрузку страницы
        e.preventDefault();

        // Сброс ошибок
        serverErrorDiv.classList.add('d-none');
        serverErrorDiv.textContent = '';

        // Валидация HTML5 (заполнены ли обязательные поля)
        if (!form.checkValidity()) {
            e.stopPropagation();
            form.classList.add('was-validated');
            return;
        }

        // Визуал загрузки
        submitBtn.disabled = true;
        btnText.classList.add('opacity-50');
        btnLoader.classList.remove('d-none');

        try {
            const formData = new FormData(form);

            // Добавляем токен анти-подделки (Razor Pages его скрыто добавляет в форму)
            // Обычно FormData его захватывает сама, но для надежности можно проверить

            const response = await fetch(window.location.href, {
                method: 'POST',
                body: formData,
                headers: {
                    'X-Requested-With': 'XMLHttpRequest'
                }
            });

            if (response.ok) {
                const result = await response.json();

                if (result.success) {
                    // УСПЕХ
                    formContainer.classList.add('d-none');
                    successMsg.classList.remove('d-none');
                    form.reset();
                    form.classList.remove('was-validated');
                } else {
                    // ОШИБКА В ДАННЫХ
                    serverErrorDiv.textContent = result.message || 'Ошибка обработки данных.';
                    serverErrorDiv.classList.remove('d-none');
                }
            } else {
                serverErrorDiv.textContent = 'Ошибка сети: ' + response.status;
                serverErrorDiv.classList.remove('d-none');
            }

        } catch (error) {
            console.error('Error:', error);
            serverErrorDiv.textContent = 'Ошибка отправки. Проверьте соединение.';
            serverErrorDiv.classList.remove('d-none');
        } finally {
            // Возвращаем кнопку в исходное состояние
            submitBtn.disabled = false;
            btnText.classList.remove('opacity-50');
            btnLoader.classList.add('d-none');
        }
    });
});

// Функция сброса (должна быть глобальной, чтобы работать из onclick в HTML)
window.resetForm = function () {
    document.getElementById('success-message').classList.add('d-none');
    document.getElementById('form-container').classList.remove('d-none');
    document.getElementById('ajax-repair-form').classList.remove('was-validated');
}