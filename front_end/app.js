const API_BASE_URL = "http://localhost:8080/api";

const STATUS = {
    ATIVO: 0,
    DEVOLVIDO: 1,
    ATRASADO: 2
};

document.addEventListener("DOMContentLoaded", () => {
    createFeedbackElements();

    const initializers = {
        dashboard: initDashboard,
        livros: initLivros,
        autores: initAutores,
        alunos: initAlunos,
        emprestimos: initEmprestimos
    };

    const initialize = initializers[document.body.dataset.page];
    if (initialize) initialize();
});

async function apiRequest(path, options = {}) {
    let response;
    try {
        response = await fetch(`${API_BASE_URL}${path}`, {
            ...options,
            headers: {
                ...(options.body ? { "Content-Type": "application/json" } : {}),
                ...options.headers
            }
        });
    } catch {
        throw new Error("Não foi possível conectar à API. Confirme se o back-end está em execução.");
    }

    if (!response.ok) {
        let problem;
        try {
            problem = await response.json();
        } catch {
            problem = null;
        }

        const validationErrors = problem?.errors
            ? Object.values(problem.errors).flat().join(" ")
            : "";
        const message = validationErrors || problem?.detail || problem?.title ||
            `Não foi possível concluir a operação (${response.status}).`;

        throw new Error(message);
    }

    return response.status === 204 ? null : response.json();
}

function createFeedbackElements() {
    const toast = document.createElement("div");
    toast.id = "app-toast";
    toast.className = "toast";
    toast.setAttribute("role", "status");
    toast.setAttribute("aria-live", "polite");
    document.body.appendChild(toast);

    const modal = document.createElement("div");
    modal.id = "confirm-modal";
    modal.className = "modal-backdrop";
    modal.hidden = true;
    modal.innerHTML = `
        <div class="modal" role="dialog" aria-modal="true" aria-labelledby="modal-title">
            <p class="eyebrow">Confirmação</p>
            <h2 id="modal-title" class="modal-title">Confirmar ação</h2>
            <p id="modal-message" class="modal-message"></p>
            <div class="modal-actions">
                <button id="modal-cancel" class="secondary-action" type="button">Cancelar</button>
                <button id="modal-confirm" class="danger-action" type="button">Confirmar</button>
            </div>
        </div>`;
    document.body.appendChild(modal);
}

let toastTimer;
function showToast(message, type = "success") {
    const toast = document.querySelector("#app-toast");
    clearTimeout(toastTimer);
    toast.textContent = message;
    toast.className = `toast toast-${type} toast-visible`;
    toastTimer = setTimeout(() => toast.classList.remove("toast-visible"), 4500);
}

function reportError(error) {
    console.error(error);
    showToast(error.message || "Não foi possível conectar à API.", "error");
}

function confirmAction(message, confirmLabel = "Confirmar") {
    const backdrop = document.querySelector("#confirm-modal");
    const messageElement = document.querySelector("#modal-message");
    const cancelButton = document.querySelector("#modal-cancel");
    const confirmButton = document.querySelector("#modal-confirm");

    messageElement.textContent = message;
    confirmButton.textContent = confirmLabel;
    backdrop.hidden = false;
    cancelButton.focus();

    return new Promise(resolve => {
        const finish = result => {
            backdrop.hidden = true;
            cancelButton.removeEventListener("click", cancel);
            confirmButton.removeEventListener("click", confirm);
            backdrop.removeEventListener("click", outside);
            document.removeEventListener("keydown", escape);
            resolve(result);
        };
        const cancel = () => finish(false);
        const confirm = () => finish(true);
        const outside = event => {
            if (event.target === backdrop) cancel();
        };
        const escape = event => {
            if (event.key === "Escape") cancel();
        };

        cancelButton.addEventListener("click", cancel);
        confirmButton.addEventListener("click", confirm);
        backdrop.addEventListener("click", outside);
        document.addEventListener("keydown", escape);
    });
}

function escapeHtml(value) {
    return String(value ?? "")
        .replaceAll("&", "&amp;")
        .replaceAll("<", "&lt;")
        .replaceAll(">", "&gt;")
        .replaceAll('"', "&quot;")
        .replaceAll("'", "&#039;");
}

function formatDate(value) {
    if (!value) return "—";
    return new Intl.DateTimeFormat("pt-BR", {
        day: "2-digit",
        month: "short",
        year: "numeric"
    }).format(new Date(value));
}

