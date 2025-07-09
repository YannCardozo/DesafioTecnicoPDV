import React from "react";
import { Routes, Route } from "react-router-dom";
import Header from "./components/Header/Header";
import Hero from "./components/Hero/Hero";
import Properties from "./components/Properties/Properties";
import About from "./components/About/About";
import Contact from "./components/Contact/Contact";
import Footer from "./components/Footer/Footer";
import Login from "./pages/Login/Login";

const Home = () => (
  <>
    <Hero />
    <Properties />
    <About />
    <Contact />
  </>
);

const App = () => (
  <>
    <Header />
    <main>
      <Routes>
        <Route path="/" element={<Home />} />
        <Route path="/login" element={<Login />} />
      </Routes>
    </main>
    <Footer />
  </>
);

export default App;