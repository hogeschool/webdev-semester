# Unit 4 – Advanced Functions
In this unit, we study the various function operators available in Typescript. We will start with basic function manipulation operators, and then move on to important design patterns that make use of these operators.

## Function composition, pipe operator
Functions in TS are not more special than, say, integers or strings. Just like these simple data types can be defined, manipulated in expressions, transformed, taken as parameters from functions, and be returned as parameters, so can functions. This means that _functions may accept as parameters, and return as result, other functions_. A function that accepts another function as parameter is called a _higher order function_ (HOF), and a function that returns a function is known as _curried_.

> This characteristic of supporting higher order and curried functions, which was originally typical of cutting\-edge functional languages, has been widely recognized to be a fundamental aspect of high quality software engineering, and is now found in most modern programming languages. In this, F\# has played an important historical role\: the adoption of functional constructs in the mainstream began many years ago in C\#, directly integrating F\# innovations and know\-how. From C\#, "lambda functions" spread to other languages, from C++ to Java and more, finally becoming an absolute requirement. Of course other languages helped popularize these concepts, among others JavaScript played a very large role next to C\#, in shaping software engineering as we understand it nowadays. And now, we get it all in TypeScript.

A simple higher order curried function is `compose`, the function that takes two functions as parameters and invokes them in sequence, turning them into a pipeline\:

```ts
const compose <a,b,c>(f,g) = x => g(f(x))
```

`compose` is a HOF because `f` and `g`, its parameters, are both functions. Given that `compose` returns a function, we also say that it is curried.

The `compose` function is actually so central to functional programming, that we can build it as a custom operator in our custom type:

```ts
type BasicFun<a,b> = ...
type Fun<a,b> = ...
const Fun = <a,b>(f:BasicFun<a,b>) : Fun<a,b> => 
  ...
```

We could play around with function composition as follows\:

```ts
const incr = Fun(x => x + 1)
const double = Fun(x => x * 2)
const halve = Fun(x => x / 2)

let f = incr.then(double)
```

Or we could go for an even longer pipeline\:

```ts
const f = double.then(incr).then(incr).then(halve)
```

Notice that `f` does not invoke `double` or `incr`. Rather, `f` is simply a newly created, dynamic function, which will invoke `double`, `incr`, etc. whenever it itself is invoked.


An interesting pattern that we can build with function composition is function repetition, which basically implements the logic of a `for` loop by composing the body of the loop (a function `f`) `n` times with itself\:

```ts
const repeat(n, f) => if n <= 0 then id else f.then(repeat((n-1), f)
```

Thanks to `repeat`, we can perform an operation many times, easily building regular patterns\:

```ts
const star = Fun(s => s + "*")
const space = Fun(s => s + " ")
const newline = Fun(s => s + "\n")

const row = Fun(n => repeat(n, star))
const square = Fun(n => repeat(n, (row n >> newline)))
```

Notice that neither `row 3`, nor `square 5` would be pictures. They are both functions, waiting to be either composed with other rendering functions, or invoked with an initial string to render from (`square 5 ""`).

> We can also use `curry` and `uncurry` to remove the fugly `n` parameter from `row` and `square` for fun and profit. We will see it later.


## HOF design patterns
Higher order functions can be assembled along standard design patterns which arise very commonly in most data structures.


### `map`
A very powerful, and the most common design pattern when it comes to higher order and curried functions is `map` over a data structure. `map` performs a transformation of the _content_ of a data structure, without altering the structure itself. We often refer to `map` as a _structure\-preserving transformation_.

For example, consider transforming the first element of a pair `(x,y)` according to a function `f`\:

```ts
type Pair<a,b> = [a,b]
const mapTupleLeft = f => ([x,y]) => ([f(x),y])
```

Notice that the resulting pair will still be a pair (moreover: the second element is precisely the same!). This is what we mean by _preserving structure_. Had the pair become a triple, then the structure would now be different. The first element is changed, by feeding the original value to function `f`. This leads to a new first element, which not only could be of a different value, but could actually even have a fully different type. We do not consider this change of _internal type_ to be a change in structure, because the only structure we wish to preserve is the outer structure of the pair, and not the inner structure of the content to be transformed.

We could call the function as follows\:

```ts
mapTupleLeft(incr)([1,"a"]) => ([2,"a"])
mapTupleLeft(double)([2,"a"]) => ([4,"a"])
mapTupleLeft (incr.then(double)) (2,["a","b"]) => (6,["a","b"])
```


