﻿var Logger;
(function () {
    if (!jQuery)
        throw "Need jQuery";

    

    Logger = function (options) {
        var _this = this;

        this.server = options.server;
        this.credential = { id: null, secret: null };

        var ajax = function (url, callback, data, type) {
            var options = {
                url: _this.server + url,
                data: data,
                success: callback,
                type: type
            };
            jQuery.ajax(options);
        };
        var get = function (url, callback, data) {
            ajax(url, callback, data, "GET");
        };
        var post = function (url, callback, data) {
            ajax(url, callback, data, "POST");
        };

        this.getCredential = function (callback) {
            get("/api/credential", function (json) {
                _this.credential = (json.message || {}).contents || { id: null, secret: null };
                callback(_this.credential);
            });
        }

        this.postLocation = function (location, callback) {

        };
    };
})();