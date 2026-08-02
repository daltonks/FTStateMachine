# FTStateMachine

[![NuGet](https://img.shields.io/nuget/v/FTStateMachine?label=FTStateMachine)](https://www.nuget.org/packages/FTStateMachine)

A small C# state machine whose triggers are ordinary objects.

FT is for free-trigger. Most state machine libraries want you to enumerate every trigger up front, alongside the states, which means a trigger can't carry data with it. Here a trigger is any class you like, so the thing that causes a transition can also bring along whatever the transition needs:

```cs
_machine.Configure(StoreStates.EnterStore)
    .On<EnterStoreTrigger>(trigger => _lastStoreName = trigger.StoreName)
    .On<AddItemToBasketTrigger>(StoreStates.ItemsInBasket)
    .On<LeaveStoreTrigger>(StoreStates.OutsideOfStore);
```

## Install

```
dotnet add package FTStateMachine
```

## Use

States are identified by a token of whatever type you pick — an enum is the usual choice. Configure each one, then start the machine and dispatch triggers at it.

```cs
var machine = new StateMachine<DoorState>(DoorState.Closed);

machine.Configure(DoorState.Closed)
    .On<StateEnteredTrigger>(() => Console.WriteLine("Door shut"))
    .On<OpenTrigger>(DoorState.Open);

machine.Configure(DoorState.Open)
    .On<StateEnteredTrigger>(() => Console.WriteLine("Door open"))
    .On<CloseTrigger>(DoorState.Closed);

await machine.StartAsync();
await machine.DispatchAsync(new OpenTrigger());
```

`DispatchAsync` takes an `object`. A trigger the current state has no handler for is ignored, so states only declare what they actually care about.

### Handling a trigger

`On<TTrigger>` comes in a few shapes, and you can register more than one handler for the same trigger on the same state:

```cs
// Run something
.On<PayTrigger>(() => Console.WriteLine("Paid"))

// Run something, with access to the trigger
.On<PayTrigger>(trigger => Console.WriteLine($"Paid {trigger.Amount}"))

// Transition
.On<PayTrigger>(StoreStates.Checkout)

// Decide the destination when the trigger arrives
.On<PayTrigger>(trigger => trigger.Amount > 0 ? StoreStates.Paid : StoreStates.Checkout)

// Only if a condition holds
.On<PayTrigger>(() => _basket.Any(), StoreStates.Checkout)
```

`OnAsync<TTrigger>` mirrors all of these with `Func<Task<TToken>>` handlers, for when a transition has to await something.

Each shape takes an optional `Func<bool> predicate`, checked when the trigger arrives. If it returns false the handler is skipped, and no transition happens.

### Entering and exiting

`StateEnteredTrigger` and `StateExitedTrigger` are dispatched by the machine itself when a state is entered and left, so setup and teardown are handled the same way as everything else:

```cs
machine.Configure(StoreStates.OutsideOfStore)
    .On<StateEnteredTrigger>(
        () => _unpaidItems.Any(),
        () => Console.WriteLine("Outside the store with unpaid items! Thief!")
    )
    .On<StateExitedTrigger>(() => Console.WriteLine("Leaving"));
```

### Forwarding

Every `On` overload ends with an optional `bool forwardTrigger`, defaulting to true. When a trigger causes a transition, it gets dispatched again at the state just entered. That's what lets one state route a trigger and the next one act on it:

```cs
machine.Configure(StoreStates.OutsideOfStore)
    .On<EnterStoreTrigger>(StoreStates.EnterStore);  // routes

machine.Configure(StoreStates.EnterStore)
    .On<EnterStoreTrigger>(t => _lastStoreName = t.StoreName);  // then acts
```

Pass `forwardTrigger: false` to stop that and have the trigger end at the transition.

## API

| Member | Purpose |
| --- | --- |
| `new StateMachine<TToken>(startingToken)` | Creates a machine. `TToken` is usually an enum. |
| `Configure(token)` | Returns the state for that token, creating it the first time. |
| `StartAsync()` | Enters the starting state. Call before dispatching. |
| `DispatchAsync(object trigger)` | Sends a trigger to the current state. |
| `GoToStartingStateAsync()` | Returns to the starting state. |

## Example

[`FTStateMachineExample/StoreExample.cs`](FTStateMachineExample/StoreExample.cs) walks a shopper through a store — entering, filling a basket, paying or not — and prints what happens at each step, including the case where they leave without paying.

## Requirements

.NET Framework 4.6.

## License

MIT.
