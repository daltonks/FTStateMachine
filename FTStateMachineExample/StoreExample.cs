using System;
using System.Collections.Generic;
using System.Linq;
using FTStateMachine;
using FTStateMachine.Interfaces;
using FTStateMachine.Triggers;

namespace FTStateMachineExample
{
    public class StoreExample
    {
        private IStateMachine<StoreStates> _machine;
        private string _lastStoreName;
        private readonly List<Item> _unpaidItems = new List<Item>();

        public void Run()
        {
            SetupMachine();
            DispatchTriggers();
        }

        private void SetupMachine()
        {
            _machine = new StateMachine<StoreStates>(StoreStates.OutsideOfStore);

            _machine.Configure(StoreStates.OutsideOfStore)
                .On<StateEnteredTrigger>(
                    () => _unpaidItems.Any(i => i.StoreName == _lastStoreName),
                    () => {
                        Console.WriteLine("Outside of the store with unpaid items! Thief!");
                    }
                )
                .On<EnterStoreTrigger>(StoreStates.EnterStore)
                .On<StateExitedTrigger>(() => Console.WriteLine("Exiting the OutsideOfStore state"));

            _machine.Configure(StoreStates.EnterStore)
                .On<EnterStoreTrigger>(trigger =>
                {
                    _lastStoreName = trigger.StoreName;
                    Console.WriteLine($"Entering store {trigger.StoreName}");
                })
                .On<AddItemToBasketTrigger>(StoreStates.ItemsInBasket)
                .On<LeaveStoreTrigger>(StoreStates.OutsideOfStore);

            _machine.Configure(StoreStates.ItemsInBasket)
                .On<AddItemToBasketTrigger>(trigger => {
                    _unpaidItems.Add(trigger.Item);
                    Console.WriteLine($"Obtained item {trigger.Item.ItemId}");
                })
                .On<GotoCheckoutTrigger>(StoreStates.Checkout)
                .On<LeaveStoreTrigger>(StoreStates.OutsideOfStore);

            _machine.Configure(StoreStates.Checkout)
                .On<PayForItemsTrigger>(() =>
                {
                    var itemsToRemove = _unpaidItems.Where(i => i.StoreName == _lastStoreName).ToArray();
                    foreach (var itemToRemove in itemsToRemove)
                    {
                        _unpaidItems.Remove(itemToRemove);
                    }
                    Console.WriteLine("Paid for your items like a good person");
                    return StoreStates.OutsideOfStore;
                })
                .On<LeaveStoreTrigger>(StoreStates.OutsideOfStore);

            _machine.Start();
        }

        private void DispatchTriggers()
        {
            var storeName = "Shopaporium";
            _machine.Dispatch(new EnterStoreTrigger(storeName));
            _machine.Dispatch(new AddItemToBasketTrigger(new Item(Guid.NewGuid(), storeName)));
            _machine.Dispatch(new AddItemToBasketTrigger(new Item(Guid.NewGuid(), storeName)));
            _machine.Dispatch(new LeaveStoreTrigger());

            _machine.Dispatch(new EnterStoreTrigger(storeName));
            _machine.Dispatch(new AddItemToBasketTrigger(new Item(Guid.NewGuid(), storeName)));
            _machine.Dispatch(new AddItemToBasketTrigger(new Item(Guid.NewGuid(), storeName)));
            _machine.Dispatch(new GotoCheckoutTrigger());
            _machine.Dispatch(new PayForItemsTrigger());
            _machine.Dispatch(new LeaveStoreTrigger());
        }

        private enum StoreStates
        {
            OutsideOfStore,
            EnterStore,
            ItemsInBasket,
            Checkout
        }

        public class EnterStoreTrigger
        {
            public string StoreName { get; }

            public EnterStoreTrigger(string storeName)
            {
                StoreName = storeName;
            }
        }

        public class AddItemToBasketTrigger
        {
            public Item Item;

            public AddItemToBasketTrigger(Item item)
            {
                Item = item;
            }
        }

        public class GotoCheckoutTrigger { }

        public class PayForItemsTrigger { }

        public class LeaveStoreTrigger { }

        public class Item
        {
            public string StoreName { get; }
            public Guid ItemId { get; }

            public Item(Guid itemId, string storeName)
            {
                ItemId = itemId;
                StoreName = storeName;
            }
        }
    }
}
