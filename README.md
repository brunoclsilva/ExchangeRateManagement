I have created the controller CurrencyPairController which contains 4 endpoints:
- GetRateAsync(string from, string to): It gets the CurrencyPair (by from and to currencies) from database, or via API call when it is not found on database. It also fires a message to a RabbitMQ queue when the rate is found via API.
- CreateCurrencyPaisAsync([FromBody] CurrencyPair newRate): It creates a CurrencyPair on database.
- UpdateCurrencyPaisAsync(int id, [FromBody] CurrencyPair updatedRate): It updates a CurrencyPair on database.
- DeleteCurrencyPaisAsync(int id): It deletes a CurrencyPair on database.

This controller comunicates with a service called CurrencyPairService, where the bussiness rules are performed.

I also created two other services:
- CurrencyPairIntegrationService which is responsible to call the API to retrieve the rate.
- MessagingService which is responsible to send the message with the rate data to a RabbitMQ queue.

I'm using a SQLite to generate the database when starting the API (we already have a Initial migration to create the table).

Also created a Middleware to handle the exceptions from the controller, called ExceptionHandlingMiddleware.

The project is designed with 5 projects:
ExchangeRateManagement: Contains the controller, appsetings, Program.cs and the database.
ExchangeRateManagement.Domain: Contains the entities and interfaces.
ExchangeRateManagement.Infra: Contains the repository and Migrations.
ExchangeRateManagement.Service: Contains the services.
ExchangeRateManagement.Test: Contains the unit tests using XUnit and Moq.
