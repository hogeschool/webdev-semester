# Webdevelopment Starter Kit
This project contains a template to get started with.
The technical setup includes: 
- .NET
- Entity Framework Core
- Node.js
- Typescript
- React 
- Bootstrap CSS

The template has allready the following functionalities implemented: 
- A HomeController that serves the routes that renders the React application
- A pre-installed frontend template using React and Typescript
- A database model that will be installed in a SqlLite database
- A LoginController and Login frontend component that allows an admin to login to the dashboard

## Instalation stepps
These install stepps needs to be executed only once at the start. 

### Install EF core
Run the follwoing commands to install Entity Framework core.
https://learn.microsoft.com/en-us/ef/core/get-started/overview/install
- `dotnet add package Microsoft.EntityFrameworkCore.SqlServer`
- `dotnet tool install --global dotnet-ef`
- `dotnet add package Microsoft.EntityFrameworkCore.Design`
- Run `dotnet ef` to verify the installation

### Install Node.js
https://nodejs.org/en 

You can use NVM (Node version manager)
https://github.com/nvm-sh/nvm
This is recommended because this will allow you switch easely between different Node versions. 
This is sometimes required for older libraries or projects that where build with an old version of Node.js. 

### Install SQllite 
https://learn.microsoft.com/en-us/ef/core/get-started/overview/first-app?tabs=netcore-cli
- `dotnet add package Microsoft.EntityFrameworkCore.Sqlite`

## Adding a new database migrations
Whenever you make a change to the `DatabaseContext` you will need to add another migrations. 
All the database changes are being managed from the `/Migrations/` folder, the content of this folder is automatically generated 
by Entity Framework Core based on the specified database model. 

To add a new migration after a database change run the following command: 
`dotnet ef migrations add <Migration name>`

Inside the migrations folder you can allready find a migration called `InitialCreate` containing the creation of the Admin entity. 
The `DatabaseContext.cs` file contains two pre-configured databases for the practicall cases. You can uncomment one of them and a new migration
to apply the model. 

After every migration you will also need to run `dotnet ef database update`, this will apply the changes to the database.


## Run the project
The following commands needs to be executed every time you want to run the project.

### Backend
- Run `dotnet ef database update` to run the database migrations. If you run this for the first time, the tool will create a local database (.db) file for you. You only need to run this if you have database changes. 
- Run `dotnet watch run` to start the backend in watch mode. 
- The application will be available on [http://localhost:5097/](http://localhost:5097/)

### Frontend
- Go inside the `/Frontend/` directory `cd Frontend`
- Run `yarn install` or `npm npm install`
- `yarn watch` or `npm run watch` will build a development Javascript bundle in watch mode



# Practicum workflow
1. Create `TheatreShowController.cs`, you can copy the content of `HomeController.cs` and use that as a baseline.
2. Create endpoints to Create (POST), Read (GET), Update (PUT) and Delete (DELETE) a show (CRUD)