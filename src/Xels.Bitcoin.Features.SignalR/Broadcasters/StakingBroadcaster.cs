using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Xels.Bitcoin.AsyncWork;
using Xels.Bitcoin.Features.Miner.Interfaces;
using Xels.Bitcoin.Features.SignalR.Events;
using Xels.Bitcoin.Utilities;

namespace Xels.Bitcoin.Features.SignalR.Broadcasters
{
    /// <summary>
    /// Broadcasts current staking information to SignalR clients
    /// </summary>
    public class StakingBroadcaster : ClientBroadcasterBase
    {
        private readonly IPosMinting posMinting;

        public StakingBroadcaster(
            ILoggerFactory loggerFactory,
            IPosMinting posMinting,
            INodeLifetime nodeLifetime,
            IAsyncProvider asyncProvider,
            EventsHub eventsHub)
            : base(eventsHub, loggerFactory, nodeLifetime, asyncProvider)
        {
            this.posMinting = posMinting;
        }

        protected override Task<IEnumerable<IClientEvent>> GetMessages(CancellationToken cancellationToken)
        {
            if (null != this.posMinting)
            {
                return Task.FromResult<IEnumerable<IClientEvent>>(new[]
                    {(IClientEvent) new StakingInfoClientEvent(this.posMinting.GetGetStakingInfoModel())});
            }

            return Task.FromResult((IEnumerable<IClientEvent>)new IClientEvent[0]);
        }
    }
}