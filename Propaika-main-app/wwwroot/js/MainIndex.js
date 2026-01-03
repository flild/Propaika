// ==========================================
// 1. СЛАЙДЕР КЕЙСОВ ("До / После")
// ==========================================
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

    // --- События мыши ---
    slider.addEventListener('mousedown', e => {
        // e.preventDefault(); // <-- УБРАЛ, т.к. может блокировать фокус, если нужно
        startDrag(e.clientX);
    });

    window.addEventListener('mousemove', e => moveDrag(e.clientX));
    window.addEventListener('mouseup', endDrag);

    // --- События тача ---
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

    // Клик по слайдеру — прыжок ручки
    slider.addEventListener('click', e => setPos(getXPercent(e.clientX)));

    // Синхронизация с input range (для доступности)
    range.addEventListener('input', e => setPos(parseFloat(e.target.value)));
});

document.addEventListener("DOMContentLoaded", function () {
    const toggle = document.getElementById('mapToggle');
    const wrapper = document.getElementById('mapWrapper');

    if (toggle && wrapper) {
        toggle.addEventListener('change', function () {
            if (this.checked) {
                wrapper.classList.add('no-filter');
            } else {
                wrapper.classList.remove('no-filter');
            }
        });
    }
});
// ==========================================
// 2. ОТПРАВКА ФОРМЫ С КАПЧЕЙ
// ==========================================
document.addEventListener("DOMContentLoaded", function () {

    const form = document.getElementById('ajax-repair-form');
    // Если формы нет на странице, выходим
    if (!form) return;

    const formContainer = document.getElementById('form-container');
    const successMsg = document.getElementById('success-message');
    const submitBtn = document.getElementById('submit-btn');
    const btnText = submitBtn.querySelector('.btn-text');
    const btnLoader = submitBtn.querySelector('.btn-loader');
    const serverErrorDiv = document.getElementById('server-error');
    const captchaErrorDiv = document.getElementById('captcha-error');

    // --- Обработчик отправки ---
    form.addEventListener('submit', async function (e) {
        e.preventDefault(); // Останавливаем стандартную отправку

        // 1. Сброс предыдущих ошибок
        if (serverErrorDiv) serverErrorDiv.classList.add('d-none');
        if (captchaErrorDiv) captchaErrorDiv.classList.add('d-none');

        // 2. Проверка HTML5 валидации (required поля)
        if (!form.checkValidity()) {
            e.stopPropagation();
            form.classList.add('was-validated');
            return;
        }

        // 3. Проверка Yandex SmartCaptcha
        // Яндекс сам создает скрытый input с name="smart-token" внутри div-контейнера капчи
        const smartTokenInput = form.querySelector('input[name="smart-token"]');

        // Если инпута нет или он пуст — значит капча не решена
        if (!smartTokenInput || !smartTokenInput.value) {
            if (captchaErrorDiv) captchaErrorDiv.classList.remove('d-none');
            return; // Прерываем отправку
        }

        // 4. Визуал загрузки
        submitBtn.disabled = true;
        btnText.classList.add('opacity-50');
        btnLoader.classList.remove('d-none');

        try {
            const formData = new FormData(form);

            // AJAX запрос
            const response = await fetch(window.location.href, {
                method: 'POST',
                body: formData,
                headers: {
                    'X-Requested-With': 'XMLHttpRequest'
                }
            });

            if (response.ok) {
                // Пытаемся распарсить JSON
                const result = await response.json();

                if (result.success) {
                    // УСПЕХ: Скрываем форму, показываем галочку
                    formContainer.classList.add('d-none');
                    successMsg.classList.remove('d-none');

                    form.reset(); // Чистим поля
                    form.classList.remove('was-validated');

                    // Сбрасываем капчу Яндекс
                    if (window.smartCaptcha) window.smartCaptcha.reset();
                } else {
                    // ОШИБКА ОТ СЕРВЕРА
                    serverErrorDiv.textContent = result.message || 'Ошибка обработки данных.';
                    serverErrorDiv.classList.remove('d-none');

                    // При ошибке капчу нужно сбрасывать, так как токен одноразовый
                    if (window.smartCaptcha) window.smartCaptcha.reset();
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
            // Возвращаем кнопку "в строй"
            submitBtn.disabled = false;
            btnText.classList.remove('opacity-50');
            btnLoader.classList.add('d-none');
        }
    });

    // --- Логика кнопки "Отправить ещё" (Reset) ---
    if (successMsg) {
        const resetBtn = successMsg.querySelector('button');
        if (resetBtn) {
            resetBtn.addEventListener('click', function () {
                // Скрываем успех, показываем форму
                successMsg.classList.add('d-none');
                formContainer.classList.remove('d-none');

                // Чистим всё начисто
                form.reset();
                form.classList.remove('was-validated');
                if (serverErrorDiv) serverErrorDiv.classList.add('d-none');
                if (captchaErrorDiv) captchaErrorDiv.classList.add('d-none');

                // Сбрасываем капчу
                if (window.smartCaptcha) window.smartCaptcha.reset();
            });
        }
    }
});