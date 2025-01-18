# Contents

* [ProjectB API Documentation](#projectb-api-documentation)
  * [Request syntax](#request-syntax)
  * [Authorization](#authorization)
  * [Response format](#response-format)
* [API Methods](#api-methods)
  * [auth.signIn](#authsignin)
  * [auth.signUp](#authsignup)
  * [invites.create](#invitescreate)
  * [members.getCard](#membersgetcard)
  * [products.create](#productscreate)
  * [products.get](#productsget)
  * [products.getCard](#productsgetcard)
  * [products.setAsFinished](#productssetasfinished)
  * [reports.changeSeverity](#reportschangeseverity)
  * [reports.changeStatus](#reportschangestatus)
  * [reports.create](#reportscreate)
  * [reports.createComment](#reportscreatecomment)
  * [reports.delete](#reportsdelete)
  * [reports.edit](#reportsedit)
  * [reports.editComment](#reportseditcomment)
  * [reports.get](#reportsget)
  * [reports.getById](#reportsgetbyid)
  * [reports.getComments](#reportsgetcomments)
  * [server.getEnumStrings](#servergetenumstrings)
  * [server.init](#serverinit)
* [API objects](#api-objects)
  * [Basic](#basic)
    * [EnumInfo](#enuminfo)
    * [Member](#member)
    * [Invite](#invite)
    * [Product](#product)
    * [Report](#report)
    * [Comment](#comment)
  * [Enums](#enums)
    * [Severity](#severity)
    * [Problem types](#problem-types)
    * [Report statuses](#report-statuses)
  * [Errors](#errors)

# ProjectB API Documentation

## Request syntax

To call an API method you need to make a POST or GET request to the specified URL:

> **HOST**`/`**METHOD_NAME**`?`**PARAMETERS**

**HOST** — the address of the server where ProjectB is running. By default, if running locally, `http://localhost:7575`

**METHOD_NAME** — method name from the list of [API functions](#api-methods).

**PARAMETERS** — parameters of the corresponding API method. You can send parameters as URL query, or in request body as `x-www-form-urlencoded` or `form-data` format.

API methods named in `section.method` format, just like [Telegram API](https://core.telegram.org/methods) or [VK API](https://dev.vk.com/en/method).

Example of the request:
```
http://localhost:7575/auth.signIn?login=myusername&password=mypassword
```

## Authorization

To run all API methods (except from `auth` section) you need to pass an `access_token`, which is a special access key, in the `Authorization` header:
```
Authorization: Bearer 1HxcvP819mtJdPmRtPOwi0SXEFVnVk9ZRYtVE3fgIyEBofUwIStm8xMTMdd3sTAUk5av7KSUyvCEZHmCwlvcqPbyVPkjsuFuPqXi3EthMKmRFvpe8ut2E5WXksywEiuzVv7l4JG3Rs8M3HPeCQ9sBN
```
Token is a string of digits and latin characters and may refer to the bug tracker's member. If you don't pass the `Authorization` header, the server will return an error (code `5`). You can obtain a token by calling the `auth.signIn` method.

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
**ProjectB** has API methods described below.

> [!NOTE]
> Parameters marked with an asterisk are mandatory. If the client does not send these parameters, the server will return an error (code `10`).

## auth.signIn
This method authorizes the member. 

### Parameters
| Name        | Type     | Description                  | 
|-------------|----------|------------------------------|
| `login` *   |  string  | Member's user name           |
| `password`* |  string  | Password                     |

### Response
An object with fields:
| Name          | Type     | Description                                                                        | 
|---------------|----------|------------------------------------------------------------------------------------|
| `memberId`    |  uint32  | Authorized member's ID                                                             |
| `accessToken` |  string  | An access token that must be sent to API methods in the `Authorization` header     |
| `expiresIn`   |  uint32  | Token lifetime in seconds                                                          |

### Errors
This method may return an error `4`.

## auth.signUp
This method registers a new member in bug tracker via invite code.

### Parameters
| Name            | Type     | Description                                            | 
|-----------------|----------|--------------------------------------------------------|
| `invite_code`*  |  string  | Invite code                                            |
| `password`*     |  string  | Password. Its length must be >= 6                      |
| `first_name`*   |  string  | Member's name. Its length must be >= 2 and <= 50       |
| `last_name`*    |  string  | Member's surname. Its length must be >= 2 and <= 50    |

### Response
An object with fields:
| Name       | Type     | Description                                    | 
|------------|----------|------------------------------------------------|
| `memberId` |  uint32  | Unique ID for new member                       |
| `login`    |  string  | Member's login, which is also the user name    |

### Errors
This method may return an error `11`.

## invites.create
This method creates an invitation to register a new member in bug tracker.

### Parameters
| Name         | Type     | Description                                                        | 
|--------------|----------|--------------------------------------------------------------------|
| `user_name`* |  string  | New member's username and login. Its length must be >= 2 and <= 32 |

### Response
An invite code. ( string )

### Errors
This method may return an error `12`.

## invites.get
This method returns invites created by the current authorized member.

### Parameters
Doesn't have

### Response
An object with fields:
| Name       | Type                    | Description      | 
|------------|-------------------------|------------------|
| `count`    |  int32                  | Invites count    |
| `items`    |  [Invite](#invite)[]    | Array of invites |

## members.getCard
This method returns an information about the bug tracker member.

### Parameters
| Name         | Type     | Description                                                                                 | 
|--------------|----------|---------------------------------------------------------------------------------------------|
| `member_id`  |  uint32  | ID of the member whose the info needs to be returned. By default — ID the authorized member |

### Response
An object with fields:
| Name                     | Type                    | Description                                                                                              | 
|--------------------------|-------------------------|----------------------------------------------------------------------------------------------------------|
| `member`                 |  [Member](#member)      | A member itself                                                                                          |
| `invitedBy`              |  uint32                 | ID of the inviting member                                                                                |
| `invitedByUserName`      |  string                 | Username of the inviting member                                                                          |
| `reportsCountPerProduct` |  object[]               | Number of created reports per product. Array of objects with `product_id(uint)` and `count(int)` fields. |
| `products`               |  [Product](#product)[]  | Array of mentioned products                                                                              |

## products.create
This method creates a product.

### Parameters
| Name     | Type     | Description                                           | 
|----------|----------|-------------------------------------------------------|
| `name`*  |  string  | New product's name. Its length must be >= 2 and <= 64 |

### Response
ID of the created product (`uint32`)

## products.get
This method returns products.

### Parameters
| Name       | Type   | Description                                                                          | 
|------------|--------|--------------------------------------------------------------------------------------|
| `filter`   |  byte  | `1` — returns all products.<br>`2` — returns only unfinished ones.<br>Default: `1`   |
| `owned`    |  byte  | `1` — returns only those products created by the current authorized member           |
| `extended` |  byte  | `1` — to return mentioned members array                                              |

### Response
An object with fields:
| Name      | Type                    | Description                             | 
|-----------|-------------------------|-----------------------------------------|
| `count`   |  int32                  | Products count                          |
| `items`   |  [Product](#product)[]  | Array of products                       |
| `members` |  [Member](#member)[]    | _(optional)_ Array of mentioned members |

## products.getCard
This method returns an information about the product.

### Parameters
| Name              | Type     | Description                                         | 
|-------------------|----------|-----------------------------------------------------|
| `product_id`*     |  uint32  | ID of the product whose infos needs to be returned. |

### Response
An object with fields:
| Name                     | Type                    | Description                                                      | 
|--------------------------|-------------------------|------------------------------------------------------------------|
| `product`                |  [Product](#product)    | A product itself                                                 |
| `reportsCount`           |  int32                  | Number of reports for the product                                |
| `openReportsCount`       |  int32                  | Number of open reports (status = 0 or 7)                         |
| `inProcessReportsCount`  |  int32                  | Number of in-process reports (status = 1, 4 or 2)                |
| `fixedReportsCount`      |  int32                  | Number of fixed reports (status = 2, 11, 12 or 15)               |
| `members`                |  [Member](#member)[]    | Array of mentioned members (contains the creator of the product) |

## products.setAsFinished
This method completes the product testing.

### Parameters
| Name          | Type     | Description       | 
|---------------|----------|-------------------|
| `product_id`* |  uint32  | ID of the product |

### Response
If authorized member is an owner of the product, the method will return `true`.

### Errors
If authorized member is not an owner of the product, the method will return an error `16`.<br>If the member will pass an ID of a non-existent product, the server will return an error `11`.

## reports.changeSeverity
This method changes the severity for the bugreport.

> [!NOTE]
> Once the product owner has changed the severity, the report author will no longer be able to change it.

### Parameters
| Name          | Type     | Description             | 
|---------------|----------|-------------------------|
| `report_id`*  |  uint32  | ID of the report        |
| `severity`*   |  byte    | Severity ID to assign   |
| `comment`     |  string  | Comment.                |

### Response
ID of the created comment (`uint32`).

### Errors
This method may return error with these codes: `11`, `16`, `42`, `43`.

## reports.changeStatus
This method changes the status for the bugreport.

### Parameters
| Name          | Type     | Description                                  | 
|---------------|----------|----------------------------------------------|
| `report_id`*  |  uint32  | ID of the report                             |
| `status`*     |  byte    | [Status](#report-statuses) ID to assign      |
| `comment`     |  string  | Comment. May be mandatory, depends on status |

### Response
ID of the created comment (`uint32`).

### Errors
This method may return error with these codes: `11`, `16`, `40`, `41`.

## reports.create
This method creates a new bug report for the product.

### Parameters
| Name            | Type     | Description                                                             | 
|-----------------|----------|-------------------------------------------------------------------------|
| `product_id`*   |  uint32  | ID of the product                                                       |
| `title`*        |  string  | Report title — short description of the bug. Its length must be <= 128  |
| `steps`*        |  string  | Steps to reproduce the bug. Its length must be <= 4096                  |
| `actual`*       |  string  | Actual behavior. Its length must be <= 2048                             |
| `expected`*     |  string  | Expected behavior. Its length must be <= 2048                           |
| `severity`*     |  byte    | Bug's [severity](#severity)                                             |
| `problem_type`* |  byte    | Bug's [problem type](#problem-types)                                    |

### Response
ID of the created report (`uint32`).

### Errors
If client will pass an ID of product whose testing has been finished, the server will return an error `20`.

## reports.createComment
This method creates a comment to the bugreport

### Parameters
| Name            | Type     | Description                                              | 
|-----------------|----------|----------------------------------------------------------|
| `report_id`*    |  uint32  | ID of the report for which the comment should be created |
| `comment`*      |  string  | Comment. Its length must be <= 1024                      |


### Response
ID of the created comment (`uint32`).

### Errors
This method may return error with these codes: `11`, `16`.

## reports.delete
This method deletes the bugreport.

### Parameters
| Name          | Type     | Description      | 
|---------------|----------|------------------|
| `report_id`*  |  uint32  | ID of the report |

### Response
`true`, if success.

### Errors
This method may return error with these codes: `11`, `16`, `44`.

## reports.edit
This method edits the bugreport.

### Parameters
| Name            | Type     | Description                                                               | 
|-----------------|----------|---------------------------------------------------------------------------|
| `report_id`*    |  uint32  | ID of the report to be edited                                             |
| `title`*        |  string  | Report title — short description of the bug. Its length must be <= 128    |
| `steps`*        |  string  | Steps to reproduce the bug. Its length must be <= 4096                    |
| `actual`*       |  string  | Actual behavior. Its length must be <= 2048                               |
| `expected`*     |  string  | Expected behavior. Its length must be <= 2048                             |
| `problem_type`* |  byte    | A bug's [problem type](#problem-types)                                    |

### Response
`true`, if success.

### Errors
This method may return error with these codes: `11`, `16`, `43`.

## reports.editComment
This method edits a comment to the bugreport

### Parameters
| Name            | Type     | Description                           | 
|-----------------|----------|---------------------------------------|
| `comment_id`*   |  uint32  | ID of the comment to be edited        |
| `comment`*      |  string  | Comment. It's length must be <= 1024  |


### Response
`true`, if success.

### Errors
This method may return error with these codes: `11`, `16`.

## reports.get
This method returns bugreports.

> [!NOTE]
> The server does not return reports with vulnerabilities that are not created by the current authorized member, or that relate to a product that is not owned by the current authorized member.

### Parameters
| Name            | Type     | Description                                                                                        | 
|-----------------|----------|----------------------------------------------------------------------------------------------------|
| `creator_id`    |  uint32  | Return only reports created by `creator_id`                                                        |
| `product_id`    |  uint32  | Return only reports created for product `product_id`                                               |
| `severity`      |  byte    | Return only reports with a specific [severity](#Severity)                                          |
| `problem_type`  |  byte    | Return only reports with a specific [problem type](#problem-types)                                 |
| `status`        |  byte    | Return only reports with a specific [status](#report-statuses)                                     |
| `extended`      |  byte    | `1` — to return mentioned members and products array, and additional (optional) fields in products |

### Response
An object with fields:
| Name       | Type                    | Description                              | 
|------------|-------------------------|------------------------------------------|
| `count`    |  int32                  | Reports count                            |
| `items`    |  [Report](#report)[]    | Array of reports                         |
| `members`  |  [Member](#member)[]    | _(optional)_ Array of mentioned members  |
| `products` |  [Product](#product)[]  | _(optional)_ Array of mentioned products |

## reports.getById
This method returns a bugreport by ID.

> [!NOTE]
> If you try to get a report that is a vulnerability and is not created by the current authorized member, or that relate to a product that is not owned by the current authorized member, the server returns an error (code `15`).

### Parameters
| Name            | Type     | Description      | 
|-----------------|----------|------------------|
| `report_id`*    |  uint32  | ID of the report |

### Response
An object with fields:
| Name       | Type                 | Description                                          | 
|------------|----------------------|------------------------------------------------------|
| `report`   |  [Report](#report)   | Report                                               |
| `author`   |  [Member](#member)   | Info about the author of the report                  |
| `product`  |  [Product](#product) | Info about the product for which the report was made |

### Errors
This method may return error with these codes: `11`, `15`.

## reports.getComments
This method returns comments under a bugreport.

> [!NOTE]
> If you try to get comments for a report that is a vulnerability and is not created by the current authorized member, or that relate to a product that is not owned by the current authorized member, the server returns an error (code `15`).

### Parameters
| Name            | Type     | Description                                             | 
|-----------------|----------|---------------------------------------------------------|
| `report_id`*    |  uint32  | ID of the report for which comments should be retrieved |
| `extended`      |  byte    | `1` — to return mentioned members array                 |


### Response
An object with fields:
| Name       | Type                    | Description                              | 
|------------|-------------------------|------------------------------------------|
| `count`    |  int32                  | Comments count                           |
| `items`    |  [Comment](#comment)[]  | Array of comments                        |
| `members`  |  [Member](#member)[]    | _(optional)_ Array of mentioned members  |

### Errors
This method may return error with these codes: `11`, `15`.

## server.getEnumStrings
This method returns descriptions for the static enum parameters used in **ProjectB**.

### Parameters
Doesn't have

### Response
An object with fields:
| Name             | Type                      | Description                                      | 
|------------------|---------------------------|--------------------------------------------------|
| `severities`     |  [EnumInfo](#enuminfo)[]  | Human-readable definitions for severity IDs      |
| `problemTypes`   |  [EnumInfo](#enuminfo)[]  | Human-readable definitions for problem type IDs  |
| `reportStatuses` |  [EnumInfo](#enuminfo)[]  | Human-readable definitions for report status IDs |

## server.init
This method initializes the database.

### Parameters
Doesn't have

### Response
`true`, if all OK, or `false`, if you try to call this method again after initialization. 

# API objects

## Basic

### EnumInfo
This object type is used to describe human readable [enum](#enums) values to the client.
| Name          | Type     | Description                                               | 
|---------------|----------|-----------------------------------------------------------|
| `id`          |  byte    | Enum value                                                |
| `name`        |  string  | Meaning                                                   |
| `description` |  string  | _(optional)_ Description                                  |
| `supported`   |  bool    | Describes if the client can use this enum in API requests |

### Member
| Name         | Type     | Description                | 
|--------------|----------|----------------------------|
| `id`         |  uint32  | Member's unique ID         |
| `userName`   |  string  | Member's user name (login) |
| `firstName`  |  string  | Member's name              |
| `lastName`   |  string  | Member's last name         |

### Invite
| Name              | Type     | Description                                                              | 
|-------------------|----------|--------------------------------------------------------------------------|
| `id`              |  uint32  | Invite's unique ID                                                       |
| `creatorid`       |  uint32  | ID of member who created the invite                                      |
| `created`         |  int64   | Creation time (unixtime)                                                 |
| `userName`        |  string  | Invited member's username and login                                      |
| `code`            |  string  | _(Optional)_ Invite code. Not returned if invite is used                 |
| `invitedMemberId` |  uint32  | _(Optional)_ Invited member's ID. Not returned if invite is not used yet |

### Product
| Name         | Type     | Description                                               | 
|--------------|----------|-----------------------------------------------------------|
| `id`         |  uint32  | Product's unique ID                                       |
| `ownerId`    |  uint32  | ID of member who created the product                      |
| `name`       |  string  | Name of the product                                       |
| `isFinished` |  bool    | Indicates that testing has been finished for this product |

### Report
| Name           | Type                    | Description                                                | 
|----------------|-------------------------|------------------------------------------------------------|
| `id`           |  uint32                 | Report's unique ID                                         |
| `productId`    |  uint32                 | ID of the product the report belongs to                    |
| `creatorId`    |  uint32                 | ID of member who created the report                        |
| `created`      |  int64                  | Creation time (unixtime)                                   |
| `updated`      |  int64?                 | Report update time (unixtime). _Optional_                  |
| `severity`     |  [EnumInfo](#enuminfo)  | Object describing the bug's [severity](#severity)          |
| `problemType`  |  [EnumInfo](#enuminfo)  | Object describing the bug's [problem type](#problem-types) |
| `status`       |  [EnumInfo](#enuminfo)  | Object describing the report [status](#report-statuses)    |
| `title`        |  string                 | Report title — short description of the bug.               |
| `steps`        |  string                 | _(optional)_ Steps to reproduce the bug.                   |
| `actual`       |  string                 | _(optional)_ Actual behavior.                              |
| `expected`     |  string                 | _(optional)_ Expected behavior.                            |

### Comment
| Name           | Type                    | Description                                                            | 
|----------------|-------------------------|------------------------------------------------------------------------|
| `id`           |  uint32                 | Comment's unique ID                                                    |
| `reportId`     |  uint32                 | ID of the report to which the comment was made                         |
| `creatorId`    |  uint32                 | ID of member who created the comment                                   |
| `created`      |  int64                  | Creation time (unixtime)                                               |
| `updated`      |  int64?                 | Comment edit time (unixtime). _Optional_                               |
| `comment`      |  string                 | The comment itself. _Optional_                                         |
| `newSeverity`  |  [EnumInfo](#enuminfo)  | Object describing the bug's [severity](#severity). _Optional_          |
| `newStatus`    |  [EnumInfo](#enuminfo)  | Object describing the report [status](#report-statuses). _Optional_    |

## Enums
The data below is stored in the `ELOR.ProjectB/API/DTO/StaticValues.cs` file.

### Severity
| ID    | Name          |  Description                                                                                                                                   | Supported |
|-------|---------------|------------------------------------------------------------------------------------------------------------------------------------------------|-----------|
| `1`   | Low           | Bugs that don't violate business logic, with an insignificant effect on the product overall, problems reflecting elements and data on the screen, grammatical and spelling mistakes. | true      |
| `2`   | Medium        | The bug doesn't critically affect the product but causes a major inconvenience. The feature doesn't work correctly, but there is a workaround. | true      |
| `3`   | High          | A feature isn't working properly or at all. For example, messages can't be sent, or photos can't be deleted.                                   | true      |
| `4`   | Critical      | Bugs that inhibit any further work from the app or further testing; crashes, freezing, loss of or damage to user data.                         | true      |
| `5`   | Vulnerability | Such reports are visible only to the report creator and the product owner.                                                                     | true      |

### Problem types
| ID    | Name                    | Description                                                                                           | Supported |
|-------|-------------------------|-------------------------------------------------------------------------------------------------------|-----------|
| `1`   | Suggestion              | Changes which you think should be made to improve the user experience.                                | true      | 
| `2`   | App crashes             | The app crashes, inhibiting any further work or testing.                                              | true      | 
| `3`   | App froze               | The app freezes, inhibiting any further work or testing.                                              | true      | 
| `4`   | Function not working    | Function not working or improperly working. For example, not sending messages or not deleting photos. | true      | 
| `5`   | Data damage             | User data fully or partially lost or corrupted.                                                       | true      | 
| `6`   | Performance             | User action response time.                                                                            | true      | 
| `7`   | Aesthetic discrepancies | Problems with element and data display on the screen.                                                 | true      | 
| `8`   | Typo                    | Grammatical, orthographic or syntactical mistakes, or problems in localization.                       | true      | 

### Report statuses
| ID    | Name              | Description                                                                                                                  | Supported |
|-------|-------------------|------------------------------------------------------------------------------------------------------------------------------|-----------|
| `0`   | Open              | _default status for newly created reports_                                                                                   | false     |
| `1`   | In progress       | The developer has begun solving this issue.                                                                                  | true      |
| `2`   | Fixed             | The problem has been fixed. Beta-testers currently aren't able to check this. The changes will appear in the newest version. | true      |
| `3`   | Declined          | The report is denied due to the problem being entered for the wrong product or not being a bug.                              | true      |
| `4`   | Under review      | A decision about this report will be made later.                                                                             | true      |
| `5`   | Closed            | The developer indicated that the issue has been resolved.                                                                    | false     |
| `6`   | Blocked           | The problem isn't related to the code of the tested product and occurs on the operating system API, library, side.           | true      |
| `7`   | Reopened          | The problem hasn't been fixed completely or at all. Please return to the report review.                                      | true      |
| `8`   | Cannot reproduce  | The problem isn't reproduced when following the steps in the report and according to the conditions described.               | true      |
| `9`   | Deferred          | The report has been accepted, but the issue described in it will be fixed much later.                                        | true      |
| `10`  | Needs correction  | There is insufficient information to localize the problem. The report wasn't created according to the rules.                 | true      |
| `11`  | Ready for testing | The problem has been fixed in the current version. The author of the report must check that this is accurate.                | true      |
| `12`  | Verified          | The issue has been fixed in the latest version.                                                                              | true      |
| `13`  | Won't be fixed    | The report describes a problem that can't be solved due to certain reasons.                                                  | true      |
| `14`  | Outdated          | The report describes a problem that was temporary or that was eliminated after a redesign, refactoring or other work.        | true      |
| `15`  | Duplicate         | The report is a duplicate of an earlier bug report. The issue has already been described.                                    | true      |

## Errors
The data below is stored in the `ELOR.ProjectB/Core/Exceptions/ServerException.cs` file.

| Code  |  Message                                                                                                                                                                         | 
|-------|----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|
| `1`   |  `Internal server error`<br>Returned if an unknown error occurred in the server. The `message` may contain additional information about the error.                               |
| `2`   |  `Not implemented`<br>Returned if you call an API method that is not ready to use.                                                                                               |
| `3`   |  `Unknown method passed`<br>Returned if you call a non-existent API method.                                                                                                      |
| `4`   |  `Login or password is incorrect`                                                                                                                                                |
| `5`   |  `Member authorization failed`<br>Returned if the client did not pass the `Authorization` header, or if the access token is invalid.                                             |
| `10`  |  `One of the parameters specified was missing or invalid`<br>Returned if the client does not send required/mandatory parameters, or if the value of these parameters is invalid. |
| `11`  |  `Not found`<br>Returned if the DB does not contain the requested item.                                                                                                          |
| `12`  |  `Already exist`                                                                                                                                                                 |
| `15`  |  `Access denied`                                                                                                                                                                 |
| `16`  |  `Permission to perform this action is denied`                                                                                                                                   |
| `20`  |  `Testing of this product is over`                                                                                                                                               |
| `40`  |  `Can't change the report status to the value you passed`                                                                                                                        |
| `41`  |  `This status requires a comment`                                                                                                                                                |
| `42`  |  `Can't change the report severity to the value you passed`                                                                                                                      |
| `43`  |  `You can no longer edit the report`                                                                                                                                             |
| `44`  |  `You can't delete the report`                                                                                                                                                   |