> There is a reason why the `map` design pattern is so common\: `map` is the homomorphism associated with a functor. Functors are an incredibly common structure both in mathematics and in programming, and as such they are found everywhere. Many developers, unaware of the existance of functors, rediscover the concept accidentally by noticing that their data structures could indeed implement `map`.

A tuple supports two different `map` functions\: one for the left element, the other for the right element. The preserved structure is clearly the same, but the preserved element is not\:

```ts
const mapTupleRight = f => ([x,y]) => ([x,f y])
```

We can define a structure\-preserving transformation for basically anything, but when it comes to containers, the transformation becomes even more interesting, as it allows us to _perform operations on all the content_ of the container at once.

For example, consider the `Option` datatype. `Option` is a container, that may contain at most one element. In this sense, its `map` function performs some operation on the content, thus either once or not at all, depending on the structure (`Some` or `None`) of the original input\:

```ts
const mapOption = f => o => if o.kind == "None" then Option.none() else Option.some(f(x))
```

Notice that the structure\-preservation guarantees that `mapOption` will always return `None` if the input was `None`, and `Some` if the input was `Some` (albeit with different content in the second case).

```ts
mapOption(incr)(None) => None
mapOption(incr)(Some 3) => Some 4
mapOption(double)(Some 3) => Some 6
```


Similarly, we can extend the `map` concept to `List`. This means that we will transform all elements of the list, in a recursive fashion, according to the given function\:

```ts
const mapList = f => l => l.kind == "empty" ? List.empty() : List.cons(f(x), mapList(f)(xs))
```

The structure preservation can here be seen in the fact that the result of `mapList` always has the same number of list elements as the input list, meaning that `mapList` will never alter the length of the original list\:

```ts
mapList (incr)(List.empty()) => []
mapList (incr)(List.cons(1, List.cons(2, List.cons(5))) => [2,3,6]
```


## Composing multiple `map`'s

It is possible to combine different `map` functions together. We can combine them by simply invoking them in a nested fashion, by explicitly specifying the content transformation\:

```ts
const mapListOption f = mapList (mapOption f)
```

Of course declaring a parameter `f` just to pass it along is not very pretty, as it clutters code without really adding anything interesting or particularly informative. Fortunately, function composition comes to the rescue, because we can simply compose (look out for the inverted order!) the two mapping functions as follows\:

```ts
const mapListOption = Fun(mapOption).then(mapList)
```

## Non\-structure\-preserving transformations\: `filter`

`map` is not the only common design pattern in functional programming. Another transformation that frequently arises, but only when dealing with a dynamic collection, is `filter`, which removes elements from a collection based on some predicate.

`filter` on an `Option` would check whether or not there is an element, and it respects the predicate. In all other cases, the element is discarded from the collection\:

```ts
const filterOption = p => l =>
  l.kind == "some" && p(l.value) ? Option.some(l.value)
  : Optino.none()
```

Filtering a list requires checking all elements in turn, recursively\:

```ts
const filterList = p => l =>
  l.kind == "empty" ? List.empty()
  : p(l.head) ? List.cons(l.head, filterList(p)(l.tail))
  : filterList(p)(l.tail)
```

Notice that, while `map` preserves structure (that is, the number of elements in a list), `filter` does not. Whereas `map` gives a result that might have a different content type (`List<int> => List<string>`), `filter` will always preserve the type.

`filter` decides which elements to keep, and which elements to discard from a collection, so for example\:

```ts
filterList (isEven) ([1;2;3;4;5;6]) => [2;4;6]
filterList 
  (startsWith("a")) 
    ["a"; "aa"; "baa"; "bb"] => ["a"; "aa"]
```


## `fold`
The most general design pattern, which only applies to data structures with variable length (just like `filter`), is `fold`. `fold` will apply a function to all elements in a sort of cascade, in order to aggregate them all together into a single result. A typical example of folding a sequence is adding all elements together, computing the minimum, computing the maximum, finding a specific element, etc.

`fold` is implemented on lists in two different ways, depending on the order in which we want to visit the elements (left\-to\-right or right\-to\-left)\:

```ts
const foldList = z => f => l =>
  l.kind == "empty" ? z : f(l.head, foldList(z)(f)(l.tail))

const foldList2 = z => f => l =>
  l.kind == "empty" ? z : foldList(f(z)(l.head))(f)(l.tail)
```

We could, for example, use `fold` to add all elements as follows\:

```ts
foldList 0 (add) [1;2;3] => (1 + (2 + (3 + 0))) => 6
```

or

```ts
foldList2 0 (add) [1;2;3] => (((0 + 1) + 2) + 3) => 6
```

