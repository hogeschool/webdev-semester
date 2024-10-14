# Unit 2 - Types in Functional Programming

In Unit 1 we introduced an untyped and typed formulation of lambda calculus, which is the foundational model of functional programming. We then proceeded to translate the constructs of lambda calculus into the functional programming language Typescript. In this chapter we introduce a statically typed version of lambda calculus and then we show how its usage in Typescript. We then proceed to define basic data structures in Typescript.


## Basic Data Structures in Typescript

Typescript natively implements complex data structures such as _tuples_. A tuple is an ordered sequence of non-homogeneous values, such as `(3,"Hello world!",-45.3f)`. The type of a tuple is denoted as

```
[t1, t2, ..., tn]
```

where `t1`, `t2`,..., `tn` are types. Thus a tuple is the n-ary Cartesian product of values of type `t1`, `t2`,..., `tn`. For example\:

```ts
const t : [bigint, string, number] = [3n,"Hello world!",-45.3]
```

You can access the elements of a tuple as you would with an array, for instance in the example above `t[0]` would return `3n`, `t[1]` woudl return `"Hello world!"`, and `t[2]` would return `-45.3`.

Tuples, of course, can be passed as arguments to functions. In this context, there is a particular application of this which is an alternative to currying. In Unit 1 we saw that in lambda calculus (and also in Typescript) a function admits one argument only. In order to model the behaviour of functions that operate on more than one argument, we relied on the notion of currying\: a function that wants to use two arguments will simply return in its body another lambda that is able to process the second argument and has in its closure the first argument. For instance\:

```ts
const add = x => y => x + y
```

When we call such function with `add(3)(5)` we replace `x` with 3 in its body thus generating `y => 3 + y`, and then we apply `(y => 3 + y)(5)` thus obtaining `3 + 5 = 8`. An alternative to this is the _uncurried_ version, where we pass the arguments in a tuple as follows\:

```ts
const addUncurried = (t : [number, number]) => t[0] + t[1]
```

Unlike array, accessing the elements of a tuple with the index operator is a safe operation, in the sense that if you use an index out of bound the compiler is able to detect this and report an error. For instance, in the function above, if you were to use `t[2]` the compiler would report an error.

Note that the curried and uncurried versions are not interchangable because their type is different. For instance, the type of `add` is\:

```ts
const add : (x : number) => (y : number) => number
```

while the type of `addUncurried` is\:

```ts
const addUncurried : (t : [number, number]) => number
```

A third way in Typescript to have an uncurried function is what you would normally write in typical object-oriented languages by separating the argument with a `,`.

```ts
const addUncurried2 : (x : number, y : number) => number = (x : number, y : number) => x + y
```

Notice that the uncurried version of a function takes both arguments all together, thus partial application is not possible\: we can call `add 3` and this will generate as result `y => 3 + y`, but we cannot call `addUncurried 3` because this would mean passing an argument of typ `number` to a function that expects `[number, number]`. We will see further ahead in this course that it is possible to define a generic function that can convert the curried version of a function to the uncurried version, and the opposite. The same applies to `addUncurried2`, because the number of arguments will not match.

> Typescript is a bit atypical in this, since usually in functional programming languages the syntax for the arguments used in `addUncurried2` is mapped directly to a tuple, thus the type of the input of the function will match the type of a tuple. In typescript you can explicitly pass a tuple or specify a function with multiple arguments, but you cannot interchange them, i.e. you cannot call `addUncurried(3, 5)`, instead you need to use the second implementation for that. Writing `addUncurried([3, 5])` is instead a valid call.

## Records

Records are finite map of names into values that can optionally define some members. This definition resembles that of Class in a object\-oriented language, but there is a profound difference\: the fields of a record are by default immutable, meaning that it is not possible to change their values directly. In Typescript you can define an `interface` for a record with the following syntax:

```ts
interface R {
    f1 : t1
    f2 : t2
    ...
    fn : tn
}
```

