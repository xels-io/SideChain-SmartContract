using Xels.Bitcoin.Features.SmartContracts.Interfaces;
using Xels.SmartContracts.Core.Hashing;

namespace Xels.Bitcoin.Features.SmartContracts
{
    /// <summary>
    /// Hashes data using Keccak256.
    /// </summary>
    public class Keccak256CodeHashingStrategy : IContractCodeHashingStrategy
    {
        /// <summary>
        /// Hashes the supplied byte array using Keccak256.
        /// </summary>
        /// <param name="data">The data to hash.</param>
        /// <returns>The Keccak256 hash of the data.</returns>
        public byte[] Hash(byte[] data)
        {
            return HashHelper.Keccak256(data);
        }
    }
}