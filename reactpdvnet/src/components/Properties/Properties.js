import React from "react";
import PropertyCard from "../PropertyCard/PropertyCard";
import "./Properties.css";

const dummy = [
  {
    id: 1,
    title: "Casa Contemporânea",
    location: "Rio de Janeiro, RJ",
    price: "R$ 2.450.000",
    img: "https://images.unsplash.com/photo-1580587771525-78b9dba3b914?auto=format&fit=crop&w=800&q=80",
  },
  {
    id: 2,
    title: "Apartamento Minimalista",
    location: "São Paulo, SP",
    price: "R$ 950.000",
    img: "https://images.unsplash.com/photo-1580587771525-78b9dba3b914?auto=format&fit=crop&w=800&q=80",
  },
  {
    id: 3,
    title: "Cobertura Luxuosa",
    location: "Belo Horizonte, MG",
    price: "R$ 3.800.000",
    img: "https://images.unsplash.com/photo-1580587771525-78b9dba3b914?auto=format&fit=crop&w=800&q=80",
  },
];

const Properties = () => (
  <section className="properties" id="properties">
    <h2>Imóveis em Destaque</h2>
    <div className="grid">
      {dummy.map((p) => (
        <PropertyCard key={p.id} {...p} />
      ))}
    </div>
  </section>
);
export default Properties;