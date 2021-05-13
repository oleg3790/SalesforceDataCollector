## Overview
This app demonstrates how to pull data from Salesforce via REST API and drop it into an AWS RDS database on a schedule.

Use the Terraform found in the `infrastructure` folder to spin up the DB for this app, and deploy this app as a Docker container to AWS ECS.
 
## Requirements

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
For this app to work, is requires the following configuration variables:

- `ConnectionStrings:default` - Default DB connection string
- `Salesforce:ClientId` - Salesforce connected app client Id
- `Salesforce:User` - Salesforce username
- `Salesforce:CertPrivateKey` - Salesforce connected app OAuth certificate private key encoded as a base 64 string (you can use an online tool to base 64 encode the private key, or run `base64 <privatekey>` from a bash shell)