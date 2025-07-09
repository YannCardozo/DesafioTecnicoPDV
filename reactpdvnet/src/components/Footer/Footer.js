import React from "react";
import "./Footer.css";

const Footer = () => (
  <footer className="footer">
    <p>
      © {new Date().getFullYear()} PDVNET — Todos os direitos reservados.
    </p>
  </footer>
);
export default Footer;
