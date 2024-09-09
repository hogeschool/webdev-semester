# Asynchronous code (`Promises`)
Web applications do not just live in the client. For example, the process by which a user logs in (or at least attempts to) with a username and password requires validation from a server.

But here's the thing: validation from a server is nowhere near instantaneous. It might take a few milliseconds in the best case, but it might also take much longer: seconds, or even minutes in extreme cases, to complete a communication with a server.

When an operation takes so much time that between its start and end a lot of other computation has taken place, we speak of _asynchronicity_, or "different timings". We need to take care of:
- how to start an async operation
- what to do/show to the user while waiting
- how to process successful results
- how to process failures and timeouts

Asynchronous communication with a server is found in every SPA around every corner. In this chapter we will learn how to use the constructs that Typescript puts at our disposal to manage async operations: `Promise`s. We will learn how to create them, build delays, use `async`/`await`, and even discuss the pitfalls associated with closures and delays.

## Login API
Let's define an asynchronous login mechanism. Even though we do not have a server that can properly validate a username and password combination against, say, a database, we can always define local mocks (fakes) of our APIs. This has several advantages:
- we don't need to wait for the backend to be done in order to start working on the frontend;
- we can simulate failures or other edge cases which are very rare during local development;
- in general, we can work with more agility and less constraints.

We start by defining what result we expect from the API. Don't worry if this is not exactly the data that will come from the backend, as long as it's possible to later translate the actual server response into what we need.
We make sure to model all the states that the application has to be able to process in the form of a discriminated union (see the _Introduction to Typescript_ course for a refresher if needed). 

For example, logging in will result in either a success or a failure because of invalid credentials or something else. In our scenario we don't care about too specific error messages such as "wrong password", "unknown username", etc. (those could also weaken security so we have to be careful not to give away too much information about users!), so we use a catch-all `"unknown error"` flag for the long tail of all possible encountered problems:

```ts
export type LoginResult = { kind:"success" } | { kind:"failure", reason:"invalid credentials"|"unknown error" }
```

We group together all API functionality in a `CurrentUserApi` object just like we did for the state updaters. The only one we have for now is the `login` promise, which is a synthetic promise that waits between 0.5s and 5s, and then with a probability of 90% resolves in, nested with a further probability of 90% a success and 10% failure because of invalid credentials. With 10% probability the promise simply fails, by invoking `reject`:

```ts
export const CurrentUserApi = {
  login:() : Promise<LoginResult> =>
    new Promise((resolve, reject) => 
      setTimeout(() => {
        if (Math.random() <= 0.9) {
          resolve(Math.random() <= 0.9 ? { kind:"success" } : { kind:"failure", reason:"invalid credentials" })
        } else {
          reject()
        }
      }, Math.random() * 4500 + 500))
}
```

> Simulating error scenarios with a high probability is an underestimated trick of great importance. Suppose you are working on your development machine and you are testing a login API. The backend is running locally, so the chance that local communication actually fails is very, very small. By controlling (and increasing) the probability of errors, you make sure that you will see them, and build code that appropriately responds to those errors. This eventually makes your application much more reliable.

Now it's time to extend the `CurrentUser` state and widget. We start with the state, because now either the user is logging in (we call this `"login-form"` step) or everything is frozen waiting for the API call that will log us in to complete (we call this `"waiting-for-api-response"` step). It's very important to model waiting states explicitly, because in real life delays are long enough that a user might attempt new interactions, whereas you shouldn't start a new API call to login if the previous one still has not finished:


```tsx
export type CurrentUserState = (User & { kind:"logged-in" }) | (LoginFormState  & { kind:"login-form" | "waiting-for-api-response", validation?:string })
```

Of course we need to adjust the state transitions between these steps:

```tsx
export const CurrentUserStateUpdaters = {
  logout:(_:CurrentUserState) : CurrentUserState => initialUserState(),
  waitingForApiResponse:(s0:CurrentUserState) : CurrentUserState => s0.kind == "logged-in" ? s0 :
    ({...s0, kind:"waiting-for-api-response" }),
  apiFailure:(s0:CurrentUserState) : CurrentUserState => s0.kind != "waiting-for-api-response" ? s0 :
    ({...s0, kind:"login-form", validation:"API error" }),
  login:(s0:CurrentUserState) : CurrentUserState => ({ kind:"logged-in", email:s0.email }),
  ...simpleUpdater<CurrentUserState>()("email"),
  password:(_:Updater<string>) : Updater<CurrentUserState> => s0 => s0.kind == "logged-in" ? s0 :
    ({...s0, password:_(s0.password)}),
  validation:(_:Updater<string|undefined>) : Updater<CurrentUserState> => s0 => s0.kind == "logged-in" ? s0 :
    ({...s0, validation:_(s0.validation)}),
}
```

