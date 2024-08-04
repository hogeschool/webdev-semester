# State
Data in a real-world application is not just readonly. As the user interacts with the frontend, the application needs to track what is happening in the form of a mutable state. This state can contain all sorts of information:
- the content of a form that is being input by the user;
- the status of an API call ("loading"/"loaded") in order to show a loader animation;
- the current user logged into the system;
- and much more.

State management is the core of all of our software development activities. If the wrong state ends up being stored, then the application will not process the right information.

**One might even say that the goal of all software systems is to turn user interactions into the _desired_ state.**

Failure to do so will result in an application that does not work. For this reason we should take state management and the software engineering surrounding it very very seriously!

## Stateful components
Reacts makes it possible to attach a mutable state to each of our components. There are two ways of doing this: hooks and class components.
Hooks are very clean and elegant, but they are very recent (at the time of writing at least). They can do _almost_ everything that class components can, but not quite.
Class components are the oldest way of adding mutable state to a component, and because of this they can do everything and you will encounter class components in many older code bases that are not necessarily legacy. Class components are less elegant and easy to use.

While you would be right in preferring hooks because of their elegance and cleanliness, for the time being let's accept that we have to learn both these ways of working.

The properties we have seen so far will be used for readonly information that are passed to a component as parameters. Various components in our application will also get a state which they can write data to.

## Case study: editable product
In this chapter we will extend our application in order to make the products editable. We will start by making the single product editable, meaning that the user can toggle editing, change the name, description, and url, and then save the data.

Let's head over to the product state definition and make the products updateable. We will define an _updater_ object. By doing this for every component in our application, each component will have the React component itself, the state, and the state updaters for that state:

```ts
// product.state.ts

export const Updaters = {
  name:(nameUpdater:(currentName:string) => string) => (currentProduct:Product) : Product => 
    ({...currentProduct, name:nameUpdater(currentProduct.name)}),
  description:(descriptionUpdater:(currentDescription:string) => string) => (currentProduct:Product) : Product => 
    ({...currentProduct, description:descriptionUpdater(currentProduct.description)}),
}
```

Uhh, what a mouthful. Let's dive into this step by step. First of all, you will notice that we deal with updaters in the shape

```ts
T => T
```

React suggests this as the best way of dealing with a state change, that is a state _change_ is the _change_ operation itself: a function that calculates the new state based on the current state. You can see this because the `setState` function accepts a `State => State` as a parameter!

This definition can be nested. A state updater can be built based on a field updater, that is the state updater for that field only. This means that we can _embed_ an updater for a field `F` into the updater for the containing entity `E` as follows:

```ts
(F => F) => (E => E)
```

As a result, the updater of the `name` of a `Product` will look like this:

```ts
  name:
    (nameUpdater:(currentName:string) => string) // the input is the function that calculates the new name based on the current name
    => 
    (currentProduct:Product) : Product => // the output is the function that calculates the new product based on the current product
    ({...currentProduct, // copy the current product over (description and url)
        name:nameUpdater(currentProduct.name) // calculate the new name based on the nameUpdater
    }),
```

This pattern may seem a bit strange at first, but the powerful thing about it is that it scales. The updater for the name (`string => string`) can be embedded into an updater for the `Product` (`Product => Product`) which in turn can be embeded into an updater for something that contains products, and so on until the root of the state of the whole application.

>  **This is a big deal!** Being able to spot generic and reusable patterns turns an application from a series of ad-hoc solutions that are all different from each other into a smooth, uniform, predictable code base.

It's time to use this. Let's head to the React component itself. We can request the current state, as well as the `setState` function that modifies the state, by calling the hook `useState`:

```ts
export const Product = (props: ProductProps): JSX.Element => {
  const [state,setState] = useState(props.product)
  return <div className='product'>
    ...
  </div>
}
```

Let's unpack this. `useState` gets as input the _initial state_. It returns a tuple with two things: the _current state_, and a _set state_ function which takes as input a state updater and saves it into the state.

