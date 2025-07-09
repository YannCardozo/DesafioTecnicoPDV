import React, { useState, useEffect } from "react";
import { Link, useNavigate } from "react-router-dom";
import "./Header.css";

const Header = () => {
  const [open, setOpen] = useState(false);
  const [logged, setLogged] = useState(false);
  const navigate = useNavigate();

  useEffect(() => {
    const token = document.cookie
      .split("; ")
      .find((c) => c.startsWith("pdvnet_token="));
    setLogged(!!token);
  }, []);

  const handleLogout = () => {
    document.cookie =
      "pdvnet_token=; expires=Thu, 01 Jan 1970 00:00:00 UTC; path=/;";
    setLogged(false);
    navigate("/login");
  };

  return (
    <header className="header">
      <Link to="/" className="logo">
        PDV<span>NET</span>
      </Link>
      <nav className={`nav ${open ? "open" : ""}`}>
        <Link to="/">Início</Link>
        <a href="#properties">Imóveis</a>
        <a href="#about">Sobre</a>
        <a href="#contact">Contato</a>
        {!logged ? (
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
  );
};
export default Header;