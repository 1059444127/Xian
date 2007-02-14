using System;
using System.Collections.Generic;
using System.Text;

using Spring.Aop;
using AopAlliance.Intercept;

namespace ClearCanvas.Enterprise
{
    public class ReadContextAdvice : PersistenceContextAdvice
    {
        internal ReadContextAdvice()
        {
        }

        public override object Invoke(IMethodInvocation invocation)
        {
            ServiceLayer serviceLayer = (ServiceLayer)invocation.This;
            try
            {
                ServiceOperationAttribute a = GetServiceOperationAttribute(invocation.Method);
                using (new PersistenceScope(PersistenceContextType.Read, a.PersistenceScopeOption))
                {
                    // set the read context as the current context of the service layer
                    serviceLayer.CurrentContext = PersistenceScope.Current;
                    return invocation.Proceed();
                }
            }
            finally
            {
                // be sure to remove the current context from the service layer
                serviceLayer.CurrentContext = null;
            }
        }
    }
}
