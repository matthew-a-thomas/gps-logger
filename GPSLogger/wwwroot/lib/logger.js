define(function (require) {
    var algo = "SHA256";
    var blockSize = 32;

    var forge = require("forge");
    var jquery = require("jquery");

    var util = {
        /**
         * Converts the given byte array to a hex string
         * @param {array} x 
         * @returns {string} 
         */
        byteArrayToHex: function (x) {
            return x
                .map(function (y) {
                    return (`0${y.toString(16)}`).slice(-2);
                })
                .join("");
        },
        /**
         * Returns the byte array equivalent of the given double-precision floating point number, in Little-Endian format
         * @param {} x 
         * @returns {} 
         */
        serializeFloat64: function (x) {
            const buffer = new ArrayBuffer(8);
            const doubles = new Float64Array(buffer);
            doubles[0] = x;
            const bytes = new Uint8Array(buffer);
            const result = [];
            for (let byte of bytes) {
                result.push(byte);
            }
            return result;
        }
    };

    // ReSharper disable once InconsistentNaming
    /**
     * Creates a new HMAC using Forge
     * @param {any} secretHex
     * @returns {hmac}
     */
    var createHMAC = function (secretHex) {
        const key = forge.util.hexToBytes(secretHex || "");
        const hmac = forge.hmac.create();
        hmac.start(algo, key);
        return hmac;
    };

    /**
     * Returns a random hex string
     * @returns {string}
     */
    var createSalt = function () {
        const bytes = forge.random.getBytesSync(blockSize);
        return forge.util.bytesToHex(bytes);
    };

    /**
     * Returns the number of seconds since Jan 1, 1970
     * @returns {int}
     */
    var getUnixTime = function () {
        return Math.floor(Date.now() / 1000);
    };

    /**
     * Populates .salt and .unixTime properties with the relevant data
     * @param {any} message
     */
    var saltAndTimestamp = function (message) {
        message.salt = createSalt();
        message.unixTime = getUnixTime();
    };

    /**
     * Serializes the given message. Note that these fields need to be in hex: contents, id, salt. This field needs to be an integer: unixTime
     * @param {any} message
     * @returns {string} A raw representation of the serialized bytes
     */
    var serialize = function (message) {
        const buffer = forge.util.createBuffer();
        // .contents must already be serialized into hex
        buffer.putBytes(forge.util.hexToBytes(message.contents));
        buffer.putBytes(forge.util.hexToBytes(message.id || ""));
        buffer.putBytes(forge.util.hexToBytes(message.salt));
        // Treat .unixTime as a Little-Endian 64-bit signed integer
        buffer.putInt32Le(message.unixTime);
        buffer.putInt32Le(0);
        return buffer.getBytes();
    };
    
    //const falseByte = "00";
    var trueByte = "01";

    return function (options) {
        // ReSharper disable once InconsistentNaming
        var _this = this;
        
        this.server = options.server;
        this.credential = { id: null, secret: null };

        var ajax = function (url, callback, data, type) {
            // ReSharper disable once DeclarationHides
            const options = {
                contentType: "application/json",
                url: _this.server + url,
                data: JSON.stringify(data),
                error: function (xhr, status, error) {
                    console.error(`${status} - ${error}`);
                },
                success: callback,
                type: type
            };
            jquery.ajax(options);
        };
        var get = function (url, callback, data) {
            ajax(url, callback, data, "GET");
        };
        var post = function (url, callback, data) {
            ajax(url, callback, data, "POST");
        };

        /**
         * Signs the given message by serializing it, then populating an "hmac" field with a signature derived from this Logger's credentials
         * @param {any} message
         */
        const getSignature = function (message) {
            const serialized = serialize(message);
            const secret = (_this.credential || {}).secret;
            const hmac = createHMAC(secret);
            hmac.update(serialized);
            const hashed = hmac.digest().toHex();
            return hashed;
        };

        /**
         * Retrieves a new set of credentials from the server and stores them inside this Logger
         * @param {} callback A callback method that is invoked with the new credential when it's available
         */
        this.getCredential = function (callback) {
            // Create a request for new credentials from the server
            var parameters = "";
            if (this.credential && this.credential.secret && this.credential.id) {
                // We have credentials already, so let's create a signed request

                // Start with a basic request object
                const request = {
                    contents: trueByte, // Note the contents have to be in hex for signing to work
                    id: this.credential.id
                };
                // Decorate it with a salt and the current time
                saltAndTimestamp(request);
                // Sign it using our credentials
                const signature = getSignature(request);
                request.hmac = signature;
                request.contents = "true"; // We needed the contents to be in hex for signing to work, but the server is expecting a string that can be parsed into a boolean

                // Turn the request object into query parameters (thanks, jQuery!)
                parameters = jquery.param(request);
            }

            // Fire off the request
            get(`/api/credential?${parameters}`, function (json) {
                // Pull out the new credential and store it
                _this.credential = (json.message || {}).contents || { id: null, secret: null };

                // Invoke the callback, giving it our new credentials
                callback(_this.credential);
            });
        }

        /**
         * Retrieves all posted locations for the given ID
         * @param {hex} forID 
         * @param {function} callback 
         * @returns {array} 
         */
        // ReSharper disable once InconsistentNaming
        this.getLocations = function (forID, callback) {
            get(`/api/location?id=${forID}`, callback);
        };

        this.postLocation = function (location, callback) {
            if (!(this.credential && this.credential.secret && this.credential.id))
                throw "Cannot post locations until this Logger's credentials are set";
            
            const contents = [location.latitude, location.longitude]
                .map(function (x) { return util.serializeFloat64(x); })
                .map(function (x) { return util.byteArrayToHex(x); })
                .join("");
            const message = {
                contents: contents,
                id: this.credential.id
            };
            saltAndTimestamp(message);
            const signature = getSignature(message);
            message.contents = location;
            const request = {
                hmac: signature,
                message: message
            };
            
            post(
                "/api/location",
                function (result) {
                    // "{"hmac":null,"message":{"contents":false,"id":null,"salt":null,"unixTime":1491595739}}"
                    if (!(result && result.message && result.message.contents))
                        throw `Failed to post location. Here's the server's response: ${JSON.stringify(result)}`;
                    callback(result);
                },
                request
            );
        };

        this.util = util;
    };
});