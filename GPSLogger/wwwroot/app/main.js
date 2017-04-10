define(function (require) {
    const Logger = require("logger");
    const jquery = require("jquery");

    const eye = jquery("#eye");
    var generateNew = jquery("#generateNew");
    var idInput = jquery("#id");
    var secretInput = jquery("#secret");

    var logger = new Logger({ server: "" });
    const generateCredentials = function () {
        generateNew.attr("disabled", "");
        logger.getCredential(function () {
            idInput.val(logger.credential.id);
            secretInput.val(logger.credential.secret);
            generateNew.attr("disabled", null);
        });
    };
    generateNew.click(generateCredentials);
    generateCredentials();

    const queryLocationButton = jquery("#queryLocation");
    // ReSharper disable once InconsistentNaming
    var queryLocationID = jquery("#locationID");
    //const locationOutput = jquery("#locationOutput");
    queryLocationButton.click(function () {
        // ReSharper disable once InconsistentNaming
        const locationID = queryLocationID.val();
        logger.getLocations(locationID, function (response) {
            console.log(response);
        });
    });

    navigator.geolocation.watchPosition(function (position) {
        console.log(position);
    });

    const forge = require("forge");
    window.forge = forge;
    window.logger = logger;

    eye.change(function () {
        secretInput.attr("type", this.checked ? null : "password");
    });

});