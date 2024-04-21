# Invoice manager

Invoice management system, microservice architecture.

The services implement Vertical Slice Architecture, YARP reverse proxy, RabbitMq message broker, request-response pattern, minimal api,
PFD file generation, miniIO object storage, integration and unit tests.

## 👷 Frameworks, Libraries and Technologies

- [xUnit](https://github.com/xunit/xunit)
- [.NET 8](https://github.com/dotnet/core)
- [ASP.NET Core 8](https://github.com/dotnet/aspnetcore)
- [Entity Framework](https://github.com/dotnet/efcore)
- [PostgreSQL](https://github.com/postgres)
- [MinIO object storage](https://github.com/minio/minio-dotnet)
- [MediatR](https://github.com/jbogard/MediatR)
- [Carter](https://github.com/CarterCommunity/Carter)
- [MassTransit](https://github.com/MassTransit/MassTransit)
- [RabbitMQ](https://github.com/rabbitmq)
- [QuestPDF](https://github.com/QuestPDF/QuestPDF)
- [FluentValidation](https://github.com/FluentValidation/FluentValidation)
- [IdentityModel](https://github.com/IdentityModel)
- [Swashbuckle](https://github.com/domaindrivendev/Swashbuckle.AspNetCore)
- [Yarp.ReverseProxy](https://github.com/microsoft/reverse-proxy)
- [Docker](https://github.com/docker)


## 🏗️ List of microservices

- Gateway service: *Single entry point to the application.*


- Identity service: *User authentication.*


- Company service: *Manages interactions with companies.*


- Invoice service: *Manages interaction with the invoices.*

 
- FileGenerator service: *Generates PDF files.*


- Storage service: *Stores invoices in MinIO storage.*


## 🔍️ Microservices diagram

![Microservices diagram](https://github.com/gitEugeneL/Invoice-manager/blob/dev/microservices-diagram.png)


## 🐳 List of docker containers

- **gateway.api** - *reverse proxy gateway container.*


- **rabbitmq** - *message broker container.*


- **minio** - *object storage for storage service.*


- **storage.api** - *asp.net app container for storage service.*


- **file-generator.api** - *asp.net app container for file generator service.*


- **identity.api** - *asp.net app container for identity service.*


- **identity.api.database** - *postgresql database container for identity service.*


- **company.api** - *asp.net app container for company service.*


- **company.api.database** - *postgresql database container for company service.*


- **invoice.api** - *asp.net app container for invoice service.*


- **invoice.api.database** - *postgresql database container for invoice service.*


## 🩺 How to run tests

*Allows you to run all integration and unit tests.*

   ```sh
   > dotnet test  # donet SKD is required
   ```


## 🚜 How to run the server

1. Build and start Docker images based on the configuration defined in the docker-compose.yml

   ```sh
   > make up  # docker-compose up --build
   ```

2. Stop and remove containers
   ```sh
   > make down  # docker-compose down
   ```

## 🔐 Service access

| container             | port         | login | password | access                                           |
|-----------------------|--------------|-------|----------|--------------------------------------------------|
| gateway.api           | 5000         | -     | -        | http://localhost:5000                            |
| rabbitMQ              | 5672 / 15672 | user  | password | http://localhost:15672 (local access)            |
| minIO                 | 9000 / 9001  | user  | password | http://localhost:9001/login (local access)       |
| fileGenerator.api     | -            | -     | -        | -                                                |
| identity.api          | 8000         | -     | -        | http://localhost:8000 (local access)             |
| company.api           | 8001         | -     | -        | http://localhost:8001 (local access)             |
| invoice.api           | 8002         | -     | -        | http://localhost:8002 (local access)             |
| storage.api           | 8080         | -     | -        | http://localhost:8080 (local access)             |
| identity.api.database | 5432         | user  | password | http://localhost:5432 (local access)             |
| company.api.database  | 5433         | user  | password | http://localhost:5433 (local access)             |
| invoice.api.database  | 5434         | user  | password | http://localhost:5434 (local access)             |


## 🖨️ Swagger UI documentation (local access)

- Identity service

        http://localhost:8000/swagger/index.html

- Company service
    
        http://localhost:8001/swagger/index.html  

- Invoice service

        http://localhost:8002/swagger/index.html  

- Storage service

        http://localhost:8080/swagger/index.html  


## 🔧 Implementation features

### Gateway base Url: http://localhost:5000

----

### Identity service

#### Register
<details>
<summary>
    <code>POST</code> <code><b>/auth/register</b></code><code>(allows you to register)</code>
</summary>

##### Body
> | name             | type         | data type    |                                                           
> |------------------|--------------|--------------|
> | email            | required     | string       |
> | password         | required     | string       |

##### Responses
> | http code | content-type       | response                                                                  |
> |-----------|--------------------|---------------------------------------------------------------------------|
> | `201`     | `application/json` | `{"userId": "3fa85f64-5717-4562-b3fc-2c963f66afa6", "email": "string"}`   |
> | `400`     | `application/json` | `array`                                                                   | 
> | `409`     | `application/json` | `string`                                                                  |
</details>

#### Login
<details>
<summary>
    <code>POST</code> <code><b>/auth/login</b></code><code>(allows you to login, issues accessToken and refreshToken)</code>
</summary>

##### Body
> | name             | type         | data type    |                                                           
> |------------------|--------------|--------------|
> | email            | required     | string       |
> | password         | required     | string       |

##### Responses
> | http code | content-type       | response                                                                                                                              |
> |-----------|--------------------|---------------------------------------------------------------------------------------------------------------------------------------|
> | `200`     | `application/json` | `{"accessToken": "string", "refreshToken": "string", "refreshTokenExpires": "2024-04-21T17:42:12.146Z", "accessTokenType": "string"}` |
> | `400`     | `application/json` | `array`                                                                                                                               | 
> | `404`     | `application/json` | `string`                                                                                                                              |
</details>

#### Refresh
<details>
<summary>
    <code>POST</code> <code><b>/auth/refresh</b></code><code>(allows to refresh access and refresh tokens)</code>
</summary>

##### Body
> | name            | type       | data type    |                                                           
> |-----------------|------------|--------------|
> | "refreshToken"  | required   | string       |

##### Responses
> | http code | content-type       | response                                                                                                                              |
> |-----------|--------------------|---------------------------------------------------------------------------------------------------------------------------------------|
> | `200`     | `application/json` | `{"accessToken": "string", "refreshToken": "string", "refreshTokenExpires": "2024-04-21T17:43:47.494Z", "accessTokenType": "string"}` |
> | `400`     | `application/json` | `array`                                                                                                                               |
> | `401`     | `application/json` | `string`                                                                                                                              |
</details>

#### Logout
<details>
<summary>
    <code>POST</code> <code><b>/auth/logout</b></code><code>(allows to logout and deactivates refresh tokens)</code>
</summary>


##### Body
> | name            | type       | data type    |                                                           
> |-----------------|------------|--------------|
> | "refreshToken"  | required   | string       |

##### Responses
> | http code | content-type       | response     |
> |-----------|--------------------|--------------|
> | `204`     | `application/json` | `NoContent`  |
> | `400`     | `application/json` | `array`      |
> | `401`     | `application/json` | `string`     |
</details>

----

### Company service

*Functionality that allows to manage and interact with companies*

#### Create new companies (🔒*Token required*)
<details>
<summary>
    <code>POST</code> <code><b>/company</b></code><code>(allows to create new companies 🔒️[token required])</code>
</summary>

##### Body
> | name          | type       | data type |                                                           
> |---------------|------------|-----------|
> | "name"        | required   | string    |
> | "taxNumber"   | required   | string    |
> | "city"        | required   | string    |
> | "street"      | required   | string    |
> | "houseNumber" | required   | string    |
> | "postalCode"  | required   | string    |

##### Responses
> | http code | content-type       | response                                                                                                                                                                                |
> |-----------|--------------------|-----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|
> | `201`     | `application/json` | `{"companyId": "3fa85f64-5717-4562-b3fc-2c963f66afa6", "name": "string", "taxNumber": "string", "city": "string", "street": "string", "houseNumber": "string", "postalCode": "string"}` |
> | `400`     | `application/json` | `array`                                                                                                                                                                                 |
> | `401`     | `application/json` | `string`                                                                                                                                                                                |
> | `403`     | `application/json` | `string`                                                                                                                                                                                |
</details>

#### Update your companies (🔒*Token required*)
<details>
<summary>
    <code>PUT</code> <code><b>/company</b></code><code>(allows to update your companies 🔒️[token required])</code>
</summary>

##### Body
> | name           | type           | data type |                                                           
> |----------------|----------------|-----------|
> | "companyId"    | required       | uuid      |
> | "name"         | not required   | string    |
> | "taxNumber"    | not required   | string    |
> | "city"         | not required   | string    |
> | "street"       | not required   | string    |
> | "houseNumber"  | not required   | string    |
> | "postalCode"   | not required   | string    |

##### Responses
> | http code | content-type       | response                                                                                                                                                                                |
> |-----------|--------------------|-----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|
> | `200`     | `application/json` | `{"companyId": "3fa85f64-5717-4562-b3fc-2c963f66afa6", "name": "string", "taxNumber": "string", "city": "string", "street": "string", "houseNumber": "string", "postalCode": "string"}` |
> | `400`     | `application/json` | `array`                                                                                                                                                                                 |
> | `401`     | `application/json` | `string`                                                                                                                                                                                |
> | `403`     | `application/json` | `string`                                                                                                                                                                                |
> | `404`     | `application/json` | `string`                                                                                                                                                                                |
</details>

#### Delete your companies (🔒*Token required*)
<details>
<summary>
    <code>DELETE</code> <code><b>/company/{ id:uuid }</b></code><code>(allows to delete your companies 🔒️[token required])</code>
</summary>

##### Responses
> | http code | content-type       | response    |
> |-----------|--------------------|-------------|
> | `204`     | `application/json` | `NoContent` |
> | `401`     | `application/json` | `string`    |
> | `403`     | `application/json` | `string`    |
> | `404`     | `application/json` | `string`    |
</details>

#### Get all your companies (🔒*Token required*)
<details>
<summary>
    <code>GET</code> <code><b>/company</b></code><code>(allows you to get all your companies 🔒️[token required])</code>
</summary>

##### Parameters
> | name          | type         | data type |                                                           
> |---------------|--------------|-----------|
> | PageNumber    | not required | int32     |
> | PageSize      | not required | int32     |

##### Responses
> | http code | content-type       | response                                                                                                                                                                                                                                                          |
> |-----------|--------------------|-------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|
> | `200`     | `application/json` | `{"items": [ { "companyId": "3fa85f64-5717-4562-b3fc-2c963f66afa6", "name": "string", "taxNumber": "string", "city": "string", "street": "string", "houseNumber": "string", "postalCode": "string" } ], "pageNumber": 0, "totalPages": 0, "totalItemsCount": 0 }` |
> | `401`     | `application/json` | `string`                                                                                                                                                                                                                                                          |
> | `403`     | `application/json` | `string`                                                                                                                                                                                                                                                          |
</details>

#### Get one your company (🔒*Token required*)
<details>
<summary>
    <code>GET</code> <code><b>/company/{ id:uuid }</b></code><code>(allows you to get one your company 🔒️[token required])</code>
</summary>

##### Responses
> | http code | content-type       | response                                                                                                                                                                                |
> |-----------|--------------------|-----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|
> | `200`     | `application/json` | `{"companyId": "3fa85f64-5717-4562-b3fc-2c963f66afa6", "name": "string", "taxNumber": "string", "city": "string", "street": "string", "houseNumber": "string", "postalCode": "string"}` |
> | `401`     | `application/json` | `string`                                                                                                                                                                                |
> | `403`     | `application/json` | `string`                                                                                                                                                                                |
</details>

----

### Invoice service

### Invoice

*Functionality that allows to manage and interact with invoices*

#### Create invoices (🔒*Token required*)
<details>
<summary>
    <code>POST</code> <code><b>/invoice</b></code><code>(allows to create new invoices 🔒️[token required])</code>
</summary>

##### Body
> | name              | type       | data type |                                                           
> |-------------------|------------|-----------|
> | "sellerCompanyId" | required   | uuid      |
> | "buyerCompanyId"  | required   | uuid      |
> | "termsOfPayment"  | required   | int       |
> | "paymentType"     | required   | string    |
> | "status"          | required   | string    |

##### Responses
> | http code | content-type       | response                                                                                                                                                                                                                                                                                                                                                            |
> |-----------|--------------------|---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|
> | `201`     | `application/json` | `{ "invoiceId": "3fa85f64-5717-4562-b3fc-2c963f66afa6", "sellerCompanyId": "3fa85f64-5717-4562-b3fc-2c963f66afa6", "buyerCompanyId": "3fa85f64-5717-4562-b3fc-2c963f66afa6", "number": "string", "totalNetPrice": 0, "totalGrossPrice": 0, "termsOfPayment": 0, "paymentType": "string", "status": "string", "itemsId": ["3fa85f64-5717-4562-b3fc-2c963f66afa6"] }` |
> | `400`     | `application/json` | `array`                                                                                                                                                                                                                                                                                                                                                             |
> | `401`     | `application/json` | `string`                                                                                                                                                                                                                                                                                                                                                            |
> | `403`     | `application/json` | `string`                                                                                                                                                                                                                                                                                                                                                            |
> | `404`     | `application/json` | `string`                                                                                                                                                                                                                                                                                                                                                            |
</details>

#### Update your unfinished invoices (🔒*Token required*)
<details>
<summary>
    <code>PUT</code> <code><b>/invoice</b></code><code>(allows to update your invoices 🔒️[token required])</code>
</summary>

##### Body
> | name             | type       | data type |                                                           
> |------------------|------------|-----------|
> | "invoiceId"      | required   | uuid      |
> | "status"         | required   | string    |

##### Responses
> | http code | content-type       | response                                                                                                                                                                                                                                                                                                                                                            |
> |-----------|--------------------|---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|
> | `200`     | `application/json` | `{ "invoiceId": "3fa85f64-5717-4562-b3fc-2c963f66afa6", "sellerCompanyId": "3fa85f64-5717-4562-b3fc-2c963f66afa6", "buyerCompanyId": "3fa85f64-5717-4562-b3fc-2c963f66afa6", "number": "string", "totalNetPrice": 0, "totalGrossPrice": 0, "termsOfPayment": 0, "paymentType": "string", "status": "string", "itemsId": ["3fa85f64-5717-4562-b3fc-2c963f66afa6"] }` |
> | `400`     | `application/json` | `array`                                                                                                                                                                                                                                                                                                                                                             |
> | `401`     | `application/json` | `string`                                                                                                                                                                                                                                                                                                                                                            |
> | `403`     | `application/json` | `string`                                                                                                                                                                                                                                                                                                                                                            |
> | `404`     | `application/json` | `string`                                                                                                                                                                                                                                                                                                                                                            |
</details>

#### Delete your unfinished invoices (🔒*Token required*)
<details>
<summary>
    <code>DELETE</code> <code><b>/invoice/{ id:uuid }</b></code><code>(allows to delete your invoices 🔒️[token required])</code>
</summary>

##### Responses
> | http code | content-type       | response    |
> |-----------|--------------------|-------------|
> | `204`     | `application/json` | `NoContent` |
> | `401`     | `application/json` | `string`    |
> | `403`     | `application/json` | `string`    |
> | `404`     | `application/json` | `string`    |
</details>

#### Finalize your unfinished invoices (🔒*Token required*)
<details>
<summary>
    <code>PATCH</code> <code><b>/invoice/lock/{ id:uuid }</b></code><code>(allows to finalize your invoices 🔒️[token required])</code>
</summary>

##### Responses
> | http code | content-type       | response |
> |-----------|--------------------|----------|
> | `200`     | `application/json` | `string` |
> | `401`     | `application/json` | `string` |
> | `403`     | `application/json` | `string` |
> | `404`     | `application/json` | `string` |
</details>


#### Get all your invoices (🔒*Token required*)
<details>
<summary>
    <code>GET</code> <code><b>/invoice</b></code><code>(allows you to get all your invoices 🔒️[token required])</code>
</summary>

##### Parameters
> | name          | type         | data type |                                                           
> |---------------|--------------|-----------|
> | PageNumber    | not required | int32     |
> | PageSize      | not required | int32     |

##### Responses
> | http code | content-type       | response                                                                                                                                                                                                                                                                                                                                                                  |
> |-----------|--------------------|---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|
> | `200`     | `application/json` | `[ { "invoiceId": "3fa85f64-5717-4562-b3fc-2c963f66afa6", "sellerCompanyId": "3fa85f64-5717-4562-b3fc-2c963f66afa6", "buyerCompanyId": "3fa85f64-5717-4562-b3fc-2c963f66afa6", "number": "string", "totalNetPrice": 0, "totalGrossPrice": 0, "termsOfPayment": 0, "paymentType": "string", "status": "string", "itemsId": [ "3fa85f64-5717-4562-b3fc-2c963f66afa6" ] } ]` |
> | `401`     | `application/json` | `string`                                                                                                                                                                                                                                                                                                                                                                  |
> | `403`     | `application/json` | `string`                                                                                                                                                                                                                                                                                                                                                                  |
</details>

#### Get one your invoice (🔒*Token required*)
<details>
<summary>
    <code>GET</code> <code><b>/invoice/{ id:uuid }</b></code><code>(allows you to get one your company 🔒️[token required])</code>
</summary>

##### Responses
> | http code | content-type       | response                                                                                                                                                                                                                                                                                                                                                                  |
> |-----------|--------------------|---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|
> | `200`     | `application/json` | `[ { "invoiceId": "3fa85f64-5717-4562-b3fc-2c963f66afa6", "sellerCompanyId": "3fa85f64-5717-4562-b3fc-2c963f66afa6", "buyerCompanyId": "3fa85f64-5717-4562-b3fc-2c963f66afa6", "number": "string", "totalNetPrice": 0, "totalGrossPrice": 0, "termsOfPayment": 0, "paymentType": "string", "status": "string", "itemsId": [ "3fa85f64-5717-4562-b3fc-2c963f66afa6" ] } ]` |
> | `401`     | `application/json` | `string`                                                                                                                                                                                                                                                                                                                                                                  |
> | `403`     | `application/json` | `string`                                                                                                                                                                                                                                                                                                                                                                  |
</details>

### Invoice item

*Functionality that allows to manage and interact with invoice items*

#### Create new items on your unfinished invoice (🔒*Token required*)
<details>
<summary>
    <code>POST</code> <code><b>/item</b></code><code>(allows to create new items 🔒️[token required])</code>
</summary>

##### Body
> | name        | type       | data type |                                                           
> |-------------|------------|-----------|
> | "invoiceId" | required   | uuid      |
> | "name"      | required   | string    |
> | "amount"    | required   | int       |
> | "unit"      | required   | string    |
> | "vat"       | required   | string    |
> | "netPrice"  | required   | decimal   |

##### Responses
> | http code | content-type       | response                                                                                                                                                                                                                           |
> |-----------|--------------------|------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|
> | `201`     | `application/json` | `{ "invoiceId": "3fa85f64-5717-4562-b3fc-2c963f66afa6", "itemId": "3fa85f64-5717-4562-b3fc-2c963f66afa6", "name": "string", "amount": 0, "unit": "string", "vat": "string", "netPrice": 0, "sumNetPrice": 0, "sumGrossPrice": 0 }` |
> | `400`     | `application/json` | `array`                                                                                                                                                                                                                            |
> | `401`     | `application/json` | `string`                                                                                                                                                                                                                           |
> | `403`     | `application/json` | `string`                                                                                                                                                                                                                           |
> | `404`     | `application/json` | `string`                                                                                                                                                                                                                           |
</details>

#### Update items on your unfinished invoice (🔒*Token required*)
<details>
<summary>
    <code>PUT</code> <code><b>/item</b></code><code>(allows to update your items 🔒️[token required])</code>
</summary>

##### Body
> | name       | type         | data type |                                                           
> |------------|--------------|-----------|
> | "itemId"   | required     | uuid      |
> | "name"     | not required | string    |
> | "amount"   | not required | int       |
> | "netPrice" | not required | decimal   |

##### Responses
> | http code | content-type       | response                                                                                                                                                                                                                           |
> |-----------|--------------------|------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|
> | `200`     | `application/json` | `{ "invoiceId": "3fa85f64-5717-4562-b3fc-2c963f66afa6", "itemId": "3fa85f64-5717-4562-b3fc-2c963f66afa6", "name": "string", "amount": 0, "unit": "string", "vat": "string", "netPrice": 0, "sumNetPrice": 0, "sumGrossPrice": 0 }` |
> | `400`     | `application/json` | `array`                                                                                                                                                                                                                            |
> | `401`     | `application/json` | `string`                                                                                                                                                                                                                           |
> | `403`     | `application/json` | `string`                                                                                                                                                                                                                           |
> | `404`     | `application/json` | `string`                                                                                                                                                                                                                           |
</details>

#### Delete items on your unfinished invoice (🔒*Token required*)
<details>
<summary>
    <code>DELETE</code> <code><b>/item/{ id:uuid }</b></code><code>(allows to delete your items 🔒️[token required])</code>
</summary>

##### Responses
> | http code | content-type       | response    |
> |-----------|--------------------|-------------|
> | `204`     | `application/json` | `NoContent` |
> | `401`     | `application/json` | `string`    |
> | `403`     | `application/json` | `string`    |
> | `404`     | `application/json` | `string`    |
</details>

#### Get all items by invoice (🔒*Token required*)
<details>
<summary>
    <code>GET</code> <code><b>/item/all-by-invoice/{ id:uuid }</b></code><code>(allows you to get items by invoice 🔒️[token required])</code>
</summary>

##### Responses
> | http code | content-type       | response                                                                                                                                                                                                                               |
> |-----------|--------------------|----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|
> | `200`     | `application/json` | `[ { "invoiceId": "3fa85f64-5717-4562-b3fc-2c963f66afa6", "itemId": "3fa85f64-5717-4562-b3fc-2c963f66afa6", "name": "string", "amount": 0, "unit": "string", "vat": "string", "netPrice": 0, "sumNetPrice": 0, "sumGrossPrice": 0 } ]` |
> | `401`     | `application/json` | `string`                                                                                                                                                                                                                               |
> | `403`     | `application/json` | `string`                                                                                                                                                                                                                               |
> | `404`     | `application/json` | `string`                                                                                                                                                                                                                               |
</details>

#### Get one item by id (🔒*Token required*)
<details>
<summary>
    <code>GET</code> <code><b>/item/{ id:uuid }</b></code><code>(allows you to get one item 🔒️[token required])</code>
</summary>

##### Responses
> | http code | content-type       | response                                                                                                                                                                                                                               |
> |-----------|--------------------|----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|
> | `200`     | `application/json` | `[ { "invoiceId": "3fa85f64-5717-4562-b3fc-2c963f66afa6", "itemId": "3fa85f64-5717-4562-b3fc-2c963f66afa6", "name": "string", "amount": 0, "unit": "string", "vat": "string", "netPrice": 0, "sumNetPrice": 0, "sumGrossPrice": 0 } ]` |
> | `401`     | `application/json` | `string`                                                                                                                                                                                                                               |
> | `403`     | `application/json` | `string`                                                                                                                                                                                                                               |
> | `404`     | `application/json` | `string`                                                                                                                                                                                                                               |
</details>

----

### Storage service

*PDF file is created automatically when the invoice is finalized*

#### Get names of all your PDF files (🔒*Token required*)
<details>
<summary>
    <code>GET</code> <code><b>/storage</b></code><code>(allows to get names of all your PDF files 🔒️[token required])</code>
</summary>

##### Responses
> | http code | content-type       | response                  |
> |-----------|--------------------|---------------------------|
> | `200`     | `application/json` | `[ "string", "string", ]` |
> | `401`     | `application/json` | `string`                  |
> | `403`     | `application/json` | `string`                  |
> | `404`     | `application/json` | `string`                  |
</details>

#### Download your PDF file by name (🔒*Token required*)
<details>
<summary>
    <code>GET</code> <code><b>/storage/{ fileName:string }</b></code><code>(allows to download your PDF file 🔒️[token required])</code>
</summary>

##### Responses
> | http code | content-type        | response  |
> |-----------|---------------------|-----------|
> | `200`     | `application/pdf`   | `PDFfile` |
> | `401`     | `application/json`  | `string`  |
> | `403`     | `application/json`  | `string`  |
> | `404`     | `application/json`  | `string`  |
</details>
