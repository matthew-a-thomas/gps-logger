# gps-logger
GPS logging website

# Work in progress
See it in action here: https://switchigan.azurewebsites.net/

# Documentation

## Initialization

### Determine if HMAC key has already been initialized
`GET /api/hmackey`

### Assign HMAC key
`POST /api/hmackey`
 - `newkey` = at least 16 bytes in hexadecimal string format

If you have trouble generating hexadecimal strings, you can just use the `ID` that comes from `GET /api/credential`

## Generate new client credentials
Make sure you have [initialized the server](#initialization) first.

`GET /api/credential`
 - `id` = a random hexadecimal string
 - `secret` = the result of the server HMAC'ing the `id` with its HMAC key (that was set during initialization). Keep this secret

Generates some new credentials. The server will be able to verify things you send if you include your `id` and HMAC your message with the `secret`.

## Begin logging location
**Incomplete:**
 - Requires sending client's secret, which instead needs to remain a secret
 - Persists only into server's memory, instead of into an object store

`POST /api/location`
 - `id` - the client's ID (from [its credentials](#generate-new-client-credentials))
 - `secret` - the client's secret(from its credentials)
 - `latitude`
 - `longitude`

## Retrieve logged locations
`GET /api/location/{id}`
 - `id` - the ID of the client you'd like to see location history of
