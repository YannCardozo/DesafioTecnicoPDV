import React, { useState } from "react";
import { useNavigate } from "react-router-dom";
import "./Login.css";

const API_URL = "https://testepdvnet.runasp.net/api/Auth/Login"; // ajuste se necessário

const Login = () => {
  const [identifier, setIdentifier] = useState("pdvnet@pdvnet.com.br");
  const [password, setPassword] = useState("PDVnet123!@");
  const [error, setError] = useState(null);
  const navigate = useNavigate();

  const handleSubmit = async (e) => {
    e.preventDefault();
    setError(null);

    const body = { Senha: password };
    if (identifier.includes("@")) {
      body.Email = identifier;
    } else {
      body.CPF = identifier.replace(/\D/g, "");
    }

    try {
      const resp = await fetch(API_URL, {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify(body),
      });
      const data = await resp.json();
      if (resp.ok) {
        const expires = new Date(data.expiration).toUTCString();
        document.cookie = `pdvnet_token=${data.token}; expires=${expires}; path=/;`;
        navigate("/");
      } else {
        setError(data);
      }
    } catch {
      setError("Erro de conexão. Tente novamente.");
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
        />
        <input
          type="password"
          placeholder="Senha"
          value={password}
          onChange={(e) => setPassword(e.target.value)}
          required
        />
        {error && <p className="error">{error}</p>}
        <button type="submit">Login</button>
      </form>
    </section>
  );
};
export default Login;