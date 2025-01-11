# ProjectB API Reference

The API methods named as `section.method` format, just like [Telegram API](https://core.telegram.org/methods) or [VK API](https://dev.vk.com/en/method). The server returns responses in JSON format and contains a `response` field in case of success, or an `error` field in case of error.

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

The client must send the `Authorization: Bearer <access_token>` header to any methods except the `auth` section. Without this header, the server will return a error (code `5`). `access_token` can be obtained by calling the `auth.signIn` method.

# API methods
ProjectB has API methods described below. Please note: parameters marked with an asterisk is mandatory. If the client does not send these parameters, the server will return an error (code `10`).

## auth.signIn
This method authorizes the member. 

### Parameters
| Name        | Type    | Description                  | 
|-------------|---------|------------------------------|
| `login` *   | `string`| Member's user name           |
| `password`* | `string`| Password                     |

### Response
An object with fields:
| Name          | Type     | Description                                                                     | 
|---------------|----------|---------------------------------------------------------------------------------|
| `memberId`    | `uint32` | Authorized member's ID                                                          |
| `accessToken` | `string` | An access token that must be sent to API methods in the `Authorization` header  |
| `expiresIn`   | `uint32` | Token lifetime in seconds                                                       |

### Errors
This method may return an error `4`.

## auth.signUp
This method registers a new member in bug tracker via invite code.

### Parameters
| Name            | Type    | Description                                            | 
|-----------------|---------|--------------------------------------------------------|
| `invite_code`*  | `string`| Invite code                                            |
| `password`*     | `string`| Password. It's length must be >= 6                     |
| `first_name`*   | `string`| Member's name. It's length must be >= 2 and <= 50      |
| `last_name`*    | `string`| Member's surname. It's length must be >= 2 and <= 50   |

### Response
An object with fields:
| Name       | Type     | Description                                   | 
|------------|----------|-----------------------------------------------|
| `memberId` | `uint32` | Unique ID for new member                      |
| `login`    | `string` | Member's login, which is also the user name   |

### Errors
This method may return an error `11`.

## invites.create
This method creates an invitation to register new member in bug tracker.

### Parameters
| Name         | Type     | Description                                                         | 
|--------------|----------|---------------------------------------------------------------------|
| `user_name`* | `string` | New member's username and login. It's length must be >= 2 and <= 32 |

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
| `extended` | `byte` | `1` — to return members array                                                        |

### Response
An object with fields:
| Name      | Type        | Description                               | 
|-----------|-------------|-------------------------------------------|
| `count`   | `int32`     | Products count                            |
| `items`   | `Product[]` | An array of [Product](#Product) objects   |
| `members` | `Report[]`  | An array of [Member](#Member) objects     |

## products.setAsFinished
This method completes the product testing.

### Parameters
| Name          | Type     | Description       | 
|---------------|----------|-------------------|
| `product_id`* | `uint32` | ID of the product |

### Response
If authorized member is a owner of the product, the method will return `true`.

### Errors
If authorized member is not a owner of the product, the method will return an error `16`.<br>If the member pass an ID of non-existent product, the server will return an error `11`.

## reports.create
This method creates a new bug report for the product.

### Parameters
| Name            | Type     | Description                                                             | 
|-----------------|----------|-------------------------------------------------------------------------|
| `product_id`*   | `uint32` | ID of the product                                                       |
| `title`*        | `string` | Report title — short description of the bug. It's length must be <= 128 |
| `steps`*        | `string` | Steps to reproduce the bug. It's length must be <= 4096                 |
| `actual`*       | `string` | Actual behavior. It's length must be <= 2048                            |
| `expected`*     | `string` | Expected behavior. It's length must be <= 2048                          |

### Response
An ID of the created report (`uint32`).

### Errors
If authorized member pass an ID of product whose testing has been finished, the server will return an error `20`.

## reports.get
This method return reports.

### Parameters
| Name            | Type     | Description                                                                | 
|-----------------|----------|----------------------------------------------------------------------------|
| `creator_id`    | `uint32` | Return only reports created by `creator_id`.                               |
| `product_id`    | `uint32` | Return only reports created for product `product_id`                       |
| `extended`      | `byte`   | `1` — to return members array and additional (optional) fields in products |

### Response
An object with fields:
| Name      | Type        | Description                               | 
|-----------|-------------|-------------------------------------------|
| `count`   | `int32`     | Reports count                             |
| `items`   | `Report[]`  | An array of [Report](#Report) objects     |
| `members` | `Report[]`  | An array of [Member](#Member) objects     |

# API objects

## Basic

### Member
| Name         | Type     | Description                                           | 
|--------------|----------|-------------------------------------------------------|
| `id`         | `uint32` | Member's unique ID                                    |
| `userName`   | `string` | Member's user name (login)                            |
| `firstName`  | `string` | Member's name                                         |
| `lastName`   | `string` | Member's last name                                    |

### Product
| Name         | Type     | Description                                           | 
|--------------|----------|-------------------------------------------------------|
| `id`         | `uint32` | Product's unique ID                                   |
| `ownerId`    | `uint32` | ID of member who created the product                  |
| `name`       | `string` | Name of the product                                   |
| `isFinished` | `bool`   | Indicates that testing this product has been finished |

### Report
| Name           | Type     | Description                                           | 
|----------------|----------|-------------------------------------------------------|
| `id`           | `uint32` | Report's unique ID                                    |
| `productId`    | `uint32` | ID of the product the report belongs to               |
| `creatorId`    | `uint32` | ID of member who created the report                   |
| `creationTime` | `int64`  | Creation timestamp (unixtime)                         |
| `title`        | `string` | Report title — short description of the bug.          |
| `steps`        | `string` | _(optional)_ Steps to reproduce the bug.              |
| `actual`       | `string` | _(optional)_ Actual behavior.                         |
| `expected`     | `string` | _(optional)_ Expected behavior.                       |

## Errors
| Code  |  Message                                                                                                                                           | 
|-------|----------------------------------------------------------------------------------------------------------------------------------------------------|
| `1`   |  `Internal server error`<br>Returned if an unknown error occurred in the server. The `message` may contain additional information about the error. |
| `2`   |  `Not implemented`<br>Returned if you call an API method that not ready to use.                                                                    |
| `3`   |  `Unknown method passed`<br>Returned if you call an non-existent API method.                                                                       |
| `4`   |  `Login or password is incorrect`                                                                                                                  |
| `5`   |  `Member authorization failed`<br>Returned if the client did not pass the `Authorization` header, or if the access token is invalid.               |
| `10`  |  `One of the parameters specified was missing or invalid`<br>Returned if the client does not send required/mandatory parameters.                   |
| `11`  |  `Not found`<br>Returned if the DB does not contains requested item.                                                                               |
| `12`  |  `Already exist`                                                                                                                                   |
| `15`  |  `Access denied`                                                                                                                                   |
| `16`  |  `Permission to perform this action is denied`                                                                                                     |
| `20`  |  `Testing of this product is over`                                                                                                                 |