Notice that management of the API state transitions takes up a lot of space and complexity. This is normal. In the _Advanced React_ course we will see how to generalize this pattern in order to reuse it instead of rebuilding it every time. For now, we do it manually.

Finally, we extend the `CurrentUser` widget to manage the three different states: waiting for the API, being logged in already, and waiting for the user to insert credentials and press the "login" button:

```tsx
export const CurrentUserWidget = (props:CurrentUserProps) =>
  props.currentUserState.kind == "waiting-for-api-response" ?
    <div>Logging in...</div>
  : props.currentUserState.kind == "logged-in" ?
    <>
      <p>Logged in as: {props.currentUserState.email}</p>
      <button onClick={_ => props.setCurrentUser(CurrentUserStateUpdaters.logout)}>Logout</button>
    </>
  : <>
      <input type="email" onChange={e => props.setCurrentUser(CurrentUserStateUpdaters.email(replaceWith(e.currentTarget.value)))} value={props.currentUserState.email} />
      <input type="password" onChange={e => props.setCurrentUser(CurrentUserStateUpdaters.password(replaceWith(e.currentTarget.value)))} value={props.currentUserState.password} />
      <button onClick={_ => {
        props.setCurrentUser(CurrentUserStateUpdaters.waitingForApiResponse)
        CurrentUserApi.login().then(loginResult => 
          loginResult.kind == "success" ?
            props.setCurrentUser(CurrentUserStateUpdaters.login)
          : props.setCurrentUser(CurrentUserStateUpdaters.validation(replaceWith<string|undefined>(loginResult.reason)))
        ).catch(() => 
          props.setCurrentUser(CurrentUserStateUpdaters.apiFailure)
        )
      }}>Login</button>
    </>
```

Ok, nice, but let's clean this up a little bit. The widget is now doing three separate things, and the "login" button event handler is also too messy. We start with that one, by simply introducing a state updater that performs the whole API call operation:

```ts
export const CurrentUserStateUpdaters = {
  invokeLoginAPI(setCurrentUser: (_:Updater<CurrentUserState>) => void) {
    setCurrentUser(CurrentUserStateUpdaters.waitingForApiResponse);
    CurrentUserApi.login().then(loginResult => loginResult.kind == "success" ?
      setCurrentUser(CurrentUserStateUpdaters.login)
      : setCurrentUser(CurrentUserStateUpdaters.validation(replaceWith<string | undefined>(loginResult.reason)))
    ).catch(() => setCurrentUser(CurrentUserStateUpdaters.apiFailure)
    );
  },  
  // ...
```

The current user widget is now a little cleaner already thanks to this monster updater being moved away:

```tsx
export const CurrentUserWidget = (props:CurrentUserProps) =>
  props.currentUserState.kind == "waiting-for-api-response" ?
    <div>Logging in...</div>
  : props.currentUserState.kind == "logged-in" ?
    <>
      <p>Logged in as: {props.currentUserState.email}</p>
      <button onClick={_ => props.setCurrentUser(CurrentUserStateUpdaters.logout)}>Logout</button>
    </>
  : <>
      <input type="email" onChange={e => props.setCurrentUser(CurrentUserStateUpdaters.email(replaceWith(e.currentTarget.value)))} value={props.currentUserState.email} />
      <input type="password" onChange={e => props.setCurrentUser(CurrentUserStateUpdaters.password(replaceWith(e.currentTarget.value)))} value={props.currentUserState.password} />
      <button onClick={_ => CurrentUserStateUpdaters.invokeLoginAPI(props.setCurrentUser)}>Login</button>
    </>
```

We now split off the three subwidgets and we end up with the cleanest variation we could have possibly hoped for. We must first split the states:

```tsx
export type CurrentUserLoggedInState = (User & { kind:"logged-in" })
export type CurrentUserLoggingInState = (LoginFormState  & { kind:"login-form" | "waiting-for-api-response", validation?:string })
export type CurrentUserState = CurrentUserLoggedInState | CurrentUserLoggingInState
```

