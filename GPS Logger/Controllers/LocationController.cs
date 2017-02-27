using System.Collections.Generic;
using System.Web.Http;
using GPS_Logger.Models;
using GPS_Logger.Extensions;
using GPS_Logger.Models.Messages;
using GPS_Logger.Security.Signing;

namespace GPS_Logger.Controllers
{
    /// <summary>
    /// Handles saving and storing Locations
    /// </summary>
    public class LocationController : ApiController
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
        /// Gives a helpful message when an ID isn't provided
        /// </summary>
        /// <returns></returns>
        public string Get() => "Please specify an ID in order to retrieve the location history of that ID";

        /// <summary>
        /// Gets all the locations for the given ID
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public IEnumerable<Location> Get(string id) => _locationProvider(ByteArrayExtensions.FromHexString(id));

        /// <summary>
        /// Posts a new location
        /// </summary>
        /// <param name="posted"></param>
        /// <returns></returns>
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
