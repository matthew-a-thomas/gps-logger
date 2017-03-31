using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Common.Extensions;
using Common.Messages;
using Common.Security.Signing;
using GPSLogger.Models;
using Microsoft.AspNetCore.Mvc;

namespace GPSLogger.Controllers
{
    /// <summary>
    /// Handles saving and storing Locations
    /// </summary>
    [Route("api/[controller]")]
    public class LocationController : ControllerBase
    {
        private readonly LocationProviderAsync _locationProviderAsync;
        private readonly HandleLocationPostAsync _handleLocationPostAsync;
        private readonly IMessageHandler<Location, bool> _messageHandler;

        /// <summary>
        /// Delegate for storing a new location. The ID has already been validated
        /// </summary>
        /// <param name="id"></param>
        /// <param name="location"></param>
        public delegate Task HandleLocationPostAsync(byte[] id, Location location);

        /// <summary>
        /// Delegate for reading locations for the given ID
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public delegate Task<IEnumerable<Location>> LocationProviderAsync(byte[] id);
        
        public LocationController(
            LocationProviderAsync locationProviderAsync,
            HandleLocationPostAsync handleLocationPostAsync,
            IMessageHandler<Location, bool> messageHandler)
        {
            _locationProviderAsync = locationProviderAsync;
            _handleLocationPostAsync = handleLocationPostAsync;
            _messageHandler = messageHandler;
        }
        
        /// <summary>
        /// Gets all the locations for the given ID
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<IEnumerable<Location>> GetAsync(string id = "") => string.IsNullOrWhiteSpace(id) ? Enumerable.Empty<Location>() : await _locationProviderAsync(await ByteArrayExtensions.FromHexStringAsync(id));

        /// <summary>
        /// Posts a new location
        /// </summary>
        /// <param name="posted"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<SignedMessage<bool>> PostAsync([FromBody] SignedMessage<Location> posted) => await _messageHandler.CreateResponseAsync(
            posted,
            async valid =>
            {
                if (valid)
                    await _handleLocationPostAsync(await ByteArrayExtensions.FromHexStringAsync(posted.Message.ID), posted.Message.Contents);

                return valid;
            });
    }
}
