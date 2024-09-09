# Unit 1 - Introduction to functional programming

Functional programming is a programming paradigm that is profoundly different from the imperative paradigm. This programming paradigm grants additional properties on the code that help the programmer to write code that is maintainable and correct. For this reason also imperative languages widely used in the industry have been including functional programming construct during the last decades: C\#, C++, Java, Javascript, Typescript all provide means of writing programs in terms  of functional programming abstractions, and some of them even integrated type systems that are as expressive as those of Haskell or CamL. It is evident that functional programming trascendended the role of a tool used only by researchers and pioneers of programming and is becoming more and more a reality in the industry.

The goal of this unit is to show the reader a comparison between the model of imperative programming, which was explained during the Development courses of the first year, and the functional paradigm. We then proceed to define the semantics of a functional programming language, namely Typescript, and we present its main features.

## Imperative programming vs functional programming

An imperative program is made of a sequence of instructions or commands that alter the memory, which is in general refered to also as _State_. In the introductory programming courses of the first year we said that a program is evaluated through its _semantics_, that is a set of rules defining the behaviour of each construct in the language. In general, in an imperative programming language, each of these rules processes a program and in general returns a new program and a new state.

$eval(\langle P \rangle ,S) \Rightarrow \langle P '\rangle,S'$


The new program is generally returned because the evaluation of a single instruction might, in general, require multiple steps. The new state is due to the fact that some of the instructions might perform operations in memory and thus change the state.

In the following more advanced courses we further extended the model of state by allowing _scoping_ and differentiate between _stack_ and _heap_, but the underlying logic does not change in its substance. Indeed the evaluation of a program with stack and heap is given by the following rule\:

$eval (\langle I;J\rangle,S,H) \Rightarrow \langle I';J\rangle,S',H' \text{ where } eval(\langle I\rangle,S,H) \Rightarrow \langle I'\rangle,S',H'$

In this context the program uses a shared memory that is read and written by each instruction. This means that the order that we use to execute the instructions change the behaviour of the program. So two different programs with the same instructions in different order might produce two different results. For example consider the following two code snippets in Python\:

```python
x = 5
y = 2
x = x + 1
y = x - y
```

```python
x = 5
y = 2
y = x - y
x = x + 1
```

In the first case, at the end of the execution, the state is $\lbrace x := 6, y := 4 \rbrace$ while in the second case the state is $\lbrace x := 6, y := 3 \rbrace$. The different results are determined by the different order of execution of line 3 and 4 in the two programs, wich is inverted in the second version. This creates undesirable side effect not only in single\-thread programs but also in multi\-thread ones. Getting rid of the problems related to shared memory often grants additional correctness properties and benefits, for example, parallelization and code maintainability. One very straightforward solution is getting rid of the mutable state itself, and this is where functional programming comes into play.

The computation in functional programming does not rely on the notion of state, and for this reason it is defined as _stateless_ or _immutable_. The computation in a functional programming language consists of expression evaluations. Recalling the development courses of last year, we defined in the semantics a special semantics, called `eval_expr`, to process expression\: its evaluation did not change the state of the program (even though we were in the context of imperative programming) because the result was simply a value resulting from a computation.

$eval\text{\textunderscore}expr(\langle E\rangle, S) \Rightarrow \langle E'\rangle$

The stateless model of computation grants an important property called _referential transparency_. Referential transparency grants that, at any point of a program, we can replace an expression with its value without changing the final result of the computation. In the following program

```python
x = 5
x = x - 2
y = x + 3
```

we cannot replace, for instance, `x` with 5, which is the value we assign to it in the first line. For example, if we replace `x` with 5 in the last line of code, `y` would get the value 8. But this is clearly wrong, because if we execute all the statements in the program, at line 2 the value of  `x` becomse 3, and thus `y` gets 6 after executing line 3. In a referential transparent program this does not happen\: we are sure that if, at any moment, we know already the value of an expression and we replace it with its value we do not alter the result of the program execution. A consequence of this is also that, for instance, calling two different functions in a different order does not change the final result of a program, while in general this is not true in imperative programming.

Being intrinsecally different from imperative programming, functional programming will require a completely different computational model which is not based on instructions and state transitions. This underlying computational model is called _Lambda Calculus_.

## Untyped lambda calculus

