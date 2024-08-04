# Controlled components
How do we build components that can be composed easily and seamlessly? React makes it possible to compose components _visually_, that is we can nest components or put them next to each other without any limitation.

Components do not just have a visual aspect though. The beating heart of a component is its ability to process data, and in particular to maintain a mutable state which changes over time. Each component creates an endless stream of states, and the trick to building a large application with lots of components is to _braid_ together all of these streams of states in order for the application to end up with a single state which makes sense.

Ok, this is all very abstract, so let's start with an example. Suppose we have a Netflix-like movie catalogue. Each movie has a `MoviePreview` component. If the mouse hovers on the movie icon, then the preview should start, and if the mouse leaves the icon, the preview should stop. But, unfortunately, there's a bug! Let's say that sometimes the browser fails to trigger the "on mouse leave event", and we are left with not one, but two previews playing. Woe and anguish! Because each `MoviePreview` component has its own separate state and cannot see (or change) the state of other components, we are done for: there's no way to fix this without a major refactoring.

> By the way, this is a true story. I actually saw this bug in production and it was a nightmare because the dev team tried a million hacks and workarounds without success.

The solution to this, and an infinity of similar issues, lies in a global approach to state management and isolation of components which the React documentation calls _controlled components_. Controlled components do not manage their state internally, but rather receive their current state and the `setState` function from the parent component. Of course the parent component is also controlled when we use this pattern, so it also receives current state and `setState` from its own parent, and so on. 

The only component with state is the root component. All other components are fully controlled. 

According to this scenario, the `MoviePreview` component will not have an internal state maintaining whether or not the preview is playing, but rather it will feature the following properties:

```tsx
{
  IsPlaying:boolean,
  setIsPlaying:(newValue:boolean) => void
}
```

The `IsPlaying` attribute is basically the state of the component, but it is passed to the component itself by the parent. `setIsPlaying` can be invoked by the component whenever the component wants to _request_ a change in its own state. And here's the trick! The parent component knows which preview is actually playing, so if one component asks to start playing, then any component that was previously playing can be set to `IsPlaying: false`.

The fact that we moved the state up one level has made it possible to _coordinate_ the state management of the child components: instead of the individual changes in the state _disappearing_ inside the child component, the state changes are moved up to a place where they can be bundled in harmony. 

The most important result though is that controlled components _compose_ more easily. Compose means that they are easier to use in new applications or new contexts than the one they were originally written for. And this is an essential property of a modular application.


# Back to our products
Let's go back to our products. We had just made products deletable. Now, a very common scenario that we will face often is that, for styling reasons, some operations such as removing a product are triggered by buttons inside the child component. Imagine that our CSS developers want some specific styling/HTML structure inside the `Product` component, and they want the `delete` button in there.

Because the product widget does not have access to the state of the parent where the collection of products is actually stored, it is not possible to perform the deletion from inside the product widget itself because this is only possible in the main component widget. The widget must therefore become a controlled component, at least for the deletion. We extend the properties (parameters) of the product widget with a `deleteProduct` function:

```tsx
type ProductProps = { product: ProductState.Product; deleteProduct:() => void };
```

The widget itself may now render a delete button which, when clicked, simply triggers `props.deleteProduct`:

```tsx
export const Product = (props: ProductProps): JSX.Element => {
  // ...
  return <div className='product'>
    // ...
    <button onClick={e => props.deleteProduct()}>Delete product</button>
    <img src={props.product.image} />
  </div>
}
```

The change so far is minimal, and that is good because we never want end-of-the-world refactorings that are too risky. But! There is a subtle difference here: the product widget is not really performing a deletion when the button is pressed, but rather it's just _requesting_ it. The parent component will be the one actually responsible for performing the change, but it might also decide _not_ to do it for whatever business logic reasons.

> Controlled components indeed always _request_ state changes, but it's up to the business logic of the ancestor components to decide whether or not the requested state change is even allowed. This means that a component might be enhanced with additional validation logic or extra functionality without even having to change its own internal implementation!

The main widget now has to connect the `deleteProduct` property of `Product` to the right state updater, and this is also quite easy to do (and the Typescript compiler will be very helpful with these refactorings because it will provide us with a very clear todo list of what still needs to be plugged where):

```tsx
export const MainContent = () => {
  // ...
  <Product product={p} deleteProduct={() => setState(Updaters.removeProduct(p.id))} />
  // ...
```

There! Easy peasy. Now let's move the whole product state to the main content widget: right now we have two versions of each product' data, one in the main content (which is outdated) and one in each product which is the most up to date but at the same time hidden inside the product' state itself and not shared with the rest of the application. The product widget itself will now become fully controlled:

```tsx
type ProductProps = { product: ProductState.Product; changeProduct:(_:Updater<ProductState.Product>) => void; deleteProduct:() => void };

export const Product = (props: ProductProps): JSX.Element => {
  return <div className='product'>
    <h2>
      <input type='text' value={props.product.name} onChange={e => props.changeProduct(ProductState.Updaters.name(replaceWith(e.currentTarget.value))) }/>
    </h2>
    <p>
      <input type='text' value={props.product.description} onChange={e => props.changeProduct(ProductState.Updaters.description(replaceWith(e.currentTarget.value))) }/>
    </p>
    <button onClick={e => props.deleteProduct()}>Delete product</button>
    <img src={props.product.image} />
  </div>
}
```

It looks almost the same, with some really easy substitutions: instead of reading from `state` we read from `props.product`, and instead of invoking `setState` we just invoke `props.changeProduct`, so the difference is really minimal but the possibilities of this design pattern are so powerful that one might wonder why on Earth would you not do this?!?!


# Orthogonal states
Let's add a fake login system. We will allow users to login (without a proper backend or validation for now), and after a user logs in we will store this in a `CurrentUserState`. 

