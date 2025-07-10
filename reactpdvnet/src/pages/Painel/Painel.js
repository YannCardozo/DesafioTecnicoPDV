import React, { useEffect, useState } from "react";
import { useNavigate } from "react-router-dom";
import "./Painel.css";
import { parseJwt } from "../../context/AuthContext";

const Painel = () => {
  const [claims, setClaims] = useState(null);
  const [token, setToken] = useState(null);
  const [alugueis, setAlugueis] = useState(null);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState(null);
  const navigate = useNavigate();

  /* ---------- carrega token & claims ---------- */
  useEffect(() => {
    const cookie = document.cookie
      .split("; ")
      .find((c) => c.startsWith("pdvnet_token="));

    if (!cookie) {
      navigate("/login", { replace: true });
      return;
    }

    const tk = cookie.split("=")[1];
    const payload = parseJwt(tk);

    if (!payload || payload.exp * 1000 < Date.now()) {
      navigate("/login", { replace: true });
      return;
    }

    setToken(tk);
    setClaims(payload);
  }, [navigate]);

  /* ---------- busca aluguéis ---------- */
  useEffect(() => {
    if (!claims || !claims.Id || !token) return;

    (async () => {
      setLoading(true);
      setError(null);
      try {
        const resp = await fetch(
          `https://testepdvnet.runasp.net/api/Aluguel/GetAllById/${claims.Id}`,
          {
            headers: { Authorization: `Bearer ${token}` }
          }
        );

        if (resp.ok) {
          setAlugueis(await resp.json());
        } else if (resp.status === 404) {
          setAlugueis([]);
        } else {
          setError((await resp.text()) || "Erro ao obter aluguéis.");
        }
      } catch {
        setError("Erro de conexão. Tente novamente.");
      } finally {
        setLoading(false);
      }
    })();
  }, [claims, token]);

  /* ---------- carregamento inicial ---------- */
  if (!claims) {
    return (
      <section className="painel-page">
        <p>Carregando informações do usuário...</p>
      </section>
    );
  }

  const { Usuario, CPF, Perfil, Nome, Id } = claims;

  return (
    <section className="painel-page">
      <h1>Bem-vindo {Nome} ao Painel PDVNET</h1>

      <div className="claims">
        <p>Exibindo as Claims do JWT:</p>
        <p><strong>Usuário:</strong> {Usuario}</p>
        <p><strong>Id:</strong> {Id}</p>
        <p><strong>CPF:</strong> {CPF}</p>
        <p><strong>Perfil:</strong> {Perfil}</p>
      </div>

      <div className="alugueis-section">
        <h2>Meus Aluguéis</h2>

        {loading && <p>Carregando aluguéis…</p>}
        {error && <p className="error">{error}</p>}

        {!loading && alugueis && alugueis.length === 0 && (
          <p>Não tem aluguel feito para esse locatário.</p>
        )}

        {!loading && alugueis && alugueis.length > 0 && (
          <ul className="alugueis-list">
            {alugueis.map((a) => {
              const diasRestantes = Math.max(
                0,
                Math.ceil(
                  (new Date(a.dataTermino) - new Date()) /
                    (1000 * 60 * 60 * 24)
                )
              );
              return (
                <li key={a.id} className="aluguel-item">
                  <h3>Imóvel: {a.imovel?.endereco ?? "-"}</h3>
                  <p><strong>Valor:</strong> R$ {a.valorLocacao}</p>
                  <p>
                    <strong>Período:</strong>{" "}
                    {new Date(a.dataInicio).toLocaleDateString()} até{" "}
                    {new Date(a.dataTermino).toLocaleDateString()}
                  </p>
                  <p>
                    <strong>Tempo Restante:</strong> {diasRestantes} dia
                    {diasRestantes !== 1 ? "s" : ""}
                  </p>
                </li>
              );
            })}
          </ul>
        )}
      </div>
    </section>
  );
};

export default Painel;