Lambda calculus is a computational model that is at the foundation of the semantics of all functional programming languages. Lambda calculus is radically different from the model above, but it has the same expressive power. This means that everything that we can compute in an imperative language can be computed also in a functional language. In this section we focus on its untyped version but later on we will extend it with a type system. Since the focus of this course is functional programming and not a theoretical course on lambda calculus we will skip some details. For example we will assume that the encoding of numbers and boolean values is already available in lambda calculus, and that arithmetic and boolean operations are possible, without formally showing their implementation in lambda calculus itself. We will just compute the result of arithmetic and boolean experssion as we would normally do.

Lambda calculus is made of three syntactic elements:

* _Variables_ as $x$, $y$, $A$, ...
* _Abstractions_, that are function declarations with **one** parameter, in the form $\text{fun} \; x \rightarrow t$ where $x$ is a variable and $t$ is the body of the function.
* _Applications_ (calling a function with one argument) in the form $t \; u$ where $t$ is a function being called and $u$ is its argument. Both $t$ and $u$ can be themselves complex expressions in lambda calculus.

Note that in lambda calculus abstractions can be passed as values, thus functions can be passed as arguments of other functions.

Now that we have defined the syntax of the language, we have to define its _semantics_, that is how its elements behave. This will be done through evaluation rules, in the same fashion as we did in previous development courses.

**Variables**

$eval \; x \Rightarrow x$

This rule states that variables cannot be further evaluated.

**Abstractions**

$eval \; (\text{fun} \; \; x \rightarrow t) \Rightarrow (\text{fun} \; x \rightarrow t)$

This rule states that abstractions **alone** cannot be further evaluated.

**Substitution rule**

$eval \; (\text{fun} \; x \rightarrow t) \; u \Rightarrow t[x \mapsto u]$

This rule states that whenever we call an abstraction with a parameter $u$, the result is the body of the abstraction where its argument variable is replaced by $u$

**Example**

$eval \; (\text{fun} \; x \rightarrow x) \; A \Rightarrow x[x \mapsto A] \Rightarrow A$

$eval \; (\text{fun} \; x \rightarrow \text{fun} \; y \rightarrow x) \; (\text{fun} \; x \rightarrow x) \Rightarrow$

$(\text{fun} \; y \rightarrow x)[x \mapsto (\text{fun} \; x \rightarrow x)] \Rightarrow$

$\text{fun} \; y \rightarrow (\text{fun} \; x \rightarrow x)$

**Function application**

$eval \; (t \; u) \Rightarrow v \text{ where } eval \; t \Rightarrow t', eval \; u \Rightarrow u', eval \; t' \; u' \Rightarrow v$

This rule states that, whenever we have a function application where the left term is not immediately an abstraction, we have to evaluate the terms left\-to\-right and then use their evaluation to compute the final result.

**Example**

$eval \; ((\text{fun} \; x \rightarrow x) \; A) \; ((\text{fun} \; y \rightarrow A) \; B)$

$eval \; A \; ((\text{fun} \; y \rightarrow A) \; B)$

$A \; A$

**Currying**

As defined above, abstractions can accept only one input parameter. This might look limiting, since in all programming languages it is possible to write functions that accept multiple parameters. However, in functional programming, we can return other abstractions as result of an abstraction to overcome this limitation. Consider the following function in Typescript:

```typescript
function add(x : number, y : number) {
  return x + y
}
```
Assuming that arithmetic operators are available in lambda calculus (see the remark below for this),  we can build a function that takes `x` as parameter and whose body contains **another abstraction** that accepts `y` as parameter and finally computes  `x + y` in its body\:

$\text{fun} \; x \rightarrow \text{fun} \; y \rightarrow x + y$

Now, let us actually test that this behaves as expected\:

$((\text{fun} \; x \rightarrow \text{fun} \; y \rightarrow x + y) \; 5) \; 3$

$(\text{fun} \; y \rightarrow 5 + y) \; 3$

$5 + 3$

This mechanism is known as _currying_\: the arguments to the abstraction are substituted one by one and replaced sequentially. Since the outer abstraction body contains another lambda, when we perform the first substitution, the result will be another abstraction whose body contains the inlined value 5 and the argument variable `y`. At the next function application, the parameter `y` of this second abstraction will be replaced by 3.

