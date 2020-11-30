### Azure Sphere CLI commands

To set the device in development mode: `azsphere device enable-development`

#### Deploy image

Optionally remove development mode: `azsphere device enable-cloud-test`

To get a list of products: `azsphere prd list`  
To get a list of devicegroups in a product: `azsphere prd dg list -i <productid>`  
To create a deployment: `azsphere dg dep create -i <devicegroupid> -p <file>.imagepackage`

### References:  
https://docs.microsoft.com/en-us/azure-sphere/deployment/deployment-concepts  
https://docs.microsoft.com/en-us/azure-sphere/deployment/create-a-deployment