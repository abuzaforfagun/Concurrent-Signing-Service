# Concurrent key signing service

*Problem Statement*
Implement a record signing service using a message driven / microservice solution.

Given a database of 100,000 records and a collection of 100 private keys, create a process to concurrently sign batches of records, storing the signatures in the database until all records are signed.

*Rules*

* No double signing: Only a signature per record should be stored (sign each record individually in batches of X size)
* Any given key in the keyring must not be used concurrently
* A single key should be used for signing all records in a single batch
* Keys should be selected from least recently used to most recently
* Batch size should be configurable by the user (does not change during runtime)

*Guidelines*

* Use a runtime environment of your choosing (we predominantly use Golang and Typescript but language knowledge assessment is not the aim of this challenge)
* Use any orchestration or process coordination tools you see fit (message queues, lambdas, etc)
* Seed the records with any random data
* Use a public key crypto algorithm of your choosing


## How to run the application
Prerequists:
* Docker
*Steps to run*:
* Open terminal to the root folder
* Execute `docker-compose build`
* Execute `docker-compose up`

*Common issues*:
* As we are running the database server in docker, sometimes it crashes. In that case, we need to rerun database server.
* As the database server creation and database creation takes random time(Sometimes it takes less than 30 seconds, sometime more than a minute), we have added delay to the data seeding tool and executor service.


## Tech stack
* .NET 6
* SQL Server
* Azure Service Bus
* Docker

## Core Services

### Collection Service
Get collection of data from the database. And we will use this service to store the signed data.

### Key Management Service
The main goal of the service is to work with public and private keys. When we ask for a key, it will lock the table, update the locked flag as true of the first row and return the first item from the table. Key Management Service return encrypted base64 encoded data.
When we call the release lock endpoint, it will update the locked flag as false and update modification time.

### Signing Service
The responsibility of this service is to handle the signing of the data using the private key.
Probably we can move the service as serverless function later on.

### Message Processor
It will listen to the trigger messages. The initial plan is to handle two messages:
* SigningTriggered
* SingingCompleted

*SigningTriggered*
This message contains X amount of document data and a public key. The processor, call the signing service to sign the data and collection service to store the signed data.
When the signning is finished, it will trigger another message `SingingCompleted`

*SigningCompleted*
`SigningCompleted` message contains the public key information, it also can contain a message about the summary of siggning (Probably we can use it for some monitoring service later on).
The processor retrieve the public key information from the message and call key management service to unlock the key.

### Signing Executor
It will work as aggregator, the responsibilities are:
* Get batch data from collection service.
* Trigger a message with batch data.


### Other tools
*Data Seeder*
Seed data to the databases.

## Possible Improvements
* Error handling
* Proper logging
* Consider pre populate the keys in a cache.

## Possible Questions
*How the solution is secured?*:
* In the solution the most confidencial data is private key. As we do the encryption from key-management-service and depcrypt it in signing-service with a shared key, we have mitigated the security risk.
* When go for production, we can run the key management service and signing service in a private network.  

*How we can confirm the data is signed for a single time?*
* When we store the data, it check if the data is already exists in the database.
* As we are using Azure Service Bus queue, the message will be processed by a single listener.

*How we can make sure one key can not be used concurrently?*
* When we get the private key, inside a transaction we do lock the key.

*How we can make sure the least used key is being used for signing?*
* When the migration is completed, we trigger another message and the listener remove the lock flag and change the modified date.
* During the key retrival, we order the keys by modification date in a asceding order.

*What happen with the key lock when data storing get failed?*
* If there is something wrong after locking the key, it release the lock.

*Why do not using separate message for storing data?*
* It might be a better approch, but as the requirement of not using a key concurrently is not clear enough, we have decided to release the lock after storing the data.

*Is there any single point of failure?*
* Collection Service: As this service is used to retrieve and save data, it could be a single point of failure.
* Key Management Service: If the service is unavailable, we reschedule the event. So this is not consider as single point of failure.
* Signing Service: When the service is unavailable, message processor retry the operations, as a result minimal downtime is acceptable.
* Message Handler Service: When the service is available, it will retry pending messages, so this is not a single point of failure.