This can be seen as a mechanism where the outer abstraction can produce infinitely many abstractions as result that are capable to add any number to 5 (but to 5 only\!).

For compactness, from now on, the following verbose notation

$\text{fun} \; x_1 \rightarrow \text{fun} \; x_2 \rightarrow ... \rightarrow \text{fun} \; x_n \rightarrow t$

will be shortened into

$\text{fun} \; x_1 \; x_2\;  ...\; x_n \rightarrow t$

so we will not write the internal arrows and $\text{fun} \;$ symbols.

**Bindings**

Let us consider the following lambda calculus expression\:

$((\text{fun} \; f \; g \rightarrow f (\text{fun} \; y \rightarrow A) \; (\text{fun} \; g \rightarrow g) \rightarrow A) \; ((\text{fun} \; y \rightarrow A) \; (\text{fun} \; g \rightarrow g)) \rightarrow A)$


The expression $(\text{fun} \; y \rightarrow A) \; (\text{fun} \; g \rightarrow g) \rightarrow A$ appears twice. Since it is inconvenient to write such a complex expression every time we need it, we would like to be able to write someting like\:

$\text{let E} = (\text{fun} \; y \rightarrow A) \; (\text{fun} \; g \rightarrow g) \rightarrow A \text{ in } ((\text{fun} \; f \; g \rightarrow f \; E) \; E)$

This construct is called _binding_. Note that this does *not* (notice the emphasis on this word) have the same semantics as variable assignment in imperative programming\: a variable assignment changes the state of a program, whilst here there is no change of state (we do not have a state at all) and it is merely a renaming of expressions.

We now show that bindings can be expressed in the current semantics of lambda calculus without adding something new. When we have a binding in the form

```
let x = y in t
```

we replace every occurrence of `x` in `t` with `y`. This allows us to use a shorter "version" `x` in `t` that will be replaced by its full form `y` when we need to evaluate the lambda calculus program. This is not much different than how we normally substitute variables in the substitution rule of lambda calculus. Actually, it is just the same! Thus, the semantics of `let` can be just expressed in terms of the substitution rule\:

$eval \text{ let } x = y \text{ in } t \Rightarrow eval \; (\text{fun} \; x \rightarrow t) \; y$

**Shadowing**

Consider the following lambda calculus expression\:

$(\text{fun} \; x \; y \; x \rightarrow y \; x) \; A$

One could blindly apply the substitution rule and end up with

$\text{fun} \; y \; x \rightarrow y \; A$

This is, however, incorrect\: the scope of the outer variable `x` and the inner variable `x` is different. The `x` that we incorrectly substituted above is referring to the inner `x` parameter and not the outer. In other words, the inner `x` _shadows_ the outer `x`. The correct result will, thus be

$\text{fun} \; y \; x \rightarrow y \; x$

For better clarity, one should imagine that the expression presented at the beginning of this paragraph was written as\:

$\text{fun} \; x_1 \; y \; x_2 \rightarrow y \; x_2$

where it is shown explicitly that the outer parameter is different than the inner.

**A final note on lambda calculus**

Bindings are one of the many examples of language extensions that can be implemented directly in lambda calculus as it is. They are just syntax to make the language more usable but lambda calculus in its current form has the same expressive power as any regular programming language. Since this is not a theoretical course on formal computational models, we will not show here how to map every construct that we explain to lambda calculus, but we will merely acknowledge that this is possible. For instance, it is possible to build numbers, boolean, arithmetic operations, boolean operators, and conditional expressions (if\-then\-else) only in terms of abstractions, but we will just accept that this is possible without proving it and use them in functional languages and use them right away with their usual semantics. The only thing that we will add is a type system to lambda calculus.

# Typed Lambda Calculus

In this section we present the typing rules for lambda\-calculus. As we have seen in the previous unit, lambda\-calculus is made of the following main components\: variables, abstractions, and function applications.

In this context we assume that we have a set of declarations that contains the type definitions encountered so far, and we call this $T$. Moreover, we to say that a term $x$ in the language has type $t$, we write $x : t$

When we encounter a variable $v$, we simply return its type looked up in the declaration table\:

$check(v,T) \Rightarrow (T[v],T)$

This means that we look up in the declarations the variable $v$. If this exists then the look-up will return its type. If the variable does not exist then the typechecking fails.

