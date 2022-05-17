using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CallCreditApiDelegation.Helpers
{
    public static class MathHelper
    {
        public static decimal Map(this decimal value, decimal fromSource, decimal toSource, decimal fromTarget, decimal toTarget) => (value - fromSource) / (toSource - fromSource) * (toTarget - fromTarget) + fromTarget;
    }
}