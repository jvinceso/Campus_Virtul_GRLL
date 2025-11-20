window.addEventListener('load', () => {
    const form = document.getElementById('formCambiarContrasena');
    const contrasenaActual = document.getElementById('contrasenaActual');
    const contrasenaNueva = document.getElementById('contrasenaNueva');
    const confirmarContrasena = document.getElementById('confirmarContrasena');

    console.log('JavaScript de Cambiar Contraseña cargado');

    const LIMITES = {
        CONTRASENA_MIN: 6,
        CONTRASENA_MAX: 20
    };

    // Prevenir envío y validar
    form.addEventListener('submit', (e) => {
        e.preventDefault();
        validarFormulario();
    });

    const validarFormulario = () => {
        let esValido = true;

        // Limpiar errores anteriores
        limpiarErrores();

        const actualValor = contrasenaActual.value.trim();
        const nuevaValor = contrasenaNueva.value.trim();
        const confirmarValor = confirmarContrasena.value.trim();

        // ✅ VALIDAR CONTRASEÑA ACTUAL
        if (actualValor === '') {
            mostrarError(contrasenaActual, 'La contraseña actual es obligatoria');
            esValido = false;
        } else if (actualValor.length < LIMITES.CONTRASENA_MIN) {
            mostrarError(contrasenaActual, `Debe tener al menos ${LIMITES.CONTRASENA_MIN} caracteres`);
            esValido = false;
        }

        // ✅ VALIDAR NUEVA CONTRASEÑA
        if (nuevaValor === '') {
            mostrarError(contrasenaNueva, 'La nueva contraseña es obligatoria');
            esValido = false;
        } else if (nuevaValor.length < LIMITES.CONTRASENA_MIN) {
            mostrarError(contrasenaNueva, `Debe tener al menos ${LIMITES.CONTRASENA_MIN} caracteres`);
            esValido = false;
        } else if (nuevaValor.length > LIMITES.CONTRASENA_MAX) {
            mostrarError(contrasenaNueva, `No puede exceder ${LIMITES.CONTRASENA_MAX} caracteres`);
            esValido = false;
        }

        // ✅ VALIDAR CONFIRMACIÓN
        if (confirmarValor === '') {
            mostrarError(confirmarContrasena, 'Debe confirmar la nueva contraseña');
            esValido = false;
        } else if (confirmarValor !== nuevaValor) {
            mostrarError(confirmarContrasena, 'Las contraseñas no coinciden');
            mostrarError(contrasenaNueva, 'Las contraseñas no coinciden');
            esValido = false;
        }

        // ✅ VALIDAR QUE LA NUEVA SEA DIFERENTE A LA ACTUAL
        if (esValido && nuevaValor === actualValor) {
            mostrarError(contrasenaNueva, 'La nueva contraseña debe ser diferente a la actual');
            esValido = false;
        }

        console.log('¿Formulario válido?:', esValido);

        if (esValido) {
            console.log('Enviando formulario...');
            form.submit();
        }
    };

    const mostrarError = (input, mensaje) => {
        const inputBox = input.parentElement;

        const aviso = document.createElement('small');
        aviso.className = 'aviso';
        aviso.style.color = '#e74c3c';
        aviso.style.fontSize = '13px';
        aviso.style.display = 'block';
        aviso.style.marginTop = '5px';
        aviso.innerText = mensaje;

        inputBox.appendChild(aviso);
        input.style.borderColor = '#e74c3c';
    };

    const limpiarErrores = () => {
        const avisos = document.querySelectorAll('.aviso');
        avisos.forEach(aviso => aviso.remove());

        [contrasenaActual, contrasenaNueva, confirmarContrasena].forEach(input => {
            input.style.borderColor = '#ccc';
        });
    };

    // Validación en tiempo real al escribir
    contrasenaNueva.addEventListener('input', () => {
        const valor = contrasenaNueva.value.trim();
        const confirmar = confirmarContrasena.value.trim();

        // Limpiar errores de este campo
        const avisos = contrasenaNueva.parentElement.querySelectorAll('.aviso');
        avisos.forEach(a => a.remove());

        if (valor.length >= LIMITES.CONTRASENA_MIN) {
            contrasenaNueva.style.borderColor = '#28a745';
        }

        // Si ya hay algo en confirmar, validar coincidencia
        if (confirmar !== '' && confirmar !== valor) {
            const avisosConfirmar = confirmarContrasena.parentElement.querySelectorAll('.aviso');
            avisosConfirmar.forEach(a => a.remove());
            mostrarError(confirmarContrasena, 'Las contraseñas no coinciden');
        } else if (confirmar === valor && confirmar !== '') {
            const avisosConfirmar = confirmarContrasena.parentElement.querySelectorAll('.aviso');
            avisosConfirmar.forEach(a => a.remove());
            confirmarContrasena.style.borderColor = '#28a745';
        }
    });

    // Validar confirmación en tiempo real
    confirmarContrasena.addEventListener('input', () => {
        const confirmar = confirmarContrasena.value.trim();
        const nueva = contrasenaNueva.value.trim();

        // Limpiar errores de este campo
        const avisos = confirmarContrasena.parentElement.querySelectorAll('.aviso');
        avisos.forEach(a => a.remove());

        if (confirmar === nueva && confirmar !== '') {
            confirmarContrasena.style.borderColor = '#28a745';
        } else if (confirmar !== '' && confirmar !== nueva) {
            mostrarError(confirmarContrasena, 'Las contraseñas no coinciden');
        }
    });

    // Limpiar errores al hacer focus
    [contrasenaActual, contrasenaNueva, confirmarContrasena].forEach(input => {
        input.addEventListener('focus', () => {
            const avisos = input.parentElement.querySelectorAll('.aviso');
            avisos.forEach(aviso => aviso.remove());
            input.style.borderColor = '#ccc';
        });
    });
});