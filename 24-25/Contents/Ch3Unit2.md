# Bootstrap

<!-- WARNING!!!! THE FOLLOWING SAMPLES ARE SLIGHTLY UPDATED, NAOMI HAS IMPROVED THE HTML/CSS BY A LOT! -->

We are now going to de-uglify our application. Thanks to styling, and in particular thanks to the help of Bootstrap, we will be able to attach visual components to our HTML elements so that the raw structure of which data is being shown where on the page can be augmented with colours, improved layout and spacing, fonts, and in general all those things that make it a lot easier to visually consume information.

> Ok, big disclaimer! I do not have the eye of a designer, so this is just a best-effort preview. I am certain your applications will look so much better 😉

## Workflow
In many organisations the development of a frontend requires the help of a variety of professionals, all experts within their respective disciplines. 

It all starts with not one, but two (!) designers: an interaction designer who defines the logical flow of information across pages and interactions, and a visual designer who defines the colours, fonts, shapes, etc.

The interaction design is translated into the logical structure of the state, its calls to the backend APIs, as well as the state updaters, the widgets, etc. This work requires in-depth knowledge of software engineering, and is so tightly integrated with the backend that sometimes the same specialist who built the backend ends up building this part of the frontend as well. Sometimes the backend is nodes and Typescript already, but even if the backend is in Java or C#, having the same person work on it can yield tremendous results. 

The visual design is translated into what I like to call the layout of the React application. The layout is comprised by the various JSX elements (let’s say the HTML) of the various pages, as well as the CSS. The two are linked by assigning a `class` attribute that maps visual and layout properties from CSS to a specific HTML element. The world of CSS features huge complexity. All those who think that it is just a little extra do a huge disservice to how much work comes into building a modern, responsive, screen-size-agnostic frontend. My favourite way of working is collaborating with specialists who only focus on CSS and HTML, and nothing more.

This separation of concerns offers a great opportunity in our file management. We can even split our React files so that the layout is further isolated from widgets. For example, consider the product widget. We have already started with some splitting, but let’s make it even more explicit. Anything related to the HTML/CSS is moved to a separate, tsx file:

```tsx
import React from "react";
import * as ProductState from "./product.state";
import { replaceWith } from "../../../../utils/simpleUpdater";
import * as Bootstrap from "react-bootstrap";
import { ProductProps } from './product.widget';

export function ProductLayout(props: ProductProps): JSX.Element {
  return     <Bootstrap.Card key={`product-${props.product.id}`} as="li" className="mb-4">
      <div className="ratio ratio-21x9">
        <Bootstrap.Card.Img variant="top" src={props.product.image} className="object-fit-cover" />
      </div>
      <Bootstrap.Card.Body>
        <Bootstrap.Card.Title as="h2">
          {props.editing == false ? (
            props.product.name
          ) : (
            <input
              type="text"
              value={props.product.name}
              onChange={(e) => props.changeProduct(ProductState.Updaters.name(replaceWith(e.currentTarget.value)))}
              className="w-100 p-2"
            />
          )}
        </Bootstrap.Card.Title>
        <Bootstrap.Card.Text>
          {props.editing == false ? (
            props.product.description
          ) : (
            <textarea
              value={props.product.description}
              rows={5}
              className="w-100 p-2"
              onChange={(e) =>
                props.changeProduct(ProductState.Updaters.description(replaceWith(e.currentTarget.value)))
              }
            />
          )}
        </Bootstrap.Card.Text>
        {props.editing == false ? (
          <></>
        ) : (
          <Bootstrap.Button variant="primary" onClick={(e) => props.deleteProduct()}>
            Delete product
          </Bootstrap.Button>
        )}
      </Bootstrap.Card.Body>
    </Bootstrap.Card>
  )
}
```

What remains in the widget file is for now quite little, the glue between: props, state, API, other widgets, and the layout. And that’s quite an important function there, so don’t worry, these files are filled to the brim with valuable information that will grow over time:

```tsx
import { useState } from 'react';
import * as ProductState from './product.state';
import { Updater } from '../../../../utils/simpleUpdater';
import { ProductLayout } from './product.layout';

export type ProductProps = { 
    editing:boolean, product: ProductState.Product; 
    changeProduct:(_:Updater<ProductState.Product>) => void; 
    deleteProduct:() => void 
  };

export const ProductWidget = (props: ProductProps): JSX.Element => {
  return ProductLayout(props)
}
```

> In this case, the `ProductWidget` is just invoking the `ProductLayout`, and this might seem like a pointless split. For now it is, but a lot of our work as software engineers has to do with sustainability of our solutions in the medium and long term, even if sometimes it might introduce a little bit of extra overhead. And indded, this widget file will most likely grow, and quite a lot too. Hooks such as `useEffect` go in the widget file, for example. In some cases, it might even be desirable to distinguish the `props` of the widget, which might be very complete, from the simpler `props` that the layout component expects. In that case, the widget will also perform this conversion of `props` from one format to the other. In short, there are plenty of things that the widget does that have to do with manipulating data and structures and that it's better to keep separate from the layout which only takes care of what things look like.

A huge advantage of this way of working is that changes in the HTML/CSS can be done fully in parallel to changes in the rest of the frontend with a lot less merge conflicts and a better separation of concerns across the functional units of the team!

> You might wonder: why not still go for a full stack developer? Well, are we really sure that a single person can learn how to set up infrastructure, write efficient databases queries, write a fast and secure API, build the whole logic of the React application, and of course also master CSS/HTML, responsiveness, and SEO? I mean, if you can find such an expert chances are they are probably 80 years old already 👴👴👴 😉


## Bootstrap and React Bootstrap
Bootstrap is one of the most popular frontend frameworks out there. It contains pre-styled utilities that cover layout (for example, responsive grids), navigation, forms, buttons, textboxes, etc. All of these utilities can be used as-is, and they look a lot nicer than unstyled HTML as it comes out of the box. Which is not difficult.

The big advantage of Bootstrap though is that it is flexible. Bootstrap can be customised very easily by overriding some SCSS variables governing spacing, borders, margins, radiuses, colours, fonts, and everything we might want to override. Of course the nature of CSS itself allows to further apply changes by defining new CSS classes, but we will not go that far in this course.

We will not use Bootstrap directly though, but rather we will use React Bootstrap. React Bootstrap is a super-handy wrapper that creates React elements that encapsulate styled Bootstrap elements. So instead of using a regular `<button />` with a the Bootstrap class, we will use the `<Bootstrap.Button />` element, which already includes the right class(es) out of the box. The code looks a bit cleaner, and the separation of concerns between our application and React is even stronger. The React Bootstrap library basically provides us with another layer of layout, just like we ourselves are doing.

> One note though. Connecting a CSS class to a React element is not done via the the `class` attribute, but rather `className`. This is done because `class` is a reserved JS keyword and could not be hijacked further without inconvenient changes to existing Javascript and Typescript parsers. Oh well, one can always hope that this ugliness will, eventually, disappear, but for now we have to live with it.

### Dependencies
We start, as usual, by adding the required libraries:

```sh
yarn add bootstrap react-bootstrap
yarn add -D @types/bootstrap @types/react-bootstrap
```

We then want to create a `theme.scss` file which will contain the custom styling and import the Bootstrap stylesheet:

```scss
@import "node_modules/bootstrap/scss/bootstrap";
```

Later we will add more to this file, so good to be prepared! We now import this file from the main React file:

```tsx
// index.tsx
require('./styling/theme.scss')
```

`.scss` is an extended format of `css` which features styling as well as variables for improved code reuse. `scss` does not work out of the box and needs to be transformed into `css` through a pipeline. This requires quite some configuration though, because out of the box we cannot just import `scss` stylesheets and also we cannot just import them from `tsx` files. We add some more dev dependencies to instruct our webpack pipeline to also process stylesheets:

```sh
yarn add -D sass style-loader css-loader babel-plugin-transform-scss 
```

We then add some extra rules to the webpack file:

