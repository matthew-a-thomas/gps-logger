using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Common.Extensions;
using Common.Messages;
using Common.Security.Signing;
using GPSLogger.Interfaces;

namespace GPSLogger.Implementations
{
    public class LocationImpl : ILocation
    {
        /// <summary>
        /// Delegate for storing a new location. The ID has already been validated
        /// </summary>
        /// <param name="id"></param>
        /// <param name="location"></param>
        public delegate Task HandleLocationPostAsync(byte[] id, Models.Location location);

        /// <summary>
        /// Delegate for reading locations for the given ID
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public delegate Task<IEnumerable<Common.RemoteStorage.Models.Location>> LocationProviderAsync(byte[] id);

        private readonly LocationProviderAsync _locationProviderAsync;
        private readonly HandleLocationPostAsync _handleLocationPostAsync;
        private readonly IMessageHandler<Models.Location, bool> _messageHandler;

        public LocationImpl(
            LocationProviderAsync locationProviderAsync,
            HandleLocationPostAsync handleLocationPostAsync,
            IMessageHandler<Models.Location, bool> messageHandler)
        {
            _locationProviderAsync = locationProviderAsync;
            _handleLocationPostAsync = handleLocationPostAsync;
            _messageHandler = messageHandler;
        }

        public async Task<SignedMessage<bool>> AddLocationAsync(SignedMessage<Models.Location> posted) => await _messageHandler.CreateResponseAsync(
            posted,
            async valid =>
            {
                if (valid)
                    await _handleLocationPostAsync(await ByteArrayExtensions.FromHexStringAsync(posted.Message.ID), posted.Message.Contents);

                return valid;
            });

        public async Task<IEnumerable<Common.RemoteStorage.Models.Location>> GetLocationsForAsync(string id = "") => string.IsNullOrWhiteSpace(id) ? Enumerable.Empty<Common.RemoteStorage.Models.Location>() : await _locationProviderAsync(await ByteArrayExtensions.FromHexStringAsync(id));
    }
}