Unfortunately, when instantiating an object implementing such interface, Typescript by default allows the mutation of its fields. In order to prevent this and correctly reflect the semantics of a record, we need to declare the fields `readonly`. A `readonly` field can be set when creating the instance of the record, but cannot be mutated after the creation.

```ts
interface R {
  readonly f1 : t1
  readonly f2 : t2
  ...
  readonly fn : tn
}
```

For instance, the following record represents the login information to connect to a server\:

```ts
interface LoginInfo {
  readonly userName        : string
  readonly password        : string
  readonly address         : string
}
```

Synce Typescript is indentation\-sensitive, we must place particular care about how we indent the record definition\: brackets should be indented with respect to the `type` keyword, and fields must be indented with respect to brackes. Failing to do so will often result in a compilation error.

Optionally a record can define some methods and properties (members). These can be specified in its interface as lambdas, and if they are instance methods you can specify that they accept the implicit argument `this`, so they can be invoked with the `.` syntax:

```ts
interface R{
  readonly f1 : t1
  readonly f2 : t2
  ...
  readonly fn : tn
  readonly m1: (arg1 : t1, arg2 : t2, ..., argn : tn1) => tr1
  readonly m2: (this : R, arg1 : t1, arg2 : t2, ..., argn : tn2) => tr2
}
```
Note that, given the immutable nature of records, their methods must also behave in an immutable way.

A record can be instantiated with the following syntax\:

```ts
const r : R = { 
  f1: value1, 
  f2: value2, 
  ... , 
  fn: valuen 
}
```

It is immediately evident that, for records with a high number of fields, this syntax becomes quite cumbersome. Moreover, for interfaces that contain methods, having to specify the function definition every time is a bit annoying. For this reason, it is preferable to define a function `createR`, which is equivalent to a constructor in an objec-oriented language, to instantiate a record\:

```ts
interface R = {
    f1 : t1
    f2 : t2
    ...
    fn : tn
}

const createR = (arg1 : t1, arg2 : t2, ..., argn : tn) : R => ({
  f1 : arg1,
  f2 : arg2,
  ...
  fn : argn
})
```
> When implementing such utility functions it is very important to manually specify the return type (which needs to be the same as the type of the record interface), because otherwise typescript will create a type on the fly for the object, which is different than the one of the interface. Moreover, notice that in one-liner lambdas you need to wrap brackets around the record expression, otherwise typescript interprets it as a code block. If you instead use a lambda with a code block and a `return` statement this is not necessary.

## Record copy and update

With the implementation given above, records are immutable, so it is not possible to directly update their fields. In order obtain the same effect of a field update, we must create a new record where all the values of the fields that are left untouched by the update are initialized by reading the corresponding values in the original record, and all the updated fields are initialized with the new value. For instance, let us consider the `LoginInfo` above, and suppose that you need to change the server address, that would require the following steps\:

```ts
const oldLogin = { 
  userName: "awesomeuser@aw.us", 
  password: "supersecretkey",
  address: "155.34.21.105" 
}
const newLogin = { 
  userName: oldLogin.userName,
  password: oldLogin.password,
  address: "165.40.69.69" 
} 
```
You can immediately notice that this operation becomes quite cumbersome when updating just a small number of fields of records with many fields. For this reason, the following shortcut is available\:

```ts
const newRecord = { ...oldRecord,
  f1: v1,
  f2: v2,
  ...
  fk: vk 
}
```
This will make a copy of oldRecord and initialize the fields `f1`,`f2`, ..., `fk` with the specified values, while the others simply contain the values from `oldRecord`. The concrete example above becomes then \:

```ts
const oldLogin = { 
  userName: "awesomeuser@aw.us", 
  password: "supersecretkey",
  address: "155.34.21.105" 
}
const newLogin = {...oldLogin,
  address: "165.40.69.69"
}
```

## Case Study\: Tanks and Guns