```js
        {
          test: /\.tsx?$/,
          use: 'babel-loader',
          exclude: '/node_modules/'
        },
        { enforce: 'pre', test: /\.js$/, loader: 'source-map-loader' },
        {
          test: /\.(woff|woff2|eot|ttf|otf|svg)$/i,
          type: 'asset/resource',
          generator: {
            filename: '[name][ext]' // Keep original filename instead of a unique one
          }
        },
        {
          test: /\.scss$/,
          use: [
            MiniCssExtractPlugin.loader, // Extracts CSS into a separate file
            'css-loader', // Translates CSS into CommonJS
            'sass-loader' // Compiles Sass to CSS
          ]
        }
```

Finally, we add the babel plugins configuration to the `package.json` file:

```json
    "plugins": [
      "babel-plugin-transform-scss"
    ]
```

And there we go! Doing this from scratch might require quite a lot of trial and error, and we are not working with a fully professional pipeline featuring things like watches for the stylesheets, but for the purposes of this introduction it will be enough.

# Let's get to styling
Great! Now we can finally start making the application prettier. Let's begin with the header. Of course first of all we must import the `react-bootstrap` library. 

> It is recommended to import the single components one by one instead of the whole library for performance reasons, because this will reduce the amount of data that needs to be sent to the frontend and make for faster pages and marginally higher Google ranking.

```tsx
import * as Bootstrap from 'react-bootstrap';
```

We wrap the whole content in a `Navbar` element with a container inside:

```tsx
  <header className="header">
    <Bootstrap.Navbar expand="lg" className="bg-white flex-column mb-4" style={{ fontFamily: 'Apercu-Mono' }}>
      <Bootstrap.Container>
        ...
      </Bootstrap.Container>
    </Bootstrap.Navbar>
  </header>
```

The `Container` is actually essential, otherwise the elements will pile up on top of each other. Knowing exactly why requires some in-depth knowledge of css, but we don't need to care that much: there is so much documentation and there are so many examples of how to use Bootstrap that we can just rely on those for decent results out of the box.

We now add a branding button (the logo or title of the application). Notice that we can use `as` in order to specify the actual element we want to use. Given that we are using React Router, we don't want the standard link which would cause a reload of the page but a `NavLink`, which we can request to React Bootstrap:

```tsx
      <Bootstrap.Navbar.Brand as={NavLink} to="/">GrandeOmega 🩵 React</Bootstrap.Navbar.Brand>
```

Bootstrap is responsive. This means that we can specify elements which are only relevant if the screen size is smaller or bigger than a given threshold. For example, we add a hamburger menu toggle which will stay hidden whenever the page is wide enough to show all the elements, and which will appear automatically when the screen becomes smaller, hiding the rest of the navbar in order to unclutter a smartphone or tablet screen:

```tsx
      <Bootstrap.Navbar.Toggle aria-controls="basic-navbar-nav" />
```

> Nice that this is all so simple and automated!

Then we define the collapsible block with all the buttons that will be hidden when the screen is too small:

```tsx
      <Bootstrap.Navbar.Collapse>
        <Bootstrap.Nav className="ms-auto">
          ...
```

We put three buttons in there. A link to the catalogue of all products, a dropdown with the single links to the individual products' pages (checking whether or not the products are actually loaded) and the login button:

```tsx
          <Bootstrap.Nav.Link className="ms-auto" as={NavLink} to="/products">Catalogue</Bootstrap.Nav.Link>
          <Bootstrap.NavDropdown className="ms-auto" title="Products" id="basic-nav-dropdown">
            {
              props.products == "loading" || props.products == "unloaded" || props.products == "API-error" ? 
              <Bootstrap.NavDropdown.Item disabled={true}>Loading...</Bootstrap.NavDropdown.Item>
              : <>
                {
                  props.products.valueSeq().toArray().map(product => 
                    <Bootstrap.NavDropdown.Item key={`/products/${product.id}`} as={NavLink} to={`/products/${product.id}`}>
                      {product.name}
                    </Bootstrap.NavDropdown.Item>
                  )
                }
                </>
            }
          </Bootstrap.NavDropdown>
          <Bootstrap.Nav className="ms-auto">
            <CurrentUserWidget currentUserState={props.currentUserState} setCurrentUser={props.setCurrentUser} />
          </Bootstrap.Nav>
```

