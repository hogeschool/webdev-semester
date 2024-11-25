# Props
Of course it's all about data! 

Conceptually and in practice, components are like normal functions. They accept arbitrary inputs (called “props”) and return React elements describing what should appear on the screen. We could even say that **components translate the data they receive as input into the corresponding React elements that visualize them**. By using properties, we will inject data into our custom components in order to make them perform the translation from data to DOM.

> Important to know: all React components must act like pure functions with respect to their props. Whether you declare a component as a function or a class, it must never modify its own props. The props are read-only!

Example: product
Let's consider a `Product` component.

Let's bring a little more structure to our application: `ProductProps`, `type Product`

```tsx
type Product = { id: string; name: string; description: string; image: string; };
type ProductProps = { product: Product; };
const Product = (props: ProductProps): JSX.Element => <div className='product'>
  <h2>{props.product.name}</h2>
  <p>{props.product.description}</p>
  <img src={props.product.image} />
</div>;
```

We may now use the `Product` component from our `Page` component by passing it the fields of the `props` (the `props` object itself is created automatically by React for us):

```tsx
  <Product product={{ id:"12345", name:"Fiat Panda 4x4 Sisley", description:"An indestructible auto. Uncomfortable, ugly, and slightly undignified, but it will take you there over and over again", image:"https://upload.wikimedia.org/wikipedia/commons/thumb/7/70/1989_Fiat_Panda_4x4_SISLEY_%283863394372%29.jpg/440px-1989_Fiat_Panda_4x4_SISLEY_%283863394372%29.jpg" }} />
```

Very cool. But, let's always behave in a civilized fashion when dealing with code organization, or we will end up saving one minute now and wasting one day of waste later. 
First, we should put the `Product` component in a `product` directory under `main-content`.

Then, we can split the `id` component of the `Product` type definition and move the `Id` type to a new file:

```ts
export type Id = { id: string; };
```

The definition of the `Id` is now universal and it can be reused for all types. A bit of standardization can go a long way!

```ts
import { Id } from '../../../utils/Id';

type Product = Id & { name: string; description: string; image: string; };
```

Let's be even better boy-scouts. It will be worth it later on. Let's split the product type from the widget:

```ts
// product.state.ts

import { Id } from '../../../utils/Id';

export type Product = Id & { name: string; description: string; image: string; };
```

and 

```ts
// product.widget.tsx

import * as ProductState from './product.state';

type ProductProps = { product: ProductState.Product; };
// ...
```

The state file will grow with more related type defitions, operations on the state, and much more. The widget file will also grow because the layout will become richer, there will be CSS and HTML at play, etc. Of the two files, the widget file will probably require further splitting later on, but we will get there eventually.

Properties are usually propagated down from parent to child components, and usually this is a form of narrowing of focus. The parent component selects a subset of data from its properties that are then given to the child component.
This way each component at every level only has to take care of the data it needs in order to perform its function, without having to deal with a lot of useless extra context.

Let's now pass to the main content some more exciting properties, for example an array of products. The main content will have as main responsibilities the management of the whole colleciton of products (which is indeed the _main content_ of the site so it makes sense), but for the specific management of the individual product we will delegate the responsibility to the `Product` component.

We start by defining the sample products:

```ts
let sampleProducts : Array<ProductState.Product> = [
  { id:"1", name:"Fiat Panda 4x4 Sisley", description:"An indestructible auto. Uncomfortable, ugly, and slightly undignified, but it will take you there over and over again.", image:"https://upload.wikimedia.org/wikipedia/commons/thumb/7/70/1989_Fiat_Panda_4x4_SISLEY_%283863394372%29.jpg/440px-1989_Fiat_Panda_4x4_SISLEY_%283863394372%29.jpg" },
  { id:"2", name:"Mercedes A Class", description:"The smallest Mercedes. Classy, practical, and comes in many engines and configurations.", image:"https://upload.wikimedia.org/wikipedia/commons/thumb/f/f7/2018_Mercedes-Benz_A200_AMG_Line_Premium_Automatic_1.3_Front.jpg/800px-2018_Mercedes-Benz_A200_AMG_Line_Premium_Automatic_1.3_Front.jpg" },
  { id:"3", name:"Toyota Corolla", description:"The manual says engine oil is optional. Legend says, this car will outlast you and everyone you know.", image:"https://upload.wikimedia.org/wikipedia/commons/thumb/d/de/1989_Toyota_Corolla_%28AE92%29_CS_5-door_hatchback_%282010-06-21%29.jpg/799px-1989_Toyota_Corolla_%28AE92%29_CS_5-door_hatchback_%282010-06-21%29.jpg" },
]
```

