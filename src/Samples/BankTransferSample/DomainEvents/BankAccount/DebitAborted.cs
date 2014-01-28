﻿using System;
using ENode.Eventing;
using Newtonsoft.Json;

namespace BankTransferSample.DomainEvents.BankAccount
{
    /// <summary>交易转出已终止
    /// </summary>
    [Serializable]
    public class DebitAborted : SourcingEvent<string>
    {
        public Guid TransactionId { get; private set; }
        public double Amount { get; private set; }
        public DateTime AbortedTime { get; private set; }

        public DebitAborted(string accountId, Guid transactionId, double amount, DateTime abortedTime) : base(accountId)
        {
            TransactionId = transactionId;
            Amount = amount;
            AbortedTime = abortedTime;
        }
    }
}