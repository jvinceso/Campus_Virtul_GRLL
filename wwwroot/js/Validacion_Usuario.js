window.addEventListener('load', () => {
    const form = document.getElementById('formulario');
    const dni = document.getElementById('dni');
    const contrasena = document.getElementById('contrasena');

    console.log('JavaScript de Login cargado');

    // Límites de caracteres
    const LIMITES = {
        DNI: 8,
        CONTRASENA_MIN: 4,
        CONTRASENA_MAX: 7
    };

    dni.addEventListener('input', (e) => {
        e.target.value = e.target.value.replace(/[^\d]/g, '').slice(0, LIMITES.DNI);
    });

    form.addEventListener('submit', (e) => {
        console.log('Submit detectado');
        e.preventDefault();
        validaCampos();
    });

    const validaCampos = () => {
        const dniValor = dni.value.trim();
        const contrasenaValor = contrasena.value.trim();

        let esValido = true;

        if (dniValor === '') {
            validaFalla(dni, 'El campo DNI es obligatorio');
            esValido = false;
        } else if (dniValor.length !== LIMITES.DNI) {
            validaFalla(dni, `El DNI debe tener exactamente ${LIMITES.DNI} dígitos`);
            esValido = false;
        } else if (!/^\d{8}$/.test(dniValor)) {
            validaFalla(dni, 'El DNI solo debe contener números');
            esValido = false;
        } else {
            validaOK(dni);
        }

        if (contrasenaValor === '') {
            validaFalla(contrasena, 'El campo contraseña es obligatorio');
            esValido = false;
        } else if (contrasenaValor.length < LIMITES.CONTRASENA_MIN) {
            validaFalla(contrasena, `La contraseña debe tener al menos ${LIMITES.CONTRASENA_MIN} caracteres`);
            esValido = false;
        } else if (contrasenaValor.length < LIMITES.CONTRASENA_MAX) {
            validaFalla(contrasena, `La contraseña debe tener al menos ${LIMITES.CONTRASENA_MAX} caracteres`);
            esValido = false;
        } else {
            validaOK(contrasena);
        }

        console.log('¿Es válido?:', esValido);

        if (esValido) {
            console.log('Enviando formulario...');
            form.submit();
        }
    };

    const validaFalla = (input, mensaje) => {
        const inputBox = input.parentElement;
        const avisoAnterior = inputBox.querySelector('.aviso');
        if (avisoAnterior) avisoAnterior.remove();

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

    const validaOK = (input) => {
        const inputBox = input.parentElement;
        const aviso = inputBox.querySelector('.aviso');
        if (aviso) aviso.remove();
        input.style.borderColor = '#28a745';
    };

    dni.addEventListener('blur', () => {
        const valor = dni.value.trim();
        if (valor === '') {
            return; 
        }
        if (valor.length === LIMITES.DNI && /^\d{8}$/.test(valor)) {
            validaOK(dni);
        } else {
            validaFalla(dni, `El DNI debe tener exactamente ${LIMITES.DNI} dígitos numéricos`);
        }
    });

    contrasena.addEventListener('blur', () => {
        const valor = contrasena.value.trim();
        if (valor === '') {
            return; 
        }
        if (valor.length >= LIMITES.CONTRASENA_MIN && valor.length <= LIMITES.CONTRASENA_MAX) {
            validaOK(contrasena);
        } else {
            validaFalla(contrasena, `La contraseña debe tener entre ${LIMITES.CONTRASENA_MAX} caracteres`);
        }
    });

    dni.addEventListener('focus', () => {
        const inputBox = dni.parentElement;
        const aviso = inputBox.querySelector('.aviso');
        if (aviso) aviso.remove();
        dni.style.borderColor = '#ccc';
    });

    contrasena.addEventListener('focus', () => {
        const inputBox = contrasena.parentElement;
        const aviso = inputBox.querySelector('.aviso');
        if (aviso) aviso.remove();
        contrasena.style.borderColor = '#ccc';
    });
});