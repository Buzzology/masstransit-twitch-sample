﻿using MassTransit;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sample.Contracts
{
    public interface OrderSubmitted
    {
        Guid OrderId { get; }
        DateTime Timestamp { get; }
        string CustomerNumber { get; }
        string PaymentCardNumber { get; }
        MessageData<string> Notes { get; }
    }
}