Then we split the three widgets. The waiting widget is really simple:

```tsx
export const WaitingForLoginAPIResponseWidget = () => <div>Logging in...</div>;
```

The logged-in widget for now is also super simple because it does only show the email and perform the logout action:

```tsx
export const LoggedInWidget = (props: CurrentUserProps) => 
  <>
    <p>Logged in as: {props.currentUserState.email}</p>
    <button onClick={_ => props.setCurrentUser(CurrentUserStateUpdaters.logout)}>Logout</button>
  </>
```

The real challenge comes into play when dealing with the logging-in widget, because that one does not depend on the `CurrentUserState`, but rather on the `CurrentUserLoggingInState`, which we have to pass separately:

```tsx
export const LoginFormWidget = (props: CurrentUserProps, currentUserState:CurrentUserLoggingInState) => 
  <>
    <input type="email" onChange={e => props.setCurrentUser(CurrentUserStateUpdaters.email(replaceWith(e.currentTarget.value)))} value={props.currentUserState.email} />
    <input type="password" onChange={e => props.setCurrentUser(CurrentUserStateUpdaters.password(replaceWith(e.currentTarget.value)))} value={currentUserState.password} />
    <button onClick={_ => CurrentUserStateUpdaters.invokeLoginAPI(props.setCurrentUser)}>Login</button>
  </>
```

The current user widget now becomes very clean and straightforward:

```tsx
export const CurrentUserWidget = (props:CurrentUserProps) =>
  props.currentUserState.kind == "waiting-for-api-response" ?
    WaitingForLoginAPIResponseWidget()
  : props.currentUserState.kind == "logged-in" ?
    LoggedInWidget(props)
  : LoginFormWidget(props, props.currentUserState)
```

> Notice that we need to pass the state explicitly to the `LoginFormWidget` (`props.currentUserState`). This is done because we have already performed a check on it and so the type is refined; doing so saves the extra check of the `props.currentUserState.kind` in the `LoginFormWidget`. As an alternative, we could have simply turned on all the widgets and let them perform the right check, but this would rob us of the mutual exclusion. This is just a matter of taste, as long as type safety is maintained.

This last exercise is not purely meant for aesthetic reasons. Software engineering swears by the principle of "single responsibility": every construct in our software should do one thing, and one thing only. It is very hard for humans to keep track of multiple things at the same time, especially when dealing with the complexity of code, and by enforcing every unit of code to be also a semantic unit with a clear name and goal makes things a lot easier to maintain.

The `CurrentUser` widget now has only one goal: to _dispatch_ control to the right sub-widget. Those sub-widgets, which right now look absolutely trivial, will grow in complexity very quickly. Styling, animations, validation, and so on will eventually turn our three sub-widgets from the tiny little thingies that they are into serious components. And by then, perhaps the time will have come to split those as well.

> Note that big files are absolutely ok. Oversplitting just for the sake of splitting is not a good practice. Splitting code into different files should be done along the lines of semantics. If a login form has a lot of HTML because of styling and design reasons, by all means: keep it in one file! As always, when it comes to software engineering, there is no substitute for logical reasoning skills, and dogma can be a pretty dangerous thing.


## Products API
We will now extend our use of asynchronous calls. We will load the products for the overview with a promise that simulates getting the products from the server.

We start by setting up the mocked API call. The mocked call has the same percentages of success and failure of the mocked login API call, but we want to be able to tweak them separately so we will not generalize. The mocked call can fail in two ways: a full-blown `reject`, which needs to be handled with `.catch`, as well as a soft failure:

```ts
import { sampleProducts } from "./mocked-products-response"
export type ProductsResult = { kind:"success", value:typeof sampleProducts } | { kind:"failure" }

export const ProductsApi = {
  loadProducts:() : Promise<ProductsResult> =>
    new Promise((resolve, reject) => 
      setTimeout(() => {
        if (Math.random() <= 0.9) {
          resolve(Math.random() <= 0.9 ? { kind:"success", value:sampleProducts } : { kind:"failure" })
        } else {
          reject()
        }
      }, Math.random() * 4500 + 500))
}
```

The products state must now encode the fact that the products may be in all possible stages of the asynchronous lifecycle: unloaded, loading, error, and loaded:

