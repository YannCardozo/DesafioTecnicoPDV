import React, { useState } from "react";
import { useNavigate } from "react-router-dom";
import { useAuth } from "../../context/AuthContext";
import Spinner from "../../components/Spinner/Spinner";
import "./Login.css";

const API_URL = "https://testepdvnet.runasp.net/api/Auth/Login";

const Login = () => {
  const [identifier, setIdentifier] = useState("pdvnet@pdvnet.com.br");
  const [password, setPassword] = useState("PDVnet123!@");
  const [error, setError] = useState(null);
  const [loading, setLoading] = useState(false);
  const navigate = useNavigate();
  const { login } = useAuth();

  const handleSubmit = async (e) => {
    e.preventDefault();
    setError(null);
    setLoading(true);

    const body = { Senha: password };
    if (identifier.includes("@")) body.Email = identifier;
    else body.CPF = identifier.replace(/\D/g, "");

    try {
      const resp = await fetch(API_URL, {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify(body),
      });
      const data = await resp.json();
      if (resp.ok) {
        login(data.token, data.expiration);
        navigate("/painel");
      } else setError(data);
    } catch {
      setError("Erro de conexão. Tente novamente.");
    } finally {
      setLoading(false);
    }
  };

  return (
    <section className="login-page">
      <form className="login-form" onSubmit={handleSubmit}>
        <h2>Entrar</h2>
        <input
          type="text"
          placeholder="E-mail ou CPF"
          value={identifier}
          onChange={(e) => setIdentifier(e.target.value)}
          required
          disabled={loading}
        />
        <input
          type="password"
          placeholder="Senha"
          value={password}
          onChange={(e) => setPassword(e.target.value)}
          required
          disabled={loading}
        />
        {error && <p className="error">{error}</p>}
        <button type="submit" disabled={loading}>
          {loading ? "Carregando..." : "Login"}
        </button>
        {loading && <Spinner />}
      </form>
    </section>
  );
};

export default Login;