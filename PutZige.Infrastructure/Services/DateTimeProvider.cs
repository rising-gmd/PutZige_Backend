using PutZige.Application.Interfaces;
using System;

namespace PutZige.Infrastructure.Services
{
    /// <summary>
    /// Production implementation returning real system time.
    /// </summary>
    public class DateTimeProvider : IDateTimeProvider
    {
        public DateTime UtcNow => DateTime.UtcNow;
    }
}
