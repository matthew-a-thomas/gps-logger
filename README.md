# gps-logger

GPS logging website

# Work in progress

See it in action here: https://switchigan.azurewebsites.net/

# Documentation

This is code for a website that lets clients record their physical location, and allows others to query that history.

Everything is being designed with embedded clients in mind.

## Security

The server can verify requests from clients by deriving the client's secret from the ID it provided, then checking an HMAC to see if the message was signed by the client owning that ID.

In a similar way, clients can verify responses from the server by HMAC'ing them using its own secret.

Therefore, clients can only post locations for themselves. The server can tell if a client (or middle man) sends a request with an ID that it doesn't have the secret for.

Timestamps are included in messages so that replay attacks can be limited. This isn't perfect, though: the server currently only checks to make sure the request timestamp is within +/- 1 minute of the server's current time.

MD5 HMAC is used for speed to be nicer to resource-constrained clients. Note that while MD5 hashing has long been broken, there are [no known vulnerabilities to an MD5 HMAC](https://tools.ietf.org/html/rfc6151). If you know of a more secure HMAC that is at least as fast for resource-constrained clients, then please [create an issue](https://github.com/matthew-a-thomas/gps-logger/issues/new).

If you need more security than this, then feel free to run this software behind an SSL certificate.

## Message protocol

Unless otherwise specified, requests and responses follow a standard format, and the parameters listed for for `GET`/`POST` operations are talking about what's in the `Contents` field.

Also note that requests with a `Content-Type` of `text/html` will receive responses in JSON.

### Requests

Requests should have these fields:
- `ClientEpoch` - the client's Unix time (seconds since [epoch](https://en.wikipedia.org/wiki/Unix_time))
- `ClientSalt` - a random hexadecimal string generated by the client
- `Contents` - the request's payload. The contents of this field vary with the context of the request
- `ID` - hexadecimal string from the client's credentials (see [generating new client credentials](#generate-new-client-credentials))
- `HMAC` - the hexadecimal representation of performing an MD5 HMAC on the request using the secret part of the client's credentials. The hashing should be performed on the the other fields in the above order. And for the fields that are hex strings, make sure you hash them as byte arrays instead of as strings

A request is considered invalid if the hex strings aren't really hex strings, if the HMAC isn't right, or if the request seems funny for some other reason.

### Responses

Responses typically have these fields:
 - `ClientSalt` - `null` if there wasn't a valid request. Otherwise this is a hexadecimal string that's a copy of the `ClientSalt` from the request
 - `Contents` - the response payload. The contents of this field vary with the context of the response
 - `ServerEpoch` - the server's Unix time (seconds since [epoch](https://en.wikipedia.org/wiki/Unix_time))
 - `ServerSalt` - a random hexadecimal string generated by the server
 - `HMAC` - `null` if there wasn't a valid request. Otherwise this is the same idea as the [request's](#requests) `HMAC`, and also uses the secret part of the client's credentials (the server is able to derive the client's secret based on the client's ID)

## Initialization

It is highly recommended that you perform the below steps over an SSL connection to your server.

Alternatively, you can publish a file called `hmac key` containing at least 16 bytes into the `App_Data` folder of your build. The contents of that file will be used as the server's HMAC key, and you can then skip this section.

### Determine if HMAC key has already been initialized

`GET /api/hmackey`

**Parameters**: none. You don't even have to provide a [request object](#requests)

**Returns**: `bool`

### Assign HMAC key

`POST /api/hmackey`

**Parameters**:
 - `NewKey` = at least 16 bytes in hexadecimal string format. If you have trouble generating hexadecimal strings, you can just use the `ID` that comes from `GET /api/credential`

**Returns**: nothing

## Generate new client credentials

Make sure you have [initialized the server](#initialization) first.

`GET /api/credential`

Generates some new credentials. The server will be able to verify things you send if you include your `id` and HMAC your message with the `secret`.

Note that eavesdroppers will be able to plainly read the response, so if you want to keep new credentials a secret then make sure you generate new credentials over SSL.

**Parameters**: none. Note that a valid request is not required. Also note that if you do provide a valid request then the response will be HMAC'd with the secret from your credentials, even though this does nothing to hide the returned value from eavesdroppers

**Returns**:
 - `ID` = a random hexadecimal string
 - `Secret` = the result of the server HMAC'ing the `id` with its own HMAC key (that was set during [initialization](#initialization)). Keep this a secret

## Begin logging location

**Incomplete:**
 - Persists only into server's memory, instead of into an object store

`POST /api/location`

**Parameters**:
 - `latitude`
 - `longitude`

**Returns**: `bool`; `true` if the request was valid, `false` otherwise

## Retrieve logged locations

`GET /api/location/{id}`

**Parameters**: a normal [request object](#requests) should not be used; instead, send the below parameter:
 - `id` - the ID of the client you'd like to see location history of

**Returns**
 - List of `Location`s, each containing:
  - `Latitude`
  - `Longitude`
