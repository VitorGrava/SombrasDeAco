async function validarPlanilha(arquivo) {
    if (!arquivo) {
        return {
            valido: false,
            mensagem: 'Nenhum arquivo foi selecionado.'
        };
    }

    const extensao = pegarExtensaoArquivo(arquivo.name);

    if (extensao === 'csv') {
        return await validarCsv(arquivo);
    }

    if (extensao === 'xlsx') {
        return await validarXlsx(arquivo);
    }

    if (extensao === 'xls') {
        return {
            valido: false,
            mensagem: 'Arquivos .xls ainda não são suportados por esta validação. Salve a planilha como .xlsx.'
        };
    }

    return {
        valido: false,
        mensagem: 'Tipo de arquivo não permitido. Envie apenas .csv ou .xlsx.'
    };
}


function pegarExtensaoArquivo(nomeArquivo) {
    return nomeArquivo
        .toLowerCase()
        .split('.')
        .pop();
}


/* ── XLSX ── */
async function validarXlsx(arquivo) {
    try {
        const buffer = await arquivo.arrayBuffer();

        const workbook = new ExcelJS.Workbook();

        await workbook.xlsx.load(buffer);

        const primeiraAba = workbook.worksheets[0];

        if (!primeiraAba) {
            return {
                valido: false,
                mensagem: 'A planilha está vazia.'
            };
        }

        const planilhaVazia = verificarSeXlsxEstaVazio(primeiraAba);

        if (planilhaVazia) {
            return {
                valido: false,
                mensagem: 'A planilha está vazia.'
            };
        }

        const resultadoColuna = verificarColunaCnpjXlsx(primeiraAba);

        if (!resultadoColuna.encontrou) {
            return {
                valido: false,
                mensagem: 'A planilha não possui a coluna CNPJ.'
            };
        }

        return {
            valido: true,
            mensagem: 'Planilha válida.',
            tipo: 'xlsx',
            colunaCnpj: resultadoColuna.numeroColuna
        };

    } catch (erro) {
        return {
            valido: false,
            mensagem: 'Não foi possível ler o arquivo .xlsx.'
        };
    }
}


function verificarSeXlsxEstaVazio(worksheet) {
    let temAlgumValor = false;

    worksheet.eachRow(row => {
        row.eachCell(cell => {
            const valor = normalizarValorCelula(cell.value);

            if (valor !== '') {
                temAlgumValor = true;
            }
        });
    });

    return !temAlgumValor;
}


function verificarColunaCnpjXlsx(worksheet) {
    let encontrou = false;
    let numeroColuna = null;

    const primeiraLinha = worksheet.getRow(1);

    primeiraLinha.eachCell((cell, colNumber) => {
        const valor = normalizarTexto(cell.value);

        if (valor === 'cnpj') {
            encontrou = true;
            numeroColuna = colNumber;
        }
    });

    return {
        encontrou,
        numeroColuna
    };
}


/* ── CSV ── */
function validarCsv(arquivo) {
    return new Promise((resolve) => {
        Papa.parse(arquivo, {
            header: false,
            skipEmptyLines: false,

            complete: function(resultado) {
                const linhas = resultado.data || [];

                const csvVazio = verificarSeCsvEstaVazio(linhas);

                if (csvVazio) {
                    resolve({
                        valido: false,
                        mensagem: 'O arquivo CSV está vazio.'
                    });

                    return;
                }

                const resultadoColuna = verificarColunaCnpjCsv(linhas);

                if (!resultadoColuna.encontrou) {
                    resolve({
                        valido: false,
                        mensagem: 'O arquivo CSV não possui a coluna CNPJ.'
                    });

                    return;
                }

                resolve({
                    valido: true,
                    mensagem: 'Planilha válida.',
                    tipo: 'csv',
                    colunaCnpj: resultadoColuna.nomeColuna,
                    indiceColunaCnpj: resultadoColuna.indiceColuna
                });
            },

            error: function() {
                resolve({
                    valido: false,
                    mensagem: 'Não foi possível ler o arquivo CSV.'
                });
            }
        });
    });
}


function verificarSeCsvEstaVazio(linhas) {
    if (!linhas || linhas.length === 0) {
        return true;
    }

    const temAlgumValorReal = linhas.some(linha => {
        if (!Array.isArray(linha)) {
            return normalizarTexto(linha) !== '';
        }

        return linha.some(celula => {
            return normalizarTexto(celula) !== '';
        });
    });

    return !temAlgumValorReal;
}


function verificarColunaCnpjCsv(linhas) {
    const primeiraLinhaComConteudo = linhas.find(linha => {
        if (!Array.isArray(linha)) return false;

        return linha.some(celula => normalizarTexto(celula) !== '');
    });

    if (!primeiraLinhaComConteudo) {
        return {
            encontrou: false,
            nomeColuna: null,
            indiceColuna: null
        };
    }

    let encontrou = false;
    let nomeColuna = null;
    let indiceColuna = null;

    primeiraLinhaComConteudo.forEach((coluna, index) => {
        const valor = normalizarTexto(coluna);

        if (valor === 'cnpj') {
            encontrou = true;
            nomeColuna = coluna;
            indiceColuna = index;
        }
    });

    return {
        encontrou,
        nomeColuna,
        indiceColuna
    };
}


/* ── Funções auxiliares ── */
function normalizarTexto(valor) {
    if (valor === null || valor === undefined) {
        return '';
    }

    return String(valor)
        .trim()
        .toLowerCase();
}


function normalizarValorCelula(valor) {
    if (valor === null || valor === undefined) {
        return '';
    }

    if (typeof valor === 'object') {
        if (valor.text) {
            return normalizarTexto(valor.text);
        }

        if (valor.result) {
            return normalizarTexto(valor.result);
        }

        if (valor.richText) {
            return normalizarTexto(
                valor.richText.map(item => item.text).join('')
            );
        }

        return '';
    }

    return normalizarTexto(valor);
}