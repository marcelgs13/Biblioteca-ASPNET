const API_BASE_URL = "http://localhost:5000/api";
const AUTH_STORAGE_KEY = "smartlib_auth";

const STATUS = {
    ATIVO: 0,
    DEVOLVIDO: 1,
    ATRASADO: 2
};

document.addEventListener("DOMContentLoaded", () => {
    createFeedbackElements();

    const initializers = {
        login: initLogin,
        dashboard: initDashboard,
        livros: initLivros,
        autores: initAutores,
        alunos: initAlunos,
        emprestimos: initEmprestimos,
        reservas: initReservas,
        notificacoes: initNotificacoes,
        bibliotecarios: initBibliotecarios,
        relatorios: initRelatorios,
        auditoria: initAuditoria
    };

    const initialize = initializers[document.body.dataset.page];
    if (document.body.dataset.page !== "login" && !getAuth()) {
        window.location.href = "login.html";
        return;
    }
    if (document.body.dataset.page !== "login") setupSession();
    if (getAuth()?.perfil === "ALUNO" && ["alunos", "autores"].includes(document.body.dataset.page)) {
        window.location.replace(document.body.dataset.page === "autores" ? "livros.html" : "index.html");
        return;
    }
    if (["bibliotecarios", "relatorios", "auditoria"].includes(document.body.dataset.page) && getAuth()?.perfil !== "ADMIN") {
        window.location.replace("index.html");
        return;
    }
    if (initialize) initialize();
});

function getAuth() {
    try { return JSON.parse(localStorage.getItem(AUTH_STORAGE_KEY)); }
    catch { return null; }
}

function setupSession() {
    const auth = getAuth();
    document.querySelectorAll(".sidebar-nav").forEach(nav => {
        nav.insertAdjacentHTML("beforeend", `
            <a class="nav-link" href="reservas.html"><span class="nav-index">06</span>Solicitações</a>`);
        if (auth?.perfil === "ADMIN") {
            nav.insertAdjacentHTML("beforeend", `
                <a class="nav-link" href="bibliotecarios.html"><span class="nav-index">07</span>Bibliotecários</a>
                <a class="nav-link" href="relatorios.html"><span class="nav-index">08</span>Relatórios</a>
                <a class="nav-link" href="auditoria.html"><span class="nav-index">09</span>Auditoria</a>`);
        }
    });
    const currentPage = window.location.pathname.split("/").pop() || "index.html";
    document.querySelectorAll(".sidebar-nav .nav-link").forEach(link => {
        if (link.getAttribute("href") === currentPage) link.setAttribute("aria-current", "page");
        else link.removeAttribute("aria-current");
    });
    document.querySelectorAll(".sidebar-footer").forEach(footer => {
        footer.innerHTML = `<strong>${escapeHtml(auth?.nome)}</strong><br>${escapeHtml(auth?.perfil)}<br><button id="logout-button" class="text-action" type="button">Sair</button>`;
    });
    document.querySelector("#logout-button")?.addEventListener("click", () => {
        localStorage.removeItem(AUTH_STORAGE_KEY);
        window.location.href = "login.html";
    });
    document.body.dataset.role = auth?.perfil || "";
    if (auth?.perfil === "ALUNO") {
        document.querySelectorAll('a[href="alunos.html"], a[href="autores.html"]').forEach(link => link.hidden = true);
        document.querySelectorAll('.sidebar-nav a[href="livros.html"]').forEach(link => {
            const index = link.querySelector(".nav-index")?.outerHTML || "";
            link.innerHTML = `${index}Catálogo`;
        });
        document.querySelectorAll('.page-header .primary-action[href="emprestimos.html"]').forEach(link => {
            link.hidden = false;
            link.href = "livros.html";
            link.textContent = "Explorar catálogo";
        });
        document.querySelectorAll('.sidebar-nav a[href="emprestimos.html"]').forEach(link => {
            const index = link.querySelector(".nav-index")?.outerHTML || "";
            link.innerHTML = `${index}Meu histórico`;
        });
        document.querySelectorAll(".topline").forEach(topline => {
            const actions = document.createElement("div");
            actions.className = "topline-actions";
            const connectionStatus = topline.querySelector(".demo-label");
            if (connectionStatus) actions.appendChild(connectionStatus);
            actions.insertAdjacentHTML("beforeend", `
                <a class="notification-bell" href="notificacoes.html"
                   aria-label="Abrir notificações" title="Notificações"
                   ${currentPage === "notificacoes.html" ? 'aria-current="page"' : ""}>
                    <svg viewBox="0 0 24 24" aria-hidden="true">
                        <path d="M18 8a6 6 0 0 0-12 0c0 7-3 7-3 9h18c0-2-3-2-3-9M10 21h4" />
                    </svg>
                </a>`);
            topline.appendChild(actions);
        });
    }
}

async function initLogin() {
    if (getAuth()) {
        window.location.href = "index.html";
        return;
    }
    const form = document.querySelector("#login-form");
    form.addEventListener("submit", async event => {
        event.preventDefault();
        const button = form.querySelector('[type="submit"]');
        setButtonBusy(button, true, "Entrando...");
        try {
            const auth = await apiRequest("/auth/login", {
                method: "POST",
                body: JSON.stringify({
                    email: document.querySelector("#login-email").value.trim(),
                    senha: document.querySelector("#login-password").value
                })
            });
            localStorage.setItem(AUTH_STORAGE_KEY, JSON.stringify(auth));
            window.location.href = "index.html";
        } catch (error) {
            reportError(error);
            setButtonBusy(button, false);
        }
    });
}

