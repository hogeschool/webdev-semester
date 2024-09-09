# Unit 5 - Advanced (hierarchical) data structures
In this unit, we discuss the definition and management of operations around hierarchical data structures. In preivous courses, these data structures were discussed in their mutable version. In this course we explore their immutable implementation, in order to prevent side effects that could happen when updating the references or trying to use `null` values in the mutable implementation. We will start by explaining the implementation of generic trees, and then we will implement boolean and value expression trees, which are a popular data structure used in filters and similar dynamic UI-construction.

## Trees

A tree can be recursively defined as either an empty tree, or a node containing data and a sequence of subtrees (children). This means that its type definition will be both polymorphic and recursive\:

```ts
type Tree<'a> =
  | { kind:"Empty" }
  | { kind:"Node", value:'a, children:List<Tree<'a>> }
```

Like for lists, we can define `map` and `fold` higher\-order functions, respectively to mutate the content of each node in a tree, and to accumulate the result of a tree operation into an accumulator.

The `map` function will traverse the tree in some order and apply a function to each node of the tree. We choose to first apply the function to the content of the current node, and then recursively traverse the list of sub\-trees and apply `map` to them. The `map` applied to an empty tree of course results in an empty tree. In the case of a non\-empty tree, we apply `f` to the current element, and then we call the `map` function **for lists** on **the list of subtrees**, by passing a function that calls the **tree map** on each element of the list. We generate a new node by taking the result of `f` applied to the current node and the result of mapping the list of trees with the tree map.

```ts
const treeMap = (f : 'a => 'b) => (self:Tree<'a>) =>
  self.kind == "Empty" ? Tree.Empty()
  : Tree.Node(f(self.value), tree.children.map(child => child.map(f)))
```

The `fold` works similarly to its counterpart for lists. It takes as input a function that takes as input a state and an element of the tree, and updates the state. Moreover, we pass to `fold` also the initial value of the state. The function updates the state by calling `f` with the current state and element, thus generating a new state that we call `state1`. It then call `fold` **for lists** passing a function that uses the accumulator and each tree. This function will use `fold` **for trees** to update the accumulator with each subtree. As initial value of the accumulator we pass `state1`, the newly generated state at the current level.

```ts
const treeFold = (f : 'state => 'a => 'state) => (state : 'state) => (self:Tree<'a>): 'state =>
  self.kind == "Empty" ? state
  : List.fold( ...treeFold ...)
```

## Expressions
We conclude the course with a practical example. Consider a frontend application that allows users to edit custom filters on tables. The table itself has a series of columns, which we call "variables" because, well, they vary with every table. Each variable has a type, which can either be primitive (number, bool, date, string, enum) or composite (array, tuple, sum):

```ts
type Type = Primitive | Composite
type Primitive = "number" | "boolean" | "date" | "string"
type Composite = { kind:"[]", element:Type } | { kind:"*", first:Type, second:Type } | { kind:"+", first:Type, second:Type }
```

> Tuples and unions are binary operations, for simplicity. We can always represent more elements by nesting.

We can compose expressions on these types. The simplest expressions are primitives, but of course we can compose multiple expressions together:

```ts
type Expression = Primitive | Composite
type Primitive = { kind:"const-number", value:number }
  | { kind:"const-bool", value:boolean }
  // string, date
  | { kind:"[a,b,c]", values:Array<Primitive> } 
  | { kind:"(a,b)", first:Primitive, second:Primitive }
  | { kind:"inl", element:Primitive } | { kind:"inr", element:Primitive }
type Composite = 
  | { kind:"lookup", variable:string }
  | { kind:"->[a,b,c]", values:Array<Expression> } | { kind:"a[i]", array:Expression, index:Expression }
  | { kind:"->(a,b)", first:Expression, second:Expression } | { kind:"fst", tuple:Expression } | { kind:"snd", tuple:Expression }
  | { kind:"match", conditional:Expression, onFirst:Expression, onSecond:Expression } | { kind:"->inl", tuple:Expression } | { kind:"->inr", tuple:Expression }
  | { kind:"if", conditional:Expression, onThen:Expression, onElse:Expression }
  | Algebraic
type Algebraic = 
  | { kind:"+", first:Expression, second:Expression }
  | { kind:"*", first:Expression, second:Expression }
  | { kind:"~-", expr:Expression }
  | { kind:"-", first:Expression, second:Expression }
  | { kind:"/", first:Expression, second:Expression }
  | { kind:"&&", first:Expression, second:Expression }
  | { kind:"||", first:Expression, second:Expression }
  | { kind:"!", expr:Expression }
  | { kind:"==", first:Expression, second:Expression }
  | { kind:">", first:Expression, second:Expression }
  | { kind:">=", first:Expression, second:Expression }
```

There you go! Now we have types and expressions. It is time to produce an evaluator. The evaluator takes as input the current value of the variables (all primitive values, meaning any possible value including arrays, tuples, and unions, but not requiring any further calculation or reduction), an expression to evaluate, and returns either a `Primitive` value or an error:



```ts
type Scope = { variables:OrderedMap<string, [Type, Primitive]> }
const eval = (scope:Scope) => (expr:Expression) : Primtiive | "error" => {
  ...
}
```

`eval` will inspect the `expr`, and based on the content determine what to do. Let us show a few cases.

When the expression is a variable lookup, we try to find it in the scope, and if it is not available we just give an error:

```t
  if (expr.kind == "lookup") return scope.has(expr.variable) ? scope.get(expr.variable)![1] : "error"
  : ...
```

when we encounter a sum, we evaluate both arguments, and if they can be added we add them, otherwise we give an error:

```ts
  if (expr.kind == "+") {
    const first = eval(expr.first)
    const second = eval(expr.second)
    if (first == "error" || second == "error" || first.kind != second.kind) return "error"
    if (first.kind == "const-string") return { kind:"const-string", value:first.value+second.value }
    if (first.kind == "const-number") return { kind:"const-number", value:first.value+second.value }
    return "error"
  }
  : ...
```

when we encounter an array lookup, we evaluate the arguments and then try to perform the lookup:

```ts
if (expr.kind == "a[i]") {
    const array = eval(expr.array)
    const index = eval(expr.index)
    if (array == "error" || index == "error" || array.kind != "[a,b,c]" || index.kind != "const-number") return "error"
    if (array.values.length <= index.value) return "error"
    return array.values[index.value]
  }
  : ...
```

and so on, according to the semantics we are used to. Omitted for brevity, but shown in class.


# Exercises

## Exercise 1 - very hard, high risk, high reward
Extend our little language with lambda declaration and invocation. You might need to carry the closure inside the lambda expression as a temporary `Scope`, and also deal with shadowing of arguments.

## Exercise 2
Extend our little language with records and record lookup.

## Exercise 3
Extend our little language with statements, in particular: conditional statement, variable assignment statement, and console printing.