> It is possible to pass a new state directly to `setState` but this has VERY DANGEROUS IMPLICATIONS SO DON'T DO IT. Ignoring the best practices of software engineering is one of the silliest things one can do, because it will eventually always come back and bite you in, well, you know where :)

Whenever something happens to the `state`, for example `setState` is invoked, then React renders the component again: first to the Virtual DOM, then to the actual DOM. Rerendering basically runs the code of our components again, including `useState`. If the state has changed, this will be reflected by a new value of the `state` variable.

An insanely important thing to remember is that 

** THE STATE VARIABLE NEEDS TO BE IMMUTABLE **

if you try to change the value of `state` directly, instead of via `setState`, React will not see that the component needs to be rendered again, and nothing will work properly.

Ok, now that this is settled, it's finally time to connect the `state` and `setState` variables to the body of the product component. The `name` will be an `input` element that gets the current value from `state.name`, and invokes `setState` `onChange`:

```tsx
<h2>
  <input type='text' value={state.name} onChange={e => { const tmp = e.currentTarget.value; setState(ProductState.Updaters.name(_ => tmp)) }}/>
</h2>
```

> Notice one minor oddity. `setState` will autonomously decide _when_ to invoke whatever state updater we pass it, usually quite some time after the event handler for `onChange` has long terminated execution. If we try to use `e.currentTarget.value` inside the `setState` callback, we will get an annoying `null` error. The correct practice is to save the `value` to a temporary variable (in our case aptly named `tmp`) which will not be cleared up after `onChange`. In general, when dealing with React, we should really get into a concurrency mindset, realizing that many actions we need to think about simply happen with a delay, and between start and end of many operations plenty of other stuff will take place. Difficult, but exciting and full of interesting concepts!

What do we pass to `setState`? Well, we want to update the `name`, so we will pass the updater for the `name` of a `Product`: `ProductState.Updaters.name`. The `name` updater takes as input the updater for the `name` itself. In our case we don't update the current `name`, but rather we just overwrite it, so the argument we pass is `_ => tmp` which ignores the `_` argument (the current name) and simply returns `tmp`, which is the new text in the input box. 

As an example of why the previous value can be useful, suppose we wanted to add one star to the name every time a button was pressed. In this case, we would need to use the previous value, and we would get the following code:

```tsx
<button onClick={e => setState(ProductState.Updaters.name(_ => _ + "⭐"))}>{state.name}</button>
```

Given that we write updaters for the general case, we only work with the variant that offers the current value of the field being updated, and if needed we can just ignore this field.

The `description` looks almost identical, besides of course using `state.description` as well as invoking another updater:

```tsx
<p>
  <input type='text' value={state.description} onChange={e => { const tmp = e.currentTarget.value; setState(ProductState.Updaters.description(_ => tmp)) }}/>
</p>
```

And there we go! If you don't like the `tmp` variable and the `_ => ` bit, we could define a utility function to create the replacement lambda:

```tsx
export const replaceWith = <V,>(v:V) => (_:V) => v 
```

which simply takes as input a value `v` and then creates a lambda that gets a parameter and ignores it. We can use it as follows:

```tsx
<input type='text' value={state.name} onChange={e => setState(ProductState.Updaters.name(replaceWith(e.currentTarget.value))) }/>
```

which, if you are into this kind of thing (spoiler alert! I am 😀😀😀) looks very pretty.

> `replaceWith` should go somewhere generic, not in the `Product` file of course!

## Let's generalize the state updaters
Ok, we might say that from a React perspective we are done, but we also have to look at things from a software engineering perspective. And from this other perspective, we are nowhere near done.

Our state updaters look very monotonous. The `name` and `description` updaters are almost exactly the same, and moreover they do not carry any interesting information because they are the most banal updaters possible: just embed the updater of the field into the updater of the entity. There will be hundreds such updaters in all our code. Sometimes we need properly custom updaters, but when we don't, it would be better not to have to repeat the same code over and over and over and over everywhere.

