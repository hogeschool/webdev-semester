# Routing
React SPAs are still web applications. Just like web applications have different pages each with its own URL, so does a React SPA.

SPAs pages are "virtual". Thanks to packages such as React Router we can select which component to render based on the pattern of the URL.

We start by adding the React Router packages specialized for routing in the browser:

```
"@types/react-router-dom": "^5.3.3",
"react-router-dom": "^6.16.0",
```

We then need to add a `--proxy` command to our backend so that all accesses to `localhost:5000` pages, such as for example `localhost:5000/about-us` or `localhost:5000/products` or `localhost:5000/products/3` are all sent to our React application, which will take care of doing the actual routing:

```
http-server -p 5000 --proxy http://127.0.0.1:5000/?
```

## Basic routing
Let's start by getting very basic routing going. We will take care of a proper integration with the logic and flow of our application later. Of course first of all we need to import the necessary functions and elements from the React Router Dom library:

```tsx
import { createBrowserRouter, createRoutesFromElements, Route, RouterProvider } from "react-router-dom";
```

We create a browser router (that is a router that works with browser integration by manipulating the url in the url bar) with `createBrowserRouter`. We need to pass it the routes as an argument, which is a series of placeholder `Route` elements that each combine a url pattern (the `path` property) with an element to render when the url pattern is matched. The routes are tested in order, so make sure to resolve possible path overlaps by putting the most specific matches first:

```tsx
const router = createBrowserRouter(
  createRoutesFromElements(
    <>
      <Route path="page-1" element={<Page />} />
      <Route path="page-2" element={<Page />} />
      <Route path="/" element={<Page />} errorElement={<ErrorBoundary />}>
      </Route>
    </>
  )
);
```

The last `Route` element also has an `errorElement` specified, which is used in order to provide a fallback renderer for when no path can be matched. It is a very good practice to provide such an `ErrorBoundary` fallback in order to greet your users with a decent page, even if the url they were looking for could not be found.

Finally, we just render a `RouterProvider` element with the `router` object we just created as parameter:

```tsx
  root.render(
    <RouterProvider router={router} />
  )
```

React Router (Dom) takes care of a few things for us. First of all, it matches the current url from the navigation bar in the browser with the right pattern we provided. It also extracts any dynamic elements, for example the `productId` in url patterns such as `/products/:productId` which can match `/products/1`, `/products/2`, and so on.

The second thing React Router does for us, is rendering the appropriate element (or the error element when needed).

## Dispatching from the `main content`
Great! Now it's time to do this properly for our application. First of all, we are going to get rid of the `page.tsx` file, and we will dispatch from the router to the `MainContent` widget directly.

We also add two new files with a few new pages. We add the homepage:

```tsx
// homepage/homepage.widget.tsx

import React from "react";

export const Homepage = () => <>
  <h1>Home</h1>
  <p>Welcome to the homepage</p>
</>;
```

We also add the no-found error page:

```tsx
// not-found-error/not-found-error.widget.tsx

import React from "react";

export const NotFoundError = () => <>
  <h1>Error - page not found</h1>
</>;
```

Most of the routing will be performed by the `main-content` widget, which will also take care of dispatching control to the appropriate sub-components. We add more props and state information next to the main-content state:

```ts
// main-content.state.ts
// this will be the parameter received by MainContent from the route
export type PageType = "homepage" | "products" | "product" 
// this will be the parameter created by MainContent based on the PageType plus route parsing
export type Route = { kind:"homepage" } | { kind:"products" } | { kind:"product", productId?:string }
```

The structure of the routes we will support is simple: either the homepage, the products overview, or a page with a single product (carrying the `id` of that product as parsed from the url).

The `main-content` widget takes as input the page type:

```tsx
export const MainContent = (props:{ pageType:PageType }) =>
```

Based on the page type, a `route:Route` object can be built, reading the parameters from the url thanks to the `useParams` utility:

```ts
export const MainContent = (props:{ pageType:PageType }) =>
  const route:Route = 
    props.pageType == "product" ? 
    { kind:props.pageType, ...useParams<"productId">() }
    : { kind:props.pageType }
```

> `useParams<"productId">` returns `{ productId?:string }`, which we can embed directly with the spread operator in the `route` object we are constructing. If you want to find out more about this and other Typescript patterns, you can head over to the _Introduction to Typescript_ course on GrandeOmega.

The renderer of `main-content` is now almost trivially a dispatcher that simply decides which elements vary based on the route:

```tsx
<>
  <Header products={products} />
  <CurrentUserWidget currentUserState={state.currentUser} setCurrentUser={u => setState(Updaters.currentUser(u))} />
  {
    route.kind == "homepage" ?
      <Homepage />
    : products == "unloaded" ? ProductsUnloadedWidget()  
    : products == "loading" ? ProductsLoadingWidget()
    : products == "API-error" ? 
      ProductsAPIErrorRetryWidget(state, setState)
    : route.kind == "products" ?
      ProductsWidget({...state, products}, setState)
    : route.productId != undefined && products.has(route.productId) ?
      <ProductWidget product={products.get(route.productId)!} editing={false} changeProduct={() => {}} deleteProduct={() => {}}/>
    : <NotFoundError />
  }
  <Footer />  
</>
```

As a final touch, we can add a few links to the `Header`. The `Header` should then receive the loaded products as input in order to provide links to the product pages when those are actually available:

```tsx
export const Header = (props:{ products:ProductsState }) => <>
  <div>
    <Link to="/">Home</Link>
    <Link to="/products">Products</Link>
    {
      // exercise!
      props.products == "loading" || props.products == "unloaded" || props.products == "API-error" ? <></>
      : props.products.valueSeq().toArray().map(product => 
        <Link to={`/products/${product.id}`}>{product.name}</Link>
      )
    }
  </div>
</>;
```

The `Link` element comes from React Router and is smart enough to not trigger a page reload, but rather just change the url in the navigation bar while triggering a route switch. The effect is exactly what you would expect, but there is one massive difference: the page does not get reloaded! The state of the components that are not unmounted remains the same, and just a re-render of the appropriate components is triggered. This is also a very quick process. Going back to the backend server to fetch a whole new HTML page would take time, bandwidth, and energy, while most likely a lot of information remains exactly the same: the header, the footer, the login state, and plenty more often does not change in any way across page transitions and it would be a huge waste of time and computational resources to reload it for nothing.
 
Ok, now it's time to bring it all together in the router itself. The routes are simple, and the element they instantiate is always the `MainContent` with just the right `pageType`:

```tsx
    <>
      <Route path="products" element={<MainContent pageType="products" />} />
      { // exercise! }
      <Route path="products/:productId" element={<MainContent pageType="product" />} />
      <Route path="/" element={<MainContent pageType="homepage" />} errorElement={<NotFoundError />}>
      </Route>
    </>
```

And there we go! The application now feels a lot more professional and "complete", with barely any effort.

> A note before we move on to the next chapter. We are always dispatching to the `MainContent` widget. This is done on purpose. We could dispatch to different widgets, but then we would lose their ability to preserve useful state such as the loaded products or the login state across page reloads, which would break the application and force the user to re-login and wait again for the products to load from API at every page transition. React Router mounting to the same stateful root component (`MainContent` in our case) is a winning pattern.

# Practice
Complete the single product route with all the necessary components.
Make the `ProductWidget` widget properly edit products when the user is logged in, just like the `ProductsWidget` does.
