define(function (require) {
    const Logger = require("logger");
    const jquery = require("jquery");

    const eye = jquery("#eye");
    var generateNew = jquery("#generateNew");
    var idInput = jquery("#id");
    var secretInput = jquery("#secret");

    var logger = new Logger({ server: "" });
    const generateCredentials = function (callback) {
        generateNew.attr("disabled", "");
        logger.getCredential(function () {
            idInput.val(logger.credential.id);
            secretInput.val(logger.credential.secret);
            generateNew.attr("disabled", null);
            if (callback)
                callback();
        });
    };
    generateNew.click(function() { generateCredentials(); });

    const queryLocationButton = jquery("#queryLocation");
    // ReSharper disable once InconsistentNaming
    var queryLocationID = jquery("#locationID");
    const locationOutput = jquery("#locationOutput");
    queryLocationButton.click(function () {
        // ReSharper disable once InconsistentNaming
        const locationID = queryLocationID.val();
        logger.getLocations(locationID, function (response) {
            locationOutput.empty();
            for (const location of response) {
                const row = jquery("<div>");
                row.text(`${location.latitude}, ${location.longitude}, ${location.unixTime}`);
                locationOutput.append(row);
            }
        });
    });

    generateCredentials(function() {
        navigator.geolocation.watchPosition(function(position) {
            const coordinates = position.coords;
            const location = {
                latitude: coordinates.latitude,
                longitude: coordinates.longitude
            };
            logger.postLocation(
                location,
                function() {
                    const row = jquery("<div>");
                    row.text(`${location.latitude}, ${location.longitude}`);
                    locationOutput.prepend(row);
                });
        });
    });

    const forge = require("forge");
    window.forge = forge;
    window.logger = logger;

    eye.change(function () {
        secretInput.attr("type", this.checked ? null : "password");
    });

});