In this section we present a small case study to show the usage of records. Let us assume that we want to model an entity `Tank` defined by name, speed, weapon, armor, and health. Each tank weapon is a gun defined by name, penetration power, and damage. A tank can shoot a shell at another tank with its gun, with the following effect\: if the gun penetration is higher than the target armour then the health of the target is reduced by the weapon damage. Otherwise the amount of armour is decreased by the gun penetration. Let us first define the records for guns and tanks\:

```ts
interface Gun {
  name          : string
  penetration   : number
  damage        : number
}

const createGun = (name : string, penetration : number, damage : number) : Gun => ({
  name:             name,
  penetration:      penetration,
  damage:           damage
})

interface Tank {
  name                : string
  weapon              : Gun
  armor               : number
  health              : number
}

const createTank = (name : string, weapon : Gun, armor : number, health : number) : Tank => ({
  name:         name,
  weapon:       weapon,
  armor:        armor,
  health:       health
})
```

and let us define some gun and tank models\:

```ts
const kwk36 = createGun("88mm KwK 36", 150.0, 90.0)
const f32 = createGun("76mm F-32", 70.0, 60.0)
const kwk40short = createGun("75mm kwk 37", 35.5, 55.5)
const kwk40Long = createGun("75mm KwK 40", 99.5, 55.5)
const m1a1 = createGun("76mm M1A1", 99.0, 60.0)
const tiger = createTank("Pz.Kpfw. VI Tiger Ausf. E", kwk36, 340.0, 800.0)
const t34 = createTank("T-34/76", f32, 200.0, 400.0)
const p4f = createTank("Pz.Kpfw. IV", kwk40short, 130.0, 350.0)
const p4g = createTank("Pz.Kpfw. IV", kwk40Long, 130.0, 350.0)
const shermanE8 = createTank("M4A3 Sherman E8", m1a1, 220.0, 450.0)
```

Now let us implement the logic of the combat as a method of `Tank`. We will extend the interface by defining a method `shoot` for the tank. The function that instantiates a `Tank` will then provide the implementation of such method.

```ts
interface Tank {
  name                : string
  weapon              : Gun
  armor               : number
  health              : number
  shoot               : (this : Tank, tank : Tank) => Tank
}
```

This method will have to check the weapon penetration of `this` against the `Armor` of `tank`. If it is higher than we print a message on the status and we update the health of `tank`\. If it is lower than the target armour we reduce the armour value of `tank`.

```ts
const createTank = (name : string, weapon : Gun, armor : number, health : number) : Tank => ({
  name:         name,
  weapon:       weapon,
  armor:        armor,
  health:       health,
  shoot:
    function (this : Tank, tank : Tank) : Tank {
      if (this.weapon.penetration > tank.armor) {
        const updatedHealth = tank.health - this.weapon.damage
        console.log(`${this.name} shoots ${tank.name} causing ${this.weapon.damage} --> HEALTH: ${updatedHealth}`)
        return {
          ...tank,
          health: updatedHealth
        }
      }
      else {
        const updatedArmor = tank.armor - this.weapon.penetration
        console.log(`${this.name} shoots ${tank.name} with ${this.weapon.name} reducing armour by ${this.weapon.penetration} --> ARMOUR: ${updatedArmor}`)
        return {
          ...tank,
          armor: updatedArmor
        }
      }
    }
})
```

Now let us make two tanks fight. We do so by implementing a function that takes two tanks and repetedly calls the `Shoot` method in turn until one of the two tanks is destroyed. This function will be recursive, since it must repeat the shooting phase an indefinite number of times. The base case of the recursion is when one of the two tanks is destroyed. Ohterwise we must call the function again with the updated tanks after they shoot each other\:

