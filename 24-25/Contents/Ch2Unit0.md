> **Learning goals**
> After reading this chapter, the student will be able to:
> - ... .

## Introduction to TypeScript
In the old days, there was JavaScript in the browser. JavaScript was the only way to add some degree of intelligence and logic to web applications on the client side. From form validation in real-time, to more sophisticated functionality that makes web applications feel a lot more like desktop applications, JavaScript became incredibly popular.

For this reason, the old days sucked badly. JavaScript was a poorly designed language, slow, filled with unusual inconsistencies such as two ways to say that something is not there (`null` and `undefined`, I am looking at you!). As recent releases of JavaScript made it better by introducing more and more standard features of modern programming languages, such as better scoped variable declaration, lambdas, etc., big applications were still suffering from the biggest missing functionality: static types. A long time ago (2012), Microsoft published TypeScript. A fully open source programming language with just one clear, explicit goal in mind: add types to JavaScript. The task was not simple at all, because JavaScript has to deal a lot with API communication and unstructured JSON data, and traditional type systems (let's say à la Java or C#) would not be able to correctly capture the subtlety of the way unstructured JSON is parsed into safe types known at compile-time. The common usage of formatted strings for routes also made this challenge extra complicated.

> Not even languages such as Haskell or F# with their advanced type systems were cut for the job. Something new and more powerful was needed.

TypeScript started off with a clean slate. On top of a simple, traditional type system (the sort of thing that says that it's not ok to pass a `string` to a function expecting a `number`), more and more advanced types were added, but always in a way that would be at the same time theoretically impeccable as well as elegant, intuitive, and pragmatic. The result was powerful, simple, and more powerful than most type systems available anywhere in the industry. TypeScript became a beloved industry standard very quickly, adopted almost universally and with great results. Developers with a theoretical background could build awesome tools and libraries that pragmatic web developers could use intuitively and without having to get a PhD in Computer Science (yes, Haskell, we are looking at you!).

### Development sample setup
TypeScript (from now on _TS_) is a compiled language.

> No, transpiled is not a word. When you take a high level language and translate it automatically into a lower level language, it's a _compiler_. Let's collectively deal with this and move on.

The process of compilation will start by type checking the program in order to make sure that there are no violations to the type system rules that might cause errors ultimately leading to a wrong datatype being used in a place where this does not make sense (like `"a string is not a number"/42`). After type-checking, if no errors have occurred, the original type annotations of TS, as well as any advanced features of the language, will be either stripped away ("elided") or simplified into plain JavaScript. The resulting JavaScript will have the guaranteed property that it is well behaved, meaning that it will not attempt to violate the rules of reasonable types.

In order to run TS, we need a few things that are very commonplane. A reasonably recent version of nodejs, and `yarn` (or `npm` if you don't care about wasting the precious and irreplaceable time of your limited existence, of course):

- create a new project with `yarn --init`
- add TS with `yarn add typescript -D` (`-D` because we want to include the TS compiler as a development utility but do not want any traces of it in the resulting compiled JavaScript
- add the TS config files with `yarn tsc -init`
- start compilation in watch mode with `yarn tsc -w`

Create a file `main.ts` in the same folder, and watch how the tsc compiler produces a corresponding JS file. That JS file can be run with `node main.js`. Add 

```tsc
console.log("Hello!")
```

and watch the magic unfold.

### Variables, primitive types, and expressions
The most basic construct of virtually any programming language is the _binding_ of values to names. This is mostly achieved through variables, constants, and function parameters. TypeScript offers three different keywords for binding: `let`, `const`, and the legacy `var`. We will not discuss `var` in depth as we consider it deprecated and do not use it: variables declared with `var` behave in a non-standard way with respect to scope, and as such we will use the better behaved `let` keyword. `let` and `const` declare a new symbol and bind it to a value right away:

```ts
let x = 10
const y = "hello world"
```

The names introduced by `let` and `const` are only available within the block (delimited by `{` and `}`) in which they are declared. This is called _scope_ and is considered the standard behaviour in modern programming languages:

```ts
let x = 100
console.log(x)
{
  let x = "hello world!"
  console.log(x)
}
console.log(x)
```

will print

```sh
100
hello world!
100
```

because after the closing `}` `x` refers back to the first declaration!

> Some constructs, such as lambda expressions, do not require `{` and `}` to define a scoping block, but the rules of scoping will apply anyway exactly as if curly brackets had been used.

While `let` introduces a variable, which can be further reassigned, `const` introduces a constant which cannot be changed later on in the program.


#### Primitive types and their expressions
TypeScript supports the usual basic arithmetic expressions on numbers, including the standard `Math.*` library of functions. For example:

```ts
let i = 2
let j = 3
console.log(i + j)
console.log(i * j)
console.log(i / j)
console.log(i % j)
console.log(i ** j)
console.log(Math.log2(i ** j))
```

Be careful: in TypeScript all numbers are `double` precision floating points (64 bits). While this might be sufficient in most scenarios, be aware that it is also possible to use the big guns (ehm, `big int`s to be precise), and sacrifice some runtime performance for high precision. We switch to `bigint` by using the `n` suffix after the last digit:

```ts
let i = 2n
let j = 3n
console.log(i + j)
console.log(i * j)
console.log(i / j)
console.log(i % j)
console.log(i ** j)
```

Booleans are also implemented the usual way, minus the `xor` operator which admittedly almost nobody ever uses:

```ts
let i = true
let j = false
console.log(i && j)
console.log(i || j)
console.log(!i)
```

Strings feature the usual basic operators (`+` for concatenation,  `length` for, well, getting the length of the string), plus a long list of available methods that perform all sorts of string processing:

```ts
let i = "hello"
let j = "world"
console.log(i + j)
console.log(`${i} ${j}`)
console.log(i.length)
console.log(`${i} ${j}${"!".repeat(5)}`)
console.log(`${i.replace("h", "H")} ${j}${"!".repeat(5)}`)
```


#### Basic type checking
TypeScript performs static type checking. We can verify this by attempting to compile the following code:

```ts
let i = "hello"
let j = 2
console.log(i / j)
```

This will result in the prompt of a type error:

```sh
error TS2362: The left-hand side of an arithmetic operation must be of type 'any', 'number', 'bigint' or an enum type.
```

Signaling that, while the code itself is sort of ok (dividing a variable by another variable), the fact that those variables are bound to types that do not support the division operator `/` causes an error. This sort of error is the result of a static analysis that runs through the code, makes use of all available information about which variable is bound to which type, and signals all incompatibilities. 

> One of the limitations of the type checker is that it will disallow code which could be ok in JavaScript, but is excluded from running within TypeScript. 
> The type checker therefore will exclude a lot of programs contain bugs, but at the same time it will also exclude a few valid programs that are caught in the crossfire.
> C'est la vie.

But how does TS know what type a variable have? Simple: unlike older languages like C or Java, where each variable must be painstakingly associated with a type manually, in TS the expression we assign to the variable when declaring is used in order to guess the type. This guessing process is called "type inference" (Computer Science people do like their big words, and "inference" sounds a lot better than "guessing").

In the following, TS has correctly inferred that `i` has type `string` and `j` has type `number`. As such they may be concatenated with the `+` operator which will convert `j` to a string:

```ts
let i = "hello"
let j = 2
console.log(i + j)
```

A variable may not change type. For this reason, the following code will not compile:

```ts
let i = "hello"
i = 100
```

with an error signaling that we are trying to put something of the wrong type for the variable:

```sh
error TS2322: Type 'number' is not assignable to type 'string'.
```

We may influence the process of type inference by providing type hints ("hints" sounds boring so the CS people went for "annotations", again sounding a lot smarter). Type annotations are given by using the semicolon:

```ts
let i:string = "hello"
let j:number = 2
console.log(i + j)
```

If we attempt something that does not make sense from a typing perspective, then we will get rewarded with a friendly error by means of which TS kindly requests that we improve the glaring errors in our miserable code. The following:

```ts
let i:string = "hello"
let j:string = 2
```

will produce an error:

```sh
error TS2322: Type 'number' is not assignable to type 'string'.
```

Simple primitive types such as `number` or `string` do not really require type annotations. Type annotations will be useful when using advanced types, which TS cannot infer on its own.

### Conditionals
Conditionals in TS follow the recognizable patterns of mainstream programming languages for the definition of conditional statements, but there are some unexpected twists in the relationship between types and conditionals that we will explore a bit later.

The conditional constructs of TS are three:
- ye olde _if statement_ `if-then-else`;
- ye olde _ternary operator_ `: ?`;
- ye olde _switch statement_ `switch-case`.

If statements require a boolean expression between brackets. If the boolean expression evaluates to `true`, we run the statement(s) in the first block of curly brackets, but if the expression evaluates to `false` then we run the statement in the second block of curly brackets (the second block is optional, if it's not there then nothing is executed when the expression is `false`):

```ts
if (x) {
  console.log("x is true")
} else {
  console.log("x is false")
}
```

We can nest if statements along multiple patterns, depending on our scenarios:

```ts
if (x) {
  if (y) {
    // x = true, y = true
  } else {
    // x = true, y = false
  }
} else if (z) {
  // x = false, z = true
} else {
  // x = false, z = false
}
```

The if statement is a statement, meaning that it is one of the building blocks we can add to the flow of our program. We use it in order to implement scenarios like _"if this do thing A, otherwise do thing B"_.

> Here _doing_ refers to the execution of other statements that ultimately result in changes to variables or the state of the application.

Sometimes we want to embed a smaller decision at the _expression_ level. 

> Expressions, unlike statements, do not affect the state of the program. Examples of expressions are: `x + 3`, `"a" + "b"`, `x + y > z * w`. Examples of statements are: `a = a + 1`, `console.log("Hello world!")`.
> This means that expressions do not end up assigning variables or in general performing operations that are not immutable (other terms for this are _pure_, _idempotent_, _stateless_, or _referentially transparent_). In general, working within the constraints of purity has some big advantages. Namely, pure code is easier to prove correct, to reason about, to debug, to optimize, and even to run in parallel.

Decisions at the expression level are also expressions that return a boolean value (`true` or `false`). These decisions can be taken by means of the ternary operator `?:`, which allows us to evaluate either one expression or the other based on the value of a boolean expression (the condition). It is very similar to an inline `if-then-else`:

```ts
x && y ? "Hello!" : "Goodbye!"
```

We can easily use such conditional expressions, for example as an argument to a function, thereby saving a few lines of code (and the associated reading effort) that would have otherwise been required by the corresponding `if-then-else`:

```ts
console.log(x && y ? "Hello!" : "Goodbye!")
```

An excerpt from an actual React snippet shows a chain of such conditional expressions in action:

```ts
currentState.kind == 'subscription-failure' ||
currentState.kind == 'subscription-cancel' ||
currentState.kind == 'change-payment-method-failure' ||
currentState.kind == 'change-payment-method-cancel' ||
currentState.kind == 'pay-open-payment-failure' ||
currentState.kind == 'pay-open-payment-cancel'
? fromJSX((setState) => (
  <SubscriptionLayout.FailureOrCancel
    backToSubscriptions={() => {
      window.history.pushState('', '', (routeUpdaters as any).subscription.url)
      setState(subscriptionUpdaters.overview)
    }}
  />
))
: currentState.kind == 'new subscription flow'
? newSubscriptionWidget(currentState)
: currentState.kind == 'stop subscription flow'
? stopSubscriptionWidget(currentState)
: currentState.kind == 'edit subscription flow'
? editSubscriptionWidget(currentState)
: currentState.kind == 'resume subscription flow'
? resumeSubscriptionWidget(currentState)
: currentState.kind == 'change payment method flow'
? changePaymentMethodWidget(currentState)
: currentState.kind == 'pay open payment flow'
? payOpenPaymentWidget(currentState)
: nothing()
```

Conditional expressions play a particularly important role in the definition of lambda functions, which we will see soon.

### Loops
TypeScript supports old-school loops. These loops allow the repetition of a block of code containing one or more statements until a given condition is met.

The simplest such construct is known as the `while` loop, which:
- evaluates a given condition
- if the condition is `true`, it runs the body and starts all over again
- if the condition is `false`, it moves on to the next instruction after the loop

For example:

```ts
let counter = 0

console.log(`Starting to count.`)

while(counter < 10) {
  counter = counter + 1
  console.log(`The counter is now at ${counter}...`)
}

console.log(`Done counting.`)
```

Will produce the following result:

```sh
Starting to count.
The counter is now at 1...
The counter is now at 2...
The counter is now at 3...
The counter is now at 4...
The counter is now at 5...
The counter is now at 6...
The counter is now at 7...
The counter is now at 8...
The counter is now at 9...
The counter is now at 10...
Done counting.
```

We may nest loops of course, for example:

```ts
let s = ""
let y = 0
while (y < 5) {
  
  let x = 0
  while (x < 5) {
    s = `${s}${x > y ? "*" : " "}`
    x++
  }
  s = `${s}\n`
  y++
}

console.log(s)
```

Will produce:

```sh
 ****
  ***
   **
    *
```

When we are certain that we want to run the body at least once, we can use the `do-while` loop:

```ts
let s = ""
let y = 0
do {  
  let x = 0
  do {
    s = `${s}${x > y ? "*" : " "}`
    x++
  } while (x < 5)
  s = `${s}\n`
  y++
} while (y < 5)

console.log(s)
```

> Notice that `while` will, if the condition is `false` right away, skip the body altogether. `do-while` will always, no matter what, run the body at least once the first time before even looking at the condition. Be advised!

`while` loops suffer from a slight issue. It is possible to accidentally write a loop which never terminates, thereby temporarily turning your PC into a very expensive book holder in the best case, or very quickly spending gazillions on cloud hosting that goes directly to our good friends at Azure or AWS (who, let's be honest, don't really need our help that much...). The existence of Hellish constructs such as `break` or `continue` (yes, go for it, Google them!) makes things surrounding `while` loops potentially even more complex and unpredictable, and complexity and unpredictability in code always end up meaning one and one thing only: bugs.

In order to limit the destructive potential of very open `while` loops, our Computer Science ~~overlords~~ predecessors noticed that most loops have to do with iterating for a specific number of steps. For example, over each element of an array, or each character of a string, or each...whatever in a whatever. This led to the discovery of the `for` family of loops.

The various kinds of `for` loops are focused on iterating over a fixed number of steps. The number of steps is either determined explicitly (for example `10`, `20`, or `n`), or implicitly (for example the number of elements of an array or another iterable).

The iteration based on a fixed number of steps is considered the basic `for` loop. We have an initialization statement, which will usually introduce a new iteration variable, a condition which (very much like in a `while` loop) decides whether or not the next iteration will be performed, and finally an incremental step that keeps the counter moving. For example:

```ts
for (let i = 0; i < 10; i++) {
  console.log(`Running iteration ${i}`)
}
```

In general, a `for` loop is built according to the following "template":

```ts
for (INIT; COND; INCR) {
  BODY
}
```

Such a template can always be translated into an equivalent `while` loop:

```ts
INIT
while (COND) {
  BODY
  INCR
}
```

The big advantage of a `for` loop over a `while` loop is that the definition of how many and which steps will be taken is all neatly grouped in the first line of code, which ends up being packed with useful information. This makes `for` loops easier to read and more immediate to understand, because they follow a fixed pattern of iteration and represent it neatly in visual format.

When dealing with collections such as arrays (which we will discuss in depth later), we could iterate all the elements of the loop as follows:

```ts
let people = [{ name:"John", surname:"Doe" }, { name:"Jane", surname:"Doe" }, { name:"Alice", surname:"Doe" }, { name:"Bob", surname:"Doe" }, { name:"Trudy", surname:"Doe" }]
for (let i = 0; i < people.length; i++) {
  let person = people[i]
  console.log(`${person.name} says "Hello!"`)
}
```

The whole management of the `i` and `person` variables is very predictable and always the same for this kind of loop. Given that this kind of loop happens all the time, it has been streamlined by allowing us to skip defining and managing the `i` variable altogether. This streamlined version of this loop is the `for-of` loop:

```ts
let people = [{ name:"John", surname:"Doe" }, { name:"Jane", surname:"Doe" }, { name:"Alice", surname:"Doe" }, { name:"Bob", surname:"Doe" }, { name:"Trudy", surname:"Doe" }]
for (let person of people) {
  console.log(`${person.name} says "Hello!"`)
}
```

Much cleaner!

> Be careful not to confuse `for-of`, which iterates the _values_ of a collection, and `for-in`, which iterates the keys of the collection.

> In practice, most advanced practitioners will rarely use loops, and more often use higher order functions that manipulate collections such as `map`, `filter`, or `reduce`. Looping statements such as `while` are very open in nature. They offer us the freedom to write loops in any way we might want. While this sort of freedom might sound appealing, consider that most loops are usually very structured: "do something to each element of an array or list" is probably the most common by far and large, and other commonly recurring patterns are also easy to find. The big advantage to using constructs such as `map`, `filter`, etc. is that they restrict our freedom to write any sort of loop we want, but in doing so also greatly reduce the margin for error.
> A lot of advanced programming and design patterns have to do with reducing the margin for error anyway, so anything that helps is usually welcome.

### Functions and lambdas
TypeScript obviously has functions. There are multiple ways of defining functions. The most extensive is derived from earlier releases of JavaScript, and somewhat implies that a function is a big thing with potentially many parameters and many lines of code in the body:

```ts
function quadratic(x:number, a:number, b:number, c:number) : number {
  const firstTerm = x * a * a
  const secondTerm = x * b
  const thirdTerm = c
  return firstTerm + secondTerm + thirdTerm
}
```

Note that the declaration contains the parameters with their types as well as the return type:

$$
\begin{align}
\texttt{function }\underbrace{\texttt{quadratic}}_{\text{name of the function}}(
  \underbrace{\texttt{x:number, a:number, b:number, c:number}}_{\text{name and type of the parameters}}) : \underbrace{\texttt{number}}_{\text{return type}}
\end{align}
$$

We can invoke a function by providing arguments for all parameters:

```ts
console.log(quadratic(1, 2, 3, 4))
```

#### Anonymous functions/lambda expressions
Sometimes the body of the function is so short that one single expression that is returned immediately is sufficient. In that case, we can shorten the definition into a lambda expression as follows:

```ts
const quadratic = (x:number, a:number, b:number, c:number) : number => x * a * a + x * b + c
```

We can invoke a function defined like this exactly like we did before:

```ts
console.log(quadratic(1, 2, 3, 4))
```

When functions are defined as expressions without a name, they are called "lambda expressions". Such a lambda expression is an expression just like any other expression. This means that it can be assigned to a variable, passed as an argument to another function, and so on. For example, we could define a function that takes another function as a parameter:

```ts
const repeat = (step:(_:number) => string, numberOfSteps:number) : string => {
  let accumulator = ""
  for (let index = 0; index < numberOfSteps; index++) {
    accumulator = accumulator + step(index)    
  }
  return accumulator
}
```

We could invoke it as follows:

```ts
console.log(repeat(i => "*".repeat(i+1) + "\n", 5))
```

Obtaining:

```sh
*
**
***
****
*****
```

If we are feeling bold we might even shorten the original definition by using recursion and conditional expressions in the very elegant, but slightly more complex:

```ts
const repeat = (step:(_:number) => string, numberOfSteps:number) : string => 
  numberOfSteps <= 0 ? ""
  : repeat(step, numberOfSteps-1) + step(numberOfSteps-1)
```

Notice that `step` is a parameter of type "function from `number` to `string`". This makes of `repeat` a so-called _higher order function_, hinting at the fact that a function that gets a function as a parameter does not manipulate low-order values (like primitive types or arrays or other concrete data structures) but rather computations (that is functions), and as such is of a higher order.

#### More on closures:
In this example the lambda function `lambdaSum` is an _higher order function_ as it returns a lambda 
(requiring a `number` as input and returning a `number`).
The lambda function `lambdaSum`: 
```ts
const lambdaSum = (x: number) => (y: number) : number  => x + y
``` 
is the currying of the simple (though _different_) function 
accepting two numbers as input and returning a number (the sum of the input values):
```ts
const lambdaSum_ = (x: number, y: number) : number  => x + y
``` 
_Partial application_ of the input parameters `x` and `y`

```ts
const lambdaSum = (x: number) => (y: number) : number  => x + y
//const lambdaSum1 = (x: number) => (y: number) => x + y //type inference of return type of returned lambda

const sum_5 = lambdaSum(5) //sum_5 is a lambda containing the closure -> {x (number): 5}

/*
sum_5: 
    {x (number): 5}       //closure
    (y: number) => x + y  //lambda
*/

const sum_5_2 = sum_5(2) //equivalent to lambdaSum(5)(2)
console.log(`(5 + 2 = ${sum_5_2})`)
```
The function `sum_5` (result of calling `lambdaSum(5)`) is the returned lambda (as in the commented code above),
which contains an extra piece of information encapsulated in the so called _closure_.
The closure of `sum_5` is the binding (`{x (number): 5}`) created at the moment `lambdaSum` was called with input value `5`.

Notice that _without_ the closure: `{x (number): 5}` 
the code here of the function `sum_5`: `(y: number) => x + y` could not have been executed as the variable `x` would have not been found 
within the _scope_ of that function.
Applying `sum_5` to the second parameter (`y`) we finally compute the sum as in the uncurried version (`lambdaSum_`).
```ts
const sum_5_2 = sum_5(2) //resulting value equivalent to lambdaSum(5)(2) or lambdaSum_(5, 2)
```

By placing a breakpoint on that line of code, in _debug_ mode we would see 
in the stack of the function `sum_5`
the binding:`{x (number): 5}` appearing in the _closures section_,
that binding will be used to compute the expression `x + y` 
whose value will then be returned by the function.
The binding `{y (number): 2}` is created when applying `sum_5` to that input value (with the call: `sum_5(2)`).


```ts
const sum_5_2_ = lambdaSum(5)(2) //equivalent to sum_5(2)

```
More examples of the same curried function `lambdaSum` using different syntax available in TypeScript:
```ts
//As function returning a function (equivalent to lambdaSum):
function lambdaSumFunc(x: number) {
  function internalSum(y: number) : number {
    return x + y
  }
  return internalSum
}

//lambda returning a function (equivalent to lambdaSum):
const LambdaFunc = (x: number) => {
  function internalSum(y: number) : number {
    return x + y
  }
  return internalSum
}

//lambda returning a function (equivalent to lambdaSum):
function FuncLambda (x: number) {
  const internalSum =  (y: number) : number  => x + y
  return internalSum
}

const sum_5_2__ = lambdaSumFunc(5)(2)


//Have a look at the return types here:
const lambdaSumAsString = (x: number) => (y: number) : string  => x + y + ""
const lambdaSumAsString_ = (x: number) => (y: number)  => x + y + ""

const lambdaSumPrint = (x: number) => (y: number) : void => console.log(x + y)
const lambdaSumPrint_ = (x: number) => (y: number) => console.log(x + y)
```

#### Scope
Functions define a _scope_. The scope represents the reach of a name, such as a function parameter, a temporary variable, etc. 


### Custom types and interfaces
...including unions...
...including flow type system...
...including structural vs nominal typing (quacking ducks)...

### Generics
Tuples, Array

#### Generic containers
immutablejs
...`for-of` loops...
...`for-in` loops...

#### Writing generic types

### Advanced types
generic constraints
constraints and recursive constraints (functors)
return constraints
structured strings
Any
Void
Never

case study: type safe routes

### Iterators and generators
...

### Object oriented programming
classes
interfaces
inheritance



# Extra reading materials
https://www.typescriptlang.org/docs/handbook/utility-types.html
https://www.typescriptlang.org/docs/handbook/advanced-types.html
https://www.typescriptlang.org/docs/handbook/2/types-from-types.html

