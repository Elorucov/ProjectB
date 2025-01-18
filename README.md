# ProjectB

A simple self-hosted bug tracker inspired by [VK's bug-tracker](https://vk.com/testing). Written in ASP.NET 8.

The project was created as one example of a backend service in order to improve my backend, API and database experience (MySQL for now) and demonstrate it in my job search. And it took me a little over a week to develop from scratch. 😅

## Getting started
1. Install MySQL (version 8 or newer) and create a database (default name is `projectb`, encoding `utf8mb4_0900_ai_ci`).
2. Write the name of the created database, as well as the connection parameters to the database, in the `btconfig.json` file.
3. Start the server. By default, the server listens on port `7575`.
4. Make the following query: `http://localhost:7575/server.init`. The server will create tables in the database and the first member, and will return `{"response":true}`. The login and password of the first member are specified in the `btconfig.json` file.

Check [the API documentation](API.md) to use the bug tracker.

## How it works
The bug tracker consists of the following entities: a member, a product, a report to the product and a comment to the report. 

A user can register in the bug tracker only by invitations from other bug tracker members. First, the member creates an invitation by setting `user_name` of the new member and receives an invitation code. The `user_name` is also the login. Then the new user registers in the bug tracker using the invitation code, sets a password for his account and receives his user name/login.

All members can create **products** as well as generate reports for all products. It is possible to complete testing of a product, and then reports cannot be created for that product.

Each **report** consists of a brief description of the bug (title), reproduction steps, actual and expected result, problem type and severity. The owner of the product to which the report was left can change the **status** of the report during the bug fixing process. For example, if the report is correct, the owner will first change the status to “under review”, then to “in progress”, “fixed” and “ready for testing”. Or if the report is incorrect, the owner can decline it. The owner will not be able to change the status to “reopened”.

The author of the report can also change the status, but only from “ready for testing” to “reopened” and “verified”. Or from “cannot reproduce” and “needs correction” to “reopened”. 

The author can edit or delete a report, but if the product owner has already changed the status or severity, it will not be possible to do so.

You can see the list of available priority values, report status and problem type on [this page](API.md#enums).

Reports with vulnerabilities are available only to the report author and the product owner. They are not visible in the list of all reports of other members.

Any member can leave a **comment** to any reports except for vulnerabilities. Only the report author and the product owner can leave comments to such reports. A comment is added every time the status or priority of a report changes, but without text.

## TODO
- [x] Getting reports count created by the one member (done by creating the `members.getCard` method);
- [x] Getting reports count for the one product (done by creating the `products.getCard` method);
- [ ] `offset`, `count` and `rev` parameters for `products.get`, `reports.get` and `reports.getComments` methods;
- [ ] Search reports, products and members.
- [ ] (maybe a simple web app to demonstrate?)