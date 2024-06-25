
export default function Contact() {


  return (
    <div className="container px-5 my-5">
      <div className="row justify-content-center">
        <div className="col-lg-8">
          <div className="card border-0 rounded-3 shadow-lg">
            <div className="card-body p-4">
              <div className="text-center">
                <div className="h1 fw-light">Contact Form</div>
                <p className="mb-4 text-muted">A functional, Bootstrap 5 contact form with validation built using SB Forms. Try it out!</p>
              </div>

              <form id="contactForm" data-sb-form-api-token="API_TOKEN">

                <div className="form-floating mb-3">
                  <input className="form-control" id="name" type="text" placeholder="Name" data-sb-validations="required" />
                  <label>Name</label>
                  <div className="invalid-feedback" data-sb-feedback="name:required">Name is required.</div>
                </div>

                <div className="form-floating mb-3">
                  <input className="form-control" id="emailAddress" type="email" placeholder="Email Address" data-sb-validations="required,email" />
                  <label>Email Address</label>
                  <div className="invalid-feedback" data-sb-feedback="emailAddress:required">Email Address is required.</div>
                  <div className="invalid-feedback" data-sb-feedback="emailAddress:email">Email Address Email is not valid.</div>
                </div>

                <div className="form-floating mb-3">
                  <textarea className="form-control" id="message" placeholder="Message" style={{ height: "10rem" }} data-sb-validations="required"></textarea>
                  <label>Message</label>
                  <div className="invalid-feedback" data-sb-feedback="message:required">Message is required.</div>
                </div>

                <div className="d-none" id="submitSuccessMessage">
                  <div className="text-center mb-3">
                    <div className="fw-bolder">Form submission successful!</div>
                    <p>To activate this form, sign up at</p>
                    <a href="https://startbootstrap.com/solution/contact-forms">https://startbootstrap.com/solution/contact-forms</a>
                  </div>
                </div>

                <div className="d-none" id="submitErrorMessage">
                  <div className="text-center text-danger mb-3">Error sending message!</div>
                </div>

                <div className="d-grid">
                  <button className="btn btn-primary btn-lg disabled" id="submitButton" type="submit">Submit</button>
                </div>
              </form>

            </div>
          </div>
        </div>
      </div>
    </div>
  );
}
