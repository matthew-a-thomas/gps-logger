using System.Collections.Generic;
using System.Linq;
using Common.Extensions;
using Common.Messages;
using Common.Security.Signing;
using GPS_Logger.Models;
using Microsoft.AspNetCore.Mvc;

namespace GPS_Logger.Controllers
{
    /// <summary>
    /// Handles saving and storing Locations
    /// </summary>
    [Route("api/[controller]")]
    public class LocationController : ControllerBase
    {
        private readonly LocationProvider _locationProvider;
        private readonly HandleLocationPost _handleLocationPost;
        private readonly MessageHandler<Location, bool> _messageHandler;

        /// <summary>
        /// Delegate for storing a new location. The ID has already been validated
        /// </summary>
        /// <param name="id"></param>
        /// <param name="location"></param>
        public delegate void HandleLocationPost(byte[] id, Location location);

        /// <summary>
        /// Delegate for reading locations for the given ID
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public delegate IEnumerable<Location> LocationProvider(byte[] id);
        
        public LocationController(
            LocationProvider locationProvider,
            HandleLocationPost handleLocationPost,
            MessageHandler<Location, bool> messageHandler)
        {
            _locationProvider = locationProvider;
            _handleLocationPost = handleLocationPost;
            _messageHandler = messageHandler;
        }
        
        /// <summary>
        /// Gets all the locations for the given ID
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet]
        public IEnumerable<Location> Get(string id = "") => string.IsNullOrWhiteSpace(id) ? Enumerable.Empty<Location>() : _locationProvider(ByteArrayExtensions.FromHexString(id));

        /// <summary>
        /// Posts a new location
        /// </summary>
        /// <param name="posted"></param>
        /// <returns></returns>
        [HttpPost]
        public SignedMessage<bool> Post(SignedMessage<Location> posted) => _messageHandler.CreateResponse(
            posted,
            valid =>
            {
                if (valid)
                    _handleLocationPost(ByteArrayExtensions.FromHexString(posted.ID), posted.Contents);

                return valid;
            });
    }
}