> `<Bootstrap.Nav className="ms-auto">` is needed to keep the login button properly aligned to the start (`s`) of the menu on small screens.


The result already looks a lot cleaner and a lot more professional than our previous iteration!

Let's continue our makeover with the homepage. We will create a big block with a single-column large title, a subtitle, as well as a little bit of decoration around the content:

```tsx
  <header className="rounded-3 border shadow-lg p-4 p-lg-5">
    <h1 className="display-3 mb-3">Introduction to React</h1>
    <p className="lead">
      by Giuseppe Maggiore and{' '}
      <Bootstrap.Anchor className="link-info" href="http://grandeomega.com">
        GrandeOmega
      </Bootstrap.Anchor>
    </p>
  </header>
```

You can see a lot of classes, all from Bootstrap. Please refer to the documentation for all of them, but for example: `m-3` adds a medium-sized margin, `shadow-lg` adds a shadow to make the content stand out more, `row` adds a row to the grid, `link-info` applies styling with an `info` highlight (there are many levels: `primary`, `secondary`, `warning`, `info`, etc.) and much more. 

The `login/logout` widget is particularly interesting and a lot more functional in nature than the widgets we have styled so far. 

First of all, the login/logout will show a dynamic modal page when we click on the login button in the navbar. We need to track whether or not the modal is supposed to be rendered by extending the corresponding state with the `showLoginModal` boolean, which of course starts off as `false`:

```ts
export type CurrentUserState = (CurrentUserLoggedInState | CurrentUserLoggingInState) & { showLoginModal:boolean }
...
export const initialUserState = () : CurrentUserState => ({ kind:"login-form", ...initialLoginFormState(), showLoginModal:false })
```

The current user widget will use a `ref`, meaning that we will be able to mount the login modal window on the same component that will be used for the login/logout buttons, which are shown based on the state as simple navigation items:

```tsx
export const CurrentUserWidget = (props:CurrentUserProps) => {
  const target = useRef(null)

  return <>
    <Bootstrap.NavItem className="mx-2">
      {
        props.currentUserState.kind == "logged-in" ?
        <Bootstrap.Button variant="secondary" ref={target} onClick={() => props.setCurrentUser(CurrentUserStateUpdaters.logout)}>
            Logout
        </Bootstrap.Button>
        :
        <Bootstrap.Button variant="secondary" ref={target} onClick={() => props.setCurrentUser(CurrentUserStateUpdaters.showLoginModal(replaceWith(true)))}>
            Login
        </Bootstrap.Button>
      }
    </Bootstrap.NavItem>
    ...
```

When the user presses the `Login` button, we enable the login modal in the callback, and when the login modal is supposed to be shown, we render it as a separate form. Notice that being inside a modal means that we do not create navigation items because we are effectively "warped out" of the navigation bar and into a separate mini-window:

```tsx
    {props.currentUserState.showLoginModal ?
    <Bootstrap.Modal target={target.current} show={props.currentUserState.showLoginModal} placement="right">
      <>
        <Bootstrap.Modal.Header closeButton onClick={() => props.setCurrentUser(CurrentUserStateUpdaters.showLoginModal(replaceWith(false)))}>
          <Bootstrap.Modal.Title>Login</Bootstrap.Modal.Title>
        </Bootstrap.Modal.Header>
        <Bootstrap.Modal.Body> 
        {
        props.currentUserState.kind == "login-form" && props.currentUserState.validation ?
          <Bootstrap.Alert variant="info">{props.currentUserState.validation}</Bootstrap.Alert>
        :
          <></>
        }
        {
          props.currentUserState.kind == "waiting-for-api-response" ?
          WaitingForLoginAPIResponseWidget()
          : props.currentUserState.kind == "logged-in" ?
          LoggedInWidget(props)
          : LoginFormWidget(props, props.currentUserState)      
        }
        </Bootstrap.Modal.Body> 
      </>
    </Bootstrap.Modal>
    : <></>
  }
```

The modal features a header with title and a main body where we show the login form. When there has been a login error or a validation error, we show it inside an `Alert` banner with a friendly and pleasant `info` variant in order to avoid terrorizing the user with bright red error messages.

