using CommonFunctionality.KeyVaultProvider;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventStore
{
    public class EventStore : IEventStore
    {
        private readonly IKeyVaultProvider _keyVaultProvider = null;

        public EventStore(IKeyVaultProvider keyVaultProvider)
        {
            _keyVaultProvider = keyVaultProvider;
        }
    }
}
