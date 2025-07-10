import React, { useState } from "react";
import { Link, useNavigate } from "react-router-dom";
import { useAuth } from "../../context/AuthContext";
import Spinner from "../Spinner/Spinner";    // <>
import "./Header.css";

const Header = () => {
  const [open, setOpen] = useState(false);
  const [loadingLogout, setLoadingLogout] = useState(false);  // <>
  const { isAuth, logout } = useAuth();
  const navigate = useNavigate();

  const handleLogout = async () => {
    setLoadingLogout(true);         // liga o spinner
    try {
      // se tivesse uma chamada de API de logout, faria aqui
      await new Promise((r) => setTimeout(r, 300)); // opcional: simula um delay
      logout();                     // limpa cookie e estado
      navigate("/login");           // manda pra página de login
    } finally {
      setLoadingLogout(false);      // apaga o spinner
    }
  };

  return (
    <>
      {loadingLogout && <Spinner />}    {/* overlay de spinner */}
      <header className="header">
        <Link to="/" className="logo">
          PDV<span>NET</span>
        </Link>
        <nav className={`nav ${open ? "open" : ""}`}>
          <Link to="/">Início</Link>
          <a href="#properties">Imóveis</a>
          <a href="#about">Sobre</a>
          <a href="#contact">Contato</a>
          {!isAuth ? (
            <Link to="/login">Login</Link>
          ) : (
            <button className="logout" onClick={handleLogout}>
              Sair
            </button>
          )}
        </nav>
        <button className="hamburger" onClick={() => setOpen(!open)}>
          <span />
          <span />
          <span />
        </button>
      </header>
    </>
  );
};

export default Header;