```ts
export type ProductsState = "unloaded" | "loading" | "API-error" | Map<ProductState.Product["id"], ProductState.Product>
export type MainContentState = { products:ProductsState , currentUser:CurrentUserState };
```

All operations on the products `Map` can now only be performed when the products state is loaded, so let's add some quality of life to our code base and define a little tool that invokes an inner updater on the products map only when it's actually possible to do so:

```ts
const onlyIfProductsLoaded = (updater:Updater<Map<ProductState.Product["id"], ProductState.Product>>) : Updater<ProductsState> =>
  products => products == "unloaded" || products == "loading" || products == "API-error" ? products : updater(products)
```

All updaters on the products can just use this wrapper, saving us from repeating this boring condition over and over again:

```ts
  addProduct: () => Updaters.products(onlyIfProductsLoaded(_ => { const p = ProductState.createEmptyProduct(); return _.set(p.id, p); })),
  changeProduct: (id:ProductState.Product["id"]) => (updateProduct:Updater<ProductState.Product>) => Updaters.products(onlyIfProductsLoaded(_ => _.update(id, ProductState.createEmptyProduct(), updateProduct))),
  removeProduct: (id:ProductState.Product["id"]) => Updaters.products(onlyIfProductsLoaded(_ => _.remove(id))),
```

Loading the products involves calling the API and then performing the appropriate state transitions. Notice that we make the `invokeLoadProductsAPI` smart enough to set the state to `"loading"` right before the API is actually invoked, so that observers can always see the most logical up to date state of the products loader:

```ts
  invokeLoadProductsAPI:(state:MainContentState, setState: Fun<Updater<MainContentState>,void>) => {
    if (state.products == "unloaded" || state.products == "API-error") setState(Updaters.products(replaceWith<ProductsState>("loading")))
    ProductsApi.loadProducts().then(result => {
      if (result.kind == "success") {
        setState(Updaters.products(_ => result.value))
      } else {
        setState(Updaters.products(replaceWith<ProductsState>("API-error")))
      }
    }).catch(_ => setState(Updaters.products(replaceWith<ProductsState>("API-error"))))
  },  
```

The widget now invokes the API via `useEffect`, which is essentially called at every effectful transition (mount, render, unmount, etc.) of the component. Given that `useEffect` will be called many times, we make sure that we only invoke the API if the state is still unloaded, in order to prevent accidentally bombarding our server with useless API calls from a single client:

```tsx
export const MainContent = () => {
  const [state,setState] = useState<MainContentState>(initialMainContentState)
  useEffect(() => {
    if (state.products == "unloaded")
      Updaters.invokeLoadProductsAPI(state, setState)
  })
  // ...
```

> `useEffect` supports two extra variations. We may pass to `useEffect` an array of dependencies, and we may also return a cleanup function that will be called when the component is unmounted in order to, for example, close connections. So basically there are ways to make `useEffect` simulate almost all of the component lifecycle methods that we have seen in a previous chapter.

The rest of the widget must now take care of the different states the products might be in, in order to show the appropriate message to the user:

```tsx
  <>
    <CurrentUserWidget currentUserState={state.currentUser} setCurrentUser={u => setState(Updaters.currentUser(u))} />
    {
      state.products == "unloaded" ? <div>No products</div>
      : state.products == "loading" ? <div>Loading products...</div>
      : state.products == "API-error" ? 
        <div>
          Error loading products...
          <button onClick={_ => Updaters.invokeLoadProductsAPI(state, setState)}>Try again</button>
        </div>
      :
        <>
          <ul>
          {
            state.products.valueSeq().sortBy(p => p.id).map(p => 
              <li key={p.id}>
                <Product editing={state.currentUser.kind == "logged-in"} product={p} changeProduct={u => setState(Updaters.changeProduct(p.id)(u))} deleteProduct={() => setState(Updaters.removeProduct(p.id))} />
              </li>).toArray()
            }
          </ul>
          <button disabled={state.currentUser.kind != "logged-in"} onClick={e => setState(Updaters.addProduct())}>Add new product</button>
        </>
    }
  </>
```

Notice that we let the user retry when a loading operation fails, but we could just as well have done it automatically with a `setTimeout` in the `.catch` of the promise. Depending on design and requirements you might see one or the other scenario.

Just like we did for the login widget, it's good practice to anticipate on the growth of our codebase and split our widget up. We know that eventually the different screens will grow in HTML and styling, so we can anticipate on that, in order to keep the logic of the application easier to understand and also to help different people work on the same codebase together with less conflicts.

