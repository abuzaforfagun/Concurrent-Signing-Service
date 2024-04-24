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


## Core Services

### Collection Service
Get collection of data from the database. And we will use this service to store the signed data.

### Key Management Service
The main goal of the service is to work with public and private keys. When we ask for a key, it will lock the table, update the locked flag as true of the first row and return the first item from the table.
When we call the release lock endpoint, it will update the locked flag as false and update modification time.

### Signing Service
The responsibility of this service is to handle the encryption of the data using public key.
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