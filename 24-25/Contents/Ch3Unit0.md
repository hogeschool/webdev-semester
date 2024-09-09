# About React
React (also known as React.js or ReactJS) is a free and open-source front-end JavaScript library[3][4] for building user interfaces based on components. It is maintained by Meta (formerly Facebook) and a community of individual developers and companies.[5][6][7]

React can be used to develop single-page, mobile, or server-rendered applications with frameworks like Next.js. Because React is only concerned with the user interface and rendering components to the DOM, React applications often rely on libraries for routing and other client-side functionality.

React is a "grown-up" library: it was first published in 2011!

## Declarative
React adheres to the declarative programming paradigm.[10]: 76  Developers design views for each state of an application, and React updates and renders components when data changes. This is in contrast with imperative programming.[11]

## Components
React code is made of entities called components.[10]: 10–12  These components are modular and reusable.[10]: 70  React applications typically consist of many layers of components. The components are rendered to a root element in the DOM using the React DOM library. When rendering a component, values are passed between components through props (short for "properties"). Values internal to a component are called its state.[12]

## Stack
React is a frontend framework. We can use it to build Single Page Applications that run in the browser and that dynamically simulate pages. SPAs are more "intelligent" than traditional websites and feel a lot faster because they can implement business logic in the browser itself, and can reload only parts of what the user sees without having to download a new page for every click.

SPAs are also lighter because they require the server to only provide the necessary "delta" (via API) for performing the desired operation, instead of the server having to rebuild a full-blown page, which is expensive.

React can be used against any backend, as long as it has an API that Javascript can consume. At the end of this course, we will see an example of integration of React with ASP .Net, one of the many frameworks React can work with.

React works with both Javascript and Typescript. Using React with Typescript reduces the number of bugs and increases developers' effectiveness, so we will use Typescript throughout the rest of this course.


# Server setup - NOT PRODUCTION READY
Throughout the course we will work with a really simple backend, which is absolutely not adequate for production but it is more than adequate for what we need to show.

First we setup the `nodejs` project, and install the necessary packages:

```sh
cd ./server
yarn init
yarn add -D http-server
```

We add a shorthand command for launching the `http-server` and listening to port `5000`:

``` json
  "scripts": {
    "be": "http-server -p 5000"
  },
```

Finally, we create an `index.html` file which will be served by `http-server`:

```sh
touch index.html
```

```html
<!DOCTYPE html>
<html lang="en">
<head>
    <title>Welcome to the Introduction to React!</title>
    <script src="/spa.bundle.js"></script>
</head>
<body>
    <div id="root" />
    <div>Hello!</div>
    <script>
        spa.main()
    </script>
</body>
</html>
```


# Client setup
Now we setup our client. Let's begin by setting up a new `nodejs` project and installing Typescript right away:

```sh
cd ./client
yarn init
yarn add -D typescript
yarn tsc --init
```

Let's setup Typescript:
``` json
{
  "compilerOptions": {
    "target": "ES6",                          /* Specify ECMAScript target version: 'ES3' (default), 'ES5', 'ES2015', 'ES2016', 'ES2017', 'ES2018', 'ES2019', 'ES2020', or 'ESNEXT'. */
    "module": "CommonJS",                     /* Specify module code generation: 'none', 'commonjs', 'amd', 'system', 'umd', 'es2015', 'es2020', or 'ESNext'. */
    "jsx": "react",                     /* Specify JSX code generation: 'preserve', 'react-native', or 'react'. */
    "declaration": true,                   /* Generates corresponding '.d.ts' file. */
    "declarationMap": true,                /* Generates a sourcemap for each corresponding '.d.ts' file. */
    "sourceMap": true,                     /* Generates corresponding '.map' file. */
    "outDir": "./dist",                        /* Redirect output structure to the directory. */
    "strict": true,                           /* Enable all strict type-checking options. */
    "allowSyntheticDefaultImports": true,  /* Allow default imports from modules with no default export. This does not affect code emit, just typechecking. */
    "esModuleInterop": true,                  /* Enables emit interoperability between CommonJS and ES Modules via creation of namespace objects for all imports. Implies 'allowSyntheticDefaultImports'. */
    "skipLibCheck": true,                     /* Skip type checking of declaration files. */
    "forceConsistentCasingInFileNames": true,  /* Disallow inconsistently-cased references to the same file. */
    "build": true 
  }
}
```