> While for many operations it does not matter which version of `fold` we use (and thus, in those cases we should use `foldList2`, as it does not use the stack as much as `foldList` and can thus be compiled into a loop via _tail recursion optimisation_), sometimes the order in which the operations are executed really matters. Be on the lookout for these cases!

`fold` is very powerful. When a datastructure supports the  `fold` pattern, then we can build most (if not all!) of the functions we need on it via `fold` itself. For example, given that `List` supports `fold`, then we can define both `map` and `filter` on `List` via `fold`, instead of building both functions manually\:


```ts
const mapList = f => foldList (List.empty()) (x => xs => List.cons(f(x),xs))

const filterList = p => l =>
  foldList (List.empty()) (x => xs => p(x) ? List.cons(x,xs) : xs)
```


## Partial application, curry, and uncurry
Currying is the technique which lets functions return other functions. It is very commonly encountered in functional programming languages such as Typescript, whenever a function takes its parameters one at a time. For example, consider a function that adds two numbers together\:

```ts
const add = x => y => x + y
```

The `add` function does not really take as input two numbers and returns their sum. Rather, it takes as input a single number, and returns the function that takes as input the second number and adds it to the first. This means that we could call `add` with only one input\:

```ts
const incr = add(1)
```

thereby obtaining the function that adds for example `1` to its input. Add is a _curried_ function, and passing some of its parameters like we have just done to produce `incr` is called _partial application_ ("partial" because we have provided _only a part_ of its input, and not all of it).

We can also perform computations between the arguments. For example, we could define a function that determines what to do based on the value of one of the first parameters\:

```ts
const conditionalPipeline = p => f => g => x => (p x ? f : g)(x)
```

We would call this function as follows\:

```fsharp
const f = conditionalPipeline (gtz) (incr) (double)
```

and then, depending on what we would give to `f`, see either `incr` or `double` happen\:

```ts
f 3 => 4
f -3 => -6
```

A simple practical example could be drawn from the `draw` functions we built when presenting `repeat`. Instead of defining `star`, `space`, etc. manually, we could simply define them as the partial application of a curried `draw` function in which the symbol to draw is the first parameter\:

```ts
const draw sym s = s + sym
const star = draw "*"
const space = draw " "
const newline = draw "\n"
```

Interestingly, we can automatically move between curried and uncurried functions, by defining the `curry` and `uncurry` higher order functions that transform an uncurried function that takes a tuple as input into a curried function that takes the inputs one at a time, and vice\-versa\:

```ts
const curry = f => x => y => f (x,y)
const uncurry = f => (x,y) => f x y
```

Suppose now that we were provided with a small\-minded draw function which, for reasons unknown to us (incompetence? malice? who knows!) is not built with currying and thus cannot be partially applied\:

```ts
const drawWrong(sym,s) = s + sym
```

Fortunately, we can use `curry` to turn this function into its proper curried equivalent, and use it to define our utilities without ugly code repetition or the introduction of useless arguments that we only pass along or use just once in uninteresting fashions\:

```ts
const draw = curry drawWrong
let star = draw "*"
let space = draw " "
let newline = draw "\n"
```

As a final example, consider the case of a `Person` record, plus a curried `rename` function\:

```ts
type Person = { name:string; surname:string }
let rename newName p = { p with name=newName }
```

We can now define some (admittedly not very useful) utility functions that change the name of a `Person` to the desired name. These functions all expect a `Person`, thanks to the partial application of `rename` to its first parameter only\:

```ts
const georgeify = rename "George"
const janeify = rename "Jane"
const jackify = rename "Jack"
```

We can then use these functions to rename some instances of `Person` as follows\:

```ts
const p1 = johnify({ name:"Giorgio"; surname:"Ruffa" })
const p2 = janeify({ name="Alex"; surname="Pomaré" })
const p3 = { name="Clint"; surname="Eastwood" }
```


## Function records
We can combine all we have seen so far in a powerful design pattern, known as type\-classes, where we define a record of functions over some generic datatype in order to implement something that somewhat resembles an interface in object\-oriented languages.

> Be careful! While it is true that type\-classes can be used in order to simulate interfaces, they also support quite a lot more than object\-oriented languages interfaces. For example, type\-classes allow us to define static methods, constructors, and operators, which interfaces do not support because of their requirement of at least one instance of the implementing class (the instance that will become `this` in the concrete implementation).

Consider two conceptually similar datatypes, built separately and not necessarily overlapping in attribute names (but indeed overlapping in attribute _meaning_)\:

```ts
type Manager = {
  name:string
  surname:string
  birthday:Date
  company:string
  salary:number
}

type Student = {
  Name:string
  Surname:string
  Birthday:Date
  StudyPoints:number
}
```