Most of the sub-widgets can be split easily because they don't depend on the refined state. The one with a dependency on the refined state though is the one that actually shows the products. First, we refine the state definition so that we give a proper name to the loaded products state (we will need to refer to this in the split widget):

```ts
export type LoadedProductsState = Map<ProductState.Product["id"], ProductState.Product>
export type ProductsState = "unloaded" | "loading" | "API-error" | LoadedProductsState

export type LoadedMainContentState = { products:LoadedProductsState , currentUser:CurrentUserState };
export type MainContentState = { products:ProductsState , currentUser:CurrentUserState };
```

> It's also time to move the `ProductsState` and associated definitions to a more reasonable file, like `products.state.ts`.

The products widget will now depend directly on the `LoadedMainContentState`, and not the `MainContentState`:

```tsx
export const ProductsWidget = (state: LoadedMainContentState, setState: React.Dispatch<React.SetStateAction<MainContentState>>) =>
  <>
    <ul>
      {state.products.valueSeq().sortBy(p => p.id).map(p => <li key={p.id}>
        <Product editing={state.currentUser.kind == "logged-in"} product={p} changeProduct={u => setState(Updaters.changeProduct(p.id)(u))} deleteProduct={() => setState(Updaters.removeProduct(p.id))} />
      </li>).toArray()}
    </ul>
    <button disabled={state.currentUser.kind != "logged-in"} onClick={e => setState(Updaters.addProduct())}>Add new product</button>
  </>
```

The main content widget just acts as a dispatcher for the sub-widgets we just split off:

```tsx
export const MainContent = () => {
  const [state,setState] = useState<MainContentState>(initialMainContentState)
  useEffect(() => {
    if (state.products == "unloaded")
      Updaters.invokeLoadProductsAPI(state, setState)
  })
  const products = state.products
  return <>
    <CurrentUserWidget currentUserState={state.currentUser} setCurrentUser={u => setState(Updaters.currentUser(u))} />
    {
      products == "unloaded" ? ProductsUnloadedWidget()  
      : products == "loading" ? ProductsLoadingWidget()
      : products == "API-error" ? 
        ProductsAPIErrorRetryWidget(state, setState)
      : ProductsWidget({...state, products}, setState)
    }
  </>
}
```

Note that Typescript here needs a little nudge. Given that we are refining the type of `state.products`, by extracting the `products` as a separate variable, in the various branches of the conditional `products` will change type in order to reflect that the `products` are not any of the status strings but rather the `Map` of actual products themselves. By reassembling the state `{...state, products}` it now has a different type which matches what we need, namely `LoadedMainContentState`.

We could have also gone "the lazy way" and just added a control in the `ProductsWidget`. The conditionals controls for such widgets would then be replicated a bit across different sub-widgets, but that is an acceptable compromise. This is what it would look like:

```tsx
export const areProductsLoaded = (state:MainContentState) : state is LoadedMainContentState =>
  state.products != "API-error" && state.products != "loading" && state.products != "unloaded"

export const ProductsWidget = (state: MainContentState, setState: React.Dispatch<React.SetStateAction<MainContentState>>) =>
  areProductsLoaded(state) ?
    <>
      <ul>
        {state.products.valueSeq().sortBy(p => p.id).map(p => <li key={p.id}>
          <Product editing={state.currentUser.kind == "logged-in"} product={p} changeProduct={u => setState(Updaters.changeProduct(p.id)(u))} deleteProduct={() => setState(Updaters.removeProduct(p.id))} />
        </li>).toArray()}
      </ul>
      <button disabled={state.currentUser.kind != "logged-in"} onClick={e => setState(Updaters.addProduct())}>Add new product</button>
    </>
  : <></>
```

If we add a series of utils like `areProductsLoaded` to the state definition we can increase reuse. Note that such utility functions can be reused also in the previous implementation, namely it does not matter where the check is: a type-refinement check is always useful!


## Actual API calls via `fetch`
Let's spend a short and insufficient section about how to perform actual API calls. This topic is quite large, and we are actually not going to talk about APIs in this course, but it feels necessary to go for a quick introduction anyway because you will need it in practice virtually everywhere!

> The topic of APIs will be covered in the _Introduction to ASP .Net_ course of the academy, where we talk extensively about backend, security, performance, and much more.

