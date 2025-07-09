import React from "react";
import "./Contact.css";

const Contact = () => (
  <section className="contact" id="contact">
    <h2>Fale Conosco</h2>
    <form onSubmit={(e) => e.preventDefault()}>
      <input type="text" placeholder="Nome" required />
      <input type="email" placeholder="E‑mail" required />
      <textarea placeholder="Mensagem" rows="4" required />
      <button type="submit">Enviar</button>
    </form>
  </section>
);
export default Contact;