The main content component then uses the `{ ... }` block in order to dynamically compute a portion of its JSX based on the props. Inside the `{ ... }` block we can put any expression that will result in either a component or an array of components. In our case, each product from the array of sample products will be transformed into the `Product` component that renders that specific product:

```tsx
  <ul>
    {
      sampleProducts.map(p => 
        <li>
          <Product product={p} />
        </li>)
    }
  </ul>  
```

We could have also added the whole code of the `Product` component here, but we want to follow the principle of loose-coupling and single responsibility. Each component, file, function should have one very clear responsibility within our application, so that it's clear what it does and it's easy to maintain, extend, and later on even debug.

Along this line of reasoning, let's split again this file. The sample products are for now just some sample fake data, but in a real application they would come from an API. We move the sample products to a file that clearly indicates that we are dealing with a fake ("mocked") API response that actually got the list of products from a CMS database:

```ts
// main-content/products/api/mocked-products-response.ts

import * as ProductState from '../product/product.state';

export let sampleProducts: Array<ProductState.Product> = [
  { id: "1", name: "Fiat Panda 4x4 Sisley", description: "An indestructible auto. Uncomfortable, ugly, and slightly undignified, but it will take you there over and over again.", image: "https://upload.wikimedia.org/wikipedia/commons/thumb/7/70/1989_Fiat_Panda_4x4_SISLEY_%283863394372%29.jpg/440px-1989_Fiat_Panda_4x4_SISLEY_%283863394372%29.jpg" },
  { id: "2", name: "Mercedes A Class", description: "The smallest Mercedes. Classy, practical, and comes in many engines and configurations.", image: "https://upload.wikimedia.org/wikipedia/commons/thumb/f/f7/2018_Mercedes-Benz_A200_AMG_Line_Premium_Automatic_1.3_Front.jpg/800px-2018_Mercedes-Benz_A200_AMG_Line_Premium_Automatic_1.3_Front.jpg" },
  { id: "3", name: "Toyota Corolla", description: "The manual says engine oil is optional. Legend says, this car will outlast you and everyone you know.", image: "https://upload.wikimedia.org/wikipedia/commons/thumb/d/de/1989_Toyota_Corolla_%28AE92%29_CS_5-door_hatchback_%282010-06-21%29.jpg/799px-1989_Toyota_Corolla_%28AE92%29_CS_5-door_hatchback_%282010-06-21%29.jpg" },
];
```

Notice that we introduced a new level in the folder structure of our application. While this might seem like a less important thing to do, keeping files and folders organized is essential! Code is a way to organize information about a complex domain, and if we don't keep things neat than that's the opposite of organization and sooner or later (trust me: sooner) all sorts of extra challenges and difficulties will emerge that make our life as developers harder.

Our products in practice will come from a database. The `id` of each product is thus the primary key, or the unique identifier that is uniquely associated with each product. We can model this better than an array by using a `Map`, which links the `id` of each product to the product data itself. Thanks to a `Map` we are guaranteeing the uniqueness of the `id`s, but we are also clearly expressing the fact that a product can be found by its `id`. This is a better representation of the underlying structure of the source data, so it is worth considering it.

> An array is also fine, don't worry 😀😀😀

We start by adding the `immutable` library:

```sh
yarn add immutable
```

We then create our map by adding the elements from the array as follows:

```ts
import * as ProductState from '../product/product.state';
import { Map } from 'immutable'

export let sampleProducts = 
  Map<ProductState.Product["id"], ProductState.Product>(
    [
      { id: "1", name: "Fiat Panda 4x4 Sisley", description: "An indestructible auto. Uncomfortable, ugly, and slightly undignified, but it will take you there over and over again.", image: "https://upload.wikimedia.org/wikipedia/commons/thumb/7/70/1989_Fiat_Panda_4x4_SISLEY_%283863394372%29.jpg/440px-1989_Fiat_Panda_4x4_SISLEY_%283863394372%29.jpg" },
      { id: "2", name: "Mercedes A Class", description: "The smallest Mercedes. Classy, practical, and comes in many engines and configurations.", image: "https://upload.wikimedia.org/wikipedia/commons/thumb/f/f7/2018_Mercedes-Benz_A200_AMG_Line_Premium_Automatic_1.3_Front.jpg/800px-2018_Mercedes-Benz_A200_AMG_Line_Premium_Automatic_1.3_Front.jpg" },
      { id: "3", name: "Toyota Corolla", description: "The manual says engine oil is optional. Legend says, this car will outlast you and everyone you know.", image: "https://upload.wikimedia.org/wikipedia/commons/thumb/d/de/1989_Toyota_Corolla_%28AE92%29_CS_5-door_hatchback_%282010-06-21%29.jpg/799px-1989_Toyota_Corolla_%28AE92%29_CS_5-door_hatchback_%282010-06-21%29.jpg" },
    ].map(p => ([p.id, p]))
  )
```

Ok, let's unpack this carefully. First, we declare that we are creating a `Map` where the `key` is whatever type the `id` field of the `Product` has (this way if that ever changes, then our `Map` definition is updated automatically. Neat eh?). Second, we create our array of entries. Third, we map the entries to create tuples with the `id` and the `Product` next to each other with the `map` function of `Array`.

Now we use the `Map` in the main-content. First, we ignore the keys and only get the values. Then we ensure to sort by `id`  because a `Map` uses its own weird sorting and we don't want that because it can feel quite random. We could also sort by other attributes such as the `name` of course, but as long as we take control of the sorting of the map entries any (combination of) attributes will do fine:

```tsx
      sampleProducts.valueSeq().sortBy(p => p.id).map(p => 
        <li>
          <Product product={p} />
        </li>).toArray()
```

The `map` function invoked on the `immutable` collections is a lazy collection, but React prefers arrays so we call `toArray` right at the end in order to materialize (iterate + copy) the results to an array.

It is quite common for React to complain whenever an array of elements does not feature a `key` property. `key` properties are supposed to be unique identifiers that help React find out after a re-render which elements of the new DOM have stayed the same: those elements do not need to be removed, updated, or inserted in the DOM, saving a lot of time and keeping the application fast and smooth.

We can add the `key` very easily as follows:

```tsx
<Product key={p.id} product={p} />
```

The `key` property is not part of the properties of the `Product` component, but it's available anyway. We can add a value to the `key` property of any element, even built-in HTML elements such as `<li>`, `<div>`, etc. because it is a universal, React-specific construct. 

We could have also put the `key` property one level up, in the `<li>`:

```tsx
<li key={p.id}>
  <Product product={p} />
</li>).toArray()
```

Notice how the "missing `key`" warning is gone from the browser console!

> The `key` property often forces us to have a unique identifier in our data structures. No worries though: most data structures such as a `Product` will eventually come from a database anyway, where such a unique identifier is almost always there and is the primary key of the entity.

# Practice
Let's define a little catalogue application with the following component structure:
```
Homepage -> Header, MainContent, Footer
MainContent -> Products
Products -> [Product]
Product -> Id, Name, Picture, Price, Options
Options -> [Option]
Option -> Id, Name, Picture, Description, Extra price
```