### API
An API, or Application Programming Interface, defines a series of operations that can be invoked in order to interact with a web application in a predefined manner. 
The most widely used protocol for building modern APIs is REST, or REpresentational State Transfer. REST uses a series of "methods", the most important being `GET`, `POST`, `PUT`, and `DELETE`, which provide a first hint about what sort of operation is being performed. REST methods always have a url. By (useful) convention, the URL will refer to some semantic operation or entity.

#### JSON
An API manipulates data. REST APIs manipulate data in a specific format: JSON. JSON is a human-readable data format that originates from the JavaScript world. This origin is even reflected in the acronym: JavaScript Object Notation!

JSON objects are built as follows: curly brackets, containing a series of comma-separated key-colon-value pairs. For example:

```json
{"name":"John"}
```

JSON supports nesting, as well as arrays (sequences of elements within curly brackets). This means that we can model complex structures, for example:

```json
{"employees":[
  { "firstName":"John", "lastName":"Doe" },
  { "firstName":"Anna", "lastName":"Smith" },
  { "firstName":"Peter", "lastName":"Jones" }
]}
``` 

#### `GET`
The `GET` method relates to reading or querying operations. For example, we might want to to get all chatrooms currently available:

```
GET http://localhost:9200/chatrooms
```

or we might want to get the structure of the footer of the CMS for the english language:

```
GET http://localhost:5000/cms/footer?lang=en
```

Notice in particular that the url, again by convention, identifies both an entity we want to read (`cms/footer`), as well as extra query parameters (`lang=en`) that act as a modifier when reading the resource in question.

If you invoke a `GET` method, you will get a JSON object as a response.

#### `POST` and `PUT`
The `POST` and `PUT` methods respectively create or change an entity. The entity is submitted in the form of extra data, the so-called _body_ of the request.

We will usually `POST` to a url referring to a whole resource, such as `http://localhost:5000/users` to create a new user. The body will have to be the user that we want to create, for example:

```json
{ "name":"John", "surname":"Doe" }
``` 

`PUT` changes an existing entity. Often the url we `PUT` to will contain to the identifier of the entity either as a query parameter `http://localhost:5000/users?id=12345` or as part of the url `http://localhost:5000/users/12345`.

#### `DELETE`
As implied by its name, `DELETE` deletes an entity. Just like `PUT`, the url will contain the id of the entity to delete, and the body will be empty because the id is sufficient to identify what needs to be deleted.

#### Oft forgotten, but surprisingly relevant: `HEAD`
The little brother/sister of `GET`, `HEAD`, is often forgotten for no good reason. `HEAD` is often used as a lightweight alternative to `GET` which does not return the content. A common use of `HEAD` is to check whether an application is still alive and responsive, without forcing expensive database or computational operations by performing a `GET` request.

#### REST headers
REST requests may also include some extra headers, such as authorization, accepted language, security tokens, and much more depending on the application. A full request could look like this:

```rest
POST https://example.com/comments HTTP/1.1
Content-Type: application/json
Authorization: token abcdxyzw

{
  "name":"sample",
  "time":"Wed, 21 Oct 2015 18:27:50 GMT"
}
```


### Structured APIs
REST APIs do not have a very well defined structure, and this can lead to a wild growth of slightly different, slightly incompatible naming and url conventions that, in a production setting, will possibly cause either bugs or overhead (because of lack of documentation and standardization).

There exist a series of standards that state how to structure and organize REST APIs according to a clear format which, even better, is machine readable so that consuming an API built according to one of these formats can be simplified or partially automated by tooling.

