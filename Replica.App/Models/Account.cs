using System;
using Replica.Core.Entity;

namespace Replica.App.Models
{
    public class Account : Model
    {
        public long UserId { get; set; }
        public string Controller { get; set; }

        public override string ToString() => $"[{Controller}] {UserId}";

        public override bool Equals(object obj)
        {
            if (obj is Account other)
                return other.Controller == Controller && other.UserId == UserId;
            return false;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Controller, UserId);
        }
    }
}