function isOverdue(loan) {
    return Number(loan.status) === STATUS.ATIVO &&
        new Date(loan.dataPrevistaDevolucao).getTime() < Date.now();
}

function getLoanStatus(loan) {
    if (Number(loan.status) === STATUS.DEVOLVIDO) {
        return { label: "Devolvido", className: "status-returned", key: "devolvido" };
    }
    if (Number(loan.status) === STATUS.ATRASADO || isOverdue(loan)) {
        return { label: "Atrasado", className: "status-overdue", key: "atrasado" };
    }
    return { label: "Ativo", className: "status-active", key: "ativo" };
}

function setLoading(tbody, columns) {
    tbody.innerHTML = `<tr><td colspan="${columns}" class="table-message">Carregando dados...</td></tr>`;
}

function setEmpty(tbody, columns, message) {
    tbody.innerHTML = `<tr><td colspan="${columns}" class="table-message">${escapeHtml(message)}</td></tr>`;
}

function setButtonBusy(button, busy, busyText = "Salvando...") {
    if (busy) {
        button.dataset.originalText = button.textContent;
        button.textContent = busyText;
        button.disabled = true;
    } else {
        button.textContent = button.dataset.originalText || button.textContent;
        button.disabled = false;
    }
}

async function initDashboard() {
    try {
        const [livros, alunos, emprestimos] = await Promise.all([
            apiRequest("/livros"),
            apiRequest("/alunos"),
            apiRequest("/emprestimos")
        ]);

        const alunosMap = new Map(alunos.map(aluno => [aluno.id, aluno]));
        const livrosMap = new Map(livros.map(livro => [livro.id, livro]));
        const ativos = emprestimos.filter(item => Number(item.status) === STATUS.ATIVO);
        const atrasados = ativos.filter(isOverdue);

        document.querySelector("#metric-livros").textContent = livros.length;
        document.querySelector("#metric-alunos").textContent = alunos.length;
        document.querySelector("#metric-ativos").textContent = ativos.length;
        document.querySelector("#metric-atrasados").textContent = atrasados.length;

        const recentBody = document.querySelector("#recent-loans-body");
        const recentes = [...emprestimos]
            .sort((a, b) => new Date(b.dataEmprestimo) - new Date(a.dataEmprestimo))
            .slice(0, 5);

        if (!recentes.length) {
            setEmpty(recentBody, 4, "Nenhum empréstimo registrado.");
        } else {
            recentBody.innerHTML = recentes.map(loan => {
                const aluno = alunosMap.get(loan.alunoId);
                const livro = livrosMap.get(loan.livroId);
                const status = getLoanStatus(loan);
                return `<tr>
                    <td><span class="cell-title">${escapeHtml(aluno?.nome || `Aluno #${loan.alunoId}`)}</span><span class="cell-detail">${escapeHtml(aluno?.matricula || "Cadastro indisponível")}</span></td>
                    <td>${escapeHtml(livro?.titulo || `Livro #${loan.livroId}`)}</td>
                    <td>${formatDate(loan.dataPrevistaDevolucao)}</td>
                    <td><span class="status ${status.className}">${status.label}</span></td>
                </tr>`;
            }).join("");
        }

        const stockList = document.querySelector("#low-stock-list");
        const estoqueBaixo = livros.filter(livro => livro.quantidade <= 1)
            .sort((a, b) => a.quantidade - b.quantidade);
        stockList.innerHTML = estoqueBaixo.length
            ? estoqueBaixo.map(livro => `<li class="stock-item"><span><span class="cell-title">${escapeHtml(livro.titulo)}</span><span class="cell-detail">${escapeHtml(livro.autorNome)}</span></span><span class="stock-count">${livro.quantidade}</span></li>`).join("")
            : '<li class="table-message">Nenhum livro com estoque reduzido.</li>';
    } catch (error) {
        reportError(error);
        document.querySelectorAll(".metric-value").forEach(item => item.textContent = "—");
        setEmpty(document.querySelector("#recent-loans-body"), 4, "Não foi possível carregar os dados.");
        document.querySelector("#low-stock-list").innerHTML = '<li class="table-message">Não foi possível carregar o estoque.</li>';
    }
}

async function initLivros() {
    const form = document.querySelector("#book-form");
    const filterForm = document.querySelector("#book-filter-form");
    const authorSelect = document.querySelector("#autor");
    const submitButton = form.querySelector('[type="submit"]');
    const tbody = document.querySelector("#books-body");
    let editingId = null;
    let currentBooks = [];

    function resetForm() {
        editingId = null;
        form.reset();
        document.querySelector("#book-form-title").textContent = "Cadastrar livro";
        submitButton.textContent = "Cadastrar livro";
    }

    async function loadAuthors() {
        const autores = await apiRequest("/autores");
        authorSelect.innerHTML = '<option value="">Selecione um autor</option>' +
            autores.map(autor => `<option value="${autor.id}">${escapeHtml(autor.nome)}</option>`).join("");
    }

    async function loadBooks() {
        setLoading(tbody, 5);
        try {
            const params = new URLSearchParams();
            const titulo = document.querySelector("#filtro-titulo").value.trim();
            const autor = document.querySelector("#filtro-autor").value.trim();
            if (titulo) params.set("titulo", titulo);
            if (autor) params.set("autor", autor);
            const query = params.toString() ? `?${params}` : "";
            const livros = await apiRequest(`/livros${query}`);
            currentBooks = livros;
            document.querySelector("#book-count").textContent = `${livros.length} ${livros.length === 1 ? "título encontrado" : "títulos encontrados"}`;
            if (!livros.length) return setEmpty(tbody, 5, "Nenhum livro encontrado.");
            tbody.innerHTML = livros.map(livro => `<tr>
                <td><span class="cell-title">${escapeHtml(livro.titulo)}</span><span class="cell-detail">ISBN ${escapeHtml(livro.isbn)}</span></td>
                <td>${escapeHtml(livro.autorNome)}</td>
                <td>${livro.anoPublicacao}</td>
                <td><span class="status ${livro.quantidade === 0 ? "status-low" : "status-active"}">${livro.quantidade} ${livro.quantidade === 1 ? "unidade" : "unidades"}</span></td>
                <td><button class="text-action" type="button" data-edit-book="${livro.id}">Editar</button></td>
            </tr>`).join("");
        } catch (error) {
            reportError(error);
            setEmpty(tbody, 5, "Não foi possível carregar o acervo.");
        }
    }

    tbody.addEventListener("click", event => {
        const editButton = event.target.closest("[data-edit-book]");
        if (!editButton) return;
        const livro = currentBooks.find(item => item.id === Number(editButton.dataset.editBook));
        if (!livro) return;

        editingId = livro.id;
        document.querySelector("#titulo").value = livro.titulo;
        document.querySelector("#isbn").value = livro.isbn;
        authorSelect.value = String(livro.autorId);
        document.querySelector("#ano").value = livro.anoPublicacao;
        document.querySelector("#quantidade").value = livro.quantidade;
        document.querySelector("#book-form-title").textContent = "Editar livro";
        submitButton.textContent = "Salvar alterações";
        form.scrollIntoView({ behavior: "smooth", block: "center" });
    });

    filterForm.addEventListener("submit", event => {
        event.preventDefault();
        loadBooks();
    });
    document.querySelector("#clear-book-filters").addEventListener("click", () => {
        filterForm.reset();
        loadBooks();
    });
    form.addEventListener("submit", async event => {
        event.preventDefault();
        const wasEditing = editingId !== null;
        setButtonBusy(submitButton, true);
        try {
            await apiRequest(wasEditing ? `/livros/${editingId}` : "/livros", {
                method: wasEditing ? "PUT" : "POST",
                body: JSON.stringify({
                    titulo: document.querySelector("#titulo").value.trim(),
                    isbn: document.querySelector("#isbn").value.trim(),
                    autorId: Number(authorSelect.value),
                    anoPublicacao: Number(document.querySelector("#ano").value),
                    quantidade: Number(document.querySelector("#quantidade").value)
                })
            });
            showToast(wasEditing ? "Livro atualizado com sucesso." : "Livro cadastrado com sucesso.");
            resetForm();
            await loadBooks();
        } catch (error) {
            reportError(error);
        } finally {
            setButtonBusy(submitButton, false);
            if (editingId === null) submitButton.textContent = "Cadastrar livro";
        }
    });
    document.querySelector("#clear-book-form").addEventListener("click", resetForm);

    try {
        await loadAuthors();
        await loadBooks();
    } catch (error) {
        reportError(error);
    }
}

async function initAutores() {
    const form = document.querySelector("#author-form");
    const tbody = document.querySelector("#authors-body");
    const submitButton = form.querySelector('[type="submit"]');
    let editingId = null;
    let currentAuthors = [];

    function resetForm() {
        editingId = null;
        form.reset();
        document.querySelector("#author-form-title").textContent = "Cadastrar autor";
        submitButton.textContent = "Cadastrar autor";
    }

    async function loadAuthors() {
        setLoading(tbody, 5);
        try {
            const [autores, livros] = await Promise.all([apiRequest("/autores"), apiRequest("/livros")]);
            currentAuthors = autores;
            document.querySelector("#author-count").textContent = `${autores.length} ${autores.length === 1 ? "autor cadastrado" : "autores cadastrados"}`;
            if (!autores.length) return setEmpty(tbody, 5, "Nenhum autor cadastrado.");
            const counts = livros.reduce((map, livro) => map.set(livro.autorId, (map.get(livro.autorId) || 0) + 1), new Map());
            tbody.innerHTML = autores.map(autor => `<tr>
                <td><span class="cell-title">${escapeHtml(autor.nome)}</span><span class="cell-detail">ID ${autor.id}</span></td>
                <td>${formatDate(autor.dataNascimento)}</td>
                <td>${escapeHtml(autor.nacionalidade)}</td>
                <td>${counts.get(autor.id) || 0} título(s)</td>
                <td class="actions-cell"><button class="text-action" type="button" data-edit-author="${autor.id}">Editar</button><button class="text-action danger-text" type="button" data-delete-author="${autor.id}">Excluir</button></td>
            </tr>`).join("");
        } catch (error) {
            reportError(error);
            setEmpty(tbody, 5, "Não foi possível carregar os autores.");
        }
    }

    tbody.addEventListener("click", async event => {
        const editButton = event.target.closest("[data-edit-author]");
        const deleteButton = event.target.closest("[data-delete-author]");
        if (editButton) {
            const autor = currentAuthors.find(item => item.id === Number(editButton.dataset.editAuthor));
            if (!autor) return;
            editingId = autor.id;
            document.querySelector("#nome").value = autor.nome;
            document.querySelector("#nascimento").value = autor.dataNascimento.slice(0, 10);
            document.querySelector("#nacionalidade").value = autor.nacionalidade;
            document.querySelector("#author-form-title").textContent = "Editar autor";
            submitButton.textContent = "Salvar alterações";
            form.scrollIntoView({ behavior: "smooth", block: "center" });
        }
        if (deleteButton) {
            const id = Number(deleteButton.dataset.deleteAuthor);
            const autor = currentAuthors.find(item => item.id === id);
            const confirmed = await confirmAction(`Excluir o autor “${autor?.nome || id}”? Esta ação não pode ser desfeita.`, "Excluir autor");
            if (!confirmed) return;
            try {
                await apiRequest(`/autores/${id}`, { method: "DELETE" });
                showToast("Autor excluído com sucesso.");
                if (editingId === id) resetForm();
                await loadAuthors();
            } catch (error) {
                reportError(error);
            }
        }
    });

    form.addEventListener("submit", async event => {
        event.preventDefault();
        setButtonBusy(submitButton, true);
        try {
            const payload = {
                nome: document.querySelector("#nome").value.trim(),
                dataNascimento: document.querySelector("#nascimento").value,
                nacionalidade: document.querySelector("#nacionalidade").value.trim()
            };
            await apiRequest(editingId ? `/autores/${editingId}` : "/autores", {
                method: editingId ? "PUT" : "POST",
                body: JSON.stringify(payload)
            });
            showToast(editingId ? "Autor atualizado com sucesso." : "Autor cadastrado com sucesso.");
            resetForm();
            await loadAuthors();
        } catch (error) {
            reportError(error);
        } finally {
            setButtonBusy(submitButton, false);
            if (!editingId) submitButton.textContent = "Cadastrar autor";
        }
    });
    document.querySelector("#clear-author-form").addEventListener("click", resetForm);
    await loadAuthors();
}

async function initAlunos() {
    const form = document.querySelector("#student-form");
    const tbody = document.querySelector("#students-body");
    const emailInput = document.querySelector("#email");
    let currentStudents = [];

    function validateEmailDomain() {
        const email = emailInput.value.trim().toLowerCase();
        const allowed = !email || email.endsWith("@gmail.com") || email.endsWith("@ifpe.edu.br");
        emailInput.setCustomValidity(allowed
            ? ""
            : "Use um e-mail com domínio @gmail.com ou @ifpe.edu.br.");
        return allowed;
    }

    emailInput.addEventListener("input", validateEmailDomain);

    async function loadStudents() {
        setLoading(tbody, 5);
        try {
            const [alunos, emprestimos] = await Promise.all([apiRequest("/alunos"), apiRequest("/emprestimos")]);
            currentStudents = alunos;
            document.querySelector("#student-count").textContent = `${alunos.length} ${alunos.length === 1 ? "aluno cadastrado" : "alunos cadastrados"}`;
            if (!alunos.length) return setEmpty(tbody, 5, "Nenhum aluno cadastrado.");
            tbody.innerHTML = alunos.map(aluno => {
                const loans = emprestimos.filter(item => item.alunoId === aluno.id);
                const overdue = loans.filter(isOverdue).length;
                const active = loans.filter(item => Number(item.status) === STATUS.ATIVO && !isOverdue(item)).length;
                const badge = overdue
                    ? `<span class="status status-overdue">${overdue} atrasado(s)</span>`
                    : active
                        ? `<span class="status status-active">${active} ativo(s)</span>`
                        : loans.length
                            ? '<span class="status status-returned">Histórico</span>'
                            : '<span class="muted-text">Nenhum</span>';
                return `<tr>
                    <td><span class="cell-title">${escapeHtml(aluno.nome)}</span><span class="cell-detail">ID ${aluno.id}</span></td>
                    <td>${escapeHtml(aluno.matricula)}</td>
                    <td>${escapeHtml(aluno.email)}</td>
                    <td>${badge}</td>
                    <td><button class="text-action danger-text" type="button" data-delete-student="${aluno.id}">Excluir</button></td>
                </tr>`;
            }).join("");
        } catch (error) {
            reportError(error);
            setEmpty(tbody, 5, "Não foi possível carregar os alunos.");
        }
    }

    tbody.addEventListener("click", async event => {
        const button = event.target.closest("[data-delete-student]");
        if (!button) return;
        const id = Number(button.dataset.deleteStudent);
        const aluno = currentStudents.find(item => item.id === id);
        const confirmed = await confirmAction(`Excluir o aluno “${aluno?.nome || id}”? Esta ação não pode ser desfeita.`, "Excluir aluno");
        if (!confirmed) return;
        try {
            await apiRequest(`/alunos/${id}`, { method: "DELETE" });
            showToast("Aluno excluído com sucesso.");
            await loadStudents();
        } catch (error) {
            reportError(error);
        }
    });

    form.addEventListener("submit", async event => {
        event.preventDefault();
        if (!validateEmailDomain()) {
            emailInput.reportValidity();
            return;
        }
        const button = form.querySelector('[type="submit"]');
        setButtonBusy(button, true);
        try {
            await apiRequest("/alunos", {
                method: "POST",
                body: JSON.stringify({
                    nome: document.querySelector("#nome").value.trim(),
                    matricula: document.querySelector("#matricula").value.trim(),
                    email: document.querySelector("#email").value.trim()
                })
            });
            form.reset();
            showToast("Aluno cadastrado com sucesso.");
            await loadStudents();
        } catch (error) {
            reportError(error);
        } finally {
            setButtonBusy(button, false);
        }
    });
    document.querySelector("#clear-student-form").addEventListener("click", () => {
        form.reset();
        emailInput.setCustomValidity("");
    });
    await loadStudents();
}

async function initEmprestimos() {
    const form = document.querySelector("#loan-form");
    const filterForm = document.querySelector("#loan-filter-form");
    const tbody = document.querySelector("#loans-body");
    let data = { alunos: [], livros: [], emprestimos: [] };

    function renderLoans() {
        const search = document.querySelector("#busca").value.trim().toLocaleLowerCase("pt-BR");
        const selectedStatus = document.querySelector("#status").value;
        const alunosMap = new Map(data.alunos.map(item => [item.id, item]));
        const livrosMap = new Map(data.livros.map(item => [item.id, item]));
        const filtered = data.emprestimos.filter(loan => {
            const aluno = alunosMap.get(loan.alunoId);
            const livro = livrosMap.get(loan.livroId);
            const matchesSearch = !search || aluno?.nome.toLocaleLowerCase("pt-BR").includes(search) ||
                aluno?.matricula.toLocaleLowerCase("pt-BR").includes(search) ||
                livro?.titulo.toLocaleLowerCase("pt-BR").includes(search);
            const matchesStatus = !selectedStatus || getLoanStatus(loan).key === selectedStatus;
            return matchesSearch && matchesStatus;
        }).sort((a, b) => new Date(b.dataEmprestimo) - new Date(a.dataEmprestimo));

        const activeCount = data.emprestimos.filter(item => Number(item.status) === STATUS.ATIVO && !isOverdue(item)).length;
        const overdueCount = data.emprestimos.filter(isOverdue).length;
        document.querySelector("#loan-count").textContent = `${activeCount} ativo(s) e ${overdueCount} em atraso`;
        if (!filtered.length) return setEmpty(tbody, 6, "Nenhum empréstimo encontrado.");

        tbody.innerHTML = filtered.map(loan => {
            const aluno = alunosMap.get(loan.alunoId);
            const livro = livrosMap.get(loan.livroId);
            const status = getLoanStatus(loan);
            const action = Number(loan.status) === STATUS.DEVOLVIDO
                ? `<span class="muted-text">Devolvido em ${formatDate(loan.dataDevolucao)}</span>`
                : `<button class="text-action" type="button" data-return-loan="${loan.id}">Devolver</button>`;
            return `<tr>
                <td><span class="cell-title">${escapeHtml(aluno?.nome || `Aluno #${loan.alunoId}`)}</span><span class="cell-detail">${escapeHtml(aluno?.matricula || "")}</span></td>
                <td>${escapeHtml(livro?.titulo || `Livro #${loan.livroId}`)}</td>
                <td>${formatDate(loan.dataEmprestimo)}</td>
                <td>${formatDate(loan.dataPrevistaDevolucao)}</td>
                <td><span class="status ${status.className}">${status.label}</span></td>
                <td>${action}</td>
            </tr>`;
        }).join("");
    }

    async function loadLoans() {
        setLoading(tbody, 6);
        try {
            const [alunos, livros, emprestimos] = await Promise.all([
                apiRequest("/alunos"), apiRequest("/livros"), apiRequest("/emprestimos")
            ]);
            data = { alunos, livros, emprestimos };
            document.querySelector("#aluno").innerHTML = '<option value="">Selecione um aluno</option>' +
                alunos.map(item => `<option value="${item.id}">${escapeHtml(item.nome)} — ${escapeHtml(item.matricula)}</option>`).join("");
            const disponiveis = livros.filter(item => item.quantidade > 0);
            document.querySelector("#livro").innerHTML = '<option value="">Selecione um livro</option>' +
                disponiveis.map(item => `<option value="${item.id}">${escapeHtml(item.titulo)} — ${item.quantidade} disponível(is)</option>`).join("");
            renderLoans();
        } catch (error) {
            reportError(error);
            setEmpty(tbody, 6, "Não foi possível carregar os empréstimos.");
        }
    }

    filterForm.addEventListener("submit", event => {
        event.preventDefault();
        renderLoans();
    });
    document.querySelector("#clear-loan-filters").addEventListener("click", () => {
        filterForm.reset();
        renderLoans();
    });
    tbody.addEventListener("click", async event => {
        const button = event.target.closest("[data-return-loan]");
        if (!button) return;
        const confirmed = await confirmAction("Confirmar a devolução deste livro? O exemplar voltará ao estoque.", "Registrar devolução");
        if (!confirmed) return;
        try {
            await apiRequest(`/emprestimos/${button.dataset.returnLoan}/devolucao`, { method: "PUT" });
            showToast("Devolução registrada com sucesso.");
            await loadLoans();
        } catch (error) {
            reportError(error);
        }
    });
    form.addEventListener("submit", async event => {
        event.preventDefault();
        const button = form.querySelector('[type="submit"]');
        setButtonBusy(button, true, "Registrando...");
        try {
            await apiRequest("/emprestimos", {
                method: "POST",
                body: JSON.stringify({
                    alunoId: Number(document.querySelector("#aluno").value),
                    livroId: Number(document.querySelector("#livro").value)
                })
            });
            form.reset();
            showToast("Empréstimo registrado com sucesso.");
            await loadLoans();
        } catch (error) {
            reportError(error);
        } finally {
            setButtonBusy(button, false);
        }
    });
    document.querySelector("#clear-loan-form").addEventListener("click", () => form.reset());
    await loadLoans();
}
