# ProjectB

A simple bug tracker written in .NET 8 for demonstration purposes.

To get started:
1. Install MySQL (version 8 or newer) and create a database.
2. Write the name of the created database, as well as the connection parameters to the database, in the `btconfig.json` file.
3. Start the server. By default, the server listens on port `7575`.
4. Make the following query: `http://localhost:7575/server.init`. The server will create tables in the database and the first user, and return `{"response":true}`. The login and password of the first user are specified in the `btconfig.json` file.

To add new members, you must have another member create an invitation.

Check [the API documentation](API.md) to use the bug tracker.