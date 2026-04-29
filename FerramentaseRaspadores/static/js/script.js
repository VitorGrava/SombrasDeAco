const botoesMenu = document.querySelectorAll('.botao-menu');
const conteudo = document.getElementById('conteudo');

let url = '';

const textoPadraoArquivo = 'Arraste e Solte a planilha, ou clique no botão para selecionar';


window.addEventListener('load', () => {
    document.getElementById('botaoFerramentas').click();
});


botoesMenu.forEach(botao => {
    botao.addEventListener('click', () => {

        if (document.querySelector('.botao-menu.active')) {
            document.querySelector('.botao-menu.active').classList.remove('active');
        }

        botao.classList.add('active');

        if (botao.id === 'botaoFerramentas') {
            url = '/ferramentas';
        } else if (botao.id === 'botaoRaspadores') {
            url = '/raspadores';
        }

        CarregarConteudo(url);
    });
});


function CarregarConteudo(url) {
    fetch(url)
        .then(response => response.text())
        .then(html => {
            conteudo.innerHTML = html;
        });
}


function toggleCard(event, card) {
    if (event.target.closest('.card-conteudo')) {
        return;
    }

    const conteudo = card.querySelector('.card-conteudo');

    conteudo.classList.toggle('show');
}


async function abrirArquivo(event, botao) {
    event.stopPropagation();

    const cardConteudo = botao.closest('.card-conteudo');
    const input = cardConteudo.querySelector('.input-arquivo');

    input.onchange = async () => {
        const arquivo = input.files[0];

        if (!arquivo) return;

        const resultadoValidacao = await validarPlanilha(arquivo);

        if (!resultadoValidacao.valido) {
            alert(resultadoValidacao.mensagem);

            input.value = '';

            return;
        }

        mostrarArquivo(cardConteudo, arquivo);
    };

    input.click();
}


function mostrarArquivo(cardConteudo, arquivo) {
    const nomeArquivo = cardConteudo.querySelector('p');
    const botaoRemover = cardConteudo.querySelector('.remover-arquivo');

    nomeArquivo.textContent = arquivo.name;

    if (botaoRemover) {
        botaoRemover.classList.remove('d-none');
    }
}


function removerArquivo(event, botao) {
    event.stopPropagation();

    const cardConteudo = botao.closest('.card-conteudo');
    const input = cardConteudo.querySelector('.input-arquivo');
    const nomeArquivo = cardConteudo.querySelector('p');

    input.value = '';

    nomeArquivo.textContent = textoPadraoArquivo;

    botao.classList.add('d-none');
}


function arrastandoSobre(event, cardConteudo) {
    event.preventDefault();
    event.stopPropagation();

    cardConteudo.classList.add('arrastando');
}


function arrastandoFora(event, cardConteudo) {
    event.preventDefault();
    event.stopPropagation();

    cardConteudo.classList.remove('arrastando');
}


async function soltouArquivo(event, cardConteudo) {
    event.preventDefault();
    event.stopPropagation();

    const arquivo = event.dataTransfer.files[0];

    cardConteudo.classList.remove('arrastando');

    if (!arquivo) return;

    const resultadoValidacao = await validarPlanilha(arquivo);

    if (!resultadoValidacao.valido) {
        alert(resultadoValidacao.mensagem);

        return;
    }

    mostrarArquivo(cardConteudo, arquivo);
}