We need `react`, `react-dom`, and the appropriate type bindings to get optimal autosuggestions in VS Code and Typescript:

```sh
yarn add react
yarn add react-dom
yarn add -D @types/react-dom
```

We will use `webpack` to compile and package multiple `*.ts` files into a single browser-friendly Javascript bundle file:

```sh
yarn add -D webpack
yarn add -D webpack-cli
yarn add -D fork-ts-checker-webpack-plugin
yarn add -D @babel/core
yarn add -D @babel/preset-react
yarn add -D @babel/preset-typescript
yarn add -D babel-loader
yarn add -D source-map-loader
```

```json
  "scripts": {
    "fe": "webpack --config webpack.config.js --watch"
  },
  ...
  ,
  "babel": {
    "presets": [
      "@babel/preset-react",
      "@babel/preset-typescript"    
    ]
  }
```

```js
const webpack = require('webpack');
const path = require('path');
const ForkTsCheckerWebpackPlugin = require('fork-ts-checker-webpack-plugin');

module.exports = env => {
  const isProd = !!env && !!env.production

  return {
    mode: isProd ? 'production' : 'development',
    entry: {
      spa: path.join(__dirname, 'index.tsx'),
    },
    target: 'web',
    resolve: {
      extensions: ['.ts', '.tsx', '.js'],
      fallback: {
        // process: require.resolve("process/browser"),
        // zlib: require.resolve("browserify-zlib"),
        // stream: require.resolve("stream-browserify"),
        // util: require.resolve("util"),
        // buffer: require.resolve("buffer"),
        // asset: require.resolve("assert"),
        // _stream_transform: require.resolve("readable-stream/transform"),
        // "crypto": require.resolve("crypto-browserify")
      },
    },
    devtool: isProd ? undefined : 'source-map',
    module: {
      rules: [
        {
          test: /\.tsx?$/,
          use: [{
            loader: 'babel-loader',
          }],
          exclude: '/node_modules/'
        },
        { enforce: 'pre', test: /\.js$/, loader: 'source-map-loader' },
      ]
    },
    watchOptions: {
      ignored: [path.resolve(__dirname, 'node_modules')],
    },
    output: {
      filename: isProd ? '[name].bundle.[contenthash].min.js' : '[name].bundle.js',
      chunkFilename: isProd ? '[name].chunk.[contenthash].min.js' : '[name].chunk.js',
      path: path.resolve(__dirname, '../server'),
      publicPath: '/',
      libraryTarget: 'var',
      library: 'spa',
    },
    optimization: {
      minimize: isProd,
      minimizer: [],
      usedExports: true,
    },
    plugins: [
      new ForkTsCheckerWebpackPlugin(),
      new webpack.ProvidePlugin({
        Buffer: ['buffer', 'Buffer'],
      }),
      new webpack.ProvidePlugin({
          process: 'process/browser',
      }),
    ]
  }
}
```

Finally, we can write our first React application and admire it in the browser:

```tsx
import React, { useState } from 'react';
import ReactDOM from 'react-dom'
import { createRoot } from 'react-dom/client';

export const main = () => {
  const rootElement = document.querySelector('#root')
  if (!rootElement) { alert("Cannot find root element!"); return }
  const root = createRoot(rootElement)
  root.render(
      <div>Hello world!!!!!</div>
  )
}
```


# JSX
React manages the dynamic assembly of a virtual "scene" of nested HTML components. This scene is mounted to the DOM of the browser.

React introduced the Javascript and Typescript extensions `*.jsx` and `*.tsx` which allow a mix of HTML-style element syntax and normal Javascript and Typescript.

The HTML-style elements can be created with 

```tsx
<div>Hello world!!!!!</div>
```

We can use any HTML tags we want, with one notable difference: `class` is a reserved keyword, so we must use `className` in React. For the rest, everything is exactly the same as plain old HTML:

```tsx
      <div className='page'>
        <h1>Welcome to the wonderful world of React</h1>
        <p>React is the best frontend framework</p>
      </div>,
```

The real power of React lies in its ability to define new elements that are not part of the HTML standard. Basically, React allows us to extend the set of HTML elements with new, domain-specific components:

```tsx
const TwoParagraphs = () =>
  <div>
    <p>Paragraph 1</p>
    <p>Paragraph 2</p>
  </div>
```

which we can now use as follows:

```tsx
  ReactDOM.render(
      <div className='page'>
        <h1>Welcome to the wonderful world of React</h1>
        <p>React is the best frontend framework</p>
        <TwoParagraphs />
      </div>,
```

> Note a restriction: custom elements must start with a capital letter. `twoParagraphs` would thus not be a valid name.

React also offers a placeholder element, the so-called `React.fragment`, for all those times when we don't want to have an enclosing `div` but still we want to define a new component which instantiates multiple children at the same time:

```tsx
const TwoParagraphs = () =>
  <>
    <p>Paragraph 1</p>
    <p>Paragraph 2</p>
  </>
```


# Virtual DOM
Making changes to the DOM is too slow for real-time usage. To avoid this slowness, at the core of React is the Virtual DOM.

React creates an in-memory data-structure cache, computes the resulting differences, and then updates the browser's displayed DOM efficiently.[26] This process is called _reconciliation_. This allows the programmer to write code as if the entire page is rendered on each change, while React only renders the components that actually change. This selective rendering provides a major performance boost.[27]

The reconciliation process is usually very smart, but if there are many elements on the screen with the same type and one after each other, then it becomes impossible for React to guess which elements became which. For example, consider the following:

```tsx
const AList = () =>
  <ul>
    <li>Item 1</li>
    <li>Item 2</li>
    <li>Item 3</li>
    <li>Item 4</li>
    <li>Item 5</li>
  </ul>
```

If the list is long (even a couple of hundred elements will be too much), and especially when its elements are computed dynamically (see next lesson!), then the reconciliation process might take a long time. We can mark any React element, HTML or custom, with the `key` property in order to give a domain-specific hint to the reconciliation process in order to make it run consistently fast:

```tsx
const AList = () =>
  <ul>
    <li key="1">Item 1</li>
    <li key="2">Item 2</li>
    <li key="3">Item 3</li>
    <li key="4">Item 4</li>
    <li key="5">Item 5</li>
  </ul>
```

The best candidates for the `key` property are any stable, unique identifiers. Stable means that they don't change over time, and unique identifiers means that each item has its own identifier without ambiguiity.
For example, the `ID` of the entity in the database would be an excellent `key`, just like the email address of a user. The first name of a user, on the other hand, could be duplicated and so would not make a good `key`.

# Composition
Components, after having been defined, can refer to each other. This is a very common and very useful practice, because when a component is defined it is very handy to then start using it as if it were just a built-in HTML tag.

For example, consider a website page. We could split a page into three logical sections: the header, the main content, and the footer. Each logical section can then be mapped to a React component as follows:

```tsx
const Header = () =>
  <>
    <div>I am the header</div>
  </>

const Footer = () =>
  <>
    <div>I am the footer</div>
  </>  

const MainContent = () =>
  <>
    <div>I am the main content of the page</div>
  </>  

const Page = () =>
  <>
    <Header />
    <MainContent />
    <Footer />
  </>  
```

Don't worry about the placeholders, it's actually a common practice to first split the logical structure of your page, and only then when you have split into the smallest components you can, to start filling things up.

Also, truth be told, now is a good time to start splitting our components across different files. In order to keep things even neater, we will create one folder for each component, so that further splitting of that component can take place inside that same folder!

```sh
page
  footer
    footer.tsx
  header
    header.tsx
  main-content
    main-content.tsx
  page.tsx
```

Notice that the footer, header, and main content, being already split from the page, fall under the `page` directory.

In order to use a component defined in another file, we simply _import_ it:

```tsx
import { Page } from './page/page';
```

> Webpack takes care of stichting all the imported sources back together for us. That's one of the most useful things webpack does for us!