#### OpenAPI
[OpenAPI](https://www.openapis.org) defines a descriptor format for REST APIs. It specifies which urls are available, with what format, what parameters, what methods, and what headers/authentication options. Thanks to OpenAPI we can generate documentation automatically, as well as consume an OpenAPI with ease thanks to extensive tooling support.

#### OData/Graph API
[OData](https://www.odata.org) is an advanced REST query language. It allows us to write queries that traverse the relations of a data model, by including related entities, as well as further refining the results by filtering, sorting, and selecting attributes. OData is very common in the enterprise world: most Microsoft products (SharePoint, Dynamics, etc.) support OData out of the box, just like SalesForce and SAP.

OData makes it possible to perform a single REST call instead of potentially dozens in order to obtain multiple layers of connected entities, for example:

```odata
GET serviceRoot/People?$expand=Trips($filter=Name eq 'Trip in US')
```

will return an array of people, each with an array of trips named `"Trip in US"`. Doing this with a vanilla REST API would require getting the people first, and then for each person performing a REST API call to the trips of that person, and filtering the results. With OData we get, with one single call, exactly the relevant data and nothing more, which is faster and requires less bandwidth, to the benefit of the application performance.

More information about the syntax and semantix of OData operators can be found at [OData](https://www.odata.org/)


### `fetch`
We can invoke one of these REST API endpoints with the built-in `fetch` function, which returns a `Promise` based on a descriptor object of the endpoint that we want to invoke (url, method, headers, and body):

```ts
export const register = async (person: Person): Promise<void> => {
  await fetch("api/registration", {
    method: "POST",
    headers: {
      "content-type": "application/json"
    },
    body: JSON.stringify(person)
  }
  )
}
```

The result (assume we have called it `res`) obtaining by awaiting a call to `fetch` can be checked with `res.ok`, in order to verify whether or not the API call terminated successfully, and if that is the case we can parse the body as JSON by invoking `await res.json()`. The result needs to be validated and converted to a statically typed construct in order to maintain type safety and avoid accidental errors.


## The hidden monster under the bed
Promises and asynchronous programming implies _delays_. Namely, we will need to write our code in such a way as to be robust with respect to multiple things happening (or _processing_) at the same time.

For example, what happens if the user is typing while an API is called in the background in order to validate the input? Do we wait for it to be done, risking the validation to be irrelevant? Do we fire a new API call for every input change, risking overwhelming the server with useless requests? The truth of course lies in the middle, but in any case, the lesson here is that a naïve approach will simply not work: we will cause performance issues, which are bad, and correctness issues, which are terrible.

Let's simulate this by defining two operations that attempt to write to the same state with some asynchronicity:

```tsx
const BrokenByDesign = () => {
  const [count,setCount] = useState<number>(0)

  return <>
    <p>Count = {count}</p>
    <button onClick={_ => setCount(count + 1)}>+1</button>
    <button onClick={_ => setTimeout(() => setCount(count + 10), 1500)}>+10</button>
    <button onClick={_ => setTimeout(() => setCount(count + 100), 3500)}>+100</button>
  </>
}
```

Here we simulate two asynchronous state changes with `setTimeout`. In practice we could have some state change operations that are performed in the `.then` callback of a promise, leading to the same result. Because the `setTimeout` contains a lambda function which holds a reference to an old value of `count`, if we click on `+10` or `+100` and then right away on `+1`, the changes done by `+1` will be undone when `setCount` triggers inside `setTimeout`.

Now take a second to imagine how nasty this could be in practice. Usually the problem will not be so neatly bundled up in a single component, and replicating it will require a slow connection, which you don't have on your local dev environment, as well as a user doing things a bit erratically (which you don't do because you treat your software with love and care :D). Jokes aside, this sort of concurrency issue is a nightmare to debug because of how hard it is to replicate such bugs reliably, followed by how hard it is to understand and fix them properly...

> The way lambdas in Typescript and other languages hold values is a mechanism known as "closures". To see how closures work, see the _Introduction to Typescript_ course where we discuss this topic at length when introducing functions and lambdas!

The solution is quite simple: never use a state value in a closure, but always use an `Updater`!!! This is why we introduced it as a best practice pattern before: by using updaters, we ask `setState` (or `setCount` in our little broken example) to first read and inject the latest value of the state in the updater so that if something happened in the meantime that made the current closure invalid, the effects are rendered harmless:

```tsx
const UnbrokenByDesign = () => {
  const [count,setCount] = useState<number>(0)

  return <>
    <p>Count = {count}</p>
    <button onClick={_ => setCount(count + 1)}>+1</button>
    <button onClick={_ => setTimeout(() => setCount(count => count + 10), 1500)}>+10</button>
    <button onClick={_ => setTimeout(() => setCount(count => count + 100), 3500)}>+100</button>
  </>
}
```

Such a tiny change mitigates such a nasty class of bugs. Nice eh?


# Practice
Define mocked APIs for every single operation related to the creation, deletion, and editing of products. Use them in code. Introduce appropriate delays in the mocked APIs so that you can make sure that your loaders work.

Build the frontend in such a way that no other editing operations can be started while any editing operation is being processed by the backend. You will need to centralize state management for this, but fortunately we have explored this topic extensively!