```ts
const fight = (t1 : Tank) => (t2 : Tank) : [Tank, Tank] => {
  if (t1.health < 0) {
    console.log(`${t1.name}: KABOOOM!!! ${t2.name} wins`)
    return [t1, t2]
  }
  else if (t2.health < 0) {
    console.log(`${t2.name}: KABOOOM!!! ${t1.name} wins`)
    return [t1, t2]
  }
  else {
    t2 = t1.shoot(t2)
    t1 = t2.shoot(t1)
    return fight(t1)(t2)
  }
}
```

Notice that we can refactor our code: in `fight` the first two cases of the `if` are doing the same thing. We can thus define a function **nested** into `fight` that prints the message in both cases and returns `t1` and `t2`. This function will be defined as

```ts
const outcome = (loser : Tank) => (winner : Tank) : [Tank, Tank] = ...
```

This function will be called as `outcome(t1)(t2)` in the first case of the `if` and as `outcome(t2)(t1)` in the second case. Notice that, since the argument in the two calls are swapped, in the body of `outcome` we cannot simply return `[loser,winner]` because that would sometimes swap the returned tanks. We can check if `loser` is indeed `t1` and, if not, return `t2,t1` instead of `t1,t2` (by convention we are returning the `loser` tank in the first position of the tuple).

```ts
const outcome = (loser : Tank) => (winner : Tank) : [Tank, Tank] => {
  console.log(`${loser.name}: KABOOOM!!! ${winner.name} wins`)
  if (t1 == loser)
    return [t1,t2]
  else
    return [t2,t1]
}
```

With this refactoring, `fight` becomes\:

```ts
const fight = (t1 : Tank) => (t2 : Tank) : [Tank, Tank] => {
  const outcome = (loser : Tank) => (winner : Tank) : [Tank, Tank] => {
    console.log(`${loser.name}: KABOOOM!!! ${winner.name} wins`)
    if (t1 == loser)
      return [t1,t2]
    else
      return [t2,t1]
  }
  if (t1.health < 0) {
    return outcome(t1)(t2)
  }
  else if (t2.health < 0) {
    return outcome(t2)(t1)
  }
  else {
    t2 = t1.shoot(t2)
    t1 = t2.shoot(t1)
    return fight(t1)(t2)
  }
}
```


# Exercises

## Exercise 1
Model a point in the space as a record `Point2D` containing a field `Position`, which is a tuple of type `[number, number]`. Define two different constructor methods for this point\: the first creates a point given 2 coordinates `x` and `y` taken as input. The second creates a random point whose coordinates are between two parameters `min` and `max` taken as input.

## Exercise 2
Extend `Point2D` with two properties to read the first and second coordinate, and a method to compute the distance between two points. Given a point $(x_1,y_1)$ and $(x_2,y_2)$, their distance is given by  $\sqrt{(x_1 - x_2)^2 + (y_1 - y_2)^2}$. You can use the `sqrt` function to compute the square root of a number.

## Exercise 3

A `Blob` is defined by a `Position` of type `Point2D` and a `Size` of type `int`. Each `Blob` randomly roams around a 100x100 area. This means that the minimum x coordinate of `Position` can be -50.0 and the maximum 50.0. The same applies for the y coordinate. Represent a `Blob` as a record with a constructor that takes no arguments and sets the position to a random `Point2D` and the speed to a random value between 1 and 5.

## Exercise 4

Extend the `Blob` record by adding a method `Move` that takes no arguments and randomly moves the `Blob`. A `Blob` randomly choose whether to go up, down, left, or right, thus you can generate a random number between 0 and 3 to decide what to do and change the position accordingly. The movement must not take the `Blob` outside the 100x100 area, thus if either the x or the y coordinates are outside the interval `[-50,50]` they are reset to the lower bound or the upper bound, depending on where the overflow occurs (if you get past 50 you go back to 50, and if you go below -50 you go back to -50).

## Exercise 5

Create a record `World` that contains two blobs and a field `Tick`. `World` contains a constructor, which takes a number of ticks and creates two blobs, and a method `Run` that takes no parameters and move the blobs around for as many ticks as specified by `World`.
