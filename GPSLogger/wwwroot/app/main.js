define(function (require) {
    var Logger = require("logger");
    var jquery = require("jquery");

    var eye = jquery("#eye");
    var generateNew = jquery("#generateNew");
    var idInput = jquery("#id");
    var secretInput = jquery("#secret");

    var logger = new Logger({ server: "" });
    var generateCredentials = function () {
        generateNew.attr("disabled", "");
        logger.getCredential(function () {
            idInput.val(logger.credential.id);
            secretInput.val(logger.credential.secret);
            generateNew.attr("disabled", null);
        });
    };
    generateNew.click(generateCredentials);
    generateCredentials();

    var queryLocationButton = jquery("#queryLocation");
    var queryLocationID = jquery("#locationID");
    var locationOutput = jquery("#locationOutput");
    queryLocationButton.click(function () {
        var locationID = queryLocationID.val();
        logger.getLocations(locationID, function (response) {
            console.log(response);
        });
    });

    navigator.geolocation.watchPosition(function (position) {
        console.log(position);
    });

    var forge = require("forge");
    window.forge = forge;
    window.logger = logger;

    eye.change(function () {
        secretInput.attr("type", this.checked ? null : "password");
    });

});