As is quite common, we will restrict the ability of users to edit products. In particular, we will only allow logged in users to edit a product. If no user is logged in, editing will be disabled.

This interaction between the state of the login widget and the product/main content widgets requires ensuring that the state of the login widget does not remain stuck and hidden inside the login widget itself, but rather we need this state to be available to the whole application. As a consequence of this we need to make the login widget a controlled component. Let's begin!

We start with a `CurrentUser` widget (a controlled component of course) and its corresponding state. The state is polymorphic because a user might be logged in or still logging in, and in these two cases there will be different data in the state:

```ts
export type CurrentUserState = (User & { kind:"logged-in" }) | (LoginFormState & { kind:"login-form" })
export type User = { email:string }
export type LoginFormState = { email:string, password:string }
```

> Remember the course _Introduction to Typescript_ if you need a little refresher on what this syntax means exactly!

It is a good practice to define the initial valuse for the whole state and optionally for the partial states:

```ts
const initialLoginFormState = () : LoginFormState => ({ email:"", password:"" })
export const initialUserState = () : CurrentUserState => ({ kind:"login-form", ...initialLoginFormState() })
```

Now it's time to define the updaters. We need to define updaters for the fields (`email` and `password`, but the `password` only when the state is not already `logged-in` because in that case it makes no sense). We also need a way to switch from logged in to logged out, and viceversa:

```ts
export const CurrentUserStateUpdaters = {
  logout:(_:CurrentUserState) : CurrentUserState => initialUserState(),
  login:(s0:CurrentUserState) : CurrentUserState => ({ kind:"logged-in", email:s0.email }),
  email:simpleUpdater<CurrentUserState>()("email"),
  password:(_:Updater<string>) : Updater<CurrentUserState> => s0 => s0.kind == "logged-in" ? s0 :
    ({...s0, password:_(s0.password)}),
}
```

> Of course the login transition in particular does not work like this. We should perform an API call here, and not just dumbly accept the email/password combination without checking it. For now, let's pretend we don't know any better but we will learn all there is to learn on the topic of API calls in a later chapter!

Great. Let's head over to the widget itself:

```tsx
type CurrentUserProps = { currentUserState:CurrentUserState, setCurrentUser:(_:Updater<CurrentUserState>) => void }

export const currentUserWidget = (props:CurrentUserProps) =>
  props.currentUserState.kind == "logged-in" ?
    <>
      <p>Logged in as: {props.currentUserState.email}</p>
      <button onClick={_ => props.setCurrentUser(CurrentUserStateUpdaters.logout)}>Logout</button>
    </>
  : <>
      <input type="email" onChange={e => props.setCurrentUser(CurrentUserStateUpdaters.email(replaceWith(e.currentTarget.value)))} value={props.currentUserState.email} />
      <input type="password" onChange={e => props.setCurrentUser(CurrentUserStateUpdaters.password(replaceWith(e.currentTarget.value)))} value={props.currentUserState.password} />
      <button onClick={_ => props.setCurrentUser(CurrentUserStateUpdaters.login)}>Login</button>
    </>
```

This widget follows the usual pattern: receive as props the current state and a callback for requesting changes to it, and then we render the appropriate HTML or custom React elements and connect the right state updaters to the callback.

> Ok, the widget itself is easy enough, but there is one point of attention that for now requires no action but will probably do so in the future as the application grows. The `currentUserWidget` does not follow the _single responsibility principle_ because it does two things (login _and_ logout). Eventually this widget will need to become two separate widgets.

We can now add the current user state to the main component state:

```tsx
export type MainContentState = { products: Map<ProductState.Product["id"], ProductState.Product>, currentUser:CurrentUserState };
export const Updaters = {
  ...simpleUpdater<MainContentState>()("currentUser"),
  // ...
}
```

And instantiate the current user widget inside the main content widget somewhere, plugging the current user state and the appropriate state setter as required:

```tsx
<CurrentUserWidget currentUserState={state.currentUser} setCurrentUser={u => setState(Updaters.currentUser(u))} />
```

That's it...Now we just pass to the products an extra flag to inhibit editing:

```tsx
type ProductProps = { editing:boolean, product: ProductState.Product; changeProduct:(_:Updater<ProductState.Product>) => void; deleteProduct:() => void };

export const Product = (props: ProductProps): JSX.Element => {
  return <div className='product'>
    <h2>
      <input readOnly={!props.editing} type='text' value={props.product.name} onChange={e => props.changeProduct(ProductState.Updaters.name(replaceWith(e.currentTarget.value))) }/>
    </h2>
    <p>
      <input readOnly={!props.editing} type='text' value={props.product.description} onChange={e => props.changeProduct(ProductState.Updaters.description(replaceWith(e.currentTarget.value))) }/>
    </p>
    <button disabled={!props.editing} onClick={e => props.deleteProduct()}>Delete product</button>
    <img src={props.product.image} />
  </div>
}
```

 We pass this flag via the main content based on the value of `CurrentUserState`:

 ```tsx
 <Product editing={state.currentUser.kind == "logged-in"} product={p} changeProduct={u => setState(Updaters.changeProduct(p.id)(u))} deleteProduct={() => setState(Updaters.removeProduct(p.id))} />
 ```

And once again, there we go! We now need to first login in order to be able to modify, add, or delete products.

In the next lesson, we will extend the login logic in order to perform some asynchronous calls in order to simulate the way a backend server would work.

# Practice
Make it possible to mark products as favorite, but no more than three. Store the favorite products in a `Map` (from the _immutable_ library) in the main state.

Let's move the state to the root, and let's move the current user widget to the header. 

Let's top it off by splitting the `currentUserWidget` into two widgets, one for logging in and one for the already logged in state. Split the state, the updaters, and the widgets.

