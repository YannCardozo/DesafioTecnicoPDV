import React from "react";
import "./PropertyCard.css";

const PropertyCard = ({ img, title, location, price }) => (
  <article className="property-card">
    <img src={img} alt={title} />
    <div className="info">
      <h3>{title}</h3>
      <p className="location">{location}</p>
      <p className="price">{price}</p>
      <button>Detalhes</button>
    </div>
  </article>
);
export default PropertyCard;
