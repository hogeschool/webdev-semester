import { Link } from "react-router-dom";

export default function Home() {


  return (
    <div className="container px-5 my-5">
      <h1>Welcome to the homepage</h1>

      <Link to="/admin">Visit the admin dashboard</Link>
    </div>
  );
}
