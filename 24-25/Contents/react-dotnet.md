### [Back to List of Topics](Contents.md)

# Unit X: Integrating React in a ASP.NET MVC application

In the previous chapter we learnt how to manage the state in a rather complex react application. In this chapter we focus on connecting our registration form to a ASP.NET application, by presenting
Razor, the template engine/markup syntax used by ASP.NET, and how to implement calls to an API in .NET from a React application to store/load data from a server.

## Razor

A template engine is a software designed to combine a data model with a template to generate a document (in our case a HTML document). We will not cover extensively all the details regarding ASP.NET and its template syntax (Razor), since the rendering of our application happens entirely in React. We cover just the basics to understand how the React script is loaded by Razor. Razor uses a series of files where HTML code can be combined with C# code to generate a web page. These files are placed in a folder called `Views` and have extension `.cshtml`. Each page has its own subfolder in `Views` named after a controller that will serve it. This means that, in order to serve a page with ASP.NET we need (replace `page_name` with the name of your page):

1. A controller  that will serve the page.
2. A Razor template in a folder under `Views`.

Razor allows to embed C# code in HTML by using the `@` symbol in a template file. Notice that a template is executed top down, so it does not have to be contained in a class. It is possible to define C# code blocks with the syntax `@{...}`. For instance

```csharp
@
{
  var foo = 1; 
}
```

would declare a variable named `foo` with value 1 that can be used in the template. The value of the variable can be used in any point of the HTML text by using `@{foo}`. It is also possible to use control structures, such as conditionals and loops:

**Conditional**
```csharp
@ if(<condition>)
{
  ...
}
else if (condition)
{
  ...
}
else
{
  ...
}
```

**For-loop**
```csharp
@ for(var i = 0; i < 5; i++)
{
  ...
}

@ foreach (var l in list)
{
  ...
}
```
**While-loop**
```csharp
@ while (<condition>)
{
  ...
}
```

Let us begin by defining a file called `HomePage.cshtml` in a folder `Home` under `Views`. This file simply sets the title of the homepage:

```csharp
@{
    ViewData["Title"] = "Person Manager";
}
```

>The dictionary `ViewData` is a dynamic dictionary where keys of type `string` are coupled to objects to exchange information between the main .NET application and the view. If necessary, you can also store custom dynamic data in such dictionary.

Let us also create a folder `Shared` and create a file called `_Layout.cshtml`. This file contains a template that can embed other views (so it is a static bit shared across all views). In such file we put the following razor code:

```csharp
<!DOCTYPE html>
<html lang="en">
<head>
    <meta charset="utf-8" />
    <meta name="viewport" content="width=device-width, initial-scale=1.0" />
    <title>@ViewData["Title"]</title>
    <script src="/spa.bundle.js"></script>
</head>
<body>
    <div id="root" />
    <script>
        spa.main()
    </script>
    @RenderBody()
</body>
</html>
```

In this snippet, pay attention to the following elements:
- An element `script` in `head` containing a javascript that we want to load. This defines a javascript file that will be executed when the view is served.
- An `div` with id `root` that calls a function `main` from `spa`.
- A call to `RenderBody`, which is a method that specify where the specific razor code for each view is rendered inside the layout.

> The script `spa.bundle.js` is created when running webpack with the provided source code. The configuration of webpack is set up to create a file called spa.bundle.js in `wwwroot`, whose code will be placed inside a module called `spa`. How to configure webpack escapes the scope of this document and will not be explained, we just use it as is.

Now we can create a controller `HomeController` to serve the view for the homepage:

```csharp
public class HomeController : Controller
{
    public HomeController()
    {
    }

    [HttpGet("/")]
    public IActionResult HomePage()
    {
        return View();
    }
}
```

The important bit of this controller is the `View` method that return a `ViewResult` (which implements `IActionResult`). Whenever we invoke such method, we start the so called view discovery by the ASP.NET engine. The view discovery works in the following way: the default behavior of the View method is to return a view with the same name as the action method from which it's called. For example, the `Home` method name of the controller is used to search for a view file named `Home.cshtml`. First, the runtime looks in the `Views/[ControllerName]` folder for the view. If it does not find a matching view there, it searches the `Shared` folder for the view. In our case, it will look in `Views/Home/Home.cshtml` first, and then if the view was not found there it would look in `Views/Shared/Home.cshtml`.

The method `View` supports 3 other overloads with the following parameters

1. An explicit view name, overriding the default behaviour, or a view path where you can explictly say where to find the template file.
2. A model to pass to the view. This is used to pass statically-typed information to the view. In the view, you can access such data by using the `@model` directive followed by the type of the model, and then using `@Model` to read its fields.
3. Both a model and a view name/path.

Now in `ClientSide/index.tsx` we can invoke the react renderer:

```typescript
import React from 'react';
import ReactDOM from 'react-dom'
import { Home } from './Home/home';

export const main = () => {
  let rootElement = document.querySelector('#root')

  ReactDOM.render(
      <Home />,
    rootElement
  )
}
```

If now you run the application (after compiling both React and .NET) and you land on the homepage, you should see our old registration form, this time loaded inside a razor template.

## Communication between React and .NET
In this section we explore how to send and receive data to/from a .NET API with Typescript and React. We will see how Typescript handles asynchronous processes and how to send HTTP requests.

Asynchronous processes in Typescript are handled by using `Promise`. A promise can be constructed by providing one type parameter, which is the type of the result that it will be eventually produced, and as argument of its constructor two callbacks, `resolve` and `reject`, that are called respectively when the promise produces correctly a result or when it fails (think about it as a `try-catch`). Reading the result of a `Promise` requires invoking the method `then`, which is used to unbox the content of the promise and read it as input of a function that defines what to do after the promise has resolved correctly. On the other hand, we can use the method `catch` to do the same when the promise is rejected, and it is usually used to handle errors. Notice that both `then` and `catch` return themselves other `Promises`, with the signature:

```typescript
interface Promise<T> {
  then: <T1>(this: Promise<T>, f: (result: T) => Promise<T1>) => Promise<T1>
  catch: <T1>(this: Promise<T>, f: (reason: any) => Promise<T1>) => Promise<T1 | T>
}
```