Let's head over to the `utils` file, because this is a general-purpose utility that will be used everywhere. We start by defining a shortcut for a function. We want to write `Fun<number,string>` instead of `(_:number) => string`, it is a bit easier on the eyes and it requires less finger contorsions:

```tsx
type Fun<a, b> = (_: a) => b
```

The core definition is the `Updater<Entity>`, a function that takes as input the current value of a given `Entity` and returns the updated version. The updated version might feature a new name, age, address, favorites, does not matter what. The only thing that matters is that an updater is:

```tsx
type Updater<Entity> = Fun<Entity, Entity>;
```

But what if we have the following situation: a `Person` contains an `Address`. If we have an `Updater<Address>`, we can embed it into an `Updater<Person>` as follows:

```ts
addressUpdater:Updater<Address> = ...
const personUpdater:Updater<Person> = currentPerson => ({...currentPerson, address:addressUpdater(currentPerson.address)})
```

that is we can _embed_ the updater of address into an updater of a person that applies the address updater to the person's address. The resulting person updater has a very cool name: a _widening_, because it is the same original updater but converted to a larger type:

```ts
type Widening<Entity, Field extends keyof Entity> = Fun<Updater<Entity[Field]>, Updater<Entity>>
```

The `Widening` of a `Field` (which has to be one of the keys of the `Entity`, for example `"name"` would be a key of a `Person`) is a function that transforms, or embeds, an updater of the field of the entity `Updater<Entity[Field]>` into the updater `Updater<Entity>` of the whole entity itself.

We can create a `Widening` object for each field. For example, for `type Person = { name:string, age:number }` we can create an object that contains one widening operation for each field in the form of an updater:

```ts
const PersonUpdater = {
  name:(updateName:Updater<Person["name"]>) : Updater<Person> => currentPerson => ({...currentPerson, name:updateName(currentPerson.name)}),
  age:(updateAge:Updater<Person["age"]>) : Updater<Person> => currentPerson => ({...currentPerson, age:updateAge(currentPerson.age)})
}
```

The type of `PersonUpdater.name` is `Widening<Person, "name">`, and the type of `PersonUpdater.age` is `Widening<Person, "age">`. Given all of this, we can actually do this per field automatically as follows:

```ts
type SimpleUpdater<Entity, Field extends keyof Entity> = {
  [f in Field]: Widening<Entity, Field>;
};
export const simpleUpdater = <Entity>() => <Field extends keyof Entity>(field: Field): SimpleUpdater<Entity, Field> => ({ [field]: (fieldUpdater: Updater<Entity[Field]>): Updater<Entity> => currentEntity => ({ ...currentEntity, [field]: fieldUpdater(currentEntity[field]) }) }) as SimpleUpdater<Entity, Field>;
```

The `simpleUpdater` now creates a single updater object for just one field, for example:

```ts
const PersonNameUpdater = {
  name:(updateName:Updater<Person["name"]>) : Updater<Person> => currentPerson => ({...currentPerson, name:updateName(currentPerson.name)})
}

const PersonAgeUpdater = {
  age:(updateAge:Updater<Person["age"]>) : Updater<Person> => currentPerson => ({...currentPerson, age:updateAge(currentPerson.age)})
}
```
and so on. We can create the updater for the whole object by merging all these partial updater objects with the spread operator which copies over all fields from an object. This is very elegant, just look at how clean this is for the `Product`:

```ts
export type Product = Id & { name: string; description: string; image: string; };

export const Updaters = {
  ...simpleUpdater<Product>()("name"),
  ...simpleUpdater<Product>()("description")
}
```

BAM! A powerful, elegant pattern, fully automated. This new definition of the updaters for the product does not require any changes in the rest of the code, so it is a drop-in replacement. Very cool, no way around it.