When we encounter an abstraction, we simply add the type of its parameter (remember that it is alway one) to $T$, and then we proceed to typecheck the function body\:

$check(\text{fun} (x : t_1) \rightarrow e) \Rightarrow (t_1 \rightarrow t_2,T) \text{ when } check(e,T[x := t_1]) \Rightarrow (t_2,T)$

Finally, when we encounter a function application, we simply make sure that the input of the function has the same type of its parameter\:

$check((f \; x),T) \Rightarrow (t_2,T) \text{ when } check(f,T) \Rightarrow (t_1 \rightarrow t_2) \text{ and } check(x,T) \Rightarrow t_1$

**Example**

Complete the type annotations for the following program (the implementation details are hidden for brevity).

```fsharp
let foo (x : int) (s : string) : int = ...
let (z : ___) = foo 5
```
Remember that the definition of `foo` is equivalent to `fun (x : int) -> (s : string) -> ...`. According to the typing rules of lambda calculus, in order to determine the type of `foo`, we have to type check the body of the function after adding the parameter to the type declaration. This results in having T = { x := int }. In order to get the full type of this function, we need to recursively type check its body, which is itself another lambda\: `(s : string) -> ...`. Now, since the text says that the body of the function is not provided, we can safely assume that is correctly formed, so that the type checking of the body succeeds and returns `int`, as provided in the function declaration `let foo (x : int) (s : string) : int`. According to the second type rule of lambda calculus, this lambda has thus type `string -> int`. Then the body of `fun (x : int) -> (s : string) -> ...` has type `string -> int`, and the full type of the function becomes `int -> string -> int`.

At this point we can determine the type of the binding in the second line. This binding contains the result of a partial application of `foo`. According to the third type rule of lambda calculus, we have to make sure that the argument passed in the call matches the type of the parameter in the definition of the function. Since, they are both `int`, this succeeds. At this point the resulting type is the type of the body of the lambda, i.e. the type of `(s : string) -> ...`. We have already determined its type above, which is `string -> int`, which is also the type of the binding `z`.

You can notice that, when we partially apply a function, the effect is that we remove the type of each argument that is passed from the "arrow" type, and what is left is the resulting type. For instance, the full type of the function `foo` was `int -> string -> int` and we passed only one argument in the call, so we remove the `int` part in the full type and we are left with `string -> int`. If we passed two arguments in the call, then we would have to remove both `int` and `string`, and we would be left with just `int`.

## Typescript vs Lambda Calculus

In this section we proceed to introduce Typescript, the language of choice for this course, by showing how the language abstractions of lambda calculus are mapped to it. Typescript is a hybrid functional-imperative language. This means that in Typescript we can combine abstractions from functional programming with imperative statement. In this course we use its imperative part only to display something in the standard output. We choose this language over pure functional languages like Haskell because I/O operations (and not only) in such languages require the knowledge of advanced functional design patterns called _Monads_ that go beyond the scope of this course.

**Bindings**

Bindings follow a syntax similar to what we have defined in lambda calculus, for example\:

```typescript
const x = 5
return x + 1
```
Notice that in Typescript there are two ways to define bindings:
```ts
const x = 5
let y = 3
```
Using `const` defines an immutable binding, which is the same as `let` in lambda calculus. The `let` keyword defines instead a mutable binding (variable).

**Type Annotation in Typescript**

Type checking in Typescript follows the same basics rules defined in the previous section. Type annotation of bindings can be achieved with the following syntax:

```ts
const (id : type) = expr
```