In Typescript it is also possible to use syntactical sugar to avoid using explicit methods for promise with `async/await`. You can declare a function `async`, which needs to return a `Promise` (similar to C# async methods that need to return `Task`). You can then use `await` to bind the result of a call to a promise to a variable/constant and use it further ahead. The `catch` is simply implemented by wrapping the code in a `try-catch` block.

## Person Manager with Promises
In this section we show a practical use of `Promises` in our React application. In the previous chapter we implemented an in-memory storage in React. In this version we will use a file storage in our .NET application. In the folder `Controllers` you will find an additional controller called `RegistrationController` with two endpoints: one to register a person (with method `POST`) and one to read all the people stored as files (with method `GET`). We will not illustrate the details of this controller. The client application has been stripped from the in-memory storage.

### Registration form with Promises
We want to change the registration form in the following way:
- When we click on the button submit, we will send an API call to `api/registration` with method `POST` sending a person in the body of the request. The backend will automatically assign a guid as identifier for the person. 
- While the api call has not returned yet, the button will be disabled, and it will be re-enabled once the request terminates. 

We will only implement the happy flow of this communication (the error handling is left as an exercise to the reader). Let us start by adding an additional file `registration.api.ts`, which contains the asynchronous api calls used by the registration form. In order to send an API request with Typescript, we need to use the function `fetch`. This function takes as input the url of the request to make, and an object that can specify the following properties:

1. The method, passed as a string, of the HTTP request
2. The headers to send in the request.
3. A possible body for the request.

Our API implementation uses the method `POST` and accepts a json in its body. In order to achieve such request we need to send the method, the serialized body, and a header that specifies that the content of the body is JSON:

```typescript
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

Notice that we have to return a `Promise` because `fetch` itself returns a promise. In this implementation we leveraged the `async/await` syntax.

At this point we need a way to maintain the state of the request. The request at the beginning is `unloaded` (the request hasn't been sent). This state will be switched by the button to `loading` before sending the API call. When the API call returns, we change the state to `loaded`. In order to achieve this we need to store in the registration state, the information of the state in which this "loader" is. Let us add a new file to the registration state and a state callback to update its content:

```typescript
export type RegistrationLoader =
"unloaded" |
"loading" |
"loaded"

export type RegistrationState = Person & {
  loader: RegistrationLoader,
  updateName: (name: string) => (state: RegistrationState) => RegistrationState
  updateLastName: (lastName: string) => (state: RegistrationState) => RegistrationState
  updateAge: (age: number) => (state: RegistrationState) => RegistrationState
  updateLoader: (state: RegistrationLoader) => (state: RegistrationState) => RegistrationState
}
```

The initial registration state becomes:


```typescript
export const initRegistrationState: RegistrationState = {
  name: "",
  lastName: "",
  age: 18,
  loader: "unloaded",
  // storage: Map(),
  // currentId: 0,
  updateName: (name: string) => (state: RegistrationState): RegistrationState =>   
    ({
      ...state,
      name: name
    }),
  updateLastName: (lastName: string) => (state: RegistrationState): RegistrationState =>
    ({
      ...state,
      lastName: lastName
    }),
  updateAge: (age: number) => (state: RegistrationState): RegistrationState =>
    ({
      ...state,
      age: age
    }),
  updateLoader: (loaderState: RegistrationLoader) => (state: RegistrationState): RegistrationState => ({
    ...state,
    loader: loaderState
  })
}
```

At this point we need to modify the code of `insertPerson` to invoke the API call and update the state. The behaviour that we want to implement is that first we set the state to `loading` and immediately after that, we need to trigger the API call, **in this precise order**. We can do so by using an overload of `setState` which accepts as second parameter a callback to execute after the state callback has been executed:

```typescript
<RegistrationForm
            insertPerson={(person: Person) => this.setState(this.state.updateRegistrationState(this.state.registrationState.updateLoader("loading")), () => {
              register(person)
              .then(_ => this.setState(this.state.updateRegistrationState(this.state.registrationState.updateLoader("loaded"))))
            })}
            ...
/>
```
With that code, first we ste the state of the loader to `loading`, after this has been set we run the call back that first makes the API call and, after the promise resolves, sets the state to `loaded`. After doing so, we can modify the registration form renderer to display a message while the promise is loading and to disable the button:

```typescript
{
  this.props.registrationState.loader == "loading" ?
  <div>
    Saving...
  </div> :
  null
}
<div>
  <button
    disabled={this.props.registrationState.loader == "loading"}
    onClick={_ => this.props.insertPerson({
      age: this.props.registrationState.age,
      lastName: this.props.registrationState.lastName,
      name: this.props.registrationState.name
    })}
  >
      Submit
  </button>
</div>
<div>
```

### Loading the people Overview with Promises
The second step in adapting our application to use the API in .NET is loading the list of people that have been stored as files by the backend. In order to do so, we need to turn the stateless `Overview` component into a stateful component, because we need to keep track also in this case when the component is still loading the data and when it's done. We define the `OverviewState` as

```typescript
export type OverviewState = ({
  kind: "loading"
} | {
  kind: "loaded",
  value: StoragePerson[]
}) & {
  storeValue: (value: StoragePerson[]) => (state: OverviewState) => OverviewState
}

export const initOverviewState: OverviewState = {
  kind: "loading",
  storeValue: (value: StoragePerson[]) => (state: OverviewState): OverviewState => ({
    ...state,
    kind: "loaded",
    value: value
  })
}
```

where `StoragePerson` is defined as:

```typescript
export type StoragePerson = Person & {
  id: string
}
```

Then we write our api call (we added a new file `overview.api.ts` just like for the registration). This call, unlike the first one that did not have to process any result, needs to resolve the promise. Again we opt to use the `async/await` syntax:

```typescript
export const loadPeople = async (): Promise<StoragePerson[]> => {
  const response = await fetch("api/registration/all", {
    method: "GET"
  })
  var content = (await response.json()) as StoragePerson[]
  return content
}
```
If desirable, we could easily turn the `async/await` syntax into the explicit promise composition with `then`:

```typescript
export const loadPeople = (): Promise<StoragePerson[]> =>
  fetch("api/registration/all", {
    method: "GET"
  })
  .then(response => response.json())
  .then(content => content as StoragePerson[])
```

At this point, we can observe that the behaviour of loading the people is slightly different from the behaviour of submitting a registration form, because the former is not triggered by any UI interaction, rather it is triggered immediately when the component loads. In order to achieve this behaviour, we need to exploit one of the lifecycle methods of React. Lifecycle methods are special methods that are triggered when particular events in the lifecycle of the component occur. In this case, we will use the `componentDidMount` lifecycle event, that is triggered after the component has been rendered in the DOM:

```typescript
componentDidMount(): void {
  if (this.state.kind == "loading") {
    loadPeople()
    .then(people => {
      this.setState(this.state.storeValue(people))
    })
  }
}
```

After the component has been rendered, and if the state of the loader is `loading` (so the data has not been loaded yet), we load the data and, once the promise resolves, we store the array of people returned by the API call into the state. We can change the renderer to show a message while we are loading the data, and to show the list of people once the data has been loaded:

```typescript
render(): JSX.Element {
  if (this.state.kind == "loading") {
    return (
    <div>
      Loading...
    </div>
    )
  }
  return (
    <div>
    {
      this.state.value.map(
        person => (
          <div key={`overview-list-item-${person.id}`}>
            Name: {person.name} Last Name: {person.lastName} Age: {person.age}
          </div>
        )
      )
    }
    <div>
      <button
        onClick={_ => this.props.backToHome()}
      >
        Back
      </button>
    </div>
    </div>
  )
}
```

## Conclusion

In this chapter we have learnt the basics of Razor, the template language for ASP.NET. We have seen how to define controllers that render views in razor, and how to load a javascript function that uses React to render a UI in a template file. Later, we have seen how to invoke API calls in typescript and how to make use of `Promises` in conjunction with the react state to store and load data from a remote source. In the next chapter we will see how to make use of the react router to simulate browser navigation in a single-page application built in React.