# gps-logger

![Build status](https://switchigan.visualstudio.com/_apis/public/build/definitions/b9ab85f0-68c5-423a-ac34-feeb8afba200/5/badge "Build status")

GPS logging website

[Skip down to the good stuff](#generate-new-client-credentials).

Try out the API here:
 - HTTPS: https://switchigan.azurewebsites.net/api/credential
 - Non-HTTPS: http://switchigan.azurewebsites.net/api/credential

# Documentation

This is code for a website that lets clients record their physical location, and allows others to query that history.

Everything is being designed with embedded clients in mind.

## Security

The server can verify requests from clients by deriving the client's secret from the ID it provided, then checking an HMAC to see if the message was signed by the client owning that ID.

In a similar way, clients can verify responses from the server by HMAC'ing them using its own secret.

Therefore, clients can only post locations for themselves. The server can tell if a client (or middle man) sends a request with an ID that it doesn't have the secret for.

Timestamps are included in messages so that replay attacks can be limited. The server checks to make sure the request timestamp is within +/- 1 minute of the server's current time, and that it hasn't seen the given request recently. Note that [issue 30](https://github.com/matthew-a-thomas/gps-logger/issues/30) is open to expand this functionality beyond just the Location controller.

MD5 HMAC is used for speed to be nicer to resource-constrained clients. Note that while MD5 hashing has long been broken, there are [no known vulnerabilities to an MD5 HMAC](https://tools.ietf.org/html/rfc6151). Still, [options will be explored for the upcoming v2.0 release](https://github.com/matthew-a-thomas/gps-logger/issues/48).

## Testing

A variety of tests are included in the solution, ranging from unit tests to integration tests.

Most of the integration tests are automatic and you won't need to provide any magic strings or config values. For example, Reflection is used to determine where to create an `App_Data` folder in relation to the `GPSLogger` assembly. Also, test files are automatically retained for a day and then cleaned up after a subsequent test run.

However, the `SQLDatabase.Tests` do require a magic string: they require a connection string to a dev/test SQL Server database against which to run integration tests. I'm doing my best to only run tests within transactions that are rolled back, but since there's no way to embed SQL Server then you'll need to provide a connection string. That connection string needs to live within a file on your hard drive called `C:\connection string.txt`. If you have other ideas for how I can keep connection strings secret (AKA separate from this GitHub project), then please [comment on this thread](https://github.com/matthew-a-thomas/gps-logger/issues/49).

## Design choices

 - Autofac is used for Dependency Injection
 - Resource-constrained clients
 - Authenticated logging; public reading
 - Abstracted data access layer (currently SQL Server, but will be easy to replace that with a document store)

## Message protocol

Unless otherwise specified, requests and responses follow a standard format, and the parameters listed for for `GET`/`POST` operations are talking about what's in the `Contents` field.

Also note that requests with a `Content-Type` of `text/html` will receive responses in JSON.

### Requests

Requests should have these fields:
 - `Contents` - the request's payload. The contents of this field vary with the context of the request
 - `HMAC` - the hexadecimal representation of performing an MD5 HMAC on the request using the secret part of the client's credentials. The hashing should be performed on the the other fields in the above order. And for the fields that are hex strings, make sure you hash them as byte arrays instead of as strings
 - `ID` - hexadecimal string from the client's credentials (see [generating new client credentials](#generate-new-client-credentials))
 - `Salt` - a random hexadecimal string generated by the client
 - `UnixTime` - the client's Unix time (seconds since [epoch](https://en.wikipedia.org/wiki/Unix_time))

A request is considered invalid if the hex strings aren't really hex strings, if the HMAC isn't right, or if the request seems funny for some other reason.

### Responses

Responses typically have these fields:
 - `Contents` - the response payload. The contents of this field vary with the context of the response
 - `HMAC` - `null` if there wasn't a valid request. Otherwise this is the same idea as the [request's](#requests) `HMAC`, and also uses the secret part of the client's credentials (the server is able to derive the client's secret based on the client's ID)
 - `ID` - `null` if there wasn't a valid request. Otherwise this is a hexadecimal string that's a copy of the `ID` from the request
 - `Salt` - `null` if there wasn't a valid request. Otherwise this is a hexadecimal string that's a copy of the `Salt` from the request
 - `UnixTime` - the server's Unix time (seconds since [epoch](https://en.wikipedia.org/wiki/Unix_time))

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

**Example** - (C#)
```c-sharp
HttpClient client; // Assuming you've got an HttpClient already instantiated
var request = new
{
  Contents = new
  {
    Latitude = /* Your latitude here */,
    Longitude = /* Your longitude here */
  },
  ID = "your ID here",
  Salt = "a random hex string here",
  UnixTime = DateTimeOffset.Now.ToUnixTimeSeconds(),
  HMAC = ""
};
request.HMAC = /* Calculate the MD5 HMAC of "request". The HMAC key will be the secret that corresponds to your ID */;
client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
var json = JsonConvert.SerializeObject(request);
var encodedContent = new StringContent(json, Encoding.UTF8, "application/json");
var response = await client.PostAsync("/api/location", encodedContent);
response.EnsureSuccessStatusCode();
var responseString = await response.Content.ReadAsStringAsync();
var deserializedResponse = JsonConvert.DeserializeObject<SignedMessage<bool>>(responseString);
```

## Retrieve logged locations

`GET /api/location/{id}`

**Parameters**: a normal [request object](#requests) should not be used; instead, send the below parameter:
 - `id` - the ID of the client you'd like to see location history of

**Returns**
 - List of `Location`s, each containing:
  - `Latitude`
  - `Longitude`
