import React from "react";
import { Routes, Route, Navigate } from "react-router-dom";
import { useAuth } from "./context/AuthContext";
import Header from "./components/Header/Header";
import Hero from "./components/Hero/Hero";
import Properties from "./components/Properties/Properties";
import About from "./components/About/About";
import Contact from "./components/Contact/Contact";
import Footer from "./components/Footer/Footer";
import Login from "./pages/Login/Login";
import Painel from "./pages/Painel/Painel";

const Home = () => (
  <>
    <Hero />
    <Properties />
    <About />
    <Contact />
  </>
);

const PrivateRoute = ({ children }) => {
  const { isAuth } = useAuth();
  return isAuth ? children : <Navigate to="/login" replace />;
};

const App = () => (
  <>
    <Header />
    <main>
      <Routes>
        <Route path="/" element={<Home />} />
        <Route path="/login" element={<Login />} />
        <Route
          path="/painel"
          element={
            <PrivateRoute>
              <Painel />
            </PrivateRoute>
          }
        />
      </Routes>
    </main>
    <Footer />
  </>
);

export default App;