where `id` is the identifier used for the binding and `type` a valid type indentified. Basic types in Typescript are `bigint`, `number` (double\-precision floating\-point numbers like `double` in C\#), `string`, ...

For example\:

```ts
const (x : number) = 5.25
```

Note that Typescript does not perform automatic type conversions like C\#, so for example the following code will produce a type error\:

```ts
const (x : bigint) = 5
```

whereas, the same code in C\# would work\:

```csharp
double x = 5;
```

In order to correctly type the expression above, we use the correct double precision integer literal:

```ts
const (x : bigint) = 5n
```

Type conversions can be achieved by using conversion functions, such as `Number`, `BigInt`, `String`, ... They are all named after the type you want to convert to. These are especially useful when doing operations between incompatible types. For instance, in C\# the expression `x / 5.0` when `x` has type `int` is accepted and typecheck correctly, while in Typescript it would return a type error because no automatic conversion is performed. For this reason one should use the conversion function `Number` to convert `x` in a floating point number:

```ts
Number(x) / 5.0
```

Another important type in Typescript is `unit`. This identifies the absence of values and its only value is `{}`. Note that the value of type `unit`, unlike `void` in C\#, can be normally bound to variables (in C\# there is no value corresponding to `void`). `unit` can be used when a function takes no arguments or returns nothing. Being an imperative language, Typescript support also `void` as C\#.

**Abstractions and Function Applications**

Abstractions can be defined by separating the arguments and the body with a `=>`. For example, the lambda calculus abstraction $\text{fun} \; x \rightarrow x + 1$ would become in Typescript\:

```ts
(x => x + 1)(3)
```

Naturally, if we bind an abstraction we can then apply it by using the name of the binding, such as\:

```ts
const inc = x => x + 1
return inc 3
```

which in lambda-calculus would become $(\text{fun} \; inc \rightarrow inc \; 3) \; (\text{fun} \; x \rightarrow x + 1)$. Note that shadowing is not allowed in Typescript, so the following snippet is an invalid program:


```typescript
let f = x => x + 1
let f = x => x - 1
return f(3)
```

It is also possible to use multiple lines of code in an abstraction, in such case we have to enclose the body of an abstraction in curly brackets and use the `return` statement to return a result:

```ts
const f = x => {
  const res = x + 1
  return res
}
```

Arguments of lambda abstractions can be typed as:

```ts
const f : (arg1 : t1) => (arg2: t2) => ... => (argn: tn) => texpr =
  (arg1 : t1) => (arg2 : t2) => ... => (argn : tn): texpr => expr
```

Notice that, typescript requires to give a name to arguments even in the type declaration.
 
Typescript supports generic polymorphism, thus we can provide type parameters to functions. Generic type parameters are defineds as follows:

 ```ts
const stringify : <T>(_ : T) => string = <T>(x : T): string => String(x) 
 ```

 This means that the function `stringify` is generic with respect to the type parameter `T`. Notice that the type parameter declaratin needs to be repeated both in the type of the binding and in the function declaration. You can of course rely on type inference and omit the type of the binding completely:

  ```ts
const stringify = <T>(x : T): string => String(x) 
 ```

## Conditionals

Conditionals in functional programming have a different semantics then in imperative programming. In the previous programming courses we saw that imperative languages support two different kinds of conditionals\: one is a conditional _statement_ and one is a conditional _expression_. Typescript supports a conditional expression with the syntax\:

```ts
condition ? thenExpr : elseExpr
```

identical to its counterpart in several programming languages, for instance C\#, and having the same semantics

`eval <if C then T else E>` $\rightarrow$ `<if C' then T else E>  where  eval <C>` $\rightarrow$  `C'`

`eval <if true then T else E>` $\rightarrow$ `<T>`

`eval <if false then T else E>` $\rightarrow$ `<E>`

For convenience, we will also use conditional statements, but all the code we write will always follow the semantics of the conditionals in functional programming, i.e. as if they were conditional expression, for instance:
```ts
  const toInfinityAndBeyond = x => {
    if (x >= 0)
      return x + 1
    else
      return x - 1
  }
```

so a conditional will always be followed by an `else` expression or a short circuit that returns a value when the condition is `false`:

```ts
  const toInfinityAndBeyond = x => {
    if (x >= 0)
      return x + 1
    return x - 1
  }
```

## Recursion

In functional programming there is no concept of loop. Indeed loops require stateful computation, as their semantics is defined as\:

`eval (<while C do T>,S)` $\rightarrow$ `(<if C then { T; while C do T } else { done }>, S)`

Functional programming can achieve an equivalent behaviour through recursive functions. In Typescript recursive functions do not require any special syntax, as in many other programming languages. For example, let us assume that we want to return a string containing all the numbers divisible by a number `n`. We can achieve this through a recursive function\: this function starts from 1 and tests all the numbers less or equal than `n` one by one. The base case of the recursion happens when the number that we are testing is greater than the current number. In this case we simply return an empty string. Otherwise we check if the number is divisible by `n` and, if that is the case, we add it to the string\:

```ts
const getDivisors = (n : number) => (i : number) : string => {
  if (i > n)
    return ""
  else
    if (n % i == 0)
      return i + " " + (getDivisors (n)(i + 1))
    else
      return getDivisors (n)(i + 1)
}
```

Note that this function is equivalent to the following imperative code in C\#\:

```csharp
static string GetDivisors(int n)
{
  int i = 0;
  string s = "";
  while (i <= n)
  {
    if (n % i == 0)
      s += " " + i;
  }
  return s;
}
```

Observe how, in the case of the recursive function, we use the stack of the recursive calls to carry ahead all the information necessary for the computation instead of relying on side effects and the state. This should give the reader an idea on why recursion and loops are two sides of the same coin, and why functional programming is as computationally expressive as imperative programming. The recursive function can then be invoked, for example, as\:

```ts
getDivisors(15)(1)
```

This approach is not ideal, since the second parameter that we provide in the function application should always be 1, but we ask the caller to provide it. The recursive function above can be encapsulated within another function\: this function takes as parameter only the number `n` and defines **internally** the code of the recursive function, that can just be invoked every time by passing `n` and `1`.

```ts
const getDivisors = (n : number) : string => {
  const mkDivisors = (n : number) => (i : number) : string => {
    if (i > n)
      return ""
    else
      if (n % i == 0)
        return i + " " + (mkDivisors(n)(i + 1))
      else
        return mkDivisors(n)(i + 1)
  }
  return mkDivisors(n)(1)
}
```


## Printing to the standard output

As said above, Typescript is a hybrid objected-oriented/functional programming language, thus it supports also side effects. In this course we rely on side effects only to print something to the standard output. In Typescript it is possible to print to the standard output with the `console.log` method. This method accepts a string as argument, and you can use string interpolation to compose more complex strings:

```ts
const city = "Rome"
const foundationYear = 748
const distance = 1615
console.log(`The city of ${city} was founded in ${foundationYear} and is ${distance} KM far from Rotterdam`)
```

## Summary

In this unit we started by describing the differences between imperative and functional programming. We showed that functional programming involves _stateless computation_ as functional program consists of a sequence of expressions that are evaluated, rather than a sequence of instructions that change a state. We hinted that this has the benefit that the result of a program never depends on the order of evaluation of its expressions or function calls.

We then proceeded to outline the model that underpins all functional programming languages, called _lambda calculus_. In this unit we presented the semantics of the untyped version of this model.

We then introduced the programming language Typescript and show how to map constructs from lambda calculus in it.

# Exercises

## Exercise 1

Implement a function

```ts
const allNumber (n: number) : string =
```

that returns a string containing all numbers from 0 to n. Separate the numbers with a white space.

## Exercise 2

Implement a function

```ts
const allNumberRev (n: number) : string
```

that returns a string containing all numbers from n to 0. Separate the numbers with a white space.

## Exercise 3


Implement a function

```ts
const allNumberRange (lower: number) (upper: number) : string
```

that returns a string containing all numbers from n to 0. Separate the numbers with a white space.

## Exercise 4

Implement a function

```ts
const allNumberRangeRev (lower: number) (upper: number) : string
```

that returns a string containing all numbers between lower and upper in reverse order. Separate the numbers with a white space.

## Exercise 5

Implement a function

```ts
const allEvenRange (lower: number) (upper: number) : string
```

that returns a string containing all even numbers between lower and upper. Separate the numbers with a white space.

## Exercise 6

Implement a function

```ts
const drawLine (length: number) : string
```

that returns a string containing length asterisks.

## Exercise 7

Implement a function

```ts
const drawSymbols (symbol: char) (length: number) : string
```

that returns a string containing length repetitions of symbol.

## Exercise 8

Implement a function

```ts
const toBinary (n: number) : string
```

that returns a string containing the binary representation of the input number (it must be positive).
The binary representation is obtained using the following procedure:

1. Add to the end of the string the remainder of the division between the current number and 2.
2. Repeat the previous step with `n / 2` until the number is 0. In this case simply don't add anything.

## Exercise 9

Implement a function

```ts
const toBase (n: number) (base: number) : string
```

that returns a string containing the representation of the input number in an arbitrary base (the number must be positive). The algorithm is the same as above except you must take the remainder of `n` divided by `base` and pass `n / base` to the next step.
