# Contents

* [ProjectB API Documentation](#projectb-api-documentation)
  * [Request syntax](#request-syntax)
  * [Authorization](#authorization)
  * [Response format](#response-format)
* [API Methods](#api-methods)
  * [auth.signIn](#authsignin)
  * [auth.signUp](#authsignup)
  * [invites.create](#invitescreate)
  * [products.create](#productscreate)
  * [products.get](#productsget)
  * [products.setAsFinished](#productssetasfinished)
  * [reports.create](#reportscreate)
  * [reports.get](#reportsget)
* [API objects](#api-objects)
  * [Basic](#basic)
  * [Member](#member)
    * [Product](#product)
    * [Report](#report)
  * [Enums](#enums)
    * [Severity](#severity)
    * [Problem types](#problem-types)
  * [Errors](#errors)

# ProjectB API Documentation

## Request syntax

To call a API method you need to make POST or GET request to the specified URL using HTTPS protocol:
```
http://localhost:7575/**METHOD_NAME**?**PARAMETERS**

```
**METHOD_NAME** — method name from the list of [API functions](#api-methods),
**PARAMETERS** — parameters of the corresponding API method. You can send parameters as query, or in request body as `x-www-form-urlencoded` or `form-data` format.

API methods named as `section.method` format, just like [Telegram API](https://core.telegram.org/methods) or [VK API](https://dev.vk.com/en/method).

Example:
```
http://localhost:7575/auth.signIn?login=myusername&password=mypassword
```

## Authorization

To run all API methods (except from `auth` section) you need to pass an `access_token`, a special access key, in the `Authorization` header:
```
Authorization: Bearer 1HxcvP819mtJdPmRtPOwi0SXEFVnVk9ZRYtVE3fgIyEBofUwIStm8xMTMdd3sTAUk5av7KSUyvCEZHmCwlvcqPbyVPkjsuFuPqXi3EthMKmRFvpe8ut2E5WXksywEiuzVv7l4JG3Rs8M3HPeCQ9sBN
```
Token is a string of digits and latin characters and may refer to a bug tracker's member. If you don't pass the `Authorization` header, the server will return a error (code `5`). You can obtain a token by calling the `auth.signIn` method.

## Response format

The server returns responses in JSON format and contains a `response` field in case of success, or an `error` field in case of error.

Example of a successful response:
```
{
  "response": {
    "memberId": 5,
    "login": "bob"
  }
}
```

Example of an error response:
```
{
  "error": {
    "code": 16,
    "message": "Permission to perform this action is denied: you are not an owner"
  }
}
```

# API methods
ProjectB has API methods described below. Please note: parameters marked with an asterisk is mandatory. If the client does not send these parameters, the server will return an error (code `10`).

## auth.signIn
This method authorizes the member. 

### Parameters
| Name        | Type    | Description                    | 
|-------------|---------|--------------------------------|
| `login` *   | `string`| A member's user name           |
| `password`* | `string`| A password                     |

### Response
An object with fields:
| Name          | Type     | Description                                                                        | 
|---------------|----------|------------------------------------------------------------------------------------|
| `memberId`    | `uint32` | An authorized member's ID                                                          |
| `accessToken` | `string` | An access token that must be sent to API methods in the `Authorization` header     |
| `expiresIn`   | `uint32` | Token lifetime in seconds                                                          |

### Errors
This method may return an error `4`.

## auth.signUp
This method registers a new member in bug tracker via invite code.

### Parameters
| Name            | Type    | Description                                               | 
|-----------------|---------|-----------------------------------------------------------|
| `invite_code`*  | `string`| An invite code                                            |
| `password`*     | `string`| A password. It's length must be >= 6                      |
| `first_name`*   | `string`| A member's name. It's length must be >= 2 and <= 50       |
| `last_name`*    | `string`| A member's surname. It's length must be >= 2 and <= 50    |

### Response
An object with fields:
| Name       | Type     | Description                                      | 
|------------|----------|--------------------------------------------------|
| `memberId` | `uint32` | An unique ID for new member                      |
| `login`    | `string` | A member's login, which is also the user name    |

### Errors
This method may return an error `11`.

## invites.create
This method creates an invitation to register new member in bug tracker.

### Parameters
| Name         | Type     | Description                                                           | 
|--------------|----------|-----------------------------------------------------------------------|
| `user_name`* | `string` | A new member's username and login. It's length must be >= 2 and <= 32 |

### Response
An invite code. (`string`)

### Errors
This method may return an error `12`.

## products.create
This method creates a product.

### Parameters
| Name     | Type     | Description                                              | 
|----------|----------|----------------------------------------------------------|
| `name`*  | `string` | A new product's name. It's length must be >= 2 and <= 64 |

### Response
An ID of created product (`uint32`)

## products.get
This method returns products.

### Parameters
| Name       | Type   | Description                                                                          | 
|------------|--------|--------------------------------------------------------------------------------------|
| `filter`   | `byte` | `1` — returns all products.<br>`2` — returns only unfinished ones.<br>By default `1` |
| `owned`    | `byte` | `1` — returns only those products created by the current authorized member           |
| `extended` | `byte` | `1` — to return mentioned members array                                              |

### Response
An object with fields:
| Name      | Type        | Description                                        | 
|-----------|-------------|----------------------------------------------------|
| `count`   | `int32`     | Products count                                     |
| `items`   | `Product[]` | An array of [Product](#Product) objects            |
| `members` | `Member[]`  | _(optional)_ An array of [Member](#Member) objects |

## products.setAsFinished
This method completes the product testing.

### Parameters
| Name          | Type     | Description          | 
|---------------|----------|----------------------|
| `product_id`* | `uint32` | An ID of the product |

### Response
If authorized member is a owner of the product, the method will return `true`.

### Errors
If authorized member is not a owner of the product, the method will return an error `16`.<br>If the member pass an ID of non-existent product, the server will return an error `11`.

## reports.create
This method creates a new bug report for the product.

### Parameters
| Name            | Type     | Description                                                                | 
|-----------------|----------|----------------------------------------------------------------------------|
| `product_id`*   | `uint32` | An ID of the product                                                       |
| `title`*        | `string` | Report title — short description of the bug. It's length must be <= 128    |
| `steps`*        | `string` | Steps to reproduce the bug. It's length must be <= 4096                    |
| `actual`*       | `string` | Actual behavior. It's length must be <= 2048                               |
| `severity`*     | `byte`   | A bug's [severity](#Severity)                                              |
| `problem_type`* | `byte`   | A bug's [problem type](#problem-types)                                     |

### Response
An ID of the created report (`uint32`).

### Errors
If client pass an ID of product whose testing has been finished, the server will return an error `20`.

## reports.get
This method return reports. Please note: the server will not return vulnerability reports whose creator is not the currently authorized member.

### Parameters
| Name            | Type     | Description                                                                                        | 
|-----------------|----------|----------------------------------------------------------------------------------------------------|
| `creator_id`    | `uint32` | Return only reports created by `creator_id`.                                                       |
| `product_id`    | `uint32` | Return only reports created for product `product_id`                                               |
| `severity`      | `byte`   | Return only reports with a specific [severity](#Severity)                                          |
| `problem_type`  | `byte`   | Return only reports with a specific [problem type](#problem-types)                                 |
| `extended`      | `byte`   | `1` — to return mentioned members and products array, and additional (optional) fields in products |

### Response
An object with fields:
| Name       | Type         | Description                                          | 
|------------|--------------|------------------------------------------------------|
| `count`    | `int32`      | Reports count                                        |
| `items`    | `Report[]`   | An array of [Report](#Report) objects                |
| `members`  | `Member[]`   | _(optional)_ An array of [Member](#Member) objects   |
| `products` | `Product[]`  | _(optional)_ An array of [Product](#Product) objects |

### Errors
If authorized member pass a different `creator_id` ID than his own with `severity = 5` (vulnerability), the server will return an error `15`.

# API objects

## Basic

### Member
| Name         | Type     | Description                | 
|--------------|----------|----------------------------|
| `id`         | `uint32` | Member's unique ID         |
| `userName`   | `string` | Member's user name (login) |
| `firstName`  | `string` | Member's name              |
| `lastName`   | `string` | Member's last name         |

### Product
| Name         | Type     | Description                                           | 
|--------------|----------|-------------------------------------------------------|
| `id`         | `uint32` | Product's unique ID                                   |
| `ownerId`    | `uint32` | ID of member who created the product                  |
| `name`       | `string` | Name of the product                                   |
| `isFinished` | `bool`   | Indicates that testing this product has been finished |

### Report
| Name           | Type     | Description                                    | 
|----------------|----------|------------------------------------------------|
| `id`           | `uint32` | A report's unique ID                           |
| `productId`    | `uint32` | An ID of the product the report belongs to     |
| `creatorId`    | `uint32` | An ID of member who created the report         |
| `creationTime` | `int64`  | Creation timestamp (unixtime)                  |
| `severity`     | `byte`   | A bug's [severity](#Severity)                  |
| `problemType`  | `byte`   | A bug's [problem type](#problem-types)         |
| `title`        | `string` | Report title — short description of the bug.   |
| `steps`        | `string` | _(optional)_ Steps to reproduce the bug.       |
| `actual`       | `string` | _(optional)_ Actual behavior.                  |
| `expected`     | `string` | _(optional)_ Expected behavior.                |

## Enums

### Severity
| Value  | Meaning       | 
|--------|---------------|
| `1`    | low           |
| `2`    | medium        |
| `3`    | high          |
| `4`    | critical      |
| `5`    | vulnerability |

### Problem types
| Value  | Meaning                 | 
|--------|-------------------------|
| `1`    | suggestion              |
| `2`    | app crashed             |
| `3`    | app froze               |
| `4`    | function not working    |
| `5`    | data damage             |
| `6`    | performance             |
| `7`    | aesthetic discrepancies |
| `8`    | typo                    |

## Errors
| Code  |  Message                                                                                                                                                                     | 
|-------|------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|
| `1`   |  `Internal server error`<br>Returned if an unknown error occurred in the server. The `message` may contain additional information about the error.                           |
| `2`   |  `Not implemented`<br>Returned if you call an API method that not ready to use.                                                                                              |
| `3`   |  `Unknown method passed`<br>Returned if you call an non-existent API method.                                                                                                 |
| `4`   |  `Login or password is incorrect`                                                                                                                                            |
| `5`   |  `Member authorization failed`<br>Returned if the client did not pass the `Authorization` header, or if the access token is invalid.                                         |
| `10`  |  `One of the parameters specified was missing or invalid`<br>Returned if the client does not send required/mandatory parameters, or if value of these parameters is invalid. |
| `11`  |  `Not found`<br>Returned if the DB does not contains requested item.                                                                                                         |
| `12`  |  `Already exist`                                                                                                                                                             |
| `15`  |  `Access denied`                                                                                                                                                             |
| `16`  |  `Permission to perform this action is denied`                                                                                                                               |
| `20`  |  `Testing of this product is over`                                                                                                                                           |