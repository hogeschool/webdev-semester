import { Link, useLoaderData, useNavigate } from "react-router-dom";
import { Outlet } from "react-router-dom";
import { ResponseWrapper } from "../api";


export default function AdminLayout() {

    const navigate = useNavigate();
    const responseMsg: ResponseWrapper<string> = useLoaderData() as ResponseWrapper<string>

    return <div>
        <nav className="navbar navbar-expand-md navbar-light bg-light">
            <div className="container">
                <Link className="navbar-brand" to="/admin/">Admin Dashboard</Link>

                <ul className="navbar-nav ms-auto">
                    <li className="nav-item">
                        <Link className="nav-link" to="/" role="button" >
                            Visit frontend
                        </Link>
                    </li>
                    <li className="nav-item">
                        <a className="nav-link" href="#" role="button" onClick={async () => {
                            var response = await fetch(`/api/v1/Login/Logout`)
                            if (response.ok) {
                                navigate('/admin/login')
                            }
                        }}>
                            Logout
                        </a>
                    </li>
                </ul>
            </div>
        </nav>

        <div className="sidebar">
            <h5>Welcome <b>{responseMsg.response}</b></h5>
            <a href="#">Dashboard</a>
            <a href="#">Pages</a>
            <a href="#">Customers</a>
            <a href="#">Admin users</a>
            <a href="#">Change password</a>
        </div>

        <main className="content">
            {/*Outlet is a special component from React Router that places the matching child components of Layout*/}
            <Outlet />
        </main>

    </div>


}