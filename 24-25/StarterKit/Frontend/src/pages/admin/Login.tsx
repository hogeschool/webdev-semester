import { Link } from "react-router-dom";
import { Form, useActionData } from "react-router-dom";
import { ResponseWrapper } from "../../api";

export default function Login() {

    const responseMsg: ResponseWrapper<string> = useActionData() as ResponseWrapper<string>

    return <div className="container-fluid ps-md-0">
        <div className="row g-0">
            <div className="d-none d-md-flex col-md-4 col-lg-6 bg-image"></div>
            <div className="col-md-8 col-lg-6">
                <div className="login d-flex align-items-center py-5">
                    <div className="container">
                        <div className="row">
                            <div className="col-md-9 col-lg-8 mx-auto">
                                <h3 className="login-heading mb-4">Admin login</h3>
                                {
                                    responseMsg != undefined && <div className="alert alert-danger" role="alert">
                                        {responseMsg.response}
                                    </div>
                                }
                                <Form method="post">
                                    <div className="form-floating mb-3">
                                        <input name="Username"
                                            type="text"
                                            className="form-control"
                                            placeholder="Username"
                                        />
                                        <label>Username</label>
                                    </div>
                                    <div className="form-floating mb-3">
                                        <input name="Password"
                                            type="password"
                                            className="form-control"
                                            id="floatingPassword"
                                            placeholder="Password"
                                        />
                                        <label>Password</label>
                                    </div>

                                    <div className="form-check mb-3">
                                        <input name="RememberMe"
                                            className="form-check-input"
                                            type="checkbox"
                                            value=""
                                        />
                                        <label className="form-check-label">
                                            Remember password
                                        </label>
                                    </div>

                                    <div className="d-grid">
                                        <button className="btn btn-lg btn-primary btn-login text-uppercase fw-bold mb-2" type="submit">
                                            Sign in
                                        </button>
                                        <div className="text-center">
                                            <Link className="small" to="/">Back to homepage</Link>
                                        </div>
                                    </div>

                                </Form>
                            </div>
                        </div>
                    </div>
                </div>
            </div>
        </div>
    </div>
}
