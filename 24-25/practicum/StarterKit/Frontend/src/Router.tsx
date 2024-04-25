import {
    createBrowserRouter,
} from "react-router-dom";
import Layout from "./shared/Layout";
import ErrorPage from "./shared/ErrorPage";
import Contact from "./pages/Contact";
import Home from "./pages/Home";
import About from "./pages/About";
import AdminLayout from "./shared/AdminLayout";
import Dashboard from "./pages/admin/Dashboard";
import Login from "./pages/admin/Login";
import { GenericDataFetch, IsAdminLoggedIn, LoginAsAdmin } from "./api";

export const router = createBrowserRouter([
    {
        path: "/",
        element: <Layout />,
        errorElement: <ErrorPage />,
        children: [
            {
                path: "",
                element: <Home />,
            },
            {
                path: "about",
                element: <About />,
            },
            {
                path: "contact",
                element: <Contact />,
            },
        ]
    },
    {
        path: "/admin",
        element: <AdminLayout />,
        errorElement: <ErrorPage />,
        loader: GenericDataFetch<string>(IsAdminLoggedIn, {
            fallbackRedirectUrl: '/admin/login',
            redirectCondition: r => r.status != 200
        }),
        children: [
            {
                path: "",
                element: <Dashboard />,
            },
        ]
    },
    {
        path: "admin/login",
        loader: GenericDataFetch<string>(IsAdminLoggedIn, {
            fallbackRedirectUrl: '/admin',
            redirectCondition: r => r.status == 200
        }),
        action: GenericDataFetch<string>(LoginAsAdmin, {
            fallbackRedirectUrl: '/admin',
            redirectCondition: r => r.status == 200
        }),
        element: <Login />,
        errorElement: <ErrorPage />,
    },
]);