The login status can be either that we are waiting for the API response, and in that case we just show a loading animation:

```tsx
export const WaitingForLoginAPIResponseWidget = () => 
  <div className="d-flex justify-content-center my-5">
    <Bootstrap.Spinner animation="border" role="status">
      <span className="visually-hidden">Loading products...</span>
    </Bootstrap.Spinner>
  </div>
```

whereas when the form is supposed to be shown, well, we show the login form:

```tsx
export const LoginFormWidget = (props: CurrentUserProps, currentUserState:CurrentUserLoggingInState) => {
  return <>
    <Bootstrap.Form>
      <Bootstrap.Form.Group className="mb-3">
        <Bootstrap.Form.Label>Email </Bootstrap.Form.Label>
        <Bootstrap.Form.Control type="email" placeholder="Enter email" 
          onChange={e => props.setCurrentUser(CurrentUserStateUpdaters.email(replaceWith(e.currentTarget.value)))} 
          value={props.currentUserState.email}
        />
      </Bootstrap.Form.Group>
      <Bootstrap.Form.Group className="mb-3">
        <Bootstrap.Form.Label>Password </Bootstrap.Form.Label>
        <Bootstrap.Form.Control type="password" placeholder="Password" 
          onChange={e => props.setCurrentUser(CurrentUserStateUpdaters.password(replaceWith(e.currentTarget.value)))} value={currentUserState.password} 
        />
      </Bootstrap.Form.Group>
    </Bootstrap.Form>
    <Bootstrap.Form.Group className="mb-3">
      <Bootstrap.Button variant="primary" 
          onClick={_ => { CurrentUserStateUpdaters.invokeLoginAPI(props.setCurrentUser) }}>
        Login
      </Bootstrap.Button>
    </Bootstrap.Form.Group>
  </>
}
```

Once again, it looks a lot prettier and a lot more professional!

## Owning it
We top it off by extending our `theme.scss` file so that we can make the style our own. We change the color variables to introduce a dark style with a very outspoken, GrandeOmega-style visual:

```scss
$white: #000444;
$blue: #E0569B;
$black: #fff;
$gray-900: #fff;
$gray-800: #FCEEF5;
$border-radius: 1.2rem;
$gray-600: #3A36AE;
$border-radius-lg: 1rem;
$gray-200: #547BE5;
```

This changes the face of the website completely, in one go! Never underestimate the power of colors!

The dropdown is broken though, the highlight is all wrong. We can fix this by overriding the style of dropdowns when hovered so that they have the right combination of contrasting colors:

```scss
@import "node_modules/bootstrap/scss/bootstrap";

.dropdown a:hover {
  color: $gray-900;
  background-color:$blue;
}
```

Fonts play an equally important role when claiming ownership of a visual style. We add some fonts (we also need to include the corresponding files in the `server` directory at the same level as the `index.html` file):

```scss
@font-face {
  font-family: "Apercu";
  src: url(/Apercu.woff) format("woff");
}
@font-face {
  font-family: "Apercu-Mono";
  src: url(/Apercu-Mono.woff) format("woff");
}
@font-face {
  font-family: "Apercu-Bold";
  src: url(/Apercu-Bold.woff) format("woff");
}
```

And we can include these fonts by using for example inline styles in the desired elements:

```tsx
<Bootstrap.Navbar expand="lg" className="bg-white flex-column" style={ { fontFamily:"Apercu-Mono" } }>
```

Inline styles are not always recommended as a best practice. Adding a class to our `theme.scss` and referring to it would probably be a cleaner way of working. For example, we could add our font as the default font for the `body` of the page as well as all its descendants (excluding the `navbar` which overrides the font explicitly):

```scss
.body {
  font-family:"Apercu"
}
```

And there we go, the application has been restyled and now has a distinctive, orderly appearance that will help end users consume the content better and more pleasantly!


# Practice
Finish splitting all the layout files from their respective widgets.
Style the homepage and error page.
Style the products and product page.
Style the footer (ATTENTION! THERE IS SOME FLEXBOX MAGIC HERE, MAKE SURE TO EXPLAIN IT PROPERLY!!!).