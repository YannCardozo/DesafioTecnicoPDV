import React from "react";
import "./Hero.css";

const Hero = () => (
  <section className="hero" id="home">
    <div className="overlay" />
    <div className="hero-content">
      <h1>Encontre o imóvel dos seus sonhos</h1>
      <p>Casas, apartamentos e terrenos selecionados para você.</p>
      <a href="#properties" className="cta">Ver Imóveis</a>
    </div>
  </section>
);
export default Hero;