## Case study 2: editable product_s_
Ok, now that we know all there is to know about React and `useState`, let's extend our sample even more. We want to make the collection of products editable, that is we want to be able to add and remove products. This is not done at the level of the single product, but at the level of the container of all products. This is the main content!

For now we will keep the main content as it is, but it is not inconceivable that at some point we will need to create a `ProductsCollection` component which will be responsible for rendering the add/remove/move up/move down/etc. operations that refer to editing the collection of components. This sort of refactoring keeps our code tight and easy to read, so it is very important to always consider when a component is getting too big because it tries to do too many things at the same time, and split it up according to the Single Responsibility Principle!

Let's head over to the main-content and add a `state` file to it. In this state file, we define the `MainContentState` with a `Map` of products accessed by `id`:

```ts
import { Map } from 'immutable';

export type MainContentState = { products: Map<ProductState.Product["id"], ProductState.Product>; };
```

> We use the `Map` implementation from `immutable`, an extensive, powerful, generic collections library from Facebook. Unmissable. We talk about it in some detail in the _generics_ lesson in the Introduction to Typescript course.

We now define the updaters. Given that we want to be able to add and remove elements to the `products`, we do this in layers. First of all, we define a `simpleUpdater` for the `products` field:

```ts
export const Updaters = {
  ...simpleUpdater<MainContentState>()("products"),
}
```

Of course the `products` simple updater on its own does not do much, but we can use it to define nested updates such as adding a product:

```ts
export const Updaters = {
  ...simpleUpdater<MainContentState>()("products"),
  addProduct: () => Updaters.products(_ => { const id = uuidv4(); return _.set(id, ({ id, name: "", description: "", image: "" })); }),
};
```

> Notice that we generate a unique id with the `uuid` library which we had to add via `yarn` and import as follows: `import { v4 as uuidv4 } from 'uuid';`.

`addProduct` builds upon `Updaters.products` in such a way that even if we change the underlying structure of the `MainContentState`, as long as there is a valid `Updaters.products` updater, `addProduct` will still work. This is a form of small scale abstraction and separation of concerns which has its own elegance and usefulness in keeping the codebase clean and powerful.

Finally, we define `removeProduct` which takes as input the `id` of a product to remove and performs the necessary change into the state via the `Updaters.products` updater:

```ts
  removeProduct: (id:ProductState.Product["id"]) => Updaters.products(_ => _.remove(id)),
```

Now let's head over to the React component itself and connect these event handlers to the appropriate React elements:

```tsx
export const MainContent = () => {
  const [state,setState] = useState<MainContentState>({ products:sampleProducts})
  return <>
    <ul>
      {
        state.products.valueSeq().sortBy(p => p.id).map(p => 
          <li key={p.id}>
            <Product product={p} />
            <button onClick={e => setState(Updaters.removeProduct(p.id))}>Delete product</button>
          </li>).toArray()
      }
    </ul>
    <button onClick={e => setState(Updaters.addProduct())}>Add new product</button>
  </>
}
```

Almost too easy to be true, right?!? Also notice how clean and classy the calls to `Updaters.removeProduct` and `Updaters.addProduct` look: there is no mention of the main state, which will be injected automatically by `setState`, they are readable, short, and effective.

> It was absolutely worth it! The extra structure and discipline we maintained throughout this lesson has made it possible to split our program into a series of files which are all focused on one single task, logical, easy to understand, easy to extend. Whenever a file becomes too big, split it quickly and the codebase will keep feeling this fresh and pleasant.

# Mutable state via legacy class-components
> Warning: brace yourselves, the next bit looks absolutely **horrible**. 

Hooks are simple, clean, and elegant. What we are about to see is fugly, complicated, and hard to read. The first versions of React did not feature hooks. Rather, they features a heavily object-oriented setup that was required for state management.

This sort of setup is not completely legacy (yet!). There are still some virtual methods that we can implement to be told by React when a component is about to render, whether it was just mounted, dismounted, etc. which cannot all be done with hooks alone. For this reason, stateful class components are still there, they can still be useful, and we see them quickly.

