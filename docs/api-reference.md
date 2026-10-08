# Claims Handling HTTP API reference

Base URL for the default local development profile: `http://localhost:5180`.

Swagger UI is available at `/swagger`. The API accepts and returns JSON, and serializes enum values as strings. Dates use ISO 8601 calendar format (`YYYY-MM-DD`). Claim and cover routes use the integer `displayId`; resource responses also include the stable GUID `id`.

## Common errors

| Status | Meaning | Response |
| --- | --- | --- |
| `400 Bad Request` | Request binding or FluentValidation/domain rule failure. | Problem Details. FluentValidation errors use a `ValidationProblemDetails` `errors` object grouped by field. |
| `404 Not Found` | A requested claim or cover does not exist. | Empty response for delete; framework/controller not-found response for read. |
| `500 Internal Server Error` | Unexpected server-side failure. | Problem Details with a `traceId`; details are logged server-side. |

Example validation response:

```json
{
  "title": "One or more validation errors occurred.",
  "status": 400,
  "errors": {
    "DamageCost": ["The specified condition was not met for 'Damage Cost'."]
  },
  "traceId": "00-..."
}
```

## Claims

### `GET /Claims`

Returns every claim.

**Success:** `200 OK`

```json
[
  {
    "id": "d40b25ac-6405-4cce-b255-29272fbe5b40",
    "displayId": 12,
    "coverId": "156235c4-83d0-493c-97c2-6d07317b0154",
    "created": "2026-10-08",
    "name": "Hull damage",
    "type": "Collision",
    "damageCost": 12500.00
  }
]
```

### `GET /Claims/{displayId}`

Returns one claim, found by its integer display ID.

| Parameter | Type | Required | Description |
| --- | --- | --- | --- |
| `displayId` | integer | Yes | Claim's human-facing integer ID; must be positive. |

**Success:** `200 OK` with a claim object. **Invalid ID:** `400 Bad Request` when `displayId` is not positive. **Not found:** `404 Not Found`.

### `POST /Claims`

Creates a claim. `coverId` is the cover's **GUID `id`**, not its integer `displayId`.

**Request body:**

```json
{
  "coverId": "156235c4-83d0-493c-97c2-6d07317b0154",
  "created": "2026-10-08",
  "name": "Hull damage",
  "type": "Collision",
  "damageCost": 12500.00
}
```

`type` values: `Collision`, `Grounding`, `BadWeather`, `Fire`.

**Rules:** Name is required (maximum 200 characters); damage cost is between 0 and 100,000 inclusive; cover must exist; claim date must be within the related cover period, including both boundary dates.

**Success:** `201 Created`, a claim object, and a `Location` header pointing to `GET /Claims/{displayId}`. **Validation failure:** `400 Bad Request`.

### `DELETE /Claims/{displayId}`

Deletes one claim by integer display ID.

**Success:** `204 No Content`. **Invalid ID:** `400 Bad Request` when `displayId` is not positive. **Not found:** `404 Not Found`.

## Covers

### `GET /Covers`

Returns every cover.

**Success:** `200 OK`

```json
[
  {
    "id": "156235c4-83d0-493c-97c2-6d07317b0154",
    "displayId": 8,
    "startDate": "2026-10-08",
    "endDate": "2027-02-08",
    "type": "Yacht",
    "premium": 162731.25
  }
]
```

### `GET /Covers/{displayId}`

Returns one cover by integer display ID.

| Parameter | Type | Required | Description |
| --- | --- | --- | --- |
| `displayId` | integer | Yes | Cover's human-facing integer ID; must be positive. |

**Success:** `200 OK` with a cover object. **Invalid ID:** `400 Bad Request` when `displayId` is not positive. **Not found:** `404 Not Found`.

### `POST /Covers`

Creates a cover and calculates its premium.

**Request body:**

```json
{
  "startDate": "2026-10-08",
  "endDate": "2027-02-08",
  "type": "Yacht"
}
```

`type` values: `Yacht`, `PassengerShip`, `ContainerShip`, `BulkCarrier`, `Tanker`.

**Rules:** Start date must be today or later; end date must be after start date; the period must be no longer than one year.

**Success:** `201 Created`, a cover object with calculated `premium`, and a `Location` header pointing to `GET /Covers/{displayId}`. **Validation failure:** `400 Bad Request`.

### `DELETE /Covers/{displayId}`

Deletes one cover by integer display ID.

**Success:** `204 No Content`. **Not found:** `404 Not Found`. **Cannot delete:** `400 Bad Request` if any claim references this cover.

### `POST /Covers/compute`

Calculates a premium without creating a cover. Values are supplied as query parameters.

| Parameter | Type | Required | Example |
| --- | --- | --- | --- |
| `startDate` | date | Yes | `2026-10-08` |
| `endDate` | date | Yes | `2027-02-08` |
| `coverType` | enum string | Yes | `Yacht` |

Example: `POST /Covers/compute?startDate=2026-10-08&endDate=2027-02-08&coverType=Yacht`

`endDate` cannot precede `startDate`; equal dates produce a zero-day premium. This calculation endpoint does not create a cover and does not apply the create-cover one-year limit.

**Success:** `200 OK` with a JSON decimal value, for example `162731.25` for a 123-day Yacht period. **Validation failure:** `400 Bad Request`.

## Premium calculation details

The base rate is 1,250 per elapsed day. Type multipliers are Yacht `1.10`, Passenger ship `1.20`, Tanker `1.50`, and other cover types `1.30`. Premium days are split into three bands:

| Elapsed day band | Yacht discount | Other type discount |
| --- | ---: | ---: |
| Days 1–30 | 0% | 0% |
| Days 31–180 | 5% | 2% |
| Day 181 onward | 8% total | 3% total |

The final band's discount is cumulative against the type-adjusted base rate (5% + an additional 3% for Yacht; 2% + an additional 1% for other types). The period uses the elapsed days between start and end; it does not count both endpoints as full days.

## Manual test collection

Import [`ClaimsApi.postman_collection.json`](../postman/ClaimsApi.postman_collection.json) into Postman. Its default `baseUrl` is `http://localhost:5180`.
