﻿using Application.Common.Interfaces;

namespace Infrastructure.Services;
public class DateTimeService : IDateTime
{
    public DateTime Now => DateTime.UtcNow.AddHours(1);
}