Let's quickly head to the `index.tsx` file. We will create a stateful component in there for illustration purposes and then promptyl delete it.

We start by defining the properties that come into the component (properties are readonly) and the state of the component (which will change throughout the lifetime of the component):

```tsx
type CounterProps = { startsFrom:number }
type CounterState = { count:number }
```

We are not going to neatly separate stuff into files because this is a short mini-tutorial but I advise you to follow the same guidelines (props, state, and updaters into a separate file) if you need to use this in practice.

Now we can define the component itself as a class which inherits from `React.Component`:

```tsx
class Counter extends React.Component<CounterProps, CounterState> {

}
```

`React.Component` is generic in the properties and the state.

> If you want to know more about this, I must recommend the _Introduction to Typescript_ course of the dev academy where we discuss generics, classes, and in general all you need to know in order to master this kind of code.

The class needs a constructor, which takes as input the properties, passes them to the constructor of `React.component` (the `super(props)` call does exactly this) and then initializes the `state` field of the class. In our case we initialize the `count` to the initial value that came in via the `props`:

```tsx
  constructor(props:CounterProps) {
    super(props)

    this.state = { count:props.startsFrom }
  }
```

Finally, we need a `render` method which returns the elements of our scene:

```tsx
  render() {
    return <>
      <button onClick={_ => this.setState(currentState => ({...currentState, count:currentState.count+1}))}>{this.state.count}</button>
    </>
  }
```

We render a simple button that invokes `this.setState`, just like we did for the equivalent hook. Notice once again that we are not using updaters, which we should because this is way too much complexity for a single line of code.

The real power of class components comes from the lifecycle methods. By implementing these methods we can obtain fine-grained control over the events surrounding the creation and re-rendering of a component.

The first lifecycle method is `componentDidMount`, which is invoked when the component is added to the DOM for the first time. The component might then re-render many times, but it is mounted only once. Here we can perform api calls, or any other operation that should be performed only at component creation:

```tsx
  componentDidMount(): void {
  }
```

React will invoke `shouldComponentUpdate` to decide whether or not new properties or a new state warrant re-rendering of the component. When a component' properties or state change very often, or the rendering of the component is very slow, we might want to decide that some new values of properties and state that do not cause any visual changes can be done without re-rendering. By default React will simply re-render every time the properties or the state change, but if this is too slow, then `shouldComponentUpdate` offers the key to speed things up:

```tsx
  shouldComponentUpdate(nextProps: Readonly<CounterProps>, nextState: Readonly<CounterState>, nextContext: any): boolean {
    return true
  }
```

Right after a re-render, `componentDidUpdate` will be triggered:

```tsx
  componentDidUpdate(prevProps: Readonly<CounterProps>, prevState: Readonly<CounterState>, snapshot?: any): void {
    
  }
```

When a component is about to be removed from the DOM, it may perform some cleanup operations. For example, a chat widget might close the websocket connection to the server, or some data might be backed up to local storage. We do this in the `componentWillUnmount` method:

```tsx
  componentWillUnmount(): void {
    
  }
```

Errors happen. Not to you, dear reader/listener, because you are a star of a programmer, but know that other people are not so fortunate as you are and so they might need to deal with errors. React will invoke the `componentDidCatch` method when an error happens when rendering the component, in order to recover gracefully from the issue:

```tsx
  componentDidCatch(error: Error, errorInfo: React.ErrorInfo): void {
    
  }
```

And that's all for state management with hooks or stateful classes. For the rest of the course we will stick to hooks whenever possible. Actually you will see that neither hooks nor stateful components are the most useful state management pattern, but that is the topic of...the next lesson!!!

# Practice
Make everything editable (including the image url of the products), down to the `Options`. Add a global property, `EditMode`, that is passed down to each component so that we can toggle editing of the whole page somewhere in the root of the page.
