﻿using System;
using System.Configuration;
using MessageBus.Core.API;

namespace MessageBus.Core
{
    public class BusConfigurator : IBusConfigurator, IBlockWatcher
    {
        private string _busId = Guid.NewGuid().ToString();
        private IPublishingErrorHandler _errorHandler;
        private ITrace _trace;
        private IBlockWatcher _blockWatcher = new NullBlockWatcher();
        private IErrorSubscriber _errorSubscriber;
        private RabbitMQConnectionString _connectionString;
        private bool _receiveSelfPublish;
        private bool _useFastReply = true;
        private string _replyExchange = "";
        private bool _blocked;

        public string BusId
        {
            get { return _busId; }
        }

        public IPublishingErrorHandler ErrorHandler
        {
            get
            {
                return _errorHandler ?? new NullPublishingErrorHandler();
            }
        }
        
        public IErrorSubscriber ErrorSubscriber
        {
            get
            {
                return _errorSubscriber ?? new NullErrorSubscriber();
            }
        }

        public ITrace Trace
        {
            get
            {
                return _trace ?? new NullTrace();
            }
        }
        
        public bool ReceiveSelfPublish
        {
            get
            {
                return _receiveSelfPublish;
            }
        }

        public bool UseFastReply
        {
            get { return _useFastReply; }
        }

        public string ReplyExchange
        {
            get { return _replyExchange; }
        }

        public bool Blocked
        {
            get { return _blocked; }
        }

        public IBusConfigurator SetBusId(string busId)
        {
            _busId = busId;

            return this;
        }

        public RabbitMQConnectionString ConnectionString
        {
            get { return _connectionString ?? new RabbitMQConnectionString(); }
        }

        public IBusConfigurator UseConnectionString(string connectionString)
        {
            _connectionString = new RabbitMQConnectionString(new Uri(connectionString));

            return this;
        }

        public IBusConfigurator UseConfiguredConnectionString(string name)
        {
            string connectionString = ConfigurationManager.ConnectionStrings[name].ConnectionString;

            return UseConnectionString(connectionString);
        }

        public IBusConfigurator UseErrorSubscriber(IErrorSubscriber errorSubscriber)
        {
            _errorSubscriber = errorSubscriber;

            return this;
        }

        public IBusConfigurator UseErrorHandler(IPublishingErrorHandler errorHandler)
        {
            _errorHandler = errorHandler;

            return this;
        }

        public IBusConfigurator UseTrace(ITrace trace)
        {
            _trace = trace;

            return this;
        }

        public IBusConfigurator SetReceiveSelfPublish()
        {
            _receiveSelfPublish = true;

            return this;
        }

        public IBusConfigurator DisableFastReply()
        {
            _useFastReply = false;

            return this;
        }

        public IBusConfigurator SetReplyExchange(string replyExchange)
        {
            _replyExchange = replyExchange;

            return this;
        }

        public IBusConfigurator UseBlockWatcher(IBlockWatcher blockWatcher)
        {
            _blockWatcher = blockWatcher;

            return this;
        }

        public void ConnectionBlocked(string reason)
        {
            _blocked = true;

            _blockWatcher.ConnectionBlocked(reason);
        }

        public void ConnectionUnblocked()
        {
            _blocked = false;

            _blockWatcher.ConnectionUnblocked();
        }
    }
}