We would now like to encode the fact that both of these data structures really represent some interface `Person`, which allows getting and setting `Name`, `Surname`, and `Birthday`. Other attributes cannot really be defined for `Person`, as they are not present in both `Manager` and `Student`. We encode this by defining a record of functions over a generic datatype `p`, which is the actual (and at this point unknown) type of a `Person`\:

```ts
type IPerson<p> = {
  getName:p => string
  getSurname:p => string
  getBirthday:p => DateTime
  setName:string => p => p
  setSurname:string > p => p
  setBirthday:DateTime => p => p
}
```

We then define the implementation of `IPerson` for a `Manager`, and for a `Student`\:

```ts
let managerPerson:IPerson<Manager> =
  {
    getName : (x:Manager) => x.name;
    getSurname : (x:Manager) => x.surname;
    getBirthday : (x:Manager) => x.birthday;
    setName : v (x:Manager) => { x with name=v };
    setSurname : v (x:Manager) => { x with surname=v };
    setBirthday : v (x:Manager) => { x with birthday=v }
  }

let studentPerson:IPerson<Student> =
  {
    getName : (x:Student) => x.Name;
    getSurname : (x:Student) => x.Surname;
    getBirthday : (x:Student) => x.Birthday;
    setName : v (x:Student) => { x with Name=v };
    setSurname : v (x:Student) => { x with Surname=v };
    setBirthday : v (x:Student) => { x with Birthday=v }
  }
```

At this point, we could define a simple, but highly generic program that takes as input both a person and its interface implementation (we call `i` the _witness_ of `p` being a `Person`, which sounds kind of awesome) and does stuff with the input `Person`\:

```ts
const genericProgram = (i:IPerson<p>) => (p:p) => {
  let p1 = i.setSurname ("'o" + i.getSurname p) p
  i.setBirthday (i.getBirthday p1 + Date.fromYears(1)) p1
}
```

We can then call this generic program on both a `Student` and a `Manager`, by providing the actual witnesses `studentPerson` and `managerPerson` respectively\:

```ts
console.log(genericProgram studentPerson 
    { 
      Name:"Jack"
      Surname:"Lantern"
      Birthday:System.DateTime(2, 3, 1985)
      StudyPoints:120
    } 
  )
console.log(
  genericProgram managerPerson 
  { 
    name:"John"
    surname:"Connor"
    birthday:System.DateTime(2, 3, 1965)
    company:"Microsoft"
    salary:150000
  } 
)
```


## Composition of functions and types
Functions have a type. The function from `'a` to `'b` is called `BasicFun<a,b>` or `Fun<a,b>`. Types built with the function `BasicFun` constructor can be mixed with all other type constructions.

For example, we could define a function that takes as input a list of curried functions, and invokes them all with the same arguments (thus multiple times). A first implementation to do this could be\:

```ts
let applyMany : (List<Fun<a,Fun<b,c>>> => a => b => List<'c>) = l => a => b =>
  List.map (f => f(b))(List.map (f => f(a))(l))
```

Of course, there is no need to go through the list twice, so we can use `List.map` only once, to invoke the function with both parameters\:

```ts
const applyMany : (List<Fun<a,Fun<b,c>>> => a => b => List<'c>) = l => a => b =>
  List.map (f => f(a)(b))
```

> The ability to compose constructs according to our will, without artificial limitations imposed by the language, is crucial, and more often than not underestimated in its impact when building high quality software.
> Composition makes it possible to build complex abstractions and large programs by leveraging the power of all the tried\-and\-tested functions and other components that we have already built and tested. Few tools offer the ability that (pure, referentially transparent) functional languages have to build components in isolation, test them, compose them, and obtain flawlessly working programs as a direct result, leading beginning practitioners of functional programming to believe that "programming in [insert functional language here] simply _works_, without bugs!".

# Exercises

## Exercise 1

Implement `map` for lists using only `fold`.

## Exercise 2
Implement `filter` for lists using only `fold`.

## Exercise 3
Implement a functionthat flattens a list of lists into a single list using `fold`.

## Exercise 4
Implement a function that applies `f : a => b =>c` to two lists of equal length `l1:List<a>` and `l2:List<b>`. If the two lists have different length, the shortest is leading.

## Exercise 5
Implement a function that folds two lists of different content and equal length. If the length is different then `None` is returned.

## Exercise 6
Implement a function
```ts
const zip (l1 : List<a>) (l2 : List<b>) : Option<List<Pair<a,b>>> = ...
```
that take two lists with the same length and creates a list of pairs containing the elements that are in the same position from both lists. Implement this function by using normal recursion and then by using `fold2`.
