import { useState } from "react";
import { Link } from "react-router-dom";
import { Outlet } from "react-router-dom";

export default function Layout() {

    const [dropdownState, setDropdownState] = useState<boolean>(false)

    return (
        <>
            <nav className="navbar navbar-expand-lg navbar-light bg-light shadow fixed-top">
                <div className="container">
                    <Link className="navbar-brand" to="/">Webdevelopment starter kit</Link>
                    <button onClick={() => {
                        if (dropdownState) {
                            setDropdownState(false)
                        } else {
                            setDropdownState(true)
                        }
                    }}
                        className="navbar-toggler" type="button" data-bs-toggle="collapse" data-bs-target="#navbarResponsive" aria-controls="navbarResponsive" aria-expanded="false" aria-label="Toggle navigation">
                        <span className="navbar-toggler-icon"></span>
                    </button>
                    <div onMouseLeave={() => {
                        setDropdownState(false)
                    }} className={`collapse navbar-collapse ${dropdownState ? 'show' : ''}`} id="navbarResponsive">
                        <ul className="navbar-nav ms-auto">
                            <li className="nav-item">
                                <Link className="nav-link" to="/">Home</Link>
                            </li>
                            <li className="nav-item">
                                <Link className="nav-link" to="/about">About</Link>
                            </li>
                            <li className="nav-item">
                                <Link className="nav-link" to="/contact">Contact</Link>
                            </li>
                        </ul>
                    </div>
                </div>
            </nav>

            <section className="py-5">
                <div className="container">
                    {/*Outlet is a special component from React Router that places the matching child components of Layout*/}
                    <Outlet />
                </div>
            </section>
        </>
    );
}