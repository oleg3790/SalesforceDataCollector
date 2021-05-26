## Overview
This app demonstrates how to pull data from Salesforce via REST API and drop it into an AWS RDS database on a schedule. It is pre-configured to pull Salesforce Account objects.

Use the Terraform found in the `infrastructure` folder to spin up the DB for this app, and deploy this app as a Docker container to AWS ECS.
 
## Requirements

#### Database Server
You will need a database to run this app as it will drop all Salesforce record fetched into it. If you plan on using AWS RDS, this repo contains Terraform in the `infrastructure/database` folder that you can use to spin up an instance. 

#### Salesforce Connected App
1. To authorize your app to the Salesforce API, you will need to generate a digital certificate that you will upload to the Salesforce connected app and sign the JWT using the certificates private key.  
    1. [Docs to Generate Certificate](https://developer.salesforce.com/docs/atlas.en-us.sfdx_dev.meta/sfdx_dev/sfdx_dev_auth_key_and_cert.htm)
1. [Setup a Salesforce connected app](https://help.salesforce.com/articleView?id=sf.connected_app_create_basics.htm&type=5)
    1. Enable `Enable OAuth Settings`
    1. Enable `Enable for Device Flow` (will auto assign callback URL which you don't need for this type of app)
    1. Enable `Use Digital Signatures`
    1. Upload certificate created in previous step
    1. Add `Selected OAuth Scopes`
    1. After save:
        1. Go to App Manager >> [App Name] >> Manage
        1. Click `Edit Policies`
        1. Change `Permitted Users` to `Admin approved users are pre-authorized`
        1. Save
        1. On the app management page, scroll down to the `Profiles` section and add the profiles that are allowed to access the app 

#### Configuration Variables
This app requires the following configuration variables that you can either pass in via environment variables or by adding a `secrets.json` file with the variables included to the project:

- `ConnectionStrings:default` - Default DB connection string
- `Salesforce:ApiVersion` - Salesforce API version to use (default is 51.0)
- `Salesforce:ClientId` - Salesforce connected app client Id
- `Salesforce:User` - Salesforce username
- `Salesforce:AuthKey` - Salesforce connected app OAuth certificate private key encoded as a base 64 string (you can use an online tool to base 64 encode the private key, or run `base64 <privatekey>` from a bash shell)

## Initialize Database via Entity Framework
All configuration to spin up a database is in `src/SalesforceDataCollector/Migrations`. When dealing with a new database, just run the `Update-Database` from the Package Manager Console to apply the migrations to a new database on your server.

## Mocking Salesforce Data
During development, if you need to quickly test this service against a large data-set, you can use mockaroo.com to create a randomized CSV file and then use dataloader.io or the [Salesforce workbench](https://workbench.developerforce.com/) to load all of the data at once into your developer account.
