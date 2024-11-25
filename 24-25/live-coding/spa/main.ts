import { Range, List, Map, Set, OrderedMap, OrderedSet, Stack } from "immutable"

// Hindley-Milner type systems
type Zero = never // 0
const zero:Zero = null!
type Unit = {} // 1
const unit:Unit = {}

type Named = { name:string }
type Aged = { age:number }
type Helloer = { sayHello:() => string }

// Aged = { age:0 }, { age:1 }, { age:2 }, ...
// Named = { name:"" }, { name:"a" }, { name:"Jim" }, { name:"Kim" }, ...
type T = Aged & Named
// T = { age:0, name:"" }, { age:0, name:"a" }, { age:0, name:"Jim" }, { age:0, name:"Kim" }, ...
//     { age:1, name:"" }, { age:1, name:"a" }, { age:1, name:"Jim" }, { age:1, name:"Kim" }, ...
//     ...
// |T| = |Aged| x |Named|

type Person = Aged & Named & { surname:string } & Helloer & { kind:"person" }
type Pet = Aged & Named & { breed:string } & Helloer & { kind:"pet" }
type PersonOrPet = Person | Pet

const p1:Person = { name:"John", surname:"Doe", age:21, sayHello:() => "Hello!", kind:"person" }
const p2:Pet = { name:"Wolfsbane", age:7, breed:"Borzhoi", sayHello:() => "Wof!", kind:"pet" }

type OtherPerson = { surname:string, married:boolean } & Helloer & { kind:"person" } & Aged & Named
const p3:OtherPerson = { name:"jane", married:true, surname:"Doe", age:21, sayHello:() => "Hallo!", kind:"person" }

const pps:Array<PersonOrPet> = [ p1, p2, p2, p1, p1, p1, p2, p3 ]
for (let i = 0; i < pps.length; i++) {
  const element = pps[i];
  console.log(element.sayHello())
  if (element.kind == "person") {
    console.log(`person with surname ${element.surname}`)
  } else {
    console.log(`pet with breed ${element.breed}`)
  }
}

// PersonOrPet = person1, person2, person3, ..., pet1, pet2, pet3, ...
// |PersonOrPet| = |Person| + |Pet|

function f(a:number, b:number): number { return a+b}
//                    f(    f(1,2),                 f(3,4))

type Updater<a> = ((_:a) => a) & {
  then:(other:Updater<a>) => Updater<a>
}
const Updater = <a>(f:(_:a) => a) : Updater<a> =>
  Object.assign(f, {
    then:(function (this:Updater<a>, other:Updater<a>) : Updater<a> {
      return Updater(_ => other(this(_)))
    })
  })
const id = <a>() : Updater<a> => Updater(_ => _)

// type Pair<a,b> = { first:a, second:b, isSavedToDB:boolean }
// const Pair = <a,b>() => ({
//   create:(a:a,b:b) : Pair<a,b> => ({ first:a, second:b, isSavedToDB:false }),
//   updaters:{
//     first:(updaterOfFirst:Updater<a>) : Updater<Pair<a,b>> =>
//       Updater(pair => ({...pair, first:updaterOfFirst(pair.first)})),
//     second:(updaterOfSecond:Updater<b>) : Updater<Pair<a,b>> =>
//       Updater(pair => ({...pair, second:updaterOfSecond(pair.second)})),
//     isSavedToDB:(u:Updater<boolean>) : Updater<Pair<a,b>> =>
//       Updater(pair => ({...pair, isSavedToDB:u(pair.isSavedToDB)})),
//   }
// })

// // we call Pair with type arguments number and number, which will be bound (assigned) to the type parameters a and b
// // in order to calculate the resulting type TwoNumbers
// type TwoStrings = Pair<string, string>
// type NumAndBool = Pair<number, boolean>
// type TwoBooleans = Pair<boolean, boolean>
// type TwoTwoNumbers = Pair<Pair<number, number>, Pair<number, number>>

// const pairStringString = Pair<string, string>()
// const changeWholePair = 
//   pairStringString.updaters.first(Updater(_ => _ + "!")).then(
//     pairStringString.updaters.second(Updater(_ => _ + "..."))
//   )

// const p10:TwoStrings = Pair<string,string>().create("Hello", "world")
// const p11 = changeWholePair(p10)


// immutablejs -> scope and currying when dealing with map
const x:Array<number | string> = [1, 2, 3, 4, 5, 6, "a", "b", "c"]
const x1 = x.map(_ => {
  if (typeof _ == "string") {
    return _
  } else {
    return _ + 1
  }
})
const x2 = x.filter(_ => typeof _ == "number")
// console.log(x1)
// console.log(x2)

const l:List<number> = Range(1, 100).map(_ => _ * 2).filter(_ => _ % 3 == 0).toList()
/* Map:
    1 -> { name:John, surname:Doe }
    2 -> { name:Jane, surname:Doe }
    3 -> { name:Jimmy, surname:Doe }
*/
console.log(l.toArray())
