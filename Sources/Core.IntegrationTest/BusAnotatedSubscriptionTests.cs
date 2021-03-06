﻿using System;
using System.Threading;
using FluentAssertions;
using MessageBus.Core;
using MessageBus.Core.API;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Core.IntegrationTest
{
    [TestClass]
    public class BusAnotatedSubscriptionTests
    {
        [TestMethod]
        public void Bus_SimpleSubscribtion_CallReceivedOnImplementationInstace()
        {
            ManualResetEvent ev = new ManualResetEvent(false);

            SimpleImplementation implementation = new SimpleImplementation(ev);

            using (IBus bus = new RabbitMQBus())
            {
                using (ISubscription subscription = bus.RegisterSubscription(implementation))
                {
                    subscription.Open();

                    using (IPublisher publisher = bus.CreatePublisher())
                    {
                        Person person = new Person {Id = 5};

                        BusMessage<Person> busMessage = new BusMessage<Person>
                            {
                                Data = person
                            };

                        busMessage.Headers.Add(new BusHeader
                            {
                                Name = "Header",
                                Value = "Value"
                            });

                        publisher.Send(busMessage);

                        bool waitOne = ev.WaitOne(TimeSpan.FromSeconds(5));

                        waitOne.Should().BeTrue();

                        person.ShouldBeEquivalentTo(implementation.Person);
                    }
                }
            }
        }
        
        [TestMethod]
        public void Bus_SubscribtionWithFilter_CallFiltered()
        {
            ManualResetEvent ev = new ManualResetEvent(false);

            FilterImplementation implementation = new FilterImplementation(ev);

            using (IBus bus = new RabbitMQBus())
            {
                using (ISubscription subscription = bus.RegisterSubscription(implementation))
                {
                    subscription.Open();

                    using (IPublisher publisher = bus.CreatePublisher())
                    {
                        Person person = new Person {Id = 5};

                        BusMessage<Person> busMessage = new BusMessage<Person>
                            {
                                Data = person
                            };

                        busMessage.Headers.Add(new BusHeader
                            {
                                Name = "Header",
                                Value = "WrongValue"
                            });

                        publisher.Send(busMessage);

                        bool waitOne = ev.WaitOne(TimeSpan.FromSeconds(5));

                        waitOne.Should().BeFalse();
                    }
                }
            }
        }
        
        [TestMethod]
        public void Bus_SubscribtionWithFilter_CallReceived()
        {
            ManualResetEvent ev = new ManualResetEvent(false);

            FilterImplementation implementation = new FilterImplementation(ev);

            using (IBus bus = new RabbitMQBus())
            {
                using (ISubscription subscription = bus.RegisterSubscription(implementation))
                {
                    subscription.Open();

                    using (IPublisher publisher = bus.CreatePublisher())
                    {
                        Person person = new Person {Id = 5};

                        BusMessage<Person> busMessage = new BusMessage<Person>
                            {
                                Data = person
                            };

                        busMessage.Headers.Add(new BusHeader
                            {
                                Name = "Header",
                                Value = "RightValue"
                            });

                        publisher.Send(busMessage);

                        bool waitOne = ev.WaitOne(TimeSpan.FromSeconds(5));

                        waitOne.Should().BeTrue();

                        person.ShouldBeEquivalentTo(implementation.Person);
                    }
                }
            }
        }
        
        [TestMethod]
        public void Bus_MessageBaseSubscribtion_CallReceived()
        {
            ManualResetEvent ev = new ManualResetEvent(false);

            MessageBasedImplementation implementation = new MessageBasedImplementation(ev);

            using (IBus bus = new RabbitMQBus())
            {
                using (ISubscription subscription = bus.RegisterSubscription(implementation))
                {
                    subscription.Open();

                    using (IPublisher publisher = bus.CreatePublisher())
                    {
                        Person person = new Person {Id = 5};

                        BusMessage<Person> busMessage = new BusMessage<Person>
                            {
                                Data = person
                            };

                        busMessage.Headers.Add(new BusHeader
                            {
                                Name = "Header",
                                Value = "RightValue"
                            });

                        publisher.Send(busMessage);

                        bool waitOne = ev.WaitOne(TimeSpan.FromSeconds(5));

                        waitOne.Should().BeTrue();

                        busMessage.ShouldBeEquivalentTo(implementation.Message,
                                                        options =>
                                                        options.Excluding(message => message.BusId)
                                                               .Excluding(message => message.Sent));
                    }
                }
            }
        }
    }

    [Subscribtion]
    public class SimpleImplementation
    {
        private readonly ManualResetEvent _ev;
        private Person _person;

        public SimpleImplementation(ManualResetEvent ev)
        {
            _ev = ev;
        }

        public Person Person
        {
            get { return _person; }
        }

        [MessageSubscribtion(ReceiveSelfPublish = true)]
        public void ProcessPerson(Person person)
        {
            _person = person;

            _ev.Set();
        }
    }
    
    [Subscribtion]
    public class MessageBasedImplementation
    {
        private readonly ManualResetEvent _ev;
        private BusMessage<Person> _message;

        public MessageBasedImplementation(ManualResetEvent ev)
        {
            _ev = ev;
        }

        public BusMessage<Person> Message
        {
            get { return _message; }
        }

        [MessageSubscribtion(ReceiveSelfPublish = true)]
        public void ProcessPerson(BusMessage<Person> message)
        {
            _message = message;

            _ev.Set();
        }
    }
    
    [Subscribtion]
    public class FilterImplementation
    {
        private readonly ManualResetEvent _ev;
        private Person _person;

        public FilterImplementation(ManualResetEvent ev)
        {
            _ev = ev;
        }

        public Person Person
        {
            get { return _person; }
        }

        [MessageSubscribtion(ReceiveSelfPublish = true)]
        public void ProcessPerson([HeaderFilter("Header", "RightValue")]Person person)
        {
            _person = person;

            _ev.Set();
        }
    }
}
