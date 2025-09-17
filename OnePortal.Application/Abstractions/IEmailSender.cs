﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnePortal.Application.Abstractions
{
    public interface IEmailSender
    {
        Task SendAsync(string to, string subject, string htmlBody, CancellationToken ct = default);
    }
}