async function apiRequest(path, options = {}) {
    const auth = getAuth();
    let response;
    try {
        response = await fetch(`${API_BASE_URL}${path}`, {
            ...options,
            headers: {
                ...(options.body ? { "Content-Type": "application/json" } : {}),
                ...(auth?.token ? { Authorization: `Bearer ${auth.token}` } : {}),
                ...options.headers
            }
        });
    } catch {
        throw new Error("Não foi possível conectar à API. Confirme se o back-end está em execução.");
    }

    if (!response.ok) {
        if (response.status === 401 && document.body.dataset.page !== "login") {
            localStorage.removeItem(AUTH_STORAGE_KEY);
            window.location.href = "login.html";
            throw new Error("Sua sessão expirou. Entre novamente.");
        }
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
                <button id="modal-cancel" class="secondary-action" type="button">Voltar</button>
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

async function getBooks(path = "/livros?pageSize=100") {
    const response = await apiRequest(path);
    return response.items || [];
}

function renderBarChart(selector, items, emptyMessage) {
    const container = document.querySelector(selector);
    if (!items.length || items.every(item => item.value === 0)) {
        container.classList.remove("column-chart");
        container.innerHTML = `<p class="chart-empty">${escapeHtml(emptyMessage)}</p>`;
        return;
    }

    const maximum = Math.max(...items.map(item => item.value), 1);
    container.classList.add("column-chart");
    container.innerHTML = items.map(item => {
        const percentage = item.value === 0 ? 0 : Math.max((item.value / maximum) * 100, 4);
        return `<div class="chart-column" title="${escapeHtml(item.label)}: ${item.value}">
            <strong class="chart-value">${item.value}</strong>
            <div class="chart-column-track" aria-hidden="true"><span class="chart-bar${item.tone ? ` chart-bar-${item.tone}` : ""}" style="height: ${percentage}%"></span></div>
            <span class="chart-label">${escapeHtml(item.label)}</span>
        </div>`;
    }).join("");
}

function renderAdminAnalytics(livros, emprestimos) {
    const livrosMap = new Map(livros.map(livro => [livro.id, livro]));
    const emprestimosPorLivro = new Map();
    emprestimos.forEach(item => emprestimosPorLivro.set(item.livroId, (emprestimosPorLivro.get(item.livroId) || 0) + 1));
    const populares = [...emprestimosPorLivro.entries()]
        .map(([livroId, value]) => ({ label: livrosMap.get(livroId)?.titulo || `Livro #${livroId}`, value }))
        .sort((a, b) => b.value - a.value)
        .slice(0, 5);
    renderBarChart("#chart-popular-books", populares, "Ainda não existem empréstimos para comparar.");

    const categoriasMap = new Map();
    livros.forEach(livro => {
        const categoria = livro.categoria?.trim() || "Sem categoria";
        categoriasMap.set(categoria, (categoriasMap.get(categoria) || 0) + 1);
    });
    const categorias = [...categoriasMap.entries()]
        .map(([label, value]) => ({ label, value }))
        .sort((a, b) => b.value - a.value)
        .slice(0, 6);
    renderBarChart("#chart-categories", categorias, "Nenhuma categoria cadastrada.");

    const monthFormatter = new Intl.DateTimeFormat("pt-BR", { month: "short", year: "2-digit" });
    const today = new Date();
    const meses = Array.from({ length: 6 }, (_, index) => {
        const date = new Date(today.getFullYear(), today.getMonth() - (5 - index), 1);
        return {
            key: `${date.getFullYear()}-${date.getMonth()}`,
            label: monthFormatter.format(date).replace(" de ", "/"),
            value: 0
        };
    });
    const mesesMap = new Map(meses.map(item => [item.key, item]));
    emprestimos.forEach(item => {
        const date = new Date(item.dataEmprestimo);
        const month = mesesMap.get(`${date.getFullYear()}-${date.getMonth()}`);
        if (month) month.value += 1;
    });
    renderBarChart("#chart-monthly-loans", meses, "Nenhum empréstimo registrado nos últimos seis meses.");

    const activeOnTime = emprestimos.filter(item => Number(item.status) === STATUS.ATIVO && !isOverdue(item)).length;
    const activeOverdue = emprestimos.filter(item => Number(item.status) === STATUS.ATRASADO || isOverdue(item)).length;
    const returnedLate = emprestimos.filter(item => Number(item.status) === STATUS.DEVOLVIDO &&
        item.dataDevolucao && new Date(item.dataDevolucao) > new Date(item.dataPrevistaDevolucao)).length;
    const returnedOnTime = emprestimos.filter(item => Number(item.status) === STATUS.DEVOLVIDO &&
        (!item.dataDevolucao || new Date(item.dataDevolucao) <= new Date(item.dataPrevistaDevolucao))).length;
    renderBarChart("#chart-deadlines", [
        { label: "Ativos no prazo", value: activeOnTime },
        { label: "Ativos em atraso", value: activeOverdue, tone: "danger" },
        { label: "Devolvidos no prazo", value: returnedOnTime },
        { label: "Devolvidos com atraso", value: returnedLate, tone: "danger" }
    ], "Ainda não existem empréstimos para analisar.");
}

async function initDashboard() {
    try {
        const auth = getAuth();
        const isAluno = auth.perfil === "ALUNO";
        const isAdmin = auth.perfil === "ADMIN";
        const [livros, emprestimos, reservas, totalUsuarios] = await Promise.all([
            getBooks(),
            apiRequest("/emprestimos"),
            isAluno ? apiRequest("/reservas") : Promise.resolve([]),
            isAdmin ? apiRequest("/usuarios/total") : Promise.resolve(null)
        ]);
        const alunos = isAluno
            ? [{ id: auth.alunoId, nome: auth.nome, matricula: "" }]
            : await apiRequest("/alunos");

        const alunosMap = new Map(alunos.map(aluno => [aluno.id, aluno]));
        const livrosMap = new Map(livros.map(livro => [livro.id, livro]));
        const ativos = emprestimos.filter(item => Number(item.status) === STATUS.ATIVO);
        const atrasados = emprestimos.filter(item => Number(item.status) === STATUS.ATRASADO || isOverdue(item));

        document.querySelector("#metric-livros").textContent = livros.length;
        document.querySelector("#metric-alunos").textContent = isAluno
            ? reservas.filter(item => Number(item.status) === 0 || Number(item.status) === 1).length
            : isAdmin ? totalUsuarios.total : alunos.length;
        document.querySelector("#metric-ativos").textContent = ativos.length;
        document.querySelector("#metric-atrasados").textContent = atrasados.length;

        if (isAluno) {
            document.querySelector(".eyebrow").textContent = "Área do aluno";
            document.querySelector(".page-title").textContent = `Olá, ${auth.nome}.`;
            document.querySelector(".page-description").textContent = "Consulte o catálogo, acompanhe seus empréstimos e gerencie suas solicitações em um só lugar.";
            document.querySelector("#metric-livros").closest(".metric-card").querySelector(".metric-label").textContent = "Livros no catálogo";
            document.querySelector("#metric-livros").closest(".metric-card").querySelector(".metric-note").textContent = "Títulos disponíveis para consulta";
            document.querySelector("#metric-secondary-label").textContent = "Solicitações ativas";
            document.querySelector("#metric-secondary-note").textContent = "Solicitações aguardando atendimento";
            document.querySelector("#metric-ativos").closest(".metric-card").querySelector(".metric-label").textContent = "Meus empréstimos ativos";
            document.querySelector("#metric-atrasados").closest(".metric-card").querySelector(".metric-label").textContent = "Meus atrasos";
            document.querySelector("#recent-panel-title").textContent = "Meus empréstimos recentes";
            document.querySelector("#recent-panel-subtitle").textContent = "Seu histórico de retiradas e devoluções";
            document.querySelector("#recent-student-column").hidden = true;
            document.querySelector("#catalog-panel-title").textContent = "Disponíveis no catálogo";
            document.querySelector("#catalog-panel-subtitle").textContent = "Livros disponíveis para você";
            document.querySelector(".quick-links").innerHTML = `
                <a class="quick-link" href="livros.html">Catálogo</a>
                <a class="quick-link" href="emprestimos.html">Meu histórico</a>
                <a class="quick-link" href="reservas.html">Solicitações</a>`;
        }

        if (isAdmin) {
            document.querySelector(".eyebrow").textContent = "Visão administrativa";
            document.querySelector(".page-title").textContent = "Indicadores da biblioteca";
            document.querySelector(".page-description").textContent = "Acompanhe o desempenho do acervo, a circulação e os prazos em uma visão executiva.";
            document.querySelector("#metric-secondary-label").textContent = "Usuários cadastrados";
            document.querySelector("#metric-secondary-note").textContent = "Contas com acesso ao sistema";
            const primaryAction = document.querySelector("#dashboard-primary-action");
            primaryAction.textContent = "Cadastrar bibliotecário";
            primaryAction.href = "bibliotecarios.html";
            document.querySelector(".dashboard-grid").hidden = true;
            document.querySelector("#admin-analytics").hidden = false;
            renderAdminAnalytics(livros, emprestimos);
        }

        const recentBody = document.querySelector("#recent-loans-body");
        const recentes = [...emprestimos]
            .sort((a, b) => new Date(b.dataEmprestimo) - new Date(a.dataEmprestimo))
            .slice(0, 5);

        if (!recentes.length) {
            setEmpty(recentBody, isAluno ? 3 : 4, isAluno ? "Você ainda não possui empréstimos." : "Nenhum empréstimo registrado.");
        } else {
            recentBody.innerHTML = recentes.map(loan => {
                const aluno = alunosMap.get(loan.alunoId);
                const livro = livrosMap.get(loan.livroId);
                const status = getLoanStatus(loan);
                return `<tr>
                    ${isAluno ? "" : `<td><span class="cell-title">${escapeHtml(aluno?.nome || `Aluno #${loan.alunoId}`)}</span><span class="cell-detail">${escapeHtml(aluno?.matricula || "Cadastro indisponível")}</span></td>`}
                    <td>${escapeHtml(livro?.titulo || `Livro #${loan.livroId}`)}</td>
                    <td>${formatDate(loan.dataPrevistaDevolucao)}</td>
                    <td><span class="status ${status.className}">${status.label}</span></td>
                </tr>`;
            }).join("");
        }

        const stockList = document.querySelector("#low-stock-list");
        const livrosDoPainel = isAluno
            ? livros.filter(livro => livro.quantidade > 0).sort((a, b) => a.titulo.localeCompare(b.titulo)).slice(0, 5)
            : livros.filter(livro => livro.quantidade <= 1).sort((a, b) => a.quantidade - b.quantidade);
        stockList.innerHTML = livrosDoPainel.length
            ? livrosDoPainel.map(livro => `<li class="stock-item"><span class="stock-copy"><span class="cell-title">${escapeHtml(livro.titulo)}</span><span class="cell-detail">${escapeHtml(livro.autorNome)}</span>${isAluno ? `<span class="book-preview">${escapeHtml(livro.descricao || "Descrição não informada.")}</span>` : ""}</span><span class="stock-count${isAluno ? " availability-label" : ""}">${isAluno ? "Disponível" : livro.quantidade}</span></li>`).join("")
            : `<li class="table-message">${isAluno ? "Nenhum livro disponível no momento." : "Nenhum livro com estoque reduzido."}</li>`;
    } catch (error) {
        reportError(error);
        document.querySelectorAll(".metric-value").forEach(item => item.textContent = "—");
        const isAluno = getAuth()?.perfil === "ALUNO";
        setEmpty(document.querySelector("#recent-loans-body"), isAluno ? 3 : 4, "Não foi possível carregar os dados.");
        document.querySelector("#low-stock-list").innerHTML = `<li class="table-message">Não foi possível carregar ${isAluno ? "o catálogo" : "o estoque"}.</li>`;
        document.querySelectorAll("#admin-analytics .chart-list").forEach(chart => {
            chart.innerHTML = '<p class="chart-empty">Não foi possível carregar este indicador.</p>';
        });
    }
}

async function initLivros() {
    const auth = getAuth();
    const form = document.querySelector("#book-form");
    const filterForm = document.querySelector("#book-filter-form");
    const authorSelect = document.querySelector("#autor");
    const submitButton = form.querySelector('[type="submit"]');
    const tbody = document.querySelector("#books-body");
    let editingId = null;
    let currentBooks = [];
    let currentPage = 1;

    function renderBookPagination(response) {
        const container = document.querySelector("#book-pagination");
        if (!container) return;
        container.innerHTML = `
            <button class="secondary-action" type="button" data-page="${response.page - 1}" ${response.page <= 1 ? "disabled" : ""}>Anterior</button>
            <span>Página ${response.page} de ${Math.max(response.totalPages, 1)}</span>
            <button class="secondary-action" type="button" data-page="${response.page + 1}" ${response.page >= response.totalPages ? "disabled" : ""}>Próxima</button>`;
    }

    function resetForm() {
        editingId = null;
        form.reset();
        document.querySelector("#book-form-title").textContent = "Cadastrar livro";
        submitButton.textContent = "Cadastrar livro";
    }

    if (auth.perfil === "ALUNO") {
        form.closest(".form-panel").hidden = true;
        document.querySelector(".page-title").textContent = "Catálogo";
        document.querySelector(".page-description").textContent = "Consulte o acervo pelo título ou autor e solicite o empréstimo de qualquer livro.";
        document.querySelector('label[for="filtro-titulo"]').textContent = "Consulta";
        document.querySelector("#filtro-titulo").placeholder = "Digite o livro ou autor";
        document.querySelector("#filtro-autor").closest(".field").hidden = true;
        filterForm.insertAdjacentHTML("afterbegin", `
            <div class="field">
                <label for="tipo-consulta">Consultar por</label>
                <select id="tipo-consulta"><option value="titulo">Livro</option><option value="autor">Autor</option></select>
            </div>`);
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
            let titulo = document.querySelector("#filtro-titulo").value.trim();
            let autor = document.querySelector("#filtro-autor").value.trim();
            if (auth.perfil === "ALUNO") {
                const termo = titulo;
                const tipo = document.querySelector("#tipo-consulta").value;
                titulo = tipo === "titulo" ? termo : "";
                autor = tipo === "autor" ? termo : "";
            }
            if (titulo) params.set("titulo", titulo);
            if (autor) params.set("autor", autor);
            params.set("page", String(currentPage));
            params.set("pageSize", "10");
            const query = params.toString() ? `?${params}` : "";
            const [response, solicitacoes, emprestimosDoAluno] = auth.perfil === "ALUNO"
                ? await Promise.all([apiRequest(`/livros${query}`), apiRequest("/reservas"), apiRequest("/emprestimos")])
                : [await apiRequest(`/livros${query}`), [], []];
            const livros = response.items || [];
            const solicitacoesAtivas = new Set(solicitacoes
                .filter(item => Number(item.status) === 0 || Number(item.status) === 1)
                .map(item => item.livroId));
            const emprestimosAbertos = new Set(emprestimosDoAluno
                .filter(item => Number(item.status) !== STATUS.DEVOLVIDO)
                .map(item => item.livroId));
            currentBooks = livros;
            renderBookPagination(response);
            document.querySelector("#book-count").textContent = `${response.totalItems} ${response.totalItems === 1 ? "título encontrado" : "títulos encontrados"}`;
            if (!livros.length) return setEmpty(tbody, 5, "Nenhum livro encontrado.");
            tbody.innerHTML = livros.map(livro => `<tr>
                <td><span class="cell-title">${escapeHtml(livro.titulo)}</span><span class="cell-detail">ISBN ${escapeHtml(livro.isbn)}</span><span class="book-preview catalog-book-preview">${escapeHtml(livro.descricao || "Descrição não informada.")}</span></td>
                <td>${escapeHtml(livro.autorNome)}</td>
                <td>${livro.anoPublicacao}</td>
                <td><span class="status ${livro.quantidade === 0 ? "status-low" : "status-active"}">${livro.quantidade} ${livro.quantidade === 1 ? "unidade" : "unidades"}</span></td>
                <td>${auth.perfil === "ALUNO"
                    ? (solicitacoesAtivas.has(livro.id)
                        ? '<span class="muted-text">Solicitação ativa</span>'
                        : emprestimosAbertos.has(livro.id)
                            ? '<span class="muted-text">Em empréstimo</span>'
                            : `<button class="text-action" type="button" data-reserve-book="${livro.id}">Solicitar empréstimo</button>`)
                    : `<button class="text-action" type="button" data-edit-book="${livro.id}">Editar</button> <button class="text-action danger-text" type="button" data-delete-book="${livro.id}">Excluir</button>`}</td>
            </tr>`).join("");
        } catch (error) {
            reportError(error);
            setEmpty(tbody, 5, "Não foi possível carregar o acervo.");
        }
    }

    tbody.addEventListener("click", async event => {
        const editButton = event.target.closest("[data-edit-book]");
        const deleteButton = event.target.closest("[data-delete-book]");
        const reserveButton = event.target.closest("[data-reserve-book]");
        if (reserveButton) {
            try {
                const livro = currentBooks.find(item => item.id === Number(reserveButton.dataset.reserveBook));
                await apiRequest("/reservas", { method: "POST", body: JSON.stringify({ livroId: Number(reserveButton.dataset.reserveBook) }) });
                showToast(livro?.quantidade > 0
                    ? "Solicitação enviada para aprovação do bibliotecário."
                    : "Solicitação adicionada à fila de disponibilidade.");
                await loadBooks();
            } catch (error) { reportError(error); }
            return;
        }
        if (deleteButton) {
            const id = Number(deleteButton.dataset.deleteBook);
            if (await confirmAction("Excluir este livro?", "Excluir livro")) {
                try { await apiRequest(`/livros/${id}`, { method: "DELETE" }); await loadBooks(); showToast("Livro excluído."); }
                catch (error) { reportError(error); }
            }
            return;
        }
        if (!editButton) return;
        const livro = currentBooks.find(item => item.id === Number(editButton.dataset.editBook));
        if (!livro) return;

        editingId = livro.id;
        document.querySelector("#titulo").value = livro.titulo;
        document.querySelector("#isbn").value = livro.isbn;
        document.querySelector("#descricao").value = livro.descricao;
        authorSelect.value = String(livro.autorId);
        document.querySelector("#editora").value = livro.editora;
        document.querySelector("#categoria").value = livro.categoria;
        document.querySelector("#localizacao").value = livro.localizacao;
        document.querySelector("#ano").value = livro.anoPublicacao;
        document.querySelector("#quantidade").value = livro.quantidade;
        document.querySelector("#book-form-title").textContent = "Editar livro";
        submitButton.textContent = "Salvar alterações";
        form.scrollIntoView({ behavior: "smooth", block: "center" });
    });

    filterForm.addEventListener("submit", event => {
        event.preventDefault();
        currentPage = 1;
        loadBooks();
    });
    document.querySelector("#clear-book-filters").addEventListener("click", () => {
        filterForm.reset();
        currentPage = 1;
        loadBooks();
    });
    document.querySelector("#book-pagination")?.addEventListener("click", event => {
        const button = event.target.closest("[data-page]");
        if (!button || button.disabled) return;
        currentPage = Number(button.dataset.page);
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
                    descricao: document.querySelector("#descricao").value.trim(),
                    autorId: Number(authorSelect.value),
                    editora: document.querySelector("#editora").value.trim(),
                    categoria: document.querySelector("#categoria").value.trim(),
                    localizacao: document.querySelector("#localizacao").value.trim(),
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
    const auth = getAuth();
    const form = document.querySelector("#author-form");
    const tbody = document.querySelector("#authors-body");
    const submitButton = form.querySelector('[type="submit"]');
    let editingId = null;
    let currentAuthors = [];

    if (auth.perfil === "ALUNO") form.closest(".form-panel").hidden = true;

    function resetForm() {
        editingId = null;
        form.reset();
        document.querySelector("#author-form-title").textContent = "Cadastrar autor";
        submitButton.textContent = "Cadastrar autor";
    }

    async function loadAuthors() {
        setLoading(tbody, 5);
        try {
            const [autores, livros] = await Promise.all([apiRequest("/autores"), getBooks()]);
            currentAuthors = autores;
            document.querySelector("#author-count").textContent = `${autores.length} ${autores.length === 1 ? "autor cadastrado" : "autores cadastrados"}`;
            if (!autores.length) return setEmpty(tbody, 5, "Nenhum autor cadastrado.");
            const counts = livros.reduce((map, livro) => map.set(livro.autorId, (map.get(livro.autorId) || 0) + 1), new Map());
            tbody.innerHTML = autores.map(autor => `<tr>
                <td><span class="cell-title">${escapeHtml(autor.nome)}</span><span class="cell-detail">ID ${autor.id}</span></td>
                <td>${formatDate(autor.dataNascimento)}</td>
                <td>${escapeHtml(autor.nacionalidade)}</td>
                <td>${counts.get(autor.id) || 0} título(s)</td>
                <td class="actions-cell">${auth.perfil === "ALUNO" ? "Consulta" : `<button class="text-action" type="button" data-edit-author="${autor.id}">Editar</button><button class="text-action danger-text" type="button" data-delete-author="${autor.id}">Excluir</button>`}</td>
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
        const allowed = !email || email.endsWith("@ifpe.edu.br");
        emailInput.setCustomValidity(allowed
            ? ""
            : "Use um e-mail com domínio @ifpe.edu.br.");
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
                const overdue = loans.filter(item => Number(item.status) === STATUS.ATRASADO || isOverdue(item)).length;
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
                    email: document.querySelector("#email").value.trim(),
                    senha: document.querySelector("#senha").value
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
    const auth = getAuth();
    const form = document.querySelector("#loan-form");
    const filterForm = document.querySelector("#loan-filter-form");
    const tbody = document.querySelector("#loans-body");
    const visibleColumns = auth.perfil === "ALUNO" ? 5 : 6;
    let data = { alunos: [], livros: [], emprestimos: [] };
    if (auth.perfil === "ALUNO") {
        form.closest(".form-panel").hidden = true;
        document.querySelector('label[for="busca"]').textContent = "Livro";
        document.querySelector("#busca").placeholder = "Buscar por título";
        document.querySelector(".page-title").textContent = "Meu histórico";
        document.querySelector(".page-description").textContent = "Consulte seus empréstimos, prazos, devoluções e multas.";
        document.querySelector(".panel-title").textContent = "Meu histórico de circulação";
        const headers = tbody.closest("table").querySelectorAll("thead th");
        headers[0].hidden = true;
        headers[headers.length - 1].textContent = "Multa";
    }

    function renderLoans() {
        const search = document.querySelector("#busca").value.trim().toLocaleLowerCase("pt-BR");
        const selectedStatus = document.querySelector("#status").value;
        const alunosMap = new Map(data.alunos.map(item => [item.id, item]));
        const livrosMap = new Map(data.livros.map(item => [item.id, item]));
        const filtered = data.emprestimos.filter(loan => {
            const aluno = alunosMap.get(loan.alunoId);
            const livro = livrosMap.get(loan.livroId);
            const matchesBook = livro?.titulo.toLocaleLowerCase("pt-BR").includes(search);
            const matchesStudent = auth.perfil !== "ALUNO" &&
                (aluno?.nome.toLocaleLowerCase("pt-BR").includes(search) ||
                 aluno?.matricula.toLocaleLowerCase("pt-BR").includes(search));
            const matchesSearch = !search || matchesBook || matchesStudent;
            const matchesStatus = !selectedStatus || getLoanStatus(loan).key === selectedStatus;
            return matchesSearch && matchesStatus;
        }).sort((a, b) => new Date(b.dataEmprestimo) - new Date(a.dataEmprestimo));

        const activeCount = data.emprestimos.filter(item => Number(item.status) === STATUS.ATIVO && !isOverdue(item)).length;
        const overdueCount = data.emprestimos.filter(item => Number(item.status) === STATUS.ATRASADO || isOverdue(item)).length;
        document.querySelector("#loan-count").textContent = `${activeCount} ativo(s) e ${overdueCount} em atraso`;
        if (!filtered.length) return setEmpty(tbody, visibleColumns, "Nenhum empréstimo encontrado.");

        tbody.innerHTML = filtered.map(loan => {
            const aluno = alunosMap.get(loan.alunoId);
            const livro = livrosMap.get(loan.livroId);
            const status = getLoanStatus(loan);
            const action = auth.perfil === "ALUNO" ? `<span class="muted-text">R$ ${Number(loan.multa || 0).toFixed(2)}</span>` : Number(loan.status) === STATUS.DEVOLVIDO
                ? `<span class="muted-text">Devolvido em ${formatDate(loan.dataDevolucao)}</span>`
                : `<button class="text-action" type="button" data-return-loan="${loan.id}">Devolver</button>`;
            return `<tr>
                ${auth.perfil === "ALUNO" ? "" : `<td><span class="cell-title">${escapeHtml(aluno?.nome || `Aluno #${loan.alunoId}`)}</span><span class="cell-detail">${escapeHtml(aluno?.matricula || "")}</span></td>`}
                <td>${escapeHtml(livro?.titulo || `Livro #${loan.livroId}`)}</td>
                <td>${formatDate(loan.dataEmprestimo)}</td>
                <td>${formatDate(loan.dataPrevistaDevolucao)}</td>
                <td><span class="status ${status.className}">${status.label}</span></td>
                <td>${action}</td>
            </tr>`;
        }).join("");
    }

    async function loadLoans() {
        setLoading(tbody, visibleColumns);
        try {
            const [livros, emprestimos] = await Promise.all([getBooks(), apiRequest("/emprestimos")]);
            const alunos = auth.perfil === "ALUNO"
                ? [{ id: auth.alunoId, nome: auth.nome, matricula: "" }]
                : await apiRequest("/alunos");
            data = { alunos, livros, emprestimos };
            document.querySelector("#aluno").innerHTML = '<option value="">Selecione um aluno</option>' +
                alunos.map(item => `<option value="${item.id}">${escapeHtml(item.nome)} — ${escapeHtml(item.matricula)}</option>`).join("");
            const disponiveis = livros.filter(item => item.quantidade > 0);
            document.querySelector("#livro").innerHTML = '<option value="">Selecione um livro</option>' +
                disponiveis.map(item => `<option value="${item.id}">${escapeHtml(item.titulo)} — ${item.quantidade} disponível(is)</option>`).join("");
            renderLoans();
        } catch (error) {
            reportError(error);
            setEmpty(tbody, visibleColumns, "Não foi possível carregar os empréstimos.");
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

async function initReservas() {
    const tbody = document.querySelector("#reservations-body");
    const isStudent = getAuth()?.perfil === "ALUNO";
    const visibleColumns = isStudent ? 5 : 6;
    const formPanel = document.querySelector("#reservation-form-panel");
    const form = document.querySelector("#reservation-form");
    const bookSelect = document.querySelector("#reserva-livro");
    if (isStudent) {
        tbody.closest("table").querySelector("thead th").hidden = true;
        document.querySelector(".page-title").textContent = "Minhas solicitações";
        document.querySelector(".page-description").textContent = "Solicite livros disponíveis ou entre automaticamente na fila quando não houver estoque.";
        document.querySelector(".panel-title").textContent = "Minhas solicitações registradas";
        formPanel.hidden = false;
    } else {
        document.querySelector(".page-title").textContent = "Solicitações de empréstimo";
        document.querySelector(".page-description").textContent = "Aprove solicitações disponíveis, acompanhe a fila e rejeite pedidos quando necessário.";
        formPanel.closest(".content-grid").classList.add("single-column");
    }

    async function loadReservations() {
        try {
            const reservas = await apiRequest("/reservas");
            const status = ["Aguardando disponibilidade", "Aguardando aprovação", "Aprovada", "Rejeitada", "Cancelada"];
            if (!reservas.length) setEmpty(tbody, visibleColumns, "Nenhuma solicitação registrada.");
            else {
                tbody.innerHTML = reservas.map(item => {
                    const value = Number(item.status);
                    const queue = value === 1
                        ? '<span class="queue-position queue-ready">Pronto para análise</span>'
                        : value === 0
                            ? `<span class="queue-position">${Number(item.quantidadeAFrente) > 0 ? `${item.quantidadeAFrente} à frente` : "Primeiro da fila"}</span>`
                            : '<span class="muted-text">—</span>';
                    let actions = '<span class="muted-text">—</span>';
                    if (isStudent && (value === 0 || value === 1)) {
                        actions = `<button class="text-action danger-text" type="button" data-cancel-request="${item.id}">Cancelar</button>`;
                    } else if (!isStudent && value === 1) {
                        actions = `<button class="text-action" type="button" data-approve-request="${item.id}">Aprovar</button> <button class="text-action danger-text" type="button" data-reject-request="${item.id}">Rejeitar</button>`;
                    } else if (!isStudent && value === 0) {
                        actions = `<button class="text-action danger-text" type="button" data-reject-request="${item.id}">Rejeitar</button>`;
                    }
                    return `<tr>
                        ${isStudent ? "" : `<td>${escapeHtml(item.alunoNome)}</td>`}<td>${escapeHtml(item.livroTitulo)}</td>
                        <td>${formatDate(item.dataReserva)}</td><td>${status[value] || item.status}</td><td>${queue}</td><td class="actions-cell">${actions}</td>
                    </tr>`;
                }).join("");
            }

            if (isStudent) {
                const [livros, emprestimos] = await Promise.all([
                    getBooks(),
                    apiRequest("/emprestimos")
                ]);
                const reservasAtivas = new Set(reservas
                    .filter(item => Number(item.status) === 0 || Number(item.status) === 1)
                    .map(item => item.livroId));
                const emprestimosAbertos = new Set(emprestimos
                    .filter(item => Number(item.status) === STATUS.ATIVO || Number(item.status) === STATUS.ATRASADO)
                    .map(item => item.livroId));
                const reservaveis = livros.filter(livro =>
                    !reservasAtivas.has(livro.id) && !emprestimosAbertos.has(livro.id));
                bookSelect.innerHTML = reservaveis.length
                    ? '<option value="">Selecione um livro</option>' + reservaveis.map(livro => `<option value="${livro.id}">${escapeHtml(livro.titulo)} — ${escapeHtml(livro.autorNome)}</option>`).join("")
                    : '<option value="">Nenhum livro disponível para nova solicitação</option>';
                form.querySelector('[type="submit"]').disabled = !reservaveis.length;
            }
        } catch (error) {
            reportError(error);
            setEmpty(tbody, visibleColumns, "Não foi possível carregar as solicitações.");
        }
    }

    if (isStudent) {
        form.addEventListener("submit", async event => {
            event.preventDefault();
            const button = form.querySelector('[type="submit"]');
            setButtonBusy(button, true, "Enviando...");
            try {
                await apiRequest("/reservas", {
                    method: "POST",
                    body: JSON.stringify({ livroId: Number(bookSelect.value) })
                });
                form.reset();
                showToast("Solicitação registrada com sucesso.");
                await loadReservations();
            } catch (error) {
                reportError(error);
            } finally {
                setButtonBusy(button, false);
                button.disabled = bookSelect.options.length === 1 && bookSelect.options[0].value === "";
            }
        });
    }

    tbody.addEventListener("click", async event => {
        const approve = event.target.closest("[data-approve-request]");
        const reject = event.target.closest("[data-reject-request]");
        const cancel = event.target.closest("[data-cancel-request]");
        const id = Number(approve?.dataset.approveRequest || reject?.dataset.rejectRequest || cancel?.dataset.cancelRequest);
        if (!id) return;

        const action = approve ? "aprovar" : reject ? "rejeitar" : "cancelar";
        const actionLabel = approve ? "Aprovar solicitação" : reject ? "Rejeitar solicitação" : "Cancelar";
        const confirmed = await confirmAction(`${action[0].toUpperCase()}${action.slice(1)} esta solicitação?`, actionLabel);
        if (!confirmed) return;
        try {
            await apiRequest(`/reservas/${id}/${action}`, { method: "PUT" });
            showToast(`Solicitação ${approve ? "aprovada" : reject ? "rejeitada" : "cancelada"} com sucesso.`);
            await loadReservations();
        } catch (error) {
            reportError(error);
        }
    });

    await loadReservations();
}

async function initNotificacoes() {
    const list = document.querySelector("#notifications-list");
    try {
        const notificacoes = await apiRequest("/notificacoes");
        list.innerHTML = notificacoes.length
            ? notificacoes.map(item => `<article class="notification-item"><strong>${escapeHtml(item.mensagem)}</strong><span class="cell-detail">${formatDate(item.data)} · ${escapeHtml(item.tipo)}</span></article>`).join("")
            : '<p class="table-message">Nenhuma notificação.</p>';
    } catch (error) { reportError(error); list.innerHTML = '<p class="table-message">Não foi possível carregar as notificações.</p>'; }
}

async function initBibliotecarios() {
    const form = document.querySelector("#librarian-form");
    const tbody = document.querySelector("#librarians-body");
    const emailInput = document.querySelector("#email");

    function validateEmailDomain() {
        const email = emailInput.value.trim().toLowerCase();
        const allowed = !email || email.endsWith("@ifpe.edu.br");
        emailInput.setCustomValidity(allowed
            ? ""
            : "Use um e-mail com domínio @ifpe.edu.br.");
        return allowed;
    }

    emailInput.addEventListener("input", validateEmailDomain);

    async function loadLibrarians() {
        setLoading(tbody, 3);
        try {
            const bibliotecarios = await apiRequest("/usuarios/bibliotecarios");
            document.querySelector("#librarian-count").textContent = `${bibliotecarios.length} ${bibliotecarios.length === 1 ? "bibliotecário cadastrado" : "bibliotecários cadastrados"}`;
            if (!bibliotecarios.length) {
                setEmpty(tbody, 3, "Nenhum bibliotecário cadastrado.");
                return;
            }
            tbody.innerHTML = bibliotecarios.map(item => `<tr>
                <td><span class="cell-title">${escapeHtml(item.nome)}</span><span class="cell-detail">ID ${item.id}</span></td>
                <td>${escapeHtml(item.email)}</td>
                <td><span class="status status-active">Bibliotecário</span></td>
            </tr>`).join("");
        } catch (error) {
            reportError(error);
            setEmpty(tbody, 3, "Não foi possível carregar os bibliotecários.");
        }
    }

    form.addEventListener("submit", async event => {
        event.preventDefault();
        if (!validateEmailDomain()) {
            emailInput.reportValidity();
            return;
        }
        const button = form.querySelector('[type="submit"]');
        setButtonBusy(button, true, "Cadastrando...");
        try {
            await apiRequest("/usuarios/bibliotecarios", {
                method: "POST",
                body: JSON.stringify({
                    nome: document.querySelector("#nome").value.trim(),
                    email: document.querySelector("#email").value.trim(),
                    senha: document.querySelector("#senha").value
                })
            });
            form.reset();
            showToast("Bibliotecário cadastrado com sucesso.");
            await loadLibrarians();
        } catch (error) {
            reportError(error);
        } finally {
            setButtonBusy(button, false);
        }
    });

    document.querySelector("#clear-librarian-form").addEventListener("click", () => {
        form.reset();
        emailInput.setCustomValidity("");
    });
    await loadLibrarians();
}

async function initRelatorios() {
    const popularBody = document.querySelector("#popular-books-body");
    const overdueBody = document.querySelector("#overdue-users-body");
    const historyBody = document.querySelector("#report-history-body");
    const form = document.querySelector("#report-period-form");
    const startInput = document.querySelector("#report-start-date");
    const endInput = document.querySelector("#report-end-date");

    const today = new Date();
    const firstDay = new Date(today.getFullYear(), today.getMonth(), 1);
    const toInputDate = date => {
        const year = date.getFullYear();
        const month = String(date.getMonth() + 1).padStart(2, "0");
        const day = String(date.getDate()).padStart(2, "0");
        return `${year}-${month}-${day}`;
    };
    startInput.value = toInputDate(firstDay);
    endInput.value = toInputDate(today);

    const formatCurrency = value => new Intl.NumberFormat("pt-BR", {
        style: "currency",
        currency: "BRL"
    }).format(Number(value) || 0);

    async function loadSummaryReports() {
        setLoading(popularBody, 4);
        setLoading(overdueBody, 6);
        try {
            const [popularBooks, overdueUsers] = await Promise.all([
                apiRequest("/relatorios/livros-mais-emprestados"),
                apiRequest("/relatorios/usuarios-inadimplentes")
            ]);

            if (!popularBooks.length) {
                setEmpty(popularBody, 4, "Nenhum empréstimo registrado.");
            } else {
                popularBody.innerHTML = popularBooks.map((item, index) => `<tr>
                    <td>${index + 1}º</td>
                    <td><span class="cell-title">${escapeHtml(item.titulo)}</span><span class="cell-detail">ID ${item.livroId}</span></td>
                    <td>${escapeHtml(item.autorNome)}</td>
                    <td><strong>${item.quantidadeEmprestimos}</strong></td>
                </tr>`).join("");
            }

            if (!overdueUsers.length) {
                setEmpty(overdueBody, 6, "Nenhum usuário inadimplente.");
            } else {
                overdueBody.innerHTML = overdueUsers.map(item => `<tr>
                    <td><span class="cell-title">${escapeHtml(item.nome)}</span><span class="cell-detail">${escapeHtml(item.matricula)}</span></td>
                    <td>${escapeHtml(item.email)}</td>
                    <td>${item.quantidadeEmprestimosAtrasados}</td>
                    <td>${item.diasAtrasoTotal}</td>
                    <td><strong>${formatCurrency(item.multaTotal)}</strong></td>
                    <td><span class="status status-overdue">Inadimplente</span></td>
                </tr>`).join("");
            }
        } catch (error) {
            reportError(error);
            setEmpty(popularBody, 4, "Não foi possível carregar o relatório.");
            setEmpty(overdueBody, 6, "Não foi possível carregar o relatório.");
        }
    }

    async function loadHistory() {
        setLoading(historyBody, 8);
        try {
            const history = await apiRequest(`/relatorios/historico?dataInicio=${encodeURIComponent(startInput.value)}&dataFim=${encodeURIComponent(endInput.value)}`);
            document.querySelector("#history-report-count").textContent = `${history.length} ${history.length === 1 ? "registro encontrado" : "registros encontrados"}`;
            if (!history.length) {
                setEmpty(historyBody, 8, "Nenhum empréstimo encontrado no período.");
                return;
            }

            historyBody.innerHTML = history.map(item => {
                const status = getLoanStatus(item);
                return `<tr>
                    <td><span class="cell-title">${escapeHtml(item.alunoNome)}</span><span class="cell-detail">${escapeHtml(item.matricula)}</span></td>
                    <td>${escapeHtml(item.livroTitulo)}</td>
                    <td>${formatDate(item.dataEmprestimo)}</td>
                    <td>${formatDate(item.dataPrevistaDevolucao)}</td>
                    <td>${formatDate(item.dataDevolucao)}</td>
                    <td><span class="status ${status.className}">${status.label}</span></td>
                    <td>${item.diasAtraso}</td>
                    <td>${formatCurrency(item.multa)}</td>
                </tr>`;
            }).join("");
        } catch (error) {
            reportError(error);
            setEmpty(historyBody, 8, "Não foi possível carregar o histórico.");
        }
    }

    form.addEventListener("submit", async event => {
        event.preventDefault();
        if (startInput.value > endInput.value) {
            showToast("A data inicial não pode ser posterior à data final.", "error");
            return;
        }
        await loadHistory();
    });

    await Promise.all([loadSummaryReports(), loadHistory()]);
}

async function initAuditoria() {
    const tbody = document.querySelector("#audit-body");
    const previousButton = document.querySelector("#audit-previous");
    const nextButton = document.querySelector("#audit-next");
    const pageInfo = document.querySelector("#audit-page-info");
    const count = document.querySelector("#audit-count");
    let currentPage = 1;

    const formatDateTime = value => new Intl.DateTimeFormat("pt-BR", {
        day: "2-digit",
        month: "2-digit",
        year: "numeric",
        hour: "2-digit",
        minute: "2-digit",
        second: "2-digit"
    }).format(new Date(value));

    async function loadAudit(page = 1) {
        setLoading(tbody, 6);
        try {
            const response = await apiRequest(`/auditoria?page=${page}&pageSize=20`);
            currentPage = response.page;
            count.textContent = `${response.totalItems} ${response.totalItems === 1 ? "ação registrada" : "ações registradas"}`;
            pageInfo.textContent = `Página ${response.page} de ${Math.max(response.totalPages, 1)}`;
            previousButton.disabled = response.page <= 1;
            nextButton.disabled = response.totalPages === 0 || response.page >= response.totalPages;

            if (!response.items.length) {
                setEmpty(tbody, 6, "Nenhuma ação auditada até o momento.");
                return;
            }

            tbody.innerHTML = response.items.map(item => `<tr>
                <td><span class="cell-title">${escapeHtml(item.usuarioNome)}</span><span class="cell-detail">Usuário #${item.usuarioId}</span></td>
                <td><span class="status status-active">${escapeHtml(item.perfil)}</span></td>
                <td><span class="cell-title">${escapeHtml(item.acao)}</span></td>
                <td>${escapeHtml(item.detalhes)}</td>
                <td>${formatDateTime(item.data)}</td>
                <td>#${item.id}</td>
            </tr>`).join("");
        } catch (error) {
            reportError(error);
            setEmpty(tbody, 6, "Não foi possível carregar a auditoria.");
        }
    }

    previousButton.addEventListener("click", () => loadAudit(currentPage - 1));
    nextButton.addEventListener("click", () => loadAudit(currentPage + 1));
    await loadAudit();
}
