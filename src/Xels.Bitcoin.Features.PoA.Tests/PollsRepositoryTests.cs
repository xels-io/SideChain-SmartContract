using System;
using System.Linq;
using NBitcoin;
using Xels.Bitcoin.Configuration.Logging;
using Xels.Bitcoin.Features.PoA.Voting;
using Xels.Bitcoin.Tests.Common;
using Xels.Bitcoin.Utilities;
using Xunit;

namespace Xels.Bitcoin.Features.PoA.Tests
{
    public class PollsRepositoryTests
    {
        private readonly PollsRepository repository;

        public PollsRepositoryTests()
        {
            string dir = TestBase.CreateTestDir(this);
            Network network = new TestPoANetwork();

            this.repository = new PollsRepository(dir, new ExtendedLoggerFactory(), new DBreezeSerializer(network.Consensus.ConsensusFactory), null, null);
            this.repository.Initialize();
        }

        [Fact]
        public void CantAddOrRemovePollsOutOfOrder()
        {
            Assert.Equal(-1, this.repository.GetHighestPollId());

            this.repository.WithTransaction(transaction =>
            {
                this.repository.AddPolls(transaction, new Poll() { Id = 0 });
                this.repository.AddPolls(transaction, new Poll() { Id = 1 });
                this.repository.AddPolls(transaction, new Poll() { Id = 2 });
                Assert.Throws<ArgumentException>(() => this.repository.AddPolls(transaction, new Poll() { Id = 5 }));
                this.repository.AddPolls(transaction, new Poll() { Id = 3 });

                transaction.Commit();
            });

            Assert.Equal(3, this.repository.GetHighestPollId());

            this.repository.WithTransaction(transaction =>
            {
                this.repository.RemovePolls(transaction, 3);

                Assert.Throws<ArgumentException>(() => this.repository.RemovePolls(transaction, 6));
                Assert.Throws<ArgumentException>(() => this.repository.RemovePolls(transaction, 3));

                this.repository.RemovePolls(transaction, 2);
                this.repository.RemovePolls(transaction, 1);
                this.repository.RemovePolls(transaction, 0);

                transaction.Commit();
            });

            this.repository.Dispose();
        }

        [Fact]
        public void SavesHighestPollId()
        {
            this.repository.WithTransaction(transaction =>
            {
                this.repository.AddPolls(transaction, new Poll() { Id = 0 });
                this.repository.AddPolls(transaction, new Poll() { Id = 1 });
                this.repository.AddPolls(transaction, new Poll() { Id = 2 });

                this.repository.SaveCurrentTip(transaction, new HashHeightPair(0, 0));

                transaction.Commit();
            });

            this.repository.Initialize();

            Assert.Equal(2, this.repository.GetHighestPollId());
        }

        [Fact]
        public void CanLoadPolls()
        {
            this.repository.WithTransaction(transaction =>
            {
                this.repository.AddPolls(transaction, new Poll() { Id = 0 });
                this.repository.AddPolls(transaction, new Poll() { Id = 1 });
                this.repository.AddPolls(transaction, new Poll() { Id = 2 });

                transaction.Commit();
            });

            this.repository.WithTransaction(transaction =>
            {
                Assert.True(this.repository.GetPolls(transaction, 0, 1, 2).Count == 3);
                Assert.True(this.repository.GetAllPolls(transaction).Count == 3);
                Assert.Throws<ArgumentException>(() => this.repository.GetPolls(transaction, -1));
                Assert.Throws<ArgumentException>(() => this.repository.GetPolls(transaction, 9));
            });
        }

        [Fact]
        public void CanUpdatePolls()
        {
            var poll = new Poll() { Id = 0, VotingData = new VotingData() { Key = VoteKey.AddFederationMember } };

            this.repository.WithTransaction(transaction =>
             {
                 this.repository.AddPolls(transaction, poll);

                 poll.VotingData.Key = VoteKey.KickFederationMember;
                 this.repository.UpdatePoll(transaction, poll);

                 transaction.Commit();
             });

            this.repository.WithTransaction(transaction =>
            {
                Assert.Equal(VoteKey.KickFederationMember, this.repository.GetPolls(transaction, poll.Id).First().VotingData.Key);
            });
        }
    }
}
